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

namespace OpenSearch.Client.CommonOptions.SystemTextJsonConverters;

internal sealed class DistanceConverter : JsonConverter<Distance>
{
	private readonly IConnectionSettingsValues _settings;

	public DistanceConverter(IConnectionSettingsValues settings) => _settings = settings;

	public override Distance? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType == JsonTokenType.Null)
			return null;

		if (reader.TokenType != JsonTokenType.String)
			return null;

		var value = reader.GetString();
		return value == null ? null : new Distance(value);
	}

	public override void Write(Utf8JsonWriter writer, Distance value, JsonSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNullValue();
			return;
		}

		writer.WriteStringValue(value.ToString());
	}
}
