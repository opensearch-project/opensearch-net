/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

#nullable enable

using System;
#nullable enable

using System.Collections.Generic;
#nullable enable

using System.Text.Json;
#nullable enable

using System.Text.Json.Serialization;
#nullable enable

using OpenSearch.Client;

namespace OpenSearch.Client.QueryDsl.SystemTextJsonConverters
{
	/// <summary>
	/// Converter for <see cref="IPercolateQuery"/>.
	/// JSON format:
	/// <code>
	/// {"field": "query", "document": {...}}
	/// </code>
	/// or for existing document lookup:
	/// <code>
	/// {"field": "query", "index": "...", "id": "..."}
	/// </code>
	/// </summary>
	internal sealed class PercolateQueryConverter : JsonConverter<IPercolateQuery>
	{
		public override IPercolateQuery Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null)
				return null;

			if (reader.TokenType != JsonTokenType.StartObject)
				throw new JsonException($"Expected StartObject for IPercolateQuery but got {reader.TokenType}");

			var query = new PercolateQuery();

			while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
			{
				if (reader.TokenType != JsonTokenType.PropertyName) continue;
				var prop = reader.GetString();
				reader.Read();

				switch (prop)
				{
					case "field":
						query.Field = new Field(reader.GetString());
						break;
					case "document":
						query.Document = JsonSerializer.Deserialize<object>(ref reader, options);
						break;
					case "documents":
						query.Documents = JsonSerializer.Deserialize<IEnumerable<object>>(ref reader, options);
						break;
					case "index":
						query.Index = reader.GetString();
						break;
					case "id":
						query.Id = new Id(reader.GetString());
						break;
					case "routing":
						query.Routing = reader.GetString();
						break;
					case "preference":
						query.Preference = reader.GetString();
						break;
					case "version":
						query.Version = reader.GetInt64();
						break;
					case "boost":
						query.Boost = reader.GetDouble();
						break;
					case "_name":
						query.Name = reader.GetString();
						break;
					default:
						reader.Skip();
						break;
				}
			}

			return query;
		}

		public override void Write(Utf8JsonWriter writer, IPercolateQuery value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			writer.WriteStartObject();

			if (value.Field != null)
			{
				writer.WritePropertyName("field");
				writer.WriteStringValue(value.Field.ToString());
			}

			if (value.Document != null)
			{
				writer.WritePropertyName("document");
				JsonSerializer.Serialize(writer, value.Document, options);
			}

			if (value.Documents != null)
			{
				writer.WritePropertyName("documents");
				JsonSerializer.Serialize(writer, value.Documents, options);
			}

			if (value.Index != null)
			{
				writer.WritePropertyName("index");
				writer.WriteStringValue(value.Index.ToString());
			}

			if (value.Id != null)
			{
				writer.WritePropertyName("id");
				writer.WriteStringValue(value.Id.ToString());
			}

			if (value.Routing != null)
			{
				writer.WritePropertyName("routing");
				writer.WriteStringValue(value.Routing.ToString());
			}

			if (!string.IsNullOrEmpty(value.Preference))
			{
				writer.WritePropertyName("preference");
				writer.WriteStringValue(value.Preference);
			}

			if (value.Version.HasValue)
			{
				writer.WritePropertyName("version");
				writer.WriteNumberValue(value.Version.Value);
			}

			if (value.Boost.HasValue)
			{
				writer.WritePropertyName("boost");
				writer.WriteNumberValue(value.Boost.Value);
			}

			if (!string.IsNullOrEmpty(value.Name))
			{
				writer.WritePropertyName("_name");
				writer.WriteStringValue(value.Name);
			}

			writer.WriteEndObject();
		}
	}
}
