/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

namespace OpenSearch.Net.Auth.AwsSigV4;

internal static class HeaderNames
{
    public const string Authorization = "authorization";
    public const string ContentLength = "content-length";
    public const string Host = "host";
    public const string UserAgent = "user-agent";
    public const string XAmzContentSha256 = "x-amz-content-sha256";
    public const string XAmzDate = "x-amz-date";
    public const string XAmzSecurityToken = "x-amz-security-token";
}
