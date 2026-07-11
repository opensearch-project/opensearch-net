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
using OpenSearch.Net.Auth.AwsSigV4;
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
		var sentRequest = await CreateIndexAndCaptureRequest(service, TestSigningTime);

		sentRequest.ShouldHaveHeader("x-amz-date", "20230113T160837Z");
		sentRequest.ShouldHaveHeader("x-amz-content-sha256", "4c770eaed349122a28302ff73d34437cad600acda5a9dd373efc7da2910f8564");
		sentRequest.ShouldHaveHeader("Authorization", $"AWS4-HMAC-SHA256 Credential=test-access-key/20230113/ap-southeast-2/{service}/aws4_request, SignedHeaders=accept;content-type;host;x-amz-content-sha256;x-amz-date, Signature={expectedSignature}");
	}

	[U]
	public async Task SignsRequestCorrectlyWithNonUtcSigningTime()
	{
		// SigV4 mandates UTC. A custom IDateTimeProvider may return a non-UTC DateTime, so the
		// signer normalizes via ToUniversalTime(). A Local-kind time representing the *same instant*
		// as the UTC vector must therefore yield a byte-identical x-amz-date and signature.
		// ToLocalTime() -> ToUniversalTime() round-trips to the same instant regardless of the
		// machine's time zone, keeping this assertion deterministic across platforms.
		var localSigningTime = TestSigningTime.ToLocalTime();
		localSigningTime.Kind.Should().Be(DateTimeKind.Local);

		var sentRequest = await CreateIndexAndCaptureRequest("aoss", localSigningTime);

		sentRequest.ShouldHaveHeader("x-amz-date", "20230113T160837Z");
		sentRequest.ShouldHaveHeader("x-amz-content-sha256", "4c770eaed349122a28302ff73d34437cad600acda5a9dd373efc7da2910f8564");
		sentRequest.ShouldHaveHeader("Authorization", "AWS4-HMAC-SHA256 Credential=test-access-key/20230113/ap-southeast-2/aoss/aws4_request, SignedHeaders=accept;content-type;host;x-amz-content-sha256;x-amz-date, Signature=34903aef90423aa7dd60575d3d45316c6ef2d57bbe564a152b41bf8f5917abf6");
	}

	[U]
	public void ComputeAuthorizationHeaderMatchesPublishedAwsReferenceVector()
	{
		// Pins the SigV4 signing to AWS's published reference example (GET iam.amazonaws.com
		// ?Action=ListUsers&Version=2010-05-08), locking in spec-correctness independently of any
		// AWSSDK.Core version. See the worked example in the AWS General Reference:
		// https://docs.aws.amazon.com/general/latest/gr/sigv4-create-canonical-request.html
		// https://docs.aws.amazon.com/general/latest/gr/sigv4-calculate-signature.html
		// https://docs.aws.amazon.com/general/latest/gr/sigv4-add-signature-to-request.html
		var credentials = new ImmutableCredentials("AKIDEXAMPLE", "wJalrXUtnFEMI/K7MDENG+bPxRfiCYEXAMPLEKEY", null);
		var signingTime = new DateTime(2015, 08, 30, 12, 36, 00, DateTimeKind.Utc);
		const string signedHeaders = "content-type;host;x-amz-date";
		const string emptyBodySha256 = "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855";

		var canonicalRequest = string.Join("\n",
			"GET",
			"/",
			"Action=ListUsers&Version=2010-05-08",
			"content-type:application/x-www-form-urlencoded; charset=utf-8",
			"host:iam.amazonaws.com",
			"x-amz-date:20150830T123600Z",
			"",
			signedHeaders,
			emptyBodySha256);

		var authorization = AwsSigV4Util.ComputeAuthorizationHeader(
			credentials, "us-east-1", signingTime, "iam", signedHeaders, canonicalRequest);

		authorization.Should().Be(
			"AWS4-HMAC-SHA256 Credential=AKIDEXAMPLE/20150830/us-east-1/iam/aws4_request, "
			+ "SignedHeaders=content-type;host;x-amz-date, "
			+ "Signature=5d672d79c15b13162d9279b0855cfba6789a8edb4c82c400e06b5924a6f2b5d7");
	}

	[U]
	public async Task SignsRequestWithSessionTokenIncludedInSignature()
	{
		// Temporary credentials (e.g. STS / EKS Pod Identity — the scenario in #968) carry a
		// session token that must be sent as x-amz-security-token AND included in the signed
		// headers. Known-answer independently verified against a spec-compliant computation.
		var credentials = new ImmutableCredentials("AKIDEXAMPLE", "wJalrXUtnFEMI/K7MDENG+bPxRfiCYEXAMPLEKEY", "AQoEXAMPLESESSIONTOKEN");
		var signingTime = new DateTime(2015, 08, 30, 12, 36, 00, DateTimeKind.Utc);
		var request = new HttpRequestMessage(HttpMethod.Get, "https://iam.amazonaws.com/?Action=ListUsers&Version=2010-05-08");

		await AwsSigV4Util.SignRequest(request, credentials, RegionEndpoint.USEast1, signingTime, "iam");

		request.ShouldHaveHeader("x-amz-date", "20150830T123600Z");
		request.ShouldHaveHeader("x-amz-content-sha256", "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855");
		request.ShouldHaveHeader("x-amz-security-token", "AQoEXAMPLESESSIONTOKEN");
		request.ShouldHaveHeader("Authorization",
			"AWS4-HMAC-SHA256 Credential=AKIDEXAMPLE/20150830/us-east-1/iam/aws4_request, "
			+ "SignedHeaders=host;x-amz-content-sha256;x-amz-date;x-amz-security-token, "
			+ "Signature=531810b58ef456f2afa3206411d237a2f99af04312fcef2b59e11f7b3dba8a21");
	}

	private static async Task<HttpRequestMessage> CreateIndexAndCaptureRequest(string service, DateTime signingTime)
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
		}, $"https://aaabbbcccddd111222333.ap-southeast-2.{service}.amazonaws.com", service, signingTime);

		await client.Indices.CreateAsync("sample-index1", d =>
			d.Settings(s =>
					s.NumberOfShards(2).NumberOfReplicas(1))
				.Map(t =>
					t.Properties(p =>
						p.Number(n =>
							n.Name("age").Type(NumberType.Integer))))
				.Aliases(a => a.Alias("sample-alias1")));

		return sentRequest;
	}

	private static OpenSearchClient CreateClient(MockHttpMessageHandler.Handler handler, string uri, string service, DateTime signingTime)
	{
		var connection =
			new TestableAwsSigV4HttpConnection(TestCredentials, TestRegion, service, new FixedDateTimeProvider(signingTime), handler);
		var settings = new ConnectionSettings(new Uri(uri), connection);
		settings.DisableMetaHeader(); // Make headers & signature stable across platforms for testing
		return new OpenSearchClient(settings);
	}
}
