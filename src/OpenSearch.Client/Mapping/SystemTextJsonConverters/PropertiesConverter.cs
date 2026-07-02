/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

#nullable enable

using System;
#nullable enable

using System.Text.Json;
#nullable enable

using System.Text.Json.Serialization;

namespace OpenSearch.Client.Mapping.SystemTextJsonConverters
{
	/// <summary>
	/// Converter for <see cref="IProperties"/> (IDictionary&lt;PropertyName, IProperty&gt;).
	/// JSON: {"field1": {"type": "text", ...}, "field2": {"type": "keyword", ...}}
	/// </summary>
	internal sealed class PropertiesConverter : JsonConverter<IProperties>
	{
		private readonly IConnectionSettingsValues _settings;

		public PropertiesConverter(IConnectionSettingsValues settings) => _settings = settings;

		public override IProperties Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null)
				return null;

			if (reader.TokenType != JsonTokenType.StartObject)
				throw new JsonException($"Expected StartObject for IProperties but got {reader.TokenType}");

			var properties = _settings != null ? new Properties(_settings) : new Properties();
			var propertyConverter = (JsonConverter<IProperty>)options.GetConverter(typeof(IProperty));

			while (reader.Read())
			{
				if (reader.TokenType == JsonTokenType.EndObject)
					break;

				if (reader.TokenType != JsonTokenType.PropertyName)
					throw new JsonException("Expected PropertyName in properties object");

				var fieldName = reader.GetString();
				reader.Read();

				if (reader.TokenType != JsonTokenType.StartObject)
				{
					reader.Skip();
					continue;
				}

				var property = propertyConverter.Read(ref reader, typeof(IProperty), options);
				if (property != null)
				{
					property.Name = fieldName;
					properties.Add(fieldName, property);
				}
			}

			return properties;
		}

		public override void Write(Utf8JsonWriter writer, IProperties value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			var propertyConverter = (JsonConverter<IProperty>)options.GetConverter(typeof(IProperty));

			writer.WriteStartObject();

			foreach (var kvp in value)
			{
				var propertyName = kvp.Key;
				var property = kvp.Value;

				var name = _settings != null
					? _settings.Inferrer.PropertyName(propertyName) ?? propertyName.Name
					: propertyName.Name;

				if (string.IsNullOrEmpty(name))
					continue;

				writer.WritePropertyName(name);
				propertyConverter.Write(writer, property, options);
			}

			writer.WriteEndObject();
		}
	}
}
