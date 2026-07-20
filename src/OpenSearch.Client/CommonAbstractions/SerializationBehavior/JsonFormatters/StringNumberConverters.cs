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
	/// System.Text.Json replacements for the legacy Utf8Json string/number formatters (StringLongFormatter,
	/// StringIntFormatter, NullableStringLongFormatter, NullableStringDoubleFormatter). Each accepts the value as a
	/// JSON number or a numeric string (some OpenSearch responses quote numbers), mirroring the legacy formatter,
	/// and writes it back as a JSON number. Applied at the member level via the [JsonFormatter] mapping in
	/// <see cref="HighLevelContractResolver"/>.
	/// </summary>
	internal class StringLongConverter : JsonConverter<long>
	{
		public override long Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			switch (reader.TokenType)
			{
				case JsonTokenType.Number:
					return reader.GetInt64();
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

	internal class StringIntConverter : JsonConverter<int>
	{
		public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			switch (reader.TokenType)
			{
				case JsonTokenType.Number:
					return reader.GetInt32();
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

	internal class NullableStringLongConverter : JsonConverter<long?>
	{
		public override long? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			switch (reader.TokenType)
			{
				case JsonTokenType.Null:
					return null;
				case JsonTokenType.Number:
					return reader.GetInt64();
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
			if (value.HasValue)
				writer.WriteNumberValue(value.Value);
			else
				writer.WriteNullValue();
		}
	}

	internal class NullableStringDoubleConverter : JsonConverter<double?>
	{
		public override double? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			switch (reader.TokenType)
			{
				case JsonTokenType.Null:
					return null;
				case JsonTokenType.Number:
					return reader.GetDouble();
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
			if (value.HasValue)
				writer.WriteNumberValue(value.Value);
			else
				writer.WriteNullValue();
		}
	}
}
