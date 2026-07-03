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
				return new StatsAggregate
				{
					Count = countElement.ValueKind == JsonValueKind.Null ? 0 : countElement.GetInt64(),
					Min = GetNullableDouble(root, "min"),
					Max = GetNullableDouble(root, "max"),
					Average = GetNullableDouble(root, "avg"),
					Sum = GetNullableDouble(root, "sum") ?? 0,
					Meta = meta,
				};
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
