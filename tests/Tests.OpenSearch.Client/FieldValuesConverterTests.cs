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
	/// Behavioural tests for <see cref="FieldValuesConverter"/>, the System.Text.Json replacement for the legacy
	/// Utf8Json <c>FieldValuesFormatter</c>. A <see cref="FieldValues"/> is a dictionary of field-name → the field's
	/// values captured verbatim as a <see cref="LazyDocument"/>; writing emits each value's raw bytes.
	/// </summary>
	public class FieldValuesConverterTests
	{
		private static JsonSerializerOptions Options()
		{
			var settings = new ConnectionSettings();
			var options = new JsonSerializerOptions();
			options.Converters.Add(new FieldValuesConverter(settings));
			options.Converters.Add(new LazyDocumentConverter(settings));
			return options;
		}

		private static FieldValues Deserialize(string json) =>
			JsonSerializer.Deserialize<FieldValues>(json, Options());

		[U] public void Read_Object_CapturesEachFieldsValues()
		{
			var values = Deserialize(@"{""name"":[""foo""],""age"":[42]}");
			values.Should().NotBeNull();
			// Values are coerced through the inferrer-backed accessors.
			values.ValuesOf<string>("name").Should().BeEquivalentTo(new[] { "foo" });
			values.ValuesOf<int>("age").Should().BeEquivalentTo(new[] { 42 });
		}

		[U] public void Read_NonObject_ReturnsNull() => Deserialize(@"[1,2,3]").Should().BeNull();

		[U] public void Read_EmptyObject_ReturnsEmpty()
		{
			var values = Deserialize(@"{}");
			values.Should().NotBeNull();
			// FieldValues.ValuesOf returns null (not an empty array) for an unknown field — existing behavior.
			values.ValuesOf<string>("missing").Should().BeNull();
		}

		[U] public void Roundtrip_EmitsRawValues()
		{
			var json = @"{""name"":[""foo""],""age"":[42]}";
			JsonSerializer.Serialize(Deserialize(json), Options()).Should().Be(json);
		}

		[U] public void Write_Null_WritesNull() =>
			JsonSerializer.Serialize<FieldValues>(null, Options()).Should().Be("null");
	}
}
