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
	/// Behavioural tests for <see cref="DateMathExpressionConverter"/>. A <see cref="DateMathExpression"/>
	/// serializes to its string representation. On read, a plain date string (no <c>||</c> separator) is
	/// anchored as a <see cref="DateTime"/>; any other string is used as the raw anchor. Non-string tokens
	/// are skipped and yield <c>null</c>.
	/// </summary>
	public class DateMathExpressionConverterTests
	{
		private static JsonSerializerOptions Options()
		{
			// Mirror the real serializer's relaxed encoder so '+' in date-math stays literal (not +).
			var options = new JsonSerializerOptions { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
			options.Converters.Add(new DateMathExpressionConverter());
			return options;
		}

		[U] public void Read_Now_Expression()
		{
			var expression = JsonSerializer.Deserialize<DateMathExpression>(@"""now""", Options());
			expression.Should().NotBeNull();
			expression.ToString().Should().Be("now");
		}

		[U] public void Read_AnchoredExpression_WithSeparator()
		{
			// Contains the date math separator, so it is kept as a raw string anchor.
			var expression = JsonSerializer.Deserialize<DateMathExpression>(@"""2015-05-05||+1M/d""", Options());
			expression.Should().NotBeNull();
			expression.ToString().Should().Be("2015-05-05||+1M/d");
		}

		[U] public void Read_PlainDate_IsAnchoredAsDateTime()
		{
			var expression = JsonSerializer.Deserialize<DateMathExpression>(@"""2015-05-05T00:00:00Z""", Options());
			expression.Should().NotBeNull();
			expression.ToString().Should().StartWith("2015-05-05T00:00:00");
		}

		[U] public void Read_Null_IsSkipped_ReturnsNull()
		{
			var expression = JsonSerializer.Deserialize<DateMathExpression>("null", Options());
			expression.Should().BeNull();
		}

		[U] public void Read_NonStringToken_IsSkipped_ReturnsNull()
		{
			var expression = JsonSerializer.Deserialize<DateMathExpression>("123", Options());
			expression.Should().BeNull();
		}

		[U] public void Write_Expression()
		{
			var expression = new DateMathExpression("now").Add("1d").Subtract("1h");
			var json = JsonSerializer.Serialize(expression, Options());
			json.Should().Be(@"""now+1d-1h""");
		}

		[U] public void Write_NullValue()
		{
			var json = JsonSerializer.Serialize<DateMathExpression>(null, Options());
			json.Should().Be("null");
		}

		[U] public void RoundTrip_Expression()
		{
			var options = Options();
			var expression = new DateMathExpression("now").Add("2h");
			var json = JsonSerializer.Serialize(expression, options);
			var roundTripped = JsonSerializer.Deserialize<DateMathExpression>(json, options);
			roundTripped.Should().NotBeNull();
			roundTripped.ToString().Should().Be("now+2h");
		}
	}
}
