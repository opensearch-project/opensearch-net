/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.Text.Json;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;

namespace Tests.OpenSearch.Client.Serialization
{
	/// <summary>
	/// Behavioural tests for the settings-aware <see cref="FieldNameQueryConverter{T, TInterface}"/>: a field-name
	/// query serializes as a single-key object <c>{ "&lt;field&gt;": &lt;body&gt; }</c> where the key is resolved through the
	/// runtime <c>Inferrer</c>, and deserializes back — including the scalar short-forms (a term/match written as a
	/// bare value rather than a nested object).
	/// </summary>
	public class FieldNameQueryConverterTests
	{
		private static JsonSerializerOptions Options(params global::System.Text.Json.Serialization.JsonConverter[] converters) =>
			Options(new ConnectionSettings(), converters);

		private static JsonSerializerOptions Options(ConnectionSettings settings,
			params global::System.Text.Json.Serialization.JsonConverter[] converters)
		{
			var options = new JsonSerializerOptions
			{
				// Mirror SystemTextJsonHighLevelSerializer so null members are omitted from the body.
				DefaultIgnoreCondition = global::System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
				TypeInfoResolver = new HighLevelContractResolver(settings)
			};
			foreach (var c in converters)
				options.Converters.Add(c);
			return options;
		}

		// --- Serialization: field-name wrapping via the runtime Inferrer ---

		[U] public void Serialize_WrapsBodyUnderResolvedFieldName()
		{
			var options = Options(new FieldNameQueryConverter<TermQuery, ITermQuery>(new ConnectionSettings()));
			ITermQuery query = new TermQuery { Field = "name", Value = "bob" };

			var json = JsonSerializer.Serialize(query, options);

			json.Should().Be(@"{""name"":{""value"":""bob""}}");
		}

		[U] public void Serialize_BodyResolvedViaSettingsDrivenContractResolver()
		{
			// The wrapping field key comes from a literal string Field (not run through DefaultFieldNameInferrer),
			// but the body member names are produced by the settings-driven HighLevelContractResolver — switching
			// inference to UPPER-case proves the converter delegates the body to the runtime-configured resolver.
			var settings = new ConnectionSettings().DefaultFieldNameInferrer(f => f.ToUpperInvariant());
			var options = Options(settings, new FieldNameQueryConverter<TermQuery, ITermQuery>(settings));
			ITermQuery query = new TermQuery { Field = "name", Value = "bob" };

			var json = JsonSerializer.Serialize(query, options);

			json.Should().Be(@"{""name"":{""VALUE"":""bob""}}");
		}

		[U] public void Serialize_Null_WritesNull()
		{
			var options = Options(new FieldNameQueryConverter<TermQuery, ITermQuery>(new ConnectionSettings()));
			var json = JsonSerializer.Serialize<ITermQuery>(null, options);
			json.Should().Be("null");
		}

		[U] public void Serialize_NullField_WritesNull()
		{
			var options = Options(new FieldNameQueryConverter<TermQuery, ITermQuery>(new ConnectionSettings()));
			ITermQuery query = new TermQuery { Value = "bob" }; // no Field
			var json = JsonSerializer.Serialize(query, options);
			json.Should().Be("null");
		}

		// --- Deserialization: nested-object body ---

		[U] public void Deserialize_ObjectBody_ResolvesFieldAndBody()
		{
			var options = Options(new FieldNameQueryConverter<TermQuery, ITermQuery>(new ConnectionSettings()));

			var query = JsonSerializer.Deserialize<ITermQuery>(@"{""name"":{""value"":""bob""}}", options);

			query.Should().NotBeNull();
			query.Field.Name.Should().Be("name");
			query.Value.ToString().Should().Be("bob");
		}

		[U] public void Deserialize_Null_ReturnsNull()
		{
			var options = Options(new FieldNameQueryConverter<TermQuery, ITermQuery>(new ConnectionSettings()));
			JsonSerializer.Deserialize<ITermQuery>("null", options).Should().BeNull();
		}

		[U] public void Deserialize_EmptyObject_ReturnsNull()
		{
			var options = Options(new FieldNameQueryConverter<TermQuery, ITermQuery>(new ConnectionSettings()));
			JsonSerializer.Deserialize<ITermQuery>("{}", options).Should().BeNull();
		}

		// --- Deserialization: scalar short-forms (legacy formatter parity) ---

		[U] public void Deserialize_TermShortForm_String()
		{
			var options = Options(new FieldNameQueryConverter<TermQuery, ITermQuery>(new ConnectionSettings()));
			var query = JsonSerializer.Deserialize<ITermQuery>(@"{""name"":""bob""}", options);
			query.Field.Name.Should().Be("name");
			query.Value.Should().Be("bob");
		}

		[U] public void Deserialize_TermShortForm_Long()
		{
			var options = Options(new FieldNameQueryConverter<TermQuery, ITermQuery>(new ConnectionSettings()));
			var query = JsonSerializer.Deserialize<ITermQuery>(@"{""count"":42}", options);
			query.Value.Should().Be(42L);
		}

		[U] public void Deserialize_TermShortForm_Double()
		{
			var options = Options(new FieldNameQueryConverter<TermQuery, ITermQuery>(new ConnectionSettings()));
			var query = JsonSerializer.Deserialize<ITermQuery>(@"{""ratio"":1.5}", options);
			query.Value.Should().Be(1.5);
		}

		[U] public void Deserialize_TermShortForm_Bool()
		{
			var options = Options(new FieldNameQueryConverter<TermQuery, ITermQuery>(new ConnectionSettings()));
			var query = JsonSerializer.Deserialize<ITermQuery>(@"{""flag"":true}", options);
			query.Value.Should().Be(true);
		}

		[U] public void Deserialize_MatchShortForm_String()
		{
			var options = Options(new FieldNameQueryConverter<MatchQuery, IMatchQuery>(new ConnectionSettings()));
			var query = JsonSerializer.Deserialize<IMatchQuery>(@"{""message"":""hello world""}", options);
			query.Field.Name.Should().Be("message");
			query.Query.Should().Be("hello world");
		}

		// --- Round-trip through the resolved field name ---

		[U] public void RoundTrip_Term()
		{
			var options = Options(new FieldNameQueryConverter<TermQuery, ITermQuery>(new ConnectionSettings()));
			ITermQuery original = new TermQuery { Field = "name", Value = "bob" };

			var json = JsonSerializer.Serialize(original, options);
			var back = JsonSerializer.Deserialize<ITermQuery>(json, options);

			back.Field.Name.Should().Be("name");
			back.Value.ToString().Should().Be("bob");
		}
	}
}
