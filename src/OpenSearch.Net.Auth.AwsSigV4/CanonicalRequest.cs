/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Amazon.Runtime;
using Amazon.Runtime.Internal.Auth;
using Amazon.Util;

#if DOTNETCORE
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
#else
using System.Collections.Specialized;
using System.IO;
using System.IO.Compression;
using System.Net;
#endif

namespace OpenSearch.Net.Auth.AwsSigV4
{
	public class CanonicalRequest
	{
		private static readonly IComparer<KeyValuePair<string, string>> StringPairComparer = new KeyValuePairComparer<string, string>();
		private static readonly ISet<string> ExcludedHeaders = new HashSet<string> { HeaderNames.UserAgent, HeaderNames.ContentLength };

		private readonly string _method;
		private readonly string _path;
		private readonly string _params;
		private readonly SortedDictionary<string, List<string>> _headers;

		public string XAmzContentSha256 { get; }

		public string XAmzDate { get; }

		public string XAmzSecurityToken { get; }

		public string SignedHeaders { get; }

		private CanonicalRequest(string method, string path, string queryParams, SortedDictionary<string, List<string>> headers,
			string xAmzContentSha256, string xAmzDate, string xAmzSecurityToken
		)
		{
			_method = method;
			_path = path;
			_params = queryParams;
			_headers = headers;
			XAmzContentSha256 = xAmzContentSha256;
			SignedHeaders = string.Join(";", _headers.Keys);
			XAmzDate = xAmzDate;
			XAmzSecurityToken = xAmzSecurityToken;
		}

#if DOTNETCORE
		public static async Task<CanonicalRequest> From(HttpRequestMessage request, ImmutableCredentials credentials, DateTime signingTime)
#else
		public static CanonicalRequest From(HttpWebRequest request, RequestData requestData, ImmutableCredentials credentials, DateTime signingTime)
#endif
		{
			var path = AWSSDKUtils.CanonicalizeResourcePath(request.RequestUri, null, false);

#if DOTNETCORE
			var bodyBytes = await GetBodyBytes(request).ConfigureAwait(false);
#else
			var bodyBytes = GetBodyBytes(requestData);
#endif

			var xAmzContentSha256 = AWSSDKUtils.ToHex(AWS4Signer.ComputeHash(bodyBytes), true);

			var xAmzDate = AWS4Signer.FormatDateTime(signingTime, "yyyyMMddTHHmmssZ");

			var canonicalHeaders = new SortedDictionary<string, List<string>>();

			CanonicalizeHeaders(canonicalHeaders, request.Headers);
#if DOTNETCORE
			CanonicalizeHeaders(canonicalHeaders, request.Content?.Headers);
#endif

			canonicalHeaders[HeaderNames.Host] = new List<string> { request.RequestUri.Authority };
			canonicalHeaders[HeaderNames.XAmzDate] = new List<string> { xAmzDate };
			canonicalHeaders[HeaderNames.XAmzContentSha256] = new List<string> { xAmzContentSha256 };

			string xAmzSecurityToken = null;
			if (credentials.UseToken)
			{
				xAmzSecurityToken = credentials.Token;
				canonicalHeaders[HeaderNames.XAmzSecurityToken] = new List<string> { xAmzSecurityToken };
			}

			var queryParams = HttpUtility.ParseQueryString(request.RequestUri.Query);

			var orderedParams = queryParams
				.AllKeys
				.SelectMany(k => queryParams.GetValues(k)
						?.Select(v => !string.IsNullOrEmpty(k)
							? new KeyValuePair<string, string>(k, v)
							: new KeyValuePair<string, string>(v, string.Empty))
					?? Enumerable.Empty<KeyValuePair<string, string>>())
				.OrderBy(pair => pair, StringPairComparer)
				.Select(pair => $"{AWSSDKUtils.UrlEncode(pair.Key, false)}={AWSSDKUtils.UrlEncode(pair.Value, false)}");

			var paramString = string.Join("&", orderedParams);

#if DOTNETCORE
			var method = request.Method.ToString();
#else
			var method = request.Method;
#endif

			return new CanonicalRequest(method, path, paramString, canonicalHeaders, xAmzContentSha256, xAmzDate, xAmzSecurityToken);
		}

#if DOTNETCORE
		private static async Task<byte[]> GetBodyBytes(HttpRequestMessage request)
		{
			if (request.Content == null) return Array.Empty<byte>();

			var body = await request.Content.ReadAsByteArrayAsync().ConfigureAwait(false);

			if (request.Content is ByteArrayContent) return body;

			var content = new ByteArrayContent(body);
			foreach (var pair in request.Content.Headers)
			{
				if (string.Equals(pair.Key, HeaderNames.ContentLength, StringComparison.OrdinalIgnoreCase)) continue;

				content.Headers.TryAddWithoutValidation(pair.Key, pair.Value);
			}
			request.Content = content;

			return body;
		}

#else
		private static byte[] GetBodyBytes(RequestData requestData)
		{
			if (requestData.PostData == null) return Array.Empty<byte>();

			using var ms = new MemoryStream();
			if (requestData.HttpCompression)
				using (var zipStream = new GZipStream(ms, CompressionMode.Compress))
					requestData.PostData.Write(zipStream, requestData.ConnectionSettings);
			else
				requestData.PostData.Write(ms, requestData.ConnectionSettings);

			return ms.ToArray();
		}
#endif

		private static void CanonicalizeHeaders(
			IDictionary<string, List<string>> canonicalHeaders,
#if DOTNETCORE
			HttpHeaders headers
#else
			NameValueCollection headers
#endif
		)
		{
			if (headers == null) return;

#if DOTNETCORE
			foreach (var pair in headers)
#else
			foreach (var pair in headers.AllKeys.Select(k => new KeyValuePair<string, IEnumerable<string>>(k, headers.GetValues(k))))
#endif
			{
				if (pair.Value == null) continue;

				var key = pair.Key.ToLowerInvariant();

				if (ExcludedHeaders.Contains(key)) continue;

				if (!canonicalHeaders.TryGetValue(key, out var dictValues))
					dictValues = canonicalHeaders[key] = new List<string>();

				dictValues.AddRange(pair.Value.Select(v => AWSSDKUtils.CompressSpaces(v).Trim()));
			}
		}

		public override string ToString()
		{
			var sb = new StringBuilder();

			sb.Append($"{_method}\n");
			sb.Append($"{_path}\n");
			sb.Append($"{_params}\n");
			foreach (var header in _headers) sb.Append($"{header.Key}:{string.Join(",", header.Value)}\n");
			sb.Append('\n');
			sb.Append($"{SignedHeaders}\n");
			sb.Append(XAmzContentSha256);

			return sb.ToString();
		}

		private class KeyValuePairComparer<TKey, TValue> : IComparer<KeyValuePair<TKey, TValue>>
			where TKey : IComparable<TKey>
			where TValue : IComparable<TValue>
		{
			public int Compare(KeyValuePair<TKey, TValue> x, KeyValuePair<TKey, TValue> y)
			{
				var keyComparison = x.Key.CompareTo(y.Key);
				return keyComparison != 0 ? keyComparison : x.Value.CompareTo(y.Value);
			}
		}
	}
}
