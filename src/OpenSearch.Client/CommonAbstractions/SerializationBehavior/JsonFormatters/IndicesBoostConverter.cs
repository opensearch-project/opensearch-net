/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Collections.Generic;
using System.Text.Json;

namespace OpenSearch.Client
{
	/// <summary>
	/// System.Text.Json replacement for the legacy Utf8Json <c>IndicesBoostFormatter</c>. Serializes an
	/// <c>IDictionary&lt;IndexName, double&gt;</c> as a JSON array of single-key objects
	/// (<c>[{"index":boost}]</c>), resolving each <see cref="IndexName"/> key through the runtime
	/// <c>Inferrer</c> — hence a <see cref="SettingsAwareConverter{T}"/>.
	/// </summary>
	internal class IndicesBoostConverter : SettingsAwareConverter<IDictionary<IndexName, double>>
	{
		public IndicesBoostConverter(IConnectionSettingsValues settings) : base(settings) { }

		public override void Write(Utf8JsonWriter writer, IDictionary<IndexName, double> value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			writer.WriteStartArray();
			foreach (var entry in value)
			{
				writer.WriteStartObject();
				var indexName = Settings.Inferrer.IndexName(entry.Key);
				writer.WritePropertyName(indexName);
				writer.WriteNumberValue(entry.Value);
				writer.WriteEndObject();
			}
			writer.WriteEndArray();
		}

		public override IDictionary<IndexName, double> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			switch (reader.TokenType)
			{
				case JsonTokenType.Null:
					return null;
				case JsonTokenType.StartObject:
					return JsonSerializer.Deserialize<Dictionary<IndexName, double>>(ref reader, options);
				case JsonTokenType.StartArray:
					var dictionary = new Dictionary<IndexName, double>();
					while (reader.Read())
					{
						if (reader.TokenType == JsonTokenType.EndArray)
							return dictionary;

						// Each element is a single-key object { "index": boost }.
						if (reader.TokenType != JsonTokenType.StartObject)
							throw new JsonException("Expected object when reading indices boost entry.");

						reader.Read(); // property name
						IndexName indexName = reader.GetString(); // implicit string -> IndexName
						reader.Read(); // value
						dictionary[indexName] = reader.GetDouble();
						reader.Read(); // end object
					}
					throw new JsonException("Unexpected end of JSON when reading indices boost array.");
				default:
					reader.Skip();
					return null;
			}
		}
	}
}
