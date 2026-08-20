/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.Text;
using System.Text.Json;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;

namespace Tests.OpenSearch.Client.Serialization
{
	/// <summary>
	/// Behavioural tests for <see cref="LazyDocumentConverter"/> / <see cref="LazyDocumentInterfaceConverter"/>, the
	/// System.Text.Json replacements for the legacy Utf8Json <c>LazyDocumentFormatter</c> /
	/// <c>LazyDocumentInterfaceFormatter</c>. The crux is raw-JSON fidelity: reading captures the value verbatim, and
	/// writing re-emits it unindented, preserving number formatting.
	/// </summary>
	public class LazyDocumentConverterTests
	{
		private static JsonSerializerOptions Options()
		{
			var settings = new ConnectionSettings();
			var options = new JsonSerializerOptions();
			options.Converters.Add(new LazyDocumentConverter(settings));
			options.Converters.Add(new LazyDocumentInterfaceConverter(settings));
			return options;
		}

		private static LazyDocument Deserialize(string json) =>
			JsonSerializer.Deserialize<LazyDocument>(json, Options());

		private static string Roundtrip(string json) =>
			JsonSerializer.Serialize(Deserialize(json), Options());

		[U] public void Read_Object_CapturesRawBytesVerbatim()
		{
			var doc = Deserialize(@"{""a"":1,""b"":""x""}");
			doc.Should().NotBeNull();
			Encoding.UTF8.GetString(doc.Bytes).Should().Be(@"{""a"":1,""b"":""x""}");
		}

		[U] public void Read_Null_ReturnsNull() => Deserialize("null").Should().BeNull();

		[U] public void Roundtrip_Object_IsCompact()
		{
			// Indented input must be re-emitted unindented (compact), matching legacy WriteUnindented.
			Roundtrip("{\n  \"a\": 1,\n  \"b\": [1, 2, 3]\n}").Should().Be(@"{""a"":1,""b"":[1,2,3]}");
		}

		[U] public void Roundtrip_Array() => Roundtrip(@"[1,""two"",true,null]").Should().Be(@"[1,""two"",true,null]");

		[U] public void Roundtrip_NestedObject() =>
			Roundtrip(@"{""outer"":{""inner"":[1,2],""flag"":false}}").Should().Be(@"{""outer"":{""inner"":[1,2],""flag"":false}}");

		[U] public void Roundtrip_Scalars()
		{
			Roundtrip(@"""hello""").Should().Be(@"""hello""");
			Roundtrip("true").Should().Be("true");
			Roundtrip("false").Should().Be("false");
		}

		[U] public void Roundtrip_PreservesNumberPrecision()
		{
			// The raw number token must be preserved (no reformatting / precision loss).
			Roundtrip("12345678901234567890").Should().Be("12345678901234567890");
			Roundtrip("1.5").Should().Be("1.5");
			Roundtrip("1e10").Should().Be("1e10");
		}

		[U] public void Write_Null_WritesNull() =>
			JsonSerializer.Serialize<LazyDocument>(null, Options()).Should().Be("null");

		// ---- interface converter ----

		[U] public void Interface_Read_CapturesRawBytes()
		{
			var doc = JsonSerializer.Deserialize<ILazyDocument>(@"{""a"":1}", Options());
			doc.Should().BeOfType<LazyDocument>();
			Encoding.UTF8.GetString(((LazyDocument)doc).Bytes).Should().Be(@"{""a"":1}");
		}

		[U] public void Interface_Roundtrip_IsCompact() =>
			JsonSerializer.Serialize(JsonSerializer.Deserialize<ILazyDocument>("{\n  \"a\": 1\n}", Options()), Options())
				.Should().Be(@"{""a"":1}");

		[U] public void Interface_Read_Null_ReturnsNull() =>
			JsonSerializer.Deserialize<ILazyDocument>("null", Options()).Should().BeNull();

		[U] public void Interface_Write_Null_WritesNull() =>
			JsonSerializer.Serialize<ILazyDocument>(null, Options()).Should().Be("null");
	}
}
