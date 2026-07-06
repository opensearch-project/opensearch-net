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
	/// Validates the settings-aware <see cref="IdConverter"/>: it serializes string, long and document-based
	/// <see cref="Id"/> values, resolving the document id through the runtime Inferrer.
	/// </summary>
	public class IdConverterTests
	{
		private class Doc
		{
			public string Id { get; set; }
		}

		private static JsonSerializerOptions Options()
		{
			var settings = new ConnectionSettings();
			var options = new JsonSerializerOptions();
			options.Converters.Add(new IdConverter(settings));
			return options;
		}

		[U] public void Serialize_StringValue()
		{
			var json = JsonSerializer.Serialize(new Id("abc"), Options());
			json.Should().Be(@"""abc""");
		}

		[U] public void Serialize_LongValue()
		{
			var json = JsonSerializer.Serialize(new Id(42L), Options());
			json.Should().Be("42");
		}

		[U] public void Serialize_Document_ResolvesViaInferrer()
		{
			var json = JsonSerializer.Serialize(new Id(new Doc { Id = "doc-id" }), Options());
			json.Should().Be(@"""doc-id""");
		}

		[U] public void Serialize_Null_WritesNull()
		{
			var json = JsonSerializer.Serialize<Id>(null, Options());
			json.Should().Be("null");
		}

		[U] public void Deserialize_String()
		{
			var id = JsonSerializer.Deserialize<Id>(@"""abc""", Options());
			id.Should().Be(new Id("abc"));
		}

		[U] public void Deserialize_Number()
		{
			var id = JsonSerializer.Deserialize<Id>("42", Options());
			id.Should().Be(new Id(42L));
		}
	}
}
