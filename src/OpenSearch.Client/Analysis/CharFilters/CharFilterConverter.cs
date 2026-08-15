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
	/// System.Text.Json replacement for the legacy Utf8Json <c>CharFilterFormatter</c>.
	///
	/// <see cref="ICharFilter"/> is polymorphic: the concrete type is selected by the value of the
	/// <c>type</c> field. System.Text.Json's <see cref="Utf8JsonReader"/> is forward-only and cannot be
	/// rewound, so — unlike the Utf8Json version which peeked at a byte segment and re-read it — we buffer
	/// the value into a <see cref="JsonDocument"/>, read the <c>type</c> discriminator from the DOM, then
	/// deserialize the whole element as the matching concrete type. Serialization writes by runtime type.
	/// </summary>
	internal class CharFilterConverter : JsonConverter<ICharFilter>
	{
		public override ICharFilter Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null)
				return null;

			using var doc = JsonDocument.ParseValue(ref reader);
			var root = doc.RootElement;

			var charFilterType = root.TryGetProperty("type", out var t) ? t.GetString() : null;
			if (charFilterType == null)
				return null;

			switch (charFilterType)
			{
				case "html_strip":
					return root.Deserialize<HtmlStripCharFilter>(options);
				case "mapping":
					return root.Deserialize<MappingCharFilter>(options);
				case "pattern_replace":
					return root.Deserialize<PatternReplaceCharFilter>(options);
				case "kuromoji_iteration_mark":
					return root.Deserialize<KuromojiIterationMarkCharFilter>(options);
				case "icu_normalizer":
					return root.Deserialize<IcuNormalizationCharFilter>(options);
				default:
					return null;
			}
		}

		public override void Write(Utf8JsonWriter writer, ICharFilter value, JsonSerializerOptions options)
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
