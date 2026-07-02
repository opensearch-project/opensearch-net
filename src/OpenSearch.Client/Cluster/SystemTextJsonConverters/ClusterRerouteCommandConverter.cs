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

namespace OpenSearch.Client.Cluster.SystemTextJsonConverters
{
	/// <summary>
	/// Polymorphic converter for <see cref="IClusterRerouteCommand"/>.
	/// Uses the type-as-key pattern where the JSON property name is the discriminator:
	/// <code>
	/// {"allocate_replica": {"index": "my-index", "shard": 0, "node": "node-1"}}
	/// {"move": {"index": "my-index", "shard": 0, "from_node": "node-1", "to_node": "node-2"}}
	/// {"cancel": {"index": "my-index", "shard": 0, "node": "node-1"}}
	/// </code>
	/// </summary>
	internal sealed class ClusterRerouteCommandConverter : JsonConverter<IClusterRerouteCommand>
	{
		public override IClusterRerouteCommand Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null)
				return null;

			if (reader.TokenType != JsonTokenType.StartObject)
				throw new JsonException($"Expected StartObject for IClusterRerouteCommand but got {reader.TokenType}");

			using var doc = JsonDocument.ParseValue(ref reader);
			var root = doc.RootElement;

			foreach (var property in root.EnumerateObject())
			{
				var commandName = property.Name;
				var rawJson = property.Value.GetRawText();

				switch (commandName)
				{
					case "allocate_replica":
						return JsonSerializer.Deserialize<AllocateReplicaClusterRerouteCommand>(rawJson, options);
					case "allocate_empty_primary":
						return JsonSerializer.Deserialize<AllocateEmptyPrimaryRerouteCommand>(rawJson, options);
					case "allocate_stale_primary":
						return JsonSerializer.Deserialize<AllocateStalePrimaryRerouteCommand>(rawJson, options);
					case "move":
						return JsonSerializer.Deserialize<MoveClusterRerouteCommand>(rawJson, options);
					case "cancel":
						return JsonSerializer.Deserialize<CancelClusterRerouteCommand>(rawJson, options);
					default:
						return null;
				}
			}

			return null;
		}

		public override void Write(Utf8JsonWriter writer, IClusterRerouteCommand value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			writer.WriteStartObject();
			writer.WritePropertyName(value.Name);

			switch (value)
			{
				case AllocateReplicaClusterRerouteCommand v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				case AllocateEmptyPrimaryRerouteCommand v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				case AllocateStalePrimaryRerouteCommand v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				case MoveClusterRerouteCommand v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				case CancelClusterRerouteCommand v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				default:
					JsonSerializer.Serialize(writer, value, value.GetType(), options);
					break;
			}

			writer.WriteEndObject();
		}
	}
}
