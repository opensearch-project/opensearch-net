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
	/// System.Text.Json replacement for the legacy Utf8Json <c>TimeFormatter</c>. A <see cref="Time"/>
	/// may be serialized either as a JSON string (e.g. <c>"5m"</c>) or as a numeric millisecond value,
	/// with the special values <c>-1</c> and <c>0</c> mapped to <see cref="Time.MinusOne"/> and
	/// <see cref="Time.Zero"/> respectively.
	/// </summary>
	internal class TimeConverter : JsonConverter<Time>
	{
		public override Time Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			switch (reader.TokenType)
			{
				case JsonTokenType.String:
					return new Time(reader.GetString());
				case JsonTokenType.Number:
					var milliseconds = reader.GetInt64();
					if (milliseconds == -1)
						return Time.MinusOne;
					if (milliseconds == 0)
						return Time.Zero;

					return new Time(milliseconds);
				default:
					reader.Skip();
					return null;
			}
		}

		public override void Write(Utf8JsonWriter writer, Time value, JsonSerializerOptions options)
		{
			if (value == Time.MinusOne) writer.WriteNumberValue(-1);
			else if (value == Time.Zero) writer.WriteNumberValue(0);
			else if (value.Factor.HasValue && value.Interval.HasValue) writer.WriteStringValue(value.ToString());
			else if (value.Milliseconds != null) writer.WriteNumberValue((long)value.Milliseconds);
		}
	}
}
