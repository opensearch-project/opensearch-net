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
	/// Validates the System.Text.Json <see cref="DynamicTemplatesConverter" /> and
	/// <see cref="DynamicTemplatesInterfaceConverter" /> that replace the legacy Utf8Json
	/// <c>DynamicTemplatesFormatter</c> / <c>DynamicTemplatesInterfaceFormatter</c>.
	///
	/// A <see cref="DynamicTemplateContainer" /> is serialized as a JSON array of single-property
	/// objects: <c>[ { "name": { ...template... } } ]</c>.
	/// </summary>
	public class DynamicTemplatesConverterTests
	{
		// Reproduce the runtime serializer wiring the converters rely on: [ReadAs] delegation so
		// IDynamicTemplate deserializes to the concrete DynamicTemplate, plus the two converters
		// under test registered directly (the production pipeline registration is out of scope here).
		private static JsonSerializerOptions Options()
		{
			var settings = new ConnectionSettings();
			var options = new JsonSerializerOptions
			{
				TypeInfoResolver = new HighLevelContractResolver(settings)
			};
			options.Converters.Add(new ReadAsConverterFactory());
			options.Converters.Add(new global::OpenSearch.Net.Serialization.Converters.StringEnumConverterFactory());
			options.Converters.Add(new DynamicTemplatesInterfaceConverter());
			options.Converters.Add(new DynamicTemplatesConverter());
			return options;
		}

		private static DynamicTemplateContainer SampleContainer()
		{
			var container = new DynamicTemplateContainer();
			container.Add("strings", new DynamicTemplate
			{
				Match = "*",
				MatchMappingType = "string"
			});
			return container;
		}

		[U] public void Serialize_WritesArrayOfSingleKeyObjects()
		{
			var json = JsonSerializer.Serialize(SampleContainer(), Options());

			// Array of a single { "strings": { ... } } object.
			json.Should().StartWith("[").And.EndWith("]");
			json.Should().Contain(@"""strings""");
			json.Should().Contain(@"""match"":""*""");
			json.Should().Contain(@"""match_mapping_type"":""string""");
		}

		[U] public void Serialize_EmptyContainer_WritesEmptyArray()
		{
			var json = JsonSerializer.Serialize(new DynamicTemplateContainer(), Options());
			json.Should().Be("[]");
		}

		[U] public void Serialize_Null_WritesNull()
		{
			// System.Text.Json short-circuits a top-level null reference to "null" before the converter's
			// Write is invoked, so the container converter never sees it. This is standard STJ behaviour.
			var json = JsonSerializer.Serialize<DynamicTemplateContainer>(null, Options());
			json.Should().Be("null");
		}

		[U] public void Deserialize_ReadsArrayOfSingleKeyObjects()
		{
			const string json =
				@"[{""strings"":{""match"":""*"",""match_mapping_type"":""string"",""match_pattern"":""regex""}}]";

			var container = JsonSerializer.Deserialize<DynamicTemplateContainer>(json, Options());

			container.Should().NotBeNull();
			var dict = (IDictionary<string, IDynamicTemplate>)container;
			dict.Should().HaveCount(1);
			dict.Should().ContainKey("strings");

			var template = container["strings"];
			template.Should().BeOfType<DynamicTemplate>();
			template.Match.Should().Be("*");
			template.MatchMappingType.Should().Be("string");
			template.MatchPattern.Should().Be(MatchType.Regex);
		}

		[U] public void Deserialize_MultipleTemplates_PreservesEachEntry()
		{
			const string json =
				@"[{""strings"":{""match"":""s*""}},{""longs"":{""match"":""l*""}}]";

			var container = JsonSerializer.Deserialize<DynamicTemplateContainer>(json, Options());

			var dict = (IDictionary<string, IDynamicTemplate>)container;
			dict.Should().HaveCount(2);
			dict.Keys.Should().BeEquivalentTo(new[] { "strings", "longs" });
			container["strings"].Match.Should().Be("s*");
			container["longs"].Match.Should().Be("l*");
		}

		[U] public void Deserialize_Null_ReturnsNull()
		{
			var container = JsonSerializer.Deserialize<DynamicTemplateContainer>("null", Options());
			container.Should().BeNull();
		}

		[U] public void RoundTrips()
		{
			var options = Options();
			var json = JsonSerializer.Serialize(SampleContainer(), options);
			var back = JsonSerializer.Deserialize<DynamicTemplateContainer>(json, options);

			((IDictionary<string, IDynamicTemplate>)back).Should().HaveCount(1);
			back["strings"].Match.Should().Be("*");
			back["strings"].MatchMappingType.Should().Be("string");
		}

		[U] public void InterfaceConverter_SerializesSameShapeAsConcrete()
		{
			var options = Options();
			DynamicTemplateContainer concrete = SampleContainer();
			IDynamicTemplateContainer asInterface = concrete;

			var concreteJson = JsonSerializer.Serialize(concrete, options);
			var interfaceJson = JsonSerializer.Serialize(asInterface, options);

			interfaceJson.Should().Be(concreteJson);
		}
	}
}
