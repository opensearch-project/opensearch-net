/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.Json;
using OpenSearch.Net;
using OpenSearch.Net.Utf8Json;
// AggregateConverter reuses the legacy Utf8Json resolver for sub-value parity, so both JsonProperty types are in
// scope; disambiguate to the System.Text.Json one used when walking the buffered DOM.
using JsonProperty = System.Text.Json.JsonProperty;

namespace OpenSearch.Client
{
	/// <summary>
	/// System.Text.Json replacement for the legacy Utf8Json <c>AggregateFormatter</c>. Polymorphically parses an
	/// aggregation-response object into the correct <see cref="IAggregate"/> subtype using a heuristic based on which
	/// fields are present and — crucially — the ORDER in which the legacy formatter inspected them.
	///
	/// <para><b>Field-presence dispatch precedence.</b> The legacy formatter looked at the FIRST property of the
	/// object (after an optional leading <c>meta</c>) and dispatched on it via a fixed key table. That precedence is
	/// preserved exactly here (a given key is only reached if the earlier keys were not the first property):</para>
	/// <list type="number">
	/// <item><description><c>values</c>            → <see cref="PercentilesAggregate"/></description></item>
	/// <item><description><c>value</c>             → <see cref="ValueAggregate"/> / <see cref="KeyedValueAggregate"/> / <see cref="ScriptedMetricAggregate"/></description></item>
	/// <item><description><c>after_key</c>         → composite <see cref="BucketAggregate"/> (with <c>AfterKey</c>)</description></item>
	/// <item><description><c>buckets</c>           → multi-bucket <see cref="BucketAggregate"/> / <see cref="FiltersAggregate"/></description></item>
	/// <item><description><c>doc_count_error_upper_bound</c> → multi-bucket <see cref="BucketAggregate"/></description></item>
	/// <item><description><c>count</c>             → <see cref="StatsAggregate"/> / <see cref="ExtendedStatsAggregate"/> (or <see cref="GeoCentroidAggregate"/> when it is the only field)</description></item>
	/// <item><description><c>doc_count</c>         → <see cref="SingleBucketAggregate"/> / <see cref="MatrixStatsAggregate"/> / <see cref="BucketAggregate"/></description></item>
	/// <item><description><c>bounds</c>            → <see cref="GeoBoundsAggregate"/></description></item>
	/// <item><description><c>hits</c>              → <see cref="TopHitsAggregate"/></description></item>
	/// <item><description><c>location</c>          → <see cref="GeoCentroidAggregate"/></description></item>
	/// <item><description><c>fields</c>            → <see cref="MatrixStatsAggregate"/></description></item>
	/// <item><description><c>min</c>               → <see cref="GeoLineAggregate"/> (legacy mapping preserved verbatim)</description></item>
	/// </list>
	/// Any other first property (including <c>top</c>/<c>type</c>, which the legacy table listed but had no handler
	/// for) yields <c>null</c>.
	///
	/// <para><b>Forward-only reader.</b> System.Text.Json's <see cref="Utf8JsonReader"/> cannot rewind, and this
	/// parser needs to peek at multiple fields before committing to a type, so — like the migrated
	/// <c>RangeQueryConverter</c> — the value is buffered into a <see cref="JsonDocument"/> and the heuristic runs
	/// over the DOM (which preserves property order).</para>
	///
	/// <para><b>Sub-value parity.</b> Where the legacy formatter delegated to another Utf8Json formatter
	/// (<c>GeoLocation</c>, <c>LatLon</c>, <c>TotalHits</c>, <c>CompositeKey</c>, <c>LineStringGeoShape</c>,
	/// <c>GeoLineProperties</c>, <c>StandardDeviationBounds</c>, <c>MatrixStatsField</c>, the <c>meta</c> dictionary,
	/// nested documents, …) this converter re-serialises the relevant <see cref="JsonElement"/> and feeds it through
	/// the very same legacy formatter (via an <see cref="OpenSearchClientFormatterResolver"/> built from the injected
	/// settings). This guarantees byte-for-byte parity for those leaf types regardless of whether they have been
	/// migrated to System.Text.Json yet, and is why the converter is <see cref="SettingsAwareConverter{T}"/>.</para>
	///
	/// Serialization is not supported (mirrors the legacy formatter which threw <see cref="NotSupportedException"/>).
	/// </summary>
	internal class AggregateConverter : SettingsAwareConverter<IAggregate>
	{
		private IJsonFormatterResolver _resolver;

		public AggregateConverter(IConnectionSettingsValues settings) : base(settings) { }

		private IJsonFormatterResolver Resolver =>
			_resolver ?? (_resolver = new OpenSearchClientFormatterResolver(Settings));

		public override IAggregate Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null)
				return null;

			using var doc = JsonDocument.ParseValue(ref reader);
			return ReadAggregateElement(doc.RootElement);
		}

		public override void Write(Utf8JsonWriter writer, IAggregate value, JsonSerializerOptions options) =>
			throw new NotSupportedException();

		private IAggregate ReadAggregateElement(JsonElement element)
		{
			// Legacy: ReadIsBeginObjectWithVerify — a non-object is a protocol error.
			if (element.ValueKind == JsonValueKind.Null)
				return null;
			if (element.ValueKind != JsonValueKind.Object)
				throw new JsonException($"Expected an aggregate object but found {element.ValueKind}.");

			var props = new List<JsonProperty>();
			foreach (var p in element.EnumerateObject())
				props.Add(p);

			// Empty object → null (legacy returned null immediately on end-of-object).
			if (props.Count == 0)
				return null;

			var index = 0;
			IReadOnlyDictionary<string, object> meta = null;

			if (props[0].NameEquals(Parser.Meta))
			{
				meta = DeserializeLegacy<Dictionary<string, object>>(props[0].Value);
				index = 1;
			}

			// After (optionally) consuming meta the legacy formatter unconditionally read the next property name; if
			// there is none the key-table lookup fails and the aggregate stays null.
			if (index >= props.Count)
				return null;

			var first = props[index];
			var name = first.Name;

			if (name == Parser.Values)
				return GetPercentilesAggregate(first.Value, meta);
			if (name == Parser.Value)
				return GetValueAggregate(props, index, meta);
			if (name == Parser.AfterKey)
				return GetCompositeWithAfterKey(props, index, meta);
			if (name == Parser.Buckets || name == Parser.DocCountErrorUpperBound)
				return GetMultiBucketAggregate(props, index, meta);
			if (name == Parser.Count)
				return GetStatsAggregate(props, index, meta);
			if (name == Parser.DocCount)
				return GetSingleBucketAggregate(props, index, meta);
			if (name == Parser.Bounds)
				return GetGeoBoundsAggregate(first.Value, meta);
			if (name == Parser.Hits)
				return GetTopHitsAggregate(first.Value, meta);
			if (name == Parser.Location)
				return GetGeoCentroidAggregate(props, index, meta);
			if (name == Parser.Fields)
				return GetMatrixStatsAggregate(first.Value, meta, 0);
			if (name == Parser.Min)
				return GetGeoLineAggregate(props, index, meta);

			// Unknown first property (incl. "top"/"type", which the legacy table listed without a handler): skip.
			return null;
		}

		// values → percentiles. The value is either an object of {"<percentile>": <double>} or an array of
		// {"key": <double>, "value": <double>} items.
		private IAggregate GetPercentilesAggregate(JsonElement values, IReadOnlyDictionary<string, object> meta)
		{
			var metric = new PercentilesAggregate { Meta = meta };

			if (values.ValueKind == JsonValueKind.Object)
			{
				foreach (var p in values.EnumerateObject())
				{
					if (p.Name.Contains(Parser.AsStringSuffix))
						continue;

					metric.Items.Add(new PercentileItem
					{
						Percentile = double.Parse(p.Name, CultureInfo.InvariantCulture),
						Value = NullableDouble(p.Value)
					});
				}
			}
			else if (values.ValueKind == JsonValueKind.Array)
			{
				foreach (var item in values.EnumerateArray())
				{
					metric.Items.Add(new PercentileItem
					{
						Percentile = item.GetProperty(Parser.Key).GetDouble(),
						Value = item.TryGetProperty(Parser.Value, out var v) ? NullableDouble(v) : null
					});
				}
			}

			return metric;
		}

		// value → single value metric, keyed value metric, or (when the value is not a number/null) a scripted metric
		// whose entire value is captured as a lazy document.
		private IAggregate GetValueAggregate(IReadOnlyList<JsonProperty> props, int index, IReadOnlyDictionary<string, object> meta)
		{
			var valueElement = props[index].Value;
			var kind = valueElement.ValueKind;

			if (kind == JsonValueKind.Number || kind == JsonValueKind.Null)
			{
				var value = NullableDouble(valueElement);
				string valueAsString = null;

				var next = index + 1;
				if (next < props.Count)
				{
					var p = props[next];
					if (p.Name == Parser.ValueAsString)
					{
						valueAsString = p.Value.ValueKind == JsonValueKind.String ? p.Value.GetString() : null;
						next++;
						if (next >= props.Count)
							return new ValueAggregate { Value = value, ValueAsString = valueAsString, Meta = meta };
						p = props[next];
					}

					if (p.Name == Parser.Keys)
					{
						return new KeyedValueAggregate
						{
							Value = value,
							Meta = meta,
							Keys = DeserializeLegacy<List<string>>(p.Value)
						};
					}

					// Any remaining properties are skipped (legacy read them as opaque blocks).
				}

				return new ValueAggregate { Value = value, ValueAsString = valueAsString, Meta = meta };
			}

			var bytes = Encoding.UTF8.GetBytes(valueElement.GetRawText());
			var doc = new LazyDocument(bytes, Resolver);
			return new ScriptedMetricAggregate(doc) { Meta = meta };
		}

		// after_key → the composite key of the last bucket, followed by a "buckets" multi-bucket aggregate.
		private IAggregate GetCompositeWithAfterKey(IReadOnlyList<JsonProperty> props, int index, IReadOnlyDictionary<string, object> meta)
		{
			var afterKey = DeserializeLegacy<CompositeKey>(props[index].Value);

			var next = index + 1;
			BucketAggregate bucketAggregate;
			if (next < props.Count && props[next].Name == Parser.Buckets)
				bucketAggregate = GetMultiBucketAggregate(props, next, meta) as BucketAggregate ?? new BucketAggregate { Meta = meta };
			else
				bucketAggregate = new BucketAggregate { Meta = meta };

			bucketAggregate.AfterKey = afterKey;
			return bucketAggregate;
		}

		// buckets / doc_count_error_upper_bound / sum_other_doc_count → multi-bucket aggregate (or filters aggregate
		// when "buckets" is an object rather than an array).
		private IAggregate GetMultiBucketAggregate(IReadOnlyList<JsonProperty> props, int index, IReadOnlyDictionary<string, object> meta)
		{
			var bucket = new BucketAggregate { Meta = meta };
			var i = index;

			if (i < props.Count && props[i].Name == Parser.DocCountErrorUpperBound)
			{
				bucket.DocCountErrorUpperBound = NullableInt64(props[i].Value);
				i++;
			}

			if (i < props.Count && props[i].Name == Parser.SumOtherDocCount)
			{
				bucket.SumOtherDocCount = NullableInt64(props[i].Value);
				i++; // advance to the "buckets" property
			}

			if (i >= props.Count)
				return bucket;

			var bucketsElement = props[i].Value;

			if (bucketsElement.ValueKind == JsonValueKind.Object)
			{
				var filterAggregates = new Dictionary<string, IAggregate>();
				foreach (var p in bucketsElement.EnumerateObject())
					filterAggregates[p.Name] = ReadAggregateElement(p.Value);
				return new FiltersAggregate(filterAggregates) { Meta = meta };
			}

			var items = new List<IBucket>();
			if (bucketsElement.ValueKind == JsonValueKind.Array)
			{
				foreach (var element in bucketsElement.EnumerateArray())
				{
					var read = ReadBucket(element);
					items.Add(read);
				}
			}
			bucket.Items = items;

			// Trailing sibling "interval" (auto_date_histogram) is captured; anything else is ignored.
			var afterBuckets = i + 1;
			if (afterBuckets < props.Count && props[afterBuckets].Name == "interval")
				bucket.AutoInterval = DeserializeLegacy<DateMathTime>(props[afterBuckets].Value);

			return bucket;
		}

		// count → stats / extended stats (or geo centroid when count is the only field).
		private IAggregate GetStatsAggregate(IReadOnlyList<JsonProperty> props, int index, IReadOnlyDictionary<string, object> meta)
		{
			var count = NullableInt64(props[index].Value).GetValueOrDefault(0);

			// count with no following fields is the no-results geo_centroid response.
			if (index + 1 >= props.Count)
				return new GeoCentroidAggregate { Count = count, Meta = meta };

			// The legacy formatter read min/max/avg/sum positionally (it did not check their names).
			var min = index + 1 < props.Count ? NullableDouble(props[index + 1].Value) : null;
			var max = index + 2 < props.Count ? NullableDouble(props[index + 2].Value) : null;
			var average = index + 3 < props.Count ? NullableDouble(props[index + 3].Value) : null;
			var sum = index + 4 < props.Count ? NullableDouble(props[index + 4].Value).GetValueOrDefault(0) : 0;

			var statsMetric = new StatsAggregate
			{
				Average = average,
				Count = count,
				Max = max,
				Min = min,
				Sum = sum,
				Meta = meta
			};

			// After the four core stats, skip any *_as_string siblings; the first non-string-suffixed sibling begins
			// the extended-stats section (sum_of_squares).
			var next = index + 5;
			while (next < props.Count && props[next].Name.EndsWith(Parser.AsStringSuffix, StringComparison.Ordinal))
				next++;

			if (next >= props.Count)
				return statsMetric;

			return GetExtendedStatsAggregate(props, next, statsMetric, meta);
		}

		private IAggregate GetExtendedStatsAggregate(IReadOnlyList<JsonProperty> props, int index, StatsAggregate statsMetric,
			IReadOnlyDictionary<string, object> meta)
		{
			var extended = new ExtendedStatsAggregate
			{
				Average = statsMetric.Average,
				Count = statsMetric.Count,
				Max = statsMetric.Max,
				Min = statsMetric.Min,
				Sum = statsMetric.Sum,
				Meta = meta,
				SumOfSquares = NullableDouble(props[index].Value)
			};

			for (var i = index + 1; i < props.Count; i++)
			{
				switch (props[i].Name)
				{
					case "variance":
						extended.Variance = NullableDouble(props[i].Value);
						break;
					case "std_deviation":
						extended.StdDeviation = NullableDouble(props[i].Value);
						break;
					case "std_deviation_bounds":
						extended.StdDeviationBounds = DeserializeLegacy<StandardDeviationBounds>(props[i].Value);
						break;
					case "variance_population":
						extended.VariancePopulation = NullableDouble(props[i].Value);
						break;
					case "variance_sampling":
						extended.VarianceSampling = NullableDouble(props[i].Value);
						break;
					case "std_deviation_population":
						extended.StdDeviationPopulation = NullableDouble(props[i].Value);
						break;
					case "std_deviation_sampling":
						extended.StdDeviationSampling = NullableDouble(props[i].Value);
						break;
					// other keys (incl. *_as_string) are ignored
				}
			}

			return extended;
		}

		// doc_count → single bucket / matrix stats / (bg_count-prefixed) bucket aggregate.
		private IAggregate GetSingleBucketAggregate(IReadOnlyList<JsonProperty> props, int index, IReadOnlyDictionary<string, object> meta)
		{
			var docCount = ReadInt64(props[index].Value);

			var i = index + 1;
			if (i >= props.Count)
				return new SingleBucketAggregate(null) { DocCount = docCount, Meta = meta };

			long bgCount = 0;
			if (props[i].Name == Parser.BgCount)
			{
				bgCount = ReadInt64(props[i].Value);
				i++;
			}

			if (i >= props.Count)
				return new SingleBucketAggregate(null) { DocCount = docCount, Meta = meta };

			if (props[i].Name == Parser.Fields)
				return GetMatrixStatsAggregate(props[i].Value, meta, docCount);

			if (props[i].Name == Parser.Buckets)
			{
				var b = GetMultiBucketAggregate(props, i, meta) as BucketAggregate;
				return new BucketAggregate
				{
					BgCount = bgCount,
					DocCount = docCount,
					Items = b?.Items ?? EmptyReadOnly<IBucket>.Collection,
					Meta = meta
				};
			}

			var subAggregates = GetSubAggregates(props, i);
			return new SingleBucketAggregate(subAggregates) { DocCount = docCount, Meta = meta };
		}

		private IAggregate GetMatrixStatsAggregate(JsonElement fields, IReadOnlyDictionary<string, object> meta, long docCount)
		{
			return new MatrixStatsAggregate
			{
				DocCount = docCount,
				Meta = meta,
				Fields = DeserializeLegacy<List<MatrixStatsField>>(fields)
			};
		}

		// bounds → geo bounds. Operates on the value object of the "bounds" property.
		private IAggregate GetGeoBoundsAggregate(JsonElement bounds, IReadOnlyDictionary<string, object> meta)
		{
			if (bounds.ValueKind == JsonValueKind.Null)
				return null;

			var metric = new GeoBoundsAggregate { Meta = meta };
			foreach (var p in bounds.EnumerateObject())
			{
				if (p.Name == Parser.TopLeft)
					metric.Bounds.TopLeft = DeserializeLegacy<LatLon>(p.Value);
				else if (p.Name == Parser.BottomRight)
					metric.Bounds.BottomRight = DeserializeLegacy<LatLon>(p.Value);
			}

			return metric;
		}

		// hits → top hits. Operates on the value object of the "hits" property.
		private IAggregate GetTopHitsAggregate(JsonElement hits, IReadOnlyDictionary<string, object> meta)
		{
			double? maxScore = null;
			TotalHits total = null;
			List<LazyDocument> topHits = null;

			if (hits.ValueKind == JsonValueKind.Object)
			{
				foreach (var p in hits.EnumerateObject())
				{
					switch (p.Name)
					{
						case Parser.Total:
							total = DeserializeLegacy<TotalHits>(p.Value);
							break;
						case Parser.MaxScore:
							maxScore = NullableDouble(p.Value);
							break;
						case Parser.Hits:
							topHits = new List<LazyDocument>();
							if (p.Value.ValueKind == JsonValueKind.Array)
							{
								foreach (var hit in p.Value.EnumerateArray())
									topHits.Add(new LazyDocument(Encoding.UTF8.GetBytes(hit.GetRawText()), Resolver));
							}
							break;
					}
				}
			}

			return new TopHitsAggregate(topHits, Resolver)
			{
				Total = total,
				MaxScore = maxScore,
				Meta = meta
			};
		}

		// location → geo centroid (value object is the location, optional sibling "count").
		private IAggregate GetGeoCentroidAggregate(IReadOnlyList<JsonProperty> props, int index, IReadOnlyDictionary<string, object> meta)
		{
			var geoCentroid = new GeoCentroidAggregate
			{
				Location = DeserializeLegacy<GeoLocation>(props[index].Value),
				Meta = meta
			};

			var next = index + 1;
			if (next < props.Count && props[next].Name == Parser.Count)
				geoCentroid.Count = ReadInt64(props[next].Value);

			return geoCentroid;
		}

		// min → geo line (legacy dispatch preserved verbatim). The matched value is read as the "type" string, the two
		// following siblings as geometry / properties.
		private IAggregate GetGeoLineAggregate(IReadOnlyList<JsonProperty> props, int index, IReadOnlyDictionary<string, object> meta)
		{
			var geoLine = new GeoLineAggregate { Meta = meta };

			if (props[index].Value.ValueKind == JsonValueKind.Null)
				return geoLine;

			geoLine.Type = props[index].Value.ValueKind == JsonValueKind.String ? props[index].Value.GetString() : null;

			for (var k = 1; k <= 2; k++)
			{
				var i = index + k;
				if (i >= props.Count)
					break;

				if (props[i].Name == Parser.Geometry)
					geoLine.Geometry = DeserializeLegacy<LineStringGeoShape>(props[i].Value);
				else if (props[i].Name == Parser.Properties)
					geoLine.Properties = DeserializeLegacy<GeoLineProperties>(props[i].Value);
			}

			return geoLine;
		}

		// -------- buckets --------

		private IBucket ReadBucket(JsonElement element)
		{
			if (element.ValueKind != JsonValueKind.Object)
				return null;

			var props = new List<JsonProperty>();
			foreach (var p in element.EnumerateObject())
				props.Add(p);

			if (props.Count == 0)
				return null;

			switch (props[0].Name)
			{
				case Parser.Key:
					return GetKeyedBucket(props);
				case Parser.From:
				case Parser.To:
					return GetRangeBucket(props, 0, null);
				case Parser.KeyAsString:
					return GetDateHistogramBucket(props);
				case Parser.DocCount:
					return GetFiltersBucket(props);
				case Parser.Min:
					return GetVariableWidthHistogramBucket(props);
				default:
					return null;
			}
		}

		private IBucket GetKeyedBucket(IReadOnlyList<JsonProperty> props)
		{
			var keyElement = props[0].Value;

			if (keyElement.ValueKind == JsonValueKind.Object)
				return GetCompositeBucket(props);

			object key;
			if (keyElement.ValueKind == JsonValueKind.String)
				key = keyElement.GetString();
			else if (keyElement.ValueKind == JsonValueKind.Array)
			{
				var keys = new List<object>();
				foreach (var k in keyElement.EnumerateArray())
					keys.Add(ReadKeyItem(k));
				key = keys;
			}
			else
				key = ReadKeyItem(keyElement);

			if (props.Count < 2)
				return new KeyedBucket<object>(null) { Key = key, DocCount = 0 };

			var i = 1;
			if (props[i].Name == Parser.From || props[i].Name == Parser.To)
			{
				var rangeKey = key is double d ? d.ToString("#.#") : key.ToString();
				return GetRangeBucket(props, i, rangeKey);
			}

			string keyAsString = null;
			if (props[i].Name == Parser.KeyAsString)
			{
				keyAsString = props[i].Value.ValueKind == JsonValueKind.String ? props[i].Value.GetString() : null;
				i++;
			}

			// doc_count
			long docCount = 0;
			if (i < props.Count && props[i].Name == Parser.DocCount)
			{
				docCount = ReadInt64(props[i].Value);
				i++;
			}

			Dictionary<string, IAggregate> subAggregates = null;
			long? docCountErrorUpperBound = null;

			if (i < props.Count)
			{
				if (props[i].Name == Parser.Score)
					return GetSignificantTermsBucket(props, i, key, docCount);

				if (props[i].Name == Parser.DocCountErrorUpperBound)
				{
					docCountErrorUpperBound = NullableInt64(props[i].Value);
					i++;
					if (i < props.Count)
						subAggregates = GetSubAggregates(props, i);
				}
				else
					subAggregates = GetSubAggregates(props, i);
			}

			return new KeyedBucket<object>(subAggregates)
			{
				Key = key,
				KeyAsString = keyAsString,
				DocCount = docCount,
				DocCountErrorUpperBound = docCountErrorUpperBound
			};
		}

		private IBucket GetCompositeBucket(IReadOnlyList<JsonProperty> props)
		{
			var key = new CompositeKey(DeserializeLegacy<IReadOnlyDictionary<string, object>>(props[0].Value));
			long? docCount = null;
			Dictionary<string, IAggregate> nestedAggregates = null;

			for (var i = 1; i < props.Count; i++)
			{
				if (props[i].Name == Parser.DocCount)
					docCount = NullableInt64(props[i].Value);
				else
				{
					nestedAggregates = GetSubAggregates(props, i);
					break;
				}
			}

			return new CompositeBucket(nestedAggregates, key) { DocCount = docCount };
		}

		private IBucket GetSignificantTermsBucket(IReadOnlyList<JsonProperty> props, int index, object key, long? docCount)
		{
			var score = props[index].Value.GetDouble();
			var bgCount = index + 1 < props.Count ? ReadInt64(props[index + 1].Value) : 0;

			Dictionary<string, IAggregate> subAggregates = null;
			if (index + 2 < props.Count)
				subAggregates = GetSubAggregates(props, index + 2);

			return new SignificantTermsBucket<object>(subAggregates)
			{
				Key = key,
				DocCount = docCount.GetValueOrDefault(0),
				BgCount = bgCount,
				Score = score
			};
		}

		private IBucket GetRangeBucket(IReadOnlyList<JsonProperty> props, int index, string key)
		{
			string fromAsString = null;
			string fromString = null;
			string toAsString = null;
			string toString = null;
			long? docCount = null;
			double? toDouble = null;
			double? fromDouble = null;
			var subAggStart = -1;

			for (var i = index; i < props.Count; i++)
			{
				var p = props[i];
				switch (p.Name)
				{
					case Parser.From:
						if (p.Value.ValueKind == JsonValueKind.Number)
							fromDouble = p.Value.GetDouble();
						else if (p.Value.ValueKind == JsonValueKind.String)
							fromString = p.Value.GetString();
						break;
					case Parser.To:
						if (p.Value.ValueKind == JsonValueKind.Number)
							toDouble = p.Value.GetDouble();
						else if (p.Value.ValueKind == JsonValueKind.String)
							toString = p.Value.GetString();
						break;
					case Parser.Key:
						key = p.Value.ValueKind == JsonValueKind.String ? p.Value.GetString() : key;
						break;
					case Parser.FromAsString:
						fromAsString = p.Value.GetString();
						break;
					case Parser.ToAsString:
						toAsString = p.Value.GetString();
						break;
					case Parser.DocCount:
						docCount = NullableInt64(p.Value).GetValueOrDefault(0);
						break;
					default:
						subAggStart = i;
						break;
				}

				if (subAggStart >= 0)
					break;
			}

			var subAggregates = subAggStart >= 0 ? GetSubAggregates(props, subAggStart) : null;

			if (fromString != null || toString != null)
				return new IpRangeBucket(subAggregates)
				{
					Key = key,
					DocCount = docCount.GetValueOrDefault(),
					From = fromString,
					To = toString
				};

			return new RangeBucket(subAggregates)
			{
				Key = key,
				From = fromDouble,
				To = toDouble,
				DocCount = docCount.GetValueOrDefault(),
				FromAsString = fromAsString,
				ToAsString = toAsString
			};
		}

		private IBucket GetDateHistogramBucket(IReadOnlyList<JsonProperty> props)
		{
			var keyAsString = props[0].Value.ValueKind == JsonValueKind.String ? props[0].Value.GetString() : null;
			var key = props.Count > 1 ? props[1].Value.GetDouble() : 0;
			var docCount = props.Count > 2 ? ReadInt64(props[2].Value) : 0;

			Dictionary<string, IAggregate> subAggregates = null;
			if (props.Count > 3)
				subAggregates = GetSubAggregates(props, 3);

			return new DateHistogramBucket(subAggregates)
			{
				Key = key,
				KeyAsString = keyAsString,
				DocCount = docCount
			};
		}

		private IBucket GetVariableWidthHistogramBucket(IReadOnlyList<JsonProperty> props)
		{
			var min = props[0].Value.GetDouble();
			var key = props.Count > 1 ? props[1].Value.GetDouble() : 0;
			var max = props.Count > 2 ? props[2].Value.GetDouble() : 0;
			var docCount = props.Count > 3 ? ReadInt64(props[3].Value) : 0;

			Dictionary<string, IAggregate> subAggregates = null;
			if (props.Count > 4)
				subAggregates = GetSubAggregates(props, 4);

			return new VariableWidthHistogramBucket(subAggregates)
			{
				Key = key,
				Minimum = min,
				Maximum = max,
				DocCount = docCount
			};
		}

		private IBucket GetFiltersBucket(IReadOnlyList<JsonProperty> props)
		{
			var docCount = NullableInt64(props[0].Value).GetValueOrDefault(0);

			if (props.Count < 2)
				return new FiltersBucketItem(EmptyReadOnly<string, IAggregate>.Dictionary) { DocCount = docCount };

			var subAggregates = GetSubAggregates(props, 1);
			return new FiltersBucketItem(subAggregates) { DocCount = docCount };
		}

		// Every property from startIndex to the end is a named nested aggregate.
		private Dictionary<string, IAggregate> GetSubAggregates(IReadOnlyList<JsonProperty> props, int startIndex)
		{
			var subAggregates = new Dictionary<string, IAggregate>();
			for (var i = startIndex; i < props.Count; i++)
				subAggregates[props[i].Name] = ReadAggregateElement(props[i].Value);
			return subAggregates;
		}

		// -------- helpers --------

		private T DeserializeLegacy<T>(JsonElement element)
		{
			var bytes = Encoding.UTF8.GetBytes(element.GetRawText());
			var reader = new JsonReader(bytes);
			return Resolver.GetFormatter<T>().Deserialize(ref reader, Resolver);
		}

		private static object ReadKeyItem(JsonElement element)
		{
			if (element.ValueKind == JsonValueKind.String)
				return element.GetString();

			if (element.ValueKind == JsonValueKind.Number)
				return element.TryGetInt64(out var l) ? (object)l : element.GetDouble();

			// Fallback: keep the raw text so no information is silently dropped.
			return element.ToString();
		}

		private static double? NullableDouble(JsonElement element)
		{
			switch (element.ValueKind)
			{
				case JsonValueKind.Null:
					return null;
				case JsonValueKind.Number:
					return element.GetDouble();
				case JsonValueKind.String:
					return double.TryParse(element.GetString(), NumberStyles.Float | NumberStyles.AllowThousands,
						CultureInfo.InvariantCulture, out var d)
						? d
						: (double?)null;
				default:
					return null;
			}
		}

		private static long ReadInt64(JsonElement element)
		{
			if (element.ValueKind == JsonValueKind.Number)
				return element.TryGetInt64(out var l) ? l : (long)element.GetDouble();
			if (element.ValueKind == JsonValueKind.String && long.TryParse(element.GetString(), out var s))
				return s;
			return 0;
		}

		private static long? NullableInt64(JsonElement element)
		{
			switch (element.ValueKind)
			{
				case JsonValueKind.Null:
					return null;
				case JsonValueKind.Number:
					return element.TryGetInt64(out var l) ? l : (long)element.GetDouble();
				case JsonValueKind.String:
					return long.TryParse(element.GetString(), out var s) ? s : (long?)null;
				default:
					return null;
			}
		}

		private static class Parser
		{
			public const string AfterKey = "after_key";
			public const string AsStringSuffix = "_as_string";
			public const string BgCount = "bg_count";
			public const string BottomRight = "bottom_right";
			public const string Bounds = "bounds";
			public const string Buckets = "buckets";
			public const string Count = "count";
			public const string DocCount = "doc_count";
			public const string DocCountErrorUpperBound = "doc_count_error_upper_bound";
			public const string Fields = "fields";
			public const string From = "from";
			public const string FromAsString = "from_as_string";
			public const string Geometry = "geometry";
			public const string Hits = "hits";
			public const string Key = "key";
			public const string KeyAsString = "key_as_string";
			public const string Keys = "keys";
			public const string Location = "location";
			public const string MaxScore = "max_score";
			public const string Meta = "meta";
			public const string Min = "min";
			public const string Properties = "properties";
			public const string Score = "score";
			public const string SumOtherDocCount = "sum_other_doc_count";
			public const string To = "to";
			public const string ToAsString = "to_as_string";
			public const string TopLeft = "top_left";
			public const string Total = "total";
			public const string Value = "value";
			public const string ValueAsString = "value_as_string";
			public const string Values = "values";
		}
	}
}
