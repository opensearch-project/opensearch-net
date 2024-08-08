/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Amazon.Runtime;
using Amazon.Runtime.Internal.Auth;
using Amazon.Util;

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

        public static async Task<CanonicalRequest> From(HttpRequestMessage request, ImmutableCredentials credentials, DateTime signingTime)
        {
            var path = AWSSDKUtils.CanonicalizeResourcePathV2(request.RequestUri, null, false, null);

            var bodyBytes = await GetBodyBytes(request).ConfigureAwait(false);

            var xAmzContentSha256 = AWSSDKUtils.ToHex(AWS4Signer.ComputeHash(bodyBytes), true);

            var xAmzDate = AWS4Signer.FormatDateTime(signingTime, "yyyyMMddTHHmmssZ");

            var canonicalHeaders = new SortedDictionary<string, List<string>>();

            CanonicalizeHeaders(canonicalHeaders, request.Headers);
            CanonicalizeHeaders(canonicalHeaders, request.Content?.Headers);

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

            var method = request.Method.ToString();

            return new CanonicalRequest(method, path, paramString, canonicalHeaders, xAmzContentSha256, xAmzDate, xAmzSecurityToken);
        }

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

        private static void CanonicalizeHeaders(
            IDictionary<string, List<string>> canonicalHeaders,
            HttpHeaders headers
        )
        {
            if (headers == null) return;

            foreach (var pair in headers)
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
