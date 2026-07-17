/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;

namespace Tests.OpenSearch.Client.Serialization
{
	/// <summary>
	/// Behavioural tests for <see cref="QueryContainerConverter"/> (+ interface / collection variants): a
	/// <see cref="QueryContainer"/> serializes as a single-key object naming the one set query (e.g. <c>{ "bool": … }</c>)
	/// and deserializes by reading that single key into the matching property. Also covers the raw-query passthrough,
	/// the string-shape read, null / non-writable skipping and the collection array + single-object shapes.
	/// </summary>
	public class QueryContainerConverterTests
	{
		private static JsonSerializerOptions Options()
		{
			var settings = new ConnectionSettings();
			var options = new JsonSerializerOptions
			{
				DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
				TypeInfoResolver = new HighLevelContractResolver(settings)
			};
			// Container converters under test.
			options.Converters.Add(new QueryContainerConverter());
			options.Converters.Add(new QueryContainerInterfaceConverter());
			options.Converters.Add(new QueryContainerCollectionConverter());
			// Per-query converters the container delegates the body to.
			options.Converters.Add(new ReadAsConverterFactory());
			options.Converters.Add(new FieldNameQueryConverterFactory(settings));
			options.Converters.Add(new RangeQueryConverter());
			// Sub-option converters used by bool / range bodies.
			options.Converters.Add(new MinimumShouldMatchConverter());
			return options;
		}

		private static QueryContainer Deserialize(string json) =>
			JsonSerializer.Deserialize<QueryContainer>(json, Options());

		private static string Serialize(QueryContainer container) =>
			JsonSerializer.Serialize(container, Options());

		[U] public void Serialize_BoolQuery_WritesBoolKey()
		{
			QueryContainer container = new BoolQuery { Boost = 2, MinimumShouldMatch = 1 };
			var json = Serialize(container);
			json.Should().StartWith("{\"bool\":");
			json.Should().Contain("\"boost\":2");
			json.Should().Contain("\"minimum_should_match\":1");
		}

		[U] public void RoundTrip_BoolQuery()
		{
			QueryContainer container = new BoolQuery
			{
				Must = new QueryContainer[] { new MatchAllQuery() },
				Boost = 2
			};
			var json = Serialize(container);

			var back = Deserialize(json);
			back.Should().NotBeNull();
			var @bool = ((IQueryContainer)back).Bool;
			@bool.Should().NotBeNull();
			@bool.Boost.Should().Be(2);
			@bool.Must.Should().NotBeNull().And.HaveCount(1);
		}

		[U] public void Serialize_MatchQuery_WritesMatchKey()
		{
			QueryContainer container = new MatchQuery { Field = "description", Query = "hello" };
			var json = Serialize(container);
			json.Should().StartWith("{\"match\":");
			json.Should().Contain("\"description\"");
			json.Should().Contain("hello");
		}

		[U] public void Deserialize_MatchQuery_PopulatesMatchProperty()
		{
			var container = Deserialize(@"{""match"":{""description"":{""query"":""hello""}}}");
			var match = ((IQueryContainer)container).Match;
			match.Should().NotBeNull();
			match.Field.Name.Should().Be("description");
			match.Query.Should().Be("hello");
		}

		[U] public void RoundTrip_TermQuery()
		{
			QueryContainer container = new TermQuery { Field = "state", Value = "active" };
			var json = Serialize(container);
			json.Should().StartWith("{\"term\":");

			var back = Deserialize(json);
			var term = ((IQueryContainer)back).Term;
			term.Should().NotBeNull();
			term.Field.Name.Should().Be("state");
			// TermQuery.Value is object-typed; under STJ it deserializes to a JsonElement rather than a boxed string
			// (a known STJ-vs-Utf8Json difference tracked separately). Assert the string form.
			term.Value.ToString().Should().Be("active");
		}

		[U] public void Serialize_RawQuery_WritesVerbatimJson()
		{
			var raw = "{\"custom\":{\"anything\":true}}";
			QueryContainer container = new RawQuery(raw);
			var json = Serialize(container);
			// The raw JSON is written verbatim, not wrapped in a "raw" key.
			json.Should().Be(raw);
		}

		[U] public void Deserialize_StringShape_ParsesAsQuery()
		{
			// A JSON string whose content is itself a query object is parsed and populated (legacy string-shape read).
			var json = "\"{\\\"term\\\":{\\\"state\\\":{\\\"value\\\":\\\"active\\\"}}}\"";
			var container = Deserialize(json);
			container.Should().NotBeNull();
			((IQueryContainer)container).Term.Should().NotBeNull();
		}

		[U] public void Serialize_NonWritableContainer_InCollection_IsSkipped()
		{
			// A conditionless (non-writable) term query is skipped in a collection.
			var containers = new List<QueryContainer>
			{
				new TermQuery { Field = "state", Value = "active" }, // writable
				new TermQuery { Field = "x", Value = null },          // conditionless -> not writable
			};
			var json = JsonSerializer.Serialize<IEnumerable<QueryContainer>>(containers, Options());
			json.Should().StartWith("[").And.EndWith("]");
			json.Should().Contain("\"state\"");
			// Only the single writable entry is emitted.
			json.Split(new[] { "{\"term\"" }, System.StringSplitOptions.None).Length.Should().Be(2);
		}

		[U] public void Deserialize_Collection_Array()
		{
			var json = @"[{""term"":{""state"":{""value"":""active""}}},{""match"":{""d"":{""query"":""hi""}}}]";
			var list = JsonSerializer.Deserialize<IEnumerable<QueryContainer>>(json, Options()).ToList();
			list.Should().HaveCount(2);
			((IQueryContainer)list[0]).Term.Should().NotBeNull();
			((IQueryContainer)list[1]).Match.Should().NotBeNull();
		}

		[U] public void Deserialize_Collection_SingleObject_AsOneElementList()
		{
			var json = @"{""term"":{""state"":{""value"":""active""}}}";
			var list = JsonSerializer.Deserialize<IEnumerable<QueryContainer>>(json, Options()).ToList();
			list.Should().HaveCount(1);
			((IQueryContainer)list[0]).Term.Should().NotBeNull();
		}

		[U] public void Deserialize_Null_ReturnsNull()
		{
			Deserialize("null").Should().BeNull();
		}

		[U] public void Serialize_NullInterface_WritesNull()
		{
			JsonSerializer.Serialize<IQueryContainer>(null, Options()).Should().Be("null");
		}

		[U] public void RoundTrip_RangeQuery()
		{
			QueryContainer container = new NumericRangeQuery { Field = "age", GreaterThanOrEqualTo = 1.5, LessThanOrEqualTo = 10 };
			var json = Serialize(container);
			// Range must be field-wrapped: { "range": { "age": { bounds } } } so it round-trips.
			json.Should().StartWith("{\"range\":").And.Contain("\"age\"");

			var back = Deserialize(json);
			((IQueryContainer)back).Range.Should().NotBeNull().And.BeOfType<NumericRangeQuery>();
		}
	}
}
