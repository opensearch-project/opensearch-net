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

namespace OpenSearch.Client.Aggregations.SystemTextJsonConverters
{
	/// <summary>
	/// Converter for <see cref="AggregationContainer"/> and <see cref="IAggregationContainer"/>.
	/// AggregationContainer is a polymorphic type - in JSON it looks like:
	/// <code>
	/// {"terms": {"field": "status"}, "aggs": {...sub-aggs...}, "meta": {...}}
	/// {"avg": {"field": "price"}}
	/// {"date_histogram": {"field": "@timestamp", "calendar_interval": "month"}, "aggs": {...}}
	/// </code>
	/// Each AggregationContainer holds exactly ONE aggregation type, plus optional sub-aggregations and metadata.
	/// The JSON property name is the type discriminator.
	/// </summary>
	internal sealed class AggregationContainerConverter : JsonConverter<AggregationContainer>
	{
		private readonly IConnectionSettingsValues _settings;

		public AggregationContainerConverter(IConnectionSettingsValues settings) => _settings = settings;

		/// <summary>
		/// All known aggregation type discriminator names.
		/// </summary>
		private static readonly HashSet<string> KnownAggregationTypes = new HashSet<string>(StringComparer.Ordinal)
		{
			// Metric
			"avg", "cardinality", "extended_stats", "geo_bounds", "geo_centroid", "geo_line",
			"max", "min", "percentile_ranks", "percentiles", "scripted_metric", "stats",
			"sum", "top_hits", "value_count", "weighted_avg", "median_absolute_deviation",
			// Bucket
			"adjacency_matrix", "auto_date_histogram", "children", "composite", "date_histogram",
			"date_range", "diversified_sampler", "filter", "filters", "geo_distance",
			"geohash_grid", "geotile_grid", "global", "histogram", "ip_range", "missing",
			"multi_terms", "nested", "parent", "range", "rare_terms", "reverse_nested",
			"sampler", "significant_terms", "significant_text", "terms", "variable_width_histogram",
			// Pipeline
			"avg_bucket", "bucket_script", "bucket_selector", "bucket_sort", "cumulative_sum",
			"derivative", "extended_stats_bucket", "max_bucket", "min_bucket", "moving_avg",
			"moving_fn", "percentiles_bucket", "serial_diff", "stats_bucket", "sum_bucket",
			// Matrix
			"matrix_stats",
		};

		/// <summary>
		/// Known non-aggregation-type properties that appear at the same level.
		/// </summary>
		private static readonly HashSet<string> ReservedProperties = new HashSet<string>(StringComparer.Ordinal)
		{
			"aggs", "aggregations", "meta"
		};

		public override AggregationContainer Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null)
				return null;

			if (reader.TokenType != JsonTokenType.StartObject)
				throw new JsonException($"Expected StartObject for AggregationContainer but got {reader.TokenType}");

			var container = new AggregationContainer();
			IAggregationContainer c = container;

			using var doc = JsonDocument.ParseValue(ref reader);
			var root = doc.RootElement;

			foreach (var property in root.EnumerateObject())
			{
				switch (property.Name)
				{
					case "aggs":
					case "aggregations":
						c.Aggregations = DeserializeSubAggregations(property.Value, options);
						break;
					case "meta":
						c.Meta = DeserializeMeta(property.Value);
						break;
					default:
						if (KnownAggregationTypes.Contains(property.Name))
						{
							// For now, skip individual aggregation type deserialization.
							// Dedicated converters will be added per aggregation type.
						}
						break;
				}
			}

			return container;
		}

		public override void Write(Utf8JsonWriter writer, AggregationContainer value, JsonSerializerOptions options)
		{
			WriteContainer(writer, value, options);
		}

		internal static void WriteContainer(Utf8JsonWriter writer, IAggregationContainer value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			writer.WriteStartObject();
			WriteAggregationType(writer, value, options);

			if (value.Meta != null && value.Meta.Count > 0)
			{
				writer.WritePropertyName("meta");
				JsonSerializer.Serialize(writer, value.Meta, options);
			}

			if (value.Aggregations != null && value.Aggregations.Count > 0)
			{
				writer.WritePropertyName("aggs");
				JsonSerializer.Serialize(writer, value.Aggregations, options);
			}

			writer.WriteEndObject();
		}

		private static void WriteAggregationType(Utf8JsonWriter writer, IAggregationContainer value, JsonSerializerOptions options)
		{
			if (value.Average != null) WriteNamedAgg(writer, "avg", value.Average, options);
			else if (value.Cardinality != null) WriteNamedAgg(writer, "cardinality", value.Cardinality, options);
			else if (value.ExtendedStats != null) WriteNamedAgg(writer, "extended_stats", value.ExtendedStats, options);
			else if (value.GeoBounds != null) WriteNamedAgg(writer, "geo_bounds", value.GeoBounds, options);
			else if (value.GeoCentroid != null) WriteNamedAgg(writer, "geo_centroid", value.GeoCentroid, options);
			else if (value.GeoLine != null) WriteNamedAgg(writer, "geo_line", value.GeoLine, options);
			else if (value.Max != null) WriteNamedAgg(writer, "max", value.Max, options);
			else if (value.Min != null) WriteNamedAgg(writer, "min", value.Min, options);
			else if (value.PercentileRanks != null) WriteNamedAgg(writer, "percentile_ranks", value.PercentileRanks, options);
			else if (value.Percentiles != null) WriteNamedAgg(writer, "percentiles", value.Percentiles, options);
			else if (value.ScriptedMetric != null) WriteNamedAgg(writer, "scripted_metric", value.ScriptedMetric, options);
			else if (value.Stats != null) WriteNamedAgg(writer, "stats", value.Stats, options);
			else if (value.Sum != null) WriteNamedAgg(writer, "sum", value.Sum, options);
			else if (value.TopHits != null) WriteNamedAgg(writer, "top_hits", value.TopHits, options);
			else if (value.ValueCount != null) WriteNamedAgg(writer, "value_count", value.ValueCount, options);
			else if (value.WeightedAverage != null) WriteNamedAgg(writer, "weighted_avg", value.WeightedAverage, options);
			else if (value.MedianAbsoluteDeviation != null) WriteNamedAgg(writer, "median_absolute_deviation", value.MedianAbsoluteDeviation, options);
			else if (value.AdjacencyMatrix != null) WriteNamedAgg(writer, "adjacency_matrix", value.AdjacencyMatrix, options);
			else if (value.AutoDateHistogram != null) WriteNamedAgg(writer, "auto_date_histogram", value.AutoDateHistogram, options);
			else if (value.Children != null) WriteNamedAgg(writer, "children", value.Children, options);
			else if (value.Composite != null) WriteNamedAgg(writer, "composite", value.Composite, options);
			else if (value.DateHistogram != null) WriteNamedAgg(writer, "date_histogram", value.DateHistogram, options);
			else if (value.DateRange != null) WriteNamedAgg(writer, "date_range", value.DateRange, options);
			else if (value.DiversifiedSampler != null) WriteNamedAgg(writer, "diversified_sampler", value.DiversifiedSampler, options);
			else if (value.Filter != null) WriteNamedAgg(writer, "filter", value.Filter, options);
			else if (value.Filters != null) WriteNamedAgg(writer, "filters", value.Filters, options);
			else if (value.GeoDistance != null) WriteNamedAgg(writer, "geo_distance", value.GeoDistance, options);
			else if (value.GeoHash != null) WriteNamedAgg(writer, "geohash_grid", value.GeoHash, options);
			else if (value.GeoTile != null) WriteNamedAgg(writer, "geotile_grid", value.GeoTile, options);
			else if (value.Global != null) WriteNamedAgg(writer, "global", value.Global, options);
			else if (value.Histogram != null) WriteNamedAgg(writer, "histogram", value.Histogram, options);
			else if (value.IpRange != null) WriteNamedAgg(writer, "ip_range", value.IpRange, options);
			else if (value.Missing != null) WriteNamedAgg(writer, "missing", value.Missing, options);
			else if (value.MultiTerms != null) WriteNamedAgg(writer, "multi_terms", value.MultiTerms, options);
			else if (value.Nested != null) WriteNamedAgg(writer, "nested", value.Nested, options);
			else if (value.Parent != null) WriteNamedAgg(writer, "parent", value.Parent, options);
			else if (value.Range != null) WriteNamedAgg(writer, "range", value.Range, options);
			else if (value.RareTerms != null) WriteNamedAgg(writer, "rare_terms", value.RareTerms, options);
			else if (value.ReverseNested != null) WriteNamedAgg(writer, "reverse_nested", value.ReverseNested, options);
			else if (value.Sampler != null) WriteNamedAgg(writer, "sampler", value.Sampler, options);
			else if (value.SignificantTerms != null) WriteNamedAgg(writer, "significant_terms", value.SignificantTerms, options);
			else if (value.SignificantText != null) WriteNamedAgg(writer, "significant_text", value.SignificantText, options);
			else if (value.Terms != null) WriteNamedAgg(writer, "terms", value.Terms, options);
			else if (value.VariableWidthHistogram != null) WriteNamedAgg(writer, "variable_width_histogram", value.VariableWidthHistogram, options);
			else if (value.AverageBucket != null) WriteNamedAgg(writer, "avg_bucket", value.AverageBucket, options);
			else if (value.BucketScript != null) WriteNamedAgg(writer, "bucket_script", value.BucketScript, options);
			else if (value.BucketSelector != null) WriteNamedAgg(writer, "bucket_selector", value.BucketSelector, options);
			else if (value.BucketSort != null) WriteNamedAgg(writer, "bucket_sort", value.BucketSort, options);
			else if (value.CumulativeSum != null) WriteNamedAgg(writer, "cumulative_sum", value.CumulativeSum, options);
			else if (value.Derivative != null) WriteNamedAgg(writer, "derivative", value.Derivative, options);
			else if (value.ExtendedStatsBucket != null) WriteNamedAgg(writer, "extended_stats_bucket", value.ExtendedStatsBucket, options);
			else if (value.MaxBucket != null) WriteNamedAgg(writer, "max_bucket", value.MaxBucket, options);
			else if (value.MinBucket != null) WriteNamedAgg(writer, "min_bucket", value.MinBucket, options);
			else if (value.MovingAverage != null) WriteNamedAgg(writer, "moving_avg", value.MovingAverage, options);
			else if (value.MovingFunction != null) WriteNamedAgg(writer, "moving_fn", value.MovingFunction, options);
			else if (value.PercentilesBucket != null) WriteNamedAgg(writer, "percentiles_bucket", value.PercentilesBucket, options);
			else if (value.SerialDifferencing != null) WriteNamedAgg(writer, "serial_diff", value.SerialDifferencing, options);
			else if (value.StatsBucket != null) WriteNamedAgg(writer, "stats_bucket", value.StatsBucket, options);
			else if (value.SumBucket != null) WriteNamedAgg(writer, "sum_bucket", value.SumBucket, options);
			else if (value.MatrixStats != null) WriteNamedAgg(writer, "matrix_stats", value.MatrixStats, options);
		}

		private static void WriteNamedAgg(Utf8JsonWriter writer, string name, object aggregation, JsonSerializerOptions options)
		{
			writer.WritePropertyName(name);
			JsonSerializer.Serialize(writer, aggregation, aggregation.GetType(), options);
		}

		private static AggregationDictionary DeserializeSubAggregations(JsonElement element, JsonSerializerOptions options)
		{
			if (element.ValueKind != JsonValueKind.Object)
				return null;

			var raw = element.GetRawText();
			return JsonSerializer.Deserialize<AggregationDictionary>(raw, options);
		}

		private static IDictionary<string, object> DeserializeMeta(JsonElement element)
		{
			if (element.ValueKind != JsonValueKind.Object)
				return null;

			var meta = new Dictionary<string, object>();
			foreach (var prop in element.EnumerateObject())
			{
				meta[prop.Name] = GetJsonElementValue(prop.Value);
			}
			return meta;
		}

		internal static object GetJsonElementValue(JsonElement element)
		{
			switch (element.ValueKind)
			{
				case JsonValueKind.String: return element.GetString();
				case JsonValueKind.Number:
					if (element.TryGetInt64(out var l)) return l;
					return element.GetDouble();
				case JsonValueKind.True: return true;
				case JsonValueKind.False: return false;
				case JsonValueKind.Null: return null;
				case JsonValueKind.Object:
					var dict = new Dictionary<string, object>();
					foreach (var prop in element.EnumerateObject())
						dict[prop.Name] = GetJsonElementValue(prop.Value);
					return dict;
				case JsonValueKind.Array:
					var list = new List<object>();
					foreach (var item in element.EnumerateArray())
						list.Add(GetJsonElementValue(item));
					return list;
				default: return element.GetRawText();
			}
		}
	}

	/// <summary>
	/// Converter for <see cref="IAggregationContainer"/> interface.
	/// Delegates to <see cref="AggregationContainerConverter"/>.
	/// </summary>
	internal sealed class AggregationContainerInterfaceConverter : JsonConverter<IAggregationContainer>
	{
		private readonly AggregationContainerConverter _containerConverter;

		public AggregationContainerInterfaceConverter(IConnectionSettingsValues settings)
		{
			_containerConverter = new AggregationContainerConverter(settings);
		}

		public override IAggregationContainer Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			return _containerConverter.Read(ref reader, typeof(AggregationContainer), options);
		}

		public override void Write(Utf8JsonWriter writer, IAggregationContainer value, JsonSerializerOptions options)
		{
			AggregationContainerConverter.WriteContainer(writer, value, options);
		}
	}
}
