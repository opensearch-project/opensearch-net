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
	/// System.Text.Json is opt-in via <c>UseSystemTextJson()</c> (or the OSC_USE_STJ / OSC_USE_UTF8JSON environment
	/// variables, which the unit CI matrix sets per leg). These tests assert the PROGRAMMATIC switch, which takes
	/// precedence over the environment variables, so they are deterministic regardless of which engine the CI leg
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
	}
}
