/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using OpenSearch.Net;

namespace OpenSearch.Client
{
	/// <summary>
	/// System.Text.Json replacement for the legacy Utf8Json <c>SortOrderFormatter&lt;TSortOrder&gt;</c>. An
	/// <see cref="ISortOrder"/> is serialized as a single-property JSON object whose property name is the
	/// <see cref="ISortOrder.Key"/> and whose value is the <see cref="SortOrder"/>.
	/// </summary>
	internal class SortOrderConverter<TSortOrder> : JsonConverter<TSortOrder>
		where TSortOrder : class, ISortOrder, new()
	{
		public override TSortOrder Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.StartObject)
			{
				reader.Skip();
				return null;
			}

			var sortOrder = new TSortOrder();
			while (reader.Read())
			{
				if (reader.TokenType == JsonTokenType.EndObject)
					break;

				sortOrder.Key = reader.GetString();
				reader.Read();
				sortOrder.Order = JsonSerializer.Deserialize<SortOrder>(ref reader, options);
			}

			return sortOrder;
		}

		public override void Write(Utf8JsonWriter writer, TSortOrder value, JsonSerializerOptions options)
		{
			if (value?.Key == null)
			{
				writer.WriteNullValue();
				return;
			}

			writer.WriteStartObject();
			writer.WritePropertyName(value.Key);
			JsonSerializer.Serialize(writer, value.Order, options);
			writer.WriteEndObject();
		}
	}
}
