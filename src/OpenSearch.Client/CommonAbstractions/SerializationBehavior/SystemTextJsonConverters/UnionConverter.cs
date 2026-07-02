/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

#nullable enable

using System;
#nullable enable

using System.Text.Json;
#nullable enable

using System.Text.Json.Serialization;

namespace OpenSearch.Client.SystemTextJsonConverters;

internal sealed class UnionConverterFactory : JsonConverterFactory
{
	public override bool CanConvert(Type typeToConvert) =>
		typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(Union<,>);

	public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
	{
		var typeArgs = typeToConvert.GetGenericArguments();
		var converterType = typeof(UnionConverter<,>).MakeGenericType(typeArgs);
		return (JsonConverter?)Activator.CreateInstance(converterType);
	}
}

internal sealed class UnionConverter<TFirst, TSecond> : JsonConverter<Union<TFirst, TSecond>>
{
	public override Union<TFirst, TSecond>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType == JsonTokenType.Null)
			return null;

		// Clone the reader so we can attempt to deserialize as TFirst without consuming the original
		var readerClone = reader;

		try
		{
			var first = JsonSerializer.Deserialize<TFirst>(ref readerClone, options);
			if (first != null)
			{
				reader = readerClone;
				return new Union<TFirst, TSecond>(first);
			}
		}
		catch (JsonException)
		{
			// TFirst deserialization failed, try TSecond
		}

		try
		{
			var second = JsonSerializer.Deserialize<TSecond>(ref reader, options);
			return second != null ? new Union<TFirst, TSecond>(second) : null;
		}
		catch (JsonException ex)
		{
			throw new JsonException(
				$"Cannot deserialize Union<{typeof(TFirst).Name}, {typeof(TSecond).Name}> from the given JSON.", ex);
		}
	}

	public override void Write(Utf8JsonWriter writer, Union<TFirst, TSecond> value, JsonSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNullValue();
			return;
		}

		value.Match(
			first => JsonSerializer.Serialize(writer, first, options),
			second => JsonSerializer.Serialize(writer, second, options)
		);
	}
}
