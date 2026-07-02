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
	/// Converter for <see cref="AggregationDictionary"/>.
	/// AggregationDictionary is a named collection of aggregations in a request:
	/// <code>
	/// {
	///   "avg_price": {"avg": {"field": "price"}},
	///   "status_terms": {"terms": {"field": "status"}, "aggs": {...}}
	/// }
	/// </code>
	/// Each key is the user-defined aggregation name, each value is an AggregationContainer.
	/// </summary>
	internal sealed class AggregationDictionaryConverter : JsonConverter<AggregationDictionary>
	{
		private readonly IConnectionSettingsValues _settings;

		public AggregationDictionaryConverter(IConnectionSettingsValues settings) => _settings = settings;

		public override AggregationDictionary Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null)
				return null;

			if (reader.TokenType != JsonTokenType.StartObject)
				throw new JsonException($"Expected StartObject for AggregationDictionary but got {reader.TokenType}");

			var dictionary = new Dictionary<string, IAggregationContainer>();

			while (reader.Read())
			{
				if (reader.TokenType == JsonTokenType.EndObject)
					break;

				if (reader.TokenType != JsonTokenType.PropertyName)
					throw new JsonException("Expected PropertyName in AggregationDictionary");

				var aggregationName = reader.GetString();
				reader.Read(); // Move to value

				var container = JsonSerializer.Deserialize<AggregationContainer>(ref reader, options);
				dictionary[aggregationName] = container;
			}

			return new AggregationDictionary(dictionary);
		}

		public override void Write(Utf8JsonWriter writer, AggregationDictionary value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			writer.WriteStartObject();

			foreach (var kvp in (IDictionary<string, IAggregationContainer>)value)
			{
				if (kvp.Value == null)
					continue;

				writer.WritePropertyName(kvp.Key);
				AggregationContainerConverter.WriteContainer(writer, kvp.Value, options);
			}

			writer.WriteEndObject();
		}
	}
}
