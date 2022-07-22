/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

namespace OpenSearch.Net.Auth.AwsSigV4
{
	using System;
	using System.Net;
	using Amazon;
	using Amazon.Runtime;

	public class AwsSigV4HttpConnection : HttpConnection
	{
		private readonly AWSCredentials _credentials;
		private readonly RegionEndpoint _region;

		public AwsSigV4HttpConnection(AWSCredentials credentials, RegionEndpoint region)
		{
			_credentials = credentials ?? throw new ArgumentNullException(nameof(credentials));
			_region = region ?? throw new ArgumentNullException(nameof(region));
		}

#if DOTNETCORE

		protected override System.Net.Http.HttpMessageHandler CreateHttpClientHandler(RequestData requestData) =>
			new AwsSigV4HttpClientHandler(_credentials, _region, base.CreateHttpClientHandler(requestData));

#else

		protected override HttpWebRequest CreateHttpWebRequest(RequestData requestData)
		{
			var request = base.CreateHttpWebRequest(requestData);
			AwsSigV4Util.SignRequest(request, requestData, _credentials.GetCredentials(), _region, DateTime.UtcNow);
			return request;
		}

#endif
	}
}
