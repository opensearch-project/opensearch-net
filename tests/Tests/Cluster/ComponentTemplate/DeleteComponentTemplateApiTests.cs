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

public class DeleteComponentTemplateApiTests
    : ApiTestBase<WritableCluster, DeleteComponentTemplateResponse, IDeleteComponentTemplateRequest, DeleteComponentTemplateDescriptor,
        DeleteComponentTemplateRequest>
{
    public DeleteComponentTemplateApiTests(WritableCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

    protected override Func<DeleteComponentTemplateDescriptor, IDeleteComponentTemplateRequest> Fluent => d => d;
    protected override HttpMethod HttpMethod => HttpMethod.DELETE;
    protected override DeleteComponentTemplateRequest Initializer => new(CallIsolatedValue);
    protected override string UrlPath => $"/_component_template/{CallIsolatedValue}";

    protected override LazyResponses ClientUsage() => Calls(
        (client, f) => client.Cluster.DeleteComponentTemplate(CallIsolatedValue, f),
        (client, f) => client.Cluster.DeleteComponentTemplateAsync(CallIsolatedValue, f),
        (client, r) => client.Cluster.DeleteComponentTemplate(r),
        (client, r) => client.Cluster.DeleteComponentTemplateAsync(r)
    );

    protected override DeleteComponentTemplateDescriptor NewDescriptor() => new(CallIsolatedValue);
}
