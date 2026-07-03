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
	/// A <see cref="System.Text.Json"/> converter for <see cref="Attachment"/>, replacing the vendored
	/// Utf8Json <c>AttachmentFormatter</c> as part of #388. When the attachment carries no metadata it
	/// is written as the bare base64 content string; otherwise it is written as an object. Reads accept
	/// both a plain string (mapped to <see cref="Attachment.Content"/>) and an object whose property
	/// names include the underscore-prefixed aliases the server may return.
	/// </summary>
	internal sealed class AttachmentConverter : JsonConverter<Attachment>
	{
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

			writer.WriteString("content", value.Content);

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

		public override Attachment Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			using var document = JsonDocument.ParseValue(ref reader);
			var root = document.RootElement;

			switch (root.ValueKind)
			{
				case JsonValueKind.Null:
					return null;
				case JsonValueKind.String:
					return new Attachment { Content = root.GetString() };
				case JsonValueKind.Object:
					break;
				default:
					return null;
			}

			var attachment = new Attachment();
			foreach (var member in root.EnumerateObject())
			{
				switch (member.Name)
				{
					case "_content":
					case "content":
						attachment.Content = member.Value.GetString();
						break;
					case "_name":
					case "name":
						attachment.Name = member.Value.GetString();
						break;
					case "author":
						attachment.Author = member.Value.GetString();
						break;
					case "keywords":
						attachment.Keywords = member.Value.GetString();
						break;
					case "date":
						attachment.Date = member.Value.Deserialize<DateTime?>(options);
						break;
					case "_content_type":
					case "content_type":
						attachment.ContentType = member.Value.GetString();
						break;
					case "_content_length":
					case "content_length":
					case "contentlength":
						attachment.ContentLength = member.Value.Deserialize<long?>(options);
						break;
					case "_language":
					case "language":
						attachment.Language = member.Value.GetString();
						break;
					case "_detect_language":
					case "detect_language":
						attachment.DetectLanguage = member.Value.Deserialize<bool?>(options);
						break;
					case "_indexed_chars":
					case "indexed_chars":
						attachment.IndexedCharacters = member.Value.Deserialize<long?>(options);
						break;
					case "title":
						attachment.Title = member.Value.GetString();
						break;
				}
			}

			return attachment;
		}
	}
}
