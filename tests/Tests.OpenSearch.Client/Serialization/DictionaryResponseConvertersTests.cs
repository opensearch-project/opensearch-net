/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.Net.Serialization.Converters;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;

namespace Tests.OpenSearch.Client.Serialization
{
	/// <summary>
	/// Behavioural tests for the System.Text.Json replacements of the three generic response-dictionary formatters
	/// and the open-generic <see cref="DictionaryResponseConverterFactory"/> that constructs them:
	/// <list type="bullet">
	/// <item><see cref="DictionaryResponseConverter{TResponse,TKey,TValue}"/> via <see cref="RemoteInfoResponse"/></item>
	/// <item><see cref="ResolvableDictionaryResponseConverter{TResponse,TKey,TValue}"/> via <see cref="GetMappingResponse"/></item>
	/// <item><see cref="DynamicResponseConverter{TResponse}"/> via <see cref="ClusterStateResponse"/></item>
	/// </list>
	/// The factory discovers the closed generic arguments from the legacy <c>[JsonFormatter]</c> attribute already on
	/// each response type, mirroring the legacy Utf8Json engine's type-arg mapping. Each response is deserialized
	/// exactly as the high-level serializer would, so these assert end-to-end parity with the OLD engine.
	/// </summary>
	public class DictionaryResponseConvertersTests
	{
		// Mirrors the relevant slice of SystemTextJsonHighLevelSerializer: the response-dictionary factory plus the
		// settings-aware member converters and contract resolver the response bodies depend on.
		private static JsonSerializerOptions Options()
		{
			var settings = new ConnectionSettings();
			var options = new JsonSerializerOptions
			{
				DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
				PropertyNameCaseInsensitive = true,
				TypeInfoResolver = new HighLevelContractResolver(settings)
			};
			options.Converters.Add(new DictionaryResponseConverterFactory(settings));
			options.Converters.Add(new IndexNameConverter(settings));
			// The server-error envelope's `error` object is deserialized as an Error; register its converter so
			// the envelope-extraction tests exercise the same path as the production serializer.
			options.Converters.Add(new ErrorConverter());
			return options;
		}

		private static T Deserialize<T>(string json) =>
			JsonSerializer.Deserialize<T>(Encoding.UTF8.GetBytes(json), Options());

		// ---- DictionaryResponseConverter (RemoteInfoResponse, string keys) ----

		[U] public void Dictionary_ParsesBodyIntoTypedResponse()
		{
			var json = @"{
				""cluster_one"": { ""connected"": true, ""num_nodes_connected"": 3, ""seeds"": [""node1:9300""] },
				""cluster_two"": { ""connected"": false, ""num_nodes_connected"": 0, ""seeds"": [] }
			}";
			var response = Deserialize<RemoteInfoResponse>(json);

			response.Should().NotBeNull();
			response.Remotes.Count.Should().Be(2);
			response.Remotes["cluster_one"].Connected.Should().BeTrue();
			response.Remotes["cluster_one"].NumNodesConnected.Should().Be(3);
			response.Remotes["cluster_one"].Seeds.Should().ContainSingle().Which.Should().Be("node1:9300");
			response.Remotes["cluster_two"].Connected.Should().BeFalse();
		}

		[U] public void Dictionary_ParsesEmptyObject()
		{
			var response = Deserialize<RemoteInfoResponse>("{}");
			response.Should().NotBeNull();
			response.Remotes.Should().NotBeNull();
			response.Remotes.Count.Should().Be(0);
		}

		[U] public void Dictionary_ParsesNull()
		{
			// HandleNull is opted in, so the converter runs for a top-level null and yields a non-null response with
			// an empty dictionary — matching the legacy formatter, which always returned a constructed response.
			var response = Deserialize<RemoteInfoResponse>("null");
			response.Should().NotBeNull();
			response.Remotes.Count.Should().Be(0);
		}

		[U] public void Dictionary_ExtractsServerErrorEnvelope()
		{
			var json = @"{ ""error"": { ""reason"": ""boom"" }, ""status"": 500 }";
			var response = Deserialize<RemoteInfoResponse>(json);

			response.Remotes.Count.Should().Be(0);
			response.ServerError.Should().NotBeNull();
			response.ServerError.Error.Reason.Should().Be("boom");
			response.ServerError.Status.Should().Be(500);
		}

		[U] public void Dictionary_ExtractsStringErrorShortForm()
		{
			var response = Deserialize<RemoteInfoResponse>(@"{ ""error"": ""kaboom"" }");
			response.ServerError.Should().NotBeNull();
			response.ServerError.Error.Reason.Should().Be("kaboom");
		}

		// ---- ResolvableDictionaryResponseConverter (GetMappingResponse, IndexName keys) ----

		[U] public void Resolvable_ParsesBodyAndResolvesKeys()
		{
			// A minimal (empty) mappings body keeps this test focused on key resolution rather than on inner
			// property converters that are migrated by other workers.
			var json = @"{ ""my-index"": { ""mappings"": {} } }";
			var response = Deserialize<GetMappingResponse>(json);

			response.Should().NotBeNull();
			response.Indices.Count.Should().Be(1);

			// Key resolution: the resolvable proxy exposes the Inferrer-resolved key and indexes by IndexName.
			response.Indices.Keys.Should().Contain((IndexName)"my-index");
			response.Indices[(IndexName)"my-index"].Should().NotBeNull();
		}

		[U] public void Resolvable_ParsesEmptyObject()
		{
			var response = Deserialize<GetMappingResponse>("{}");
			response.Should().NotBeNull();
			response.Indices.Should().NotBeNull();
			response.Indices.Count.Should().Be(0);
		}

		[U] public void Resolvable_ParsesNull()
		{
			var response = Deserialize<GetMappingResponse>("null");
			response.Should().NotBeNull();
			response.Indices.Count.Should().Be(0);
		}

		[U] public void Resolvable_ExtractsServerErrorEnvelope()
		{
			var response = Deserialize<GetMappingResponse>(@"{ ""error"": { ""reason"": ""nope"" }, ""status"": 404 }");
			response.Indices.Count.Should().Be(0);
			response.ServerError.Should().NotBeNull();
			response.ServerError.Error.Reason.Should().Be("nope");
			response.ServerError.Status.Should().Be(404);
		}

		// ---- DynamicResponseConverter (ClusterStateResponse) ----

		[U] public void Dynamic_CapturesWholeBody()
		{
			var json = @"{
				""cluster_name"": ""opensearch"",
				""version"": 12,
				""blocks"": { ""indices"": { ""a"": true } },
				""nodes"": [ ""n1"", ""n2"" ]
			}";
			var response = Deserialize<ClusterStateResponse>(json);

			response.Should().NotBeNull();
			response.State.Get<string>("cluster_name").Should().Be("opensearch");
			response.State.Get<long>("version").Should().Be(12);
			response.State.Get<bool>("blocks.indices.a").Should().BeTrue();
		}

		[U] public void Dynamic_ParsesEmptyObject()
		{
			var response = Deserialize<ClusterStateResponse>("{}");
			response.Should().NotBeNull();
			(response.State == null).Should().BeFalse();
			response.State.Count.Should().Be(0);
		}

		[U] public void Dynamic_ParsesNull()
		{
			var response = Deserialize<ClusterStateResponse>("null");
			response.Should().NotBeNull();
			response.State.Count.Should().Be(0);
		}

		[U] public void Dynamic_ExtractsServerErrorEnvelope()
		{
			var response = Deserialize<ClusterStateResponse>(@"{ ""error"": { ""reason"": ""fail"" }, ""status"": 503 }");
			response.State.Count.Should().Be(0);
			response.ServerError.Should().NotBeNull();
			response.ServerError.Error.Reason.Should().Be("fail");
			response.ServerError.Status.Should().Be(503);
		}
	}
}
