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
	/// System.Text.Json replacement for the legacy Utf8Json <c>DynamicTemplatesInterfaceFormatter</c>.
	/// A <see cref="IDynamicTemplateContainer" /> is serialized as a JSON array of single-property
	/// objects, each mapping a template name to its <see cref="IDynamicTemplate" /> definition.
	/// </summary>
	internal class DynamicTemplatesInterfaceConverter : JsonConverter<IDynamicTemplateContainer>
	{
		public override IDynamicTemplateContainer Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
			JsonSerializer.Deserialize<DynamicTemplateContainer>(ref reader, options);

		public override void Write(Utf8JsonWriter writer, IDynamicTemplateContainer value, JsonSerializerOptions options) =>
			WriteContainer(writer, value, options);

		internal static void WriteContainer(Utf8JsonWriter writer, IDynamicTemplateContainer value, JsonSerializerOptions options)
		{
			// Mirrors the legacy formatter: an empty/absent container writes an empty array.
			writer.WriteStartArray();
			if (value != null && value.HasAny())
			{
				foreach (var p in value)
				{
					writer.WriteStartObject();
					writer.WritePropertyName(p.Key);
					JsonSerializer.Serialize(writer, p.Value, options);
					writer.WriteEndObject();
				}
			}
			writer.WriteEndArray();
		}
	}

	/// <summary>
	/// System.Text.Json replacement for the legacy Utf8Json <c>DynamicTemplatesFormatter</c>.
	/// Reads a JSON array of single-property objects into a <see cref="DynamicTemplateContainer" />
	/// and delegates serialization to <see cref="DynamicTemplatesInterfaceConverter" />.
	/// </summary>
	internal class DynamicTemplatesConverter : JsonConverter<DynamicTemplateContainer>
	{
		public override DynamicTemplateContainer Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null)
				return null;

			var container = new DynamicTemplateContainer();

			if (reader.TokenType != JsonTokenType.StartArray)
				throw new JsonException($"Unexpected token type {reader.TokenType} when deserializing {nameof(DynamicTemplateContainer)}.");

			while (reader.Read())
			{
				if (reader.TokenType == JsonTokenType.EndArray)
					break;

				// Each array element is an object with a single property: { name: template }.
				string name = null;
				IDynamicTemplate template = null;

				if (reader.TokenType != JsonTokenType.StartObject)
					throw new JsonException($"Unexpected token type {reader.TokenType} when deserializing {nameof(DynamicTemplateContainer)} element.");

				while (reader.Read())
				{
					if (reader.TokenType == JsonTokenType.EndObject)
						break;

					name = reader.GetString();
					reader.Read();
					template = JsonSerializer.Deserialize<IDynamicTemplate>(ref reader, options);
				}

				container.Add(name, template);
			}

			return container;
		}

		public override void Write(Utf8JsonWriter writer, DynamicTemplateContainer value, JsonSerializerOptions options) =>
			DynamicTemplatesInterfaceConverter.WriteContainer(writer, value, options);
	}
}
