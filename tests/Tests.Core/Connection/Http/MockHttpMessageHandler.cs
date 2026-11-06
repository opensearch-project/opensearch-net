/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Tests.Core.Connection.Http;

public class MockHttpMessageHandler : HttpMessageHandler
{
    public delegate HttpResponseMessage Handler(HttpRequestMessage request);

    private readonly Handler _handler;

    public MockHttpMessageHandler(Handler handler) => _handler = handler;

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
        Task.FromResult(_handler(request));
}
