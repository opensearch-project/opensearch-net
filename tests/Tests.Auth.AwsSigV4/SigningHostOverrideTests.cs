/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Collections.Specialized;
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
/// Tests that the SigV4 signature is computed against a caller-supplied <c>Host</c> header when present, falling back
/// to the request URI's authority otherwise. This lets a request be dispatched to a different host/port than it is
/// signed for — e.g. tunnelling to a cluster on a private subnet via SSH local port forwarding, or connecting directly
/// to <c>http://localhost:9200</c> — while the signature still matches what AWS verifies. See issue #978.
/// </summary>
public class SigningHostOverrideTests
{
	private static readonly BasicAWSCredentials TestCredentials = new("test-access-key", "test-secret-key");
	private static readonly RegionEndpoint TestRegion = RegionEndpoint.APSoutheast2;
	private static readonly DateTime TestSigningTime = new(2023, 01, 13, 16, 08, 37, DateTimeKind.Utc);

	private const string RealEndpoint = "aaabbbcccddd111222333.ap-southeast-2.es.amazonaws.com";

	// This is the exact signature the SignsRequestCorrectly test expects for the "es" service against the real
	// endpoint. Re-using it proves the signature is unchanged when connecting via a tunnel but overriding the Host.
	private const string ExpectedEsSignature = "10c9be415f4b9f15b12abbb16bd3e3730b2e6c76e0cf40db75d08a44ed04a3a1";

	private const string ExpectedAuthorization =
		"AWS4-HMAC-SHA256 Credential=test-access-key/20230113/ap-southeast-2/es/aws4_request, "
		+ "SignedHeaders=accept;content-type;host;x-amz-content-sha256;x-amz-date, Signature=" + ExpectedEsSignature;

	[U] public async Task Tunnel_SignsForHostHeader_ButDispatchesToTunnel()
	{
		// Connect to a local tunnel, but override the Host header to the real endpoint.
		var sentRequest = await SendAsync($"http://localhost:9200", hostHeader: RealEndpoint);

		// 1. The request was dispatched to the local tunnel, not the real endpoint.
		sentRequest.RequestUri!.Host.Should().Be("localhost");
		sentRequest.RequestUri!.Port.Should().Be(9200);

		// 2. The signature matches the real endpoint: signing used the Host header, not the tunnel URI.
		sentRequest.ShouldHaveHeader("Authorization", ExpectedAuthorization);
	}

	[U] public async Task NoHostHeader_SignsForRequestUri()
	{
		// With no Host override, connecting straight to the real endpoint produces the same signature.
		var sentRequest = await SendAsync($"https://{RealEndpoint}", hostHeader: null);

		sentRequest.RequestUri!.Host.Should().Be(RealEndpoint);
		sentRequest.ShouldHaveHeader("Authorization", ExpectedAuthorization);
	}

	private static async Task<HttpRequestMessage> SendAsync(string connectionUri, string hostHeader)
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
			});

		var settings = new ConnectionSettings(new Uri(connectionUri), connection);
		settings.DisableMetaHeader(); // Make headers & signature stable across platforms for testing
		if (hostHeader != null) settings.GlobalHeaders(new NameValueCollection { { "Host", hostHeader } });
		var client = new OpenSearchClient(settings);

		await client.Indices.CreateAsync("sample-index1", d =>
			d.Settings(s => s.NumberOfShards(2).NumberOfReplicas(1))
				.Map(t => t.Properties(p => p.Number(n => n.Name("age").Type(NumberType.Integer))))
				.Aliases(a => a.Alias("sample-alias1")));

		return sentRequest;
	}
}
