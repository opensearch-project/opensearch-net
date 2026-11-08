/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;

namespace Tests.OpenSearch.Client.Serialization
{
	/// <summary>
	/// Behavioural tests for <see cref="StopWordsConverter"/>. <see cref="StopWords"/> is a union of a single
	/// (CSV) string or an array of strings. On read the JSON token dispatches the branch (array => string
	/// collection, tag 1; anything else => single string, tag 0). On write the legacy shape is preserved: the
	/// string branch writes a bare string, the collection branch writes a JSON array.
	/// </summary>
	public class StopWordsConverterTests
	{
		private static JsonSerializerOptions Options()
		{
			var options = new JsonSerializerOptions();
			options.Converters.Add(new StopWordsConverter());
			return options;
		}

		[U] public void Read_SingleString_CsvBranch()
		{
			var stopWords = JsonSerializer.Deserialize<StopWords>(@"""the,a,an""", Options());
			stopWords.Should().NotBeNull();
			stopWords.Tag.Should().Be(0);
			stopWords.Item1.Should().Be("the,a,an");
		}

		[U] public void Read_PredefinedString_CsvBranch()
		{
			// A predefined language stopword set (e.g. "_english_") is a plain string, not an array.
			var stopWords = JsonSerializer.Deserialize<StopWords>(@"""_english_""", Options());
			stopWords.Tag.Should().Be(0);
			stopWords.Item1.Should().Be("_english_");
		}

		[U] public void Read_Array_CollectionBranch()
		{
			var stopWords = JsonSerializer.Deserialize<StopWords>(@"[""the"",""a"",""an""]", Options());
			stopWords.Should().NotBeNull();
			stopWords.Tag.Should().Be(1);
			stopWords.Item2.Should().BeEquivalentTo(new[] { "the", "a", "an" });
		}

		[U] public void Read_EmptyArray_CollectionBranch()
		{
			var stopWords = JsonSerializer.Deserialize<StopWords>(@"[]", Options());
			stopWords.Tag.Should().Be(1);
			stopWords.Item2.Should().NotBeNull().And.BeEmpty();
		}

		[U] public void Read_Null_ReturnsNull()
		{
			var stopWords = JsonSerializer.Deserialize<StopWords>("null", Options());
			stopWords.Should().BeNull();
		}

		[U] public void Write_StringBranch_WritesBareString()
		{
			var json = JsonSerializer.Serialize(new StopWords("the,a,an"), Options());
			json.Should().Be(@"""the,a,an""");
		}

		[U] public void Write_CollectionBranch_WritesArray()
		{
			var json = JsonSerializer.Serialize(new StopWords(new List<string> { "the", "a" }), Options());
			json.Should().Be(@"[""the"",""a""]");
		}

		[U] public void Write_Null_WritesNull()
		{
			var json = JsonSerializer.Serialize<StopWords>(null, Options());
			json.Should().Be("null");
		}

		[U] public void RoundTrip_StringBranch()
		{
			var options = Options();
			var json = JsonSerializer.Serialize(new StopWords("_english_"), options);
			var back = JsonSerializer.Deserialize<StopWords>(json, options);
			back.Tag.Should().Be(0);
			back.Item1.Should().Be("_english_");
		}

		[U] public void RoundTrip_CollectionBranch()
		{
			var options = Options();
			var json = JsonSerializer.Serialize(new StopWords(new[] { "x", "y", "z" }), options);
			var back = JsonSerializer.Deserialize<StopWords>(json, options);
			back.Tag.Should().Be(1);
			back.Item2.ToArray().Should().Equal("x", "y", "z");
		}
	}
}
