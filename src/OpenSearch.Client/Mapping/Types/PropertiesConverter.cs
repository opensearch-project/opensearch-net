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
	/// System.Text.Json replacement for the legacy Utf8Json <c>PropertiesFormatter</c>.
	///
	/// <see cref="IProperties"/> is a dictionary of <see cref="PropertyName"/> → <see cref="IProperty"/>. On read each
	/// value is dispatched to its concrete property type through the <see cref="PropertyConverter"/> (registered for
	/// <see cref="IProperty"/>); a value that is not a JSON object is skipped, exactly like the legacy formatter. On
	/// write the legacy formatter deduplicated entries against the runtime property mappings (honouring
	/// <c>Ignore</c> and explicit names) before serializing, so we reproduce that logic here.
	///
	/// The <see cref="PropertyName"/> keys are resolved through the runtime <c>Inferrer</c> (both when sanitizing keys
	/// into a <see cref="Properties"/> instance and when writing property names), so this is a
	/// <see cref="SettingsAwareConverter{T}"/>.
	/// </summary>
	internal class PropertiesConverter : SettingsAwareConverter<IProperties>
	{
		public PropertiesConverter(IConnectionSettingsValues settings) : base(settings) { }

		public override IProperties Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null)
				return null;

			if (reader.TokenType != JsonTokenType.StartObject)
			{
				reader.Skip();
				return null;
			}

			var properties = new Properties(Settings);

			while (reader.Read())
			{
				if (reader.TokenType == JsonTokenType.EndObject)
					break;

				if (reader.TokenType != JsonTokenType.PropertyName)
					continue;

				PropertyName name = reader.GetString();
				reader.Read();

				// The legacy formatter only reads a value that is itself an object; anything else is skipped.
				if (reader.TokenType != JsonTokenType.StartObject)
				{
					reader.Skip();
					continue;
				}

				var property = JsonSerializer.Deserialize<IProperty>(ref reader, options);
				if (property == null)
					continue;

				property.Name = name;
				properties.Add(name, property);
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

			// HACK: Deduplicate property mappings with an instance of Properties that has access to ConnectionSettings
			// to sanitize PropertyName keys (mirrors the legacy PropertiesFormatter.Serialize).
			var properties = new Properties(Settings);

			foreach (var kv in value)
			{
				var clrOrigin = kv.Value as IPropertyWithClrOrigin;
				var propertyInfo = clrOrigin?.ClrOrigin;
				if (propertyInfo == null)
				{
					properties[kv.Key] = kv.Value;
					continue;
				}
				// Check against connection settings mappings
				if (Settings.PropertyMappings.TryGetValue(propertyInfo, out var propertyMapping))
				{
					if (propertyMapping.Ignore)
						continue;

					properties[propertyMapping.Name] = kv.Value;
					continue;
				}
				// Check against attribute mapping, CreatePropertyMapping caches.
				// We do not have to take .Name into account from serializer PropertyName (kv.Key) already handles this
				propertyMapping = Settings.PropertyMappingProvider?.CreatePropertyMapping(propertyInfo);
				if (propertyMapping == null || !propertyMapping.Ignore)
					properties[kv.Key] = kv.Value;
			}

			writer.WriteStartObject();
			foreach (var kv in properties)
			{
				writer.WritePropertyName(Settings.Inferrer.PropertyName(kv.Key));
				JsonSerializer.Serialize(writer, kv.Value, options);
			}
			writer.WriteEndObject();
		}
	}
}
