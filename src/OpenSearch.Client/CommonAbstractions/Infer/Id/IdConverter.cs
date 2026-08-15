/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Text.Json;

namespace OpenSearch.Client
{
	/// <summary>
	/// System.Text.Json replacement for the legacy Utf8Json <c>IdFormatter</c>. Serializes an <see cref="Id"/> as a
	/// JSON string or number, resolving a document-based id through the runtime <c>Inferrer</c> — hence a
	/// <see cref="SettingsAwareConverter{T}"/>.
	/// </summary>
	internal class IdConverter : SettingsAwareConverter<Id>
	{
		public IdConverter(IConnectionSettingsValues settings) : base(settings) { }

		public override Id Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
			reader.TokenType == JsonTokenType.Number
				? new Id(reader.GetInt64())
				: new Id(reader.GetString());

		public override void Write(Utf8JsonWriter writer, Id value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			if (value.Document != null)
			{
				var documentId = Settings.Inferrer.Id(value.Document.GetType(), value.Document);
				writer.WriteStringValue(documentId);
			}
			else if (value.LongValue != null)
				writer.WriteNumberValue(value.LongValue.Value);
			else
				writer.WriteStringValue(value.StringValue);
		}

		// Id can be a dictionary key. Property names are always strings, so resolve to the id's string form.
		public override Id ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
			new Id(reader.GetString());

		public override void WriteAsPropertyName(Utf8JsonWriter writer, Id value, JsonSerializerOptions options)
		{
			if (value == null) { writer.WritePropertyName(string.Empty); return; }
			var key = value.Document != null
				? Settings.Inferrer.Id(value.Document.GetType(), value.Document)
				: value.LongValue?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? value.StringValue;
			writer.WritePropertyName(key ?? string.Empty);
		}
	}
}
