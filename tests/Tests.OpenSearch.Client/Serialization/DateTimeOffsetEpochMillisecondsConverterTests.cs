/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;

namespace Tests.OpenSearch.Client.Serialization
{
	/// <summary>
	/// Unit tests for <see cref="DateTimeOffsetEpochMillisecondsConverter"/> and
	/// <see cref="NullableDateTimeOffsetEpochMillisecondsConverter"/>, the System.Text.Json replacements for the
	/// legacy Utf8Json epoch-milliseconds <see cref="DateTimeOffset"/> formatters. Note the legacy behavior of
	/// writing the epoch milliseconds as a quoted JSON string.
	/// </summary>
	public class DateTimeOffsetEpochMillisecondsConverterTests
	{
		private static readonly DateTimeOffsetEpochMillisecondsConverter Converter = new();
		private static readonly NullableDateTimeOffsetEpochMillisecondsConverter NullableConverter = new();

		// 2021-01-01T00:00:00Z == 1609459200000 ms since epoch.
		private static readonly DateTimeOffset Sample = new DateTimeOffset(2021, 1, 1, 0, 0, 0, TimeSpan.Zero);
		private const long SampleMs = 1609459200000L;

		private static DateTimeOffset Read(string json)
		{
			var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
			reader.Read();
			return Converter.Read(ref reader, typeof(DateTimeOffset), new JsonSerializerOptions());
		}

		private static string Write(DateTimeOffset value)
		{
			using var ms = new System.IO.MemoryStream();
			using (var writer = new Utf8JsonWriter(ms))
				Converter.Write(writer, value, new JsonSerializerOptions());
			return Encoding.UTF8.GetString(ms.ToArray());
		}

		private static DateTimeOffset? ReadNullable(string json)
		{
			var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
			reader.Read();
			return NullableConverter.Read(ref reader, typeof(DateTimeOffset?), new JsonSerializerOptions());
		}

		private static string WriteNullable(DateTimeOffset? value)
		{
			using var ms = new System.IO.MemoryStream();
			using (var writer = new Utf8JsonWriter(ms))
				NullableConverter.Write(writer, value, new JsonSerializerOptions());
			return Encoding.UTF8.GetString(ms.ToArray());
		}

		// --- non-nullable ---

		[U] public void Read_Number_ReturnsUtc() => Read(SampleMs.ToString()).ToUniversalTime().Should().Be(Sample);

		[U] public void Read_Zero_ReturnsEpoch() =>
			Read("0").ToUniversalTime().Should().Be(new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero));

		[U] public void Read_IsoString_ReturnsValue() =>
			Read("\"2021-01-01T00:00:00Z\"").ToUniversalTime().Should().Be(Sample);

		[U] public void Read_Null_ReturnsDefault() => Read("null").Should().Be(default(DateTimeOffset));

		[U] public void Read_UnexpectedToken_Throws()
		{
			Action act = () => Read("true");
			act.Should().Throw<JsonException>();
		}

		[U] public void Write_WritesQuotedEpochMilliseconds() => Write(Sample).Should().Be($"\"{SampleMs}\"");

		// Round-trip through the JSON *number* form (how OpenSearch commonly sends epoch values on read).
		[U] public void RoundTrips_NumberForm() => Read(SampleMs.ToString()).ToUniversalTime().Should().Be(Sample);

		// Legacy quirk preserved: Write emits a quoted epoch-number string, but the read String-branch parses
		// ISO-8601 only, so a write->read round trip through the string form throws (matches the legacy
		// ISO8601DateTimeOffsetFormatter behavior).
		[U] public void WriteThenRead_StringForm_Throws()
		{
			var json = Write(Sample);
			Action act = () => Read(json);
			act.Should().Throw<FormatException>();
		}

		// --- nullable ---

		[U] public void Nullable_Read_Number_ReturnsUtc()
		{
			var result = ReadNullable(SampleMs.ToString());
			result.Should().NotBeNull();
			result.Value.ToUniversalTime().Should().Be(Sample);
		}

		[U] public void Nullable_Read_Null_ReturnsNull() => ReadNullable("null").Should().BeNull();

		[U] public void Nullable_Read_IsoString_ReturnsValue()
		{
			var result = ReadNullable("\"2021-01-01T00:00:00Z\"");
			result.Should().NotBeNull();
			result.Value.ToUniversalTime().Should().Be(Sample);
		}

		[U] public void Nullable_Write_Value_WritesQuotedEpochMilliseconds() =>
			WriteNullable(Sample).Should().Be($"\"{SampleMs}\"");

		[U] public void Nullable_Write_Null_WritesNull() => WriteNullable(null).Should().Be("null");

		[U] public void Nullable_RoundTrips_NumberForm()
		{
			var back = ReadNullable(SampleMs.ToString());
			back.Should().NotBeNull();
			back.Value.ToUniversalTime().Should().Be(Sample);
		}

		[U] public void Nullable_RoundTrips_Null() => ReadNullable(WriteNullable(null)).Should().BeNull();
	}
}
