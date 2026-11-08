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
	/// Validates the settings-aware <see cref="ChildrenConverter"/>: a single child serializes as a bare string,
	/// multiple children as a JSON array of strings, resolving each <c>RelationName</c> through the runtime Inferrer.
	/// </summary>
	public class ChildrenConverterTests
	{
		private static JsonSerializerOptions Options()
		{
			var settings = new ConnectionSettings();
			var options = new JsonSerializerOptions();
			options.Converters.Add(new ChildrenConverter(settings));
			return options;
		}

		[U] public void Serialize_SingleChild_WritesString()
		{
			var children = new Children("child-a");
			var json = JsonSerializer.Serialize(children, Options());
			json.Should().Be(@"""child-a""");
		}

		[U] public void Serialize_MultipleChildren_WritesArray()
		{
			var children = new Children("child-a", "child-b");
			var json = JsonSerializer.Serialize(children, Options());
			json.Should().Be(@"[""child-a"",""child-b""]");
		}

		[U] public void Serialize_Null_WritesNull()
		{
			var json = JsonSerializer.Serialize<Children>(null, Options());
			json.Should().Be("null");
		}

		[U] public void Serialize_Empty_WritesNull()
		{
			var json = JsonSerializer.Serialize(new Children(), Options());
			json.Should().Be("null");
		}

		[U] public void Deserialize_String()
		{
			var children = JsonSerializer.Deserialize<Children>(@"""child-a""", Options());
			children.Should().HaveCount(1);
			children[0].Should().Be((RelationName)"child-a");
		}

		[U] public void Deserialize_Array()
		{
			var children = JsonSerializer.Deserialize<Children>(@"[""child-a"",""child-b""]", Options());
			children.Should().HaveCount(2);
			children[0].Should().Be((RelationName)"child-a");
			children[1].Should().Be((RelationName)"child-b");
		}

		[U] public void RoundTrip_Array()
		{
			var children = new Children("child-a", "child-b");
			var json = JsonSerializer.Serialize(children, Options());
			var back = JsonSerializer.Deserialize<Children>(json, Options());
			back.Should().HaveCount(2);
		}
	}
}
