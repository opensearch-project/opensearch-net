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
	/// Validates <see cref="PropertyNameConverter"/>: the settings-aware converter resolves a
	/// <see cref="PropertyName"/> through the runtime Inferrer when serializing and reads a JSON string
	/// back into a <see cref="PropertyName"/>.
	/// </summary>
	public class PropertyNameConverterTests
	{
		private static JsonSerializerOptions Options()
		{
			var settings = new ConnectionSettings();
			var options = new JsonSerializerOptions();
			options.Converters.Add(new PropertyNameConverter(settings));
			return options;
		}

		[U] public void Serialize_WritesResolvedName()
		{
			PropertyName name = "myProperty";
			var json = JsonSerializer.Serialize(name, Options());
			json.Should().Be(@"""myProperty""");
		}

		[U] public void Serialize_Null_WritesNull()
		{
			var json = JsonSerializer.Serialize<PropertyName>(null, Options());
			json.Should().Be("null");
		}

		[U] public void Deserialize_String()
		{
			var result = JsonSerializer.Deserialize<PropertyName>(@"""myProperty""", Options());
			result.Should().Be((PropertyName)"myProperty");
		}

		[U] public void Deserialize_NonString_ReturnsNull()
		{
			var result = JsonSerializer.Deserialize<PropertyName>("123", Options());
			result.Should().BeNull();
		}

		[U] public void RoundTrip()
		{
			PropertyName name = "field1";
			var json = JsonSerializer.Serialize(name, Options());
			var back = JsonSerializer.Deserialize<PropertyName>(json, Options());
			back.Should().Be((PropertyName)"field1");
		}
	}
}
