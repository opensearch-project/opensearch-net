/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.Net.Http;
using Amazon;
using Amazon.Runtime;
using OpenSearch.Net;
using OpenSearch.Net.Auth.AwsSigV4;
using Tests.Core.Connection.Http;

namespace Tests.Auth.AwsSigV4.Utils;

internal class TestableAwsSigV4HttpConnection : AwsSigV4HttpConnection
{
	private readonly MockHttpMessageHandler _handler;

	public TestableAwsSigV4HttpConnection(AWSCredentials credentials, RegionEndpoint region, string service, IDateTimeProvider dateTimeProvider, MockHttpMessageHandler.Handler handler)
		: base(credentials, region, service, dateTimeProvider) =>
		_handler = new MockHttpMessageHandler(handler);

	protected override HttpMessageHandler InnerCreateHttpClientHandler(RequestData requestData) => _handler;
}
