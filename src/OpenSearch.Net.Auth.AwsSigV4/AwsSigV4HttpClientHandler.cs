/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

#if DOTNETCORE

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Amazon;
using Amazon.Runtime;

namespace OpenSearch.Net.Auth.AwsSigV4
{
	internal class AwsSigV4HttpClientHandler : DelegatingHandler
	{
		private readonly AWSCredentials _credentials;
		private readonly RegionEndpoint _region;
		private readonly string _serviceId;

		public AwsSigV4HttpClientHandler(AWSCredentials credentials, RegionEndpoint region, string serviceId, HttpMessageHandler innerHandler)
			: base(innerHandler)
		{
			_credentials = credentials ?? throw new ArgumentNullException(nameof(credentials));
			_region = region ?? throw new ArgumentNullException(nameof(region));
			_serviceId = serviceId ?? throw new ArgumentNullException(nameof(serviceId));
		}

		protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			var credentials = await _credentials.GetCredentialsAsync().ConfigureAwait(false);

			await AwsSigV4Util.SignRequest(request, credentials, _region, DateTime.UtcNow, _serviceId).ConfigureAwait(false);

			return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
		}
	}
}

#endif
