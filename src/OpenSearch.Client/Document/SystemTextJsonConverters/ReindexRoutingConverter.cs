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

namespace OpenSearch.Client.Document.SystemTextJsonConverters;

/// <summary>
/// Converts <see cref="ReindexRouting"/> which can be:
/// - "keep" (preserve original routing)
/// - "discard" (remove routing)
/// - "=value" (set explicit routing value)
/// Simple string serialization/deserialization.
/// </summary>
internal sealed class ReindexRoutingConverter : JsonConverter<ReindexRouting>
{
	private readonly IConnectionSettingsValues _settings;

	public ReindexRoutingConverter(IConnectionSettingsValues settings) => _settings = settings;

	public override ReindexRouting? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType == JsonTokenType.Null)
			return null;

		if (reader.TokenType != JsonTokenType.String)
			throw new JsonException($"Unexpected token type '{reader.TokenType}' when reading ReindexRouting.");

		var value = reader.GetString();
		return value switch
		{
			null => null,
			"keep" => ReindexRouting.Keep,
			"discard" => ReindexRouting.Discard,
			_ => new ReindexRouting(value)
		};
	}

	public override void Write(Utf8JsonWriter writer, ReindexRouting value, JsonSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNullValue();
			return;
		}

		writer.WriteStringValue(value.ToString());
	}
}
