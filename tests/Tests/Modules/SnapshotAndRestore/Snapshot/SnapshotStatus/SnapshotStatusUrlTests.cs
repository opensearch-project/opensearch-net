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

using System.Threading.Tasks;
using OpenSearch.Client;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Framework.EndpointTests;
using static Tests.Framework.EndpointTests.UrlTester;

namespace Tests.Modules.SnapshotAndRestore.Snapshot.SnapshotStatus;

public class SnapshotStatusUrlTests
{
    [U]
    public async Task Urls()
    {
        await GET("/_snapshot/_status")
                .Fluent(c => c.Snapshot.Status())
                .Request(c => c.Snapshot.Status(new SnapshotStatusRequest()))
                .FluentAsync(c => c.Snapshot.StatusAsync())
                .RequestAsync(c => c.Snapshot.StatusAsync(new SnapshotStatusRequest()))
            ;

        var repository = "repos";
        await GET($"/_snapshot/{repository}/_status")
                .Fluent(c => c.Snapshot.Status(s => s.RepositoryName(repository)))
                .Request(c => c.Snapshot.Status(new SnapshotStatusRequest(repository)))
                .FluentAsync(c => c.Snapshot.StatusAsync(s => s.RepositoryName(repository)))
                .RequestAsync(c => c.Snapshot.StatusAsync(new SnapshotStatusRequest(repository)))
            ;
        var snapshot = "snap";
        await GET($"/_snapshot/{repository}/{snapshot}/_status")
                .Fluent(c => c.Snapshot.Status(s => s.RepositoryName(repository).Snapshot(snapshot)))
                .Request(c => c.Snapshot.Status(new SnapshotStatusRequest(repository, snapshot)))
                .FluentAsync(c => c.Snapshot.StatusAsync(s => s.RepositoryName(repository).Snapshot(snapshot)))
                .RequestAsync(c => c.Snapshot.StatusAsync(new SnapshotStatusRequest(repository, snapshot)))
            ;
    }
}
