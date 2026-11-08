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
	/// Behavioural tests for the open-generic <see cref="SuggestDictionaryConverter{T}"/> and its
	/// <see cref="SuggestDictionaryConverterFactory"/>. The dictionary maps a suggestion name to an array of
	/// <see cref="ISuggest{T}"/> results; keys are plain strings (no Inferrer). The factory constructs the closed
	/// converter per document type from the legacy <c>[JsonFormatter(typeof(SuggestDictionaryFormatter&lt;&gt;))]</c>
	/// attribute on <see cref="ISuggestDictionary{T}"/>.
	/// </summary>
	public class SuggestDictionaryConverterTests
	{
		// A minimal document type; ISuggest<T> requires T : class.
		public class Doc
		{
			public string Name { get; set; }
		}

		private static JsonSerializerOptions Options()
		{
			var settings = new ConnectionSettings();
			var options = new JsonSerializerOptions
			{
				// The value type ISuggest<T>[] deserializes to the concrete Suggest<T> via [ReadAs].
				TypeInfoResolver = new HighLevelContractResolver(settings)
			};
			options.Converters.Add(new ReadAsConverterFactory());
			options.Converters.Add(new SuggestDictionaryConverterFactory());
			return options;
		}

		private static ISuggestDictionary<Doc> Deserialize(string json) =>
			JsonSerializer.Deserialize<ISuggestDictionary<Doc>>(json, Options());

		[U] public void Factory_CanConvert_ClosedInterfaceOnly()
		{
			var factory = new SuggestDictionaryConverterFactory();
			factory.CanConvert(typeof(ISuggestDictionary<Doc>)).Should().BeTrue();
			factory.CanConvert(typeof(SuggestDictionary<Doc>)).Should().BeFalse();
			factory.CanConvert(typeof(string)).Should().BeFalse();
		}

		[U] public void Deserialize_ReadsNamedSuggestions()
		{
			var dict = Deserialize(
				@"{""my-suggest"":[{""text"":""hello"",""offset"":0,""length"":5,""options"":[]}]}");

			dict.Should().NotBeNull();
			dict.Keys.Should().Contain("my-suggest");
			var results = dict["my-suggest"];
			results.Should().HaveCount(1);
			results[0].Text.Should().Be("hello");
			results[0].Length.Should().Be(5);
		}

		[U] public void Deserialize_MultipleEntries()
		{
			var dict = Deserialize(
				@"{""a"":[{""text"":""x"",""options"":[]}],""b"":[{""text"":""y"",""options"":[]}]}");

			dict.Keys.Should().BeEquivalentTo(new[] { "a", "b" });
			dict["a"][0].Text.Should().Be("x");
			dict["b"][0].Text.Should().Be("y");
		}

		[U] public void Deserialize_TypedKeys_AreSanitized()
		{
			// typed_keys=true returns suggest keys as "<type>#<name>"; SuggestDictionary.Sanitize strips the prefix.
			var dict = Deserialize(@"{""term#my-suggest"":[{""text"":""hello"",""options"":[]}]}");
			dict.Keys.Should().Contain("my-suggest");
			dict.ContainsKey("my-suggest").Should().BeTrue();
		}

		[U] public void Deserialize_Empty_ReturnsEmptyDictionary()
		{
			var dict = Deserialize(@"{}");
			dict.Should().NotBeNull();
			dict.Keys.Should().BeEmpty();
		}

		[U] public void Deserialize_Null_ReturnsEmptyDictionary()
		{
			var dict = Deserialize("null");
			dict.Should().NotBeNull();
			dict.Keys.Should().BeEmpty();
		}

		[U] public void Serialize_WritesObjectKeyedBySuggestName()
		{
			var backing = new Dictionary<string, ISuggest<Doc>[]>
			{
				{ "my-suggest", new ISuggest<Doc>[] { new Suggest<Doc> { Text = "hello", Length = 5 } } }
			};
			ISuggestDictionary<Doc> dict = new SuggestDictionary<Doc>(backing);

			var json = JsonSerializer.Serialize(dict, Options());
			json.Should().Contain(@"""my-suggest""").And.Contain(@"""text"":""hello""");
		}

		[U] public void Serialize_Null_WritesNull()
		{
			var json = JsonSerializer.Serialize<ISuggestDictionary<Doc>>(null, Options());
			json.Should().Be("null");
		}

		[U] public void Serialize_SkipsNullValueArrays()
		{
			var backing = new Dictionary<string, ISuggest<Doc>[]>
			{
				{ "present", new ISuggest<Doc>[] { new Suggest<Doc> { Text = "x" } } },
				{ "absent", null }
			};
			ISuggestDictionary<Doc> dict = new SuggestDictionary<Doc>(backing);

			var json = JsonSerializer.Serialize(dict, Options());
			json.Should().Contain(@"""present""").And.NotContain(@"""absent""");
		}

		[U] public void RoundTrip()
		{
			var backing = new Dictionary<string, ISuggest<Doc>[]>
			{
				{ "my-suggest", new ISuggest<Doc>[] { new Suggest<Doc> { Text = "hello", Length = 5, Offset = 0 } } }
			};
			ISuggestDictionary<Doc> dict = new SuggestDictionary<Doc>(backing);

			var json = JsonSerializer.Serialize(dict, Options());
			var back = JsonSerializer.Deserialize<ISuggestDictionary<Doc>>(json, Options());

			back.Keys.Should().Contain("my-suggest");
			back["my-suggest"].Single().Text.Should().Be("hello");
			back["my-suggest"].Single().Length.Should().Be(5);
		}
	}
}
