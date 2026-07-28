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
	/// System.Text.Json replacement for the legacy Utf8Json <c>PropertyNameFormatter</c>. Resolves a
	/// <see cref="PropertyName"/> through the runtime <c>Inferrer</c> when serializing — hence a
	/// <see cref="SettingsAwareConverter{T}"/>.
	/// </summary>
	internal class PropertyNameConverter : SettingsAwareConverter<PropertyName>
	{
		public PropertyNameConverter(IConnectionSettingsValues settings) : base(settings) { }

		public override PropertyName Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.String)
			{
				reader.Skip();
				return null;
			}

			PropertyName propertyName = reader.GetString();
			return propertyName;
		}

		public override void Write(Utf8JsonWriter writer, PropertyName value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			writer.WriteStringValue(Settings.Inferrer.PropertyName(value));
		}

		// PropertyName is used as a dictionary key. STJ needs these overrides to (de)serialize it as a property name.
		public override PropertyName ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			PropertyName propertyName = reader.GetString();
			return propertyName;
		}

		public override void WriteAsPropertyName(Utf8JsonWriter writer, PropertyName value, JsonSerializerOptions options) =>
			writer.WritePropertyName(value == null ? string.Empty : Settings.Inferrer.PropertyName(value));
	}
}
