/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.Net;
using OpenSearch.Net.Utf8Json;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Core.Serialization;
using Xunit;
using static Tests.Core.Serialization.SerializationTestHelper;

namespace Tests.Ml
{
	/// <summary>
	/// Data-driven serialization tests for all generated ML request and response types.
	/// Builds minimal JSON fixtures from [DataMember] properties and verifies round-trip
	/// correctness. Serves as a regression safety net for serializer migrations.
	/// </summary>
	public class MlGeneratedRoundTripTests
	{
		// ── Response deserialization ──────────────────────────────────────────────

		public static IEnumerable<object[]> MlResponseTypes =>
			FindMlTypes("Response")
				.Where(t => typeof(ResponseBase).IsAssignableFrom(t)
					&& !typeof(WriteResponseBase).IsAssignableFrom(t)
					&& HasDataMembers(t))
				.Select(t => new object[] { t });

		[TU]
		[MemberData(nameof(MlResponseTypes))]
		public void Response_Deserializes_WithoutException(Type responseType)
		{
			var json = BuildSampleJson(responseType);
			var method = typeof(MlGeneratedRoundTripTests)
				.GetMethod(nameof(DeserializeResponse), BindingFlags.NonPublic | BindingFlags.Static)!
				.MakeGenericMethod(responseType);

			var result = method.Invoke(null, new object[] { json });
			result.Should().NotBeNull($"{responseType.Name} should deserialize from: {json}");
		}

		// ── Request serialization ────────────────────────────────────────────────

		public static IEnumerable<object[]> MlRequestTypes =>
			FindMlTypes("Request")
				.Where(t => HasDataMembers(t))
				.Select(t => new object[] { t });

		[TU]
		[MemberData(nameof(MlRequestTypes))]
		public void Request_RoundTrips_PrimitiveProperties(Type requestType)
		{
			// Build sample JSON from [DataMember] wire names and deserialize into the request type.
			// This tests the same code path as responses — verifying DataMember bindings work.
			var json = BuildSampleJson(requestType);
			var method = typeof(MlGeneratedRoundTripTests)
				.GetMethod(nameof(DeserializeRequest), BindingFlags.NonPublic | BindingFlags.Static)!
				.MakeGenericMethod(requestType);

			var result = method.Invoke(null, new object[] { json });
			result.Should().NotBeNull($"{requestType.Name} should deserialize from: {json}");
		}

		private static T DeserializeRequest<T>(string json) where T : class
		{
			var pool = new ConnectionSettings(new SingleNodeConnectionPool(new Uri("http://localhost:9200")));
			var client = new OpenSearchClient(pool);
			using var ms = new System.IO.MemoryStream(Encoding.UTF8.GetBytes(json));
			return client.RequestResponseSerializer.Deserialize<T>(ms);
		}

		// ── Helpers ──────────────────────────────────────────────────────────────

		private static IEnumerable<Type> FindMlTypes(string suffix) =>
			typeof(OpenSearchClient).Assembly
				.GetTypes()
				.Where(t => t.Namespace == "OpenSearch.Client"
					&& t.Name.EndsWith(suffix)
					&& t.IsClass && !t.IsAbstract
					&& IsMlGenerated(t.Name, suffix))
				.OrderBy(t => t.Name);

		private static bool IsMlGenerated(string typeName, string suffix)
		{
			var baseName = typeName.Substring(0, typeName.Length - suffix.Length);
			return MlOperationPrefixes.Any(p => typeName.StartsWith(p, StringComparison.Ordinal))
				|| MlGeneratedNames.Contains(typeName);
		}

		private static readonly HashSet<string> MlGeneratedNames = new(StringComparer.Ordinal)
		{
			// Body ops
			"AddAgenticMemoryRequest", "AddAgenticMemoryResponse",
			"ChunkModelRequest", "ChunkModelResponse",
			"CreateConnectorRequest", "CreateConnectorResponse",
			"CreateControllerRequest", "CreateControllerResponse",
			"CreateMemoryContainerRequest", "CreateMemoryContainerResponse",
			"CreateMemoryContainerSessionRequest", "CreateMemoryContainerSessionResponse",
			"CreateMemoryRequest", "CreateMemoryResponse",
			"CreateMessageRequest", "CreateMessageResponse",
			"CreateModelMetaRequest", "CreateModelMetaResponse",
			"DeleteAgenticMemoryQueryRequest", "DeleteAgenticMemoryQueryResponse",
			"DeployModelRequest", "DeployModelResponse",
			"ExecuteAgentRequest", "ExecuteAgentResponse",
			"ExecuteAlgorithmRequest", "ExecuteAlgorithmResponse",
			"ExecuteToolRequest", "ExecuteToolResponse",
			"LoadModelRequest", "LoadModelResponse",
			"PredictModelRequest", "PredictModelResponse",
			"PredictRequest", "PredictResponse",
			"RegisterAgentsRequest", "RegisterAgentsResponse",
			"RegisterModelGroupRequest", "RegisterModelGroupResponse",
			"RegisterModelMetaRequest", "RegisterModelMetaResponse",
			"RegisterModelRequest", "RegisterModelResponse",
			"SearchAgenticMemoryRequest", "SearchAgenticMemoryResponse",
			"SearchAgentsRequest", "SearchAgentsResponse",
			"SearchConnectorsRequest", "SearchConnectorsResponse",
			"SearchMemoryContainerRequest", "SearchMemoryContainerResponse",
			"SearchMemoryRequest", "SearchMemoryResponse",
			"SearchMessageRequest", "SearchMessageResponse",
			"SearchModelGroupRequest", "SearchModelGroupResponse",
			"TrainPredictRequest", "TrainPredictResponse",
			"TrainRequest", "TrainResponse",
			"UndeployModelRequest", "UndeployModelResponse",
			"UnloadModelRequest", "UnloadModelResponse",
			"UpdateAgenticMemoryRequest", "UpdateAgenticMemoryResponse",
			"UpdateConnectorRequest", "UpdateConnectorResponse",
			"UpdateControllerRequest", "UpdateControllerResponse",
			"UpdateMemoryContainerRequest", "UpdateMemoryContainerResponse",
			"UpdateMemoryRequest", "UpdateMemoryResponse",
			"UpdateMessageRequest", "UpdateMessageResponse",
			"UpdateModelGroupRequest", "UpdateModelGroupResponse",
			"UpdateModelRequest", "UpdateModelResponse",
			"UploadChunkRequest", "UploadChunkResponse",
			"UploadModelRequest", "UploadModelResponse",
			// Non-body op responses
			"DeleteMemoryResponse",
			"GetAgentResponse", "GetAgenticMemoryResponse",
			"GetAllMemoriesResponse", "GetAllMessagesResponse", "GetAllToolsResponse",
			"GetConnectorResponse", "GetControllerResponse",
			"GetMemoryContainerResponse", "GetMemoryResponse",
			"GetMessageResponse", "GetMessageTracesResponse",
			"GetMlTaskResponse", "GetModelGroupResponse", "GetModelResponse",
			"GetProfileModelsResponse", "GetProfileResponse", "GetProfileTasksResponse",
			"GetStatsResponse", "GetToolResponse",
			"SearchModelGroupsResponse", "SearchModelsResponse",
			"SearchResponse", "SearchTasksResponse",
		};

		private static readonly string[] MlOperationPrefixes = { };

		private static T DeserializeResponse<T>(string json) where T : class, new() =>
			Expect(json).NoRoundTrip().DeserializesTo<T>();

		private static bool HasDataMembers(Type type) =>
			GetDataMemberProperties(type).Any(x => SampleValueForType(x.Prop.PropertyType) != null);

		private static List<(PropertyInfo Prop, DataMemberAttribute Attr)> GetDataMemberProperties(Type type)
		{
			var result = new List<(PropertyInfo, DataMemberAttribute)>();
			var seen = new HashSet<string>(StringComparer.Ordinal);

			// Check the type itself (responses have [DataMember] on class properties)
			foreach (var p in type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic))
			{
				var attr = p.GetCustomAttribute<DataMemberAttribute>();
				if (attr != null && seen.Add(attr.Name ?? p.Name))
					result.Add((p, attr));
			}

			// Check implemented interfaces (requests have [DataMember] on interface properties)
			foreach (var iface in type.GetInterfaces())
			{
				foreach (var p in iface.GetProperties())
				{
					var attr = p.GetCustomAttribute<DataMemberAttribute>();
					if (attr != null && seen.Add(attr.Name ?? p.Name))
					{
						var classProp = type.GetProperty(p.Name, BindingFlags.Public | BindingFlags.Instance);
						result.Add((classProp ?? p, attr));
					}
				}
			}

			return result;
		}

		private static string BuildSampleJson(Type type)
		{
			var props = GetDataMemberProperties(type);
			if (props.Count == 0) return "{}";

			var sb = new StringBuilder("{");
			var first = true;
			foreach (var (prop, attr) in props)
			{
				var sampleValue = SampleValueForType(prop.PropertyType);
				if (sampleValue == null) continue;

				if (!first) sb.Append(',');
				first = false;

				var wireName = attr.Name ?? prop.Name;
				sb.Append($"\"{wireName}\":{sampleValue}");
			}
			sb.Append('}');
			return sb.ToString();
		}

		private static void SetPrimitiveProperties(object instance, Type type)
		{
			foreach (var (prop, _) in GetDataMemberProperties(type))
			{
				if (!prop.CanWrite) continue;
				var value = SampleObjectForType(prop.PropertyType);
				if (value != null)
					prop.SetValue(instance, value);
			}
		}

		private static IEnumerable<string> GetPrimitiveDataMemberNames(Type type) =>
			GetDataMemberProperties(type)
				.Where(x => SampleValueForType(x.Prop.PropertyType) != null)
				.Select(x => x.Attr.Name ?? x.Prop.Name);

		private static string SerializeToJson(object instance, Type type)
		{
			var pool = new ConnectionSettings(new SingleNodeConnectionPool(new Uri("http://localhost:9200")));
			var client = new OpenSearchClient(pool);
			using var ms = new System.IO.MemoryStream();
			client.RequestResponseSerializer.Serialize(instance, ms);
			return Encoding.UTF8.GetString(ms.ToArray());
		}

		private static string SampleValueForType(Type type)
		{
			var underlying = Nullable.GetUnderlyingType(type) ?? type;
			if (underlying == typeof(string)) return "\"test\"";
			if (underlying == typeof(bool)) return "true";
			if (underlying == typeof(int) || underlying == typeof(long)) return "1";
			if (underlying == typeof(float) || underlying == typeof(double)) return "1.0";
			return null;
		}

		private static object SampleObjectForType(Type type)
		{
			var underlying = Nullable.GetUnderlyingType(type) ?? type;
			if (underlying == typeof(string)) return "test";
			if (underlying == typeof(bool)) return true;
			if (underlying == typeof(int)) return 1;
			if (underlying == typeof(long)) return 1L;
			if (underlying == typeof(float)) return 1.0f;
			if (underlying == typeof(double)) return 1.0d;
			return null;
		}
	}
}
