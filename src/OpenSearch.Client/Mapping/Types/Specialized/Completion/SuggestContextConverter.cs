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
	/// A <see cref="System.Text.Json"/> converter for the polymorphic <see cref="ISuggestContext"/>
	/// (completion suggester mapping contexts), replacing the vendored Utf8Json
	/// <c>SuggestContextFormatter</c> as part of #388. Dispatches on the <c>type</c> discriminator
	/// (<c>geo</c> → <see cref="GeoSuggestContext"/>, otherwise <see cref="CategorySuggestContext"/>).
	/// On write the concrete instance is serialized as itself.
	/// </summary>
	internal sealed class SuggestContextConverter : JsonConverter<ISuggestContext>
	{
		public override ISuggestContext Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null) return null;

			using var document = JsonDocument.ParseValue(ref reader);
			var root = document.RootElement;
			if (root.ValueKind != JsonValueKind.Object) return null;

			var type = root.TryGetProperty("type", out var typeElement) && typeElement.ValueKind == JsonValueKind.String
				? typeElement.GetString()
				: null;

			return string.Equals(type, "geo", StringComparison.Ordinal)
				? root.Deserialize<GeoSuggestContext>(options)
				: (ISuggestContext)root.Deserialize<CategorySuggestContext>(options);
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
