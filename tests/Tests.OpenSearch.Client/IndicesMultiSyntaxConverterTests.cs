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
	/// Validates the settings-aware <see cref="IndicesMultiSyntaxConverter"/>: it serializes <see cref="Indices"/>
	/// as a single comma-delimited JSON string, resolving index names through the runtime Inferrer.
	/// </summary>
	public class IndicesMultiSyntaxConverterTests
	{
		private static JsonSerializerOptions Options()
		{
			var settings = new ConnectionSettings();
			var options = new JsonSerializerOptions();
			options.Converters.Add(new IndicesMultiSyntaxConverter(settings));
			return options;
		}

		[U] public void Serialize_ManyIndices_WritesCommaDelimitedString()
		{
			Indices indices = Indices.Index("index-a", "index-b");
			var json = JsonSerializer.Serialize(indices, Options());
			json.Should().Be(@"""index-a,index-b""");
		}

		[U] public void Serialize_All_WritesAllString()
		{
			var json = JsonSerializer.Serialize(Indices.All, Options());
			json.Should().Be(@"""_all""");
		}

		[U] public void Serialize_Null_WritesNull()
		{
			var json = JsonSerializer.Serialize<Indices>(null, Options());
			json.Should().Be("null");
		}

		[U] public void Deserialize_String()
		{
			var indices = JsonSerializer.Deserialize<Indices>(@"""index-a""", Options());
			indices.Should().Be((Indices)"index-a");
		}
	}
}
