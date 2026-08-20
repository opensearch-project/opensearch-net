/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using OpenSearch.Net.Utf8Json; // IJsonFormatterResolver (reused for source serializer parity)

namespace OpenSearch.Client
{
	/// <summary>
	/// System.Text.Json replacement for the legacy Utf8Json <c>FieldValuesFormatter</c>.
	///
	/// <see cref="FieldValues"/> is a dictionary of field-name → the field's values captured as a
	/// <see cref="LazyDocument"/> (the "fields" section of a search hit). Each value is retained verbatim so it can be
	/// coerced to the requested CLR type later via the <c>Inferrer</c>, hence the converter is
	/// <see cref="SettingsAwareConverter{T}"/> (the resulting <see cref="FieldValues"/> is constructed with the
	/// connection's <c>Inferrer</c>).
	///
	/// On read, a non-object yields null (mirroring the legacy formatter). On write, each captured value is emitted
	/// <b>verbatim</b> via <see cref="Utf8JsonWriter.WriteRawValue(System.ReadOnlySpan{byte},bool)"/> — the legacy
	/// formatter wrote the raw stored bytes directly (<c>WriteRaw</c>), NOT through the unindenting path, so this
	/// converter does the same.
	/// </summary>
	internal class FieldValuesConverter : SettingsAwareConverter<FieldValues>
	{
		private IJsonFormatterResolver _resolver;

		public FieldValuesConverter(IConnectionSettingsValues settings) : base(settings) { }

		private IJsonFormatterResolver Resolver =>
			_resolver ?? (_resolver = new OpenSearchClientFormatterResolver(Settings));

		public override FieldValues Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.StartObject)
			{
				reader.Skip();
				return null;
			}

			var fields = new Dictionary<string, LazyDocument>();
			while (reader.Read())
			{
				if (reader.TokenType == JsonTokenType.EndObject)
					break;

				var propertyName = reader.GetString();
				reader.Read();

				using var doc = JsonDocument.ParseValue(ref reader);
				var bytes = Encoding.UTF8.GetBytes(doc.RootElement.GetRawText());
				fields[propertyName] = new LazyDocument(bytes, Resolver);
			}

			return new FieldValues(Settings.Inferrer, fields);
		}

		public override void Write(Utf8JsonWriter writer, FieldValues value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			writer.WriteStartObject();
			foreach (var fieldValue in value)
			{
				writer.WritePropertyName(fieldValue.Key);
				writer.WriteRawValue(fieldValue.Value.Bytes);
			}
			writer.WriteEndObject();
		}
	}
}
