/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenSearch.Client
{
	/// <summary>
	/// System.Text.Json replacement for the legacy Utf8Json <c>UnionFormatter&lt;TFirst, TSecond&gt;</c>.
	///
	/// A union value may be serialized as either <typeparamref name="TFirst"/> or <typeparamref name="TSecond"/>.
	/// On read we must try one type and, if it does not fit, fall back to the other. System.Text.Json's
	/// <see cref="Utf8JsonReader"/> is forward-only and cannot be rewound, so — unlike the Utf8Json version which
	/// re-read a byte segment — we parse the value into a <see cref="JsonDocument"/> once and attempt each type
	/// against that buffered DOM.
	/// </summary>
	internal class UnionConverter<TFirst, TSecond> : JsonConverter<Union<TFirst, TSecond>>
	{
		private readonly bool _attemptTSecondIfTFirstIsNull;

		public UnionConverter() => _attemptTSecondIfTFirstIsNull = false;

		public UnionConverter(bool attemptTSecondIfTFirstIsNull) => _attemptTSecondIfTFirstIsNull = attemptTSecondIfTFirstIsNull;

		public override Union<TFirst, TSecond> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null)
				return null;

			using var doc = JsonDocument.ParseValue(ref reader);
			var raw = doc.RootElement.GetRawText();

			if (TryRead<TFirst>(raw, options, out var first))
			{
				if (first == null && _attemptTSecondIfTFirstIsNull)
				{
					if (TryRead<TSecond>(raw, options, out var second))
						return second;
				}
				else
				{
					return first;
				}
			}
			else if (TryRead<TSecond>(raw, options, out var second))
			{
				return second;
			}

			return null;
		}

		public override void Write(Utf8JsonWriter writer, Union<TFirst, TSecond> value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			switch (value.Tag)
			{
				case 0:
					JsonSerializer.Serialize(writer, value.Item1, options);
					break;
				case 1:
					JsonSerializer.Serialize(writer, value.Item2, options);
					break;
				default:
					throw new Exception($"Unrecognized tag value: {value.Tag}");
			}
		}

		private static bool TryRead<T>(string raw, JsonSerializerOptions options, out T value)
		{
			try
			{
				value = JsonSerializer.Deserialize<T>(raw, options);
				return true;
			}
			catch
			{
				value = default;
				return false;
			}
		}
	}

	/// <summary>
	/// Builds the <see cref="UnionConverter{TFirst,TSecond}"/> for every closed <see cref="Union{TFirst,TSecond}"/>.
	/// This reproduces the legacy engine's type-level <c>[JsonFormatter(typeof(UnionFormatter&lt;,&gt;))]</c> on
	/// <c>Union&lt;TFirst,TSecond&gt;</c>: without it System.Text.Json tries to (de)serialize the concrete Union type
	/// directly and throws NotSupportedException ("Deserialization of types without a parameterless constructor ...")
	/// or JsonException ("could not be converted to ... Union`2[...]").
	///
	/// The type-level default always uses the parameterless <c>UnionConverter</c> (attemptTSecondIfTFirstIsNull =
	/// false), matching <c>UnionFormatter&lt;,&gt;</c>'s parameterless ctor. The single site that needs the
	/// "attempt second when first is null" behaviour (DistanceFeature origin) has its own dedicated converter, so it
	/// is not affected by this global factory.
	/// </summary>
	internal class UnionConverterFactory : JsonConverterFactory
	{
		public override bool CanConvert(Type typeToConvert) =>
			typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(Union<,>);

		public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
		{
			var args = typeToConvert.GetGenericArguments();
			var converterType = typeof(UnionConverter<,>).MakeGenericType(args);
			return (JsonConverter)Activator.CreateInstance(converterType);
		}
	}
}
