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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenSearch.Client
{
	/// <summary>
	/// System.Text.Json replacement for the legacy Utf8Json <c>CompositeKeyFormatter</c>.
	///
	/// A <see cref="CompositeKey"/> is a string-keyed dictionary of heterogeneous scalar values (the value of each
	/// composite-aggregation source for a bucket). The legacy formatter delegated to the verbatim key-preserving
	/// dictionary formatter, which used the Utf8Json primitive-object formatter for each value. Two behaviours of that
	/// primitive formatter are load-bearing and are reproduced exactly here:
	/// <list type="bullet">
	/// <item><description><b>Number precision.</b> An integral JSON number is boxed as a <see cref="long"/> (checked
	/// before falling back to <see cref="double"/>), so <c>1</c> round-trips as <c>1</c> and not <c>1.0</c>. Only a
	/// number with a fractional part / exponent becomes a <see cref="double"/>. This matches
	/// <c>PrimitiveObjectFormatter</c>'s <c>IsLong</c>-first check and is what the <see cref="CompositeKey"/> typed
	/// accessors (e.g. <c>TryGetValue(key, out long)</c>) depend on.</description></item>
	/// <item><description><b>Null preservation.</b> Null values are kept (the legacy used the
	/// <c>PreservingNull</c> variant that never skips a value), so a key with a JSON <c>null</c> value survives the
	/// round-trip.</description></item>
	/// </list>
	/// Nested objects/arrays are supported for completeness (boxed as <c>Dictionary&lt;string, object&gt;</c> /
	/// <c>List&lt;object&gt;</c>), mirroring the recursive primitive-object formatter. A JSON <c>null</c> root yields a
	/// null <see cref="CompositeKey"/>. This converter is not settings-aware — the legacy value formatter used no
	/// connection settings.
	/// </summary>
	internal class CompositeKeyConverter : JsonConverter<CompositeKey>
	{
		public override CompositeKey Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null)
				return null;

			using var doc = JsonDocument.ParseValue(ref reader);
			var root = doc.RootElement;

			if (root.ValueKind != JsonValueKind.Object)
				return null;

			var dictionary = new Dictionary<string, object>();
			foreach (var property in root.EnumerateObject())
				dictionary[property.Name] = ReadValue(property.Value);

			return new CompositeKey(dictionary);
		}

		// Mirrors PrimitiveObjectFormatter.Deserialize: numbers are long when integral (checked first, to avoid
		// floating-point rounding) else double; nested objects/arrays recurse; string/bool/null map directly.
		private static object ReadValue(JsonElement element)
		{
			switch (element.ValueKind)
			{
				case JsonValueKind.Object:
					var dict = new Dictionary<string, object>();
					foreach (var p in element.EnumerateObject())
						dict[p.Name] = ReadValue(p.Value);
					return dict;
				case JsonValueKind.Array:
					var list = new List<object>();
					foreach (var item in element.EnumerateArray())
						list.Add(ReadValue(item));
					return list;
				case JsonValueKind.Number:
					return element.TryGetInt64(out var l) ? l : (object)element.GetDouble();
				case JsonValueKind.String:
					return element.GetString();
				case JsonValueKind.True:
				case JsonValueKind.False:
					return element.GetBoolean();
				default:
					return null;
			}
		}

		public override void Write(Utf8JsonWriter writer, CompositeKey value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			writer.WriteStartObject();
			foreach (var entry in value)
			{
				writer.WritePropertyName(entry.Key);
				WriteValue(writer, entry.Value);
			}
			writer.WriteEndObject();
		}

		// Mirrors PrimitiveObjectFormatter.Serialize for the value types a CompositeKey can hold, preserving the
		// long-vs-double distinction produced on read (and by users).
		private static void WriteValue(Utf8JsonWriter writer, object value)
		{
			switch (value)
			{
				case null:
					writer.WriteNullValue();
					break;
				case bool b:
					writer.WriteBooleanValue(b);
					break;
				case string s:
					writer.WriteStringValue(s);
					break;
				case sbyte sb:
					writer.WriteNumberValue(sb);
					break;
				case byte bt:
					writer.WriteNumberValue(bt);
					break;
				case short sh:
					writer.WriteNumberValue(sh);
					break;
				case ushort us:
					writer.WriteNumberValue(us);
					break;
				case int i:
					writer.WriteNumberValue(i);
					break;
				case uint ui:
					writer.WriteNumberValue(ui);
					break;
				case long l:
					writer.WriteNumberValue(l);
					break;
				case ulong ul:
					writer.WriteNumberValue(ul);
					break;
				case float f:
					writer.WriteNumberValue(f);
					break;
				case double d:
					writer.WriteNumberValue(d);
					break;
				case decimal m:
					writer.WriteNumberValue(m);
					break;
				case DateTime dt:
					writer.WriteStringValue(dt.ToString("o", CultureInfo.InvariantCulture));
					break;
				case IDictionary<string, object> dict:
					writer.WriteStartObject();
					foreach (var kv in dict)
					{
						writer.WritePropertyName(kv.Key);
						WriteValue(writer, kv.Value);
					}
					writer.WriteEndObject();
					break;
				case IEnumerable enumerable:
					writer.WriteStartArray();
					foreach (var item in enumerable)
						WriteValue(writer, item);
					writer.WriteEndArray();
					break;
				default:
					writer.WriteStringValue(value.ToString());
					break;
			}
		}
	}
}
