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
	public class ComponentTemplateExistsUrlTests
	{
		[U] public async Task Urls()
		{
			var name = "temp";
			await HEAD($"/_component_template/{name}")
					.Fluent(c => c.Cluster.ComponentTemplateExists(name))
					.Request(c => c.Cluster.ComponentTemplateExists(new ComponentTemplateExistsRequest(name)))
					.FluentAsync(c => c.Cluster.ComponentTemplateExistsAsync(name))
					.RequestAsync(c => c.Cluster.ComponentTemplateExistsAsync(new ComponentTemplateExistsRequest(name)))
				;
		}
	}
}
