/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.IO;
using System.Text;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.Net;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Core.Client.Serializers;
using Tests.Domain;

namespace Tests.Document.Single.Source;

public class SourceSerializerBuilderTests
{
	// The _source GET response body must be deserialized by the connection's configured SourceSerializer (a Newtonsoft
	// TestSourceSerializerBase here, which registers SourceOnlyUsingBuiltInConverter), not the built-in serializer.
	// Under System.Text.Json the SourceRequestResponseBuilder previously fell through to the built-in serializer because
	// it only recognised the Utf8Json IInternalSerializer, so SourceOnly came back null — the integration failure
	// "Expected sourceOnly.NotReadByDefaultSerializer to be 'read', but found <null>".
	[U] public void SourceResponseUsesConfiguredSourceSerializer()
	{
		var settings = new ConnectionSettings(
				new SingleNodeConnectionPool(new System.Uri("http://localhost:9200")),
				new InMemoryConnection(),
				(builtin, values) => new TestSourceSerializerBase(builtin, values))
			.DefaultIndex("default");

		var client = new OpenSearchClient(settings);
		var builtIn = client.RequestResponseSerializer;

		// The custom Newtonsoft converter (registered for SourceOnlyObject) ignores the wire JSON and yields
		// NotReadByDefaultSerializer = "read". It is only invoked when the "sourceOnly" property is present in the body,
		// which mirrors the indexed document.
		var body = Encoding.UTF8.GetBytes("{\"name\":\"x\",\"sourceOnly\":{\"notWrittenByDefaultSerializer\":\"written\"}}");
		var apiCall = new ApiCallDetails { Success = true, HttpStatusCode = 200 };

		var response = (SourceResponse<Project>)SourceRequestResponseBuilder<Project>.Instance
			.DeserializeResponse(builtIn, apiCall, new MemoryStream(body));

		response.Body.Should().NotBeNull();
		response.Body.SourceOnly.Should().NotBeNull("the source serializer must materialize SourceOnly");
		response.Body.SourceOnly.NotReadByDefaultSerializer.Should().Be("read");
	}
}
