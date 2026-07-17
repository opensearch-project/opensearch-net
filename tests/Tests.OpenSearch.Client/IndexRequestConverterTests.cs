/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.Net; // Refresh, OpType enums
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;

namespace Tests.OpenSearch.Client.Serialization
{
	/// <summary>
	/// Validates the <see cref="IndexRequestConverter{TDocument}"/> / <see cref="IndexRequestConverterFactory"/>
	/// reproduce the legacy Utf8Json <c>IndexRequestFormatter&lt;TDocument&gt;</c> (a proxy request formatter). An index
	/// request is a proxy request: the wire body IS the document; the request-wrapper members (index, id, refresh,
	/// op_type, routing, …) are URL/query parameters and must NOT appear in the body. Also covers the factory
	/// constructing the correctly-bound closed converter and the null cases.
	/// </summary>
	public class IndexRequestConverterTests
	{
		private class Doc
		{
			public string Name { get; set; }
			public int Value { get; set; }
		}

		private static JsonSerializerOptions Options()
		{
			var settings = new ConnectionSettings();
			var options = new JsonSerializerOptions
			{
				DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
				PropertyNameCaseInsensitive = true,
				Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
				TypeInfoResolver = new HighLevelContractResolver(settings)
			};
			options.Converters.Add(new IndexRequestConverterFactory());
			return options;
		}

		[U] public void Serialize_BodyIsTheDocument()
		{
			var request = new IndexRequest<Doc>(new Doc { Name = "bob", Value = 7 }, index: "idx", id: 1);

			var json = JsonSerializer.Serialize<IIndexRequest<Doc>>(request, Options());

			// Field-name inference camel-cases the members; body is the document exactly.
			json.Should().Be("{\"name\":\"bob\",\"value\":7}");
		}

		[U] public void Serialize_RequestWrapperMembersAreNotInBody()
		{
			var request = new IndexRequest<Doc>(new Doc { Name = "bob", Value = 7 }, index: "idx", id: 42)
			{
				Refresh = Refresh.True,
				OpType = OpType.Create,
				Routing = "route"
			};

			var json = JsonSerializer.Serialize<IIndexRequest<Doc>>(request, Options());

			// None of the URL/query params leak into the body.
			json.Should().NotContain("refresh")
				.And.NotContain("op_type")
				.And.NotContain("routing")
				.And.NotContain("_index")
				.And.NotContain("_id")
				.And.NotContain("42");
			json.Should().Be("{\"name\":\"bob\",\"value\":7}");
		}

		[U] public void Serialize_NullRequest_WritesNull()
		{
			var json = JsonSerializer.Serialize<IIndexRequest<Doc>>(null, Options());
			json.Should().Be("null");
		}

		[U] public void RoundTrip_ReconstructsRequestWithDocument()
		{
			var request = new IndexRequest<Doc>(new Doc { Name = "bob", Value = 7 });

			var json = JsonSerializer.Serialize<IIndexRequest<Doc>>(request, Options());
			var back = JsonSerializer.Deserialize<IIndexRequest<Doc>>(json, Options());

			back.Should().NotBeNull();
			back.Document.Should().NotBeNull();
			back.Document.Name.Should().Be("bob");
			back.Document.Value.Should().Be(7);
		}

		// --- Factory ---

		[U] public void Factory_ConstructsClosedConverterBoundToDocumentType()
		{
			var factory = new IndexRequestConverterFactory();

			factory.CanConvert(typeof(IIndexRequest<Doc>)).Should().BeTrue();

			var converter = factory.CreateConverter(typeof(IIndexRequest<Doc>), Options());
			converter.Should().BeOfType<IndexRequestConverter<Doc>>();
		}

		[U] public void Factory_DoesNotConvertUnrelatedType()
		{
			var factory = new IndexRequestConverterFactory();
			factory.CanConvert(typeof(Doc)).Should().BeFalse();
			factory.CanConvert(typeof(ICreateRequest<Doc>)).Should().BeFalse();
		}
	}
}
