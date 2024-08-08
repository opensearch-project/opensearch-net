/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Net.Http;
using FluentAssertions;
using OpenSearch.Net;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Core.Connection.Http;
using Xunit;
using HttpMethod = OpenSearch.Net.HttpMethod;

namespace Tests.Connection.Http;

public class HttpConnectionTests
{
    public static TheoryData<HttpMethod> HttpMethods()
    {
        var data = new TheoryData<HttpMethod>();
        foreach (var httpMethod in Enum.GetValues<HttpMethod>()) data.Add(httpMethod);
        return data;
    }

    [TU]
    [MemberData(nameof(HttpMethods))]
    public void UsesCorrectHttpMethod(HttpMethod method)
    {
        HttpRequestMessage sentRequest = null;
        var connection = new MockableHttpConnection(r =>
        {
            sentRequest = r;
            return new HttpResponseMessage();
        });
        var client = new OpenSearchLowLevelClient(new ConnectionConfiguration(connection: connection));

        client.DoRequest<VoidResponse>(method, "/");

        sentRequest.ShouldHaveMethod(method.ToString());
    }
}
