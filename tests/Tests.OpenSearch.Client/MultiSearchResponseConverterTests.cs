/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;

namespace Tests.OpenSearch.Client.Serialization
{
	/// <summary>
	/// Behavioural tests for the request-stateful <see cref="MultiSearchResponseConverter"/> (the System.Text.Json
	/// replacement for the legacy Utf8Json <c>MultiSearchResponseFormatter</c>).
	///
	/// The <c>responses</c> array carries no per-response type discriminator, so the converter recovers each inner
	/// search response's concrete document type by positionally zipping the array against the originating request's
	/// operations, deserializing each entry as <c>SearchResponse&lt;operation.ClrType&gt;</c> and keying it by the
	/// operation's key. The converter is constructed with the request (mirroring the legacy per-request
	/// <c>CreateStateful</c> installation); these tests instantiate it directly with a hand-built request.
	///
	/// Assertions target the dispatch outcome (response keys + concrete <c>SearchResponse&lt;T&gt;</c> types + Took),
	/// which is the converter's responsibility.
	/// </summary>
	public class MultiSearchResponseConverterTests
	{
		private class DocA { public string Name { get; set; } }
		private class DocB { public string Name { get; set; } }

		private static JsonSerializerOptions Options()
		{
			var settings = new ConnectionSettings();
			return new JsonSerializerOptions
			{
				DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
				PropertyNameCaseInsensitive = true,
				TypeInfoResolver = new HighLevelContractResolver(settings)
			};
		}

		private static IMultiSearchRequest Request(params (string key, System.Type clr)[] ops)
		{
			var operations = new Dictionary<string, ISearchRequest>();
			foreach (var (key, clr) in ops)
				operations.Add(key, clr == typeof(DocA)
					? (ISearchRequest)new SearchRequest<DocA>()
					: new SearchRequest<DocB>());
			return new MultiSearchRequest { Operations = operations };
		}

		private static MultiSearchResponse Deserialize(string json, IRequest request)
		{
			var options = Options();
			var converter = new MultiSearchResponseConverter(request);
			var reader = new Utf8JsonReader(System.Text.Encoding.UTF8.GetBytes(json));
			reader.Read();
			return converter.Read(ref reader, typeof(MultiSearchResponse), options);
		}

		[U] public void Read_SingleResponse_DispatchesConcreteTypeAndKey()
		{
			// Bodies are kept hits-free: this test targets the converter's dispatch (key + concrete SearchResponse<T>
			// + Took), not full search-response body deserialization (which needs the whole serializer stack).
			var response = Deserialize(
				@"{""took"":5,""responses"":[{""took"":3,""timed_out"":false}]}",
				Request(("a", typeof(DocA))));

			response.Took.Should().Be(5);
			response.Responses.Should().HaveCount(1);
			response.Responses.Should().ContainKey("a");
			response.Responses["a"].Should().BeOfType<SearchResponse<DocA>>();
		}

		[U] public void Read_MultipleResponses_ZippedPositionallyByOperation()
		{
			var response = Deserialize(
				@"{""took"":9,""responses"":[{""took"":1},{""took"":2}]}",
				Request(("a", typeof(DocA)), ("b", typeof(DocB))));

			response.Responses.Should().HaveCount(2);
			response.Responses["a"].Should().BeOfType<SearchResponse<DocA>>();
			response.Responses["b"].Should().BeOfType<SearchResponse<DocB>>();
		}

		[U] public void Read_NullRequest_ReturnsNull()
		{
			Deserialize(@"{""responses"":[]}", null).Should().BeNull();
		}

		[U] public void Read_NoResponsesProperty_ReturnsResponseWithTook()
		{
			var response = Deserialize(@"{""took"":7}", Request(("a", typeof(DocA))));
			response.Should().NotBeNull();
			response.Took.Should().Be(7);
			response.Responses.Should().BeEmpty();
		}

		[U] public void Read_EmptyResponsesArray_ReturnsEmpty()
		{
			var response = Deserialize(@"{""took"":1,""responses"":[]}", Request(("a", typeof(DocA))));
			response.Took.Should().Be(1);
			response.Responses.Should().BeEmpty();
		}

		[U] public void Read_Null_ReturnsEmptyResponse()
		{
			var response = Deserialize("null", Request(("a", typeof(DocA))));
			response.Should().NotBeNull();
			response.Responses.Should().BeEmpty();
		}

		[U] public void Read_MoreResponsesThanOperations_DropsExtras()
		{
			// Zip stops at the shorter sequence: one operation => only the first response is taken.
			var response = Deserialize(
				@"{""responses"":[{""took"":1},{""took"":2}]}",
				Request(("a", typeof(DocA))));
			response.Responses.Should().HaveCount(1);
			response.Responses.Should().ContainKey("a");
		}

		[U] public void Read_UnsupportedRequestType_Throws()
		{
			// Legacy threw InvalidOperationException for a request that is neither IMultiSearchRequest nor
			// IMultiSearchTemplateRequest. Any request carrying a "responses" array triggers descriptor resolution.
			System.Action read = () => Deserialize(@"{""responses"":[{}]}", new SearchRequest<DocA>());
			read.Should().Throw<System.InvalidOperationException>();
		}

		// NOTE: the write path is not unit-tested in isolation. MultiSearchResponse is a per-request stateful
		// converter (installed via CustomResponseBuilder, never globally registered) and its Write re-serializes full
		// SearchResponse<T> bodies, which pulls in the entire hits/aggregations serialization stack. Write is
		// exercised through real response-builder integration; the read/dispatch responsibility unique to this
		// converter is fully covered by the Read_* tests above.
	}
}
