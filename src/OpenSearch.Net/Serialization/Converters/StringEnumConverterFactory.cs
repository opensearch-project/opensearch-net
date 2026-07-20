/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;
using OpenSearch.Net;

namespace OpenSearch.Net.Serialization.Converters
{
	/// <summary>
	/// A <see cref="JsonConverterFactory"/> that produces string-based enum converters for
	/// enum types decorated with <see cref="StringEnumAttribute"/>. Mirrors the legacy Utf8Json
	/// <c>EnumFormatter</c>: the serialized name is <see cref="EnumMemberAttribute"/>.Value, else
	/// <see cref="DataMemberAttribute"/>.Name, else the verbatim enum field name (no camelCasing).
	/// </summary>
	public class StringEnumConverterFactory : JsonConverterFactory
	{
		/// <inheritdoc />
		public override bool CanConvert(Type typeToConvert)
		{
			var enumType = GetEnumType(typeToConvert);
			return enumType != null && enumType.GetCustomAttribute<StringEnumAttribute>() != null;
		}

		/// <inheritdoc />
		public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
		{
			var enumType = GetEnumType(typeToConvert);
			if (enumType == null)
				return null;

			var isNullable = IsNullableEnum(typeToConvert);
			var converterType = isNullable
				? typeof(NullableStringEnumConverter<>).MakeGenericType(enumType)
				: typeof(StringEnumConverter<>).MakeGenericType(enumType);

			return (JsonConverter)Activator.CreateInstance(converterType);
		}

		/// <summary>
		/// Builds a string-enum converter for the given (nullable) enum type regardless of whether the enum type
		/// carries <see cref="StringEnumAttribute"/>. Used for a member marked [StringEnum] whose enum type is not
		/// itself so marked (e.g. HttpStatusCode).
		/// </summary>
		public static JsonConverter CreateForType(Type memberType)
		{
			var enumType = GetEnumType(memberType);
			if (enumType == null)
				return null;

			var converterType = IsNullableEnum(memberType)
				? typeof(NullableStringEnumConverter<>).MakeGenericType(enumType)
				: typeof(StringEnumConverter<>).MakeGenericType(enumType);
			return (JsonConverter)Activator.CreateInstance(converterType);
		}

		private static Type GetEnumType(Type type)
		{
			if (type.IsEnum) return type;

			var underlying = Nullable.GetUnderlyingType(type);
			if (underlying != null && underlying.IsEnum) return underlying;

			return null;
		}

		private static bool IsNullableEnum(Type type) =>
			Nullable.GetUnderlyingType(type)?.IsEnum == true;

		private class StringEnumConverter<TEnum> : JsonConverter<TEnum> where TEnum : struct, Enum
		{
			private static readonly ConcurrentDictionary<TEnum, string> EnumToString = new ConcurrentDictionary<TEnum, string>();
			private static readonly ConcurrentDictionary<string, TEnum> StringToEnum = new ConcurrentDictionary<string, TEnum>(StringComparer.OrdinalIgnoreCase);

			static StringEnumConverter()
			{
				foreach (var field in typeof(TEnum).GetFields(BindingFlags.Public | BindingFlags.Static))
				{
					var enumValue = (TEnum)field.GetValue(null);
					var enumMember = field.GetCustomAttribute<EnumMemberAttribute>();
					var dataMember = field.GetCustomAttribute<DataMemberAttribute>();
					// Mirror the legacy Utf8Json EnumFormatter precedence: [EnumMember].Value, then [DataMember].Name,
					// then the VERBATIM field name (no camelCasing). Utf8Json wrote the raw field name, so "BellyUp"
					// stays "BellyUp" (not "bellyUp").
					var serializedName = enumMember?.Value ?? dataMember?.Name ?? field.Name;

					EnumToString[enumValue] = serializedName;
					StringToEnum[serializedName] = enumValue;
					// Also map the original field name for deserialization flexibility
					StringToEnum[field.Name] = enumValue;
				}
			}

			public override TEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
			{
				switch (reader.TokenType)
				{
					case JsonTokenType.String:
						var str = reader.GetString();
						if (StringToEnum.TryGetValue(str, out var result))
							return result;
						if (Enum.TryParse<TEnum>(str, ignoreCase: true, out var parsed))
							return parsed;
						throw new JsonException($"Unable to convert \"{str}\" to enum type {typeof(TEnum).Name}.");
					case JsonTokenType.Number:
						var intVal = reader.GetInt32();
						return (TEnum)Enum.ToObject(typeof(TEnum), intVal);
					default:
						throw new JsonException($"Unexpected token type {reader.TokenType} when reading enum {typeof(TEnum).Name}.");
				}
			}

			public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options)
			{
				var str = EnumToString.GetOrAdd(value, v => v.ToString());
				writer.WriteStringValue(str);
			}
		}

		private class NullableStringEnumConverter<TEnum> : JsonConverter<TEnum?> where TEnum : struct, Enum
		{
			private readonly StringEnumConverter<TEnum> _innerConverter = new StringEnumConverter<TEnum>();

			public override TEnum? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
			{
				if (reader.TokenType == JsonTokenType.Null)
					return null;

				return _innerConverter.Read(ref reader, typeof(TEnum), options);
			}

			public override void Write(Utf8JsonWriter writer, TEnum? value, JsonSerializerOptions options)
			{
				if (!value.HasValue)
				{
					writer.WriteNullValue();
					return;
				}

				_innerConverter.Write(writer, value.Value, options);
			}
		}
	}
}
