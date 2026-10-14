/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.Collections.Generic;
using OpenSearch.Net.Utf8Json;

namespace OpenSearch.Client
{
	/// <summary>
	/// Serializes <see cref="NodeIds"/> as a single JSON string (one id) or an array of strings (several ids), and
	/// reads either form; a string is parsed as a comma-separated list. An empty array reads as <c>null</c>.
	/// </summary>
	internal class NodeIdsFormatter : IJsonFormatter<NodeIds>
	{
		public NodeIds Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
		{
			switch (reader.GetCurrentJsonToken())
			{
				case JsonToken.String:
					return NodeIds.Parse(reader.ReadString());
				case JsonToken.BeginArray:
					var ids = new List<string>();
					var count = 0;
					while (reader.ReadIsInArray(ref count))
					{
						var id = reader.ReadString();
						if (!string.IsNullOrEmpty(id))
							ids.Add(id);
					}
					return ids.Count == 0 ? null : new NodeIds(ids);
				default:
					reader.ReadNextBlock();
					return null;
			}
		}

		public void Serialize(ref JsonWriter writer, NodeIds value, IJsonFormatterResolver formatterResolver)
		{
			if (value == null)
			{
				writer.WriteNull();
				return;
			}

			if (value.Value.Count == 1)
			{
				writer.WriteString(value.Value[0]);
				return;
			}

			writer.WriteBeginArray();
			for (var i = 0; i < value.Value.Count; i++)
			{
				if (i > 0)
					writer.WriteValueSeparator();
				writer.WriteString(value.Value[i]);
			}
			writer.WriteEndArray();
		}
	}
}
