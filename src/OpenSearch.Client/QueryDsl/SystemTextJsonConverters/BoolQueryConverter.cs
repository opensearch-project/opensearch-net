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

namespace OpenSearch.Client.QueryDsl.SystemTextJsonConverters
{
	/// <summary>
	/// Converter for <see cref="IBoolQuery"/>.
	/// JSON format:
	/// <code>
	/// {
	///   "must": [...],
	///   "should": [...],
	///   "must_not": [...],
	///   "filter": [...],
	///   "minimum_should_match": 1,
	///   "boost": 1.0,
	///   "_name": "named_query"
	/// }
	/// </code>
	/// </summary>
	internal sealed class BoolQueryConverter : JsonConverter<IBoolQuery>
	{
		private static readonly QueryContainerCollectionConverter CollectionConverter = new QueryContainerCollectionConverter();

		public override IBoolQuery Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null)
				return null;

			if (reader.TokenType != JsonTokenType.StartObject)
				throw new JsonException($"Expected StartObject token but got {reader.TokenType}");

			var query = new BoolQuery();

			while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
			{
				if (reader.TokenType != JsonTokenType.PropertyName)
					throw new JsonException("Expected PropertyName token");

				var propertyName = reader.GetString();
				reader.Read();

				switch (propertyName)
				{
					case "must":
						query.Must = CollectionConverter.Read(ref reader, typeof(IEnumerable<QueryContainer>), options);
						break;
					case "should":
						query.Should = CollectionConverter.Read(ref reader, typeof(IEnumerable<QueryContainer>), options);
						break;
					case "must_not":
						query.MustNot = CollectionConverter.Read(ref reader, typeof(IEnumerable<QueryContainer>), options);
						break;
					case "filter":
						query.Filter = CollectionConverter.Read(ref reader, typeof(IEnumerable<QueryContainer>), options);
						break;
					case "minimum_should_match":
						query.MinimumShouldMatch = ReadMinimumShouldMatch(ref reader);
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

		public override void Write(Utf8JsonWriter writer, IBoolQuery value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			writer.WriteStartObject();

			if (value.Must != null && value.ShouldSerializeMust())
			{
				writer.WritePropertyName("must");
				CollectionConverter.Write(writer, value.Must, options);
			}

			if (value.Should != null && value.ShouldSerializeShould())
			{
				writer.WritePropertyName("should");
				CollectionConverter.Write(writer, value.Should, options);
			}

			if (value.MustNot != null && value.ShouldSerializeMustNot())
			{
				writer.WritePropertyName("must_not");
				CollectionConverter.Write(writer, value.MustNot, options);
			}

			if (value.Filter != null && value.ShouldSerializeFilter())
			{
				writer.WritePropertyName("filter");
				CollectionConverter.Write(writer, value.Filter, options);
			}

			if (value.MinimumShouldMatch != null)
			{
				writer.WritePropertyName("minimum_should_match");
				WriteMinimumShouldMatch(writer, value.MinimumShouldMatch);
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

		private static MinimumShouldMatch ReadMinimumShouldMatch(ref Utf8JsonReader reader)
		{
			if (reader.TokenType == JsonTokenType.Number)
				return reader.GetInt32();
			if (reader.TokenType == JsonTokenType.String)
				return reader.GetString();
			return null;
		}

		private static void WriteMinimumShouldMatch(Utf8JsonWriter writer, MinimumShouldMatch msm)
		{
			if (msm == null)
			{
				writer.WriteNullValue();
				return;
			}
			var value = msm.ToString();
			if (int.TryParse(value, out var intVal))
				writer.WriteNumberValue(intVal);
			else
				writer.WriteStringValue(value);
		}
	}
}
