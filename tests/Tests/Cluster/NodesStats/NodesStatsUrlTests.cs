/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/
/*
* Modifications Copyright OpenSearch Contributors. See
* GitHub history for details.
*
*  Licensed to Elasticsearch B.V. under one or more contributor
*  license agreements. See the NOTICE file distributed with
*  this work for additional information regarding copyright
*  ownership. Elasticsearch B.V. licenses this file to you under
*  the Apache License, Version 2.0 (the "License"); you may
*  not use this file except in compliance with the License.
*  You may obtain a copy of the License at
*
* 	http://www.apache.org/licenses/LICENSE-2.0
*
*  Unless required by applicable law or agreed to in writing,
*  software distributed under the License is distributed on an
*  "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
*  KIND, either express or implied.  See the License for the
*  specific language governing permissions and limitations
*  under the License.
*/

using System.Threading.Tasks;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using OpenSearch.Net;
using OpenSearch.Client;
using Tests.Framework.EndpointTests;
using static Tests.Framework.EndpointTests.UrlTester;

namespace Tests.Cluster.NodesStats
{
	public class NodesStatsUrlTests : UrlTestsBase
	{
		[U] public override async Task Urls()
		{
			await GET("/_nodes/stats")
					.Fluent(c => c.Nodes.Stats())
					.Request(c => c.Nodes.Stats(new NodesStatsRequest()))
					.FluentAsync(c => c.Nodes.StatsAsync())
					.RequestAsync(c => c.Nodes.StatsAsync(new NodesStatsRequest()))
				;

			await GET("/_nodes/foo/stats")
					.Fluent(c => c.Nodes.Stats(n => n.NodeId("foo")))
					.Request(c => c.Nodes.Stats(new NodesStatsRequest("foo")))
					.FluentAsync(c => c.Nodes.StatsAsync(n => n.NodeId("foo")))
					.RequestAsync(c => c.Nodes.StatsAsync(new NodesStatsRequest("foo")))
				;

			var metrics = NodesStatsMetric.Fs | NodesStatsMetric.Jvm;
			await GET("/_nodes/stats/fs%2Cjvm")
					.Fluent(c => c.Nodes.Stats(p => p.Metric(metrics)))
					.Request(c => c.Nodes.Stats(new NodesStatsRequest(metrics)))
					.FluentAsync(c => c.Nodes.StatsAsync(p => p.Metric(metrics)))
					.RequestAsync(c => c.Nodes.StatsAsync(new NodesStatsRequest(metrics)))
				;

			await GET("/_nodes/foo/stats/fs%2Cjvm")
					.Fluent(c => c.Nodes.Stats(p => p.NodeId("foo").Metric(metrics)))
					.Request(c => c.Nodes.Stats(new NodesStatsRequest("foo", metrics)))
					.FluentAsync(c => c.Nodes.StatsAsync(p => p.NodeId("foo").Metric(metrics)))
					.RequestAsync(c => c.Nodes.StatsAsync(new NodesStatsRequest("foo", metrics)))
				;

			var indexMetrics = NodesStatsIndexMetric.Fielddata | NodesStatsIndexMetric.Merge;
			await GET("/_nodes/stats/fs%2Cjvm/fielddata%2Cmerge")
					.Fluent(c => c.Nodes.Stats(p => p.Metric(metrics).IndexMetric(indexMetrics)))
					.Request(c => c.Nodes.Stats(new NodesStatsRequest(metrics, indexMetrics)))
					.FluentAsync(c => c.Nodes.StatsAsync(p => p.Metric(metrics).IndexMetric(indexMetrics)))
					.RequestAsync(c => c.Nodes.StatsAsync(new NodesStatsRequest(metrics, indexMetrics)))
				;

			await GET("/_nodes/foo/stats/fs%2Cjvm/fielddata%2Cmerge")
					.Fluent(c => c.Nodes.Stats(p => p.NodeId("foo").Metric(metrics).IndexMetric(indexMetrics)))
					.Request(c => c.Nodes.Stats(new NodesStatsRequest("foo", metrics, indexMetrics)))
					.FluentAsync(c => c.Nodes.StatsAsync(p => p.NodeId("foo").Metric(metrics).IndexMetric(indexMetrics)))
					.RequestAsync(c => c.Nodes.StatsAsync(new NodesStatsRequest("foo", metrics, indexMetrics)))
				;
		}
	}
}
