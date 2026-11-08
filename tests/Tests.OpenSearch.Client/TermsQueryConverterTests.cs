/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;

namespace Tests.OpenSearch.Client.Serialization
{
	/// <summary>
	/// Behavioural tests for the settings-aware <see cref="TermsQueryConverter"/>: an <see cref="ITermsQuery"/> is a
	/// field-name query serialized as <c>{ "&lt;field&gt;": [ ...terms... ] }</c> (verbatim terms) or
	/// <c>{ "&lt;field&gt;": { index/id/path/routing } }</c> (a terms-lookup), plus the common <c>_name</c>/<c>boost</c>.
	/// Covers both dispatch shapes on read and write, the IsVerbatim write dispatch, the field wrapper on write, mixed
	/// term value kinds, null and round-trip.
	/// </summary>
	public class TermsQueryConverterTests
	{
		private static JsonSerializerOptions Options()
		{
			var settings = new ConnectionSettings();
			var options = new JsonSerializerOptions
			{
				DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
				TypeInfoResolver = new HighLevelContractResolver(settings)
			};
			options.Converters.Add(new TermsQueryConverter(settings));
			// Terms-lookup body members resolve through the inferrer / infer converters.
			options.Converters.Add(new IdConverter(settings));
			options.Converters.Add(new IndexNameConverter(settings));
			options.Converters.Add(new RoutingConverter(settings));
			options.Converters.Add(new FieldConverter(settings));
			return options;
		}

		private static ITermsQuery Deserialize(string json) =>
			JsonSerializer.Deserialize<ITermsQuery>(json, Options());

		private static string Serialize(ITermsQuery value) =>
			JsonSerializer.Serialize(value, Options());

		// --- Dispatch: verbatim terms array ---

		[U] public void Deserialize_TermsList()
		{
			var query = Deserialize(@"{""description"":[""term1"",""term2""]}");
			query.Should().BeOfType<TermsQuery>();
			query.Field.Name.Should().Be("description");
			query.Terms.Should().BeEquivalentTo(new object[] { "term1", "term2" });
			query.TermsLookup.Should().BeNull();
		}

		[U] public void Deserialize_TermsList_WithCommonOptions()
		{
			var query = Deserialize(@"{""_name"":""named_query"",""boost"":1.1,""description"":[""term1"",""term2""]}");
			query.Name.Should().Be("named_query");
			query.Boost.Should().Be(1.1);
			query.Field.Name.Should().Be("description");
			query.Terms.Should().HaveCount(2);
		}

		[U] public void Deserialize_TermsList_MixedValueKinds()
		{
			var query = Deserialize(@"{""f"":[""a"",1,2.5,true,null]}");
			var terms = query.Terms.ToList();
			terms[0].Should().Be("a");
			terms[1].Should().Be(1L);
			terms[2].Should().Be(2.5);
			terms[3].Should().Be(true);
			terms[4].Should().BeNull();
		}

		[U] public void Serialize_TermsList_EmitsFieldWrapper()
		{
			ITermsQuery query = new TermsQuery { Field = "description", Terms = new object[] { "term1", "term2" } };
			Serialize(query).Should().Be(@"{""description"":[""term1"",""term2""]}");
		}

		[U] public void Serialize_TermsList_WithCommonOptions()
		{
			ITermsQuery query = new TermsQuery
			{
				Name = "named_query", Boost = 1.1, Field = "description", Terms = new object[] { "term1", "term2" }
			};
			Serialize(query).Should().Be(@"{""_name"":""named_query"",""boost"":1.1,""description"":[""term1"",""term2""]}");
		}

		// --- Dispatch: terms-lookup object ---

		[U] public void Deserialize_TermsLookup()
		{
			var query = Deserialize(@"{""description"":{""id"":""12"",""index"":""devs"",""path"":""lastName"",""routing"":""r""}}");
			query.Field.Name.Should().Be("description");
			query.Terms.Should().BeNull();
			query.TermsLookup.Should().NotBeNull();
			query.TermsLookup.Id.ToString().Should().Be("12");
			query.TermsLookup.Path.Name.Should().Be("lastName");
			query.TermsLookup.Routing.ToString().Should().Be("r");
		}

		[U] public void Serialize_TermsLookup_EmitsFieldWrapper()
		{
			ITermsQuery query = new TermsQuery
			{
				Field = "description",
				TermsLookup = new FieldLookup { Id = "12", Index = "devs", Path = "lastName", Routing = "r" }
			};
			var json = Serialize(query);
			json.Should().StartWith(@"{""description"":{").And.EndWith("}}");
			json.Should().Contain(@"""id"":""12""").And.Contain(@"""index"":""devs""")
				.And.Contain(@"""path"":""lastName""").And.Contain(@"""routing"":""r""");
		}

		// --- IsVerbatim write dispatch ---

		[U] public void Serialize_Verbatim_PrefersTermsLookup_WhenBothSet()
		{
			// Verbatim: legacy dispatch prefers TermsLookup over Terms.
			ITermsQuery query = new TermsQuery
			{
				IsVerbatim = true,
				Field = "description",
				Terms = new object[] { "term1" },
				TermsLookup = new FieldLookup { Id = "12", Index = "devs", Path = "lastName" }
			};
			Serialize(query).Should().Contain(@"""id"":""12""").And.NotContain("term1");
		}

		[U] public void Serialize_Verbatim_EmptyTermsArray_IsStillWritten()
		{
			// A verbatim empty array is serialized (it is not conditionless when verbatim).
			ITermsQuery query = new TermsQuery { IsVerbatim = true, Field = "description", Terms = new object[] { } };
			Serialize(query).Should().Be(@"{""description"":[]}");
		}

		[U] public void Serialize_NonVerbatim_PrefersTerms_WhenBothSet()
		{
			// Non-verbatim: legacy dispatch prefers a non-empty Terms array over TermsLookup.
			ITermsQuery query = new TermsQuery
			{
				Field = "description",
				Terms = new object[] { "term1" },
				TermsLookup = new FieldLookup { Id = "12", Index = "devs", Path = "lastName" }
			};
			Serialize(query).Should().Contain("term1").And.NotContain(@"""id""");
		}

		[U] public void Serialize_NonVerbatim_EmptyTerms_FallsBackToLookup()
		{
			ITermsQuery query = new TermsQuery
			{
				Field = "description",
				Terms = new object[] { },
				TermsLookup = new FieldLookup { Id = "12", Index = "devs", Path = "lastName" }
			};
			Serialize(query).Should().Contain(@"""id"":""12""");
		}

		// --- null ---

		[U] public void Deserialize_Null_ReturnsNull() => Deserialize("null").Should().BeNull();

		[U] public void Serialize_Null_WritesNull() =>
			JsonSerializer.Serialize<ITermsQuery>(null, Options()).Should().Be("null");

		// --- Round-trip ---

		[U] public void RoundTrip_TermsList()
		{
			ITermsQuery original = new TermsQuery
			{
				Name = "n", Boost = 2.0, Field = "description", Terms = new object[] { "a", "b" }
			};
			var back = Deserialize(Serialize(original));
			back.Field.Name.Should().Be("description");
			back.Name.Should().Be("n");
			back.Boost.Should().Be(2.0);
			back.Terms.Should().BeEquivalentTo(new object[] { "a", "b" });
		}

		[U] public void RoundTrip_TermsLookup()
		{
			ITermsQuery original = new TermsQuery
			{
				Field = "description",
				TermsLookup = new FieldLookup { Id = "12", Index = "devs", Path = "lastName" }
			};
			var back = Deserialize(Serialize(original));
			back.TermsLookup.Should().NotBeNull();
			back.TermsLookup.Id.ToString().Should().Be("12");
			back.TermsLookup.Path.Name.Should().Be("lastName");
		}
	}
}
