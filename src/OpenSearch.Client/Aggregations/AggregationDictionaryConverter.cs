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
	/// A <see cref="System.Text.Json"/> converter for <see cref="AggregationDictionary"/> (the
	/// user-named <c>aggs</c> map), replacing the vendored Utf8Json formatter as part of #388.
	/// Serialized as <c>{ "&lt;name&gt;": { &lt;aggregation&gt; }, … }</c>; each value is an
	/// <see cref="IAggregationContainer"/> handled via its <c>[ReadAs]</c> mapping.
	/// </summary>
	internal sealed class AggregationDictionaryConverter : JsonConverter<AggregationDictionary>
	{
		public override AggregationDictionary Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null) return null;

			using var document = JsonDocument.ParseValue(ref reader);
			var root = document.RootElement;
			if (root.ValueKind != JsonValueKind.Object) return null;

			var dictionary = new Dictionary<string, IAggregationContainer>();
			foreach (var member in root.EnumerateObject())
				dictionary[member.Name] = member.Value.Deserialize<IAggregationContainer>(options);

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
			foreach (var entry in (IEnumerable<KeyValuePair<string, IAggregationContainer>>)value)
			{
				writer.WritePropertyName(entry.Key);
				JsonSerializer.Serialize(writer, entry.Value, options);
			}
			writer.WriteEndObject();
		}
	}
}
