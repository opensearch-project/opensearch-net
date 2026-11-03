/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.Threading.Tasks;
using OpenSearch.Client;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Framework.EndpointTests;
using static Tests.Framework.EndpointTests.UrlTester;

namespace Tests.Indices.IndexSettings.ComposableIndexTemplates.DeleteComposableIndexTemplate;

public class DeleteComposableIndexTemplateUrlTests
{
    [U]
    public async Task Urls()
    {
        const string name = "temp";
        await DELETE($"/_index_template/{name}")
                .Fluent(c => c.Indices.DeleteComposableTemplate(name))
                .Request(c => c.Indices.DeleteComposableTemplate(new DeleteComposableIndexTemplateRequest(name)))
                .FluentAsync(c => c.Indices.DeleteComposableTemplateAsync(name))
                .RequestAsync(c => c.Indices.DeleteComposableTemplateAsync(new DeleteComposableIndexTemplateRequest(name)))
            ;
    }
}
