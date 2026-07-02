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

namespace OpenSearch.Client.QueryDsl.SystemTextJsonConverters
{
	/// <summary>
	/// Converter for <see cref="QueryContainer"/> and <see cref="IQueryContainer"/>.
	/// QueryContainer is a polymorphic type - in JSON it looks like:
	/// <code>
	/// {"match": {"title": {"query": "search text"}}}
	/// {"bool": {"must": [...], "should": [...]}}
	/// {"term": {"status": {"value": "published"}}}
	/// </code>
	/// Each QueryContainer holds exactly ONE query type. The JSON property name is the discriminator.
	/// </summary>
	internal sealed class QueryContainerConverter : JsonConverter<QueryContainer>
	{
		private static readonly BoolQueryConverter BoolConverter = new BoolQueryConverter();
		private static readonly MatchQueryConverter MatchConverter = new MatchQueryConverter();
		private static readonly TermQueryConverter TermConverter = new TermQueryConverter();
		private static readonly RangeQueryConverter RangeConverter = new RangeQueryConverter();

		public override QueryContainer Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null)
				return null;

			if (reader.TokenType != JsonTokenType.StartObject)
				throw new JsonException($"Expected StartObject for QueryContainer but got {reader.TokenType}");

			reader.Read(); // Move past StartObject

			if (reader.TokenType == JsonTokenType.EndObject)
				return null; // Empty object

			if (reader.TokenType != JsonTokenType.PropertyName)
				throw new JsonException("Expected property name (query type discriminator)");

			var queryType = reader.GetString();
			reader.Read(); // Move to value

			var container = new QueryContainer();
			ReadQueryIntoContainer(ref reader, queryType, container, options);

			// Read the EndObject of the outer container
			reader.Read();

			return container;
		}

		public override void Write(Utf8JsonWriter writer, QueryContainer value, JsonSerializerOptions options)
		{
			WriteContainer(writer, value, options);
		}

		internal static void WriteContainer(Utf8JsonWriter writer, IQueryContainer value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			// Handle raw query
			if (value.RawQuery?.Raw != null && value.IsWritable)
			{
				using var doc = JsonDocument.Parse(value.RawQuery.Raw);
				doc.RootElement.WriteTo(writer);
				return;
			}

			writer.WriteStartObject();

			if (value.Bool != null)
			{
				writer.WritePropertyName("bool");
				BoolConverter.Write(writer, value.Bool, options);
			}
			else if (value.Match != null)
			{
				writer.WritePropertyName("match");
				MatchConverter.Write(writer, value.Match, options);
			}
			else if (value.Term != null)
			{
				writer.WritePropertyName("term");
				TermConverter.Write(writer, value.Term, options);
			}
			else if (value.Range != null)
			{
				writer.WritePropertyName("range");
				RangeConverter.Write(writer, value.Range, options);
			}
			else if (value.MatchAll != null)
			{
				WriteMatchAll(writer, value.MatchAll);
			}
			else if (value.MatchNone != null)
			{
				WriteMatchNone(writer, value.MatchNone);
			}
			else if (value.Nested != null)
			{
				writer.WritePropertyName("nested");
				WriteGenericQuery(writer, value.Nested, options);
			}
			else if (value.Exists != null)
			{
				writer.WritePropertyName("exists");
				WriteGenericQuery(writer, value.Exists, options);
			}
			else if (value.Terms != null)
			{
				writer.WritePropertyName("terms");
				WriteGenericQuery(writer, value.Terms, options);
			}
			else if (value.Ids != null)
			{
				writer.WritePropertyName("ids");
				WriteGenericQuery(writer, value.Ids, options);
			}
			else if (value.Wildcard != null)
			{
				writer.WritePropertyName("wildcard");
				WriteGenericQuery(writer, value.Wildcard, options);
			}
			else if (value.Prefix != null)
			{
				writer.WritePropertyName("prefix");
				WriteGenericQuery(writer, value.Prefix, options);
			}
			else if (value.Fuzzy != null)
			{
				writer.WritePropertyName("fuzzy");
				WriteGenericQuery(writer, value.Fuzzy, options);
			}
			else if (value.Regexp != null)
			{
				writer.WritePropertyName("regexp");
				WriteGenericQuery(writer, value.Regexp, options);
			}
			else if (value.QueryString != null)
			{
				writer.WritePropertyName("query_string");
				WriteGenericQuery(writer, value.QueryString, options);
			}
			else if (value.SimpleQueryString != null)
			{
				writer.WritePropertyName("simple_query_string");
				WriteGenericQuery(writer, value.SimpleQueryString, options);
			}
			else if (value.MultiMatch != null)
			{
				writer.WritePropertyName("multi_match");
				WriteGenericQuery(writer, value.MultiMatch, options);
			}
			else if (value.MatchPhrase != null)
			{
				writer.WritePropertyName("match_phrase");
				WriteGenericQuery(writer, value.MatchPhrase, options);
			}
			else if (value.MatchPhrasePrefix != null)
			{
				writer.WritePropertyName("match_phrase_prefix");
				WriteGenericQuery(writer, value.MatchPhrasePrefix, options);
			}
			else if (value.MatchBoolPrefix != null)
			{
				writer.WritePropertyName("match_bool_prefix");
				WriteGenericQuery(writer, value.MatchBoolPrefix, options);
			}
			else if (value.DisMax != null)
			{
				writer.WritePropertyName("dis_max");
				WriteGenericQuery(writer, value.DisMax, options);
			}
			else if (value.ConstantScore != null)
			{
				writer.WritePropertyName("constant_score");
				WriteGenericQuery(writer, value.ConstantScore, options);
			}
			else if (value.Boosting != null)
			{
				writer.WritePropertyName("boosting");
				WriteGenericQuery(writer, value.Boosting, options);
			}
			else if (value.FunctionScore != null)
			{
				writer.WritePropertyName("function_score");
				WriteGenericQuery(writer, value.FunctionScore, options);
			}
			else if (value.Script != null)
			{
				writer.WritePropertyName("script");
				WriteGenericQuery(writer, value.Script, options);
			}
			else if (value.ScriptScore != null)
			{
				writer.WritePropertyName("script_score");
				WriteGenericQuery(writer, value.ScriptScore, options);
			}
			else if (value.GeoDistance != null)
			{
				writer.WritePropertyName("geo_distance");
				WriteGenericQuery(writer, value.GeoDistance, options);
			}
			else if (value.GeoBoundingBox != null)
			{
				writer.WritePropertyName("geo_bounding_box");
				WriteGenericQuery(writer, value.GeoBoundingBox, options);
			}
			else if (value.GeoShape != null)
			{
				writer.WritePropertyName("geo_shape");
				WriteGenericQuery(writer, value.GeoShape, options);
			}
			else if (value.HasChild != null)
			{
				writer.WritePropertyName("has_child");
				WriteGenericQuery(writer, value.HasChild, options);
			}
			else if (value.HasParent != null)
			{
				writer.WritePropertyName("has_parent");
				WriteGenericQuery(writer, value.HasParent, options);
			}
			else if (value.ParentId != null)
			{
				writer.WritePropertyName("parent_id");
				WriteGenericQuery(writer, value.ParentId, options);
			}
			else if (value.Percolate != null)
			{
				writer.WritePropertyName("percolate");
				WriteGenericQuery(writer, value.Percolate, options);
			}
			else if (value.MoreLikeThis != null)
			{
				writer.WritePropertyName("more_like_this");
				WriteGenericQuery(writer, value.MoreLikeThis, options);
			}
			else if (value.RankFeature != null)
			{
				writer.WritePropertyName("rank_feature");
				WriteGenericQuery(writer, value.RankFeature, options);
			}
			else if (value.DistanceFeature != null)
			{
				writer.WritePropertyName("distance_feature");
				WriteGenericQuery(writer, value.DistanceFeature, options);
			}
			else if (value.Knn != null)
			{
				writer.WritePropertyName("knn");
				WriteGenericQuery(writer, value.Knn, options);
			}
			else if (value.Neural != null)
			{
				writer.WritePropertyName("neural");
				WriteGenericQuery(writer, value.Neural, options);
			}
			else if (value.Hybrid != null)
			{
				writer.WritePropertyName("hybrid");
				WriteGenericQuery(writer, value.Hybrid, options);
			}

			writer.WriteEndObject();
		}

		private static void WriteMatchAll(Utf8JsonWriter writer, IMatchAllQuery matchAll)
		{
			writer.WritePropertyName("match_all");
			writer.WriteStartObject();
			if (matchAll.Boost.HasValue)
			{
				writer.WritePropertyName("boost");
				writer.WriteNumberValue(matchAll.Boost.Value);
			}
			if (!string.IsNullOrEmpty(matchAll.Name))
			{
				writer.WritePropertyName("_name");
				writer.WriteStringValue(matchAll.Name);
			}
			writer.WriteEndObject();
		}

		private static void WriteMatchNone(Utf8JsonWriter writer, IMatchNoneQuery matchNone)
		{
			writer.WritePropertyName("match_none");
			writer.WriteStartObject();
			if (!string.IsNullOrEmpty(matchNone.Name))
			{
				writer.WritePropertyName("_name");
				writer.WriteStringValue(matchNone.Name);
			}
			writer.WriteEndObject();
		}

		/// <summary>
		/// Placeholder for query types that don't yet have dedicated converters.
		/// Falls back to System.Text.Json default serialization.
		/// </summary>
		private static void WriteGenericQuery(Utf8JsonWriter writer, object query, JsonSerializerOptions options)
		{
			JsonSerializer.Serialize(writer, query, query.GetType(), options);
		}

		private void ReadQueryIntoContainer(ref Utf8JsonReader reader, string queryType, QueryContainer container, JsonSerializerOptions options)
		{
			IQueryContainer qc = container;

			switch (queryType)
			{
				case "bool":
					qc.Bool = BoolConverter.Read(ref reader, typeof(IBoolQuery), options);
					break;
				case "match":
					qc.Match = MatchConverter.Read(ref reader, typeof(IMatchQuery), options);
					break;
				case "term":
					qc.Term = TermConverter.Read(ref reader, typeof(ITermQuery), options);
					break;
				case "range":
					qc.Range = RangeConverter.Read(ref reader, typeof(IRangeQuery), options);
					break;
				case "match_all":
					qc.MatchAll = ReadMatchAll(ref reader);
					break;
				case "match_none":
					qc.MatchNone = ReadMatchNone(ref reader);
					break;
				default:
					// For query types without dedicated converters, skip the value
					reader.Skip();
					break;
			}
		}

		private static IMatchAllQuery ReadMatchAll(ref Utf8JsonReader reader)
		{
			if (reader.TokenType == JsonTokenType.Null)
				return new MatchAllQuery();

			if (reader.TokenType != JsonTokenType.StartObject)
				throw new JsonException("Expected StartObject for match_all query");

			var query = new MatchAllQuery();
			while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
			{
				if (reader.TokenType != JsonTokenType.PropertyName)
					continue;
				var prop = reader.GetString();
				reader.Read();
				switch (prop)
				{
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

		private static IMatchNoneQuery ReadMatchNone(ref Utf8JsonReader reader)
		{
			if (reader.TokenType == JsonTokenType.Null)
				return new MatchNoneQuery();

			if (reader.TokenType != JsonTokenType.StartObject)
				throw new JsonException("Expected StartObject for match_none query");

			var query = new MatchNoneQuery();
			while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
			{
				if (reader.TokenType != JsonTokenType.PropertyName)
					continue;
				var prop = reader.GetString();
				reader.Read();
				switch (prop)
				{
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
	}

	/// <summary>
	/// Converter for <see cref="IQueryContainer"/> interface.
	/// Delegates to <see cref="QueryContainerConverter"/>.
	/// </summary>
	internal sealed class QueryContainerInterfaceConverter : JsonConverter<IQueryContainer>
	{
		private static readonly QueryContainerConverter ContainerConverter = new QueryContainerConverter();

		public override IQueryContainer Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			return ContainerConverter.Read(ref reader, typeof(QueryContainer), options);
		}

		public override void Write(Utf8JsonWriter writer, IQueryContainer value, JsonSerializerOptions options)
		{
			QueryContainerConverter.WriteContainer(writer, value, options);
		}
	}
}
