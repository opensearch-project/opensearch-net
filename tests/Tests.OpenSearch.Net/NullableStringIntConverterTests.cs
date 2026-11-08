/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.Text.Json;
using FluentAssertions;
using OpenSearch.Net.Serialization.Converters;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;

namespace Tests.OpenSearch.Net.Serialization
{
	/// <summary>
	/// Behavioural tests for <see cref="NullableStringIntConverter"/>, which accepts an int that OpenSearch
	/// may send as a JSON number, a numeric string, or null.
	/// </summary>
	public class NullableStringIntConverterTests
	{
		private static JsonSerializerOptions Options()
		{
			var options = new JsonSerializerOptions();
			options.Converters.Add(new NullableStringIntConverter());
			return options;
		}

		[U] public void Read_Number_ReturnsInt()
		{
			JsonSerializer.Deserialize<int?>("42", Options()).Should().Be(42);
		}

		[U] public void Read_NumericString_ReturnsInt()
		{
			JsonSerializer.Deserialize<int?>(@"""42""", Options()).Should().Be(42);
		}

		[U] public void Read_Null_ReturnsNull()
		{
			JsonSerializer.Deserialize<int?>("null", Options()).Should().BeNull();
		}

		[U] public void Read_EmptyString_ReturnsNull()
		{
			// NOTE: intentional behaviour change from the legacy Utf8Json formatter, which threw on "".
			// The System.Text.Json converter treats an empty string as null.
			JsonSerializer.Deserialize<int?>(@"""""", Options()).Should().BeNull();
		}

		[U] public void Read_NonNumericString_Throws()
		{
			var act = () => JsonSerializer.Deserialize<int?>(@"""abc""", Options());
			act.Should().Throw<JsonException>();
		}

		[U] public void Write_Value_WritesNumber()
		{
			JsonSerializer.Serialize<int?>(7, Options()).Should().Be("7");
		}

		[U] public void Write_Null_WritesNull()
		{
			JsonSerializer.Serialize<int?>(null, Options()).Should().Be("null");
		}
	}
}
