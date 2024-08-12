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

namespace Tests.Indices.IndexSettings.ComposableIndexTemplates.ComposableIndexTemplateExists;

public class ComposableIndexTemplateExistsApiTests
    : ApiTestBase<WritableCluster, ExistsResponse, IComposableIndexTemplateExistsRequest, ComposableIndexTemplateExistsDescriptor,
        ComposableIndexTemplateExistsRequest>
{
    public ComposableIndexTemplateExistsApiTests(WritableCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

    protected override Func<ComposableIndexTemplateExistsDescriptor, IComposableIndexTemplateExistsRequest> Fluent => d => d;

    protected override HttpMethod HttpMethod => HttpMethod.HEAD;

    protected override ComposableIndexTemplateExistsRequest Initializer => new(CallIsolatedValue);
    protected override string UrlPath => $"/_index_template/{CallIsolatedValue}";

    protected override LazyResponses ClientUsage() => Calls(
        (client, f) => client.Indices.ComposableTemplateExists(CallIsolatedValue, f),
        (client, f) => client.Indices.ComposableTemplateExistsAsync(CallIsolatedValue, f),
        (client, r) => client.Indices.ComposableTemplateExists(r),
        (client, r) => client.Indices.ComposableTemplateExistsAsync(r)
    );

    protected override ComposableIndexTemplateExistsDescriptor NewDescriptor() => new(CallIsolatedValue);
}
