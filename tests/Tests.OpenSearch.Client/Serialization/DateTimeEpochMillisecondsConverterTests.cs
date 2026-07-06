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
	/// Unit tests for <see cref="NullableDateTimeEpochMillisecondsConverter"/>, the System.Text.Json replacement
	/// for the legacy Utf8Json <c>NullableDateTimeEpochMillisecondsFormatter</c>. Epoch is milliseconds since
	/// 1970-01-01T00:00:00Z.
	/// </summary>
	public class DateTimeEpochMillisecondsConverterTests
	{
		private static readonly NullableDateTimeEpochMillisecondsConverter Converter = new();

		// 2021-01-01T00:00:00Z == 1609459200000 ms since epoch.
		// The converter returns DateTimeOffset.DateTime, i.e. the UTC wall-clock as a DateTime with
		// Kind=Unspecified. We therefore compare against the UTC wall-clock directly (no ToUniversalTime,
		// which would misinterpret an Unspecified value as local time).
		private static readonly DateTime SampleWallClock = new DateTime(2021, 1, 1, 0, 0, 0);
		private static readonly DateTime EpochWallClock = new DateTime(1970, 1, 1, 0, 0, 0);
		private const long SampleMs = 1609459200000L;

		private static DateTime? Read(string json)
		{
			var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
			reader.Read();
			return Converter.Read(ref reader, typeof(DateTime?), new JsonSerializerOptions());
		}

		private static string Write(DateTime? value)
		{
			using var ms = new System.IO.MemoryStream();
			using (var writer = new Utf8JsonWriter(ms))
				Converter.Write(writer, value, new JsonSerializerOptions());
			return Encoding.UTF8.GetString(ms.ToArray());
		}

		[U] public void Read_Number_ReturnsUtcDateTime()
		{
			var result = Read(SampleMs.ToString());
			result.Should().NotBeNull();
			result.Value.Should().Be(SampleWallClock);
		}

		[U] public void Read_Zero_ReturnsEpoch()
		{
			var result = Read("0");
			result.Should().NotBeNull();
			result.Value.Should().Be(EpochWallClock);
		}

		[U] public void Read_Null_ReturnsNull() => Read("null").Should().BeNull();

		[U] public void Read_IsoString_ReturnsDateTime()
		{
			var result = Read("\"2021-01-01T00:00:00Z\"");
			result.Should().NotBeNull();
			result.Value.ToUniversalTime().Should().Be(new DateTime(2021, 1, 1, 0, 0, 0, DateTimeKind.Utc));
		}

		[U] public void Read_UnexpectedToken_Throws()
		{
			Action act = () => Read("true");
			act.Should().Throw<JsonException>();
		}

		// A UTC-kind DateTime is subtracted from the epoch (a DateTimeOffset at offset 0), so the wall-clock
		// value is interpreted as UTC and yields the expected epoch milliseconds.
		private static readonly DateTime SampleUtc = new DateTime(2021, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		[U] public void Write_Value_WritesEpochMilliseconds() => Write(SampleUtc).Should().Be(SampleMs.ToString());

		[U] public void Write_Epoch_WritesZero() =>
			Write(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).Should().Be("0");

		[U] public void Write_Null_WritesNull() => Write(null).Should().Be("null");

		[U] public void RoundTrips_Value()
		{
			// Write emits epoch ms; reading it back returns the UTC wall-clock as an Unspecified DateTime.
			var back = Read(Write(SampleUtc));
			back.Should().NotBeNull();
			back.Value.Should().Be(SampleWallClock);
		}

		[U] public void RoundTrips_Null() => Read(Write(null)).Should().BeNull();
	}
}
