/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Text.Json;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;

namespace Tests.OpenSearch.Client.Serialization
{
	/// <summary>
	/// Behavioural tests for <see cref="DateMathConverter"/>. A <see cref="DateMath"/> serializes to
	/// its string representation. On read, a plain date string (no <c>||</c> separator) becomes an
	/// anchored date, while an expression string is parsed into a <see cref="DateMathExpression"/>;
	/// non-string tokens are skipped and yield <c>null</c>.
	/// </summary>
	public class DateMathConverterTests
	{
		private static JsonSerializerOptions Options()
		{
			// Mirror the real serializer's relaxed encoder so '+' in date-math stays literal (not +).
			var options = new JsonSerializerOptions { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
			options.Converters.Add(new DateMathConverter());
			return options;
		}

		[U] public void Read_Now_Expression()
		{
			var dateMath = JsonSerializer.Deserialize<DateMath>(@"""now""", Options());
			dateMath.Should().NotBeNull();
			dateMath.ToString().Should().Be("now");
		}

		[U] public void Read_Expression_WithRangesAndRounding()
		{
			var dateMath = JsonSerializer.Deserialize<DateMath>(@"""now+1d-1h/d""", Options());
			dateMath.Should().NotBeNull();
			dateMath.ToString().Should().Be("now+1d-1h/d");
		}

		[U] public void Read_AnchoredExpression_WithSeparator()
		{
			var dateMath = JsonSerializer.Deserialize<DateMath>(@"""2015-05-05||+1M/d""", Options());
			dateMath.Should().NotBeNull();
			dateMath.ToString().Should().Be("2015-05-05||+1M/d");
		}

		[U] public void Read_PlainDate_IsAnchoredAsDateTime()
		{
			// No date-math separator and parses as a date => DateMath.Anchored(DateTime).
			var dateMath = JsonSerializer.Deserialize<DateMath>(@"""2015-05-05T00:00:00Z""", Options());
			dateMath.Should().NotBeNull();
			dateMath.ToString().Should().StartWith("2015-05-05T00:00:00");
		}

		[U] public void Read_Null_IsSkipped_ReturnsNull()
		{
			var dateMath = JsonSerializer.Deserialize<DateMath>("null", Options());
			dateMath.Should().BeNull();
		}

		[U] public void Read_NonStringToken_IsSkipped_ReturnsNull()
		{
			var dateMath = JsonSerializer.Deserialize<DateMath>("123", Options());
			dateMath.Should().BeNull();
		}

		[U] public void Write_Expression()
		{
			var json = JsonSerializer.Serialize(DateMath.Now.Add("1d").RoundTo(DateMathTimeUnit.Day), Options());
			json.Should().Be(@"""now+1d/d""");
		}

		[U] public void Write_NullValue()
		{
			var json = JsonSerializer.Serialize<DateMath>(null, Options());
			json.Should().Be("null");
		}

		[U] public void RoundTrip_Expression()
		{
			var options = Options();
			var json = JsonSerializer.Serialize<DateMath>("now-2h/h", options);
			var dateMath = JsonSerializer.Deserialize<DateMath>(json, options);
			dateMath.Should().NotBeNull();
			dateMath.ToString().Should().Be("now-2h/h");
		}
	}
}
