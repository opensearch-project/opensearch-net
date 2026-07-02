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
/// Polymorphic converter for <see cref="IAliasAction"/>.
/// The discriminator is the action name key: "add", "remove", or "remove_index".
/// </summary>
internal sealed class AliasConverter : JsonConverter<IAliasAction>
{
	public override IAliasAction? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType == JsonTokenType.Null)
			return null;

		if (reader.TokenType != JsonTokenType.StartObject)
			throw new JsonException($"Expected StartObject for IAliasAction but got {reader.TokenType}");

		using var doc = JsonDocument.ParseValue(ref reader);
		var root = doc.RootElement;

		if (root.TryGetProperty("add", out var addElement))
		{
			var rawJson = root.GetRawText();
			return JsonSerializer.Deserialize<AliasAddAction>(rawJson, options);
		}

		if (root.TryGetProperty("remove", out var removeElement))
		{
			var rawJson = root.GetRawText();
			return JsonSerializer.Deserialize<AliasRemoveAction>(rawJson, options);
		}

		if (root.TryGetProperty("remove_index", out var removeIndexElement))
		{
			var rawJson = root.GetRawText();
			return JsonSerializer.Deserialize<AliasRemoveIndexAction>(rawJson, options);
		}

		throw new JsonException("Unknown alias action. Expected 'add', 'remove', or 'remove_index' property.");
	}

	public override void Write(Utf8JsonWriter writer, IAliasAction value, JsonSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNullValue();
			return;
		}

		switch (value)
		{
			case IAliasAddAction addAction:
				JsonSerializer.Serialize(writer, addAction, addAction.GetType(), options);
				break;
			case IAliasRemoveAction removeAction:
				JsonSerializer.Serialize(writer, removeAction, removeAction.GetType(), options);
				break;
			case IAliasRemoveIndexAction removeIndexAction:
				JsonSerializer.Serialize(writer, removeIndexAction, removeIndexAction.GetType(), options);
				break;
			default:
				JsonSerializer.Serialize(writer, value, value.GetType(), options);
				break;
		}
	}
}
