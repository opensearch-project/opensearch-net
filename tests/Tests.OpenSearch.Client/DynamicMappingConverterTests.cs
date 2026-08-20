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
	/// Behavioural tests for <see cref="DynamicMappingConverter"/>: a <see cref="Union{Boolean, DynamicMapping}"/>
	/// mapping value serializes as a JSON boolean or as the <see cref="DynamicMapping"/> enum string ("strict"),
	/// and deserializes booleans, the "true"/"false"/"strict" spellings, unknown strings (=> null) and null.
	/// </summary>
	public class DynamicMappingConverterTests
	{
		private static JsonSerializerOptions Options()
		{
			var options = new JsonSerializerOptions();
			options.Converters.Add(new DynamicMappingConverter());
			return options;
		}

		private static Union<bool, DynamicMapping> Deserialize(string json) =>
			JsonSerializer.Deserialize<Union<bool, DynamicMapping>>(json, Options());

		private static string Serialize(Union<bool, DynamicMapping> value) =>
			JsonSerializer.Serialize(value, Options());

		// --- Read ---

		[U] public void Deserialize_True_ReadsBoolBranch()
		{
			var value = Deserialize("true");
			value.Tag.Should().Be(0);
			value.Item1.Should().BeTrue();
		}

		[U] public void Deserialize_False_ReadsBoolBranch()
		{
			var value = Deserialize("false");
			value.Tag.Should().Be(0);
			value.Item1.Should().BeFalse();
		}

		[U] public void Deserialize_StrictString_ReadsEnumBranch()
		{
			var value = Deserialize(@"""strict""");
			value.Tag.Should().Be(1);
			value.Item2.Should().Be(DynamicMapping.Strict);
		}

		[U] public void Deserialize_TrueString_ReadsBoolBranch()
		{
			// The legacy automata accepted the string spellings "true"/"false" and mapped them to the bool branch.
			var value = Deserialize(@"""true""");
			value.Tag.Should().Be(0);
			value.Item1.Should().BeTrue();
		}

		[U] public void Deserialize_FalseString_ReadsBoolBranch()
		{
			var value = Deserialize(@"""false""");
			value.Tag.Should().Be(0);
			value.Item1.Should().BeFalse();
		}

		[U] public void Deserialize_UnknownString_ReturnsNull()
		{
			// Legacy formatter returned null for an unrecognised string.
			Deserialize(@"""nonsense""").Should().BeNull();
		}

		[U] public void Deserialize_Null_ReturnsNull()
		{
			Deserialize("null").Should().BeNull();
		}

		// --- Write ---

		[U] public void Serialize_BoolTrue_WritesBoolean()
		{
			Serialize(new Union<bool, DynamicMapping>(true)).Should().Be("true");
		}

		[U] public void Serialize_BoolFalse_WritesBoolean()
		{
			Serialize(new Union<bool, DynamicMapping>(false)).Should().Be("false");
		}

		[U] public void Serialize_Strict_WritesEnumString()
		{
			Serialize(new Union<bool, DynamicMapping>(DynamicMapping.Strict)).Should().Be(@"""strict""");
		}

		[U] public void Serialize_Null_WritesNull()
		{
			Serialize(null).Should().Be("null");
		}

		// --- Round-trip ---

		[U] public void RoundTrip_Bool()
		{
			var back = Deserialize(Serialize(new Union<bool, DynamicMapping>(true)));
			back.Tag.Should().Be(0);
			back.Item1.Should().BeTrue();
		}

		[U] public void RoundTrip_Strict()
		{
			var back = Deserialize(Serialize(new Union<bool, DynamicMapping>(DynamicMapping.Strict)));
			back.Tag.Should().Be(1);
			back.Item2.Should().Be(DynamicMapping.Strict);
		}
	}
}
