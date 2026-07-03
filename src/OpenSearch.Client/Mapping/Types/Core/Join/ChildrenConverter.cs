/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenSearch.Client
{
	/// <summary>
	/// A <see cref="System.Text.Json"/> converter for <see cref="Children"/> (join relations), replacing
	/// the vendored Utf8Json <c>ChildrenFormatter</c> as part of #388. A single relation is written as a
	/// string, multiple as a string array; relation-name inference is delegated to the registered
	/// <see cref="RelationName"/> converter.
	/// </summary>
	internal sealed class ChildrenConverter : JsonConverter<Children>
	{
		public override void Write(Utf8JsonWriter writer, Children value, JsonSerializerOptions options)
		{
			if (value == null || value.Count == 0)
			{
				writer.WriteNullValue();
				return;
			}

			var relations = value.ToList();
			if (relations.Count == 1)
			{
				JsonSerializer.Serialize(writer, relations[0], options);
				return;
			}

			writer.WriteStartArray();
			foreach (var relation in relations)
				JsonSerializer.Serialize(writer, relation, options);
			writer.WriteEndArray();
		}

		public override Children Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			var children = new Children();
			switch (reader.TokenType)
			{
				case JsonTokenType.String:
					children.Add(reader.GetString());
					return children;
				case JsonTokenType.StartArray:
					var relations = JsonSerializer.Deserialize<List<RelationName>>(ref reader, options);
					if (relations != null) children.AddRange(relations);
					return children;
				default:
					reader.Skip();
					return children;
			}
		}
	}
}
