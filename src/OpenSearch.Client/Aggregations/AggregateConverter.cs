/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenSearch.Client
{
	/// <summary>
	/// A read-only <see cref="System.Text.Json"/> converter for the polymorphic <see cref="IAggregate"/>
	/// response type, replacing (a first slice of) the vendored Utf8Json <c>AggregateFormatter</c>
	/// heuristic reader as part of #388. It buffers the aggregate object and dispatches on which
	/// properties are present — <c>value</c> → metric value, <c>count</c>+min/max/avg/sum → stats,
	/// <c>buckets</c> → multi-bucket (keyed buckets with recursive sub-aggregations), <c>doc_count</c>
	/// → single-bucket. Sub-aggregations recurse through this converter.
	/// </summary>
	internal sealed class AggregateConverter : JsonConverter<IAggregate>
	{
		private static readonly HashSet<string> ReservedBucketKeys = new(StringComparer.Ordinal)
		{
			"key", "key_as_string", "doc_count", "from", "to", "from_as_string", "to_as_string",
			"doc_count_error_upper_bound", "score", "bg_count",
		};

		public override void Write(Utf8JsonWriter writer, IAggregate value, JsonSerializerOptions options) =>
			throw new NotSupportedException("IAggregate is a response type and is not serialized.");

		public override IAggregate Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null) return null;

			using var document = JsonDocument.ParseValue(ref reader);
			return ReadAggregate(document.RootElement, options);
		}

		private static IAggregate ReadAggregate(JsonElement root, JsonSerializerOptions options)
		{
			if (root.ValueKind != JsonValueKind.Object) return null;

			IReadOnlyDictionary<string, object> meta = null;
			if (root.TryGetProperty("meta", out var metaElement) && metaElement.ValueKind == JsonValueKind.Object)
				meta = metaElement.Deserialize<Dictionary<string, object>>(options);

			if (root.TryGetProperty("values", out var valuesElement))
				return ReadPercentiles(valuesElement, meta);

			if (root.TryGetProperty("value", out var valueElement))
			{
				var aggregate = new ValueAggregate
				{
					Value = valueElement.ValueKind == JsonValueKind.Null ? null : valueElement.GetDouble(),
					Meta = meta,
				};
				if (root.TryGetProperty("value_as_string", out var valueAsString))
					aggregate.ValueAsString = valueAsString.GetString();
				return aggregate;
			}

			if (root.TryGetProperty("buckets", out var buckets))
				return ReadMultiBucket(root, buckets, meta, options);

			if (root.TryGetProperty("count", out var countElement)
				&& (root.TryGetProperty("min", out _) || root.TryGetProperty("max", out _) || root.TryGetProperty("avg", out _)))
			{
				var count = countElement.ValueKind == JsonValueKind.Null ? 0 : countElement.GetInt64();
				var min = GetNullableDouble(root, "min");
				var max = GetNullableDouble(root, "max");
				var average = GetNullableDouble(root, "avg");
				var sum = GetNullableDouble(root, "sum") ?? 0;

				// Extended stats carry additional dispersion fields.
				if (root.TryGetProperty("sum_of_squares", out _) || root.TryGetProperty("std_deviation", out _)
					|| root.TryGetProperty("variance", out _))
				{
					var extended = new ExtendedStatsAggregate
					{
						Count = count, Min = min, Max = max, Average = average, Sum = sum, Meta = meta,
						SumOfSquares = GetNullableDouble(root, "sum_of_squares"),
						Variance = GetNullableDouble(root, "variance"),
						VariancePopulation = GetNullableDouble(root, "variance_population"),
						VarianceSampling = GetNullableDouble(root, "variance_sampling"),
						StdDeviation = GetNullableDouble(root, "std_deviation"),
						StdDeviationPopulation = GetNullableDouble(root, "std_deviation_population"),
						StdDeviationSampling = GetNullableDouble(root, "std_deviation_sampling"),
					};
					if (root.TryGetProperty("std_deviation_bounds", out var bounds) && bounds.ValueKind == JsonValueKind.Object)
						extended.StdDeviationBounds = bounds.Deserialize<StandardDeviationBounds>(options);
					return extended;
				}

				return new StatsAggregate { Count = count, Min = min, Max = max, Average = average, Sum = sum, Meta = meta };
			}

			if (root.TryGetProperty("doc_count", out var docCountElement))
			{
				var subAggregates = ReadSubAggregates(root, options);
				return new SingleBucketAggregate(subAggregates) { DocCount = docCountElement.GetInt64(), Meta = meta };
			}

			return null;
		}

		private static IAggregate ReadMultiBucket(JsonElement root, JsonElement buckets, IReadOnlyDictionary<string, object> meta, JsonSerializerOptions options)
		{
			var aggregate = new BucketAggregate { Meta = meta };
			if (root.TryGetProperty("doc_count_error_upper_bound", out var dce) && dce.ValueKind == JsonValueKind.Number)
				aggregate.DocCountErrorUpperBound = dce.GetInt64();
			if (root.TryGetProperty("sum_other_doc_count", out var sod) && sod.ValueKind == JsonValueKind.Number)
				aggregate.SumOtherDocCount = sod.GetInt64();
			// auto_date_histogram reports the chosen interval alongside its buckets.
			if (root.TryGetProperty("interval", out var interval) && interval.ValueKind != JsonValueKind.Null)
				aggregate.AutoInterval = interval.Deserialize<DateMathTime>(options);

			var items = new List<IBucket>();
			if (buckets.ValueKind == JsonValueKind.Array)
			{
				foreach (var bucketElement in buckets.EnumerateArray())
					items.Add(ReadKeyedBucket(bucketElement, options));
			}
			else if (buckets.ValueKind == JsonValueKind.Object)
			{
				// Named (keyed) buckets — e.g. the filters aggregation — are a JSON object whose property
				// names are the bucket keys (which may themselves contain '#').
				foreach (var member in buckets.EnumerateObject())
				{
					var bucket = ReadKeyedBucket(member.Value, options);
					if (bucket is KeyedBucket<object> keyed && keyed.Key == null)
						keyed.Key = member.Name;
					items.Add(bucket);
				}
			}
			aggregate.Items = items;
			return aggregate;
		}

		private static IBucket ReadKeyedBucket(JsonElement element, JsonSerializerOptions options)
		{
			// Range buckets carry from/to bounds.
			if (element.TryGetProperty("from", out _) || element.TryGetProperty("to", out _))
				return ReadRangeBucket(element, options);

			// The first property distinguishes a date-histogram bucket (key_as_string first) from a
			// terms/keyed bucket (key first), mirroring the Utf8Json ReadBucket dispatch.
			var firstProperty = FirstPropertyName(element);
			if (firstProperty == "key_as_string")
			{
				var dateBucket = new DateHistogramBucket(ReadSubAggregates(element, options));
				if (element.TryGetProperty("key", out var dhKey) && dhKey.ValueKind == JsonValueKind.Number)
					dateBucket.Key = dhKey.GetDouble();
				if (element.TryGetProperty("key_as_string", out var dhKeyAs))
					dateBucket.KeyAsString = dhKeyAs.GetString();
				if (element.TryGetProperty("doc_count", out var dhCount))
					dateBucket.DocCount = dhCount.GetInt64();
				return dateBucket;
			}

			object key = null;
			if (element.TryGetProperty("key", out var keyElement))
			{
				key = keyElement.ValueKind switch
				{
					JsonValueKind.String => keyElement.GetString(),
					JsonValueKind.Number => keyElement.TryGetInt64(out var l) ? l : keyElement.GetDouble(),
					_ => key,
				};
			}

			var bucket = new KeyedBucket<object>(ReadSubAggregates(element, options)) { Key = key };
			if (element.TryGetProperty("key_as_string", out var keyAsString))
				bucket.KeyAsString = keyAsString.GetString();
			if (element.TryGetProperty("doc_count", out var docCount))
				bucket.DocCount = docCount.GetInt64();
			if (element.TryGetProperty("doc_count_error_upper_bound", out var dce) && dce.ValueKind == JsonValueKind.Number)
				bucket.DocCountErrorUpperBound = dce.GetInt64();
			return bucket;
		}

		private static IBucket ReadRangeBucket(JsonElement element, JsonSerializerOptions options)
		{
			var fromString = element.TryGetProperty("from", out var f) && f.ValueKind == JsonValueKind.String ? f.GetString() : null;
			var toString = element.TryGetProperty("to", out var t) && t.ValueKind == JsonValueKind.String ? t.GetString() : null;

			if (fromString != null || toString != null)
			{
				var ipBucket = new IpRangeBucket(ReadSubAggregates(element, options)) { From = fromString, To = toString };
				if (element.TryGetProperty("key", out var ipKey)) ipBucket.Key = ipKey.GetString();
				if (element.TryGetProperty("doc_count", out var ipCount)) ipBucket.DocCount = ipCount.GetInt64();
				return ipBucket;
			}

			var bucket = new RangeBucket(ReadSubAggregates(element, options))
			{
				From = GetNullableDouble(element, "from"),
				To = GetNullableDouble(element, "to"),
			};
			if (element.TryGetProperty("key", out var key)) bucket.Key = key.GetString();
			if (element.TryGetProperty("from_as_string", out var fas)) bucket.FromAsString = fas.GetString();
			if (element.TryGetProperty("to_as_string", out var tas)) bucket.ToAsString = tas.GetString();
			if (element.TryGetProperty("doc_count", out var docCount)) bucket.DocCount = docCount.GetInt64();
			return bucket;
		}

		private static IAggregate ReadPercentiles(JsonElement values, IReadOnlyDictionary<string, object> meta)
		{
			var aggregate = new PercentilesAggregate { Meta = meta };

			if (values.ValueKind == JsonValueKind.Object)
			{
				foreach (var member in values.EnumerateObject())
				{
					if (member.Name.Contains("_as_string")) continue;
					aggregate.Items.Add(new PercentileItem
					{
						Percentile = double.Parse(member.Name, System.Globalization.CultureInfo.InvariantCulture),
						Value = member.Value.ValueKind == JsonValueKind.Null ? null : member.Value.GetDouble(),
					});
				}
			}
			else if (values.ValueKind == JsonValueKind.Array)
			{
				foreach (var item in values.EnumerateArray())
				{
					aggregate.Items.Add(new PercentileItem
					{
						Percentile = item.GetProperty("key").GetDouble(),
						Value = item.TryGetProperty("value", out var v) && v.ValueKind != JsonValueKind.Null ? v.GetDouble() : (double?)null,
					});
				}
			}

			return aggregate;
		}

		private static string FirstPropertyName(JsonElement element)
		{
			foreach (var member in element.EnumerateObject())
				return member.Name;
			return null;
		}

		private static Dictionary<string, IAggregate> ReadSubAggregates(JsonElement element, JsonSerializerOptions options)
		{
			Dictionary<string, IAggregate> subAggregates = null;
			foreach (var member in element.EnumerateObject())
			{
				if (ReservedBucketKeys.Contains(member.Name) || member.Value.ValueKind != JsonValueKind.Object)
					continue;

				var subAggregate = ReadAggregate(member.Value, options);
				if (subAggregate == null) continue;
				(subAggregates ??= new Dictionary<string, IAggregate>())[member.Name] = subAggregate;
			}
			return subAggregates;
		}

		private static double? GetNullableDouble(JsonElement root, string name) =>
			root.TryGetProperty(name, out var element) && element.ValueKind == JsonValueKind.Number
				? element.GetDouble()
				: (double?)null;
	}
}
