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
	/// A <see cref="System.Text.Json"/> converter for <see cref="PropertyName"/>, replacing the
	/// vendored Utf8Json <c>PropertyNameFormatter</c> as part of #388. Constructed with the connection
	/// settings so it can infer the property name via <c>settings.Inferrer.PropertyName</c>. Supports
	/// use both as a value and as a JSON property name (object keys).
	/// </summary>
	internal sealed class PropertyNameConverter : JsonConverter<PropertyName>
	{
		private readonly IConnectionSettingsValues _settings;

		public PropertyNameConverter(IConnectionSettingsValues settings) =>
			_settings = settings ?? throw new ArgumentNullException(nameof(settings));

		public override PropertyName Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
			reader.TokenType == JsonTokenType.String ? reader.GetString() : null;

		public override void Write(Utf8JsonWriter writer, PropertyName value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			writer.WriteStringValue(_settings.Inferrer.PropertyName(value));
		}

		/// <inheritdoc />
		public override PropertyName ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
			reader.GetString();

		/// <inheritdoc />
		public override void WriteAsPropertyName(Utf8JsonWriter writer, PropertyName value, JsonSerializerOptions options) =>
			writer.WritePropertyName(_settings.Inferrer.PropertyName(value));
	}
}
