/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Amazon;
using Amazon.Runtime;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Auth.AwsSigV4.Utils;
using Tests.Core.Connection.Http;
using Xunit;

namespace Tests.Auth.AwsSigV4;

public class AwsSigV4HttpConnectionTests
{
	private static readonly BasicAWSCredentials TestCredentials = new("test-access-key", "test-secret-key");
	private static readonly RegionEndpoint TestRegion = RegionEndpoint.APSoutheast2;
	private static readonly DateTime TestSigningTime = new(2023, 01, 13, 16, 08, 37, DateTimeKind.Utc);

	[TU]
	[InlineData("es", "10c9be415f4b9f15b12abbb16bd3e3730b2e6c76e0cf40db75d08a44ed04a3a1")]
	[InlineData("aoss", "34903aef90423aa7dd60575d3d45316c6ef2d57bbe564a152b41bf8f5917abf6")]
	[InlineData("arbitrary", "156e65c504ea2b2722a481b7515062e7692d27217b477828854e715f507e6a36")]
	public async Task SignsRequestCorrectly(string service, string expectedSignature)
	{
		var response = new HttpResponseMessage(HttpStatusCode.OK);
		response.Content = new StringContent(@"{
	""acknowledged"": true,
	""shards_acknowledged"": true,
    ""index"": ""sample-index1""
}", Encoding.UTF8, "application/json");

		HttpRequestMessage sentRequest = null;

		var client = CreateClient(r =>
		{
			sentRequest = r;
			return response;
		}, $"https://aaabbbcccddd111222333.ap-southeast-2.{service}.amazonaws.com", service);

		await client.Indices.CreateAsync("sample-index1", d =>
			d.Settings(s =>
					s.NumberOfShards(2).NumberOfReplicas(1))
				.Map(t =>
					t.Properties(p =>
						p.Number(n =>
							n.Name("age").Type(NumberType.Integer))))
				.Aliases(a => a.Alias("sample-alias1")));

		sentRequest.ShouldHaveHeader("x-amz-date", "20230113T160837Z");
		sentRequest.ShouldHaveHeader("x-amz-content-sha256", "4c770eaed349122a28302ff73d34437cad600acda5a9dd373efc7da2910f8564");
		sentRequest.ShouldHaveHeader("Authorization", $"AWS4-HMAC-SHA256 Credential=test-access-key/20230113/ap-southeast-2/{service}/aws4_request, SignedHeaders=accept;content-type;host;x-amz-content-sha256;x-amz-date, Signature={expectedSignature}");
	}

	private static OpenSearchClient CreateClient(MockHttpMessageHandler.Handler handler, string uri, string service)
	{
		var connection =
			new TestableAwsSigV4HttpConnection(TestCredentials, TestRegion, service, new FixedDateTimeProvider(TestSigningTime), handler);
		var settings = new ConnectionSettings(new Uri(uri), connection);
		settings.DisableMetaHeader(); // Make headers & signature stable across platforms for testing
		return new OpenSearchClient(settings);
	}
}
