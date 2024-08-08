/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using OpenSearch.Client;
using OpenSearch.Net;
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Framework.EndpointTests;
using Tests.Framework.EndpointTests.TestState;

namespace Tests.Cluster.ComponentTemplate;

public class ComponentTemplateExistsApiTests
    : ApiTestBase<WritableCluster, ExistsResponse, IComponentTemplateExistsRequest, ComponentTemplateExistsDescriptor, ComponentTemplateExistsRequest>
{
    public ComponentTemplateExistsApiTests(WritableCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

    protected override Func<ComponentTemplateExistsDescriptor, IComponentTemplateExistsRequest> Fluent => d => d;

    protected override HttpMethod HttpMethod => HttpMethod.HEAD;

    protected override ComponentTemplateExistsRequest Initializer => new(CallIsolatedValue);
    protected override string UrlPath => $"/_component_template/{CallIsolatedValue}";

    protected override LazyResponses ClientUsage() => Calls(
        (client, f) => client.Cluster.ComponentTemplateExists(CallIsolatedValue, f),
        (client, f) => client.Cluster.ComponentTemplateExistsAsync(CallIsolatedValue, f),
        (client, r) => client.Cluster.ComponentTemplateExists(r),
        (client, r) => client.Cluster.ComponentTemplateExistsAsync(r)
    );

    protected override ComponentTemplateExistsDescriptor NewDescriptor() => new(CallIsolatedValue);
}
