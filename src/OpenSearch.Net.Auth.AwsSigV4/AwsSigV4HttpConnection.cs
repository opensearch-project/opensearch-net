/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using Amazon;
using Amazon.Runtime;

namespace OpenSearch.Net.Auth.AwsSigV4
{
    /// <summary>
    /// An <see cref="IConnection"/> implementation that performs AWS SigV4 request signing, for performing authentication with Amazon Managed OpenSearch.
    /// </summary>
    public class AwsSigV4HttpConnection : HttpConnection
    {
        public const string OpenSearchService = "es";
        public const string OpenSearchServerlessService = "aoss";

        private readonly AWSCredentials _credentials;
        private readonly RegionEndpoint _region;
        private readonly string _service;
        private readonly IDateTimeProvider _dateTimeProvider;

        /// <summary>
        /// Construct a new connection discovering both the credentials and region from the environment.
        /// </summary>
        /// <param name="service">The service code to use when signing, defaults to the service code for the Amazon OpenSearch Service (<c>"es"</c>).</param>
        /// <param name="dateTimeProvider">The date time proved to use, safe to pass null to use the default</param>
        /// <seealso cref="AwsSigV4HttpConnection(AWSCredentials, RegionEndpoint, string, IDateTimeProvider)"/>
        public AwsSigV4HttpConnection(string service = OpenSearchService, IDateTimeProvider dateTimeProvider = null) : this(null, null, service, dateTimeProvider) { }

        /// <summary>
        /// Construct a new connection configured with the specified credentials and using the region discovered from the environment.
        /// </summary>
        /// <param name="credentials">The credentials to use when signing.</param>
        /// <param name="service">The service code to use when signing, defaults to the service code for the Amazon OpenSearch Service (<c>"es"</c>).</param>
        /// <param name="dateTimeProvider">The date time proved to use, safe to pass null to use the default</param>
        /// <seealso cref="AwsSigV4HttpConnection(AWSCredentials, RegionEndpoint, string, IDateTimeProvider)"/>
        public AwsSigV4HttpConnection(AWSCredentials credentials, string service = OpenSearchService, IDateTimeProvider dateTimeProvider = null) : this(credentials, null, service, dateTimeProvider) { }

        /// <summary>
        /// Construct a new connection configured with a specified region and using credentials discovered from the environment.
        /// </summary>
        /// <param name="region">The region to use when signing.</param>
        /// <param name="service">The service code to use when signing, defaults to the service code for the Amazon OpenSearch Service (<c>"es"</c>).</param>
        /// <param name="dateTimeProvider">The date time proved to use, safe to pass null to use the default</param>
        /// <seealso cref="AwsSigV4HttpConnection(AWSCredentials, RegionEndpoint, string, IDateTimeProvider)"/>
        public AwsSigV4HttpConnection(RegionEndpoint region, string service = OpenSearchService, IDateTimeProvider dateTimeProvider = null) : this(null, region, service, dateTimeProvider) { }

        /// <summary>
        /// Construct a new connection configured with the given credentials and region.
        /// </summary>
        /// <param name="credentials">The credentials to use when signing, or null to have them discovered automatically by the AWS SDK.</param>
        /// <param name="region">The region to use when signing, or null to have it discovered automatically by the AWS SDK.</param>
        ///	<param name="service">The service code to use when signing, defaults to the service code for the Amazon OpenSearch Service (<c>"es"</c>).</param>
        /// <param name="dateTimeProvider">The date time proved to use, safe to pass null to use the default</param>
        /// <exception cref="ArgumentNullException">Thrown if region is null and is unable to be automatically discovered by the AWS SDK.</exception>
        /// <remarks>
        /// The same logic as the <a href="https://docs.aws.amazon.com/sdk-for-net/v3/developer-guide/creds-assign.html">AWS SDK for .NET</a>
        /// is used to automatically discover the credentials and region to use if not provided explicitly.
        /// </remarks>
        public AwsSigV4HttpConnection(AWSCredentials credentials, RegionEndpoint region, string service = OpenSearchService, IDateTimeProvider dateTimeProvider = null)
        {
            _credentials = credentials ?? FallbackCredentialsFactory.GetCredentials(); // FallbackCredentialsFactory throws in case of not finding credentials.
            _region = region
                ?? FallbackRegionFactory.GetRegionEndpoint() // FallbackRegionFactory can return null.
                ?? throw new ArgumentNullException(nameof(region), "A RegionEndpoint was not provided and was unable to be determined from the environment.");
            _service = service ?? OpenSearchService;
            _dateTimeProvider = dateTimeProvider ?? DateTimeProvider.Default;
        }

        protected virtual System.Net.Http.HttpMessageHandler InnerCreateHttpClientHandler(RequestData requestData) =>
            base.CreateHttpClientHandler(requestData);

        protected override System.Net.Http.HttpMessageHandler CreateHttpClientHandler(RequestData requestData) =>
            new AwsSigV4HttpClientHandler(_credentials, _region, _service, _dateTimeProvider, InnerCreateHttpClientHandler(requestData));
    }
}
