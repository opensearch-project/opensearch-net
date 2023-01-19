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
using Xunit;

namespace Tests.Auth.AwsSigV4;

public class AwsSigV4HttpConnectionTests
{
	private static readonly BasicAWSCredentials TestCredentials = new("test-access-key", "test-secret-key");
	private static readonly RegionEndpoint TestRegion = RegionEndpoint.APSoutheast2;
	private static readonly DateTime TestSigningTime = new(2023, 01, 13, 16, 08, 37, DateTimeKind.Utc);

	[TU]
	[InlineData("es", "275d72b784a3183861ae13b4fcb486ff31a7a9dd7f2d6ebb780f89008aa689cc")]
	[InlineData("aoss", "89fb38096ed2e9f4a2908d1e40c3e1b7ac68fc4ab2f723feddec61b4ada4607a")]
	[InlineData("arbitrary", "9bdf7d1cf23d8ca4b41f84cd15aafbc7b665dd4985cf68258c9e7ac40311521b")]
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
		sentRequest.ShouldHaveHeader("Authorization", $"AWS4-HMAC-SHA256 Credential=test-access-key/20230113/ap-southeast-2/{service}/aws4_request, SignedHeaders=accept;content-type;host;opensearch-client-meta;x-amz-content-sha256;x-amz-date, Signature={expectedSignature}");
	}

	private static OpenSearchClient CreateClient(MockHttpMessageHandler.Handler handler, string uri, string service) =>
		new(new ConnectionSettings(
			new Uri(uri),
			new TestableAwsSigV4HttpConnection(TestCredentials, TestRegion, service, new FixedDateTimeProvider(TestSigningTime), handler)
			));
}
