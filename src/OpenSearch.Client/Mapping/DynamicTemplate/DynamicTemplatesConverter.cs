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
	/// A <see cref="System.Text.Json"/> converter for <see cref="IDynamicTemplateContainer"/>, replacing
	/// the vendored Utf8Json <c>DynamicTemplatesInterfaceFormatter</c>/<c>DynamicTemplatesFormatter</c>
	/// as part of #388. Serialized as an ordered array of single-property objects
	/// (<c>[ { "&lt;name&gt;": { …template… } }, … ]</c>).
	/// </summary>
	internal sealed class DynamicTemplatesConverter : JsonConverter<IDynamicTemplateContainer>
	{
		public override void Write(Utf8JsonWriter writer, IDynamicTemplateContainer value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			writer.WriteStartArray();
			foreach (var entry in value)
			{
				writer.WriteStartObject();
				writer.WritePropertyName(entry.Key);
				JsonSerializer.Serialize(writer, entry.Value, entry.Value?.GetType() ?? typeof(IDynamicTemplate), options);
				writer.WriteEndObject();
			}
			writer.WriteEndArray();
		}

		public override IDynamicTemplateContainer Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.StartArray) { reader.Skip(); return null; }

			using var document = JsonDocument.ParseValue(ref reader);
			var container = new DynamicTemplateContainer();
			foreach (var element in document.RootElement.EnumerateArray())
			{
				if (element.ValueKind != JsonValueKind.Object) continue;
				foreach (var member in element.EnumerateObject())
					container.Add(member.Name, member.Value.Deserialize<DynamicTemplate>(options));
			}

			return container;
		}
	}
}
