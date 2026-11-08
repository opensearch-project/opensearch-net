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
	/// Validates the System.Text.Json <see cref="TimeSpanTicksConverter"/> and
	/// <see cref="NullableTimeSpanTicksConverter"/> replacements for the legacy Utf8Json formatters.
	/// A <see cref="TimeSpan"/> is written as its <see cref="TimeSpan.Ticks"/> and read from either a
	/// numeric ticks value or a parseable string.
	/// </summary>
	public class TimeSpanTicksConverterTests
	{
		private static readonly JsonSerializerOptions Options = CreateOptions();

		private static JsonSerializerOptions CreateOptions()
		{
			var options = new JsonSerializerOptions();
			options.Converters.Add(new TimeSpanTicksConverter());
			options.Converters.Add(new NullableTimeSpanTicksConverter());
			return options;
		}

		private static string Serialize<T>(T value) =>
			Encoding.UTF8.GetString(JsonSerializer.SerializeToUtf8Bytes(value, Options));

		private static T Deserialize<T>(string json) =>
			JsonSerializer.Deserialize<T>(Encoding.UTF8.GetBytes(json), Options);

		[U] public void Serialize_WritesTicksAsNumber()
		{
			var value = TimeSpan.FromMinutes(5);
			Serialize(value).Should().Be(value.Ticks.ToString());
		}

		[U] public void Deserialize_FromTicksNumber()
		{
			var value = TimeSpan.FromHours(2);
			Deserialize<TimeSpan>(value.Ticks.ToString()).Should().Be(value);
		}

		[U] public void Deserialize_FromString()
		{
			Deserialize<TimeSpan>("\"00:05:00\"").Should().Be(TimeSpan.FromMinutes(5));
		}

		[U] public void Deserialize_FromUnexpectedToken_Throws()
		{
			Action act = () => Deserialize<TimeSpan>("true");
			act.Should().Throw<JsonException>();
		}

		[U] public void RoundTrips()
		{
			var value = new TimeSpan(1, 2, 3, 4, 5);
			var json = Serialize(value);
			Deserialize<TimeSpan>(json).Should().Be(value);
		}

		[U] public void Nullable_Serialize_WritesTicksAsNumber()
		{
			TimeSpan? value = TimeSpan.FromSeconds(42);
			Serialize(value).Should().Be(value.Value.Ticks.ToString());
		}

		[U] public void Nullable_Serialize_Null_WritesNull()
		{
			TimeSpan? value = null;
			Serialize(value).Should().Be("null");
		}

		[U] public void Nullable_Deserialize_FromTicksNumber()
		{
			var value = TimeSpan.FromHours(3);
			Deserialize<TimeSpan?>(value.Ticks.ToString()).Should().Be(value);
		}

		[U] public void Nullable_Deserialize_Null()
		{
			Deserialize<TimeSpan?>("null").Should().BeNull();
		}

		[U] public void Nullable_Deserialize_FromString()
		{
			Deserialize<TimeSpan?>("\"01:00:00\"").Should().Be(TimeSpan.FromHours(1));
		}

		[U] public void Nullable_RoundTrips_Value()
		{
			TimeSpan? value = new TimeSpan(0, 10, 20, 30);
			var json = Serialize(value);
			Deserialize<TimeSpan?>(json).Should().Be(value);
		}

		[U] public void Nullable_RoundTrips_Null()
		{
			TimeSpan? value = null;
			var json = Serialize(value);
			Deserialize<TimeSpan?>(json).Should().BeNull();
		}
	}
}
