/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.Text.Json;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;

namespace Tests.OpenSearch.Client.Serialization
{
	/// <summary>
	/// Behavioural tests for <see cref="DateMathTimeConverter"/>. A <see cref="DateMathTime"/> serializes
	/// to its string representation (e.g. <c>"5m"</c>), or JSON <c>null</c> when the value is <c>null</c>.
	/// On read, a JSON string is parsed into a <see cref="DateMathTime"/> via the implicit string
	/// conversion.
	/// </summary>
	public class DateMathTimeConverterTests
	{
		private static JsonSerializerOptions Options()
		{
			var options = new JsonSerializerOptions();
			options.Converters.Add(new DateMathTimeConverter());
			return options;
		}

		[U] public void Read_Minutes()
		{
			var time = JsonSerializer.Deserialize<DateMathTime>(@"""5m""", Options());
			time.Should().NotBeNull();
			time.Factor.Should().Be(5);
			time.Interval.Should().Be(DateMathTimeUnit.Minute);
			time.ToString().Should().Be("5m");
		}

		[U] public void Read_Days()
		{
			var time = JsonSerializer.Deserialize<DateMathTime>(@"""2d""", Options());
			time.Should().NotBeNull();
			time.Factor.Should().Be(2);
			time.Interval.Should().Be(DateMathTimeUnit.Day);
		}

		[U] public void Read_Months_UppercaseInterval()
		{
			// 'M' is months (distinct from 'm' minutes).
			var time = JsonSerializer.Deserialize<DateMathTime>(@"""3M""", Options());
			time.Should().NotBeNull();
			time.Factor.Should().Be(3);
			time.Interval.Should().Be(DateMathTimeUnit.Month);
		}

		[U] public void Write_Value()
		{
			var json = JsonSerializer.Serialize(new DateMathTime("5m"), Options());
			json.Should().Be(@"""5m""");
		}

		[U] public void Write_NullValue()
		{
			var json = JsonSerializer.Serialize<DateMathTime>(null, Options());
			json.Should().Be("null");
		}

		[U] public void RoundTrip()
		{
			var options = Options();
			var json = JsonSerializer.Serialize<DateMathTime>(new DateMathTime(7, DateMathTimeUnit.Hour), options);
			var time = JsonSerializer.Deserialize<DateMathTime>(json, options);
			time.Should().NotBeNull();
			time.Factor.Should().Be(7);
			time.Interval.Should().Be(DateMathTimeUnit.Hour);
			time.Should().Be(new DateMathTime(7, DateMathTimeUnit.Hour));
		}
	}
}
