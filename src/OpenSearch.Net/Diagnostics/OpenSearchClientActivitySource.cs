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

using System.Diagnostics;

namespace OpenSearch.Net.Diagnostics
{
	/// <summary>
	/// Holds the <see cref="ActivitySource"/> used to emit OpenTelemetry spans for requests to OpenSearch,
	/// along with helpers to keep instrumentation cheap when nobody is listening.
	/// </summary>
	/// <remarks>
	/// This is separate from the legacy <see cref="DiagnosticSources"/> / <see cref="DiagnosticListener"/>
	/// mechanism, which is retained for backwards compatibility. OpenTelemetry SDKs only observe
	/// <see cref="ActivitySource"/>, so a consumer only needs to add this source name to start collecting traces:
	/// <code>AddSource(OpenSearchClientActivitySource.ActivitySourceName)</code>
	/// </remarks>
	public static class OpenSearchClientActivitySource
	{
		/// <summary>
		/// The name of the <see cref="ActivitySource"/> emitting spans for requests to OpenSearch.
		/// Add this name to an OpenTelemetry tracer provider to collect spans.
		/// </summary>
		public const string ActivitySourceName = "OpenSearch.Net.RequestPipeline";

		internal static readonly ActivitySource ActivitySource = new(ActivitySourceName, "1.0.0");

		/// <summary>
		/// Whether the <see cref="ActivitySource"/> currently has any listeners. When <c>false</c>, callers
		/// should avoid the cost of starting an <see cref="Activity"/> and computing its tags.
		/// </summary>
		public static bool HasListeners => ActivitySource.HasListeners();
	}
}
