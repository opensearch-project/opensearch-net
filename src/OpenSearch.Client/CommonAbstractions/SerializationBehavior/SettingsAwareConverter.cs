/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.Text.Json.Serialization;

namespace OpenSearch.Client
{
	/// <summary>
	/// Base class for System.Text.Json converters that need access to the runtime
	/// <see cref="IConnectionSettingsValues"/> (e.g. to resolve <c>IndexName</c>/<c>Field</c> via the
	/// <c>Inferrer</c>). In the legacy Utf8Json engine formatters obtained settings from the resolver
	/// (<c>formatterResolver.GetConnectionSettings()</c>); System.Text.Json converters have no such hook, so
	/// settings are injected at construction time by <see cref="SystemTextJsonHighLevelSerializer"/>.
	/// </summary>
	internal abstract class SettingsAwareConverter<T> : JsonConverter<T>
	{
		protected IConnectionSettingsValues Settings { get; }

		protected SettingsAwareConverter(IConnectionSettingsValues settings) => Settings = settings;
	}
}
