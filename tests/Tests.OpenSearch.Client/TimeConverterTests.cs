/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.Text.Json;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.Net;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;

namespace Tests.OpenSearch.Client.Serialization
{
	/// <summary>
	/// Behavioural tests for <see cref="TimeConverter"/>. A <see cref="Time"/> reads from either a JSON
	/// string expression (e.g. <c>"5m"</c>) or a numeric millisecond value, with <c>-1</c> and <c>0</c>
	/// mapping to <see cref="Time.MinusOne"/> and <see cref="Time.Zero"/>. Non-string/number tokens are
	/// skipped and yield <c>null</c>.
	/// </summary>
	public class TimeConverterTests
	{
		private static JsonSerializerOptions Options()
		{
			var options = new JsonSerializerOptions();
			options.Converters.Add(new TimeConverter());
			return options;
		}

		[U] public void Read_String_Expression()
		{
			var time = JsonSerializer.Deserialize<Time>(@"""5m""", Options());
			time.Should().NotBeNull();
			time.Factor.Should().Be(5);
			time.Interval.Should().Be(TimeUnit.Minutes);
		}

		[U] public void Read_Number_MinusOne()
		{
			var time = JsonSerializer.Deserialize<Time>("-1", Options());
			time.Should().Be(Time.MinusOne);
		}

		[U] public void Read_Number_Zero()
		{
			var time = JsonSerializer.Deserialize<Time>("0", Options());
			time.Should().Be(Time.Zero);
		}

		[U] public void Read_Number_Milliseconds()
		{
			// 5000 ms reduces to 5 seconds.
			var time = JsonSerializer.Deserialize<Time>("5000", Options());
			time.Should().NotBeNull();
			time.Milliseconds.Should().Be(5000);
			time.Factor.Should().Be(5);
			time.Interval.Should().Be(TimeUnit.Seconds);
		}

		[U] public void Read_NonStringOrNumberToken_IsSkipped_ReturnsNull()
		{
			var time = JsonSerializer.Deserialize<Time>(@"{""foo"":1}", Options());
			time.Should().BeNull();
		}

		[U] public void Read_Null_IsSkipped_ReturnsNull()
		{
			var time = JsonSerializer.Deserialize<Time>("null", Options());
			time.Should().BeNull();
		}

		[U] public void Write_MinusOne()
		{
			var json = JsonSerializer.Serialize(Time.MinusOne, Options());
			json.Should().Be("-1");
		}

		[U] public void Write_Zero()
		{
			var json = JsonSerializer.Serialize(Time.Zero, Options());
			json.Should().Be("0");
		}

		[U] public void Write_FactorAndInterval_AsString()
		{
			var json = JsonSerializer.Serialize(new Time("5m"), Options());
			json.Should().Be(@"""5m""");
		}

		[U] public void RoundTrip_Expression()
		{
			var options = Options();
			var json = JsonSerializer.Serialize(new Time("30s"), options);
			var time = JsonSerializer.Deserialize<Time>(json, options);
			time.Should().NotBeNull();
			time.Factor.Should().Be(30);
			time.Interval.Should().Be(TimeUnit.Seconds);
		}

		[U] public void RoundTrip_MinusOne()
		{
			var options = Options();
			var json = JsonSerializer.Serialize(Time.MinusOne, options);
			var time = JsonSerializer.Deserialize<Time>(json, options);
			time.Should().Be(Time.MinusOne);
		}
	}
}
