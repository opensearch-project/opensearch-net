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

namespace Tests.Mapping.Types.Specialized.Wildcard;

[SkipVersion("<2.15.0", "Wildcard field type was added in OpenSearch 2.15.0")]
public class WildcardPropertyTests : PropertyTestsBase
{
    public WildcardPropertyTests(WritableCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

    protected override object ExpectJson => new
    {
        properties = new
        {
            name = new
            {
                type = "wildcard",
                null_value = "NULL",
                normalizer = "my_normalizer",
                doc_values = true,
            }
        }
    };

    protected override Func<PropertiesDescriptor<Project>, IPromise<IProperties>> FluentProperties => f => f
        .Wildcard(s => s
            .Name(p => p.Name)
            .NullValue("NULL")
            .Normalizer("my_normalizer")
            .DocValues()
        );

    protected override IProperties InitializerProperties => new Properties
    {
        {
            "name", new WildcardProperty
            {
                NullValue = "NULL",
                Normalizer = "my_normalizer",
                DocValues = true,
            }
        }
    };
}
