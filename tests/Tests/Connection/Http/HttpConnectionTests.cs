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
		var mockHttpConnection = new MockHttpConnection();
		mockHttpConnection.Request<StringResponse>(new RequestData(method, "", null, null, null, null));
		mockHttpConnection.LastRequest.Method.Should().Be(new System.Net.Http.HttpMethod(method.ToString()));
	}

	private class MockHttpConnection : HttpConnection
	{
		public HttpRequestMessage LastRequest { get; private set; }

		protected override HttpRequestMessage CreateHttpRequestMessage(RequestData requestData)
		{
			LastRequest = base.CreateHttpRequestMessage(requestData);
			return LastRequest;
		}

		public override TResponse Request<TResponse>(RequestData requestData)
		{
			CreateHttpRequestMessage(requestData);
			return new TResponse();
		}
	}
}
