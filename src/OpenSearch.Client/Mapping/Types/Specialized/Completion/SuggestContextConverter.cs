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
	/// System.Text.Json replacement for the legacy Utf8Json <c>SuggestContextFormatter</c>.
	///
	/// <see cref="ISuggestContext"/> is polymorphic: the concrete variant is chosen from the value of the
	/// <c>type</c> discriminator property (<c>geo</c> → <see cref="GeoSuggestContext"/>, <c>category</c> →
	/// <see cref="CategorySuggestContext"/>). Anything else (including a missing <c>type</c>) falls back to
	/// <see cref="CategorySuggestContext"/>, mirroring the legacy formatter's default branch.
	///
	/// The legacy formatter peeked at a byte segment to find the <c>type</c> property before re-reading the whole
	/// object. <see cref="Utf8JsonReader"/> is forward-only and cannot be rewound, so we buffer the value into a
	/// <see cref="JsonDocument"/>, inspect the DOM to pick the concrete type, then deserialize the buffered element
	/// into that type. Serialization writes by runtime type (as the legacy formatter did via the object formatter).
	/// </summary>
	internal class SuggestContextConverter : JsonConverter<ISuggestContext>
	{
		public override ISuggestContext Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null)
				return null;

			using var doc = JsonDocument.ParseValue(ref reader);
			var root = doc.RootElement;

			if (root.ValueKind != JsonValueKind.Object)
				return null;

			string type = null;
			if (root.TryGetProperty("type", out var t) && t.ValueKind == JsonValueKind.String)
				type = t.GetString();

			switch (type)
			{
				case "geo":
					return root.Deserialize<GeoSuggestContext>(options);
				case "category":
				default:
					return root.Deserialize<CategorySuggestContext>(options);
			}
		}

		public override void Write(Utf8JsonWriter writer, ISuggestContext value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			JsonSerializer.Serialize(writer, value, value.GetType(), options);
		}
	}
}
