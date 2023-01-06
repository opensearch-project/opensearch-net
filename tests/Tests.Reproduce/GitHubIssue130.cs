/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Linq;
using System.Threading;
using OpenSearch.Client;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Core.Extensions;
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Domain;

namespace Tests.Reproduce
{
	/// <summary>
	/// Parsing histogram interval failed: <a href="https://github.com/opensearch-project/opensearch-net/issues/130">Issue #130</a>
	/// </summary>
	///
	public class GitHubIssue130 : IClusterFixture<WritableCluster>
	{
		private readonly WritableCluster _cluster;

		public GitHubIssue130(WritableCluster cluster) => _cluster = cluster;

		[I] public void CanDeserializeDateHistogramBucket()
		{
			var response = _cluster.Client.Search<Project>(c => c
				.Size(0)
				.Query(q => q.MatchAll())
				.Aggregations(a => a.Histogram("aggregation_ranges", r => r
						.Field(f => f.LastActivity)
						.Interval(5000)
					)
				)
			);

			response.ShouldBeValid();
		}
	}
}
