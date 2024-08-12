/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/
/*
* Modifications Copyright OpenSearch Contributors. See
* GitHub history for details.
*
*  Licensed to Elasticsearch B.V. under one or more contributor
*  license agreements. See the NOTICE file distributed with
*  this work for additional information regarding copyright
*  ownership. Elasticsearch B.V. licenses this file to you under
*  the Apache License, Version 2.0 (the "License"); you may
*  not use this file except in compliance with the License.
*  You may obtain a copy of the License at
*
* 	http://www.apache.org/licenses/LICENSE-2.0
*
*  Unless required by applicable law or agreed to in writing,
*  software distributed under the License is distributed on an
*  "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
*  KIND, either express or implied.  See the License for the
*  specific language governing permissions and limitations
*  under the License.
*/

using System;
using System.Threading;

namespace OpenSearch.Client;

public partial interface IOpenSearchClient
{
    /// <summary>
    /// Helper method that allows you to reindex from one index into another using ScrollAll and BulkAll.
    /// </summary>
    /// <returns>An IObservable&lt;ReindexResponse&lt;T&gt;$gt; you can subscribe to to listen to the progress of the reindex process</returns>
    IObservable<BulkAllResponse> Reindex<TSource, TTarget>(
        Func<TSource, TTarget> mapper,
        Func<ReindexDescriptor<TSource, TTarget>, IReindexRequest<TSource, TTarget>> selector,
        CancellationToken cancellationToken = default(CancellationToken)
    )
        where TSource : class
        where TTarget : class;

    /// <summary>
    /// Helper method that allows you to reindex from one index into another using ScrollAll and BulkAll.
    /// </summary>
    /// <returns>An IObservable&lt;ReindexResponse&lt;T&gt;$gt; you can subscribe to to listen to the progress of the reindex process</returns>
    IObservable<BulkAllResponse> Reindex<TSource>(
        Func<ReindexDescriptor<TSource, TSource>, IReindexRequest<TSource, TSource>> selector,
        CancellationToken cancellationToken = default(CancellationToken)
    )
        where TSource : class;

    /// <summary>
    /// Helper method that allows you to reindex from one index into another using ScrollAll and BulkAll.
    /// </summary>
    /// <param name="request">a request object to describe the reindex operation</param>
    /// <returns>An IObservable&lt;ReindexResponse&lt;T&gt;$gt; you can subscribe to to listen to the progress of the reindex process</returns>
    IObservable<BulkAllResponse> Reindex<TSource, TTarget>(
        IReindexRequest<TSource, TTarget> request,
        CancellationToken cancellationToken = default(CancellationToken)
    )
        where TSource : class
        where TTarget : class;

    /// <summary>
    /// Helper method that allows you to reindex from one index into another using ScrollAll and BulkAll.
    /// </summary>
    /// <param name="request">a request object to describe the reindex operation</param>
    /// <returns>An IObservable&lt;ReindexResponse&lt;T&gt;$gt; you can subscribe to to listen to the progress of the reindex process</returns>
    IObservable<BulkAllResponse> Reindex<TSource>(
        IReindexRequest<TSource> request,
        CancellationToken cancellationToken = default(CancellationToken)
    )
        where TSource : class;

    /// <summary>
    /// Simplified form for reindex which will cover 80% of its usecases. Allows you to index all documents of type T from
    /// <paramref name="fromIndex" /> to <paramref name="toIndex" />
    /// optionally limiting the documents found in <paramref name="fromIndex" /> by using <paramref name="selector" />.
    /// </summary>
    /// <param name="fromIndex">The source index, from which all types will be returned</param>
    /// <param name="toIndex">
    /// The target index, if it does not exist already will be created using the same settings of
    /// <paramref name="fromIndex" />
    /// </param>
    /// <param name="selector">an optional query limiting the documents found in <paramref name="fromIndex" /></param>
    IObservable<BulkAllResponse> Reindex<TSource, TTarget>(
        IndexName fromIndex,
        IndexName toIndex,
        Func<TSource, TTarget> mapper,
        Func<QueryContainerDescriptor<TSource>, QueryContainer> selector = null,
        CancellationToken cancellationToken = default(CancellationToken)
    )
        where TSource : class
        where TTarget : class;

    /// <summary>
    /// Simplified form for reindex which will cover 80% of its use cases. Allows you to index all documents of type T from
    /// <paramref name="fromIndex" /> to <paramref name="toIndex" />
    /// optionally limiting the documents found in <paramref name="fromIndex" /> by using <paramref name="selector" />.
    /// </summary>
    /// <param name="fromIndex">The source index, from which all types will be returned</param>
    /// <param name="toIndex">
    /// The target index, if it does not exist already will be created using the same settings of
    /// <paramref name="fromIndex" />
    /// </param>
    /// <param name="selector">an optional query limiting the documents found in <paramref name="fromIndex" /></param>
    IObservable<BulkAllResponse> Reindex<TSource>(
        IndexName fromIndex,
        IndexName toIndex,
        Func<QueryContainerDescriptor<TSource>, QueryContainer> selector = null,
        CancellationToken cancellationToken = default(CancellationToken)
    )
        where TSource : class;
}

public partial class OpenSearchClient
{
    /// <inheritdoc />
    public IObservable<BulkAllResponse> Reindex<TSource>(
        Func<ReindexDescriptor<TSource, TSource>, IReindexRequest<TSource, TSource>> selector,
        CancellationToken cancellationToken = default(CancellationToken)
    )
        where TSource : class =>
        Reindex(selector.InvokeOrDefault(new ReindexDescriptor<TSource, TSource>(s => s)));

    /// <inheritdoc />
    public IObservable<BulkAllResponse> Reindex<TSource, TTarget>(
        Func<TSource, TTarget> mapper,
        Func<ReindexDescriptor<TSource, TTarget>, IReindexRequest<TSource, TTarget>> selector,
        CancellationToken cancellationToken = default(CancellationToken)
    )
        where TTarget : class
        where TSource : class =>
        Reindex(selector.InvokeOrDefault(new ReindexDescriptor<TSource, TTarget>(mapper)));

    /// <inheritdoc />
    public IObservable<BulkAllResponse> Reindex<TSource>(
        IReindexRequest<TSource> request,
        CancellationToken cancellationToken = default(CancellationToken)
    )
        where TSource : class =>
        Reindex<TSource, TSource>(request, cancellationToken);


    /// <inheritdoc />
    public IObservable<BulkAllResponse> Reindex<TSource, TTarget>(
        IReindexRequest<TSource, TTarget> request,
        CancellationToken cancellationToken = default(CancellationToken)
    )
        where TTarget : class
        where TSource : class
    {
        if (request.ScrollAll == null)
            throw new ArgumentException(
                "ScrollAll property must be set in order to get the source of a Reindex operation");
        if (request.BulkAll == null)
            throw new ArgumentException(
                "BulkAll property must set in order to get the target of a Reindex operation");

        return new ReindexObservable<TSource, TTarget>(this, ConnectionSettings, request, cancellationToken);
    }

    /// <inheritdoc />
    public IObservable<BulkAllResponse> Reindex<TSource, TTarget>(
        IndexName fromIndex,
        IndexName toIndex,
        Func<TSource, TTarget> mapper,
        Func<QueryContainerDescriptor<TSource>, QueryContainer> selector = null,
        CancellationToken cancellationToken = default(CancellationToken)
    )
        where TTarget : class
        where TSource : class =>
        Reindex(
            mapper,
            SimplifiedReindexer<TSource, TTarget>(fromIndex, toIndex, selector)
            , cancellationToken);

    /// <inheritdoc />
    public IObservable<BulkAllResponse> Reindex<TSource>(
        IndexName fromIndex,
        IndexName toIndex,
        Func<QueryContainerDescriptor<TSource>, QueryContainer> selector = null,
        CancellationToken cancellationToken = default(CancellationToken)
    )
        where TSource : class =>
        Reindex(
            s => s,
            SimplifiedReindexer<TSource, TSource>(fromIndex, toIndex, selector)
            , cancellationToken);

    private static Func<ReindexDescriptor<TSource, TTarget>, IReindexRequest<TSource, TTarget>> SimplifiedReindexer<TSource, TTarget>(
        IndexName fromIndex,
        IndexName toIndex,
        Func<QueryContainerDescriptor<TSource>, QueryContainer> selector
    )
        where TTarget : class
        where TSource : class => r => r
        .ScrollAll("1m", -1, search => search
            .Search(ss => ss
                .Size(CoordinatedRequestDefaults.ReindexScrollSize)
                .Index(fromIndex)
                .Query(selector)
            )
        )
        .BulkAll(b => b.Index(toIndex));
}
