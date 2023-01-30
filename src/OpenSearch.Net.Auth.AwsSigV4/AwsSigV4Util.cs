/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

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

namespace OpenSearch.Net.Auth.AwsSigV4
{
	public static class AwsSigV4Util
	{
#if DOTNETCORE
		public static async Task SignRequest(
			HttpRequestMessage request,
			ImmutableCredentials credentials,
			RegionEndpoint region,
			DateTime signingTime,
			string service)
		{
			var canonicalRequest = await CanonicalRequest.From(request, credentials, signingTime).ConfigureAwait(false);

			var signature = AWS4Signer.ComputeSignature(credentials, region.SystemName, signingTime, service, canonicalRequest.SignedHeaders,
				canonicalRequest.ToString());

			request.Headers.TryAddWithoutValidation(HeaderNames.XAmzDate, canonicalRequest.XAmzDate);
			request.Headers.TryAddWithoutValidation(HeaderNames.XAmzContentSha256, canonicalRequest.XAmzContentSha256);
			request.Headers.TryAddWithoutValidation(HeaderNames.Authorization, signature.ForAuthorizationHeader);
			if (!string.IsNullOrEmpty(canonicalRequest.XAmzSecurityToken)) request.Headers.TryAddWithoutValidation(HeaderNames.XAmzSecurityToken, canonicalRequest.XAmzSecurityToken);
		}
#else
		public static void SignRequest(
			HttpWebRequest request,
			RequestData requestData,
			ImmutableCredentials credentials,
			RegionEndpoint region,
			DateTime signingTime,
			string service)
		{
			var canonicalRequest = CanonicalRequest.From(request, requestData, credentials, signingTime);

			var signature = AWS4Signer.ComputeSignature(credentials, region.SystemName, signingTime, service, canonicalRequest.SignedHeaders,
				canonicalRequest.ToString());

			request.Headers[HeaderNames.XAmzDate] = canonicalRequest.XAmzDate;
			request.Headers[HeaderNames.XAmzContentSha256] = canonicalRequest.XAmzContentSha256;
			request.Headers[HeaderNames.Authorization] = signature.ForAuthorizationHeader;
			if (!string.IsNullOrEmpty(canonicalRequest.XAmzSecurityToken))
				request.Headers[HeaderNames.XAmzSecurityToken] = canonicalRequest.XAmzSecurityToken;
		}
#endif
	}
}
