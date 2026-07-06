/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using OpenSearch.Client;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Core.Client;
using Tests.Domain;
using static Tests.Core.Serialization.SerializationTestHelper;

namespace Tests.QueryDsl.Specialized.Knn
{
	/// <summary>
	/// Fast, deterministic request-serialization coverage for the k-NN plugin query (#388). The base
	/// <see cref="KnnQueryUsageTests"/> only exercises <c>vector</c>/<c>k</c>; the k-NN-specific
	/// <c>max_distance</c>, <c>min_score</c>, and the interface-typed <c>filter</c> (an
	/// <see cref="IQueryContainer"/>) otherwise have no unit coverage. These pin the on-the-wire shape
	/// so the plugin query keeps serializing correctly under the System.Text.Json serializer.
	/// </summary>
	public class KnnQuerySerializationTests
	{
		private readonly IOpenSearchClient _client = TestClient.DisabledStreaming;

		private static readonly object Expected = new
		{
			query = new
			{
				knn = new
				{
					vector = new
					{
						vector = new[] { 1.5f, -2.6f },
						k = 30,
						filter = new { term = new { status = new { value = "active" } } },
						max_distance = 0.5f,
						min_score = 0.8f
					}
				}
			}
		};

		[U]
		public void Fluent() =>
			Expect(Expected).FromRequest(_client.Search<Project>(s => s
				.Query(q => q
					.Knn(k => k
						.Field(f => f.Vector)
						.Vector(1.5f, -2.6f)
						.K(30)
						.MaxDistance(0.5f)
						.MinScore(0.8f)
						.Filter(f => f.Term(t => t.Field("status").Value("active")))
					)
				)
			));

		[U]
		public void Initializer() =>
			Expect(Expected).FromRequest(_client.Search<Project>(new SearchRequest<Project>
			{
				Query = new KnnQuery
				{
					Field = Infer.Field<Project>(f => f.Vector),
					Vector = new[] { 1.5f, -2.6f },
					K = 30,
					MaxDistance = 0.5f,
					MinScore = 0.8f,
					Filter = new QueryContainer(new TermQuery { Field = "status", Value = "active" })
				}
			}));
	}
}
