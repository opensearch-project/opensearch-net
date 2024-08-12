/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using OpenSearch.Client;
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Domain;
using Tests.Framework.EndpointTests.TestState;

namespace Tests.Mapping.Types.Specialized.Knn;

public class KnnVectorPropertyTests : PropertyTestsBase
{
    public KnnVectorPropertyTests(WritableCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

    protected override object ExpectJson => new
    {
        properties = new
        {
            name = new
            {
                type = "knn_vector",
                dimension = 2,
                method = new
                {
                    name = "hnsw",
                    space_type = "l2",
                    engine = "nmslib",
                    parameters = new
                    {
                        ef_construction = 128,
                        m = 24
                    }
                }
            }
        }
    };

    protected override Func<PropertiesDescriptor<Project>, IPromise<IProperties>> FluentProperties => f => f
        .KnnVector(k => k
            .Name(p => p.Name)
            .Dimension(2)
            .Method(m => m
                .Name("hnsw")
                .SpaceType("l2")
                .Engine("nmslib")
                .Parameters(p => p
                    .Parameter("ef_construction", 128)
                    .Parameter("m", 24)
                )
            )
        );

    protected override IProperties InitializerProperties => new Properties
    {
        {
            "name", new KnnVectorProperty
            {
                Dimension = 2,
                Method = new KnnMethod
                {
                    Name = "hnsw",
                    SpaceType = "l2",
                    Engine = "nmslib",
                    Parameters = new KnnMethodParameters
                    {
                        {"ef_construction", 128},
                        {"m", 24}
                    }
                }
            }
        }
    };
}
