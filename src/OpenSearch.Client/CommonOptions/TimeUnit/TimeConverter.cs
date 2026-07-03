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
	/// A <see cref="System.Text.Json"/> converter for <see cref="Time"/>, replacing the vendored
	/// Utf8Json <c>TimeFormatter</c> as part of #388. <c>-1</c>/<c>0</c> are written as numbers, a
	/// factor+unit as a string (e.g. <c>"1d"</c>), otherwise the millisecond value as a number.
	/// </summary>
	internal sealed class TimeConverter : JsonConverter<Time>
	{
		public override void Write(Utf8JsonWriter writer, Time value, JsonSerializerOptions options)
		{
			if (value == null)
				writer.WriteNullValue();
			else if (value == Time.MinusOne)
				writer.WriteNumberValue(-1);
			else if (value == Time.Zero)
				writer.WriteNumberValue(0);
			else if (value.Factor.HasValue && value.Interval.HasValue)
				writer.WriteStringValue(value.ToString());
			else if (value.Milliseconds != null)
				writer.WriteNumberValue((long)value.Milliseconds);
			else
				writer.WriteNullValue();
		}

		public override Time Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			switch (reader.TokenType)
			{
				case JsonTokenType.Null:
					return null;
				case JsonTokenType.String:
					return new Time(reader.GetString());
				case JsonTokenType.Number:
					var ms = reader.GetDouble();
					if (ms == -1) return Time.MinusOne;
					if (ms == 0) return Time.Zero;
					return new Time(ms);
				default:
					throw new JsonException($"Cannot deserialize {nameof(Time)} from token {reader.TokenType}.");
			}
		}
	}
}
