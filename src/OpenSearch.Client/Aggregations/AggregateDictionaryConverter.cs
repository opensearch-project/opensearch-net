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
	/// System.Text.Json replacement for the legacy Utf8Json <c>AggregateDictionaryFormatter</c>. Reads a JSON
	/// object of aggregation responses into an <see cref="AggregateDictionary"/>. Supports the <c>typed_keys</c>
	/// response format where each key is returned as <c>&lt;type&gt;#&lt;name&gt;</c>; the dictionary is keyed by the
	/// bare name.
	///
	/// Each aggregate value is delegated to <see cref="JsonSerializer.Deserialize{TValue}(ref Utf8JsonReader, JsonSerializerOptions)"/>
	/// for <see cref="IAggregate"/>, i.e. it relies on an <see cref="IAggregate"/> converter being registered on the
	/// supplied <see cref="JsonSerializerOptions"/>. Serialization is not supported (mirrors the legacy formatter
	/// which threw <see cref="NotSupportedException"/> on write).
	/// </summary>
	internal class AggregateDictionaryConverter : JsonConverter<AggregateDictionary>
	{
		public override AggregateDictionary Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			var dictionary = new Dictionary<string, IAggregate>();

			if (reader.TokenType != JsonTokenType.StartObject)
			{
				reader.Skip();
				return new AggregateDictionary(dictionary);
			}

			while (reader.Read())
			{
				if (reader.TokenType == JsonTokenType.EndObject)
					break;

				if (reader.TokenType != JsonTokenType.PropertyName)
					throw new JsonException("Expected property name.");

				var typedProperty = reader.GetString();
				reader.Read();

				if (string.IsNullOrEmpty(typedProperty))
				{
					reader.Skip();
					continue;
				}

				var tokens = AggregateDictionary.TypedKeyTokens(typedProperty);
				var name = tokens.Length > 1 ? tokens[1] : tokens[0];

				var aggregate = JsonSerializer.Deserialize<IAggregate>(ref reader, options);
				dictionary[name] = aggregate;
			}

			return new AggregateDictionary(dictionary);
		}

		public override void Write(Utf8JsonWriter writer, AggregateDictionary value, JsonSerializerOptions options) =>
			throw new NotSupportedException();
	}
}
