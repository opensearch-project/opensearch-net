/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using OpenSearch.Net.Utf8Json;

namespace OpenSearch.Client;

/// <summary>
/// A neural query.
/// </summary>
[InterfaceDataContract]
[JsonFormatter(typeof(FieldNameQueryFormatter<NeuralQuery, INeuralQuery>))]
public interface INeuralQuery : IFieldNameQuery
{
	/// <summary>
	/// The query text from which to produce queries.
	/// You must specify at least one of <see cref="QueryText" /> or <see cref="QueryImage" />.
	/// </summary>
	[DataMember(Name = "query_text")]
	string QueryText { get; set; }

	/// <summary>
	/// A Base64-encoded string that corresponds to the query image from which to generate vector embeddings.
	/// You must specify at least one of <see cref="QueryText" /> or <see cref="QueryImage" />.
	/// </summary>
	[DataMember(Name = "query_image")]
	string QueryImage { get; set; }

	/// <summary>
	/// The number of results the k-NN search returns. Only one of <see cref="K" />, <see cref="MaxDistance" />,
	/// or <see cref="MinScore" /> can be specified; if none is set, the server default is <c>k = 10</c>.
	/// </summary>
	[DataMember(Name = "k")]
	int? K { get; set; }

	/// <summary>
	/// The maximum physical vector space distance required in order for a neighbor to be considered a hit (radial search).
	/// Only one of <see cref="K" />, <see cref="MaxDistance" />, or <see cref="MinScore" /> can be specified.
	/// </summary>
	[DataMember(Name = "max_distance")]
	float? MaxDistance { get; set; }

	/// <summary>
	/// The minimum similarity score required in order for a neighbor to be considered a hit (radial search).
	/// Only one of <see cref="K" />, <see cref="MaxDistance" />, or <see cref="MinScore" /> can be specified.
	/// </summary>
	[DataMember(Name = "min_score")]
	float? MinScore { get; set; }

	/// <summary>
	/// The ID of the model that will be used in the embedding interface.
	/// The model must be indexed in OpenSearch before it can be used in Neural Search.
	/// </summary>
	[DataMember(Name = "model_id")]
	string ModelId { get; set; }

	/// <summary>
	/// A query used to reduce the number of documents considered.
	/// </summary>
	[DataMember(Name = "filter")]
	IQueryContainer Filter { get; set; }

	/// <summary>
	/// A raw sparse vector, expressed as tokens and their weights. Used as an alternative to
	/// <see cref="QueryText" /> for direct vector input.
	/// </summary>
	[DataMember(Name = "query_tokens")]
	IDictionary<string, float> QueryTokens { get; set; }

	/// <summary>
	/// An analyzer for tokenizing <see cref="QueryText" /> when using a sparse encoding model.
	/// Cannot be used together with <see cref="ModelId" />.
	/// </summary>
	[DataMember(Name = "semantic_field_search_analyzer")]
	string SemanticFieldSearchAnalyzer { get; set; }

	/// <summary>
	/// Additional engine-specific parameters that control the approximate k-NN search at query time,
	/// such as <c>ef_search</c> (HNSW) or <c>nprobes</c> (IVF).
	/// </summary>
	[DataMember(Name = "method_parameters")]
	IDictionary<string, object> MethodParameters { get; set; }

	/// <summary>
	/// Controls the rescoring of approximate k-NN search results using full-precision vectors.
	/// Pass a <see cref="bool" /> to enable or disable rescoring, or a <see cref="KnnQueryRescoreContext" />
	/// to configure it.
	/// </summary>
	[DataMember(Name = "rescore")]
	Union<bool, KnnQueryRescoreContext> Rescore { get; set; }

	/// <summary>
	/// When <c>true</c>, retrieves scores for all nested field documents within each parent document.
	/// Used with nested queries.
	/// </summary>
	[DataMember(Name = "expand_nested_docs")]
	bool? ExpandNestedDocs { get; set; }
}

[DataContract]
public class NeuralQuery : FieldNameQueryBase, INeuralQuery
{
	/// <inheritdoc />
	public string QueryText { get; set; }
	/// <inheritdoc />
	public string QueryImage { get; set; }
	/// <inheritdoc />
	public int? K { get; set; }
	/// <inheritdoc />
	public float? MaxDistance { get; set; }
	/// <inheritdoc />
	public float? MinScore { get; set; }
	/// <inheritdoc />
	public string ModelId { get; set; }
	/// <inheritdoc />
	public IQueryContainer Filter { get; set; }
	/// <inheritdoc />
	public IDictionary<string, float> QueryTokens { get; set; }
	/// <inheritdoc />
	public string SemanticFieldSearchAnalyzer { get; set; }
	/// <inheritdoc />
	public IDictionary<string, object> MethodParameters { get; set; }
	/// <inheritdoc />
	public Union<bool, KnnQueryRescoreContext> Rescore { get; set; }
	/// <inheritdoc />
	public bool? ExpandNestedDocs { get; set; }

	protected override bool Conditionless => IsConditionless(this);

	internal override void InternalWrapInContainer(IQueryContainer container) => container.Neural = this;

	// A neural query needs an input and a field. The input is one of query_text, query_image (dense),
	// or query_tokens (raw sparse vector). For dense k-NN it is bounded by at most one of k / max_distance /
	// min_score (radial search); when none is set the server defaults to k=10, so an input-and-field query is
	// valid on its own. Requiring k alone silently dropped valid radial-search queries.
	internal static bool IsConditionless(INeuralQuery q) =>
		(string.IsNullOrEmpty(q.QueryText) && string.IsNullOrEmpty(q.QueryImage) && (q.QueryTokens == null || q.QueryTokens.Count == 0))
		|| q.Field.IsConditionless();
}

public class NeuralQueryDescriptor<T>
	: FieldNameQueryDescriptorBase<NeuralQueryDescriptor<T>, INeuralQuery, T>,
		INeuralQuery
	where T : class
{
	protected override bool Conditionless => NeuralQuery.IsConditionless(this);
	string INeuralQuery.QueryText { get; set; }
	string INeuralQuery.QueryImage { get; set; }
	int? INeuralQuery.K { get; set; }
	float? INeuralQuery.MaxDistance { get; set; }
	float? INeuralQuery.MinScore { get; set; }
	string INeuralQuery.ModelId { get; set; }
	IQueryContainer INeuralQuery.Filter { get; set; }
	IDictionary<string, float> INeuralQuery.QueryTokens { get; set; }
	string INeuralQuery.SemanticFieldSearchAnalyzer { get; set; }
	IDictionary<string, object> INeuralQuery.MethodParameters { get; set; }
	Union<bool, KnnQueryRescoreContext> INeuralQuery.Rescore { get; set; }
	bool? INeuralQuery.ExpandNestedDocs { get; set; }

	/// <inheritdoc cref="INeuralQuery.QueryText" />
	public NeuralQueryDescriptor<T> QueryText(string queryText) => Assign(queryText, (a, v) => a.QueryText = v);

	/// <inheritdoc cref="INeuralQuery.QueryImage" />
	public NeuralQueryDescriptor<T> QueryImage(string queryImage) => Assign(queryImage, (a, v) => a.QueryImage = v);

	/// <inheritdoc cref="INeuralQuery.K" />
	public NeuralQueryDescriptor<T> K(int? k) => Assign(k, (a, v) => a.K = v);

	/// <inheritdoc cref="INeuralQuery.MaxDistance" />
	public NeuralQueryDescriptor<T> MaxDistance(float? maxDistance) => Assign(maxDistance, (a, v) => a.MaxDistance = v);

	/// <inheritdoc cref="INeuralQuery.MinScore" />
	public NeuralQueryDescriptor<T> MinScore(float? minScore) => Assign(minScore, (a, v) => a.MinScore = v);

	/// <inheritdoc cref="INeuralQuery.ModelId" />
	public NeuralQueryDescriptor<T> ModelId(string modelId) => Assign(modelId, (a, v) => a.ModelId = v);

	/// <inheritdoc cref="INeuralQuery.Filter" />
	public NeuralQueryDescriptor<T> Filter(Func<QueryContainerDescriptor<T>, QueryContainer> filterSelector) =>
		Assign(filterSelector, (a, v) => a.Filter = v?.Invoke(new QueryContainerDescriptor<T>()));

	/// <inheritdoc cref="INeuralQuery.QueryTokens" />
	public NeuralQueryDescriptor<T> QueryTokens(Func<FluentDictionary<string, float>, FluentDictionary<string, float>> selector) =>
		Assign(selector, (a, v) => a.QueryTokens = v?.Invoke(new FluentDictionary<string, float>()));

	/// <inheritdoc cref="INeuralQuery.SemanticFieldSearchAnalyzer" />
	public NeuralQueryDescriptor<T> SemanticFieldSearchAnalyzer(string analyzer) =>
		Assign(analyzer, (a, v) => a.SemanticFieldSearchAnalyzer = v);

	/// <inheritdoc cref="INeuralQuery.MethodParameters" />
	public NeuralQueryDescriptor<T> MethodParameters(Func<FluentDictionary<string, object>, FluentDictionary<string, object>> selector) =>
		Assign(selector, (a, v) => a.MethodParameters = v?.Invoke(new FluentDictionary<string, object>()));

	/// <inheritdoc cref="INeuralQuery.Rescore" />
	public NeuralQueryDescriptor<T> Rescore(bool? enable = true) =>
		Assign(enable, (a, v) => a.Rescore = v.HasValue ? new Union<bool, KnnQueryRescoreContext>(v.Value) : null);

	/// <inheritdoc cref="INeuralQuery.Rescore" />
	public NeuralQueryDescriptor<T> Rescore(Func<KnnQueryRescoreContextDescriptor, IKnnQueryRescoreContext> selector) =>
		Assign(selector, (a, v) => a.Rescore = new KnnQueryRescoreContext { OversampleFactor = v?.Invoke(new KnnQueryRescoreContextDescriptor())?.OversampleFactor });

	/// <inheritdoc cref="INeuralQuery.ExpandNestedDocs" />
	public NeuralQueryDescriptor<T> ExpandNestedDocs(bool? expandNestedDocs = true) =>
		Assign(expandNestedDocs, (a, v) => a.ExpandNestedDocs = v);
}
