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
#nullable enable

using OpenSearch.Net;

namespace OpenSearch.Client.Document.SystemTextJsonConverters;

/// <summary>
/// Converts <see cref="BulkResponseItemBase"/> which is polymorphic based on action type.
/// Response items are wrapped in an object with the action name as key:
/// {"index": {"_index": "...", "_id": "...", "status": 200, ...}}
/// </summary>
internal sealed class BulkResponseItemConverter : JsonConverter<BulkResponseItemBase>
{
	private readonly IConnectionSettingsValues _settings;

	public BulkResponseItemConverter(IConnectionSettingsValues settings) => _settings = settings;

	public override BulkResponseItemBase? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType == JsonTokenType.Null)
			return null;

		if (reader.TokenType != JsonTokenType.StartObject)
			throw new JsonException("Expected StartObject token for BulkResponseItemBase.");

		reader.Read(); // Move past StartObject

		if (reader.TokenType != JsonTokenType.PropertyName)
			throw new JsonException("Expected property name (action type) in bulk response item.");

		var operation = reader.GetString();
		reader.Read(); // Move past property name to the value object

		BulkResponseItemBase item = operation switch
		{
			"index" => new BulkIndexResponseItem(),
			"create" => new BulkCreateResponseItem(),
			"delete" => new BulkDeleteResponseItem(),
			"update" => new BulkUpdateResponseItem(),
			_ => throw new JsonException($"Unknown bulk operation type: '{operation}'.")
		};

		// Parse the inner object properties
		if (reader.TokenType != JsonTokenType.StartObject)
			throw new JsonException("Expected StartObject for bulk response item details.");

		while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
		{
			if (reader.TokenType != JsonTokenType.PropertyName)
				continue;

			var propertyName = reader.GetString();
			reader.Read(); // Move to value

			switch (propertyName)
			{
				case "_index":
					item.Index = reader.GetString();
					break;
				case "_type":
					item.Type = reader.GetString();
					break;
				case "_id":
					item.Id = reader.GetString();
					break;
				case "_version":
					item.Version = reader.GetInt64();
					break;
				case "result":
					item.Result = reader.GetString();
					break;
				case "status":
					item.Status = reader.GetInt32();
					break;
				case "_seq_no":
					item.SequenceNumber = reader.GetInt64();
					break;
				case "_primary_term":
					item.PrimaryTerm = reader.GetInt64();
					break;
				case "_shards":
					item.Shards = JsonSerializer.Deserialize<ShardStatistics>(ref reader, options);
					break;
				case "error":
					item.Error = JsonSerializer.Deserialize<Error>(ref reader, options);
					break;
				case "get":
					// Store as raw JSON for later typed deserialization
					using (var doc = JsonDocument.ParseValue(ref reader))
					{
						// LazyDocument handling would require the transport serializer;
						// for now skip this complex case
					}
					break;
				default:
					reader.Skip();
					break;
			}
		}

		// Read past the outer EndObject
		reader.Read();

		return item;
	}

	public override void Write(Utf8JsonWriter writer, BulkResponseItemBase value, JsonSerializerOptions options) =>
		throw new NotSupportedException("Serialization of BulkResponseItemBase is not supported.");
}
