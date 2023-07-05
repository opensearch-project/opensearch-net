/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

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
		private readonly string _service;
		private readonly IDateTimeProvider _dateTimeProvider;

		public AwsSigV4HttpClientHandler(AWSCredentials credentials, RegionEndpoint region, string service, IDateTimeProvider dateTimeProvider, HttpMessageHandler innerHandler)
			: base(innerHandler)
		{
			_credentials = credentials ?? throw new ArgumentNullException(nameof(credentials));
			_region = region ?? throw new ArgumentNullException(nameof(region));
			_service = service ?? throw new ArgumentNullException(nameof(service));
			_dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
		}

		protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			var credentials = await _credentials.GetCredentialsAsync().ConfigureAwait(false);

			await AwsSigV4Util.SignRequest(request, credentials, _region, _dateTimeProvider.Now(), _service).ConfigureAwait(false);

			return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
		}
	}
}
