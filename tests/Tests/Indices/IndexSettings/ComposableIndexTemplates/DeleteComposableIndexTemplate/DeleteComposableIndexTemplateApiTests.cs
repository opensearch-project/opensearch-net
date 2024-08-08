/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using OpenSearch.Client;
using OpenSearch.Net;
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Framework.EndpointTests;
using Tests.Framework.EndpointTests.TestState;

namespace Tests.Indices.IndexSettings.ComposableIndexTemplates.DeleteComposableIndexTemplate;

public class DeleteComposableIndexTemplateApiTests
    : ApiTestBase<WritableCluster, DeleteComposableIndexTemplateResponse, IDeleteComposableIndexTemplateRequest,
        DeleteComposableIndexTemplateDescriptor, DeleteComposableIndexTemplateRequest>
{
    public DeleteComposableIndexTemplateApiTests(WritableCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

    protected override HttpMethod HttpMethod => HttpMethod.DELETE;

    protected override DeleteComposableIndexTemplateRequest Initializer => new(CallIsolatedValue);
    protected override string UrlPath => $"/_index_template/{CallIsolatedValue}";

    protected override LazyResponses ClientUsage() => Calls(
        (client, f) => client.Indices.DeleteComposableTemplate(CallIsolatedValue),
        (client, f) => client.Indices.DeleteComposableTemplateAsync(CallIsolatedValue),
        (client, r) => client.Indices.DeleteComposableTemplate(r),
        (client, r) => client.Indices.DeleteComposableTemplateAsync(r)
    );
}
