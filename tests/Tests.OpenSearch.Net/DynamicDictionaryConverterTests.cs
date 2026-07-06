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
	}
}
