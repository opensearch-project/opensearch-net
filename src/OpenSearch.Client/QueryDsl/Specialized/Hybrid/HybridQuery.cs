/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using OpenSearch.Net.Utf8Json;

namespace OpenSearch.Client;

/// <summary>
/// A hybrid query.
/// </summary>
[InterfaceDataContract]
[ReadAs(typeof(HybridQuery))]
public interface IHybridQuery : IQuery
{
	[DataMember(Name = "pagination_depth")]
    int? PaginationDepth { get; set; }

    [DataMember(Name = "queries")]
    IEnumerable<QueryContainer> Queries { get; set; }
}

[DataContract]
public class HybridQuery : QueryBase, IHybridQuery
{
    private IList<QueryContainer> _queries;

	/// <inheritdoc />
	public int? PaginationDepth { get; set; }

    /// <inheritdoc />
    public IEnumerable<QueryContainer> Queries
    {
        get => _queries;
        set => _queries = value?.AsInstanceOrToListOrNull();
    }

	protected override bool Conditionless => IsConditionless(this);

	internal override void InternalWrapInContainer(IQueryContainer container) => container.Hybrid = this;

	internal static bool IsConditionless(IHybridQuery q) => q.Queries.NotWritable();
}

public class HybridQueryDescriptor<T>
	: QueryDescriptorBase<HybridQueryDescriptor<T>, IHybridQuery>,
		IHybridQuery
	where T : class
{
    private IList<QueryContainer> _queries;

	protected override bool Conditionless => HybridQuery.IsConditionless(this);

	int? IHybridQuery.PaginationDepth { get; set; }
    IEnumerable<QueryContainer> IHybridQuery.Queries
    {
        get => _queries;
        set => _queries = value?.AsInstanceOrToListOrNull();
    }

	/// <inheritdoc cref="IHybridQuery.PaginationDepth" />
	public HybridQueryDescriptor<T> PaginationDepth(int? paginationDepth) => Assign(paginationDepth, (a, v) => a.PaginationDepth = v);

    /// <inheritdoc cref="IHybridQuery.Queries" />
    public HybridQueryDescriptor<T> Queries(params Func<QueryContainerDescriptor<T>, QueryContainer>[] queries) =>
        Assign(queries.Select(q => q?.Invoke(new QueryContainerDescriptor<T>())).ToListOrNullIfEmpty(), (a, v) => a.Queries = v);

    /// <inheritdoc cref="IHybridQuery.Queries" />
    public HybridQueryDescriptor<T> Queries(IEnumerable<Func<QueryContainerDescriptor<T>, QueryContainer>> queries) =>
        Assign(queries.Select(q => q?.Invoke(new QueryContainerDescriptor<T>())).ToListOrNullIfEmpty(), (a, v) => a.Queries = v);

    /// <inheritdoc cref="IHybridQuery.Queries" />
    public HybridQueryDescriptor<T> Queries(params QueryContainer[] queries) =>
        Assign(queries.ToListOrNullIfEmpty(), (a, v) => a.Queries = v);
}
