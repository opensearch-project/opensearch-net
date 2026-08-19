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
	/// Validates the settings-aware <see cref="IndexNameConverter"/>: the converter is constructed with
	/// <see cref="IConnectionSettingsValues"/> and uses the runtime Inferrer to resolve an <c>IndexName</c>.
	/// </summary>
	public class IndexNameConverterTests
	{
		private static JsonSerializerOptions Options()
		{
			var settings = new ConnectionSettings();
			var options = new JsonSerializerOptions();
			options.Converters.Add(new IndexNameConverter(settings));
			return options;
		}

		[U] public void Serialize_WritesString()
		{
			IndexName index = "my-index";
			var json = JsonSerializer.Serialize(index, Options());
			json.Should().Be(@"""my-index""");
		}

		[U] public void Serialize_Null_WritesNull()
		{
			var json = JsonSerializer.Serialize<IndexName>(null, Options());
			json.Should().Be("null");
		}

		[U] public void Deserialize_String()
		{
			var result = JsonSerializer.Deserialize<IndexName>(@"""my-index""", Options());
			result.Should().Be((IndexName)"my-index");
		}

		[U] public void Deserialize_NonString_ReturnsNull()
		{
			var result = JsonSerializer.Deserialize<IndexName>("123", Options());
			result.Should().BeNull();
		}

		[U] public void RoundTrip()
		{
			IndexName index = "idx";
			var json = JsonSerializer.Serialize(index, Options());
			var back = JsonSerializer.Deserialize<IndexName>(json, Options());
			back.Should().Be((IndexName)"idx");
		}
	}
}
