/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

#nullable enable

using System;
#nullable enable

using System.Text.Json;
#nullable enable

using System.Text.Json.Serialization;

namespace OpenSearch.Client.SystemTextJsonConverters;

internal sealed class RoutingConverter : JsonConverter<Routing>
{
	private readonly IConnectionSettingsValues _settings;

	public RoutingConverter(IConnectionSettingsValues settings) => _settings = settings;

	public override Routing? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType == JsonTokenType.Null)
			return null;

		var routing = reader.GetString();
		return routing == null ? null : (Routing)routing;
	}

	public override void Write(Utf8JsonWriter writer, Routing value, JsonSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNullValue();
			return;
		}

		writer.WriteStringValue(value.ToString(_settings));
	}
}
