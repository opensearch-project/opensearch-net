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
	/// System.Text.Json replacement for the legacy Utf8Json <c>ContextFormatter</c>.
	///
	/// A <see cref="Context"/> wraps a <c>Union&lt;string, GeoLocation&gt;</c>: it is read either as a category
	/// <c>string</c> (<see cref="Context.Category"/>) or a <see cref="GeoLocation"/> object (<see cref="Context.Geo"/>).
	/// The legacy formatter delegated to the union formatter, which tries the first type (<c>string</c>) before the
	/// second (<see cref="GeoLocation"/>). <see cref="Utf8JsonReader"/> is forward-only, so — like the STJ
	/// <c>UnionConverter</c> — we buffer the value into a <see cref="JsonDocument"/> and attempt each type against
	/// the buffered DOM. A <c>null</c> token (or a value that matches neither type) yields <c>null</c>.
	/// </summary>
	internal class ContextConverter : JsonConverter<Context>
	{
		public override Context Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null)
				return null;

			using var doc = JsonDocument.ParseValue(ref reader);
			var raw = doc.RootElement.GetRawText();

			if (TryRead<string>(raw, options, out var category) && category != null)
				return new Context(category);

			if (TryRead<GeoLocation>(raw, options, out var geo) && geo != null)
				return new Context(geo);

			return null;
		}

		public override void Write(Utf8JsonWriter writer, Context value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			switch (value.Tag)
			{
				case 0:
					JsonSerializer.Serialize(writer, value.Category, options);
					break;
				case 1:
					JsonSerializer.Serialize(writer, value.Geo, options);
					break;
				default:
					writer.WriteNullValue();
					break;
			}
		}

		private static bool TryRead<T>(string raw, JsonSerializerOptions options, out T value)
		{
			try
			{
				value = JsonSerializer.Deserialize<T>(raw, options);
				return true;
			}
			catch
			{
				value = default;
				return false;
			}
		}
	}
}
