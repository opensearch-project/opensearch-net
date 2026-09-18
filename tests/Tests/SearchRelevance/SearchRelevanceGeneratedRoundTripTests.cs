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
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Xunit;

namespace Tests.SearchRelevance
{
	/// <summary>
	/// Data-driven serialization tests for all generated search_relevance request and response
	/// types. Builds minimal JSON fixtures from [DataMember] properties and verifies round-trip
	/// correctness. Mirrors <see cref="Tests.ML.MLGeneratedRoundTripTests"/>; serves as a
	/// regression safety net for serializer migrations.
	/// </summary>
	public class SearchRelevanceGeneratedRoundTripTests
	{
		// ── Response deserialization ──────────────────────────────────────────────

		public static IEnumerable<object[]> SearchRelevanceResponseTypes =>
			FindSearchRelevanceTypes("Response")
				.Where(t => typeof(ResponseBase).IsAssignableFrom(t)
					&& !typeof(WriteResponseBase).IsAssignableFrom(t)
					&& HasDataMembers(t))
				.Select(t => new object[] { t });

		[TU]
		[MemberData(nameof(SearchRelevanceResponseTypes))]
		public void Response_Deserializes_WithoutException(Type responseType)
		{
			var json = BuildSampleJson(responseType);
			var method = typeof(SearchRelevanceGeneratedRoundTripTests)
				.GetMethod(nameof(DeserializeResponse), BindingFlags.NonPublic | BindingFlags.Static)!
				.MakeGenericMethod(responseType);

			var result = method.Invoke(null, new object[] { json });
			result.Should().NotBeNull($"{responseType.Name} should deserialize from: {json}");
		}

		// ── Request serialization ────────────────────────────────────────────────

		public static IEnumerable<object[]> SearchRelevanceRequestTypes =>
			FindSearchRelevanceTypes("Request")
				.Where(t => HasDataMembers(t))
				.Select(t => new object[] { t });

		[TU]
		[MemberData(nameof(SearchRelevanceRequestTypes))]
		public void Request_RoundTrips_PrimitiveProperties(Type requestType)
		{
			// Build sample JSON from [DataMember] wire names and deserialize into the request type.
			// This tests the same code path as responses — verifying DataMember bindings work,
			// including camelCase wire names like "querySetId"/"searchConfigurationList".
			var json = BuildSampleJson(requestType);
			var method = typeof(SearchRelevanceGeneratedRoundTripTests)
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

		private static IEnumerable<Type> FindSearchRelevanceTypes(string suffix) =>
			typeof(OpenSearchClient).Assembly
				.GetTypes()
				.Where(t => t.Namespace == "OpenSearch.Client"
					&& t.Name.EndsWith(suffix)
					&& t.IsClass && !t.IsAbstract
					&& SearchRelevanceGeneratedNames.Contains(t.Name))
				.OrderBy(t => t.Name);

		private static readonly HashSet<string> SearchRelevanceGeneratedNames = new(StringComparer.Ordinal)
		{
			// Body ops
			"PostQuerySetsRequest", "PostQuerySetsResponse",
			"PostScheduledExperimentsRequest", "PostScheduledExperimentsResponse",
			"PutExperimentsRequest", "PutExperimentsResponse",
			"PutJudgmentsRequest", "PutJudgmentsResponse",
			"PutQuerySetsRequest", "PutQuerySetsResponse",
			"PutSearchConfigurationsRequest", "PutSearchConfigurationsResponse",
			"ExperimentsSearchRequest", "ExperimentsSearchResponse",
			"JudgmentsSearchRequest", "JudgmentsSearchResponse",
			"QuerySetsSearchRequest", "QuerySetsSearchResponse",
			"SearchConfigurationsSearchRequest", "SearchConfigurationsSearchResponse",
			// Non-body op responses
			"DeleteExperimentsResponse", "DeleteJudgmentsResponse", "DeleteQuerySetsResponse",
			"DeleteScheduledExperimentsResponse", "DeleteSearchConfigurationsResponse",
			"GetExperimentsResponse", "GetJudgmentsResponse", "GetNodeStatsResponse",
			"GetQuerySetsResponse", "GetScheduledExperimentsResponse",
			"GetSearchConfigurationsResponse", "GetSearchRelevanceStatsResponse",
			// Composition-flattened variant types (search_relevance.put_experiments/put_judgments
			// anyOf/oneOf siblings — see OperationModel.FlattenCompositionProperties)
			"PutHybridOptimizerExperimentRequest", "PutPairwiseExperimentRequest",
			"PutPointwiseExperimentRequest", "PutLLMJudgmentsRequest", "PutUBIJudgmentsRequest",
			"PutImportJudgmentsRequest",
			// Shared model with a request-shaped name (search_relevance._common___PutSearchConfigurationRequest,
			// the per-configuration schema also referenced from PutSearchConfigurationsRequest's body)
			"PutSearchConfigurationRequest",
			// Shared body schema for the four *_search operations above
			// (search_relevance._common___SearchRequest, renamed to avoid colliding with the
			// core Search API's SearchRequest — see SearchRelevanceModelOverrides.RenamedTypes).
			"SearchRelevanceSearchRequest",
		};

		private static T DeserializeResponse<T>(string json) where T : class, new()
		{
			var pool = new ConnectionSettings(new SingleNodeConnectionPool(new Uri("http://localhost:9200")));
			var client = new OpenSearchClient(pool);
			using var ms = new System.IO.MemoryStream(Encoding.UTF8.GetBytes(json));
			return client.RequestResponseSerializer.Deserialize<T>(ms);
		}

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

		private static string SampleValueForType(Type type)
		{
			var underlying = Nullable.GetUnderlyingType(type) ?? type;
			if (underlying == typeof(string)) return "\"test\"";
			if (underlying == typeof(bool)) return "true";
			if (underlying == typeof(int) || underlying == typeof(long)) return "1";
			if (underlying == typeof(float) || underlying == typeof(double)) return "1.0";
			return null;
		}
	}
}
