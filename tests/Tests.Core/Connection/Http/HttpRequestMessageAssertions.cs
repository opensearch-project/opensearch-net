/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.Net.Http;
using FluentAssertions;

namespace Tests.Core.Connection.Http;

public static class HttpRequestMessageAssertions
{
    public static void ShouldHaveMethod(this HttpRequestMessage request, string method) =>
        request.Method.Should().Be(new HttpMethod(method));

    public static void ShouldHaveHeader(this HttpRequestMessage request, string name, string value) =>
        request.Headers.GetValues(name).Should().BeEquivalentTo(value);
}
