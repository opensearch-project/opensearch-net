/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.Net.Serialization.Converters;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;

namespace Tests.OpenSearch.Client.Serialization
{
	/// <summary>
	/// Behavioural tests for <see cref="IndicesStatsDictionaryConverter"/>, the dedicated (non-generic) settings-aware
	/// replacement for the legacy Utf8Json <c>IndicesStatsDictionary.Converter</c>. The wire shape is a flat object
	/// keyed by index name whose values are <see cref="IndicesStats"/>; keys resolve through the runtime
	/// <c>Inferrer</c> (<see cref="IndexName"/>) and the parsed entries are wrapped in a <c>ResolvableDictionaryProxy</c>.
	/// </summary>
	public class IndicesStatsDictionaryConverterTests
	{
		private static JsonSerializerOptions Options()
		{
			var settings = new ConnectionSettings();
			var options = new JsonSerializerOptions
			{
				DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
				PropertyNameCaseInsensitive = true,
				TypeInfoResolver = new HighLevelContractResolver(settings)
			};
			options.Converters.Add(new IndicesStatsDictionaryConverter(settings));
			options.Converters.Add(new IndexNameConverter(settings));
			options.Converters.Add(new StringEnumConverterFactory());
			return options;
		}

		private static IndicesStatsDictionary Deserialize(string json) =>
			JsonSerializer.Deserialize<IndicesStatsDictionary>(Encoding.UTF8.GetBytes(json), Options());

		[U] public void Parses_Indices_AndResolvesKeys()
		{
			var json = @"{
				""my-index"": { ""uuid"": ""abc123"", ""primaries"": { ""docs"": { ""count"": 10 } } },
				""other-index"": { ""uuid"": ""def456"" }
			}";
			var dict = Deserialize(json);

			dict.Should().NotBeNull();
			dict.Count.Should().Be(2);

			// Key resolution: the proxy indexes by the Inferrer-resolved index name and exposes IndexName keys.
			dict.ResolvedKeys.Should().Contain("my-index").And.Contain("other-index");
			dict.Keys.Should().Contain((IndexName)"my-index");

			var myIndex = dict[(IndexName)"my-index"];
			myIndex.Should().NotBeNull();
			myIndex.Uuid.Should().Be("abc123");
			myIndex.Primaries.Should().NotBeNull();
			myIndex.Primaries.Documents.Count.Should().Be(10);
		}

		[U] public void Parses_EmptyObject()
		{
			var dict = Deserialize("{}");
			dict.Should().NotBeNull();
			dict.Count.Should().Be(0);
		}

		[U] public void Parses_Null_YieldsEmpty()
		{
			// HandleNull is opted in; a top-level null yields a non-null, empty proxy (matching the legacy formatter,
			// whose dictionary formatter returned null and the proxy treated as empty).
			var dict = Deserialize("null");
			dict.Should().NotBeNull();
			dict.Count.Should().Be(0);
		}

		[U] public void Write_Throws_NotSupported()
		{
			// The legacy formatter's Serialize threw NotSupportedException (a response-only dictionary).
			var dict = Deserialize(@"{ ""my-index"": { ""uuid"": ""abc"" } }");
			var act = () => JsonSerializer.Serialize(dict, Options());
			act.Should().Throw<System.NotSupportedException>();
		}
	}
}
