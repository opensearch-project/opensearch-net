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
	/// Behavioural tests for <see cref="MinimumShouldMatchConverter"/>. A MinimumShouldMatch is a union of int
	/// (JSON number) and string (JSON string, e.g. a percentage). Covers both read branches, the invalid-token
	/// branch, both write branches and the null write branch.
	/// </summary>
	public class MinimumShouldMatchConverterTests
	{
		private static JsonSerializerOptions Options()
		{
			var options = new JsonSerializerOptions();
			options.Converters.Add(new MinimumShouldMatchConverter());
			return options;
		}

		[U] public void Read_Number()
		{
			var value = JsonSerializer.Deserialize<MinimumShouldMatch>("3", Options());
			value.Should().NotBeNull();
			value.Tag.Should().Be(0);
			value.Item1.Should().Be(3);
		}

		[U] public void Read_String()
		{
			var value = JsonSerializer.Deserialize<MinimumShouldMatch>(@"""75%""", Options());
			value.Should().NotBeNull();
			value.Tag.Should().Be(1);
			value.Item2.Should().Be("75%");
		}

		[U] public void Read_InvalidTokenThrows()
		{
			System.Action act = () => JsonSerializer.Deserialize<MinimumShouldMatch>("true", Options());
			act.Should().Throw<JsonException>();
		}

		[U] public void Write_Number()
		{
			JsonSerializer.Serialize<MinimumShouldMatch>(3, Options()).Should().Be("3");
		}

		[U] public void Write_String()
		{
			JsonSerializer.Serialize<MinimumShouldMatch>("75%", Options()).Should().Be(@"""75%""");
		}

		[U] public void Write_Null()
		{
			JsonSerializer.Serialize<MinimumShouldMatch>(null, Options()).Should().Be("null");
		}

		[U] public void RoundTrip_Number()
		{
			var json = JsonSerializer.Serialize<MinimumShouldMatch>(2, Options());
			var back = JsonSerializer.Deserialize<MinimumShouldMatch>(json, Options());
			back.Tag.Should().Be(0);
			back.Item1.Should().Be(2);
		}

		[U] public void RoundTrip_String()
		{
			var json = JsonSerializer.Serialize<MinimumShouldMatch>("50%", Options());
			var back = JsonSerializer.Deserialize<MinimumShouldMatch>(json, Options());
			back.Tag.Should().Be(1);
			back.Item2.Should().Be("50%");
		}
	}
}
