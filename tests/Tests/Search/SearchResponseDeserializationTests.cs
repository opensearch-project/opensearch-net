/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.IO;
using System.Linq;
using System.Text;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.Net;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Core.Client;
using Tests.Domain;

namespace Tests.Search;

public class SearchResponseDeserializationTests
{
	// A search response exercising hits, _explanation, suggest, and collapsed inner_hits — the four areas the
	// integration tests (ExplainUsageTests, SuggestUsageTests, FieldCollapseUsageTests, SearchProfileApiTests) cover.
	private const string Json = @"{
		""took"": 1,
		""timed_out"": false,
		""_shards"": { ""total"": 1, ""successful"": 1, ""skipped"": 0, ""failed"": 0 },
		""hits"": {
			""total"": { ""value"": 2, ""relation"": ""eq"" },
			""max_score"": 1.0,
			""hits"": [
				{
					""_index"": ""project"",
					""_id"": ""1"",
					""_score"": 1.0,
					""_source"": { ""name"": ""x"" },
					""_explanation"": { ""value"": 1.0, ""description"": ""weight"", ""details"": [] },
					""inner_hits"": {
						""commits"": {
							""hits"": { ""total"": { ""value"": 1, ""relation"": ""eq"" }, ""max_score"": 1.0, ""hits"": [] }
						}
					}
				}
			]
		},
		""suggest"": {
			""my-suggest"": [
				{ ""text"": ""x"", ""offset"": 0, ""length"": 1, ""options"": [ { ""text"": ""xy"", ""score"": 0.8 } ] }
			]
		}
	}";

	[U] public void DeserializesHitsExplanationSuggestAndInnerHits()
	{
		var response = TestClient.DisabledStreaming.RequestResponseSerializer.Deserialize<SearchResponse<Project>>(
			new MemoryStream(Encoding.UTF8.GetBytes(Json)));

		response.Total.Should().Be(2);
		response.Hits.Should().NotBeEmpty("hits must deserialize");
		var hit = response.Hits.First();
		hit.Id.Should().Be("1");
		hit.Explanation.Should().NotBeNull("_explanation must deserialize");
		hit.Explanation.Value.Should().Be(1.0f);
		hit.InnerHits.ContainsKey("commits").Should().BeTrue("inner_hits must deserialize");
		response.Suggest.ContainsKey("my-suggest").Should().BeTrue("suggest must deserialize");
	}
}
