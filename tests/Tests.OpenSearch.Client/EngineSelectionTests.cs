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

namespace Tests.OpenSearch.Client.Serialization
{
	/// <summary>
	/// System.Text.Json is opt-in via <c>UseSystemTextJson()</c> (or the OSC_USE_STJ environment
	/// variable, which the unit CI matrix sets per leg). These tests assert the PROGRAMMATIC switch, which takes
	/// precedence over the environment variable, so they are deterministic regardless of which engine the CI leg
	/// selects. The plain default (no method call, no env var) is Utf8Json, but that is not asserted here because the
	/// process environment cannot be assumed clean under the test matrix.
	/// </summary>
	public class EngineSelectionTests
	{
		// DiagnosticsSerializerProxy.InnerSerializer is internal to OpenSearch.Net; read the engine's type name via
		// reflection to avoid an InternalsVisibleTo dependency.
		private static string EngineTypeName(IConnectionSettingsValues settings)
		{
			var serializer = ((IConnectionConfigurationValues)settings).RequestResponseSerializer;
			var innerProp = serializer.GetType().GetProperty("InnerSerializer",
				System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
			var inner = innerProp?.GetValue(serializer) ?? serializer;
			return inner.GetType().Name;
		}

		private static ConnectionSettings NewSettings() =>
			new ConnectionSettings(new SingleNodeConnectionPool(new System.Uri("http://localhost:9200")));

		[U] public void UseSystemTextJson_OptsIntoStj()
		{
			var settings = NewSettings().UseSystemTextJson();
			EngineTypeName(settings).Should().Be("SystemTextJsonHighLevelSerializer");
		}

		[U] public void UseSystemTextJson_False_ForcesUtf8Json()
		{
			// Explicit false overrides any OSC_USE_STJ=true set by the CI matrix.
			var settings = NewSettings().UseSystemTextJson(false);
			EngineTypeName(settings).Should().Be(nameof(DefaultHighLevelSerializer));
		}

		[U] public void UseSystemTextJson_IsRepeatableAndLastCallWins()
		{
			var settings = NewSettings().UseSystemTextJson().UseSystemTextJson(false);
			EngineTypeName(settings).Should().Be(nameof(DefaultHighLevelSerializer));

			settings.UseSystemTextJson();
			EngineTypeName(settings).Should().Be("SystemTextJsonHighLevelSerializer");
		}

		// Switching the engine must also rebuild the source serializer (used for _source bodies), not just the
		// request/response serializer, so the two never disagree about which engine is active.
		[U] public void UseSystemTextJson_AlsoSwitchesSourceSerializer()
		{
			var settings = NewSettings().UseSystemTextJson();
			var source = ((IConnectionSettingsValues)settings).SourceSerializer;
			var innerProp = source.GetType().GetProperty("InnerSerializer",
				System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
			var inner = innerProp?.GetValue(source) ?? source;
			inner.GetType().Name.Should().Be("SystemTextJsonHighLevelSerializer");
		}

		// The opt-in System.Text.Json high-level engine must reproduce the legacy engine's tolerance of an empty,
		// whitespace-only, or absent response body (HEAD requests, 200-with-no-body): deserialize to default rather
		// than throwing "The input does not contain any JSON tokens".
		private static IOpenSearchSerializer StjSerializer() =>
			((IConnectionConfigurationValues)NewSettings().UseSystemTextJson()).RequestResponseSerializer;

		[U] public void Stj_Deserialize_EmptyStream_ReturnsNull()
		{
			using var stream = new MemoryStream();
			StjSerializer().Deserialize<ClusterHealthResponse>(stream).Should().BeNull();
		}

		[U] public void Stj_Deserialize_WhitespaceOnlyStream_ReturnsNull()
		{
			using var stream = new MemoryStream(Encoding.UTF8.GetBytes("  \n\t "));
			StjSerializer().Deserialize<ClusterHealthResponse>(stream).Should().BeNull();
		}

		[U] public void Stj_Deserialize_NullStream_ReturnsNull() =>
			StjSerializer().Deserialize<ClusterHealthResponse>(Stream.Null).Should().BeNull();

		// The non-generic Deserialize(Type, Stream) must return a value type's boxed default (not null) for a blank
		// body, so a caller can unbox it — matching the low-level SystemTextJsonSerializer.
		[U] public void Stj_DeserializeNonGeneric_EmptyStream_ReturnsBoxedDefaultForValueType()
		{
			using var stream = new MemoryStream();
			StjSerializer().Deserialize(typeof(int), stream).Should().Be(0);
		}

		[U] public void Stj_DeserializeNonGeneric_EmptyStream_ReturnsNullForReferenceType()
		{
			using var stream = new MemoryStream();
			StjSerializer().Deserialize(typeof(ClusterHealthResponse), stream).Should().BeNull();
		}

		// Null data must write nothing (empty body), matching the low-level serializer's early return rather than
		// writing the literal "null".
		[U] public void Stj_Serialize_NullData_WritesNothing()
		{
			using var stream = new MemoryStream();
			StjSerializer().Serialize<ClusterHealthResponse>(null, stream);
			stream.ToArray().Should().BeEmpty();
		}

		// A genuine successful high-level request/response round-trip under both engines, complementing the
		// engine-selection-only checks above with an actual typed response built from a real (mocked) body.
		[U] public void SuccessfulSearchResponse_Utf8Json_DeserializesCorrectly() =>
			AssertSuccessfulSearchResponse(useStj: false);

		[U] public void SuccessfulSearchResponse_SystemTextJson_DeserializesCorrectly() =>
			AssertSuccessfulSearchResponse(useStj: true);

		private static void AssertSuccessfulSearchResponse(bool useStj)
		{
			const string body = @"{
				""took"": 1, ""timed_out"": false,
				""_shards"": { ""total"": 1, ""successful"": 1, ""skipped"": 0, ""failed"": 0 },
				""hits"": {
					""total"": { ""value"": 1, ""relation"": ""eq"" },
					""max_score"": 1.0,
					""hits"": [ { ""_index"": ""my-index"", ""_id"": ""1"", ""_score"": 1.0, ""_source"": { } } ]
				}
			}";
			var connection = new InMemoryConnection(Encoding.UTF8.GetBytes(body));
			var pool = new SingleNodeConnectionPool(new System.Uri("http://localhost:9200"));
			// Set the engine explicitly in both directions so the test is deterministic under the CI leg that sets
			// OSC_USE_STJ=true process-wide (a programmatic UseSystemTextJson call takes precedence over the env var).
			var settings = new ConnectionSettings(pool, connection).UseSystemTextJson(useStj);

			var client = new OpenSearchClient(settings);
			var response = client.Search<object>(s => s.AllIndices());

			response.IsValid.Should().BeTrue();
			response.Took.Should().Be(1);
			response.HitsMetadata.Total.Value.Should().Be(1);
			response.Hits.Should().ContainSingle().Which.Index.Should().Be("my-index");
		}

		// The HIGH-LEVEL error path is a separate code path from the low-level ServerError/OpenSearchResponseBase
		// one fixed and tested in Tests.OpenSearch.Net (ConfiguredSerializerIsHonoredTests.cs /
		// SystemTextJsonSerializerTests.cs): ResponseBase.Error/StatusCode are internal-setter [DataMember]
		// properties deserialized directly onto the typed response by whichever high-level engine is configured --
		// they never call ServerError.Create/TryCreate. Confirms this path (unlike the low-level one before its
		// fix) already tolerates the internal setters under STJ, via HighLevelContractResolver, which
		// SystemTextJsonHighLevelSerializer has always registered.
		[U] public void ServerErrorResponse_Utf8Json_PopulatesErrorAndStatus() =>
			AssertServerErrorResponse(useStj: false);

		[U] public void ServerErrorResponse_SystemTextJson_PopulatesErrorAndStatus() =>
			AssertServerErrorResponse(useStj: true);

		private static void AssertServerErrorResponse(bool useStj)
		{
			const string errorJson = @"{""error"":{""reason"":""index not found"",""type"":""index_not_found_exception""},""status"":404}";
			var connection = new InMemoryConnection(Encoding.UTF8.GetBytes(errorJson), 404);
			var pool = new SingleNodeConnectionPool(new System.Uri("http://localhost:9200"));
			// Set the engine explicitly in both directions so the test is deterministic under the CI leg that sets
			// OSC_USE_STJ=true process-wide (a programmatic UseSystemTextJson call takes precedence over the env var).
			var settings = new ConnectionSettings(pool, connection).UseSystemTextJson(useStj);

			var client = new OpenSearchClient(settings);
			var response = client.Search<object>(s => s.AllIndices());

			response.IsValid.Should().BeFalse();
			response.ServerError.Should().NotBeNull();
			response.ServerError.Status.Should().Be(404);
			response.ServerError.Error.Reason.Should().Be("index not found");
			response.ServerError.Error.Type.Should().Be("index_not_found_exception");
		}

		// Guards against the shared OpenSearch.Net.SystemTextJsonEnvironment.ReadOverride() (used by both this
		// class's high-level toggle and the new low-level ConnectionConfiguration.UseSystemTextJson()) being read at
		// the wrong point during construction: ConnectionSettingsBase extends ConnectionConfiguration<T> and calls
		// base(pool, connection, null), which runs the low-level toggle logic too, before ConnectionSettingsBase's
		// own BuildHighLevelSerializers() runs. Confirms that ordering doesn't leak the low-level engine selection
		// into the high-level slot, and that OpenSearchClient.LowLevel still shares the resulting (high-level)
		// engine as it did before the low-level toggle existed -- not a second, independently-defaulted engine.
		[U] public void OscUseStj_DoesNotLeakLowLevelToggleIntoHighLevelSlot_AndLowLevelClientStillShares()
		{
			System.Environment.SetEnvironmentVariable("OSC_USE_STJ", "true");
			try
			{
				var settings = NewSettings();
				var client = new OpenSearchClient(settings);

				var highLevelEngine = EngineTypeName(settings);
				highLevelEngine.Should().Be("SystemTextJsonHighLevelSerializer",
					"the high level's own OSC_USE_STJ handling in ConnectionSettingsBase must still take effect");

				var lowLevel = (OpenSearchLowLevelClient)client.LowLevel;
				var lowLevelSerializer = ((IConnectionConfigurationValues)lowLevel.Settings).RequestResponseSerializer;
				var innerProp = lowLevelSerializer.GetType().GetProperty("InnerSerializer",
					System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
				var lowLevelEngine = (innerProp?.GetValue(lowLevelSerializer) ?? lowLevelSerializer).GetType().Name;

				lowLevelEngine.Should().Be(highLevelEngine,
					"client.LowLevel must keep sharing the parent's engine, not fall back to a separately-defaulted one");
			}
			finally
			{
				System.Environment.SetEnvironmentVariable("OSC_USE_STJ", null);
			}
		}
	}
}
