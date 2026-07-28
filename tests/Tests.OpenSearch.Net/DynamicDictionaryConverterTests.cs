/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.Text.Json;
using FluentAssertions;
using OpenSearch.Net;
using OpenSearch.Net.Serialization.Converters;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;

namespace Tests.OpenSearch.Net.Serialization
{
	/// <summary>
	/// Behavioural tests for <see cref="DynamicDictionaryConverter"/>, which reads arbitrary JSON objects/arrays
	/// into a <see cref="DynamicDictionary"/> and writes them back.
	/// </summary>
	public class DynamicDictionaryConverterTests
	{
		private static JsonSerializerOptions Options()
		{
			var options = new JsonSerializerOptions();
			options.Converters.Add(new DynamicDictionaryConverter());
			return options;
		}

		[U] public void Read_Null_ReturnsNull()
		{
			var dict = JsonSerializer.Deserialize<DynamicDictionary>("null", Options());
			(dict == null).Should().BeTrue();
		}

		[U] public void Read_FlatObject_ReadsValues()
		{
			var dict = JsonSerializer.Deserialize<DynamicDictionary>(@"{""name"":""bob"",""age"":30}", Options());

			(dict == null).Should().BeFalse();
			dict["name"].Value.Should().Be("bob");
			// Integral numbers are read as long (see converter's ReadValue).
			dict["age"].Value.Should().Be(30L);
		}

		[U] public void Read_IsCaseInsensitive()
		{
			var dict = JsonSerializer.Deserialize<DynamicDictionary>(@"{""Name"":""bob""}", Options());
			dict["name"].Value.Should().Be("bob");
		}

		[U] public void Read_IntegralNumber_IsLong()
		{
			var dict = JsonSerializer.Deserialize<DynamicDictionary>(@"{""n"":5}", Options());
			dict["n"].Value.Should().BeOfType(typeof(long));
		}

		[U] public void Read_FractionalNumber_IsDouble()
		{
			var dict = JsonSerializer.Deserialize<DynamicDictionary>(@"{""n"":1.5}", Options());
			dict["n"].Value.Should().BeOfType(typeof(double));
		}

		[U] public void Read_BooleanAndNullValues()
		{
			var dict = JsonSerializer.Deserialize<DynamicDictionary>(@"{""ok"":true,""missing"":null}", Options());
			dict["ok"].Value.Should().Be(true);
			dict["missing"].Value.Should().BeNull();
		}

		[U] public void Read_NestedObject()
		{
			var dict = JsonSerializer.Deserialize<DynamicDictionary>(@"{""outer"":{""inner"":""v""}}", Options());
			// Nested values are traversed via Get<T>(path); the indexer only matches a single top-level key.
			dict.Get<string>("outer.inner").Should().Be("v");
		}

		[U] public void Write_Null_WritesNull()
		{
			JsonSerializer.Serialize<DynamicDictionary>(null, Options()).Should().Be("null");
		}

		// A fractional value that happens to be integral (e.g. 3.0) must round-trip with its trailing ".0" preserved.
		// System.Text.Json's WriteNumberValue(double) would emit "3", breaking dynamic response comparisons such as the
		// YAML runner's search.backpressure heap_variance assertion (expected "3.0", got "3").
		[U] public void Write_IntegralDouble_PreservesTrailingZero()
		{
			var dict = JsonSerializer.Deserialize<DynamicDictionary>(@"{""n"":3.0}", Options());
			dict["n"].Value.Should().BeOfType(typeof(double));

			var json = JsonSerializer.Serialize(dict, Options());
			json.Should().Contain("\"n\":3.0");
		}

		[U] public void Write_FractionalDouble_Preserved()
		{
			var dict = JsonSerializer.Deserialize<DynamicDictionary>(@"{""n"":1.5}", Options());
			var json = JsonSerializer.Serialize(dict, Options());
			json.Should().Contain("\"n\":1.5");
		}

		// Nested arrays/objects must recurse through the converter so integral doubles inside them keep ".0" too
		// (mirrors the YAML flat_object case: [["great",99.8],["ok",80.0]] must not render 80.0 as 80).
		[U] public void Write_NestedArrayOfMixedNumbers_PreservesTrailingZero()
		{
			const string json = @"{""review"":[[""great"",99.8],[""ok"",80.0]]}";
			var dict = JsonSerializer.Deserialize<DynamicDictionary>(json, Options());

			var roundTripped = JsonSerializer.Serialize(dict, Options());
			roundTripped.Should().Contain("80.0").And.Contain("99.8");
		}

		[U] public void Write_NestedObject_PreservesTrailingZero()
		{
			const string json = @"{""catalog"":{""rating"":9.0,""title"":""x""}}";
			var dict = JsonSerializer.Deserialize<DynamicDictionary>(json, Options());

			var roundTripped = JsonSerializer.Serialize(dict, Options());
			roundTripped.Should().Contain("\"rating\":9.0").And.Contain("\"title\":\"x\"");
		}
	}
}
