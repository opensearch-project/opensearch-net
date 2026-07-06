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
	/// Behavioural tests for <see cref="AnalyzerConverter"/>: polymorphic dispatch of an <see cref="IAnalyzer"/> by
	/// its JSON <c>type</c> discriminator (with a tokenizer-presence fallback for custom vs. language analyzers).
	/// </summary>
	public class AnalyzerConverterTests
	{
		private static JsonSerializerOptions Options()
		{
			var settings = new ConnectionSettings();
			var options = new JsonSerializerOptions
			{
				TypeInfoResolver = new HighLevelContractResolver(settings)
			};
			options.Converters.Add(new AnalyzerConverter());
			return options;
		}

		[U] public void Deserialize_KnownType_AsConcrete()
		{
			// The converter dispatches on the DOM "type" discriminator to the concrete StopAnalyzer. Member-level
			// mapping (e.g. stopwords_path) is not asserted here: the analyzer types are not yet wired into the
			// STJ data-contract resolver ([InterfaceDataContract]), so per-member DataMember names are out of scope.
			var analyzer = JsonSerializer.Deserialize<IAnalyzer>(
				@"{""type"":""stop"",""stopwords_path"":""stops.txt""}", Options());

			analyzer.Should().BeOfType<StopAnalyzer>();
		}

		[U] public void Deserialize_Standard_AsConcrete()
		{
			var analyzer = JsonSerializer.Deserialize<IAnalyzer>(@"{""type"":""standard""}", Options());
			analyzer.Should().BeOfType<StandardAnalyzer>();
		}

		[U] public void Deserialize_TokenizerPresentUnknownType_AsCustom()
		{
			var analyzer = JsonSerializer.Deserialize<IAnalyzer>(
				@"{""tokenizer"":""standard"",""filter"":[""lowercase""]}", Options());

			analyzer.Should().BeOfType<CustomAnalyzer>();
			var custom = (ICustomAnalyzer)analyzer;
			custom.Tokenizer.Should().Be("standard");
			custom.Filter.Should().ContainSingle().Which.Should().Be("lowercase");
		}

		[U] public void Deserialize_NoTokenizerUnknownType_AsLanguage()
		{
			// No tokenizer + a type the converter does not recognise falls back to LanguageAnalyzer. The concrete
			// type resolution is the converter's responsibility; the language discriminator round-trip depends on the
			// (not-yet-migrated) data-contract wiring of AnalyzerBase.Type and is out of scope.
			var analyzer = JsonSerializer.Deserialize<IAnalyzer>(
				@"{""type"":""english"",""stopwords_path"":""stops.txt""}", Options());

			analyzer.Should().BeOfType<LanguageAnalyzer>();
		}

		[U] public void Deserialize_Null_ReturnsNull()
		{
			var analyzer = JsonSerializer.Deserialize<IAnalyzer>("null", Options());
			analyzer.Should().BeNull();
		}

		[U] public void Serialize_KnownType_WritesType()
		{
			// The converter dispatches on value.Type to serialize via the IStopAnalyzer contract; we assert the
			// discriminator is emitted. Snake_case member names depend on the (not-yet-migrated) data-contract wiring.
			IAnalyzer analyzer = new StopAnalyzer { StopwordsPath = "stops.txt" };

			var json = JsonSerializer.Serialize(analyzer, Options());

			json.Should().Contain(@"""type""").And.Contain("stop");
		}

		[U] public void Serialize_Custom_WritesTokenizer()
		{
			IAnalyzer analyzer = new CustomAnalyzer { Tokenizer = "standard", Filter = new[] { "lowercase" } };

			var json = JsonSerializer.Serialize(analyzer, Options());

			json.Should().Contain(@"""tokenizer""").And.Contain("standard");
			json.Should().Contain(@"""filter""").And.Contain("lowercase");
		}

		[U] public void Serialize_Null_WritesNull() =>
			JsonSerializer.Serialize<IAnalyzer>(null, Options()).Should().Be("null");

		[U] public void RoundTrip_Stop()
		{
			IAnalyzer original = new StopAnalyzer { StopwordsPath = "stops.txt" };
			var json = JsonSerializer.Serialize(original, Options());
			var back = JsonSerializer.Deserialize<IAnalyzer>(json, Options());

			back.Should().BeOfType<StopAnalyzer>();
			((IStopAnalyzer)back).StopwordsPath.Should().Be("stops.txt");
		}
	}
}
