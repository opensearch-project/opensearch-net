/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.Net;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Core.Client;
using Tests.Domain;

namespace Tests.QueryDsl.Specialized.Neural;

/// <summary>
/// Serialization unit coverage for the radial-search fields on <see cref="INeuralQuery"/>.
/// Radial search (OpenSearch 2.14+) bounds a neural query by <c>max_distance</c> or <c>min_score</c>
/// INSTEAD OF <c>k</c>. Such a query must not be treated as conditionless just because <c>k</c> is
/// unset, otherwise the whole <c>neural</c> clause is silently dropped from the request (issue #1049).
/// </summary>
public class NeuralQueryFieldsTests
{
	private static string Serialize(QueryContainer query) =>
		TestClient.DisabledStreaming.RequestResponseSerializer.SerializeToString(query);

	[U]
	public void MaxDistanceOnlySerializes()
	{
		var fluent = Serialize(new QueryContainerDescriptor<Project>()
			.Neural(n => n.Field(f => f.Vector).QueryText("wild west").ModelId("aFcV879").MaxDistance(1.0f)));
		fluent.Should().Contain("\"max_distance\":1").And.NotContain("\"k\":");

		var initializer = Serialize(new NeuralQuery
		{
			Field = "passage_embedding", QueryText = "wild west", ModelId = "aFcV879", MaxDistance = 1.0f,
		});
		initializer.Should().Contain("\"max_distance\":1").And.NotContain("\"k\":");
	}

	[U]
	public void MinScoreOnlySerializes()
	{
		var fluent = Serialize(new QueryContainerDescriptor<Project>()
			.Neural(n => n.Field(f => f.Vector).QueryText("wild west").ModelId("aFcV879").MinScore(0.95f)));
		fluent.Should().Contain("\"min_score\":0.95").And.NotContain("\"k\":");

		var initializer = Serialize(new NeuralQuery
		{
			Field = "passage_embedding", QueryText = "wild west", ModelId = "aFcV879", MinScore = 0.95f,
		});
		initializer.Should().Contain("\"min_score\":0.95").And.NotContain("\"k\":");
	}

	[U]
	public void QueryImageOnlySerializes()
	{
		// query_image is an alternative embedding input; a query with only query_image (no query_text) is valid.
		var fluent = Serialize(new QueryContainerDescriptor<Project>()
			.Neural(n => n.Field(f => f.Vector).QueryImage("aVZCT1J3").ModelId("aFcV879").K(5)));
		fluent.Should().Contain("\"query_image\":\"aVZCT1J3\"");
	}

	[U]
	public void AllFieldsSerialize()
	{
		var query = new QueryContainerDescriptor<Project>()
			.Neural(n => n
				.Field(f => f.Vector)
				.QueryText("wild west")
				.QueryImage("aVZCT1J3")
				.ModelId("aFcV879")
				.K(5)
				.Filter(f => f.Term(t => t.Field("parking").Value("true")))
				.SemanticFieldSearchAnalyzer("bert-uncased")
				.MethodParameters(mp => mp.Add("ef_search", 100))
				.Rescore(r => r.OversampleFactor(1.5f))
				.ExpandNestedDocs());

		var json = Serialize(query);

		json.Should().Contain("\"query_text\":\"wild west\"");
		json.Should().Contain("\"query_image\":\"aVZCT1J3\"");
		json.Should().Contain("\"model_id\":\"aFcV879\"");
		json.Should().Contain("\"filter\":");
		json.Should().Contain("\"semantic_field_search_analyzer\":\"bert-uncased\"");
		json.Should().Contain("\"method_parameters\":{\"ef_search\":100}");
		json.Should().Contain("\"rescore\":{\"oversample_factor\":1.5}");
		json.Should().Contain("\"expand_nested_docs\":true");
	}

	[U]
	public void QueryTokensOnlySerializes()
	{
		// A raw sparse vector (query_tokens) is a valid input on its own -- no query_text or query_image.
		// It must not be treated as conditionless and dropped.
		var query = new QueryContainerDescriptor<Project>()
			.Neural(n => n.Field(f => f.Vector).QueryTokens(t => t.Add("worlds", 0.57605183f)));

		Serialize(query).Should().Contain("\"query_tokens\":{\"worlds\":0.57605183}");
	}
}
