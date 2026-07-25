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
/// An approximate k-NN query.
/// </summary>
[InterfaceDataContract]
[JsonFormatter(typeof(FieldNameQueryFormatter<KnnQuery, IKnnQuery>))]
public interface IKnnQuery : IFieldNameQuery
{
	/// <summary>
	/// The vector to search for.
	/// </summary>
	[DataMember(Name = "vector")]
	float[] Vector { get; set; }

	/// <summary>
	/// The number of neighbors the search of each graph will return.
	/// </summary>
	[DataMember(Name = "k")]
	int? K { get; set; }

	/// <summary>
	/// The result restriction filter query.
	/// </summary>
	[DataMember(Name = "filter")]
	IQueryContainer Filter { get; set; }

    /// <summary>
    /// The maximum physical vector space distance required in order for a neighbor to be considered a hit.
    /// </summary>
    [DataMember(Name = "max_distance")]
    float? MaxDistance { get; set; }

    /// <summary>
    /// The minimum similarity score required in order for a neighbor to be considered a hit.
    /// </summary>
    [DataMember(Name = "min_score")]
    float? MinScore { get; set; }

	/// <summary>
	/// Additional engine-specific parameters that control the approximate k-NN search at query time,
	/// such as <c>ef_search</c> (HNSW) or <c>nprobes</c> (IVF). Supported from OpenSearch 2.16.
	/// </summary>
	[DataMember(Name = "method_parameters")]
	IDictionary<string, object> MethodParameters { get; set; }

	/// <summary>
	/// Controls the rescoring of approximate k-NN search results using full-precision vectors.
	/// Pass a <see cref="bool" /> to enable or disable rescoring, or a <see cref="KnnQueryRescoreContext" />
	/// to configure it. Supported from OpenSearch 2.17.
	/// </summary>
	[DataMember(Name = "rescore")]
	Union<bool, KnnQueryRescoreContext> Rescore { get; set; }

	/// <summary>
	/// Whether to expand and search over the nested documents of each matching parent document.
	/// Supported from OpenSearch 2.19.
	/// </summary>
	[DataMember(Name = "expand_nested_docs")]
	bool? ExpandNestedDocs { get; set; }
}

/// <summary>
/// Configures rescoring of approximate k-NN search results using full-precision vectors.
/// </summary>
[InterfaceDataContract]
[ReadAs(typeof(KnnQueryRescoreContext))]
public interface IKnnQueryRescoreContext
{
	/// <summary>
	/// The factor by which the candidate result set is expanded before rescoring with full-precision vectors.
	/// </summary>
	[DataMember(Name = "oversample_factor")]
	float? OversampleFactor { get; set; }
}

/// <inheritdoc cref="IKnnQueryRescoreContext" />
public class KnnQueryRescoreContext : IKnnQueryRescoreContext
{
	/// <inheritdoc />
	public float? OversampleFactor { get; set; }
}

[DataContract]
public class KnnQuery : FieldNameQueryBase, IKnnQuery
{
	/// <inheritdoc />
	public float[] Vector { get; set; }
	/// <inheritdoc />
	public int? K { get; set; }
	/// <inheritdoc />
	public IQueryContainer Filter { get; set; }
    /// <inheritdoc />
    public float? MaxDistance { get; set; }
    /// <inheritdoc />
    public float? MinScore { get; set; }
	/// <inheritdoc />
	public IDictionary<string, object> MethodParameters { get; set; }
	/// <inheritdoc />
	public Union<bool, KnnQueryRescoreContext> Rescore { get; set; }
	/// <inheritdoc />
	public bool? ExpandNestedDocs { get; set; }

	protected override bool Conditionless => IsConditionless(this);

	internal override void InternalWrapInContainer(IQueryContainer container) => container.Knn = this;

	internal static bool IsConditionless(IKnnQuery q) => q.Vector == null || q.Vector.Length == 0 || q.K == null || q.K == 0 || q.Field.IsConditionless();
}

public class KnnQueryDescriptor<T>
	: FieldNameQueryDescriptorBase<KnnQueryDescriptor<T>, IKnnQuery, T>,
		IKnnQuery
	where T : class
{
	protected override bool Conditionless => KnnQuery.IsConditionless(this);
	float[] IKnnQuery.Vector { get; set; }
	int? IKnnQuery.K { get; set; }
	IQueryContainer IKnnQuery.Filter { get; set; }
    float? IKnnQuery.MaxDistance { get; set; }
    float? IKnnQuery.MinScore { get; set; }
	IDictionary<string, object> IKnnQuery.MethodParameters { get; set; }
	Union<bool, KnnQueryRescoreContext> IKnnQuery.Rescore { get; set; }
	bool? IKnnQuery.ExpandNestedDocs { get; set; }

	/// <inheritdoc cref="IKnnQuery.Vector" />
	public KnnQueryDescriptor<T> Vector(params float[] vector) => Assign(vector, (a, v) => a.Vector = v);

	/// <inheritdoc cref="IKnnQuery.K" />
	public KnnQueryDescriptor<T> K(int? k) => Assign(k, (a, v) => a.K = v);

	/// <inheritdoc cref="IKnnQuery.Filter" />
	public KnnQueryDescriptor<T> Filter(Func<QueryContainerDescriptor<T>, QueryContainer> filterSelector) =>
		Assign(filterSelector, (a, v) => a.Filter = v?.Invoke(new QueryContainerDescriptor<T>()));

    /// <inheritdoc cref="IKnnQuery.MaxDistance" />
    public KnnQueryDescriptor<T> MaxDistance(float? maxDistance) =>
        Assign(maxDistance, (a, v) => a.MaxDistance = v);

    /// <inheritdoc cref="IKnnQuery.MinScore" />
    public KnnQueryDescriptor<T> MinScore(float? minScore) =>
        Assign(minScore, (a, v) => a.MinScore = v);

	/// <inheritdoc cref="IKnnQuery.MethodParameters" />
	public KnnQueryDescriptor<T> MethodParameters(Func<FluentDictionary<string, object>, FluentDictionary<string, object>> selector) =>
		Assign(selector, (a, v) => a.MethodParameters = v?.Invoke(new FluentDictionary<string, object>()));

	/// <inheritdoc cref="IKnnQuery.Rescore" />
	public KnnQueryDescriptor<T> Rescore(bool? enable = true) =>
		Assign(enable, (a, v) => a.Rescore = v.HasValue ? new Union<bool, KnnQueryRescoreContext>(v.Value) : null);

	/// <inheritdoc cref="IKnnQuery.Rescore" />
	public KnnQueryDescriptor<T> Rescore(Func<KnnQueryRescoreContextDescriptor, IKnnQueryRescoreContext> selector) =>
		Assign(selector, (a, v) => a.Rescore = new KnnQueryRescoreContext { OversampleFactor = v?.Invoke(new KnnQueryRescoreContextDescriptor())?.OversampleFactor });

	/// <inheritdoc cref="IKnnQuery.ExpandNestedDocs" />
	public KnnQueryDescriptor<T> ExpandNestedDocs(bool? expandNestedDocs = true) =>
		Assign(expandNestedDocs, (a, v) => a.ExpandNestedDocs = v);
}

/// <summary>
/// A descriptor for configuring k-NN query rescoring.
/// </summary>
public class KnnQueryRescoreContextDescriptor
	: DescriptorBase<KnnQueryRescoreContextDescriptor, IKnnQueryRescoreContext>, IKnnQueryRescoreContext
{
	float? IKnnQueryRescoreContext.OversampleFactor { get; set; }

	/// <inheritdoc cref="IKnnQueryRescoreContext.OversampleFactor" />
	public KnnQueryRescoreContextDescriptor OversampleFactor(float? oversampleFactor) =>
		Assign(oversampleFactor, (a, v) => a.OversampleFactor = v);
}
