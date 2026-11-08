/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.Collections.Generic;
using System.Text.Json;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;

namespace Tests.OpenSearch.Client.Serialization
{
	/// <summary>
	/// Behavioural tests for <see cref="BucketsPathConverter"/>. An <see cref="IBucketsPath"/> is read from a string
	/// (<see cref="SingleBucketsPath"/>) or an object (<see cref="MultiBucketsPath"/>); any other token (array, null)
	/// yields null. On write a single path is a string, a multi path is an object, otherwise null.
	/// </summary>
	public class BucketsPathConverterTests
	{
		private static JsonSerializerOptions Options()
		{
			var options = new JsonSerializerOptions();
			options.Converters.Add(new BucketsPathConverter());
			return options;
		}

		private static IBucketsPath Deserialize(string json) =>
			JsonSerializer.Deserialize<IBucketsPath>(json, Options());

		// ---- read ----

		[U] public void Read_String_BecomesSingle()
		{
			var path = Deserialize(@"""my_bucket""");
			path.Should().BeOfType<SingleBucketsPath>();
			((SingleBucketsPath)path).BucketsPath.Should().Be("my_bucket");
		}

		[U] public void Read_Object_BecomesMulti()
		{
			var path = Deserialize(@"{""a"":""path_a"",""b"":""path_b""}");
			path.Should().BeOfType<MultiBucketsPath>();
			var multi = (MultiBucketsPath)path;
			multi.Should().Contain(new KeyValuePair<string, string>("a", "path_a"));
			multi.Should().Contain(new KeyValuePair<string, string>("b", "path_b"));
		}

		[U] public void Read_EmptyObject_BecomesEmptyMulti()
		{
			var path = Deserialize(@"{}");
			path.Should().BeOfType<MultiBucketsPath>();
			((MultiBucketsPath)path).Should().BeEmpty();
		}

		[U] public void Read_Array_ReturnsNull() => Deserialize(@"[""a"",""b""]").Should().BeNull();

		[U] public void Read_Null_ReturnsNull() => Deserialize("null").Should().BeNull();

		[U] public void Read_Number_ReturnsNull() => Deserialize("5").Should().BeNull();

		// ---- write ----

		[U] public void Write_Single_WritesString()
		{
			var json = JsonSerializer.Serialize<IBucketsPath>(new SingleBucketsPath("my_bucket"), Options());
			json.Should().Be(@"""my_bucket""");
		}

		[U] public void Write_Multi_WritesObject()
		{
			var multi = new MultiBucketsPath { { "a", "path_a" }, { "b", "path_b" } };
			var json = JsonSerializer.Serialize<IBucketsPath>(multi, Options());
			json.Should().Be(@"{""a"":""path_a"",""b"":""path_b""}");
		}

		[U] public void Write_Null_WritesNull() =>
			JsonSerializer.Serialize<IBucketsPath>(null, Options()).Should().Be("null");

		// ---- round trip ----

		[U] public void RoundTrip_Single()
		{
			var options = Options();
			var json = JsonSerializer.Serialize<IBucketsPath>(new SingleBucketsPath("p"), options);
			var back = JsonSerializer.Deserialize<IBucketsPath>(json, options);
			back.Should().BeOfType<SingleBucketsPath>();
			((SingleBucketsPath)back).BucketsPath.Should().Be("p");
		}

		[U] public void RoundTrip_Multi()
		{
			var options = Options();
			var multi = new MultiBucketsPath { { "x", "y" } };
			var json = JsonSerializer.Serialize<IBucketsPath>(multi, options);
			var back = JsonSerializer.Deserialize<IBucketsPath>(json, options);
			back.Should().BeOfType<MultiBucketsPath>();
			((MultiBucketsPath)back).Should().Contain(new KeyValuePair<string, string>("x", "y"));
		}
	}
}
