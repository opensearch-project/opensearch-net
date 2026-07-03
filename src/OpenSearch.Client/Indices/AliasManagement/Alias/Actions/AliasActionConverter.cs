/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenSearch.Client
{
	/// <summary>
	/// A <see cref="System.Text.Json"/> converter for <see cref="IAliasAction"/>, replacing the vendored
	/// Utf8Json <c>AliasActionFormatter</c> as part of #388. An alias action is a single-key wrapper object
	/// whose key selects the concrete action: <c>add</c> → <see cref="AliasAddAction"/>,
	/// <c>remove</c> → <see cref="AliasRemoveAction"/>, <c>remove_index</c> → <see cref="AliasRemoveIndexAction"/>.
	/// Modelled on <see cref="SimilarityConverter"/>'s discriminator dispatch.
	/// </summary>
	internal sealed class AliasActionConverter : JsonConverter<IAliasAction>
	{
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
					JsonSerializer.Serialize(writer, addAction, options);
					break;
				case IAliasRemoveAction removeAction:
					JsonSerializer.Serialize(writer, removeAction, options);
					break;
				case IAliasRemoveIndexAction removeIndexAction:
					JsonSerializer.Serialize(writer, removeIndexAction, options);
					break;
				default:
					writer.WriteNullValue();
					break;
			}
		}

		public override IAliasAction Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null) return null;

			using var document = JsonDocument.ParseValue(ref reader);
			var root = document.RootElement;
			if (root.ValueKind != JsonValueKind.Object) return null;

			foreach (var member in root.EnumerateObject())
			{
				switch (member.Name)
				{
					case "add":
						return root.Deserialize<AliasAddAction>(options);
					case "remove":
						return root.Deserialize<AliasRemoveAction>(options);
					case "remove_index":
						return root.Deserialize<AliasRemoveIndexAction>(options);
				}
			}

			return null;
		}
	}
}
