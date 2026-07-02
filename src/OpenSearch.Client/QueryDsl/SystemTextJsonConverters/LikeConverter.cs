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
#nullable enable

using OpenSearch.Client;

namespace OpenSearch.Client.QueryDsl.SystemTextJsonConverters
{
	/// <summary>
	/// Converter for <see cref="Like"/>.
	/// Like can be:
	/// <list type="bullet">
	///   <item>A string (document text): <c>"some text"</c></item>
	///   <item>A document reference: <c>{"_index": "...", "_id": "..."}</c></item>
	/// </list>
	/// </summary>
	internal sealed class LikeConverter : JsonConverter<Like>
	{
		public override Like Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null)
				return null;

			if (reader.TokenType == JsonTokenType.String)
				return new Like(reader.GetString());

			if (reader.TokenType == JsonTokenType.StartObject)
			{
				var likeDoc = ReadLikeDocument(ref reader, options);
				return new Like(likeDoc);
			}

			throw new JsonException($"Unexpected token {reader.TokenType} when reading Like");
		}

		public override void Write(Utf8JsonWriter writer, Like value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			// Union<string, ILikeDocument>
			if (value.Tag == 0)
			{
				writer.WriteStringValue(value.Item1);
			}
			else if (value.Item2 != null)
			{
				WriteLikeDocument(writer, value.Item2, options);
			}
			else
			{
				writer.WriteNullValue();
			}
		}

		private static ILikeDocument ReadLikeDocument(ref Utf8JsonReader reader, JsonSerializerOptions options)
		{
			var doc = new LikeDocument<object>();

			while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
			{
				if (reader.TokenType != JsonTokenType.PropertyName)
					continue;

				var prop = reader.GetString();
				reader.Read();

				switch (prop)
				{
					case "_index":
						doc.Index = reader.GetString();
						break;
					case "_id":
						doc.Id = new Id(reader.GetString());
						break;
					case "_routing":
						doc.Routing = reader.GetString();
						break;
					case "doc":
					case "_source":
						doc.Document = JsonSerializer.Deserialize<object>(ref reader, options);
						break;
					case "fields":
						doc.Fields = JsonSerializer.Deserialize<Fields>(ref reader, options);
						break;
					case "per_field_analyzer":
						doc.PerFieldAnalyzer = JsonSerializer.Deserialize<IPerFieldAnalyzer>(ref reader, options);
						break;
					default:
						reader.Skip();
						break;
				}
			}

			return doc;
		}

		private static void WriteLikeDocument(Utf8JsonWriter writer, ILikeDocument doc, JsonSerializerOptions options)
		{
			writer.WriteStartObject();

			if (doc.Index != null)
			{
				writer.WritePropertyName("_index");
				writer.WriteStringValue(doc.Index.ToString());
			}

			if (doc.Id != null)
			{
				writer.WritePropertyName("_id");
				writer.WriteStringValue(doc.Id.ToString());
			}

			if (doc.Routing != null)
			{
				writer.WritePropertyName("_routing");
				writer.WriteStringValue(doc.Routing.ToString());
			}

			if (doc.Document != null)
			{
				writer.WritePropertyName("doc");
				JsonSerializer.Serialize(writer, doc.Document, options);
			}

			if (doc.Fields != null)
			{
				writer.WritePropertyName("fields");
				JsonSerializer.Serialize(writer, doc.Fields, options);
			}

			if (doc.PerFieldAnalyzer != null)
			{
				writer.WritePropertyName("per_field_analyzer");
				JsonSerializer.Serialize(writer, doc.PerFieldAnalyzer, options);
			}

			writer.WriteEndObject();
		}
	}
}
