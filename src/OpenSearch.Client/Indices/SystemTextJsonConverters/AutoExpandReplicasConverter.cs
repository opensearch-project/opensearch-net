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

namespace OpenSearch.Client.Indices.SystemTextJsonConverters;

/// <summary>
/// System.Text.Json converter for <see cref="AutoExpandReplicas"/>.
/// Format: "0-5", "0-all", or "false".
/// </summary>
internal sealed class AutoExpandReplicasConverter : JsonConverter<AutoExpandReplicas>
{
	public override AutoExpandReplicas? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType == JsonTokenType.Null)
			return null;

		if (reader.TokenType == JsonTokenType.False)
			return AutoExpandReplicas.Disabled;

		if (reader.TokenType == JsonTokenType.True)
			return null; // "true" is not a valid value; treat as disabled

		if (reader.TokenType == JsonTokenType.String)
		{
			var value = reader.GetString();
			if (string.IsNullOrEmpty(value))
				return null;

			if (string.Equals(value, "false", StringComparison.OrdinalIgnoreCase))
				return AutoExpandReplicas.Disabled;

			return AutoExpandReplicas.Create(value);
		}

		throw new JsonException($"Cannot deserialize AutoExpandReplicas from token {reader.TokenType}");
	}

	public override void Write(Utf8JsonWriter writer, AutoExpandReplicas value, JsonSerializerOptions options)
	{
		if (value == null || !value.Enabled)
		{
			writer.WriteBooleanValue(false);
			return;
		}

		writer.WriteStringValue(value.ToString());
	}
}
