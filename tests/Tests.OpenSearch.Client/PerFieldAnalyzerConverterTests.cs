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
	/// Behavioural tests for the settings-aware <see cref="PerFieldAnalyzerConverter"/>: a dictionary of
	/// <see cref="Field"/> -> analyzer name serialized as a JSON object. Field keys are resolved through the runtime
	/// Inferrer (like the legacy <c>VerbatimDictionaryKeysFormatter</c> <c>_keyIsField</c> branch), which is why this
	/// converter is settings-aware rather than reusing the settings-independent shared
	/// <see cref="VerbatimDictionaryKeysConverter{TDictionary,TInterface,TKey,TValue}"/>.
	/// </summary>
	public class PerFieldAnalyzerConverterTests
	{
		private static JsonSerializerOptions Options()
		{
			var settings = new ConnectionSettings();
			var options = new JsonSerializerOptions();
			options.Converters.Add(new PerFieldAnalyzerConverter(settings));
			return options;
		}

		private static IPerFieldAnalyzer Deserialize(string json) =>
			JsonSerializer.Deserialize<IPerFieldAnalyzer>(json, Options());

		[U] public void Serialize_WritesFieldToAnalyzerObject()
		{
			IPerFieldAnalyzer analyzer = new PerFieldAnalyzer
			{
				{ "name", "standard" },
				{ "description", "english" }
			};

			var json = JsonSerializer.Serialize(analyzer, Options());
			json.Should().Contain(@"""name"":""standard""").And.Contain(@"""description"":""english""");
		}

		[U] public void Serialize_ResolvesFieldKeyThroughInferrer()
		{
			// A string implicitly becomes a Field whose name resolves verbatim through the Inferrer.
			IPerFieldAnalyzer analyzer = new PerFieldAnalyzer { { "my-field", "keyword" } };
			var json = JsonSerializer.Serialize(analyzer, Options());
			json.Should().Be(@"{""my-field"":""keyword""}");
		}

		[U] public void Serialize_SkipsNullValues()
		{
			IPerFieldAnalyzer analyzer = new PerFieldAnalyzer
			{
				{ "present", "standard" },
				{ "absent", null }
			};

			var json = JsonSerializer.Serialize(analyzer, Options());
			json.Should().Contain(@"""present""").And.NotContain(@"""absent""");
		}

		[U] public void Serialize_Null_WritesNull()
		{
			var json = JsonSerializer.Serialize<IPerFieldAnalyzer>(null, Options());
			json.Should().Be("null");
		}

		[U] public void Serialize_Empty_WritesEmptyObject()
		{
			IPerFieldAnalyzer analyzer = new PerFieldAnalyzer();
			var json = JsonSerializer.Serialize(analyzer, Options());
			json.Should().Be(@"{}");
		}

		[U] public void Deserialize_ReadsFieldToAnalyzerObject()
		{
			var analyzer = Deserialize(@"{""name"":""standard"",""description"":""english""}");
			analyzer.Should().NotBeNull();
			analyzer[(Field)"name"].Should().Be("standard");
			analyzer[(Field)"description"].Should().Be("english");
		}

		[U] public void Deserialize_Empty_ReturnsEmpty()
		{
			var analyzer = Deserialize(@"{}");
			analyzer.Should().NotBeNull();
			analyzer.Count.Should().Be(0);
		}

		[U] public void Deserialize_Null_ReturnsEmpty()
		{
			var analyzer = Deserialize("null");
			analyzer.Should().NotBeNull();
			analyzer.Count.Should().Be(0);
		}

		[U] public void RoundTrip()
		{
			IPerFieldAnalyzer analyzer = new PerFieldAnalyzer
			{
				{ "name", "standard" },
				{ "title", "english" }
			};

			var json = JsonSerializer.Serialize(analyzer, Options());
			var back = JsonSerializer.Deserialize<IPerFieldAnalyzer>(json, Options());

			back.Should().NotBeNull();
			back[(Field)"name"].Should().Be("standard");
			back[(Field)"title"].Should().Be("english");
		}
	}
}
