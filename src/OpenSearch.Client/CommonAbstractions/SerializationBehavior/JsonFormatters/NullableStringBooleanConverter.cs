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
	/// System.Text.Json replacement for the legacy Utf8Json <c>NullableStringBooleanFormatter</c>. Handles a
	/// <see cref="Nullable{Boolean}"/> whose value may arrive from OpenSearch as a JSON boolean, a string
	/// containing a boolean (e.g. <c>"true"</c>), or null.
	/// </summary>
	internal class NullableStringBooleanConverter : JsonConverter<bool?>
	{
		public override bool? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			switch (reader.TokenType)
			{
				case JsonTokenType.Null:
					return null;
				case JsonTokenType.True:
				case JsonTokenType.False:
					return reader.GetBoolean();
				case JsonTokenType.String:
					var s = reader.GetString();
					if (!bool.TryParse(s, out var b))
						throw new JsonException($"Cannot parse {typeof(bool).FullName} from: {s}");

					return b;
				default:
					throw new JsonException($"Cannot parse {typeof(bool).FullName} from: {reader.TokenType}");
			}
		}

		public override void Write(Utf8JsonWriter writer, bool? value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			writer.WriteBooleanValue(value.Value);
		}
	}
}
