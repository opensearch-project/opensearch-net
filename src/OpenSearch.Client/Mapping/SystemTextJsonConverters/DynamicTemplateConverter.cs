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
	/// Converter for <see cref="IDynamicTemplateContainer"/>.
	/// Dynamic templates serialize as an array of single-key objects:
	/// [{"template_name": {"match": "*", "mapping": {...}}}, ...]
	/// </summary>
	internal sealed class DynamicTemplateContainerInterfaceConverter : JsonConverter<IDynamicTemplateContainer>
	{
		public override IDynamicTemplateContainer Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null)
				return null;

			if (reader.TokenType != JsonTokenType.StartArray)
				throw new JsonException($"Expected StartArray for IDynamicTemplateContainer but got {reader.TokenType}");

			var container = new DynamicTemplateContainer();

			while (reader.Read())
			{
				if (reader.TokenType == JsonTokenType.EndArray)
					break;

				if (reader.TokenType != JsonTokenType.StartObject)
					throw new JsonException("Expected StartObject for dynamic template array element");

				reader.Read();
				if (reader.TokenType == JsonTokenType.EndObject)
					continue;

				if (reader.TokenType != JsonTokenType.PropertyName)
					throw new JsonException("Expected PropertyName in dynamic template element");

				var templateName = reader.GetString();
				reader.Read();

				var template = JsonSerializer.Deserialize<DynamicTemplate>(ref reader, options);
				if (template != null)
					container.Add(templateName, template);

				// Read EndObject of the single-key wrapper
				reader.Read();
			}

			return container;
		}

		public override void Write(Utf8JsonWriter writer, IDynamicTemplateContainer value, JsonSerializerOptions options)
		{
			if (value == null || !value.HasAny())
			{
				writer.WriteNullValue();
				return;
			}

			writer.WriteStartArray();

			foreach (var kvp in value)
			{
				writer.WriteStartObject();
				writer.WritePropertyName(kvp.Key);
				JsonSerializer.Serialize(writer, kvp.Value, typeof(IDynamicTemplate), options);
				writer.WriteEndObject();
			}

			writer.WriteEndArray();
		}
	}

	/// <summary>
	/// Converter for <see cref="DynamicTemplateContainer"/> concrete type.
	/// Delegates to <see cref="DynamicTemplateContainerInterfaceConverter"/>.
	/// </summary>
	internal sealed class DynamicTemplateContainerConverter : JsonConverter<DynamicTemplateContainer>
	{
		private static readonly DynamicTemplateContainerInterfaceConverter InterfaceConverter =
			new DynamicTemplateContainerInterfaceConverter();

		public override DynamicTemplateContainer Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			var result = InterfaceConverter.Read(ref reader, typeof(IDynamicTemplateContainer), options);
			if (result is DynamicTemplateContainer concrete)
				return concrete;
			return result != null ? new DynamicTemplateContainer(result) : null;
		}

		public override void Write(Utf8JsonWriter writer, DynamicTemplateContainer value, JsonSerializerOptions options) =>
			InterfaceConverter.Write(writer, value, options);
	}
}
