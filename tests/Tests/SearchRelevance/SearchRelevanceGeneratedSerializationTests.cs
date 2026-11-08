/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Core.Serialization;
using static Tests.Core.Serialization.SerializationTestHelper;

namespace Tests.SearchRelevance
{
	/// <summary>
	/// Round-trip serialization tests for a representative sample of generated search_relevance
	/// namespace types, covering three shapes specific to this namespace: (1) a request whose wire
	/// properties are camelCase rather than the snake_case used elsewhere in the spec, (2) the
	/// composition-flattened <c>PutExperimentsRequest</c>/<c>PutJudgmentsRequest</c> (a bare
	/// <c>anyOf</c>/<c>oneOf</c> of sibling schemas merged into one type — see
	/// <c>OperationModel.FlattenCompositionProperties</c>), and (3) a list-endpoint response that
	/// reuses the shared <c>_core.search___SearchResult</c> envelope from outside the
	/// search_relevance namespace.
	/// </summary>
	public class SearchRelevanceGeneratedSerializationTests
	{
		/// <summary>
		/// Test 1: <c>search_relevance.put_search_configurations</c> request with camelCase wire
		/// names (<c>searchPipeline</c>). Verifies that <c>NamingConventions.ToPascal</c> splits
		/// camelCase word boundaries correctly (<c>SearchPipeline</c>, not the pre-fix
		/// <c>Searchpipeline</c>).
		/// </summary>
		[U]
		public void PutSearchConfigurationsRequest_CamelCaseWireNames_SerializeCorrectly()
		{
			var expectedJson = new
			{
				name = "baseline",
				query = "{\"match\":{\"title\":\"%SearchText%\"}}",
				index = "ecommerce",
				searchPipeline = "my-pipeline",
			};

			var request = new PutSearchConfigurationsRequest
			{
				Name = "baseline",
				Query = "{\"match\":{\"title\":\"%SearchText%\"}}",
				Index = "ecommerce",
				SearchPipeline = "my-pipeline",
			};

			Expect(expectedJson).FromRequest(c => c.SearchRelevance.PutSearchConfigurations(request));
		}

		/// <summary>
		/// Test 2: <c>PutExperimentsRequest</c> — the flattened <c>anyOf</c> of
		/// <c>PutHybridOptimizerExperimentRequest</c>/<c>PutPointwiseExperimentRequest</c>/
		/// <c>PutPairwiseExperimentRequest</c>. Verifies that fields from all three sibling schemas
		/// (including the pointwise-only <c>judgmentList</c>) are present on the single merged
		/// request type and serialize with their original camelCase wire names.
		/// </summary>
		[U]
		public void PutExperimentsRequest_FlattenedUnionFields_SerializeCorrectly()
		{
			var expectedJson = new
			{
				querySetId = "qs-1",
				searchConfigurationList = new[] { "config-a", "config-b" },
				judgmentList = new[] { "judg-1" },
				size = 10,
				type = "PAIRWISE_COMPARISON",
			};

			var request = new PutExperimentsRequest
			{
				QuerySetId = "qs-1",
				SearchConfigurationList = new[] { "config-a", "config-b" },
				JudgmentList = new[] { "judg-1" },
				Size = 10,
				Type = "PAIRWISE_COMPARISON",
			};

			Expect(expectedJson).FromRequest(c => c.SearchRelevance.PutExperiments(request));
		}

		/// <summary>
		/// Test 3: <see cref="PutJudgmentsResponse"/> deserialization.
		/// Verifies the <c>judgment_id</c> field maps to its C# property via
		/// <c>[DataMember(Name=...)]</c>.
		/// </summary>
		[U]
		public void PutJudgmentsResponse_DeserializesFieldCorrectly()
		{
			const string json = @"{""judgment_id"":""j1""}";

			var response = Expect(json).NoRoundTrip().DeserializesTo<PutJudgmentsResponse>();

			response.Should().NotBeNull();
			response.JudgmentId.Should().Be("j1");
		}

		/// <summary>
		/// Test 4: <see cref="GetQuerySetsResponse"/> deserialization of the shared
		/// <c>_core.search___SearchResult</c> envelope fields that are mapped to existing OSC types
		/// (<see cref="ClusterStatistics"/> for <c>_clusters</c>, <see cref="ShardStatistics"/> for
		/// <c>_shards</c>) — see <c>ModelOverridesBase.GlobalMappedTypes</c>.
		/// </summary>
		[U]
		public void GetQuerySetsResponse_DeserializesSharedEnvelopeFields()
		{
			const string json = @"{
				""took"":5,
				""timed_out"":false,
				""_shards"":{""total"":1,""successful"":1,""failed"":0},
				""_clusters"":{""total"":1,""successful"":1,""skipped"":0}
			}";

			var response = Expect(json).NoRoundTrip().DeserializesTo<GetQuerySetsResponse>();

			response.Should().NotBeNull();
			response.Took.Should().Be(5);
			response.TimedOut.Should().Be(false);
			response.Shards.Total.Should().Be(1);
			response.Clusters.Total.Should().Be(1);
		}
	}
}
