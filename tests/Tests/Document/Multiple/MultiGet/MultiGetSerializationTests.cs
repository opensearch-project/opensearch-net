/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.Linq;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.Net;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Core.Client;
using Tests.Domain;

namespace Tests.Document.Multiple.MultiGet;

public class MultiGetSerializationTests
{
	// The MultiGetParent integration test round-trips CommitActivity join/routing docs. Verify the request body
	// carries the per-operation routing (the _mget "docs" entries), and the join document serializes its relation.
	[U] public void MultiGetRequestSerializesRouting()
	{
		var request = new MultiGetRequest(Infer.Index<Project>())
		{
			Documents = new IMultiGetOperation[]
			{
				new MultiGetOperation<CommitActivity>("c1") { Routing = "proj-a" },
				new MultiGetOperation<CommitActivity>("c2") { Routing = "proj-b" },
			}
		};

		var json = TestClient.DisabledStreaming.RequestResponseSerializer.SerializeToString(request);

		json.Should().Contain("\"_id\":\"c1\"").And.Contain("\"_id\":\"c2\"");
		json.Should().Contain("proj-a").And.Contain("proj-b");
	}

	[U] public void JoinFieldChildSerializesRelationAndParent()
	{
		var activity = new CommitActivity { Id = "c1", ProjectName = "proj-a", Message = "m" };

		var json = TestClient.DisabledStreaming.SourceSerializer.SerializeToString(activity);

		// A child join field serializes as { "name": "<relation>", "parent": "<parentId>" }.
		json.Should().Contain("commits").And.Contain("proj-a");
	}

	// The MultiGetParent integration failure is on the RESPONSE: hit.Found / hit.Routing must populate from the
	// _mget "docs" hits. This exercises the per-request MultiGetResponseConverter path (found docs with _routing).
	[U] public void MultiGetResponseDeserializesFoundAndRouting()
	{
		const string json = @"{""docs"":[
			{""_index"":""project"",""_id"":""c1"",""_routing"":""proj-a"",""_version"":1,""found"":true,""_source"":{""message"":""m""}},
			{""_index"":""project"",""_id"":""c2"",""_routing"":""proj-b"",""_version"":1,""found"":true,""_source"":{""message"":""n""}}
		]}";

		var request = new MultiGetRequest(Infer.Index<Project>())
		{
			Documents = new IMultiGetOperation[]
			{
				new MultiGetOperation<CommitActivity>("c1") { Routing = "proj-a" },
				new MultiGetOperation<CommitActivity>("c2") { Routing = "proj-b" },
			}
		};

		var builder = new MultiGetResponseBuilder(request);
		var apiCall = new ApiCallDetails { Success = true, HttpStatusCode = 200 };
		var response = (MultiGetResponse)builder.DeserializeResponse(
			TestClient.DisabledStreaming.RequestResponseSerializer, apiCall,
			new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(json)));

		response.Hits.Should().HaveCount(2);
		foreach (var hit in response.GetMany<CommitActivity>(new[] { "c1", "c2" }))
		{
			hit.Found.Should().BeTrue();
			hit.Routing.Should().NotBeNullOrEmpty();
		}
	}
}
