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

namespace Tests.Auth.AwsSigV4;

/// <summary>
/// Tests for <see cref="OpenSearch.Net.Auth.AwsSigV4.AwsSigV4HttpConnection.TunnelPort"/>. When set, the request must
/// be signed against the canonical host (no port) — so the signature still matches what AWS computes — yet be
/// dispatched to the configured local tunnel port. See issue #978.
/// </summary>
public class TunnelPortTests
{
	private static readonly BasicAWSCredentials TestCredentials = new("test-access-key", "test-secret-key");
	private static readonly RegionEndpoint TestRegion = RegionEndpoint.APSoutheast2;
	private static readonly DateTime TestSigningTime = new(2023, 01, 13, 16, 08, 37, DateTimeKind.Utc);

	// This is the exact signature the existing SignsRequestCorrectly test expects for the "es" service against the
	// port-less host. Re-using it proves the signature is unchanged by the tunnel port.
	private const string ExpectedEsSignature = "10c9be415f4b9f15b12abbb16bd3e3730b2e6c76e0cf40db75d08a44ed04a3a1";

	[U] public async Task TunnelPort_SignsWithoutPort_ButDispatchesWithPort()
	{
		var sentRequest = await SendWithTunnelPort(9200);

		// 1. The request was ultimately dispatched to the tunnel port.
		sentRequest.RequestUri!.Port.Should().Be(9200);

		// 2. The signature is identical to the no-port case: signing ignored the tunnel port.
		sentRequest.ShouldHaveHeader("Authorization",
			$"AWS4-HMAC-SHA256 Credential=test-access-key/20230113/ap-southeast-2/es/aws4_request, SignedHeaders=accept;content-type;host;x-amz-content-sha256;x-amz-date, Signature={ExpectedEsSignature}");
	}

	[U] public async Task NoTunnelPort_LeavesRequestUnchanged()
	{
		var sentRequest = await SendWithTunnelPort(null);

		// Without a tunnel port, an https URI keeps its default port (443) and the same signature holds.
		sentRequest.RequestUri!.Port.Should().Be(443);
		sentRequest.ShouldHaveHeader("Authorization",
			$"AWS4-HMAC-SHA256 Credential=test-access-key/20230113/ap-southeast-2/es/aws4_request, SignedHeaders=accept;content-type;host;x-amz-content-sha256;x-amz-date, Signature={ExpectedEsSignature}");
	}

	private static async Task<HttpRequestMessage> SendWithTunnelPort(int? tunnelPort)
	{
		var response = new HttpResponseMessage(HttpStatusCode.OK)
		{
			Content = new StringContent(@"{ ""acknowledged"": true, ""shards_acknowledged"": true, ""index"": ""sample-index1"" }",
				Encoding.UTF8, "application/json")
		};

		HttpRequestMessage sentRequest = null;

		var connection = new TestableAwsSigV4HttpConnection(TestCredentials, TestRegion, "es",
			new FixedDateTimeProvider(TestSigningTime), r =>
			{
				sentRequest = r;
				return response;
			})
		{
			TunnelPort = tunnelPort
		};

		var settings = new ConnectionSettings(new Uri("https://aaabbbcccddd111222333.ap-southeast-2.es.amazonaws.com"), connection);
		settings.DisableMetaHeader(); // Make headers & signature stable across platforms for testing
		var client = new OpenSearchClient(settings);

		await client.Indices.CreateAsync("sample-index1", d =>
			d.Settings(s => s.NumberOfShards(2).NumberOfReplicas(1))
				.Map(t => t.Properties(p => p.Number(n => n.Name("age").Type(NumberType.Integer))))
				.Aliases(a => a.Alias("sample-alias1")));

		return sentRequest;
	}
}
