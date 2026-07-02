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

using OpenSearch.Net;

namespace OpenSearch.Client.Aggregations.SystemTextJsonConverters
{
	/// <summary>
	/// Converter for <see cref="IAggregate"/> - the polymorphic aggregate response type.
	/// This is the most complex converter in the aggregation system because response JSON
	/// does not include an explicit type discriminator. We must infer the aggregate type
	/// from the properties present in the JSON object.
	///
	/// Classification heuristics:
	/// <list type="bullet">
	///   <item>"buckets" property → <see cref="BucketAggregate"/></item>
	///   <item>"value" property → <see cref="ValueAggregate"/></item>
	///   <item>"values" property → <see cref="PercentilesAggregate"/></item>
	///   <item>"doc_count" (without "buckets") → <see cref="SingleBucketAggregate"/></item>
	///   <item>Otherwise → <see cref="ValueAggregate"/> with null value (generic fallback)</item>
	/// </list>
	///
	/// Phase 2 implementation: focused on correct classification with basic property extraction.
	/// Full nested bucket/sub-aggregation parsing will be added as dedicated converters mature.
	/// </summary>
	internal sealed class AggregateConverter : JsonConverter<IAggregate>
	{
		private readonly IConnectionSettingsValues _settings;

		public AggregateConverter(IConnectionSettingsValues settings) => _settings = settings;

		public override IAggregate Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null)
				return null;

			if (reader.TokenType != JsonTokenType.StartObject)
			{
				// Some aggregates can be a simple value (rare edge case)
				reader.Skip();
				return null;
			}

			using var doc = JsonDocument.ParseValue(ref reader);
			var root = doc.RootElement;

			// Extract meta if present (common to all aggregate types)
			IReadOnlyDictionary<string, object> meta = null;
			if (root.TryGetProperty("meta", out var metaElement))
				meta = DeserializeMeta(metaElement);

			// Classification by presence of discriminating properties
			if (root.TryGetProperty("buckets", out var bucketsElement))
				return ReadBucketAggregate(root, bucketsElement, meta);

			if (root.TryGetProperty("doc_count", out var docCountElement))
				return ReadSingleBucketAggregate(root, docCountElement, meta, options);

			if (root.TryGetProperty("values", out var valuesElement))
				return ReadPercentilesAggregate(valuesElement, meta);

			if (root.TryGetProperty("value", out var valueElement))
				return ReadValueAggregate(root, valueElement, meta);

			// Fallback: return a ValueAggregate with null value
			return new ValueAggregate
			{
				Value = null,
				Meta = meta ?? EmptyReadOnly<string, object>.Dictionary
			};
		}

		public override void Write(Utf8JsonWriter writer, IAggregate value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			// Dispatch to typed serialization
			switch (value)
			{
				case ValueAggregate va:
					WriteValueAggregate(writer, va);
					break;
				case PercentilesAggregate pa:
					WritePercentilesAggregate(writer, pa);
					break;
				case BucketAggregate ba:
					WriteBucketAggregate(writer, ba, options);
					break;
				case SingleBucketAggregate sba:
					WriteSingleBucketAggregate(writer, sba, options);
					break;
				default:
					// Generic fallback - serialize as the concrete type
					JsonSerializer.Serialize(writer, value, value.GetType(), options);
					break;
			}
		}

		private static ValueAggregate ReadValueAggregate(
			JsonElement root, JsonElement valueElement, IReadOnlyDictionary<string, object> meta)
		{
			double? numericValue = null;
			string valueAsString = null;

			if (valueElement.ValueKind == JsonValueKind.Number)
				numericValue = valueElement.GetDouble();
			else if (valueElement.ValueKind == JsonValueKind.Null)
				numericValue = null;

			if (root.TryGetProperty("value_as_string", out var vasElement))
				valueAsString = vasElement.GetString();

			return new ValueAggregate
			{
				Value = numericValue,
				ValueAsString = valueAsString,
				Meta = meta ?? EmptyReadOnly<string, object>.Dictionary
			};
		}

		private static PercentilesAggregate ReadPercentilesAggregate(
			JsonElement valuesElement, IReadOnlyDictionary<string, object> meta)
		{
			var items = new List<PercentileItem>();

			if (valuesElement.ValueKind == JsonValueKind.Object)
			{
				foreach (var prop in valuesElement.EnumerateObject())
				{
					if (double.TryParse(prop.Name, System.Globalization.NumberStyles.Any,
						System.Globalization.CultureInfo.InvariantCulture, out var percentile))
					{
						double? val = null;
						if (prop.Value.ValueKind == JsonValueKind.Number)
							val = prop.Value.GetDouble();

						items.Add(new PercentileItem { Percentile = percentile, Value = val });
					}
				}
			}

			return new PercentilesAggregate
			{
				Items = items,
				Meta = meta ?? EmptyReadOnly<string, object>.Dictionary
			};
		}

		private static BucketAggregate ReadBucketAggregate(
			JsonElement root, JsonElement bucketsElement, IReadOnlyDictionary<string, object> meta)
		{
			var aggregate = new BucketAggregate
			{
				Meta = meta ?? EmptyReadOnly<string, object>.Dictionary
			};

			if (root.TryGetProperty("doc_count_error_upper_bound", out var dceub))
				aggregate.DocCountErrorUpperBound = dceub.GetInt64();

			if (root.TryGetProperty("sum_other_doc_count", out var sodc))
				aggregate.SumOtherDocCount = sodc.GetInt64();

			// Buckets parsing is complex - for Phase 2, store empty collection.
			// Full bucket parsing will be implemented with dedicated bucket converters.
			aggregate.Items = EmptyReadOnly<IBucket>.Collection;

			return aggregate;
		}

		private static SingleBucketAggregate ReadSingleBucketAggregate(
			JsonElement root, JsonElement docCountElement,
			IReadOnlyDictionary<string, object> meta, JsonSerializerOptions options)
		{
			var subAggregates = new Dictionary<string, IAggregate>();

			// Any property that is not a known single-bucket property is a sub-aggregation
			foreach (var prop in root.EnumerateObject())
			{
				switch (prop.Name)
				{
					case "doc_count":
					case "meta":
					case "bg_count":
					case "doc_count_error_upper_bound":
						// Known single-bucket properties, skip
						break;
					default:
						// Attempt to parse as a sub-aggregate
						if (prop.Value.ValueKind == JsonValueKind.Object)
						{
							var raw = prop.Value.GetRawText();
							var subAgg = JsonSerializer.Deserialize<IAggregate>(raw, options);
							if (subAgg != null)
								subAggregates[prop.Name] = subAgg;
						}
						break;
				}
			}

			var aggregate = new SingleBucketAggregate(subAggregates)
			{
				DocCount = docCountElement.GetInt64(),
				Meta = meta ?? EmptyReadOnly<string, object>.Dictionary
			};

			return aggregate;
		}

		private static void WriteValueAggregate(Utf8JsonWriter writer, ValueAggregate value)
		{
			writer.WriteStartObject();

			if (value.Value.HasValue)
				writer.WriteNumber("value", value.Value.Value);
			else
				writer.WriteNull("value");

			if (value.ValueAsString != null)
				writer.WriteString("value_as_string", value.ValueAsString);

			WriteMeta(writer, value.Meta);
			writer.WriteEndObject();
		}

		private static void WritePercentilesAggregate(Utf8JsonWriter writer, PercentilesAggregate value)
		{
			writer.WriteStartObject();
			writer.WriteStartObject("values");

			foreach (var item in value.Items)
			{
				var key = item.Percentile.ToString(System.Globalization.CultureInfo.InvariantCulture);
				if (item.Value.HasValue)
					writer.WriteNumber(key, item.Value.Value);
				else
					writer.WriteNull(key);
			}

			writer.WriteEndObject();
			WriteMeta(writer, value.Meta);
			writer.WriteEndObject();
		}

		private static void WriteBucketAggregate(Utf8JsonWriter writer, BucketAggregate value, JsonSerializerOptions options)
		{
			writer.WriteStartObject();

			if (value.DocCountErrorUpperBound.HasValue)
				writer.WriteNumber("doc_count_error_upper_bound", value.DocCountErrorUpperBound.Value);

			if (value.SumOtherDocCount.HasValue)
				writer.WriteNumber("sum_other_doc_count", value.SumOtherDocCount.Value);

			writer.WriteStartArray("buckets");
			// Bucket serialization will be expanded with dedicated bucket converters
			writer.WriteEndArray();

			WriteMeta(writer, value.Meta);
			writer.WriteEndObject();
		}

		private static void WriteSingleBucketAggregate(
			Utf8JsonWriter writer, SingleBucketAggregate value, JsonSerializerOptions options)
		{
			writer.WriteStartObject();
			writer.WriteNumber("doc_count", value.DocCount);

			// Write sub-aggregations
			foreach (var kvp in (IReadOnlyDictionary<string, IAggregate>)value)
			{
				if (kvp.Value == null) continue;
				writer.WritePropertyName(kvp.Key);
				JsonSerializer.Serialize(writer, kvp.Value, kvp.Value.GetType(), options);
			}

			WriteMeta(writer, value.Meta);
			writer.WriteEndObject();
		}

		private static void WriteMeta(Utf8JsonWriter writer, IReadOnlyDictionary<string, object> meta)
		{
			if (meta == null || meta.Count == 0)
				return;

			writer.WriteStartObject("meta");
			foreach (var kvp in meta)
			{
				writer.WritePropertyName(kvp.Key);
				WriteObjectValue(writer, kvp.Value);
			}
			writer.WriteEndObject();
		}

		private static void WriteObjectValue(Utf8JsonWriter writer, object value)
		{
			switch (value)
			{
				case null:
					writer.WriteNullValue();
					break;
				case string s:
					writer.WriteStringValue(s);
					break;
				case bool b:
					writer.WriteBooleanValue(b);
					break;
				case int i:
					writer.WriteNumberValue(i);
					break;
				case long l:
					writer.WriteNumberValue(l);
					break;
				case double d:
					writer.WriteNumberValue(d);
					break;
				case float f:
					writer.WriteNumberValue(f);
					break;
				case decimal dec:
					writer.WriteNumberValue(dec);
					break;
				default:
					writer.WriteStringValue(value.ToString());
					break;
			}
		}

		private static IReadOnlyDictionary<string, object> DeserializeMeta(JsonElement element)
		{
			if (element.ValueKind != JsonValueKind.Object)
				return EmptyReadOnly<string, object>.Dictionary;

			var meta = new Dictionary<string, object>();
			foreach (var prop in element.EnumerateObject())
			{
				meta[prop.Name] = AggregationContainerConverter.GetJsonElementValue(prop.Value);
			}
			return meta;
		}
	}
}
