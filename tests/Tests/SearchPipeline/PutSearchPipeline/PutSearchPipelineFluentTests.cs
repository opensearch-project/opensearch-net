/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/
/*
* Modifications Copyright OpenSearch Contributors. See
* GitHub history for details.
*/

using System.Collections.Generic;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.Net;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;

namespace Tests.SearchPipeline.PutSearchPipeline
{
	public class PutSearchPipelineFluentTests
	{
		private static readonly OpenSearchClient Client = new OpenSearchClient();

		[U]
		public void FluentProcessorListsSerializeSameAsObjectInitializer()
		{
			// Fully fluent: the query is built inline via the query-builder selector,
			// not pre-constructed. Each stage takes MORE THAN ONE processor, exercising
			// the builder's append-in-order accumulation.
			var descriptor = new PutSearchPipelineDescriptor("my-pipeline")
				.Description("normalize and drop low-value hits")
				.RequestProcessors(rp => rp
					.FilterQuery(fq => fq.Query(q => q.Term(t => t.Field("visibility").Value("public"))))
					.Oversample(os => os.SampleFactor(2.0f)))
				.ResponseProcessors(resp => resp
					.RenameField(rf => rf.Field("message").TargetField("comment"))
					.TruncateHits(th => th.TargetSize(10)));

			QueryContainer termQuery = new TermQuery { Field = "visibility", Value = "public" };

			var initializer = new PutSearchPipelineRequest("my-pipeline")
			{
				Description = "normalize and drop low-value hits",
				RequestProcessors = new List<IRequestProcessor>
				{
					new FilterQueryRequestProcessor { Query = termQuery },
					new OversampleRequestProcessor { SampleFactor = 2.0f }
				},
				ResponseProcessors = new List<IResponseProcessor>
				{
					new RenameFieldResponseProcessor { Field = "message", TargetField = "comment" },
					new TruncateHitsResponseProcessor { TargetSize = 10 }
				}
			};

			var fluentJson = Client.RequestResponseSerializer.SerializeToString(
				(IPutSearchPipelineRequest)descriptor
			);
			var initializerJson = Client.RequestResponseSerializer.SerializeToString(
				(IPutSearchPipelineRequest)initializer
			);

			// Byte-identical JSON proves both stages carry both processors, in order,
			// and that each processor serialized under its own wrapper key (not null).
			fluentJson.Should().Be(initializerJson);
			fluentJson.Should().Contain("\"filter_query\"").And.Contain("\"oversample\"");
			fluentJson.Should().Contain("\"rename_field\"").And.Contain("\"truncate_hits\"");
			fluentJson.Should().NotContain("null");
		}

		[U]
		public void DeeplyNestedFluentBuilderSerializesSameAsObjectInitializer()
		{
			// Three fluent layers: request body -> processor-collection selector ->
			// per-variant selector, with the variant itself carrying a nested collection
			// (ml_inference's input_map / output_map).
			var inputMap = new List<IDictionary<string, string>>
			{
				new Dictionary<string, string> { ["input"] = "$.query.term.field" }
			};
			var outputMap = new List<IDictionary<string, string>>
			{
				new Dictionary<string, string> { ["rank_score"] = "$.score" }
			};

			var descriptor = new PutSearchPipelineDescriptor("deep-pipeline")
				.RequestProcessors(rp => rp
					.MlInference(ml => ml
						.ModelId("model-1")
						.InputMap(inputMap)
						.OutputMap(outputMap)));

			var initializer = new PutSearchPipelineRequest("deep-pipeline")
			{
				RequestProcessors = new List<IRequestProcessor>
				{
					new MLInferenceRequestProcessor
					{
						ModelId = "model-1",
						InputMap = inputMap,
						OutputMap = outputMap
					}
				}
			};

			var fluentJson = Client.RequestResponseSerializer.SerializeToString(
				(IPutSearchPipelineRequest)descriptor
			);
			var initializerJson = Client.RequestResponseSerializer.SerializeToString(
				(IPutSearchPipelineRequest)initializer
			);

			fluentJson.Should().Be(initializerJson);
			fluentJson.Should().Contain("\"ml_inference\"");
			fluentJson.Should().Contain("\"input_map\"").And.Contain("\"output_map\"");
		}
	}
}
