/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenSearch.Client
{
	/// <summary>
	/// Per-property <see cref="System.Text.Json"/> converters replacing the vendored Utf8Json
	/// primitive "string-or-number/bool" formatters (#388). OpenSearch frequently emits numeric and
	/// boolean values as JSON strings (e.g. <c>"5"</c>, <c>"true"</c>); these converters accept either
	/// form on read and write the plain JSON primitive. They are applied per-member (via
	/// <see cref="OpenSearch.Net.DataContractResolver.PropertyConverterOverrides"/>) rather than
	/// globally, so they never hijack ordinary primitive handling.
	/// </summary>
	internal sealed class NullableStringBooleanConverter : JsonConverter<bool?>
	{
		public override bool? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			switch (reader.TokenType)
			{
				case JsonTokenType.Null: return null;
				case JsonTokenType.True: return true;
				case JsonTokenType.False: return false;
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
			if (value == null) writer.WriteNullValue();
			else writer.WriteBooleanValue(value.Value);
		}
	}

	/// <inheritdoc cref="NullableStringBooleanConverter"/>
	internal sealed class NullableStringIntConverter : JsonConverter<int?>
	{
		public override int? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			switch (reader.TokenType)
			{
				case JsonTokenType.Null: return null;
				case JsonTokenType.Number: return reader.GetInt32();
				case JsonTokenType.String:
					var s = reader.GetString();
					if (!int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var i))
						throw new JsonException($"Cannot parse {typeof(int).FullName} from: {s}");
					return i;
				default:
					throw new JsonException($"Cannot parse {typeof(int).FullName} from: {reader.TokenType}");
			}
		}

		public override void Write(Utf8JsonWriter writer, int? value, JsonSerializerOptions options)
		{
			if (value == null) writer.WriteNullValue();
			else writer.WriteNumberValue(value.Value);
		}
	}

	/// <inheritdoc cref="NullableStringBooleanConverter"/>
	internal sealed class NullableStringLongConverter : JsonConverter<long?>
	{
		public override long? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			switch (reader.TokenType)
			{
				case JsonTokenType.Null: return null;
				case JsonTokenType.Number: return reader.GetInt64();
				case JsonTokenType.String:
					var s = reader.GetString();
					if (!long.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var l))
						throw new JsonException($"Cannot parse {typeof(long).FullName} from: {s}");
					return l;
				default:
					throw new JsonException($"Cannot parse {typeof(long).FullName} from: {reader.TokenType}");
			}
		}

		public override void Write(Utf8JsonWriter writer, long? value, JsonSerializerOptions options)
		{
			if (value == null) writer.WriteNullValue();
			else writer.WriteNumberValue(value.Value);
		}
	}

	/// <inheritdoc cref="NullableStringBooleanConverter"/>
	internal sealed class NullableStringDoubleConverter : JsonConverter<double?>
	{
		public override double? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			switch (reader.TokenType)
			{
				case JsonTokenType.Null: return null;
				case JsonTokenType.Number: return reader.GetDouble();
				case JsonTokenType.String:
					var s = reader.GetString();
					if (!double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var d))
						throw new JsonException($"Cannot parse {typeof(double).FullName} from: {s}");
					return d;
				default:
					throw new JsonException($"Cannot parse {typeof(double).FullName} from: {reader.TokenType}");
			}
		}

		public override void Write(Utf8JsonWriter writer, double? value, JsonSerializerOptions options)
		{
			// Delegate to the global double converter so integral doubles keep their trailing ".0"
			// (Utf8Json-compatible formatting), matching the rest of the client.
			if (value == null) writer.WriteNullValue();
			else JsonSerializer.Serialize(writer, value.Value, options);
		}
	}

	/// <inheritdoc cref="NullableStringBooleanConverter"/>
	internal sealed class StringLongConverter : JsonConverter<long>
	{
		public override long Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			switch (reader.TokenType)
			{
				case JsonTokenType.Number: return reader.GetInt64();
				case JsonTokenType.String:
					var s = reader.GetString();
					if (!long.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var l))
						throw new JsonException($"Cannot parse {typeof(long).FullName} from: {s}");
					return l;
				default:
					throw new JsonException($"Cannot parse {typeof(long).FullName} from: {reader.TokenType}");
			}
		}

		public override void Write(Utf8JsonWriter writer, long value, JsonSerializerOptions options) =>
			writer.WriteNumberValue(value);
	}

	/// <inheritdoc cref="NullableStringBooleanConverter"/>
	internal sealed class StringIntConverter : JsonConverter<int>
	{
		public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			switch (reader.TokenType)
			{
				case JsonTokenType.Number: return reader.GetInt32();
				case JsonTokenType.String:
					var s = reader.GetString();
					if (!int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var i))
						throw new JsonException($"Cannot parse {typeof(int).FullName} from: {s}");
					return i;
				default:
					throw new JsonException($"Cannot parse {typeof(int).FullName} from: {reader.TokenType}");
			}
		}

		public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options) =>
			writer.WriteNumberValue(value);
	}

	/// <summary>
	/// Per-property converter for a <see cref="string"/> member that is carried on the wire as an
	/// integer (#388): reads a number (or string) into a string, and writes the string back as an
	/// integer. Mirrors the vendored <c>IntStringFormatter</c>.
	/// </summary>
	internal sealed class IntStringConverter : JsonConverter<string>
	{
		public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			switch (reader.TokenType)
			{
				case JsonTokenType.Number: return reader.GetInt32().ToString(CultureInfo.InvariantCulture);
				case JsonTokenType.String: return reader.GetString();
				default:
					throw new JsonException($"expected string or int but found {reader.TokenType}");
			}
		}

		public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
		{
			if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var i))
				writer.WriteNumberValue(i);
			else
				throw new InvalidOperationException($"expected a int string value, but found {value}");
		}
	}
}
