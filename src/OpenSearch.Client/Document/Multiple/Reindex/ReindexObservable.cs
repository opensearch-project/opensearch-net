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
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using OpenSearch.Client;
using OpenSearch.Net;
using static OpenSearch.Client.Infer;

namespace OpenSearch.Client;

public class ReindexObservable<TSource> : ReindexObservable<TSource, TSource>
    where TSource : class
{
    public ReindexObservable(IOpenSearchClient client, IConnectionSettingsValues connectionSettings,
        IReindexRequest<TSource, TSource> reindexRequest, CancellationToken cancellationToken
    )
        : base(client, connectionSettings, reindexRequest, cancellationToken) { }
}

public class ReindexObservable<TSource, TTarget> : IDisposable, IObservable<BulkAllResponse>
    where TSource : class
    where TTarget : class
{
    private readonly IOpenSearchClient _client;
    private readonly CancellationToken _compositeCancelToken;
    private readonly CancellationTokenSource _compositeCancelTokenSource;
    private readonly IConnectionSettingsValues _connectionSettings;
    private readonly IReindexRequest<TSource, TTarget> _reindexRequest;

    private Action<long> _incrementSeenDocuments = l => { };
    private Action _incrementSeenScrollOperations = () => { };

    public ReindexObservable(
        IOpenSearchClient client,
        IConnectionSettingsValues connectionSettings,
        IReindexRequest<TSource, TTarget> reindexRequest,
        CancellationToken cancellationToken
    )
    {
        _connectionSettings = connectionSettings;
        _reindexRequest = reindexRequest;

        _client = client;
        _compositeCancelTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _compositeCancelToken = _compositeCancelTokenSource.Token;
    }

    public bool IsDisposed { get; private set; }

    public void Dispose()
    {
        IsDisposed = true;
        _compositeCancelTokenSource?.Cancel();
    }

    public IDisposable Subscribe(IObserver<BulkAllResponse> observer)
    {
        observer.ThrowIfNull(nameof(observer));
        try
        {
            Reindex(observer);
        }
        catch (Exception e)
        {
            observer.OnError(e);
        }
        return this;
    }

    public IDisposable Subscribe(ReindexObserver observer)
    {
        _incrementSeenDocuments = observer.IncrementSeenScrollDocuments;
        _incrementSeenScrollOperations = observer.IncrementSeenScrollOperations;
        return Subscribe((IObserver<BulkAllResponse>)observer);
    }

    private void Reindex(IObserver<BulkAllResponse> observer)
    {
        var bulkMeta = _reindexRequest.BulkAll?.Invoke(Enumerable.Empty<IHitMetadata<TTarget>>());
        var scrollAll = _reindexRequest.ScrollAll;
        var toIndex = bulkMeta?.Index.Resolve(_connectionSettings);

        var slices = CreateIndex(toIndex, scrollAll);

        var backPressure = CreateBackPressure(bulkMeta, scrollAll, slices);

        var scrollDocuments = ScrollAll(slices, backPressure)
            .SelectMany(r => r.SearchResponse.Hits)
            .Select(r => r.Copy(_reindexRequest.Map));

        var observableBulk = BulkAll(scrollDocuments, backPressure, toIndex);

        //by casting the observable can potentially store more meta data on the user provided observer
        if (observer is BulkAllObserver moreInfoObserver)
            observableBulk.Subscribe(moreInfoObserver);
        else
            observableBulk.Subscribe(observer);
    }

    private BulkAllObservable<IHitMetadata<TTarget>> BulkAll(IEnumerable<IHitMetadata<TTarget>> scrollDocuments,
        ProducerConsumerBackPressure backPressure, string toIndex
    )
    {
        var bulkAllRequest = (_reindexRequest.BulkAll?.Invoke(scrollDocuments)) ?? throw new Exception("BulkAll must set on ReindexRequest in order to get the target of a Reindex operation");

        bulkAllRequest.BackPressure = backPressure;
        bulkAllRequest.BufferToBulk = (bulk, hits) =>
        {
            foreach (var hit in hits)
            {
                var item = new BulkIndexOperation<TTarget>(hit.Source)
                {
                    Index = toIndex,
                    Id = hit.Id,
                    Routing = hit.Routing,
                };
                bulk.AddOperation(item);
            }
        };

        // Mark the parent for the bulk all request as this reindex helper
        if (bulkAllRequest is IHelperCallable helperCallable)
        {
            var parentMetaData = RequestMetaDataFactory.ReindexHelperRequestMetaData();
            helperCallable.ParentMetaData = parentMetaData;
        }

        var observableBulk = _client.BulkAll(bulkAllRequest, _compositeCancelToken);
        return observableBulk;
    }

    private ProducerConsumerBackPressure CreateBackPressure(IBulkAllRequest<IHitMetadata<TTarget>> bulkMeta, IScrollAllRequest scrollAll,
        int slices
    )
    {
        var backPressureFactor = _reindexRequest.BackPressureFactor ?? CoordinatedRequestDefaults.ReindexBackPressureFactor;
        var maxConcurrentConsumers = bulkMeta?.MaxDegreeOfParallelism ?? CoordinatedRequestDefaults.BulkAllMaxDegreeOfParallelismDefault;
        var maxConcurrentProducers = scrollAll?.MaxDegreeOfParallelism ?? slices;
        var maxConcurrency = Math.Min(maxConcurrentConsumers, maxConcurrentProducers);

        var bulkSize = bulkMeta?.Size ?? CoordinatedRequestDefaults.BulkAllSizeDefault;
        var searchSize = scrollAll?.Search?.Size ?? 10;
        var producerBandwidth = searchSize * maxConcurrency * backPressureFactor;
        var funnelTooSmall = producerBandwidth < bulkSize;
        if (funnelTooSmall)
            throw new Exception("The back pressure settings are too conservative in providing enough documents for a single bulk operation. "
                + $"searchSize:{searchSize} * maxConcurrency:{maxConcurrency} * backPressureFactor:{backPressureFactor} = {producerBandwidth}"
                + $" which is smaller then the bulkSize:{bulkSize}."
            );

        var funnelExact = producerBandwidth == bulkSize;
        if (funnelExact)
            throw new Exception(
                "The back pressure settings are too conservative. They provide enough documents for a single bulk but not enough room to advance "
                + $"searchSize:{searchSize} * maxConcurrency:{maxConcurrency} * backPressureFactor:{backPressureFactor} = {producerBandwidth}"
                + $" which is exactly the bulkSize:{bulkSize}. Increase the BulkAll max concurrency or the backPressureFactor"
            );

        var backPressure = new ProducerConsumerBackPressure(backPressureFactor, maxConcurrency);
        return backPressure;
    }

    private IEnumerable<IScrollAllResponse<TSource>> ScrollAll(int slices, ProducerConsumerBackPressure backPressure)
    {
        var scrollAll = _reindexRequest.ScrollAll;
        var scroll = _reindexRequest.ScrollAll?.ScrollTime ?? TimeSpan.FromMinutes(2);

        var scrollAllRequest = new ScrollAllRequest(scroll, slices)
        {
            RoutingField = scrollAll.RoutingField,
            MaxDegreeOfParallelism = scrollAll.MaxDegreeOfParallelism ?? slices,
            Search = scrollAll.Search,
            BackPressure = backPressure,
            ParentMetaData = RequestMetaDataFactory.ReindexHelperRequestMetaData()
        };

        var scrollObservable = _client.ScrollAll<TSource>(scrollAllRequest, _compositeCancelToken);
        return new GetEnumerator<IScrollAllResponse<TSource>>()
            .ToEnumerable(scrollObservable)
            .Select(ObserveScrollAllResponse);
    }

    private IScrollAllResponse<TSource> ObserveScrollAllResponse(IScrollAllResponse<TSource> response)
    {
        _incrementSeenScrollOperations();
        _incrementSeenDocuments(response.SearchResponse.Hits.Count);
        return response;
    }

    private static OpenSearchClientException Throw(string message, IApiCallDetails details) =>
        new OpenSearchClientException(PipelineFailure.BadResponse, message, details);

    private int CreateIndex(string toIndex, IScrollAllRequest scrollAll)
    {
        var fromIndices = _reindexRequest.ScrollAll?.Search?.Index ?? Indices<TSource>();

        if (string.IsNullOrEmpty(toIndex))
            throw new Exception($"Could not resolve the target index name to reindex to make sure the bulk all operation describes one");

        var numberOfShards = CreateIndexIfNeeded(fromIndices, toIndex);

        var slices = scrollAll?.Slices ?? numberOfShards;
        if (scrollAll?.Slices < 0 && numberOfShards.HasValue)
            slices = numberOfShards;
        else if (scrollAll?.Slices < 0)
            throw new Exception("Slices is a negative number and no sane default could be inferred from the origin index's number_of_shards");

        if (!slices.HasValue)
            throw new Exception("Slices is not specified and could not be inferred from the number of "
                + "shards hint from the source. This could happen if the scroll all points to multiple indices and no slices have been set");

        return slices.Value;
    }

    /// <summary>
    /// Tries to create the target index if it does not exist already, if the source points to a single index we'll
    /// use the original index settings to bootstrap the create index request if not provided
    /// </summary>
    /// <returns>Either the number of shards from to source or the target as a slice hint to ScrollAll</returns>
    private int? CreateIndexIfNeeded(Indices fromIndices, string resolvedTo)
    {
        var requestMetaData = RequestMetaDataFactory.ReindexHelperRequestMetaData();

        if (_reindexRequest.OmitIndexCreation)
            return null;

        var pointsToSingleSourceIndex = fromIndices.Match((a) => false, (m) => m.Indices.Count == 1);
        var targetExistsAlready = _client.Indices.Exists(resolvedTo, e => e.RequestConfiguration(rc => rc.RequestMetaData(requestMetaData)));
        if (targetExistsAlready.Exists)
            return null;

        _compositeCancelToken.ThrowIfCancellationRequested();
        IndexState originalIndexState = null;
        var resolvedFrom = fromIndices.Resolve(_connectionSettings);

        if (pointsToSingleSourceIndex)
        {
            var getIndexResponse = _client.Indices.Get(resolvedFrom, i => i.RequestConfiguration(rc => rc.RequestMetaData(requestMetaData)));
            _compositeCancelToken.ThrowIfCancellationRequested();
            originalIndexState = getIndexResponse.Indices[resolvedFrom];
            if (_reindexRequest.OmitIndexCreation)
                return originalIndexState.Settings.NumberOfShards;
        }

        var createIndexRequest = _reindexRequest.CreateIndexRequest ??
            (originalIndexState != null
                ? new CreateIndexRequest(resolvedTo, originalIndexState)
                : new CreateIndexRequest(resolvedTo));

        createIndexRequest.RequestParameters.SetRequestMetaData(requestMetaData);

        var createIndexResponse = _client.Indices.Create(createIndexRequest);
        _compositeCancelToken.ThrowIfCancellationRequested();
        if (!createIndexResponse.IsValid)
            throw Throw($"Could not create destination index {resolvedTo}.", createIndexResponse.ApiCall);

        return createIndexRequest.Settings?.NumberOfShards;
    }
}
