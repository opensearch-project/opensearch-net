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
	/// Converter for <see cref="IMoreLikeThisQuery"/>.
	/// JSON format:
	/// <code>
	/// {"fields": [...], "like": [...], "unlike": [...], "min_term_freq": 1, ...}
	/// </code>
	/// The "like" and "unlike" arrays contain <see cref="Like"/> elements.
	/// </summary>
	internal sealed class MoreLikeThisQueryConverter : JsonConverter<IMoreLikeThisQuery>
	{
		private static readonly LikeConverter LikeConv = new LikeConverter();

		public override IMoreLikeThisQuery Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null)
				return null;

			if (reader.TokenType != JsonTokenType.StartObject)
				throw new JsonException($"Expected StartObject for IMoreLikeThisQuery but got {reader.TokenType}");

			var query = new MoreLikeThisQuery();

			while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
			{
				if (reader.TokenType != JsonTokenType.PropertyName) continue;
				var prop = reader.GetString();
				reader.Read();

				switch (prop)
				{
					case "fields":
						query.Fields = JsonSerializer.Deserialize<Fields>(ref reader, options);
						break;
					case "like":
						query.Like = ReadLikeArray(ref reader, options);
						break;
					case "unlike":
						query.Unlike = ReadLikeArray(ref reader, options);
						break;
					case "min_term_freq":
						query.MinTermFrequency = reader.GetInt32();
						break;
					case "max_query_terms":
						query.MaxQueryTerms = reader.GetInt32();
						break;
					case "min_doc_freq":
						query.MinDocumentFrequency = reader.GetInt32();
						break;
					case "max_doc_freq":
						query.MaxDocumentFrequency = reader.GetInt32();
						break;
					case "min_word_length":
						query.MinWordLength = reader.GetInt32();
						break;
					case "max_word_length":
						query.MaxWordLength = reader.GetInt32();
						break;
					case "boost_terms":
						query.BoostTerms = reader.GetDouble();
						break;
					case "analyzer":
						query.Analyzer = reader.GetString();
						break;
					case "include":
						query.Include = reader.GetBoolean();
						break;
					case "minimum_should_match":
						query.MinimumShouldMatch = JsonSerializer.Deserialize<MinimumShouldMatch>(ref reader, options);
						break;
					case "stop_words":
						query.StopWords = JsonSerializer.Deserialize<StopWords>(ref reader, options);
						break;
					case "per_field_analyzer":
						query.PerFieldAnalyzer = JsonSerializer.Deserialize<IPerFieldAnalyzer>(ref reader, options);
						break;
					case "routing":
						query.Routing = reader.GetString();
						break;
					case "version":
						query.Version = reader.GetInt64();
						break;
					case "version_type":
						query.VersionType = JsonSerializer.Deserialize<OpenSearch.Net.VersionType?>(ref reader, options);
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

		public override void Write(Utf8JsonWriter writer, IMoreLikeThisQuery value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			writer.WriteStartObject();

			if (value.Fields != null)
			{
				writer.WritePropertyName("fields");
				JsonSerializer.Serialize(writer, value.Fields, options);
			}

			if (value.Like != null)
			{
				writer.WritePropertyName("like");
				WriteLikeArray(writer, value.Like, options);
			}

			if (value.Unlike != null)
			{
				writer.WritePropertyName("unlike");
				WriteLikeArray(writer, value.Unlike, options);
			}

			if (value.MinTermFrequency.HasValue)
				writer.WriteNumber("min_term_freq", value.MinTermFrequency.Value);

			if (value.MaxQueryTerms.HasValue)
				writer.WriteNumber("max_query_terms", value.MaxQueryTerms.Value);

			if (value.MinDocumentFrequency.HasValue)
				writer.WriteNumber("min_doc_freq", value.MinDocumentFrequency.Value);

			if (value.MaxDocumentFrequency.HasValue)
				writer.WriteNumber("max_doc_freq", value.MaxDocumentFrequency.Value);

			if (value.MinWordLength.HasValue)
				writer.WriteNumber("min_word_length", value.MinWordLength.Value);

			if (value.MaxWordLength.HasValue)
				writer.WriteNumber("max_word_length", value.MaxWordLength.Value);

			if (value.BoostTerms.HasValue)
				writer.WriteNumber("boost_terms", value.BoostTerms.Value);

			if (!string.IsNullOrEmpty(value.Analyzer))
				writer.WriteString("analyzer", value.Analyzer);

			if (value.Include.HasValue)
			{
				writer.WritePropertyName("include");
				writer.WriteBooleanValue(value.Include.Value);
			}

			if (value.MinimumShouldMatch != null)
			{
				writer.WritePropertyName("minimum_should_match");
				JsonSerializer.Serialize(writer, value.MinimumShouldMatch, options);
			}

			if (value.StopWords != null)
			{
				writer.WritePropertyName("stop_words");
				JsonSerializer.Serialize(writer, value.StopWords, options);
			}

			if (value.PerFieldAnalyzer != null)
			{
				writer.WritePropertyName("per_field_analyzer");
				JsonSerializer.Serialize(writer, value.PerFieldAnalyzer, options);
			}

			if (value.Routing != null)
				writer.WriteString("routing", value.Routing.ToString());

			if (value.Version.HasValue)
				writer.WriteNumber("version", value.Version.Value);

			if (value.VersionType.HasValue)
			{
				writer.WritePropertyName("version_type");
				JsonSerializer.Serialize(writer, value.VersionType, options);
			}

			if (value.Boost.HasValue)
				writer.WriteNumber("boost", value.Boost.Value);

			if (!string.IsNullOrEmpty(value.Name))
				writer.WriteString("_name", value.Name);

			writer.WriteEndObject();
		}

		private List<Like> ReadLikeArray(ref Utf8JsonReader reader, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.StartArray)
				throw new JsonException("Expected StartArray for like/unlike");

			var list = new List<Like>();
			while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
			{
				var like = LikeConv.Read(ref reader, typeof(Like), options);
				if (like != null)
					list.Add(like);
			}
			return list;
		}

		private void WriteLikeArray(Utf8JsonWriter writer, IEnumerable<Like> likes, JsonSerializerOptions options)
		{
			writer.WriteStartArray();
			foreach (var like in likes)
				LikeConv.Write(writer, like, options);
			writer.WriteEndArray();
		}
	}
}
