/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using OpenSearch.Client;
using OpenSearch.Net;
using Tests.Core.Extensions;
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Framework.EndpointTests;
using Tests.Framework.EndpointTests.TestState;

namespace Tests.ML.SearchModels
{
	// Exercises the generated ml.search_models operation end-to-end.
	//
	// Key aspect: the request body's Query property is typed as IQueryContainer — a
	// hand-written OSC type shared across the entire query DSL.  The [U] serialization
	// tests prove that a generated request can compose with existing OSC types:
	// a MatchAllQuery serializes as { "match_all": {} } inside the generated body.
	//
	// Size is a long? to verify primitive nullable mapping.
	//
	// [I]: on a fresh cluster search returns an empty hits list (200/valid), so
	// ExpectIsValid => true and ExpectStatusCode => 200. This tolerant behaviour only holds from
	// ml-commons 2.12.0 onward; earlier versions 404 with IndexNotFoundException when the backing
	// .plugins-ml-model index has not been created yet. SkipVersion suppresses only the [I] cases
	// below 2.12.0 — the [U] serialization/URL cases still run on every version.
	[SkipVersion("<2.12.0", "ml-commons search_models 404s on the missing .plugins-ml-model index before 2.12.0")]
	public class SearchModelsApiTests
		: ApiIntegrationTestBase<WritableCluster, SearchModelsResponse, ISearchModelsRequest,
			SearchModelsDescriptor, SearchModelsRequest>
	{
		public SearchModelsApiTests(WritableCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

		protected override bool ExpectIsValid => true;

		protected override object ExpectJson => new
		{
			query = new { match_all = new { } },
			size = 10,
		};

		protected override int ExpectStatusCode => 200;

		protected override Func<SearchModelsDescriptor, ISearchModelsRequest> Fluent => d => d
			.Query(new QueryContainer(new MatchAllQuery()))
			.Size(10);

		protected override HttpMethod HttpMethod => HttpMethod.POST;

		protected override SearchModelsRequest Initializer => new()
		{
			Query = new QueryContainer(new MatchAllQuery()),
			Size = 10,
		};

		protected override bool SupportsDeserialization => false;
		protected override string UrlPath => "/_plugins/_ml/models/_search";

		protected override LazyResponses ClientUsage() => Calls(
			(client, f) => client.Ml.SearchModels(f),
			(client, f) => client.Ml.SearchModelsAsync(f),
			(client, r) => client.Ml.SearchModels(r),
			(client, r) => client.Ml.SearchModelsAsync(r)
		);
	}
}
