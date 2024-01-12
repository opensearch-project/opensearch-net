/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.Net.Http;
using OpenSearch.Net;
using Tests.Core.Connection.Http;

namespace Tests.Connection.Http;

public class MockableHttpConnection : HttpConnection
{
	private readonly MockHttpMessageHandler _handler;

	public MockableHttpConnection(MockHttpMessageHandler.Handler handler) =>
		_handler = new MockHttpMessageHandler(handler);

	protected override HttpMessageHandler CreateHttpClientHandler(RequestData requestData) => _handler;
}
