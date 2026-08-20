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
	/// Validates the settings-aware <see cref="RelationNameConverter"/>: the converter is constructed with
	/// <see cref="IConnectionSettingsValues"/> and uses the runtime Inferrer to resolve a <c>RelationName</c>.
	/// </summary>
	public class RelationNameConverterTests
	{
		private static JsonSerializerOptions Options()
		{
			var settings = new ConnectionSettings();
			var options = new JsonSerializerOptions();
			options.Converters.Add(new RelationNameConverter(settings));
			return options;
		}

		[U] public void Serialize_WritesString()
		{
			RelationName relation = "my-relation";
			var json = JsonSerializer.Serialize(relation, Options());
			json.Should().Be(@"""my-relation""");
		}

		[U] public void Serialize_Null_WritesNull()
		{
			var json = JsonSerializer.Serialize<RelationName>(null, Options());
			json.Should().Be("null");
		}

		[U] public void Deserialize_String()
		{
			var result = JsonSerializer.Deserialize<RelationName>(@"""my-relation""", Options());
			result.Should().Be((RelationName)"my-relation");
		}

		[U] public void Deserialize_NonString_ReturnsNull()
		{
			var result = JsonSerializer.Deserialize<RelationName>("123", Options());
			result.Should().BeNull();
		}

		[U] public void RoundTrip()
		{
			RelationName relation = "rel";
			var json = JsonSerializer.Serialize(relation, Options());
			var back = JsonSerializer.Deserialize<RelationName>(json, Options());
			back.Should().Be((RelationName)"rel");
		}
	}
}
