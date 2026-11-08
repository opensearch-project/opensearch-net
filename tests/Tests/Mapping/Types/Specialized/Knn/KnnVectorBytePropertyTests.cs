/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using OpenSearch.Client;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Domain;
using Tests.Framework.EndpointTests.TestState;

namespace Tests.Mapping.Types.Specialized.Knn;

/// <summary>
/// Integration coverage for a <c>knn_vector</c> field declared with <c>data_type: "byte"</c>.
/// The <c>data_type</c> parameter was introduced in the k-NN plugin in OpenSearch 2.9, so the
/// integration assertions are skipped on older clusters. The unit serialization assertions in
/// <see cref="KnnVectorDataTypeTests"/> are version-independent.
/// </summary>
[SkipVersion("<2.9.0", "The knn_vector data_type parameter (byte vectors) was introduced in OpenSearch 2.9")]
public class KnnVectorBytePropertyTests : PropertyTestsBase
{
	public KnnVectorBytePropertyTests(WritableCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

	protected override object ExpectJson => new
	{
		properties = new
		{
			name = new
			{
				type = "knn_vector",
				dimension = 3,
				data_type = "byte",
				method = new
				{
					name = "hnsw",
					space_type = "l2",
					engine = "lucene",
				}
			}
		}
	};

	protected override Func<PropertiesDescriptor<Project>, IPromise<IProperties>> FluentProperties => f => f
		.KnnVector(k => k
			.Name(p => p.Name)
			.Dimension(3)
			.DataType("byte")
			.Method(m => m
				.Name("hnsw")
				.SpaceType("l2")
				.Engine("lucene")
			)
		);

	protected override IProperties InitializerProperties => new Properties
	{
		{
			"name", new KnnVectorProperty
			{
				Dimension = 3,
				DataType = "byte",
				Method = new KnnMethod
				{
					Name = "hnsw",
					SpaceType = "l2",
					Engine = "lucene",
				}
			}
		}
	};

	protected override void IntegrationSetup(IOpenSearchClient client, CallUniqueValues values)
	{
		foreach (var v in values.Values)
		{
			client.Indices.Create(v, d => d
				.Settings(s => s
					.Setting("index.knn", true)
					.Setting("index.knn.algo_param.ef_search", 100)));
		}
	}

	protected override void IntegrationTeardown(IOpenSearchClient client, CallUniqueValues values)
	{
		foreach (var v in values.Values) client.Indices.Delete(v);
	}
}
