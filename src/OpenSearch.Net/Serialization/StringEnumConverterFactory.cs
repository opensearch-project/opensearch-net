/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenSearch.Net
{
	/// <summary>
	/// A <see cref="System.Text.Json"/> converter factory that serializes enums decorated with
	/// <see cref="StringEnumAttribute"/> as strings, honoring <see cref="EnumMemberAttribute"/>
	/// values (for example <c>TokenChar.Whitespace</c> → <c>"whitespace"</c>).
	/// <para>
	/// (Named with a <c>Factory</c> suffix to avoid colliding with
	/// <c>Newtonsoft.Json.Converters.StringEnumConverter</c> in consumers that import both namespaces.)
	/// </para>
	/// <para>
	/// This reproduces the vendored Utf8Json behavior (<c>EnumFormatter</c> with
	/// <c>serializeByName: true</c>): the built-in <see cref="JsonStringEnumConverter"/> emits the
	/// CLR member name and does not read <see cref="EnumMemberAttribute"/> on the target frameworks
	/// this client supports, so a dedicated converter is required for wire compatibility (#388).
	/// </para>
	/// </summary>
	public class StringEnumConverterFactory : JsonConverterFactory
	{
		/// <summary> A shared instance of the factory. </summary>
		public static readonly StringEnumConverterFactory Instance = new();

		private static readonly ConcurrentDictionary<Type, JsonConverter> Cache = new();

		/// <inheritdoc />
		public override bool CanConvert(Type typeToConvert)
		{
			var enumType = Nullable.GetUnderlyingType(typeToConvert) ?? typeToConvert;
			return enumType.IsEnum && enumType.GetCustomAttribute<StringEnumAttribute>() != null;
		}

		/// <inheritdoc />
		public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
		{
			var enumType = Nullable.GetUnderlyingType(typeToConvert) ?? typeToConvert;
			var isNullable = Nullable.GetUnderlyingType(typeToConvert) != null;
			var converterType = (isNullable ? typeof(NullableEnumConverter<>) : typeof(EnumConverter<>)).MakeGenericType(enumType);
			return Cache.GetOrAdd(typeToConvert, _ => (JsonConverter)Activator.CreateInstance(converterType));
		}

		private static (Dictionary<TEnum, string> toString, Dictionary<string, TEnum> fromString) BuildMaps<TEnum>()
			where TEnum : struct, Enum
		{
			var toString = new Dictionary<TEnum, string>();
			var fromString = new Dictionary<string, TEnum>(StringComparer.Ordinal);

			foreach (var name in Enum.GetNames(typeof(TEnum)))
			{
				var value = (TEnum)Enum.Parse(typeof(TEnum), name);
				var member = typeof(TEnum).GetField(name);
				var enumMember = member?.GetCustomAttribute<EnumMemberAttribute>();
				var wire = enumMember != null && !string.IsNullOrEmpty(enumMember.Value) ? enumMember.Value : name;

				toString[value] = wire;
				fromString[wire] = value;
			}

			return (toString, fromString);
		}

		private sealed class EnumConverter<TEnum> : JsonConverter<TEnum>
			where TEnum : struct, Enum
		{
			private readonly Dictionary<TEnum, string> _toString;
			private readonly Dictionary<string, TEnum> _fromString;

			public EnumConverter() => (_toString, _fromString) = BuildMaps<TEnum>();

			public override TEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
			{
				var s = reader.GetString();
				if (s != null && _fromString.TryGetValue(s, out var value)) return value;
				if (s != null && Enum.TryParse<TEnum>(s, true, out var parsed)) return parsed;
				throw new JsonException($"Unable to convert \"{s}\" to enum {typeof(TEnum).Name}.");
			}

			public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options)
			{
				if (_toString.TryGetValue(value, out var s)) writer.WriteStringValue(s);
				else writer.WriteStringValue(value.ToString());
			}
		}

		private sealed class NullableEnumConverter<TEnum> : JsonConverter<TEnum?>
			where TEnum : struct, Enum
		{
			private readonly EnumConverter<TEnum> _inner = new();

			public override TEnum? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
				reader.TokenType == JsonTokenType.Null ? null : _inner.Read(ref reader, typeof(TEnum), options);

			public override void Write(Utf8JsonWriter writer, TEnum? value, JsonSerializerOptions options)
			{
				if (value == null) writer.WriteNullValue();
				else _inner.Write(writer, value.Value, options);
			}
		}
	}
}
