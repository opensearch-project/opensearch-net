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
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenSearch.Client
{
	/// <summary>
	/// System.Text.Json replacement for the legacy Utf8Json <c>NullableTimeSpanTicksFormatter</c>. A
	/// nullable <see cref="TimeSpan"/> is serialized as its <see cref="TimeSpan.Ticks"/> (a JSON number)
	/// or JSON null, and may be deserialized from a JSON number of ticks, a JSON string parseable by
	/// <see cref="TimeSpan.Parse(string)"/>, or JSON null.
	/// </summary>
	internal class NullableTimeSpanTicksConverter : JsonConverter<TimeSpan?>
	{
		public override TimeSpan? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			var token = reader.TokenType;
			switch (token)
			{
				case JsonTokenType.Null:
					return null;
				case JsonTokenType.String: return TimeSpan.Parse(reader.GetString());
				case JsonTokenType.Number: return new TimeSpan(reader.GetInt64());
			}
			throw new JsonException($"Cannot convert token of type {token} to {nameof(TimeSpan)}?.");
		}

		public override void Write(Utf8JsonWriter writer, TimeSpan? value, JsonSerializerOptions options)
		{
			if (value == null)
				writer.WriteNullValue();
			else
				writer.WriteNumberValue(value.Value.Ticks);
		}
	}
}
