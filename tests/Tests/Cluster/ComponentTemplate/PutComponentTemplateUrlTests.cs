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

namespace Tests.Cluster.ComponentTemplate;

public class PutComponentTemplateUrlTests
{
    [U]
    public async Task Urls()
    {
        var name = "temp";
        await PUT($"/_component_template/{name}")
                .Fluent(c => c.Cluster.PutComponentTemplate(name, p => p))
                .Request(c => c.Cluster.PutComponentTemplate(new PutComponentTemplateRequest(name)))
                .FluentAsync(c => c.Cluster.PutComponentTemplateAsync(name, p => p))
                .RequestAsync(c => c.Cluster.PutComponentTemplateAsync(new PutComponentTemplateRequest(name)))
            ;
    }
}
