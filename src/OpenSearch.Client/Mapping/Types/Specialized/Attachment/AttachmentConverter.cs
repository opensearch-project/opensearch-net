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
	/// System.Text.Json replacement for the legacy Utf8Json <c>AttachmentFormatter</c>.
	///
	/// An <see cref="Attachment"/> is read either from a bare JSON string (interpreted as the base64
	/// <see cref="Attachment.Content"/>) or from an object. The object form has a field-name quirk carried over
	/// verbatim: every metadata field accepts both a plain and an underscore-prefixed alias (e.g. <c>content</c> /
	/// <c>_content</c>, <c>name</c> / <c>_name</c>, <c>content_length</c> / <c>_content_length</c> /
	/// <c>contentlength</c>, …). On write, only the plain field names are emitted, and only when populated.
	///
	/// Write mirrors the legacy behavior exactly: when the attachment carries no metadata it is written as a bare
	/// string (the content); otherwise an object is written with <c>content</c> first followed by the populated
	/// metadata fields in the legacy field order.
	/// </summary>
	internal class AttachmentConverter : JsonConverter<Attachment>
	{
		public override Attachment Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			switch (reader.TokenType)
			{
				case JsonTokenType.Null:
					return null;
				case JsonTokenType.String:
					return new Attachment { Content = reader.GetString() };
				case JsonTokenType.StartObject:
					var attachment = new Attachment();
					while (reader.Read())
					{
						if (reader.TokenType == JsonTokenType.EndObject)
							break;

						var propertyName = reader.GetString();
						reader.Read();

						switch (propertyName)
						{
							case "_content":
							case "content":
								attachment.Content = ReadStringOrNull(ref reader);
								break;
							case "_name":
							case "name":
								attachment.Name = ReadStringOrNull(ref reader);
								break;
							case "author":
								attachment.Author = ReadStringOrNull(ref reader);
								break;
							case "keywords":
								attachment.Keywords = ReadStringOrNull(ref reader);
								break;
							case "date":
								attachment.Date = JsonSerializer.Deserialize<DateTime?>(ref reader, options);
								break;
							case "_content_type":
							case "content_type":
								attachment.ContentType = ReadStringOrNull(ref reader);
								break;
							case "_content_length":
							case "content_length":
							case "contentlength":
								attachment.ContentLength = ReadNullableLong(ref reader);
								break;
							case "_language":
							case "language":
								attachment.Language = ReadStringOrNull(ref reader);
								break;
							case "_detect_language":
							case "detect_language":
								attachment.DetectLanguage = ReadNullableBoolean(ref reader);
								break;
							case "_indexed_chars":
							case "indexed_chars":
								attachment.IndexedCharacters = ReadNullableLong(ref reader);
								break;
							case "title":
								attachment.Title = ReadStringOrNull(ref reader);
								break;
							default:
								// Legacy left unknown fields' values unconsumed (a latent bug that never bit because
								// attachments only carry known fields); STJ requires the value be consumed to keep the
								// reader valid, so we skip it.
								reader.Skip();
								break;
						}
					}

					return attachment;
				default:
					reader.Skip();
					return null;
			}
		}

		public override void Write(Utf8JsonWriter writer, Attachment value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			if (!value.ContainsMetadata)
			{
				writer.WriteStringValue(value.Content);
				return;
			}

			writer.WriteStartObject();

			writer.WritePropertyName("content");
			writer.WriteStringValue(value.Content);

			if (!string.IsNullOrEmpty(value.Author))
				writer.WriteString("author", value.Author);

			if (value.ContentLength.HasValue)
				writer.WriteNumber("content_length", value.ContentLength.Value);

			if (!string.IsNullOrEmpty(value.ContentType))
				writer.WriteString("content_type", value.ContentType);

			if (value.Date.HasValue)
			{
				writer.WritePropertyName("date");
				JsonSerializer.Serialize(writer, value.Date, options);
			}

			if (value.DetectLanguage.HasValue)
				writer.WriteBoolean("detect_language", value.DetectLanguage.Value);

			if (value.IndexedCharacters.HasValue)
				writer.WriteNumber("indexed_chars", value.IndexedCharacters.Value);

			if (!string.IsNullOrEmpty(value.Keywords))
				writer.WriteString("keywords", value.Keywords);

			if (!string.IsNullOrEmpty(value.Language))
				writer.WriteString("language", value.Language);

			if (!string.IsNullOrEmpty(value.Name))
				writer.WriteString("name", value.Name);

			if (!string.IsNullOrEmpty(value.Title))
				writer.WriteString("title", value.Title);

			writer.WriteEndObject();
		}

		private static string ReadStringOrNull(ref Utf8JsonReader reader) =>
			reader.TokenType == JsonTokenType.Null ? null : reader.GetString();

		private static long? ReadNullableLong(ref Utf8JsonReader reader) =>
			reader.TokenType == JsonTokenType.Null ? (long?)null : reader.GetInt64();

		private static bool? ReadNullableBoolean(ref Utf8JsonReader reader) =>
			reader.TokenType == JsonTokenType.Null ? (bool?)null : reader.GetBoolean();
	}
}
