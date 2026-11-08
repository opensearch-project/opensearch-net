/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.Collections.Generic;
using System.Text.Json;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;

namespace Tests.OpenSearch.Client.Serialization
{
	/// <summary>
	/// Validates the settings-aware <see cref="FieldConverter"/>: field-name inference through the runtime
	/// Inferrer, string vs object serialization based on <c>Format</c>, null handling, and the dictionary-key
	/// (property-name) direction in both read and write.
	/// </summary>
	public class FieldConverterTests
	{
		private static JsonSerializerOptions Options()
		{
			var settings = new ConnectionSettings();
			var options = new JsonSerializerOptions();
			options.Converters.Add(new FieldConverter(settings));
			return options;
		}

		[U] public void Serialize_Name_WritesString()
		{
			Field field = "my-field";
			var json = JsonSerializer.Serialize(field, Options());
			json.Should().Be(@"""my-field""");
		}

		[U] public void Serialize_WithFormat_WritesObject()
		{
			var field = new Field("created", format: "yyyy-MM-dd");
			var json = JsonSerializer.Serialize(field, Options());
			json.Should().Be(@"{""field"":""created"",""format"":""yyyy-MM-dd""}");
		}

		[U] public void Serialize_Null_WritesNull()
		{
			var json = JsonSerializer.Serialize<Field>(null, Options());
			json.Should().Be("null");
		}

		[U] public void Deserialize_String()
		{
			var result = JsonSerializer.Deserialize<Field>(@"""my-field""", Options());
			result.Should().Be((Field)"my-field");
		}

		[U] public void Deserialize_Object_WithFormat()
		{
			var result = JsonSerializer.Deserialize<Field>(
				@"{""field"":""created"",""boost"":2.0,""format"":""yyyy-MM-dd""}", Options());
			result.Name.Should().Be("created");
			result.Boost.Should().Be(2.0);
			result.Format.Should().Be("yyyy-MM-dd");
		}

		[U] public void Deserialize_Object_IgnoresUnknownMembers()
		{
			var result = JsonSerializer.Deserialize<Field>(
				@"{""field"":""created"",""unknown"":{""nested"":1},""format"":""epoch_millis""}", Options());
			result.Name.Should().Be("created");
			result.Format.Should().Be("epoch_millis");
		}

		[U] public void Deserialize_Null_ReturnsNull()
		{
			var result = JsonSerializer.Deserialize<Field>("null", Options());
			result.Should().BeNull();
		}

		[U] public void RoundTrip_WithFormat()
		{
			var field = new Field("created", format: "yyyy-MM-dd");
			var json = JsonSerializer.Serialize(field, Options());
			var back = JsonSerializer.Deserialize<Field>(json, Options());
			back.Name.Should().Be("created");
			back.Format.Should().Be("yyyy-MM-dd");
		}

		// Dictionary-key (property-name) direction: Field is used as a dictionary key.

		[U] public void Serialize_AsDictionaryKey_WritesFieldName()
		{
			var dict = new Dictionary<Field, int> { { "my-field", 1 } };
			var json = JsonSerializer.Serialize(dict, Options());
			json.Should().Be(@"{""my-field"":1}");
		}

		[U] public void Deserialize_AsDictionaryKey_ReadsFieldName()
		{
			var dict = JsonSerializer.Deserialize<Dictionary<Field, int>>(@"{""my-field"":1}", Options());
			dict.Should().ContainKey("my-field");
			dict["my-field"].Should().Be(1);
		}
	}
}
