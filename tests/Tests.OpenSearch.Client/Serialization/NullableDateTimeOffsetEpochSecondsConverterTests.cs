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
	/// Unit tests for <see cref="NullableDateTimeOffsetEpochSecondsConverter"/>, the System.Text.Json replacement
	/// for the legacy Utf8Json <c>NullableDateTimeOffsetEpochSecondsFormatter</c>. Epoch is seconds since
	/// 1970-01-01T00:00:00Z; only JSON numbers are treated as values, everything else yields null.
	/// </summary>
	public class NullableDateTimeOffsetEpochSecondsConverterTests
	{
		private static readonly NullableDateTimeOffsetEpochSecondsConverter Converter = new();

		// 2021-01-01T00:00:00Z == 1609459200 seconds since epoch.
		private static readonly DateTimeOffset Sample = new DateTimeOffset(2021, 1, 1, 0, 0, 0, TimeSpan.Zero);
		private const long SampleSeconds = 1609459200L;

		private static DateTimeOffset? Read(string json)
		{
			var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
			reader.Read();
			return Converter.Read(ref reader, typeof(DateTimeOffset?), new JsonSerializerOptions());
		}

		private static string Write(DateTimeOffset? value)
		{
			using var ms = new System.IO.MemoryStream();
			using (var writer = new Utf8JsonWriter(ms))
				Converter.Write(writer, value, new JsonSerializerOptions());
			return Encoding.UTF8.GetString(ms.ToArray());
		}

		[U] public void Read_Number_ReturnsUtc()
		{
			var result = Read(SampleSeconds.ToString());
			result.Should().NotBeNull();
			result.Value.ToUniversalTime().Should().Be(Sample);
		}

		[U] public void Read_Zero_ReturnsEpoch()
		{
			var result = Read("0");
			result.Should().NotBeNull();
			result.Value.ToUniversalTime().Should().Be(new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero));
		}

		[U] public void Read_Null_ReturnsNull() => Read("null").Should().BeNull();

		// Legacy behavior: a string token is not a number, so it is skipped and yields null.
		[U] public void Read_String_ReturnsNull() => Read("\"2021-01-01T00:00:00Z\"").Should().BeNull();

		[U] public void Read_UnexpectedToken_ReturnsNull() => Read("true").Should().BeNull();

		[U] public void Write_Value_WritesEpochSeconds() => Write(Sample).Should().Be(SampleSeconds.ToString());

		[U] public void Write_Epoch_WritesZero() =>
			Write(new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero)).Should().Be("0");

		[U] public void Write_Null_WritesNull() => Write(null).Should().Be("null");

		[U] public void RoundTrips_Value()
		{
			var back = Read(Write(Sample));
			back.Should().NotBeNull();
			back.Value.ToUniversalTime().Should().Be(Sample);
		}

		[U] public void RoundTrips_Null() => Read(Write(null)).Should().BeNull();
	}
}
