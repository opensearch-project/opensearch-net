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
	/// Behavioural tests for <see cref="SimpleQueryStringFlagsConverter"/>. The <see cref="SimpleQueryStringFlags"/>
	/// <c>[Flags]</c> enum is serialized as a single "|"-delimited string of upper-case token names in a fixed order
	/// (<c>ALL, NONE, AND, OR, NOT, PREFIX, PHRASE, PRECEDENCE, ESCAPE, WHITESPACE, FUZZY, NEAR, SLOP</c>). On read the
	/// string is split on "|", each token mapped case-insensitively to its enum member (unknown tokens ignored) and
	/// OR'd together. A JSON null maps to a null value.
	/// </summary>
	public class SimpleQueryStringFlagsConverterTests
	{
		private static JsonSerializerOptions Options()
		{
			var options = new JsonSerializerOptions();
			options.Converters.Add(new SimpleQueryStringFlagsConverter());
			return options;
		}

		[U] public void Write_SingleFlag()
		{
			var json = JsonSerializer.Serialize<SimpleQueryStringFlags?>(SimpleQueryStringFlags.And, Options());
			json.Should().Be(@"""AND""");
		}

		[U] public void Write_MultipleFlags_PreservesLegacyOrder()
		{
			// Legacy order emits AND before OR before NOT regardless of how the value was composed.
			var value = SimpleQueryStringFlags.Not | SimpleQueryStringFlags.Or | SimpleQueryStringFlags.And;
			var json = JsonSerializer.Serialize<SimpleQueryStringFlags?>(value, Options());
			json.Should().Be(@"""AND|OR|NOT""");
		}

		[U] public void Write_All_FirstToken()
		{
			var json = JsonSerializer.Serialize<SimpleQueryStringFlags?>(SimpleQueryStringFlags.All, Options());
			json.Should().Be(@"""ALL""");
		}

		[U] public void Write_None()
		{
			var json = JsonSerializer.Serialize<SimpleQueryStringFlags?>(SimpleQueryStringFlags.None, Options());
			json.Should().Be(@"""NONE""");
		}

		[U] public void Write_Null_WritesNull()
		{
			var json = JsonSerializer.Serialize<SimpleQueryStringFlags?>(null, Options());
			json.Should().Be("null");
		}

		[U] public void Read_SingleFlag()
		{
			var value = JsonSerializer.Deserialize<SimpleQueryStringFlags?>(@"""AND""", Options());
			value.Should().Be(SimpleQueryStringFlags.And);
		}

		[U] public void Read_MultipleFlags()
		{
			var value = JsonSerializer.Deserialize<SimpleQueryStringFlags?>(@"""AND|OR|NOT""", Options());
			value.Should().Be(SimpleQueryStringFlags.And | SimpleQueryStringFlags.Or | SimpleQueryStringFlags.Not);
		}

		[U] public void Read_CaseInsensitive()
		{
			var value = JsonSerializer.Deserialize<SimpleQueryStringFlags?>(@"""and|or""", Options());
			value.Should().Be(SimpleQueryStringFlags.And | SimpleQueryStringFlags.Or);
		}

		[U] public void Read_UnknownTokens_AreIgnored()
		{
			// Unknown tokens produce no flag and are filtered out; known tokens still combine.
			var value = JsonSerializer.Deserialize<SimpleQueryStringFlags?>(@"""AND|BOGUS|OR""", Options());
			value.Should().Be(SimpleQueryStringFlags.And | SimpleQueryStringFlags.Or);
		}

		[U] public void Read_AllUnknownTokens_ReturnsDefault()
		{
			// No token resolves, so the aggregate seed (default(SimpleQueryStringFlags) == 0) is returned.
			var value = JsonSerializer.Deserialize<SimpleQueryStringFlags?>(@"""BOGUS""", Options());
			value.Should().Be(default(SimpleQueryStringFlags));
		}

		[U] public void Read_Null_ReturnsNull()
		{
			var value = JsonSerializer.Deserialize<SimpleQueryStringFlags?>("null", Options());
			value.Should().BeNull();
		}

		[U] public void RoundTrip()
		{
			var options = Options();
			var original = SimpleQueryStringFlags.Prefix | SimpleQueryStringFlags.Phrase | SimpleQueryStringFlags.Fuzzy;
			var json = JsonSerializer.Serialize<SimpleQueryStringFlags?>(original, options);
			var back = JsonSerializer.Deserialize<SimpleQueryStringFlags?>(json, options);
			back.Should().Be(original);
		}
	}
}
