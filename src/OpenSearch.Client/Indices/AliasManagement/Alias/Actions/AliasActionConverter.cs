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
	/// System.Text.Json replacement for the legacy Utf8Json <c>AliasActionFormatter</c>.
	///
	/// An <see cref="IAliasAction"/> is polymorphic: every variant is wrapped in a single-key object whose outer
	/// property name selects the concrete type — <c>{ "add": { ... } }</c> → <see cref="AliasAddAction"/>,
	/// <c>{ "remove": { ... } }</c> → <see cref="AliasRemoveAction"/>, <c>{ "remove_index": { ... } }</c> →
	/// <see cref="AliasRemoveIndexAction"/>. The wrapping key itself is produced by the concrete type's own
	/// <c>[DataMember]</c> property (<c>Add</c>/<c>Remove</c>/<c>RemoveIndex</c>), so on read we deserialize the whole
	/// object as the resolved concrete type and on write we serialize the value as its declared action interface.
	///
	/// System.Text.Json's <see cref="Utf8JsonReader"/> is forward-only and cannot be rewound — unlike the Utf8Json
	/// version which peeked at a byte segment and re-read it — so we buffer the value into a <see cref="JsonDocument"/>,
	/// read the discriminating first property name from the DOM, then deserialize the whole element.
	/// </summary>
	internal class AliasActionConverter : JsonConverter<IAliasAction>
	{
		public override IAliasAction Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null)
				return null;

			using var doc = JsonDocument.ParseValue(ref reader);
			var root = doc.RootElement;

			if (root.ValueKind != JsonValueKind.Object)
				return null;

			foreach (var property in root.EnumerateObject())
			{
				switch (property.Name)
				{
					case "add": return root.Deserialize<AliasAddAction>(options);
					case "remove": return root.Deserialize<AliasRemoveAction>(options);
					case "remove_index": return root.Deserialize<AliasRemoveIndexAction>(options);
					default: return null;
				}
			}

			return null;
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
	}
}
