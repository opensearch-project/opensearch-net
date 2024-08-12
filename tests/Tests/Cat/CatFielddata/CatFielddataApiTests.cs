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
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.Net;
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Core.Xunit;
using Tests.Domain;
using Tests.Framework.EndpointTests;
using Tests.Framework.EndpointTests.TestState;

namespace Tests.Cat.CatFielddata;

public class CatFielddataApiTests
    : ApiIntegrationTestBase<ReadOnlyCluster, CatResponse<CatFielddataRecord>, ICatFielddataRequest, CatFielddataDescriptor, CatFielddataRequest>
{
    private ISearchResponse<Project> _initialSearchResponse;

    public CatFielddataApiTests(ReadOnlyCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

    protected override bool ExpectIsValid => true;
    protected override int ExpectStatusCode => 200;
    protected override HttpMethod HttpMethod => HttpMethod.GET;
    protected override string UrlPath => "/_cat/fielddata";

    protected override LazyResponses ClientUsage() => Calls(
        (client, f) => client.Cat.Fielddata(),
        (client, f) => client.Cat.FielddataAsync(),
        (client, r) => client.Cat.Fielddata(r),
        (client, r) => client.Cat.FielddataAsync(r)
    );

    protected override void IntegrationSetup(IOpenSearchClient client, CallUniqueValues values)
    {
        // ensure some fielddata is loaded
        _initialSearchResponse = client.Search<Project>(s => s
            .Query(q => q
                .Terms(t => t
                    .Field(p => p.CuratedTags.First().Name)
                    .Terms(Project.Projects.SelectMany(p => p.CuratedTags).Take(50).ToList())
                )
            )
        );

        if (!_initialSearchResponse.IsValid)
            throw new Exception($"Failure setting up integration test. {_initialSearchResponse.DebugInformation}");
    }

    protected override void ExpectResponse(CatResponse<CatFielddataRecord> response)
    {
        //this tests is very flaky, only do assertions if the query actually returned
        // TODO investigate flakiness
        // build seed:64178 integrate 1.0.0 "readonly" "catfielddata"
        // fails on TeamCity but not locally, assuming the different PC sizes come into play
        if (SkipOnCiAttribute.RunningOnTeamCity || _initialSearchResponse == null || _initialSearchResponse.Total <= 0)
            return;

        response.Records.Should().NotBeEmpty();
        foreach (var record in response.Records)
        {
            record.Node.Should().NotBeNullOrEmpty();
            record.Id.Should().NotBeNullOrEmpty();
            record.Host.Should().NotBeNullOrEmpty();
            record.Ip.Should().NotBeNullOrEmpty();
            record.Field.Should().NotBeNullOrEmpty();
            record.Size.Should().NotBeNullOrEmpty();
        }
    }
}
