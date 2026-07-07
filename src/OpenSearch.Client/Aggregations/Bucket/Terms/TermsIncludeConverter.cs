/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/
/*
* Modifications Copyright OpenSearch Contributors. See
* GitHub history for details.
*
*  Licensed to Elasticsearch B.V. under one or more contributor
*  license agreements. See the NOTICE file distributed with
*  this work for additional information regarding copyright
*  ownership. Elasticsearch B.V. licenses this file to you under
*  the Apache License, Version 2.0 (the "License"); you may
*  not use this file except in compliance with the License.
*  You may obtain a copy of the License at
*
* 	http://www.apache.org/licenses/LICENSE-2.0
*
*  Unless required by applicable law or agreed to in writing,
*  software distributed under the License is distributed on an
*  "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
*  KIND, either express or implied.  See the License for the
*  specific language governing permissions and limitations
*  under the License.
*/

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenSearch.Client
{
	/// <summary>
	/// System.Text.Json replacement for the legacy Utf8Json <c>TermsIncludeFormatter</c>.
	///
	/// A <see cref="TermsInclude"/> value may be serialized as one of three JSON shapes: an array of
	/// strings (exact terms), a string (regular expression pattern), or an object with
	/// <c>partition</c>/<c>num_partitions</c> (partitioned terms).
	/// </summary>
	internal class TermsIncludeConverter : JsonConverter<TermsInclude>
	{
		public override TermsInclude Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			switch (reader.TokenType)
			{
				case JsonTokenType.Null:
					return null;
				case JsonTokenType.String:
					return new TermsInclude(reader.GetString());
				case JsonTokenType.StartArray:
					var values = JsonSerializer.Deserialize<IEnumerable<string>>(ref reader, options);
					return new TermsInclude(values);
				case JsonTokenType.StartObject:
					long partition = 0;
					long numberOfPartitions = 0;
					while (reader.Read())
					{
						if (reader.TokenType == JsonTokenType.EndObject)
							break;

						if (reader.TokenType != JsonTokenType.PropertyName)
							throw new JsonException($"Expected property name when deserializing {nameof(TermsInclude)}.");

						var propertyName = reader.GetString();
						reader.Read();
						switch (propertyName)
						{
							case "partition":
								partition = reader.GetInt64();
								break;
							case "num_partitions":
								numberOfPartitions = reader.GetInt64();
								break;
							default:
								reader.Skip();
								break;
						}
					}

					return new TermsInclude(partition, numberOfPartitions);
				default:
					throw new JsonException($"Unexpected token {reader.TokenType} when deserializing {nameof(TermsInclude)}");
			}
		}

		public override void Write(Utf8JsonWriter writer, TermsInclude value, JsonSerializerOptions options)
		{
			if (value == null)
				writer.WriteNullValue();
			else if (value.Values != null)
				JsonSerializer.Serialize(writer, value.Values, options);
			else if (value.Partition.HasValue && value.NumberOfPartitions.HasValue)
			{
				writer.WriteStartObject();
				writer.WriteNumber("partition", value.Partition.Value);
				writer.WriteNumber("num_partitions", value.NumberOfPartitions.Value);
				writer.WriteEndObject();
			}
			else
				writer.WriteStringValue(value.Pattern);
		}
	}
}
