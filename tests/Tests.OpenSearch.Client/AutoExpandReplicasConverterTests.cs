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
	/// Behavioural tests for <see cref="AutoExpandReplicasConverter"/>. Reads a JSON <c>false</c> as the disabled
	/// singleton and a JSON string (e.g. <c>"0-5"</c>, <c>"0-all"</c>, <c>"false"</c>) via
	/// <see cref="AutoExpandReplicas.Create(string)"/>; other tokens throw. Writes <c>false</c> when null/disabled,
	/// otherwise the range string.
	/// </summary>
	public class AutoExpandReplicasConverterTests
	{
		private static JsonSerializerOptions Options()
		{
			var options = new JsonSerializerOptions();
			options.Converters.Add(new AutoExpandReplicasConverter());
			return options;
		}

		[U] public void Read_False_ReturnsDisabled()
		{
			var value = JsonSerializer.Deserialize<AutoExpandReplicas>("false", Options());
			value.Should().BeSameAs(AutoExpandReplicas.Disabled);
			value.Enabled.Should().BeFalse();
		}

		[U] public void Read_StringFalse_ReturnsDisabled()
		{
			var value = JsonSerializer.Deserialize<AutoExpandReplicas>(@"""false""", Options());
			value.Should().BeSameAs(AutoExpandReplicas.Disabled);
			value.Enabled.Should().BeFalse();
		}

		[U] public void Read_NumericRange()
		{
			var value = JsonSerializer.Deserialize<AutoExpandReplicas>(@"""0-5""", Options());
			value.Enabled.Should().BeTrue();
			value.MinReplicas.Should().Be(0);
			value.MaxReplicas.Match(i => i ?? -1, s => -1).Should().Be(5);
			value.ToString().Should().Be("0-5");
		}

		[U] public void Read_AllRange()
		{
			var value = JsonSerializer.Deserialize<AutoExpandReplicas>(@"""0-all""", Options());
			value.Enabled.Should().BeTrue();
			value.MinReplicas.Should().Be(0);
			value.MaxReplicas.Match(i => "int", s => s).Should().Be("all");
			value.ToString().Should().Be("0-all");
		}

		[U] public void Read_InvalidToken_Throws()
		{
			System.Action act = () => JsonSerializer.Deserialize<AutoExpandReplicas>("123", Options());
			act.Should().Throw<JsonException>();
		}

		[U] public void Write_Null_WritesFalse()
		{
			var json = JsonSerializer.Serialize<AutoExpandReplicas>(null, Options());
			json.Should().Be("false");
		}

		[U] public void Write_Disabled_WritesFalse()
		{
			var json = JsonSerializer.Serialize(AutoExpandReplicas.Disabled, Options());
			json.Should().Be("false");
		}

		[U] public void Write_NumericRange()
		{
			var json = JsonSerializer.Serialize(AutoExpandReplicas.Create(0, 5), Options());
			json.Should().Be(@"""0-5""");
		}

		[U] public void Write_AllRange()
		{
			var json = JsonSerializer.Serialize(AutoExpandReplicas.Create(1), Options());
			json.Should().Be(@"""1-all""");
		}

		[U] public void RoundTrip_Range()
		{
			var options = Options();
			var json = JsonSerializer.Serialize(AutoExpandReplicas.Create(0, 3), options);
			var value = JsonSerializer.Deserialize<AutoExpandReplicas>(json, options);
			value.ToString().Should().Be("0-3");
		}

		[U] public void RoundTrip_Disabled()
		{
			var options = Options();
			var json = JsonSerializer.Serialize(AutoExpandReplicas.Disabled, options);
			var value = JsonSerializer.Deserialize<AutoExpandReplicas>(json, options);
			value.Enabled.Should().BeFalse();
		}
	}
}
