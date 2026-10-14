/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.Net;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Xunit;

namespace Tests.ML
{
	/// <summary>
	/// Data-driven serialization tests for all generated ML request and response types.
	/// Builds a JSON fixture for every <c>[DataMember]</c> property — recursing into lists, dictionaries and
	/// nested models — deserializes it with both the Utf8Json and System.Text.Json serializers, and asserts every
	/// value arrived. Serves as a regression safety net for type mappings and serializer migrations.
	/// <para>
	/// Fixtures are derived from the generated C# types, so they catch types that cannot be (de)serialized but not
	/// types that are self-consistent yet wrong for the wire; <c>ApiGenerator.Tests.SpecTypeFidelityTests</c>
	/// covers the latter against the OpenAPI spec.
	/// </para>
	/// </summary>
	public class MLGeneratedRoundTripTests
	{
		private const int MaxDepth = 4;

		// ── Response deserialization ──────────────────────────────────────────────

		public static IEnumerable<object[]> MLResponseTypes =>
			FindMLTypes("Response")
				.Where(t => typeof(ResponseBase).IsAssignableFrom(t)
					&& !typeof(WriteResponseBase).IsAssignableFrom(t)
					&& HasDataMembers(t))
				.SelectMany(WithBothSerializers);

		[TU]
		[MemberData(nameof(MLResponseTypes))]
		public void Response_Deserializes_AllProperties(Type responseType, bool useSystemTextJson) =>
			AssertDeserializesAllProperties(responseType, useSystemTextJson);

		// ── Request deserialization ──────────────────────────────────────────────

		public static IEnumerable<object[]> MLRequestTypes =>
			FindMLTypes("Request")
				.Where(t => HasDataMembers(t))
				.SelectMany(WithBothSerializers);

		[TU]
		[MemberData(nameof(MLRequestTypes))]
		public void Request_Deserializes_AllProperties(Type requestType, bool useSystemTextJson) =>
			AssertDeserializesAllProperties(requestType, useSystemTextJson);

		// ── Coverage guard ───────────────────────────────────────────────────────

		/// <summary>
		/// Property types the fixture builder cannot produce a value for are skipped, which is how the original
		/// version of this test missed <c>Name</c>, <c>NodeIds</c> and list properties. Fail on any such type
		/// rather than skipping it silently; extend <see cref="SampleJson"/> when this fires.
		/// </summary>
		[U]
		public void Fixtures_CoverEveryPropertyType()
		{
			var unsupported = new SortedSet<string>(StringComparer.Ordinal);
			foreach (var type in FindMLTypes("Request").Concat(FindMLTypes("Response")))
				CollectUnsupported(type, unsupported, new HashSet<Type>());

			unsupported.Should().BeEmpty("every generated ML property type should have a JSON fixture value, missing:\n"
				+ string.Join("\n", unsupported));
		}

		// ── Assertion ────────────────────────────────────────────────────────────

		private static void AssertDeserializesAllProperties(Type type, bool useSystemTextJson)
		{
			var json = BuildSampleJson(type, 0);
			var serializer = useSystemTextJson ? "System.Text.Json" : "Utf8Json";

			object result;
			try
			{
				result = Deserialize(type, json, useSystemTextJson);
			}
			catch (Exception e)
			{
				throw new Exception($"{type.Name} failed to deserialize with {serializer} from: {json}", e);
			}

			result.Should().NotBeNull($"{type.Name} should deserialize with {serializer} from: {json}");
			AssertPopulated(type, result, type.Name, 0, $"{serializer}, json: {json}");
		}

		/// <summary>Every [DataMember] property that received a fixture value must be populated after reading.</summary>
		private static void AssertPopulated(Type type, object instance, string path, int depth, string context)
		{
			foreach (var (prop, attr) in GetDataMemberProperties(type))
			{
				if (SampleJson(prop.PropertyType, depth + 1) == null) continue;

				var value = prop.GetValue(instance);
				var propPath = $"{path}.{prop.Name} (\"{attr.Name ?? prop.Name}\")";
				value.Should().NotBeNull($"{propPath} should be populated ({context})");

				switch (value)
				{
					case string s:
						s.Should().Be("test", $"{propPath} ({context})");
						break;
					case IDictionary dictionary:
						dictionary.Count.Should().Be(1, $"{propPath} should contain the fixture entry ({context})");
						break;
					case IEnumerable enumerable when !(value is string):
						var items = enumerable.Cast<object>().ToList();
						items.Should().HaveCount(1, $"{propPath} should contain the fixture item ({context})");
						items[0].Should().NotBeNull($"{propPath}[0] should be populated ({context})");
						break;
					default:
						var valueType = value.GetType();
						if (IsStringBacked(valueType))
							value.ToString().Should().Be("test", $"{propPath} ({context})");
						else if (!LeafFixtures.ContainsKey(valueType) && IsModel(valueType) && depth < MaxDepth)
							AssertPopulated(valueType, value, propPath, depth + 1, context);
						break;
				}
			}
		}

		private static object Deserialize(Type type, string json, bool useSystemTextJson)
		{
			var settings = new ConnectionSettings(new SingleNodeConnectionPool(new Uri("http://localhost:9200")))
				.UseSystemTextJson(useSystemTextJson);
			var client = new OpenSearchClient(settings);
			using var ms = new MemoryStream(Encoding.UTF8.GetBytes(json));
			return client.RequestResponseSerializer.Deserialize(type, ms);
		}

		// ── Fixture builder ──────────────────────────────────────────────────────

		private static string BuildSampleJson(Type type, int depth)
		{
			var sb = new StringBuilder("{");
			var first = true;
			foreach (var (prop, attr) in GetDataMemberProperties(type))
			{
				var sampleValue = SampleJson(prop.PropertyType, depth + 1);
				if (sampleValue == null) continue;

				if (!first) sb.Append(',');
				first = false;
				sb.Append($"\"{attr.Name ?? prop.Name}\":{sampleValue}");
			}
			sb.Append('}');
			return sb.ToString();
		}

		/// <summary>A JSON fixture value for a property of <paramref name="type"/>, or <c>null</c> if unsupported.</summary>
		private static string SampleJson(Type type, int depth)
		{
			var t = Nullable.GetUnderlyingType(type) ?? type;

			if (LeafFixtures.TryGetValue(t, out var leaf)) return leaf;
			if (IsStringBacked(t)) return "\"test\"";
			if (t.IsEnum) return EnumWireValue(t);

			if (depth > MaxDepth) return null;

			if (TryGetDictionaryValueType(t, out var valueType))
			{
				var value = SampleJson(valueType, depth + 1);
				return value == null ? null : $"{{\"key\":{value}}}";
			}

			if (TryGetEnumerableItemType(t, out var itemType))
			{
				var item = SampleJson(itemType, depth + 1);
				return item == null ? null : $"[{item}]";
			}

			var concrete = ConcreteModelType(t);
			if (concrete != null && GetDataMemberProperties(concrete).Count > 0)
				return BuildSampleJson(concrete, depth);

			return null;
		}

		/// <summary>
		/// Walks every model reachable from <paramref name="type"/> (each visited once, so depth is unbounded) and
		/// records properties whose type has no fixture. Support is checked from depth 1, independent of where the
		/// type sits in the graph, so a deeply nested model is checked like a top-level one.
		/// </summary>
		private static void CollectUnsupported(Type type, ISet<string> unsupported, ISet<Type> visited)
		{
			if (!visited.Add(type)) return;

			foreach (var (prop, _) in GetDataMemberProperties(type))
			{
				var propType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
				if (SampleJson(propType, 1) == null)
					unsupported.Add($"{type.Name}.{prop.Name}: {Describe(propType)}");

				var nested = ConcreteModelType(ElementType(propType));
				if (nested != null)
					CollectUnsupported(nested, unsupported, visited);
			}
		}

		private static string Describe(Type t) =>
			t.IsGenericType
				? $"{t.Name.Substring(0, t.Name.IndexOf('`'))}<{string.Join(", ", t.GetGenericArguments().Select(Describe))}>"
				: t.Name;

		/// <summary>
		/// Fixture values for types that are read as a single value. Handwritten polymorphic containers (queries)
		/// are leaves too: they expose one property per variant, so recursing into them would expect all variants.
		/// </summary>
		private static readonly Dictionary<Type, string> LeafFixtures = new Dictionary<Type, string>
		{
			{ typeof(string), "\"test\"" },
			{ typeof(object), "\"test\"" },
			{ typeof(bool), "true" },
			{ typeof(short), "1" },
			{ typeof(int), "1" },
			{ typeof(long), "1" },
			{ typeof(float), "1.5" },
			{ typeof(double), "1.5" },
			{ typeof(decimal), "1.5" },
			{ typeof(DateTime), "\"2024-01-01T00:00:00Z\"" },
			{ typeof(DateTimeOffset), "\"2024-01-01T00:00:00Z\"" },
			{ typeof(Time), "\"1s\"" },
			{ typeof(IQueryContainer), "{\"match_all\":{}}" },
			{ typeof(QueryContainer), "{\"match_all\":{}}" },
			{ typeof(Fields), "[\"test\"]" },
			// NodeIds is a list of node ids and is always an array on the wire.
			{ typeof(NodeIds), "[\"test\"]" },
			{ typeof(ErrorCause), "{\"type\":\"test\",\"reason\":\"test\"}" },
		};

		/// <summary>Path-parameter style types whose fixture value reads back through <c>ToString()</c> as <c>"test"</c>.</summary>
		private static bool IsStringBacked(Type t) =>
			t == typeof(Name) || t == typeof(Id) || t == typeof(IndexName) || t == typeof(NodeIds);

		private static bool IsModel(Type t) => ConcreteModelType(t) != null && GetDataMemberProperties(t).Count > 0;

		/// <summary>The concrete type to read a model as: the class itself, or an interface's [ReadAs] target.</summary>
		private static Type ConcreteModelType(Type t)
		{
			if (t == null || t.IsPrimitive || t.IsEnum || LeafFixtures.ContainsKey(t) || IsStringBacked(t)) return null;
			if (t.IsInterface)
			{
				var readAs = t.GetCustomAttributes(false).FirstOrDefault(a => a.GetType().Name == "ReadAsAttribute");
				var target = readAs?.GetType().GetProperty("Type")?.GetValue(readAs) as Type;
				return target != null && !target.IsGenericTypeDefinition ? target : null;
			}
			return t.IsClass && !t.IsAbstract && (t.Namespace == "OpenSearch.Client" || t.Namespace == "OpenSearch.Net") ? t : null;
		}

		private static Type ElementType(Type t)
		{
			if (TryGetDictionaryValueType(t, out var v)) return ElementType(v);
			if (TryGetEnumerableItemType(t, out var i)) return ElementType(i);
			return Nullable.GetUnderlyingType(t) ?? t;
		}

		private static bool TryGetDictionaryValueType(Type t, out Type valueType)
		{
			var dict = t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IDictionary<,>)
				? t
				: t.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDictionary<,>))
				?? (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IReadOnlyDictionary<,>) ? t : null);
			valueType = dict != null && dict.GetGenericArguments()[0] == typeof(string) ? dict.GetGenericArguments()[1] : null;
			return valueType != null;
		}

		private static bool TryGetEnumerableItemType(Type t, out Type itemType)
		{
			itemType = null;
			if (t == typeof(string)) return false;
			if (t.IsArray) itemType = t.GetElementType();
			else if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>)) itemType = t.GetGenericArguments()[0];
			else
				itemType = t.GetInterfaces()
					.FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>))
					?.GetGenericArguments()[0];
			return itemType != null;
		}

		private static string EnumWireValue(Type enumType)
		{
			var field = enumType.GetFields(BindingFlags.Public | BindingFlags.Static).First();
			var wire = field.GetCustomAttribute<EnumMemberAttribute>()?.Value ?? field.Name;
			return $"\"{wire}\"";
		}

		// ── Type discovery ───────────────────────────────────────────────────────

		private static IEnumerable<object[]> WithBothSerializers(Type t) =>
			new[] { new object[] { t, false }, new object[] { t, true } };

		private static IEnumerable<Type> FindMLTypes(string suffix) =>
			typeof(OpenSearchClient).Assembly
				.GetTypes()
				.Where(t => t.Namespace == "OpenSearch.Client"
					&& t.Name.EndsWith(suffix)
					&& t.IsClass && !t.IsAbstract
					&& IsMLGenerated(t.Name, suffix))
				.OrderBy(t => t.Name);

		private static bool IsMLGenerated(string typeName, string suffix) =>
			MLOperationPrefixes.Any(p => typeName.StartsWith(p, StringComparison.Ordinal))
			|| MLGeneratedNames.Contains(typeName);

		private static readonly HashSet<string> MLGeneratedNames = new(StringComparer.Ordinal)
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
			"GetProfileRequest",
			"GetProfileModelsRequest",
			"GetProfileTasksRequest",
			"LoadModelRequest", "LoadModelResponse",
			"PredictModelRequest", "PredictModelResponse",
			"PredictRequest", "PredictResponse",
			"ProfileRequest",
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
			"SearchModelsRequest",
			"SearchTasksRequest",
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
			"GetMLTaskResponse", "GetModelGroupResponse", "GetModelResponse",
			"GetProfileModelsResponse", "GetProfileResponse", "GetProfileTasksResponse",
			"GetStatsResponse", "GetToolResponse",
			"SearchModelGroupsResponse", "SearchModelsResponse",
			"SearchResponse", "SearchTasksResponse",
		};

		private static readonly string[] MLOperationPrefixes = { };

		private static bool HasDataMembers(Type type) =>
			GetDataMemberProperties(type).Any(x => SampleJson(x.Prop.PropertyType, 1) != null);

		private static List<(PropertyInfo Prop, DataMemberAttribute Attr)> GetDataMemberProperties(Type type)
		{
			var result = new List<(PropertyInfo, DataMemberAttribute)>();
			var seen = new HashSet<string>(StringComparer.Ordinal);

			// Check the type itself (responses have [DataMember] on class properties)
			foreach (var p in type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic))
			{
				if (p.DeclaringType == typeof(ResponseBase)) continue;
				var attr = p.GetCustomAttribute<DataMemberAttribute>();
				if (attr != null && seen.Add(attr.Name ?? p.Name))
					result.Add((p, attr));
			}

			// Check implemented interfaces (requests and models have [DataMember] on interface properties)
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
	}
}
