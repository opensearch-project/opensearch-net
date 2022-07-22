/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

namespace OpenSearch.Net.Auth.AwsSigV4
{
	using System;
#if DOTNETCORE
	using System.Net.Http;
	using System.Threading.Tasks;
#else
	using System.Net;
#endif
	using Amazon;
	using Amazon.Runtime;
	using Amazon.Runtime.Internal.Auth;

	public static class AwsSigV4Util
	{
#if DOTNETCORE
		public static async Task SignRequest(
			HttpRequestMessage request,
			ImmutableCredentials credentials,
			RegionEndpoint region,
			DateTime signingTime)
		{
			var canonicalRequest = await CanonicalRequest.From(request, credentials, signingTime).ConfigureAwait(false);

			var signature = AWS4Signer.ComputeSignature(credentials, region.SystemName, signingTime, "es", canonicalRequest.SignedHeaders,
				canonicalRequest.ToString());

			request.Headers.TryAddWithoutValidation("x-amz-date", canonicalRequest.XAmzDate);
			request.Headers.TryAddWithoutValidation("authorization", signature.ForAuthorizationHeader);
			if (!string.IsNullOrEmpty(canonicalRequest.XAmzSecurityToken)) request.Headers.TryAddWithoutValidation("x-amz-security-token", canonicalRequest.XAmzSecurityToken);
		}
#else
		public static void SignRequest(
			HttpWebRequest request,
			RequestData requestData,
			ImmutableCredentials credentials,
			RegionEndpoint region,
			DateTime signingTime)
		{
			var canonicalRequest = CanonicalRequest.From(request, requestData, credentials, signingTime);

			var signature = AWS4Signer.ComputeSignature(credentials, region.SystemName, signingTime, "es", canonicalRequest.SignedHeaders,
				canonicalRequest.ToString());

			request.Headers["x-amz-date"] = canonicalRequest.XAmzDate;
			request.Headers["authorization"] = signature.ForAuthorizationHeader;
			if (!string.IsNullOrEmpty(canonicalRequest.XAmzSecurityToken))
				request.Headers["x-amz-security-token"] = canonicalRequest.XAmzSecurityToken;
		}
#endif
	}
}
