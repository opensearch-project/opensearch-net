/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenSearch.Client
{
	/// <summary>
	/// A <see cref="System.Text.Json"/> converter for <see cref="IProperties"/> — the mapping's named
	/// <c>PropertyName</c> → <c>IProperty</c> dictionary — replacing the vendored Utf8Json
	/// <c>PropertiesFormatter</c> as part of #388. Constructed with the connection settings so keys are
	/// resolved through the property-name inferrer. Multi-fields and object/nested sub-properties
	/// recurse back through this converter.
	/// </summary>
	internal sealed class PropertiesConverter : JsonConverter<IProperties>
	{
		private readonly IConnectionSettingsValues _settings;

		public PropertiesConverter(IConnectionSettingsValues settings) =>
			_settings = settings ?? throw new ArgumentNullException(nameof(settings));

		public override IProperties Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null) return null;

			using var document = JsonDocument.ParseValue(ref reader);
			var root = document.RootElement;
			if (root.ValueKind != JsonValueKind.Object) return null;

			var properties = new Properties();
			foreach (var member in root.EnumerateObject())
			{
				if (member.Value.ValueKind != JsonValueKind.Object) continue;

				var property = member.Value.Deserialize<IProperty>(options);
				if (property == null) continue;
				property.Name = member.Name;
				properties.Add(member.Name, property);
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

			// Mirror the vendored PropertiesFormatter: drop members whose CLR origin is mapped as
			// ignored (via ConnectionSettings or an attribute/serializer mapping) and re-key explicitly
			// mapped members, deduplicating through a settings-aware Properties instance before writing.
			var properties = new Properties(_settings);
			foreach (var entry in (IEnumerable<KeyValuePair<PropertyName, IProperty>>)value)
			{
				var propertyInfo = (entry.Value as IPropertyWithClrOrigin)?.ClrOrigin;
				if (propertyInfo == null)
				{
					properties[entry.Key] = entry.Value;
					continue;
				}

				if (_settings.PropertyMappings.TryGetValue(propertyInfo, out var propertyMapping))
				{
					if (propertyMapping.Ignore)
						continue;

					properties[propertyMapping.Name] = entry.Value;
					continue;
				}

				var attributeMapping = _settings.PropertyMappingProvider?.CreatePropertyMapping(propertyInfo);
				if (attributeMapping == null || !attributeMapping.Ignore)
					properties[entry.Key] = entry.Value;
			}

			writer.WriteStartObject();
			foreach (var entry in (IEnumerable<KeyValuePair<PropertyName, IProperty>>)properties)
			{
				writer.WritePropertyName(_settings.Inferrer.PropertyName(entry.Key));
				JsonSerializer.Serialize(writer, entry.Value, options);
			}
			writer.WriteEndObject();
		}
	}
}
