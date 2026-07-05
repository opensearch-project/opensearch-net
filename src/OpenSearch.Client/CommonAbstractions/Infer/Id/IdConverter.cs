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
	/// A <see cref="System.Text.Json"/> converter for <see cref="Id"/>, replacing the vendored
	/// Utf8Json <c>IdFormatter</c> as part of #388. An id backed by a document infers its id from the
	/// settings; a long id is written as a number; a string id as a string. Constructed with the
	/// connection settings (decision D1) for document-id inference.
	/// </summary>
	internal sealed class IdConverter : JsonConverter<Id>
	{
		private readonly IConnectionSettingsValues _settings;

		public IdConverter(IConnectionSettingsValues settings) =>
			_settings = settings ?? throw new ArgumentNullException(nameof(settings));

		public override Id Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			switch (reader.TokenType)
			{
				case JsonTokenType.Null:
					return null;
				case JsonTokenType.Number:
					return new Id(reader.GetInt64());
				case JsonTokenType.String:
					return new Id(reader.GetString());
				default:
					throw new JsonException($"Cannot deserialize {nameof(Id)} from token {reader.TokenType}.");
			}
		}

		public override void Write(Utf8JsonWriter writer, Id value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			if (value.Document != null)
				writer.WriteStringValue(_settings.Inferrer.Id(value.Document.GetType(), value.Document));
			else if (value.LongValue != null)
				writer.WriteNumberValue(value.LongValue.Value);
			else
				writer.WriteStringValue(value.StringValue);
		}

		/// <inheritdoc />
		public override Id ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
			new(reader.GetString());

		/// <inheritdoc />
		public override void WriteAsPropertyName(Utf8JsonWriter writer, Id value, JsonSerializerOptions options)
		{
			// A dictionary key must be a string; a document-backed id infers its value, a long id is
			// rendered as its decimal string, otherwise the raw string value is used.
			var key = value.Document != null
				? _settings.Inferrer.Id(value.Document.GetType(), value.Document)
				: value.LongValue?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? value.StringValue;
			writer.WritePropertyName(key);
		}
	}
}
