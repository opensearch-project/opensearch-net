/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.Collections;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenSearch.Net.Serialization.Converters
{
	/// <summary>
	/// Writes values whose declared (static) type is <see cref="object"/> — the common case for dynamic request bodies
	/// such as <c>Dictionary&lt;string, object&gt;</c> built from a parsed YAML/JSON document. System.Text.Json writes a
	/// dictionary's <see cref="object"/> values through the declared <see cref="object"/> type, which bypasses the
	/// registered <see cref="DoubleConverter"/>/<see cref="SingleConverter"/>; an integral double like <c>3.0</c> would
	/// then be emitted as <c>3</c>, changing the value the server stores. This converter dispatches on the runtime type
	/// so boxed doubles/floats keep their trailing <c>.0</c> and nested objects/arrays recurse through the same rules.
	///
	/// On read, System.Text.Json's own <see cref="object"/> handling (or a more specific converter such as
	/// <c>DynamicDictionaryConverter</c>) applies, so this converter is write-only and defers reading to the reader's
	/// element deserialization.
	/// </summary>
	public class ObjectConverter : JsonConverter<object>
	{
		// Only claim the exact object type: derived/concrete types keep their own handling.
		public override bool CanConvert(System.Type typeToConvert) => typeToConvert == typeof(object);

		// Read object-typed values into native CLR primitives (long/double/string/bool/nested dictionary/list) rather
		// than leaking JsonElement, matching the legacy engine's dynamic object reads.
		public override object Read(ref Utf8JsonReader reader, System.Type typeToConvert, JsonSerializerOptions options)
		{
			switch (reader.TokenType)
			{
				case JsonTokenType.Null:
					return null;
				case JsonTokenType.True:
					return true;
				case JsonTokenType.False:
					return false;
				case JsonTokenType.String:
					return reader.GetString();
				case JsonTokenType.Number:
					return reader.TryGetInt64(out var l) ? l : (object)reader.GetDouble();
				case JsonTokenType.StartObject:
					var dict = new Dictionary<string, object>();
					while (reader.Read())
					{
						if (reader.TokenType == JsonTokenType.EndObject)
							return dict;
						var key = reader.GetString();
						reader.Read();
						dict[key] = Read(ref reader, typeof(object), options);
					}
					return dict;
				case JsonTokenType.StartArray:
					var list = new List<object>();
					while (reader.Read())
					{
						if (reader.TokenType == JsonTokenType.EndArray)
							return list;
						list.Add(Read(ref reader, typeof(object), options));
					}
					return list;
				default:
					using (var doc = JsonDocument.ParseValue(ref reader))
						return doc.RootElement.Clone();
			}
		}

		public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
		{
			WriteValue(writer, value, options);
		}

		private static void WriteValue(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
		{
			switch (value)
			{
				case null:
					writer.WriteNullValue();
					break;
				case string s:
					writer.WriteStringValue(s);
					break;
				case bool b:
					writer.WriteBooleanValue(b);
					break;
				case double d:
					RealNumberFormat.WriteDouble(writer, d);
					break;
				case float f:
					RealNumberFormat.WriteSingle(writer, f);
					break;
				case decimal dec:
					RealNumberFormat.WriteDecimal(writer, dec);
					break;
				// ulong is handled separately: values above long.MaxValue would overflow Convert.ToInt64. All the other
				// integer types fit in a long, so route them through the long overload (System.Text.Json and Utf8Json
				// both emit these as plain JSON numbers).
				case ulong ul:
					writer.WriteNumberValue(ul);
					break;
				case byte or sbyte or short or ushort or int or uint or long:
					writer.WriteNumberValue(System.Convert.ToInt64(value));
					break;
				case IDictionary<string, object> nested:
					writer.WriteStartObject();
					foreach (var kvp in nested)
					{
						writer.WritePropertyName(kvp.Key);
						WriteValue(writer, kvp.Value, options);
					}
					writer.WriteEndObject();
					break;
				case IDictionary nonGenericDict:
					writer.WriteStartObject();
					foreach (DictionaryEntry entry in nonGenericDict)
					{
						writer.WritePropertyName(entry.Key?.ToString() ?? string.Empty);
						WriteValue(writer, entry.Value, options);
					}
					writer.WriteEndObject();
					break;
				case IEnumerable enumerable:
					writer.WriteStartArray();
					foreach (var item in enumerable)
						WriteValue(writer, item, options);
					writer.WriteEndArray();
					break;
				default:
					var runtimeType = value.GetType();
					// A bare System.Object carries no members and its runtime type is object itself, so delegating back
					// to JsonSerializer with typeof(object) would re-enter this converter (CanConvert(object) == true)
					// and recurse until the stack overflows. Emit an empty object instead — which is also what STJ's
					// own default object handling produces for `new object()`. Should not occur with real data.
					if (runtimeType == typeof(object))
					{
						writer.WriteStartObject();
						writer.WriteEndObject();
					}
					else
					{
						JsonSerializer.Serialize(writer, value, runtimeType, options);
					}
					break;
			}
		}
	}
}
