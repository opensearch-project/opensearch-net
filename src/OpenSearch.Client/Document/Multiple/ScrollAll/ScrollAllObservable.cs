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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OpenSearch.Net;

namespace OpenSearch.Client;

public class ScrollAllObservable<T> : IDisposable, IObservable<ScrollAllResponse<T>> where T : class
{
    private readonly ProducerConsumerBackPressure _backPressure;
    private readonly IOpenSearchClient _client;

    private readonly CancellationToken _compositeCancelToken;
    private readonly CancellationTokenSource _compositeCancelTokenSource;
    private readonly IScrollAllRequest _scrollAllRequest;

    //since we modify the passed searchrequest during the setup phase we use a simple
    //semaphore async await to make sure we do not mutate over multiple request during the initial
    //sliced scroll setup
    private readonly SemaphoreSlim _scrollInitiationLock = new SemaphoreSlim(1, 1);
    private readonly ISearchRequest _searchRequest;

    public ScrollAllObservable(IOpenSearchClient client, IScrollAllRequest scrollAllRequest, CancellationToken cancellationToken = default)
    {
        _scrollAllRequest = scrollAllRequest;
        _searchRequest = scrollAllRequest?.Search ?? new SearchRequest<T>();

        switch (_scrollAllRequest)
        {
            case IHelperCallable helperCallable when helperCallable.ParentMetaData is object:
                _searchRequest.RequestParameters.SetRequestMetaData(helperCallable.ParentMetaData);
                break;
            default:
                _searchRequest.RequestParameters.SetRequestMetaData(RequestMetaDataFactory.ScrollHelperRequestMetaData());
                break;
        }

        if (_searchRequest.Sort == null)
            _searchRequest.Sort = FieldSort.ByDocumentOrder;
        _searchRequest.RequestParameters.Scroll = _scrollAllRequest.ScrollTime.ToTimeSpan();
        _client = client;
        _compositeCancelTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _compositeCancelToken = _compositeCancelTokenSource.Token;
        _backPressure = _scrollAllRequest.BackPressure;
    }

    public bool IsDisposed { get; private set; }

    public void Dispose()
    {
        IsDisposed = true;
        _compositeCancelTokenSource?.Cancel();
    }

    public IDisposable Subscribe(IObserver<ScrollAllResponse<T>> observer)
    {
        observer.ThrowIfNull(nameof(observer));
        ScrollAll(observer);
        return this;
    }

    private void ScrollAll(IObserver<ScrollAllResponse<T>> observer)
    {
        var slices = _scrollAllRequest.Slices;
        var maxSlicesAtOnce = _scrollAllRequest.MaxDegreeOfParallelism ?? _scrollAllRequest.Slices;

#pragma warning disable 4014
        Enumerable.Range(0, slices)
            .ForEachAsync(
#pragma warning restore 4014
                (slice, l) => ScrollSliceAsync(observer, slice),
                (slice, r) => { },
                t => OnCompleted(t, observer),
                maxSlicesAtOnce
            );
    }

    private async Task<bool> ScrollSliceAsync(IObserver<ScrollAllResponse<T>> observer, int slice)
    {
        var searchResult = await InitiateSearchAsync(slice).ConfigureAwait(false);
        await ScrollToCompletionAsync(slice, observer, searchResult).ConfigureAwait(false);
        return true;
    }

    private static OpenSearchClientException Throw(string message, IApiCallDetails details) =>
        new OpenSearchClientException(PipelineFailure.BadResponse, message, details);

    private void ThrowOnBadSearchResult(ISearchResponse<T> result, int slice, int page)
    {
        if (result == null || !result.IsValid)
        {
            var path = result?.ApiCall.Uri.PathAndQuery ?? "(unknown)";
            throw Throw($"scrolling search on {path} with slice {slice} was not valid on scroll iteration {page}", result?.ApiCall);
        }
        _compositeCancelToken.ThrowIfCancellationRequested();
    }

    private async Task ScrollToCompletionAsync(int slice, IObserver<ScrollAllResponse<T>> observer, ISearchResponse<T> searchResult)
    {
        var page = 0;
        ThrowOnBadSearchResult(searchResult, slice, page);
        var scroll = _scrollAllRequest.ScrollTime;
        while (searchResult.IsValid && searchResult.Documents.HasAny())
        {
            if (_backPressure != null)
                await _backPressure.WaitAsync(_compositeCancelToken).ConfigureAwait(false);

            observer.OnNext(new ScrollAllResponse<T>()
            {
                Slice = slice,
                SearchResponse = searchResult,
                Scroll = page
            });
            page++;
            var request = new ScrollRequest(searchResult.ScrollId, scroll);

            if (request.RequestConfiguration is null)
                request.RequestConfiguration = new RequestConfiguration();

            switch (_scrollAllRequest)
            {
                case IHelperCallable helperCallable when helperCallable.ParentMetaData is object:
                    request.RequestConfiguration.SetRequestMetaData(helperCallable.ParentMetaData);
                    break;
                default:
                    request.RequestConfiguration.SetRequestMetaData(RequestMetaDataFactory.ScrollHelperRequestMetaData());
                    break;
            }

            searchResult = await _client.ScrollAsync<T>(request, _compositeCancelToken).ConfigureAwait(false);
            ThrowOnBadSearchResult(searchResult, slice, page);
        }
    }

    private async Task<ISearchResponse<T>> InitiateSearchAsync(int slice)
    {
        //since we are mutating the searchRequests .Slice it can not be shared across threads for the initial searches
        //so these need to happen in a serial fashion
        await _scrollInitiationLock.WaitAsync(_compositeCancelToken).ConfigureAwait(false);
        try
        {
            _searchRequest.Slice = new SlicedScroll
            {
                Id = slice,
                Max = _scrollAllRequest.Slices,
                Field = _scrollAllRequest.RoutingField
            };
            var response = await _client.SearchAsync<T>(_searchRequest, _compositeCancelToken).ConfigureAwait(false);
            //response gets passed to ScrollToCompletionAsync which does validation already
            return response;
        }
        finally
        {
            _scrollInitiationLock.Release();
        }
    }

    private static void OnCompleted(Exception exception, IObserver<ScrollAllResponse<T>> observer)
    {
        if (exception == null)
            observer.OnCompleted();
        else
            observer.OnError(exception);
    }
}
