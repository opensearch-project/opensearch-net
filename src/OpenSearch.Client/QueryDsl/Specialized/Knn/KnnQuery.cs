/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
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

    /// <inheritdoc cref="IKnnQuery.Vector" />
    public KnnQueryDescriptor<T> Vector(params float[] vector) => Assign(vector, (a, v) => a.Vector = v);

    /// <inheritdoc cref="IKnnQuery.K" />
    public KnnQueryDescriptor<T> K(int? k) => Assign(k, (a, v) => a.K = v);

    /// <inheritdoc cref="IKnnQuery.Filter" />
    public KnnQueryDescriptor<T> Filter(Func<QueryContainerDescriptor<T>, QueryContainer> filterSelector) =>
        Assign(filterSelector, (a, v) => a.Filter = v?.Invoke(new QueryContainerDescriptor<T>()));
}
