/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

namespace OpenSearch.Net.Auth.AwsSigV4
{
	using System;
	using Amazon;
	using Amazon.Runtime;

	public class AwsSigV4HttpConnection : HttpConnection
	{
		private readonly AWSCredentials _credentials;
		private readonly RegionEndpoint _region;

		public AwsSigV4HttpConnection(AWSCredentials credentials = null, RegionEndpoint region = null)
		{
			_credentials = credentials
				?? FallbackCredentialsFactory.GetCredentials()
				?? throw new ArgumentNullException(nameof(credentials), "The AWSCredentials were not provided and were unable to be determined from the environment.");
			_region = region
				?? FallbackRegionFactory.GetRegionEndpoint()
				?? throw new ArgumentNullException(nameof(region), "A RegionEndpoint was not provided and was unable to be determined from the environment.");
		}

#if DOTNETCORE

		protected override System.Net.Http.HttpMessageHandler CreateHttpClientHandler(RequestData requestData) =>
			new AwsSigV4HttpClientHandler(_credentials, _region, base.CreateHttpClientHandler(requestData));

#else

		protected override System.Net.HttpWebRequest CreateHttpWebRequest(RequestData requestData)
		{
			var request = base.CreateHttpWebRequest(requestData);
			AwsSigV4Util.SignRequest(request, requestData, _credentials.GetCredentials(), _region, DateTime.UtcNow);
			return request;
		}

#endif
	}
}
