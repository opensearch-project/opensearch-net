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
	/// Behavioural tests for <see cref="NormalizerConverter"/>: deserializes INormalizer as the concrete
	/// CustomNormalizer and serializes via the ICustomNormalizer contract.
	/// </summary>
	public class NormalizerConverterTests
	{
		private static JsonSerializerOptions Options()
		{
			var settings = new ConnectionSettings();
			var options = new JsonSerializerOptions
			{
				TypeInfoResolver = new HighLevelContractResolver(settings)
			};
			options.Converters.Add(new NormalizerConverter());
			return options;
		}

		[U] public void Deserialize_AsCustomNormalizer()
		{
			var norm = JsonSerializer.Deserialize<INormalizer>(
				@"{""type"":""custom"",""filter"":[""lowercase""],""char_filter"":[""html_strip""]}", Options());

			norm.Should().BeOfType<CustomNormalizer>();
			var custom = (ICustomNormalizer)norm;
			custom.Filter.Should().ContainSingle().Which.Should().Be("lowercase");
			custom.CharFilter.Should().ContainSingle().Which.Should().Be("html_strip");
		}

		[U] public void Deserialize_Null_ReturnsNull()
		{
			var norm = JsonSerializer.Deserialize<INormalizer>("null", Options());
			norm.Should().BeNull();
		}

		[U] public void Serialize_WritesFilters()
		{
			INormalizer norm = new CustomNormalizer
			{
				Filter = new[] { "lowercase" },
				CharFilter = new[] { "html_strip" }
			};

			var json = JsonSerializer.Serialize(norm, Options());

			json.Should().Contain(@"""filter""").And.Contain("lowercase");
			json.Should().Contain(@"""char_filter""").And.Contain("html_strip");
		}

		[U] public void Serialize_Null_WritesNull()
		{
			JsonSerializer.Serialize<INormalizer>(null, Options()).Should().Be("null");
		}
	}
}
