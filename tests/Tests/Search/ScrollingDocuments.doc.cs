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
using System.Runtime.ExceptionServices;
using System.Threading;
using OpenSearch.Client;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Domain;
using Tests.Framework.DocumentationTests;

namespace Tests.Search;

/**=== Scrolling documents
	 *
	 * The scroll API can be used to return a large collection of documents from OpenSearch.
	 *
	 * OSC exposes the scroll API and an observable scroll implementation that can be used
	 * to write concurrent scroll requests.
	 */
public class ScrollDocuments : IntegrationDocumentationTestBase, IClusterFixture<ReadOnlyCluster>
{
    public ScrollDocuments(ReadOnlyCluster cluster) : base(cluster) { }

    // hide
    private void ProcessResponse(ISearchResponse<Project> response) { }

    /**==== Simple use
		 *
		 * The simplest use of the scroll API is to perform a search request with a
		 * scroll timeout, then pass the scroll id returned in each response to
		 * the next request to the scroll API, until no more documents are returned
		 */
    [I]
    public void SimpleUse()
    {
        var searchResponse = Client.Search<Project>(s => s
            .Query(q => q
                .Term(f => f.State, StateOfBeing.Stable)
            )
            .Scroll("10s") // <1> Specify a scroll time for how long OpenSearch should keep this scroll open on the server side. The time specified should be sufficient to process the response on the client side.
        );

        while (searchResponse.Documents.Any()) // <2> make subsequent requests to the scroll API to keep fetching documents, whilst documents are returned
        {
            ProcessResponse(searchResponse); // <3> do something with the response
            searchResponse = Client.Scroll<Project>("10s", searchResponse.ScrollId);
        }
    }

    /**[[scrollall-observable]]
		 * ==== ScrollAllObservable
		 *
		 * Similar to <<bulkall-observable, `BulkAllObservable`>> for bulk indexing a large number of documents,
		 * OSC exposes an observable scroll implementation, `ScrollAllObservable`, that can be used
		 * to write concurrent scroll requests. `ScrollAllObservable` uses sliced scrolls to split the scroll into
		 * multiple slices that can be consumed concurrently.
		 *
		 * The simplest use of `ScrollAllObservable` is
		 */
    [I]
    public void SimpleScrollAllObservable()
    {
        var numberOfSlices = Environment.ProcessorCount; // <1> See documentation for choosing an appropriate number of slices.

        var scrollAllObservable = Client.ScrollAll<Project>("10s", numberOfSlices, sc => sc
            .MaxDegreeOfParallelism(numberOfSlices) // <2> Number of concurrent sliced scroll requests. Usually want to set this to the same value as the number of slices
            .Search(s => s
                .Query(q => q
                    .Term(f => f.State, StateOfBeing.Stable)
                )
            )
        );

        scrollAllObservable.Wait(TimeSpan.FromMinutes(10), response => // <3> Total overall time for scrolling **all** documents. Ensure this is a sufficient value to scroll all documents
        {
            ProcessResponse(response.SearchResponse); // <4> do something with the response
        });
    }

    /**
		 * More control over how the observable is consumed can be achieved by writing
		 * your own observer and subscribing to the observable, which will initiate scrolling
		 */
    [I]
    public void ComplexScrollAllObservable()
    {
        var numberOfSlices = Environment.ProcessorCount;

        var scrollAllObservable = Client.ScrollAll<Project>("10s", numberOfSlices, sc => sc
            .MaxDegreeOfParallelism(numberOfSlices)
            .Search(s => s
                .Query(q => q
                    .Term(f => f.State, StateOfBeing.Stable)
                )
            )
        );

        var waitHandle = new ManualResetEvent(false);
        ExceptionDispatchInfo info = null;

        var scrollAllObserver = new ScrollAllObserver<Project>(
            onNext: response => ProcessResponse(response.SearchResponse), // <1> do something with the response
            onError: e =>
            {
                info = ExceptionDispatchInfo.Capture(e); // <2> if an exception is thrown, capture it to throw outside of the observer
                waitHandle.Set();
            },
            onCompleted: () => waitHandle.Set()
        );

        scrollAllObservable.Subscribe(scrollAllObserver); // <3> initiate scrolling

        waitHandle.WaitOne(); // <4> block the current thread until the wait handle is set
        info?.Throw(); // <5> if an exception was captured whilst scrolling, throw it
    }
}
