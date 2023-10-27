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

namespace Tests.Cluster.ComponentTemplate;

public class DeleteComponentTemplateUrlTests
{
	[U] public async Task Urls()
	{
		var name = "temp";
		await DELETE($"/_component_template/{name}")
				.Fluent(c => c.Cluster.DeleteComponentTemplate(name))
				.Request(c => c.Cluster.DeleteComponentTemplate(new DeleteComponentTemplateRequest(name)))
				.FluentAsync(c => c.Cluster.DeleteComponentTemplateAsync(name))
				.RequestAsync(c => c.Cluster.DeleteComponentTemplateAsync(new DeleteComponentTemplateRequest(name)))
			;
	}
}