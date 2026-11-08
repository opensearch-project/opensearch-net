/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Collections.Generic;
using System.Text.Json;

namespace OpenSearch.Client
{
	/// <summary>
	/// System.Text.Json replacement for the legacy Utf8Json <see cref="FieldMappingFormatter"/>.
	///
	/// The <c>mapping</c> object of a get-field-mapping response is a dictionary keyed by field name whose values are
	/// polymorphic <see cref="IFieldMapping"/> instances: the meta fields (<c>_source</c>, <c>_routing</c>, <c>_size</c>)
	/// map to their concrete field types while every other key is a normal property mapping dispatched through
	/// <see cref="IProperty"/>. System.Text.Json cannot deserialize the abstract <see cref="IFieldMapping"/> on its own
	/// (it throws <c>NotSupportedException</c> for interfaces), so — exactly like the legacy formatter — we read the
	/// discriminating key ourselves and route to the correct concrete type, then wrap the result in a
	/// <see cref="ResolvableDictionaryProxy{TKey,TValue}"/> so keys resolve through the runtime inferrer.
	/// </summary>
	internal class FieldMappingConverter : SettingsAwareConverter<IReadOnlyDictionary<Field, IFieldMapping>>
	{
		public FieldMappingConverter(IConnectionSettingsValues settings) : base(settings) { }

		public override IReadOnlyDictionary<Field, IFieldMapping> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			var fieldMappings = new Dictionary<Field, IFieldMapping>();

			if (reader.TokenType != JsonTokenType.StartObject)
			{
				reader.Skip();
				return new ResolvableDictionaryProxy<Field, IFieldMapping>(Settings, fieldMappings);
			}

			while (reader.Read())
			{
				if (reader.TokenType == JsonTokenType.EndObject)
					break;

				if (reader.TokenType != JsonTokenType.PropertyName)
					continue;

				var propertyName = reader.GetString();
				reader.Read();

				IFieldMapping mapping = null;
				switch (propertyName)
				{
					case "_all":
					case "_index":
						// No dedicated mapping type; the legacy formatter read and discarded these keys.
						reader.Skip();
						break;
					case "_source":
						mapping = JsonSerializer.Deserialize<SourceField>(ref reader, options);
						break;
					case "_routing":
						mapping = JsonSerializer.Deserialize<RoutingField>(ref reader, options);
						break;
					case "_size":
						mapping = JsonSerializer.Deserialize<SizeField>(ref reader, options);
						break;
					default:
						var property = JsonSerializer.Deserialize<IProperty>(ref reader, options);
						if (property != null)
							property.Name = propertyName;
						mapping = property;
						break;
				}

				if (mapping != null)
					fieldMappings[propertyName] = mapping;
			}

			return new ResolvableDictionaryProxy<Field, IFieldMapping>(Settings, fieldMappings);
		}

		public override void Write(Utf8JsonWriter writer, IReadOnlyDictionary<Field, IFieldMapping> value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			writer.WriteStartObject();
			foreach (var kv in value)
			{
				writer.WritePropertyName(Settings.Inferrer.Field(kv.Key));
				if (kv.Value == null)
					writer.WriteNullValue();
				else
					JsonSerializer.Serialize(writer, kv.Value, kv.Value.GetType(), options);
			}
			writer.WriteEndObject();
		}
	}
}
