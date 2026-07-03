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
	/// A <see cref="System.Text.Json"/> converter for <see cref="Field"/>, replacing the vendored
	/// Utf8Json <c>FieldFormatter</c> as part of #388. It is constructed with the connection settings
	/// (decision D1: settings are threaded through converters rather than a formatter resolver), so it
	/// can run field-name inference via <c>settings.Inferrer.Field</c>.
	/// <para>
	/// A field with no <c>Format</c> is written as the inferred name string; otherwise as
	/// <c>{ "field": &lt;name&gt;, "format": &lt;format&gt; }</c>.
	/// </para>
	/// </summary>
	internal sealed class FieldConverter : JsonConverter<Field>
	{
		private readonly IConnectionSettingsValues _settings;

		public FieldConverter(IConnectionSettingsValues settings) =>
			_settings = settings ?? throw new ArgumentNullException(nameof(settings));

		public override Field Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			switch (reader.TokenType)
			{
				case JsonTokenType.Null:
					return null;
				case JsonTokenType.String:
					return new Field(reader.GetString());
				case JsonTokenType.StartObject:
				{
					string field = null, format = null;
					double? boost = null;
					using var document = JsonDocument.ParseValue(ref reader);
					foreach (var member in document.RootElement.EnumerateObject())
					{
						switch (member.Name)
						{
							case "field": field = member.Value.GetString(); break;
							case "boost": boost = member.Value.GetDouble(); break;
							case "format": format = member.Value.GetString(); break;
						}
					}
					return new Field(field, boost, format);
				}
				default:
					throw new JsonException($"Cannot deserialize {nameof(Field)} from token {reader.TokenType}.");
			}
		}

		public override void Write(Utf8JsonWriter writer, Field value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			var fieldName = _settings.Inferrer.Field(value);
			if (string.IsNullOrEmpty(value.Format))
			{
				writer.WriteStringValue(fieldName);
				return;
			}

			writer.WriteStartObject();
			writer.WriteString("field", fieldName);
			writer.WriteString("format", value.Format);
			writer.WriteEndObject();
		}

		/// <inheritdoc />
		public override Field ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
			new(reader.GetString());

		/// <inheritdoc />
		public override void WriteAsPropertyName(Utf8JsonWriter writer, Field value, JsonSerializerOptions options) =>
			writer.WritePropertyName(_settings.Inferrer.Field(value));
	}
}
