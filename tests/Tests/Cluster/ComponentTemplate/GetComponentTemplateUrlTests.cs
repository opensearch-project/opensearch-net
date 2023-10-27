/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.Threading.Tasks;
using OpenSearch.Client.Specification.ClusterApi;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Framework.EndpointTests;
using static Tests.Framework.EndpointTests.UrlTester;

namespace Tests.Cluster.ComponentTemplate
{
	public class GetComponentTemplateUrlTests
	{
		[U] public async Task Urls()
		{
			var name = "temp";
			await GET($"/_component_template/{name}")
					.Fluent(c => c.Cluster.GetComponentTemplate(name))
					.Request(c => c.Cluster.GetComponentTemplate(new GetComponentTemplateRequest(name)))
					.FluentAsync(c => c.Cluster.GetComponentTemplateAsync(name))
					.RequestAsync(c => c.Cluster.GetComponentTemplateAsync(new GetComponentTemplateRequest(name)))
				;

			await GET($"/_component_template")
					.Fluent(c => c.Cluster.GetComponentTemplate())
					.Request(c => c.Cluster.GetComponentTemplate(new GetComponentTemplateRequest()))
					.FluentAsync(c => c.Cluster.GetComponentTemplateAsync())
					.RequestAsync(c => c.Cluster.GetComponentTemplateAsync(new GetComponentTemplateRequest()))
				;
		}
	}
}
