/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Amazon.Runtime;
using FluentAssertions;
using OpenSearch.Net.Auth.AwsSigV4;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;

namespace Tests.Auth.AwsSigV4
{
	public class CanonicalRequestTests
	{
		private const string EmptyBodySha256 = "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855";
		private static readonly ImmutableCredentials TestCredentials = new("test-access-key", "test-secret-key", null);
		private static readonly DateTime TestSigningTime = new(2021, 05, 11, 15, 40, 45, DateTimeKind.Utc);

		[U] public async Task TestDoubleEncodePath()
		{
			var request = new HttpRequestMessage(HttpMethod.Post,
				"https://tj9n5r0m12.execute-api.us-east-1.amazonaws.com/test/@connections/JBDvjfGEIAMCERw%3D");

			await TestCanonicalRequest(request, @$"POST
/test/%40connections/JBDvjfGEIAMCERw%253D

host:tj9n5r0m12.execute-api.us-east-1.amazonaws.com
x-amz-content-sha256:{EmptyBodySha256}
x-amz-date:20210511T154045Z

host;x-amz-content-sha256;x-amz-date
{EmptyBodySha256}");
		}

		[U] public async Task TestBodyIsHashedCorrectly()
		{
			var request = new HttpRequestMessage(HttpMethod.Put,
				"https://aaabbbcccddd111222333.ap-southeast-2.aoss.amazonaws.com/sample-index1");

			var body = @"{
	""settings"": {
		""index"": {
			""number_of_shards"": 2,
			""number_of_replicas"": 1
		}
	},
	""mappings"": {
		""properties"": {
			""age"": {
				""type"": ""integer""
			}
		}
	},
	""aliases"": {
		""sample-alias1"": {}
	}
}".Replace("\r\n", "\n");

			const string bodyHash = "ea713058c116b72f762c89f7fdb5dc24751cd31be412087db17512637b5c0105";

			request.Content = new StringContent(body, Encoding.UTF8, "application/json");

			await TestCanonicalRequest(request, @$"PUT
/sample-index1

content-type:application/json; charset=utf-8
host:aaabbbcccddd111222333.ap-southeast-2.aoss.amazonaws.com
x-amz-content-sha256:{bodyHash}
x-amz-date:20210511T154045Z

content-type;host;x-amz-content-sha256;x-amz-date
{bodyHash}");
		}

		[U] public async Task TestDoubleUrlEncode()
		{
			var request = new HttpRequestMessage(HttpMethod.Post,
				"https://lambda.us-east-2.amazonaws.com/2015-03-31/functions/arn%3Aaws%3Alambda%3Aus-west-2%3A892717189312%3Afunction%3Amy-rusty-fun/invocations");

			await TestCanonicalRequest(request, @$"POST
/2015-03-31/functions/arn%253Aaws%253Alambda%253Aus-west-2%253A892717189312%253Afunction%253Amy-rusty-fun/invocations

host:lambda.us-east-2.amazonaws.com
x-amz-content-sha256:{EmptyBodySha256}
x-amz-date:20210511T154045Z

host;x-amz-content-sha256;x-amz-date
{EmptyBodySha256}");
		}

		[U] public async Task TestTildeInUri()
		{
			var request = new HttpRequestMessage(HttpMethod.Get,
				"https://s3.us-east-1.amazonaws.com/my-bucket?list-type=2&prefix=~objprefix&single&k=&unreserved=-_.~");

			await TestCanonicalRequest(request, @$"GET
/my-bucket
k=&list-type=2&prefix=~objprefix&single=&unreserved=-_.~
host:s3.us-east-1.amazonaws.com
x-amz-content-sha256:{EmptyBodySha256}
x-amz-date:20210511T154045Z

host;x-amz-content-sha256;x-amz-date
{EmptyBodySha256}");
		}

		[U] public async Task TestQueryParamMultipleValues()
		{
			var request = new HttpRequestMessage(HttpMethod.Get,
				"https://s3.us-east-1.amazonaws.com/my-bucket?list-type=2&list-type=1");

			await TestCanonicalRequest(request, @$"GET
/my-bucket
list-type=1&list-type=2
host:s3.us-east-1.amazonaws.com
x-amz-content-sha256:{EmptyBodySha256}
x-amz-date:20210511T154045Z

host;x-amz-content-sha256;x-amz-date
{EmptyBodySha256}");
		}

		[U] public static async Task TestAllPrintableAsciiQueryParam()
		{
			var printableAscii = string.Concat(Enumerable.Range(32, 95).Select(c => (char)c));
			var queryParams = HttpUtility.ParseQueryString(string.Empty);
			queryParams["list-type"] = "2";
			queryParams["prefix"] = printableAscii;

			var uri = new UriBuilder
				{
					Scheme = "https",
					Host = "s3.us-east-1.amazonaws.com",
					Path = "/my-bucket",
					Query = queryParams.ToString() ?? string.Empty
				}
				.Uri;

			var request = new HttpRequestMessage(HttpMethod.Get, uri);

			await TestCanonicalRequest(request, @$"GET
/my-bucket
list-type=2&prefix=%20%21%22%23%24%25%26%27%28%29%2A%2B%2C-.%2F0123456789%3A%3B%3C%3D%3E%3F%40ABCDEFGHIJKLMNOPQRSTUVWXYZ%5B%5C%5D%5E_%60abcdefghijklmnopqrstuvwxyz%7B%7C%7D~
host:s3.us-east-1.amazonaws.com
x-amz-content-sha256:{EmptyBodySha256}
x-amz-date:20210511T154045Z

host;x-amz-content-sha256;x-amz-date
{EmptyBodySha256}");
		}

		private static async Task TestCanonicalRequest(HttpRequestMessage request, string expected)
		{
			var canonicalRequest = await CanonicalRequest.From(request, TestCredentials, TestSigningTime);

			canonicalRequest
				.ToString()
				.Should()
				.Be(expected.Replace("\r\n", "\n"));
		}
	}
}
