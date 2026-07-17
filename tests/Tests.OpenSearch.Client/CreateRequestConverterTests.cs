/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

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
	/// Validates the <see cref="CreateRequestConverter{TDocument}"/> / <see cref="CreateRequestConverterFactory"/>
	/// reproduce the legacy Utf8Json <c>CreateRequestFormatter&lt;TDocument&gt;</c> (a proxy request formatter). A create
	/// request is a proxy request: the wire body IS the document; the request-wrapper members (index, id, refresh,
	/// routing, …) are URL/query parameters and must NOT appear in the body. Also covers the factory constructing the
	/// correctly-bound closed converter and the null cases.
	/// </summary>
	public class CreateRequestConverterTests
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
			options.Converters.Add(new CreateRequestConverterFactory());
			return options;
		}

		[U] public void Serialize_BodyIsTheDocument()
		{
			var request = new CreateRequest<Doc>(new Doc { Name = "bob", Value = 7 }, index: "idx", id: 1);

			var json = JsonSerializer.Serialize<ICreateRequest<Doc>>(request, Options());

			json.Should().Be("{\"name\":\"bob\",\"value\":7}");
		}

		[U] public void Serialize_RequestWrapperMembersAreNotInBody()
		{
			var request = new CreateRequest<Doc>(new Doc { Name = "bob", Value = 7 }, index: "idx", id: 42)
			{
				Refresh = Refresh.True,
				Routing = "route"
			};

			var json = JsonSerializer.Serialize<ICreateRequest<Doc>>(request, Options());

			json.Should().NotContain("refresh")
				.And.NotContain("routing")
				.And.NotContain("_index")
				.And.NotContain("_id")
				.And.NotContain("42");
			json.Should().Be("{\"name\":\"bob\",\"value\":7}");
		}

		[U] public void Serialize_NullRequest_WritesNull()
		{
			var json = JsonSerializer.Serialize<ICreateRequest<Doc>>(null, Options());
			json.Should().Be("null");
		}

		[U] public void RoundTrip_ReconstructsRequestWithDocument()
		{
			var request = new CreateRequest<Doc>(new Doc { Name = "bob", Value = 7 });

			var json = JsonSerializer.Serialize<ICreateRequest<Doc>>(request, Options());
			var back = JsonSerializer.Deserialize<ICreateRequest<Doc>>(json, Options());

			back.Should().NotBeNull();
			back.Document.Should().NotBeNull();
			back.Document.Name.Should().Be("bob");
			back.Document.Value.Should().Be(7);
		}

		// --- Factory ---

		[U] public void Factory_ConstructsClosedConverterBoundToDocumentType()
		{
			var factory = new CreateRequestConverterFactory();

			factory.CanConvert(typeof(ICreateRequest<Doc>)).Should().BeTrue();

			var converter = factory.CreateConverter(typeof(ICreateRequest<Doc>), Options());
			converter.Should().BeOfType<CreateRequestConverter<Doc>>();
		}

		[U] public void Factory_DoesNotConvertUnrelatedType()
		{
			var factory = new CreateRequestConverterFactory();
			factory.CanConvert(typeof(Doc)).Should().BeFalse();
			factory.CanConvert(typeof(IIndexRequest<Doc>)).Should().BeFalse();
		}
	}
}
