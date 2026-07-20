/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Globalization;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Amazon;
using Amazon.Runtime;
using Amazon.Util;

namespace OpenSearch.Net.Auth.AwsSigV4
{
	public static class AwsSigV4Util
	{
		internal const string Algorithm = "AWS4-HMAC-SHA256";
		internal const string Terminator = "aws4_request";
		internal const string DateStampFormat = "yyyyMMdd";
		internal const string AmzDateFormat = "yyyyMMdd'T'HHmmss'Z'";

		public static async Task SignRequest(
			HttpRequestMessage request,
			ImmutableCredentials credentials,
			RegionEndpoint region,
			DateTime signingTime,
			string service)
		{
			var canonicalRequest = await CanonicalRequest.From(request, credentials, signingTime).ConfigureAwait(false);

			var authorizationHeader = ComputeAuthorizationHeader(
				credentials,
				region.SystemName,
				signingTime,
				service,
				canonicalRequest.SignedHeaders,
				canonicalRequest.ToString());

			request.Headers.TryAddWithoutValidation(HeaderNames.XAmzDate, canonicalRequest.XAmzDate);
			request.Headers.TryAddWithoutValidation(HeaderNames.XAmzContentSha256, canonicalRequest.XAmzContentSha256);
			request.Headers.TryAddWithoutValidation(HeaderNames.Authorization, authorizationHeader);
			if (!string.IsNullOrEmpty(canonicalRequest.XAmzSecurityToken)) request.Headers.TryAddWithoutValidation(HeaderNames.XAmzSecurityToken, canonicalRequest.XAmzSecurityToken);
		}

		// Computes the SigV4 `Authorization` header value directly from the canonical request.
		//
		// This intentionally does NOT use the AWS SDK's `Amazon.Runtime.Internal.Auth.AWS4Signer`.
		// That type is an *internal* SDK API whose behavior is not stable across major versions,
		// which caused signing to break (HTTP 401) when the host application resolved AWSSDK.Core
		// to a different major version than the one this package was compiled against (see #968).
		// The SigV4 signing algorithm itself is a small, stable, well-specified sequence of
		// HMAC-SHA256 operations, so we implement it here against public crypto primitives only.
		internal static string ComputeAuthorizationHeader(
			ImmutableCredentials credentials,
			string region,
			DateTime signingTime,
			string service,
			string signedHeaders,
			string canonicalRequest)
		{
			var signingTimeUtc = signingTime.ToUniversalTime();
			var dateStamp = signingTimeUtc.ToString(DateStampFormat, CultureInfo.InvariantCulture);
			var amzDate = signingTimeUtc.ToString(AmzDateFormat, CultureInfo.InvariantCulture);
			var credentialScope = $"{dateStamp}/{region}/{service}/{Terminator}";

			var stringToSign = string.Join("\n",
				Algorithm,
				amzDate,
				credentialScope,
				AWSSDKUtils.ToHex(Sha256(Encoding.UTF8.GetBytes(canonicalRequest)), true));

			var signingKey = DeriveSigningKey(credentials.SecretKey, dateStamp, region, service);
			var signature = AWSSDKUtils.ToHex(HmacSha256(signingKey, stringToSign), true);

			return $"{Algorithm} Credential={credentials.AccessKey}/{credentialScope}, SignedHeaders={signedHeaders}, Signature={signature}";
		}

		// kSigning = HMAC(HMAC(HMAC(HMAC("AWS4" + secretKey, dateStamp), region), service), "aws4_request")
		private static byte[] DeriveSigningKey(string secretKey, string dateStamp, string region, string service)
		{
			var kSecret = Encoding.UTF8.GetBytes("AWS4" + secretKey);
			var kDate = HmacSha256(kSecret, dateStamp);
			var kRegion = HmacSha256(kDate, region);
			var kService = HmacSha256(kRegion, service);
			return HmacSha256(kService, Terminator);
		}

		internal static byte[] Sha256(byte[] data)
		{
			using var algorithm = SHA256.Create();
			return algorithm.ComputeHash(data);
		}

		private static byte[] HmacSha256(byte[] key, string data)
		{
			using var algorithm = new HMACSHA256(key);
			return algorithm.ComputeHash(Encoding.UTF8.GetBytes(data));
		}
	}
}
