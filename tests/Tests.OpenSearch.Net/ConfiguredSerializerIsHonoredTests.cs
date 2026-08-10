/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Reflection;
using System.Text;
using FluentAssertions;
using OpenSearch.Net;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;

namespace Tests.OpenSearch.Net.Serialization
{
	/// <summary>
	/// <see cref="DynamicResponse"/> and <see cref="ServerError"/> parsing used to hardcode
	/// <see cref="LowLevelRequestResponseSerializer.Instance"/> (the legacy Utf8Json engine) regardless of which
	/// serializer the request was configured with -- silently ignoring
	/// <see cref="ConnectionConfiguration.UseSystemTextJson"/> for those two response paths specifically. These
	/// tests drive both through a real <see cref="OpenSearchLowLevelClient"/> over an
	/// <see cref="InMemoryConnection"/> under both engines to confirm they now honor the configured serializer.
	/// </summary>
	public class ConfiguredSerializerIsHonoredTests
	{
		private static OpenSearchLowLevelClient ClientFor(string responseJson, bool useStj, int statusCode = 200)
		{
			var connection = new InMemoryConnection(Encoding.UTF8.GetBytes(responseJson), statusCode);
			var pool = new SingleNodeConnectionPool(new Uri("http://localhost:9200"));
			// Set the engine explicitly in both directions: a programmatic UseSystemTextJson(...) call takes
			// precedence over the OSC_USE_STJ environment variable, so this stays deterministic even on the CI
			// leg that sets OSC_USE_STJ=true process-wide.
			var config = new ConnectionConfiguration(pool, connection).UseSystemTextJson(useStj);

			return new OpenSearchLowLevelClient(config);
		}

		// DiagnosticsSerializerProxy.InnerSerializer is internal; read the engine's type name via reflection to
		// avoid an InternalsVisibleTo dependency, matching LowLevelEngineSelectionTests.cs.
		private static string EngineTypeName(IOpenSearchSerializer serializer)
		{
			var innerProp = serializer.GetType().GetProperty("InnerSerializer",
				BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
			var inner = innerProp?.GetValue(serializer) ?? serializer;
			return inner.GetType().Name;
		}

		[U] public void DynamicResponse_Utf8Json_ReadsBodyThroughConfiguredEngine() =>
			AssertDynamicResponse(useStj: false, expectedEngine: nameof(LowLevelRequestResponseSerializer));

		[U] public void DynamicResponse_SystemTextJson_ReadsBodyThroughConfiguredEngine() =>
			AssertDynamicResponse(useStj: true, expectedEngine: nameof(SystemTextJsonSerializer));

		private static void AssertDynamicResponse(bool useStj, string expectedEngine)
		{
			var client = ClientFor(@"{""cluster_name"":""test"",""number_of_nodes"":3}", useStj);

			// Confirm which engine is actually configured, so a future refactor that silently reverts to the
			// hardcoded singleton would show up as an engine-name mismatch here, not just a value mismatch that
			// could coincidentally still pass (both engines produce the same shape for these two fields).
			EngineTypeName(((IConnectionConfigurationValues)client.Settings).RequestResponseSerializer).Should().Be(expectedEngine);

			var response = client.DoRequest<DynamicResponse>(HttpMethod.GET, "/");
			response.Success.Should().BeTrue();
			((string)response.Body["cluster_name"]).Should().Be("test");
			((long)response.Body["number_of_nodes"]).Should().Be(3);
		}

		[U] public void ServerError_Utf8Json_ParsesThroughConfiguredEngine() =>
			AssertServerError(useStj: false);

		[U] public void ServerError_SystemTextJson_ParsesThroughConfiguredEngine() =>
			AssertServerError(useStj: true);

		private static void AssertServerError(bool useStj)
		{
			const string errorJson = @"{""error"":{""reason"":""index not found"",""type"":""index_not_found_exception""},""status"":404}";
			var client = ClientFor(errorJson, useStj, statusCode: 404);

			var response = client.DoRequest<StringResponse>(HttpMethod.GET, "/missing-index");
			response.TryGetServerError(out var serverError).Should().BeTrue();
			serverError.Status.Should().Be(404);
			serverError.Error.Reason.Should().Be("index not found");
			serverError.Error.Type.Should().Be("index_not_found_exception");
		}

		// The toggle/hardcoded-bypass fixes above only exercised the READ (deserialize) direction. Requests are
		// serialized too (PostData.Serializable<T> -> settings.RequestResponseSerializer.Serialize), so confirm the
		// toggle also takes effect on write, not just on read.
		[U] public void RequestBody_Utf8Json_WritesThroughConfiguredEngine() =>
			AssertRequestBodyEngine(useStj: false, expectedEngine: nameof(LowLevelRequestResponseSerializer));

		[U] public void RequestBody_SystemTextJson_WritesThroughConfiguredEngine() =>
			AssertRequestBodyEngine(useStj: true, expectedEngine: nameof(SystemTextJsonSerializer));

		private static void AssertRequestBodyEngine(bool useStj, string expectedEngine)
		{
			var connection = new InMemoryConnection(Encoding.UTF8.GetBytes("{}"));
			var pool = new SingleNodeConnectionPool(new Uri("http://localhost:9200"));
			// Set the engine explicitly (see ClientFor) so this is deterministic under the OSC_USE_STJ=true CI leg.
			var config = new ConnectionConfiguration(pool, connection).DisableDirectStreaming().UseSystemTextJson(useStj);

			var client = new OpenSearchLowLevelClient(config);
			EngineTypeName(((IConnectionConfigurationValues)client.Settings).RequestResponseSerializer).Should().Be(expectedEngine);

			// A POCO with no hand-written converter on either engine, so the wire shape below is purely a function
			// of which engine actually did the writing: Utf8Json/LowLevelRequestResponseSerializer PascalCases
			// unannotated properties, while the low-level SystemTextJsonSerializer's JsonNamingPolicy.CamelCase
			// lowercases the first letter -- a hard, unambiguous per-engine difference (unlike client-annotated
			// domain types, which both engines are designed to render identically).
			var response = client.DoRequest<StringResponse>(HttpMethod.POST, "/_test",
				PostData.Serializable(new PlainPoco { SampleValue = 42 }));

			var requestBody = Encoding.UTF8.GetString(response.ApiCall.RequestBodyInBytes);
			if (useStj)
				requestBody.Should().Contain("\"sampleValue\":42", "System.Text.Json's CamelCase naming policy must have written this");
			else
				requestBody.Should().Contain("\"SampleValue\":42", "the legacy Utf8Json engine's PascalCase default must have written this");
		}

		private class PlainPoco
		{
			public int SampleValue { get; set; }
		}
	}
}
