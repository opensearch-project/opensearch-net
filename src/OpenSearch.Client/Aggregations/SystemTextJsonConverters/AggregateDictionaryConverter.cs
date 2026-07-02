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
	/// Converter for <see cref="AggregateDictionary"/>.
	/// AggregateDictionary parses the response "aggregations" object from OpenSearch.
	/// <code>
	/// {
	///   "aggregations": {
	///     "avg_price": {"value": 42.5},
	///     "status_terms": {"buckets": [{"key": "active", "doc_count": 10}]}
	///   }
	/// }
	/// </code>
	/// Each key is the aggregation name (potentially prefixed with "type#" when typed_keys=true),
	/// and each value is a polymorphic aggregate result parsed by <see cref="AggregateConverter"/>.
	/// </summary>
	internal sealed class AggregateDictionaryConverter : JsonConverter<AggregateDictionary>
	{
		private readonly IConnectionSettingsValues _settings;

		public AggregateDictionaryConverter(IConnectionSettingsValues settings) => _settings = settings;

		public override AggregateDictionary Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null)
				return AggregateDictionary.Default;

			if (reader.TokenType != JsonTokenType.StartObject)
			{
				reader.Skip();
				return AggregateDictionary.Default;
			}

			var dictionary = new Dictionary<string, IAggregate>();

			while (reader.Read())
			{
				if (reader.TokenType == JsonTokenType.EndObject)
					break;

				if (reader.TokenType != JsonTokenType.PropertyName)
					throw new JsonException("Expected PropertyName in AggregateDictionary");

				var key = reader.GetString();
				reader.Read(); // Move to value

				var aggregate = JsonSerializer.Deserialize<IAggregate>(ref reader, options);
				if (aggregate != null)
					dictionary[key] = aggregate;
			}

			return new AggregateDictionary(dictionary);
		}

		public override void Write(Utf8JsonWriter writer, AggregateDictionary value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			writer.WriteStartObject();

			foreach (var kvp in (IReadOnlyDictionary<string, IAggregate>)value)
			{
				if (kvp.Value == null)
					continue;

				writer.WritePropertyName(kvp.Key);
				JsonSerializer.Serialize(writer, kvp.Value, kvp.Value.GetType(), options);
			}

			writer.WriteEndObject();
		}
	}
}
