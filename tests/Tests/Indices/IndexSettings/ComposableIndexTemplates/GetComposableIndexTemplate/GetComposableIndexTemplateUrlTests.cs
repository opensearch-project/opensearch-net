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

namespace Tests.Indices.IndexSettings.ComposableIndexTemplates.GetComposableIndexTemplate;

public class GetComposableIndexTemplateUrlTests
{
    [U]
    public async Task Urls()
    {
        const string name = "temp";
        await GET($"/_index_template/{name}")
                .Fluent(c => c.Indices.GetComposableTemplate(name))
                .Request(c => c.Indices.GetComposableTemplate(new GetComposableIndexTemplateRequest(name)))
                .FluentAsync(c => c.Indices.GetComposableTemplateAsync(name))
                .RequestAsync(c => c.Indices.GetComposableTemplateAsync(new GetComposableIndexTemplateRequest(name)))
            ;

        await GET("/_index_template")
                .Fluent(c => c.Indices.GetComposableTemplate())
                .Request(c => c.Indices.GetComposableTemplate(new GetComposableIndexTemplateRequest()))
                .FluentAsync(c => c.Indices.GetComposableTemplateAsync())
                .RequestAsync(c => c.Indices.GetComposableTemplateAsync(new GetComposableIndexTemplateRequest()))
            ;
    }
}
