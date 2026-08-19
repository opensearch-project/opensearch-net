/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.Net;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Core.Client;
using Tests.Domain;

namespace Tests.QueryDsl.Specialized.Knn;

/// <summary>
/// Unit serialization coverage for the version-gated <see cref="IKnnQuery"/> fields
/// <c>method_parameters</c> (2.16), <c>rescore</c> (2.17), and <c>expand_nested_docs</c> (2.19).
/// These are asserted at the serialization level only — they are deliberately kept out of the
/// live-cluster <c>KnnQueryUsageTests</c> because they are unsupported on older versions in the
/// integration matrix and <c>expand_nested_docs</c> additionally requires a nested field server-side.
/// </summary>
public class KnnQueryFieldsTests
{
	private static string Serialize(QueryContainer query) =>
		TestClient.DisabledStreaming.RequestResponseSerializer.SerializeToString(query);

	[U]
	public void FluentSerializesAllFields()
	{
		var query = new QueryContainerDescriptor<Project>()
			.Knn(k => k
				.Field(f => f.Vector)
				.Vector(1.5f, -2.6f)
				.K(30)
				.MethodParameters(mp => mp.Add("ef_search", 100))
				.Rescore(r => r.OversampleFactor(1.5f))
				.ExpandNestedDocs());

		var json = Serialize(query);

		json.Should().Contain("\"method_parameters\":{\"ef_search\":100}");
		json.Should().Contain("\"rescore\":{\"oversample_factor\":1.5}");
		json.Should().Contain("\"expand_nested_docs\":true");
	}

	[U]
	public void InitializerSerializesAllFields()
	{
		var query = new KnnQuery
		{
			Field = "vector",
			Vector = new[] { 1.5f, -2.6f },
			K = 30,
			MethodParameters = new System.Collections.Generic.Dictionary<string, object> { { "ef_search", 100 } },
			Rescore = new KnnQueryRescoreContext { OversampleFactor = 1.5f },
			ExpandNestedDocs = true,
		};

		var json = Serialize(query);

		json.Should().Contain("\"method_parameters\":{\"ef_search\":100}");
		json.Should().Contain("\"rescore\":{\"oversample_factor\":1.5}");
		json.Should().Contain("\"expand_nested_docs\":true");
	}

	[U]
	public void RescoreBooleanSerializes()
	{
		var enabled = Serialize(new QueryContainerDescriptor<Project>()
			.Knn(k => k.Field(f => f.Vector).Vector(1.5f, -2.6f).K(30).Rescore(true)));
		enabled.Should().Contain("\"rescore\":true");

		var disabled = Serialize(new QueryContainerDescriptor<Project>()
			.Knn(k => k.Field(f => f.Vector).Vector(1.5f, -2.6f).K(30).Rescore(false)));
		disabled.Should().Contain("\"rescore\":false");
	}
}
