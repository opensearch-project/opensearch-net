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
	/// System.Text.Json replacement for the legacy Utf8Json <c>BucketsPathFormatter</c>.
	///
	/// <see cref="IBucketsPath"/> is a union of JSON shapes:
	/// <list type="bullet">
	/// <item><description>a <c>string</c> becomes a <see cref="SingleBucketsPath"/>;</description></item>
	/// <item><description>an object becomes a <see cref="MultiBucketsPath"/> (a string→string dictionary);</description></item>
	/// <item><description>any other token (including an array or <c>null</c>) yields <c>null</c>.</description></item>
	/// </list>
	/// On write a <see cref="SingleBucketsPath"/> is emitted as its string, a <see cref="MultiBucketsPath"/> as a JSON
	/// object of its key/value pairs, and anything else as <c>null</c> — matching the legacy formatter exactly.
	/// </summary>
	internal class BucketsPathConverter : JsonConverter<IBucketsPath>
	{
		public override IBucketsPath Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			switch (reader.TokenType)
			{
				case JsonTokenType.String:
					return new SingleBucketsPath(reader.GetString());
				case JsonTokenType.StartObject:
					var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(ref reader, options);
					return new MultiBucketsPath(dict);
				default:
					reader.Skip();
					return null;
			}
		}

		public override void Write(Utf8JsonWriter writer, IBucketsPath value, JsonSerializerOptions options)
		{
			switch (value)
			{
				case SingleBucketsPath single:
					writer.WriteStringValue(single.BucketsPath);
					break;
				case MultiBucketsPath multi:
					writer.WriteStartObject();
					foreach (var kv in multi)
					{
						writer.WritePropertyName(kv.Key);
						writer.WriteStringValue(kv.Value);
					}
					writer.WriteEndObject();
					break;
				default:
					writer.WriteNullValue();
					break;
			}
		}
	}
}
