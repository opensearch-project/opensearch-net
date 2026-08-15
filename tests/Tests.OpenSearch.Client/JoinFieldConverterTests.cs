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
	/// Validates the settings-aware <see cref="JoinFieldConverter"/>: a parent relation serializes as a bare
	/// string while a child relation serializes as a <c>{ "name", "parent" }</c> object, resolving the
	/// <c>RelationName</c> and parent <c>Id</c> through the runtime Inferrer.
	/// </summary>
	public class JoinFieldConverterTests
	{
		private static JsonSerializerOptions Options()
		{
			var settings = new ConnectionSettings();
			var options = new JsonSerializerOptions();
			options.Converters.Add(new JoinFieldConverter(settings));
			options.Converters.Add(new IdConverter(settings));
			return options;
		}

		[U] public void Serialize_Parent_WritesString()
		{
			JoinField field = JoinField.Root("my-parent");
			var json = JsonSerializer.Serialize(field, Options());
			json.Should().Be(@"""my-parent""");
		}

		[U] public void Serialize_Child_WritesObject()
		{
			JoinField field = JoinField.Link("my-child", "parent-id");
			var json = JsonSerializer.Serialize(field, Options());
			json.Should().Be(@"{""name"":""my-child"",""parent"":""parent-id""}");
		}

		[U] public void Serialize_Null_WritesNull()
		{
			var json = JsonSerializer.Serialize<JoinField>(null, Options());
			json.Should().Be("null");
		}

		[U] public void Deserialize_String_AsParent()
		{
			var field = JsonSerializer.Deserialize<JoinField>(@"""my-parent""", Options());
			field.Should().NotBeNull();
			field.Match(p => p.Name, c => c.Name).Should().Be((RelationName)"my-parent");
		}

		[U] public void Deserialize_Object_AsChild()
		{
			var field = JsonSerializer.Deserialize<JoinField>(
				@"{""name"":""my-child"",""parent"":""parent-id""}", Options());
			field.Should().NotBeNull();
			field.Match(p => (RelationName)null, c => c.Name).Should().Be((RelationName)"my-child");
			field.Match(p => (Id)null, c => c.ParentId).Should().Be(new Id("parent-id"));
		}

		[U] public void RoundTrip_Child()
		{
			JoinField field = JoinField.Link("my-child", "parent-id");
			var json = JsonSerializer.Serialize(field, Options());
			var back = JsonSerializer.Deserialize<JoinField>(json, Options());
			back.Match(p => (RelationName)null, c => c.Name).Should().Be((RelationName)"my-child");
		}
	}
}
