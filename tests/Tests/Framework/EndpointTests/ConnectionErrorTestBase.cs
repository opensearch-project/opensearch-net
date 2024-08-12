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
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.OpenSearch.Ephemeral;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Core.Extensions;
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Framework.EndpointTests.TestState;

namespace Tests.Framework.EndpointTests;

public abstract class ConnectionErrorTestBase<TCluster>
    : RequestResponseApiTestBase<TCluster, RootNodeInfoResponse, IRootNodeInfoRequest, RootNodeInfoDescriptor, RootNodeInfoRequest>
    where TCluster : IEphemeralCluster<EphemeralClusterConfiguration>, IOpenSearchClientTestCluster, new()
{
    protected ConnectionErrorTestBase(TCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

    public override IOpenSearchClient Client => Cluster.Client;

    protected override object ExpectJson { get; } = null;
    protected override RootNodeInfoRequest Initializer => new RootNodeInfoRequest();

    protected override LazyResponses ClientUsage() => Calls(
        (client, f) => client.RootNodeInfo(f),
        (client, f) => client.RootNodeInfoAsync(f),
        (client, r) => client.RootNodeInfo(r),
        (client, r) => client.RootNodeInfoAsync(r)
    );

    [I] public async Task IsValidIsFalse() => await AssertOnAllResponses(r => r.ShouldHaveExpectedIsValid(false));

    [I]
    public async Task AssertException() => await AssertOnAllResponses(r =>
    {
        var e = r.OriginalException;
        e.Should().NotBeNull();
        FindWebExceptionOrHttpRequestException(e, e);
    });

    private void FindWebExceptionOrHttpRequestException(Exception mainException, Exception currentException)
    {
        mainException.Should().NotBeNull();
        currentException.Should().NotBeNull();
        if (currentException is WebException exception) AssertWebException(exception);
        else if (currentException is HttpRequestException requestException) AssertHttpRequestException(requestException);
        else if (currentException.InnerException != null)
            FindWebExceptionOrHttpRequestException(mainException, currentException.InnerException);
        else
            throw new Exception("Unable to find WebException or HttpRequestException on" + mainException.GetType().FullName);
    }

    protected abstract void AssertWebException(WebException e);

    protected abstract void AssertHttpRequestException(HttpRequestException e);
}
