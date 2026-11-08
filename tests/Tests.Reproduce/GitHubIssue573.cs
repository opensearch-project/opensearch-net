/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Text;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.Net;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Core.Extensions;

namespace Tests.Reproduce;

/// <summary>
/// S3 Snapshot Repository Without Settings Fails to Deserialize: <a href="https://github.com/opensearch-project/opensearch-net/issues/573">Issue #573</a>
/// </summary>
public class GitHubIssue573
{
	[U] public void DeserializingS3SnapshotRepositoryWithoutSettingsShouldSucceed()
	{
		var pool = new SingleNodeConnectionPool(new Uri("http://localhost:9200"));

		const string json = @"{
				""cs-automated"": {
					""type"": ""s3""
				},
				""authoring-service-snapshots"": {
					""type"": ""s3"",
					""settings"": {
						""bucket"": ""some-bucket"",
						""region"": ""us-west-2"",
						""role_arn"": ""arn:aws:iam::123456789:role/SomeRole""
					}
				}
			}";

		var connection = new InMemoryConnection(Encoding.UTF8.GetBytes(json), 200);
		var settings = new ConnectionSettings(pool, connection);
		var client = new OpenSearchClient(settings);

		var response = client.Snapshot.GetRepository();
		response.ShouldBeValid();
		response.Repositories
			.Should()
			.NotBeNull()
			.And.HaveCount(2)
			.And.ContainKeys("cs-automated", "authoring-service-snapshots")
			.And.AllSatisfy(p => p.Value.Should().BeOfType<S3Repository>());

		((S3Repository) response.Repositories["cs-automated"]).Settings.Should().BeNull();

		((S3Repository) response.Repositories["authoring-service-snapshots"]).Settings.Should().NotBeNull();
	}
}
