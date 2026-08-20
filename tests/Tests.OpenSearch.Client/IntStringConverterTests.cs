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
	/// Behavioural tests for <see cref="IntStringConverter"/>. On read it accepts a JSON number or
	/// string and yields a string; on write it parses the string to an int and writes a JSON number,
	/// throwing when the value is not an int string.
	/// </summary>
	public class IntStringConverterTests
	{
		private static JsonSerializerOptions Options()
		{
			var options = new JsonSerializerOptions();
			options.Converters.Add(new IntStringConverter());
			return options;
		}

		[U] public void Read_Number_ReturnsString()
		{
			var value = JsonSerializer.Deserialize<string>("42", Options());
			value.Should().Be("42");
		}

		[U] public void Read_String_ReturnsString()
		{
			var value = JsonSerializer.Deserialize<string>(@"""99""", Options());
			value.Should().Be("99");
		}

		[U] public void Read_NegativeNumber()
		{
			var value = JsonSerializer.Deserialize<string>("-7", Options());
			value.Should().Be("-7");
		}

		[U] public void Read_UnexpectedToken_Throws()
		{
			Action act = () => JsonSerializer.Deserialize<string>("true", Options());
			act.Should().Throw<JsonException>();
		}

		[U] public void Write_IntString_WritesNumber()
		{
			var json = JsonSerializer.Serialize("123", Options());
			json.Should().Be("123");
		}

		[U] public void Write_NonIntString_Throws()
		{
			Action act = () => JsonSerializer.Serialize("abc", Options());
			act.Should().Throw<InvalidOperationException>();
		}

		[U] public void RoundTrip()
		{
			var json = JsonSerializer.Serialize("500", Options());
			var value = JsonSerializer.Deserialize<string>(json, Options());
			value.Should().Be("500");
		}
	}
}
