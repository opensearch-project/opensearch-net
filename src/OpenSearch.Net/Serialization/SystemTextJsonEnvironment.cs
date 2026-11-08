/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;

namespace OpenSearch.Net
{
	/// <summary>
	/// Shared environment-variable resolution for the System.Text.Json / Utf8Json engine switch (GitHub issue
	/// #388), used by both <see cref="ConnectionConfiguration{T}"/> (the low-level engine) and
	/// <c>OpenSearch.Client.ConnectionSettingsBase</c> (the high-level engine). Each layer's own programmatic
	/// <c>UseSystemTextJson()</c> call takes precedence over this; the two layers select their engines
	/// independently even though they read the same variable.
	/// </summary>
	public static class SystemTextJsonEnvironment
	{
		/// <summary>
		/// Reads the <c>OSC_USE_STJ</c> environment variable, returning <c>true</c> (System.Text.Json),
		/// <c>false</c> (Utf8Json), or <c>null</c> when it is not set so the caller falls back to its own default.
		/// </summary>
		public static bool? ReadOverride()
		{
			var stjEnv = Environment.GetEnvironmentVariable("OSC_USE_STJ");

			if (string.Equals(stjEnv, "true", StringComparison.OrdinalIgnoreCase))
				return true;

			if (string.Equals(stjEnv, "false", StringComparison.OrdinalIgnoreCase))
				return false;

			return null;
		}
	}
}
