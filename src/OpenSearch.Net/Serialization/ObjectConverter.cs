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

namespace OpenSearch.Net
{
	/// <summary>
	/// A <see cref="System.Text.Json"/> converter for <see cref="object"/>-typed values (dynamic
	/// payloads such as script <c>params</c>, aggregation <c>meta</c>, <c>_source</c> fragments and
	/// <c>Dictionary&lt;string, object&gt;</c> values), part of the migration tracked by #388.
	/// <para>
	/// By default <c>System.Text.Json</c> deserializes <see cref="object"/> to
	/// <see cref="JsonElement"/>, which neither matches the CLR shapes the client's code expects nor
	/// round-trips through the rest of the pipeline. This reproduces the vendored Utf8Json
	/// <c>PrimitiveObjectFormatter</c> mapping instead:
	/// </para>
	/// <list type="bullet">
	/// <item>JSON object → <see cref="Dictionary{TKey,TValue}"/> of <c>string</c> to <c>object</c>.</item>
	/// <item>JSON array → <see cref="List{T}"/> of <c>object</c>.</item>
	/// <item>Integral number → <see cref="long"/>; other numbers → <see cref="double"/>.</item>
	/// <item>String → <see cref="string"/>; <c>true</c>/<c>false</c> → <see cref="bool"/>; null → <c>null</c>.</item>
	/// </list>
	/// <para>Writing delegates to the value's runtime type, so the shapes above serialize as expected.</para>
	/// </summary>
	public class ObjectConverter : JsonConverter<object>
	{
		/// <summary> A shared instance of the converter. </summary>
		public static readonly ObjectConverter Instance = new();

		/// <inheritdoc />
		public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
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
					// Box long and double separately: a ternary would unify both to double and lose
					// the integral distinction (matching Utf8Json's IsLong check). An integral outside
					// Int64 range falls through to double (and loses precision) -- this matches the
					// vendored Utf8Json behavior for dynamic `object` payloads.
					if (reader.TryGetInt64(out var l)) return l;
					return reader.GetDouble();
				case JsonTokenType.StartArray:
				{
					var list = new List<object>();
					while (reader.Read())
					{
						if (reader.TokenType == JsonTokenType.EndArray) return list;
						list.Add(Read(ref reader, typeof(object), options));
					}
					throw new JsonException("Unexpected end of JSON array.");
				}
				case JsonTokenType.StartObject:
				{
					var dictionary = new Dictionary<string, object>();
					while (reader.Read())
					{
						if (reader.TokenType == JsonTokenType.EndObject) return dictionary;

						var key = reader.GetString();
						reader.Read();
						dictionary[key] = Read(ref reader, typeof(object), options);
					}
					throw new JsonException("Unexpected end of JSON object.");
				}
				default:
					throw new JsonException($"Unexpected token {reader.TokenType} when reading object.");
			}
		}

		/// <inheritdoc />
		public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			var runtimeType = value.GetType();
			if (runtimeType == typeof(object))
			{
				writer.WriteStartObject();
				writer.WriteEndObject();
				return;
			}

			// Serialize by the concrete runtime type; runtimeType != object, so this does not recurse.
			JsonSerializer.Serialize(writer, value, runtimeType, options);
		}
	}
}
