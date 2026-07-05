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
				// A scripted_metric aggregation returns an arbitrary value (an object or array), which the
				// standard metric aggregates never do. Preserve it as a LazyDocument so
				// ScriptedMetricAggregate.Value<T>() can materialize it into the caller's shape (#388).
				if (valueElement.ValueKind == JsonValueKind.Object || valueElement.ValueKind == JsonValueKind.Array)
				{
					var lazy = valueElement.Deserialize<LazyDocument>(options);
					return new ScriptedMetricAggregate(lazy) { Meta = meta };
				}

				var scalarValue = valueElement.ValueKind == JsonValueKind.Null ? (double?)null : valueElement.GetDouble();

				// A siblingpipeline bucket metric (max_bucket/min_bucket) carries the bucket keys.
				if (root.TryGetProperty("keys", out var keysElement) && keysElement.ValueKind == JsonValueKind.Array)
					return new KeyedValueAggregate
					{
						Value = scalarValue,
						Meta = meta,
						Keys = keysElement.Deserialize<List<string>>(options),
					};

				var aggregate = new ValueAggregate { Value = scalarValue, Meta = meta };
				if (root.TryGetProperty("value_as_string", out var valueAsString))
					aggregate.ValueAsString = valueAsString.GetString();
				return aggregate;
			}

			// top_hits: { "hits": { "total": …, "max_score": …, "hits": [ …raw hit docs… ] } }
			if (root.TryGetProperty("hits", out var topHits) && topHits.ValueKind == JsonValueKind.Object
				&& topHits.TryGetProperty("hits", out _))
				return ReadTopHits(topHits, meta, options);

			// composite aggregation: an after_key alongside the buckets.
			if (root.TryGetProperty("after_key", out var afterKeyElement))
			{
				var composite = root.TryGetProperty("buckets", out var compositeBuckets)
					? ReadMultiBucket(root, compositeBuckets, meta, options) as BucketAggregate ?? new BucketAggregate { Meta = meta }
					: new BucketAggregate { Meta = meta };
				composite.AfterKey = new CompositeKey(afterKeyElement.Deserialize<Dictionary<string, object>>(options));
				return composite;
			}

			if (root.TryGetProperty("buckets", out var buckets))
				return ReadMultiBucket(root, buckets, meta, options);

			// geo_bounds: { "bounds": { "top_left": …, "bottom_right": … } }
			if (root.TryGetProperty("bounds", out var boundsElement) && boundsElement.ValueKind == JsonValueKind.Object)
			{
				var geoBounds = new GeoBoundsAggregate { Meta = meta };
				if (boundsElement.TryGetProperty("top_left", out var topLeft))
					geoBounds.Bounds.TopLeft = topLeft.Deserialize<LatLon>(options);
				if (boundsElement.TryGetProperty("bottom_right", out var bottomRight))
					geoBounds.Bounds.BottomRight = bottomRight.Deserialize<LatLon>(options);
				return geoBounds;
			}

			// geo_centroid: { "location": { "lat": …, "lon": … }, "count": … }
			if (root.TryGetProperty("location", out var locationElement))
			{
				var centroid = new GeoCentroidAggregate
				{
					Meta = meta,
					Location = locationElement.ValueKind == JsonValueKind.Null ? null : locationElement.Deserialize<GeoLocation>(options),
				};
				if (root.TryGetProperty("count", out var centroidCount) && centroidCount.ValueKind == JsonValueKind.Number)
					centroid.Count = centroidCount.GetInt64();
				return centroid;
			}

			// matrix_stats: { "doc_count": …, "fields": [ … ] }
			if (root.TryGetProperty("fields", out var fieldsElement) && fieldsElement.ValueKind == JsonValueKind.Array)
			{
				var matrix = new MatrixStatsAggregate { Meta = meta, Fields = fieldsElement.Deserialize<List<MatrixStatsField>>(options) };
				if (root.TryGetProperty("doc_count", out var matrixDocCount) && matrixDocCount.ValueKind == JsonValueKind.Number)
					matrix.DocCount = matrixDocCount.GetInt64();
				return matrix;
			}

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

		private static IAggregate ReadTopHits(JsonElement hits, IReadOnlyDictionary<string, object> meta, JsonSerializerOptions options)
		{
			var documents = new List<LazyDocument>();
			if (hits.TryGetProperty("hits", out var hitArray) && hitArray.ValueKind == JsonValueKind.Array)
			{
				foreach (var hit in hitArray.EnumerateArray())
				{
					var document = hit.Deserialize<LazyDocument>(options);
					if (document != null)
						documents.Add(document);
				}
			}

			var aggregate = new TopHitsAggregate(documents) { Meta = meta };
			if (hits.TryGetProperty("max_score", out var maxScore) && maxScore.ValueKind == JsonValueKind.Number)
				aggregate.MaxScore = maxScore.GetDouble();
			if (hits.TryGetProperty("total", out var total) && total.ValueKind != JsonValueKind.Null)
				aggregate.Total = total.Deserialize<TotalHits>(options);
			return aggregate;
		}

		private static IAggregate ReadMultiBucket(JsonElement root, JsonElement buckets, IReadOnlyDictionary<string, object> meta, JsonSerializerOptions options)
		{
			var aggregate = new BucketAggregate { Meta = meta };
			if (root.TryGetProperty("doc_count_error_upper_bound", out var dce) && dce.ValueKind == JsonValueKind.Number)
				aggregate.DocCountErrorUpperBound = dce.GetInt64();
			if (root.TryGetProperty("sum_other_doc_count", out var sod) && sod.ValueKind == JsonValueKind.Number)
				aggregate.SumOtherDocCount = sod.GetInt64();
			// significant_terms / significant_text carry the aggregate-level doc_count and bg_count
			// alongside their buckets.
			if (root.TryGetProperty("doc_count", out var aggDocCount) && aggDocCount.ValueKind == JsonValueKind.Number)
				aggregate.DocCount = aggDocCount.GetInt64();
			if (root.TryGetProperty("bg_count", out var aggBgCount) && aggBgCount.ValueKind == JsonValueKind.Number)
				aggregate.BgCount = aggBgCount.GetInt64();
			// auto_date_histogram reports the chosen interval alongside its buckets.
			if (root.TryGetProperty("interval", out var interval) && interval.ValueKind != JsonValueKind.Null)
				aggregate.AutoInterval = interval.Deserialize<DateMathTime>(options);

			// Named (keyed) buckets — the filters aggregation — are a JSON object whose property names are
			// the bucket keys (which may contain '#'). These are modeled as a FiltersAggregate: an
			// AggregateDictionary of named single-bucket aggregates (see FiltersAggregate remarks).
			if (buckets.ValueKind == JsonValueKind.Object)
			{
				var named = new Dictionary<string, IAggregate>();
				foreach (var member in buckets.EnumerateObject())
				{
					var single = new SingleBucketAggregate(ReadSubAggregates(member.Value, options));
					if (member.Value.TryGetProperty("doc_count", out var docCount) && docCount.ValueKind == JsonValueKind.Number)
						single.DocCount = docCount.GetInt64();
					named[member.Name] = single;
				}
				return new FiltersAggregate(named) { Meta = meta };
			}

			var items = new List<IBucket>();
			if (buckets.ValueKind == JsonValueKind.Array)
			{
				foreach (var bucketElement in buckets.EnumerateArray())
					items.Add(ReadKeyedBucket(bucketElement, options));
			}
			aggregate.Items = items;
			return aggregate;
		}

		private static IBucket ReadKeyedBucket(JsonElement element, JsonSerializerOptions options)
		{
			// composite bucket: the key is an object of source-name → value.
			if (element.TryGetProperty("key", out var objectKey) && objectKey.ValueKind == JsonValueKind.Object)
			{
				var compositeKey = new CompositeKey(objectKey.Deserialize<Dictionary<string, object>>(options));
				var composite = new CompositeBucket(ReadSubAggregates(element, options), compositeKey);
				if (element.TryGetProperty("doc_count", out var compositeCount) && compositeCount.ValueKind == JsonValueKind.Number)
					composite.DocCount = compositeCount.GetInt64();
				return composite;
			}

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

			// anonymous filters bucket: doc_count first, no key (mirrors the vendored ReadBucket dispatch).
			if (firstProperty == "doc_count")
			{
				var filtersBucket = new FiltersBucketItem(ReadSubAggregates(element, options));
				if (element.TryGetProperty("doc_count", out var fdc) && fdc.ValueKind == JsonValueKind.Number)
					filtersBucket.DocCount = fdc.GetInt64();
				return filtersBucket;
			}

			// variable_width_histogram bucket: min / key / max are all present.
			if (firstProperty == "min" && element.TryGetProperty("min", out var vwMin) && vwMin.ValueKind == JsonValueKind.Number
				&& element.TryGetProperty("max", out var vwMax) && vwMax.ValueKind == JsonValueKind.Number
				&& element.TryGetProperty("key", out var vwKey) && vwKey.ValueKind == JsonValueKind.Number)
			{
				var vwBucket = new VariableWidthHistogramBucket(ReadSubAggregates(element, options))
				{
					Key = vwKey.GetDouble(),
					Minimum = vwMin.GetDouble(),
					Maximum = vwMax.GetDouble(),
				};
				if (element.TryGetProperty("doc_count", out var vwCount)) vwBucket.DocCount = vwCount.GetInt64();
				return vwBucket;
			}

			var key = ReadBucketKey(element);

			// significant_terms / significant_text bucket: score + bg_count.
			if (element.TryGetProperty("score", out var scoreElement) && scoreElement.ValueKind == JsonValueKind.Number
				&& element.TryGetProperty("bg_count", out var bgCountElement) && bgCountElement.ValueKind == JsonValueKind.Number)
			{
				var significant = new SignificantTermsBucket<object>(ReadSubAggregates(element, options))
				{
					Key = key,
					Score = scoreElement.GetDouble(),
					BgCount = bgCountElement.GetInt64(),
				};
				if (element.TryGetProperty("doc_count", out var sigCount)) significant.DocCount = sigCount.GetInt64();
				return significant;
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

		/// <summary>Reads a bucket key: a string, a long/double, or (multi-terms) an array of such values.</summary>
		private static object ReadBucketKey(JsonElement element)
		{
			if (!element.TryGetProperty("key", out var keyElement))
				return null;

			switch (keyElement.ValueKind)
			{
				case JsonValueKind.String:
					return keyElement.GetString();
				// (object) cast on the long is required: without it the conditional unifies both arms to
				// double, silently converting a large long key and losing precision.
				case JsonValueKind.Number:
					return keyElement.TryGetInt64(out var l) ? (object)l : keyElement.GetDouble();
				case JsonValueKind.Array:
					var keys = new List<object>();
					foreach (var item in keyElement.EnumerateArray())
					{
						keys.Add(item.ValueKind switch
						{
							JsonValueKind.String => item.GetString(),
							JsonValueKind.Number => item.TryGetInt64(out var il) ? (object)il : item.GetDouble(),
							_ => null,
						});
					}
					return keys;
				default:
					return null;
			}
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
