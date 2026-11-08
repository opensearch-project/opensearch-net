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
	/// Validates the settings-aware <see cref="IndicesConverter"/>: it serializes <see cref="Indices"/> as a JSON
	/// array of index names, resolving each <see cref="IndexName"/> through the runtime Inferrer.
	/// </summary>
	public class IndicesConverterTests
	{
		private static JsonSerializerOptions Options()
		{
			var settings = new ConnectionSettings();
			var options = new JsonSerializerOptions();
			options.Converters.Add(new IndicesConverter(settings));
			return options;
		}

		[U] public void Serialize_ManyIndices_WritesArray()
		{
			Indices indices = Indices.Index("index-a", "index-b");
			var json = JsonSerializer.Serialize(indices, Options());
			json.Should().Be(@"[""index-a"",""index-b""]");
		}

		[U] public void Serialize_All_WritesAllArray()
		{
			var json = JsonSerializer.Serialize(Indices.All, Options());
			json.Should().Be(@"[""_all""]");
		}

		[U] public void Serialize_Null_WritesNull()
		{
			var json = JsonSerializer.Serialize<Indices>(null, Options());
			json.Should().Be("null");
		}

		[U] public void Deserialize_Array()
		{
			var indices = JsonSerializer.Deserialize<Indices>(@"[""index-a"",""index-b""]", Options());
			indices.Should().Be((Indices)Indices.Index("index-a", "index-b"));
		}

		[U] public void Deserialize_String()
		{
			var indices = JsonSerializer.Deserialize<Indices>(@"""index-a""", Options());
			indices.Should().Be((Indices)"index-a");
		}

		[U] public void RoundTrip()
		{
			Indices indices = Indices.Index("idx-1", "idx-2");
			var json = JsonSerializer.Serialize(indices, Options());
			var back = JsonSerializer.Deserialize<Indices>(json, Options());
			back.Should().Be(indices);
		}
	}
}
