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
	/// A <see cref="System.Text.Json"/> converter for the <c>get field mapping</c> response
	/// <c>mapping</c> object (<c>IReadOnlyDictionary&lt;Field, IFieldMapping&gt;</c>), replacing the
	/// vendored Utf8Json <c>FieldMappingFormatter</c> as part of #388. Each entry is a meta field
	/// (<c>_source</c>/<c>_routing</c>/<c>_size</c>) or a user <see cref="IProperty"/>; other reserved
	/// keys (<c>_all</c>, <c>_index</c>) are ignored. The result is wrapped in a resolvable dictionary.
	/// </summary>
	internal sealed class FieldMappingConverter : JsonConverter<IReadOnlyDictionary<Field, IFieldMapping>>
	{
		private readonly IConnectionSettingsValues _settings;

		public FieldMappingConverter(IConnectionSettingsValues settings) => _settings = settings;

		public override IReadOnlyDictionary<Field, IFieldMapping> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			var fieldMappings = new Dictionary<Field, IFieldMapping>();
			if (reader.TokenType == JsonTokenType.Null)
				return new ResolvableDictionaryProxy<Field, IFieldMapping>(_settings, fieldMappings);

			using var document = JsonDocument.ParseValue(ref reader);
			var root = document.RootElement;
			if (root.ValueKind != JsonValueKind.Object)
				return new ResolvableDictionaryProxy<Field, IFieldMapping>(_settings, fieldMappings);

			foreach (var member in root.EnumerateObject())
			{
				IFieldMapping mapping = member.Name switch
				{
					"_all" => null,
					"_index" => null,
					"_source" => member.Value.Deserialize<SourceField>(options),
					"_routing" => member.Value.Deserialize<RoutingField>(options),
					"_size" => member.Value.Deserialize<SizeField>(options),
					_ => ReadProperty(member.Name, member.Value, options),
				};

				if (mapping != null)
					fieldMappings[member.Name] = mapping;
			}

			return new ResolvableDictionaryProxy<Field, IFieldMapping>(_settings, fieldMappings);
		}

		private static IFieldMapping ReadProperty(string name, JsonElement element, JsonSerializerOptions options)
		{
			var property = element.Deserialize<IProperty>(options);
			if (property != null)
				property.Name = name;
			return property;
		}

		public override void Write(Utf8JsonWriter writer, IReadOnlyDictionary<Field, IFieldMapping> value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			writer.WriteStartObject();
			foreach (var entry in value)
			{
				writer.WritePropertyName(_settings.Inferrer.Field(entry.Key));
				JsonSerializer.Serialize(writer, entry.Value, entry.Value?.GetType() ?? typeof(IFieldMapping), options);
			}
			writer.WriteEndObject();
		}
	}
}
