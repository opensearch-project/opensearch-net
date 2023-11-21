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

namespace Tests.Indices.IndexSettings.ComposableIndexTemplates.PutComposableIndexTemplate;

public class PutComposableIndexTemplateUrlTests
{
	[U] public async Task Urls()
	{
		const string name = "temp";
		await PUT($"/_index_template/{name}")
				.Fluent(c => c.Indices.PutComposableTemplate(name, p => p))
				.Request(c => c.Indices.PutComposableTemplate(new PutComposableIndexTemplateRequest(name)))
				.FluentAsync(c => c.Indices.PutComposableTemplateAsync(name, p => p))
				.RequestAsync(c => c.Indices.PutComposableTemplateAsync(new PutComposableIndexTemplateRequest(name)))
			;
	}
}
