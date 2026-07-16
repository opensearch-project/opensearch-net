/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.IO;
using System.Text;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.Net;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;

namespace Tests.OpenSearch.Client.Serialization
{
	/// <summary>
	/// SPIKE validation for <see cref="SystemTextJsonHighLevelSerializer"/> — proves a System.Text.Json based
	/// high-level serializer can be driven by the runtime <see cref="IConnectionSettingsValues"/> configuration,
	/// which is the hardest capability of the legacy Utf8Json engine to reproduce.
	/// </summary>
	public class HighLevelSerializerSpikeTests
	{
		public class Doc
		{
			public string FirstName { get; set; }
			public int ItemCount { get; set; }
		}

		private static string Serialize<T>(IOpenSearchSerializer serializer, T value)
		{
			using var ms = new MemoryStream();
			serializer.Serialize(value, ms);
			return Encoding.UTF8.GetString(ms.ToArray());
		}

		[U] public void UsesDefaultCamelCaseFieldInference()
		{
			var settings = new ConnectionSettings(); // default DefaultFieldNameInferrer = camelCase
			var serializer = new SystemTextJsonHighLevelSerializer(settings);

			var json = Serialize(serializer, new Doc { FirstName = "bob", ItemCount = 3 });

			json.Should().Contain(@"""firstName"":""bob""");
			json.Should().Contain(@"""itemCount"":3");
		}

		[U] public void HonoursCustomFieldNameInferrer()
		{
			// Prove the resolver is driven by runtime settings: switch inference to UPPER-case.
			var settings = new ConnectionSettings().DefaultFieldNameInferrer(f => f.ToUpperInvariant());
			var serializer = new SystemTextJsonHighLevelSerializer(settings);

			var json = Serialize(serializer, new Doc { FirstName = "bob", ItemCount = 3 });

			json.Should().Contain(@"""FIRSTNAME"":""bob""");
			json.Should().Contain(@"""ITEMCOUNT"":3");
		}

		[U] public void RoundTrips()
		{
			var settings = new ConnectionSettings();
			var serializer = new SystemTextJsonHighLevelSerializer(settings);

			var json = Serialize(serializer, new Doc { FirstName = "alice", ItemCount = 7 });
			using var ms = new MemoryStream(Encoding.UTF8.GetBytes(json));
			var back = serializer.Deserialize<Doc>(ms);

			back.FirstName.Should().Be("alice");
			back.ItemCount.Should().Be(7);
		}

		[U] public void ReadAs_DeserializesInterfaceAsConcreteType()
		{
			// ITextIndexPrefixes is marked [ReadAs(typeof(TextIndexPrefixes))]; deserializing the interface must
			// produce the concrete TextIndexPrefixes via ReadAsConverterFactory.
			var settings = new ConnectionSettings();
			var serializer = new SystemTextJsonHighLevelSerializer(settings);

			// Field names come from [DataMember(Name=...)] on the interface: min_chars / max_chars.
			using var ms = new MemoryStream(Encoding.UTF8.GetBytes(@"{""min_chars"":2,""max_chars"":5}"));
			var result = serializer.Deserialize<ITextIndexPrefixes>(ms);

			result.Should().BeOfType<TextIndexPrefixes>();
			result.MinCharacters.Should().Be(2);
			result.MaxCharacters.Should().Be(5);
		}

		// The following tests prove the migrated type-level converters are actually reached *through the serializer*
		// (registered in options.Converters), not just when instantiated directly — this is the "wiring" B5 delivers.

		private static string Serialize<T>(T value)
		{
			var serializer = new SystemTextJsonHighLevelSerializer(new ConnectionSettings());
			return Serialize(serializer, value);
		}

		private static T RoundTrip<T>(T value)
		{
			var serializer = new SystemTextJsonHighLevelSerializer(new ConnectionSettings());
			var json = Serialize(serializer, value);
			using var ms = new MemoryStream(Encoding.UTF8.GetBytes(json));
			return serializer.Deserialize<T>(ms);
		}

		[U] public void StatelessConverter_Time_IsReachedThroughSerializer()
		{
			// Time serializes to a compact unit string (e.g. "7d") via the registered TimeConverter.
			Serialize<Time>("7d").Should().Contain("7d");
			var back = RoundTrip<Time>("7d");
			back.Factor.Should().Be(7);
			back.Interval.Should().Be(TimeUnit.Days);
		}

		[U] public void StatelessConverter_Distance_IsReachedThroughSerializer()
		{
			Serialize<Distance>("10m").Should().Contain("10m");
			var back = RoundTrip<Distance>("10m");
			back.Precision.Should().Be(10);
			back.Unit.Should().Be(DistanceUnit.Meters);
		}

		[U] public void SettingsAwareConverter_IndexName_UsesInferrer()
		{
			// IndexName is resolved through the settings Inferrer and serialized as a bare string by IndexNameConverter.
			Serialize<IndexName>("my-index").Should().Be(@"""my-index""");
		}

		[U] public void SettingsAwareConverter_Indices_SerializedByMultiSyntaxConverter()
		{
			// The type-level default for Indices is IndicesMultiSyntaxConverter: multiple indices join as a CSV string.
			Indices indices = Indices.Index("a", "b");
			Serialize(indices).Should().Be(@"""a,b""");
		}

		// The FieldNameQueryConverterFactory unblocks field-name queries ({ "field": { <body> } }). These prove the
		// factory constructs the right converter per interface and that it is reached through the serializer.

		[U] public void FieldNameQueryFactory_MatchQuery_WrapsInFieldObject()
		{
			IMatchQuery query = new MatchQuery { Field = "message", Query = "hello world" };
			var json = Serialize<IMatchQuery>(query);
			// Wrapped as { "message": { "query": "hello world" } } — the field key is the outer property.
			json.Should().Contain(@"""message""").And.Contain(@"""query"":""hello world""");
		}

		[U] public void FieldNameQueryFactory_TermQuery_RoundTrips()
		{
			ITermQuery query = new TermQuery { Field = "status", Value = "active" };
			var json = Serialize<ITermQuery>(query);
			json.Should().Contain(@"""status""").And.Contain(@"""active""");

			using var ms = new MemoryStream(Encoding.UTF8.GetBytes(json));
			var back = new SystemTextJsonHighLevelSerializer(new ConnectionSettings()).Deserialize<ITermQuery>(ms);
			back.Should().NotBeNull();
			// The factory correctly unwraps the field name from the outer object.
			back.Field.Name.Should().Be("status");
			// NOTE: TermQuery.Value is object-typed; under STJ an object property deserializes to a JsonElement
			// rather than a boxed string (a known STJ-vs-Utf8Json difference, tracked separately). Assert on the
			// string form so this test stays focused on the factory's field-wrapping responsibility.
			back.Value.ToString().Should().Be("active");
		}
	}
}
