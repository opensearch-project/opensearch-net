/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/
/*
* Modifications Copyright OpenSearch Contributors. See
* GitHub history for details.
*/

namespace OpenSearch.Net.Diagnostics
{
	/// <summary>
	/// Attribute (tag) names for spans emitted by <see cref="OpenSearchClientActivitySource"/>.
	/// These follow the OpenTelemetry semantic conventions so that any OpenTelemetry-aware backend
	/// can interpret the spans without additional configuration.
	/// </summary>
	internal static class OpenTelemetryAttributes
	{
		// The OpenTelemetry semantic conventions schema version the attributes below conform to.
		public const string SchemaVersion = "https://opentelemetry.io/schemas/1.21.0";

		// Database
		public const string DbSystem = "db.system";
		public const string DbOperation = "db.operation";

		// HTTP
		public const string HttpRequestMethod = "http.request.method";
		public const string HttpResponseStatusCode = "http.response.status_code";

		// Server
		public const string ServerAddress = "server.address";
		public const string ServerPort = "server.port";

		// URL
		public const string UrlFull = "url.full";

		/// <summary>The value of <see cref="DbSystem"/> for this client.</summary>
		public const string DbSystemValue = "opensearch";
	}
}
