/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenSearch.Client
{
	/// <summary>
	/// System.Text.Json counterpart of <see cref="NodeIdsFormatter"/>. Writes a JSON array of node id strings; reads
	/// either an array or a single string (parsed as a comma-separated list). An empty array reads as <c>null</c>.
	/// </summary>
	internal class NodeIdsConverter : JsonConverter<NodeIds>
	{
		public override NodeIds Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			switch (reader.TokenType)
			{
				case JsonTokenType.String:
					return NodeIds.Parse(reader.GetString());
				case JsonTokenType.StartArray:
					var ids = new List<string>();
					while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
					{
						var id = reader.GetString();
						if (!string.IsNullOrEmpty(id))
							ids.Add(id);
					}
					return ids.Count == 0 ? null : new NodeIds(ids);
				default:
					reader.Skip();
					return null;
			}
		}

		public override void Write(Utf8JsonWriter writer, NodeIds value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			writer.WriteStartArray();
			foreach (var id in value.Value)
				writer.WriteStringValue(id);
			writer.WriteEndArray();
		}
	}
}
