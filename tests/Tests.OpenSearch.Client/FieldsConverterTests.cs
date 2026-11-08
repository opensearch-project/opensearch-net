/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.Linq;
using System.Text.Json;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;

namespace Tests.OpenSearch.Client.Serialization
{
	/// <summary>
	/// Validates the settings-aware <see cref="FieldsConverter"/>: a <c>Fields</c> collection is serialized as a JSON
	/// array of fields (each delegated to <see cref="FieldConverter"/>), a non-array token reads as <c>null</c>, and
	/// null / null-element handling matches the legacy formatter.
	/// </summary>
	public class FieldsConverterTests
	{
		private static JsonSerializerOptions Options()
		{
			var settings = new ConnectionSettings();
			var options = new JsonSerializerOptions();
			// FieldsConverter delegates to a FieldConverter internally, so only the collection converter is needed.
			options.Converters.Add(new FieldsConverter(settings));
			return options;
		}

		[U] public void Serialize_WritesArrayOfStrings()
		{
			Fields fields = new[] { "field1", "field2" };
			var json = JsonSerializer.Serialize(fields, Options());
			json.Should().Be(@"[""field1"",""field2""]");
		}

		[U] public void Serialize_FieldWithFormat_WritesObjectElement()
		{
			var fields = new Fields().And("plain").And("created", format: "yyyy-MM-dd");
			var json = JsonSerializer.Serialize(fields, Options());
			json.Should().Be(@"[""plain"",{""field"":""created"",""format"":""yyyy-MM-dd""}]");
		}

		[U] public void Serialize_Empty_WritesEmptyArray()
		{
			var fields = new Fields();
			var json = JsonSerializer.Serialize(fields, Options());
			json.Should().Be("[]");
		}

		[U] public void Serialize_Null_WritesNull()
		{
			var json = JsonSerializer.Serialize<Fields>(null, Options());
			json.Should().Be("null");
		}

		[U] public void Deserialize_Array()
		{
			var result = JsonSerializer.Deserialize<Fields>(@"[""field1"",""field2""]", Options());
			result.Should().NotBeNull();
			result.Select(f => f.Name).Should().Equal("field1", "field2");
		}

		[U] public void Deserialize_NonArray_ReturnsNull()
		{
			var result = JsonSerializer.Deserialize<Fields>(@"""field1""", Options());
			result.Should().BeNull();
		}

		[U] public void Deserialize_DropsNullElements()
		{
			var result = JsonSerializer.Deserialize<Fields>(@"[""field1"",null,""field2""]", Options());
			result.Should().NotBeNull();
			result.Select(f => f.Name).Should().Equal("field1", "field2");
		}

		[U] public void RoundTrip()
		{
			Fields fields = new[] { "field1", "field2" };
			var json = JsonSerializer.Serialize(fields, Options());
			var back = JsonSerializer.Deserialize<Fields>(json, Options());
			back.Select(f => f.Name).Should().Equal("field1", "field2");
		}
	}
}
