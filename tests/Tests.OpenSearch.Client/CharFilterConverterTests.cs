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
	/// Behavioural tests for <see cref="CharFilterConverter"/>: dispatches an <see cref="ICharFilter"/> to the
	/// concrete type named by the <c>type</c> discriminator field, and serializes by runtime type.
	/// </summary>
	public class CharFilterConverterTests
	{
		private static JsonSerializerOptions Options()
		{
			var settings = new ConnectionSettings();
			var options = new JsonSerializerOptions
			{
				TypeInfoResolver = new HighLevelContractResolver(settings)
			};
			options.Converters.Add(new CharFilterConverter());
			return options;
		}

		[U] public void Deserialize_HtmlStrip()
		{
			var filter = JsonSerializer.Deserialize<ICharFilter>(@"{""type"":""html_strip""}", Options());

			filter.Should().BeOfType<HtmlStripCharFilter>();
			filter.Type.Should().Be("html_strip");
		}

		[U] public void Deserialize_Mapping()
		{
			// NOTE: the shared HighLevelContractResolver only honours [DataMember(Name=...)] snake_case naming for
			// interfaces marked [InterfaceDataContract]; ICharFilter / IMappingCharFilter are not so marked, so the
			// resolver camelCases the CLR names. "mappings" happens to match camelCase(Mappings); "mappings_path"
			// would arrive as "mappingsPath" and is therefore not asserted here (see task report).
			var filter = JsonSerializer.Deserialize<ICharFilter>(
				@"{""type"":""mapping"",""mappings"":[""ab""]}", Options());

			filter.Should().BeOfType<MappingCharFilter>();
			var mapping = (IMappingCharFilter)filter;
			mapping.Mappings.Should().ContainSingle().Which.Should().Be("ab");
		}

		[U] public void Deserialize_PatternReplace()
		{
			var filter = JsonSerializer.Deserialize<ICharFilter>(
				@"{""type"":""pattern_replace"",""pattern"":""\\d"",""replacement"":""#"",""flags"":""CASE_INSENSITIVE""}", Options());

			filter.Should().BeOfType<PatternReplaceCharFilter>();
			var pr = (IPatternReplaceCharFilter)filter;
			pr.Pattern.Should().Be(@"\d");
			pr.Replacement.Should().Be("#");
			pr.Flags.Should().Be("CASE_INSENSITIVE");
		}

		[U] public void Deserialize_UnknownType_ReturnsNull()
		{
			var filter = JsonSerializer.Deserialize<ICharFilter>(@"{""type"":""does_not_exist""}", Options());
			filter.Should().BeNull();
		}

		[U] public void Deserialize_MissingType_ReturnsNull()
		{
			var filter = JsonSerializer.Deserialize<ICharFilter>(@"{""foo"":""bar""}", Options());
			filter.Should().BeNull();
		}

		[U] public void Deserialize_Null_ReturnsNull()
		{
			var filter = JsonSerializer.Deserialize<ICharFilter>("null", Options());
			filter.Should().BeNull();
		}

		[U] public void Serialize_ByRuntimeType()
		{
			ICharFilter filter = new MappingCharFilter { Mappings = new[] { "ab" } };

			var json = JsonSerializer.Serialize(filter, Options());

			json.Should().Contain(@"""type"":""mapping""");
			json.Should().Contain(@"""mappings""").And.Contain("ab");
		}

		[U] public void Serialize_Null_WritesNull()
		{
			JsonSerializer.Serialize<ICharFilter>(null, Options()).Should().Be("null");
		}

		[U] public void RoundTrip_PatternReplace()
		{
			ICharFilter original = new PatternReplaceCharFilter { Pattern = @"\d", Replacement = "#" };

			var json = JsonSerializer.Serialize(original, Options());
			var back = JsonSerializer.Deserialize<ICharFilter>(json, Options());

			back.Should().BeOfType<PatternReplaceCharFilter>();
			var pr = (IPatternReplaceCharFilter)back;
			pr.Pattern.Should().Be(@"\d");
			pr.Replacement.Should().Be("#");
		}
	}
}
