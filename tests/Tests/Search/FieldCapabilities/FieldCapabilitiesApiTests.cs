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
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Core.Extensions;
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Domain;
using Tests.Framework.EndpointTests;
using Tests.Framework.EndpointTests.TestState;
using static OpenSearch.Client.Infer;

namespace Tests.Search.FieldCapabilities;

public class FieldCapabilitiesApiTests
    : ApiIntegrationTestBase<ReadOnlyCluster, FieldCapabilitiesResponse, IFieldCapabilitiesRequest, FieldCapabilitiesDescriptor,
        FieldCapabilitiesRequest>
{
    public FieldCapabilitiesApiTests(ReadOnlyCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

    protected override bool ExpectIsValid => true;

    protected override int ExpectStatusCode => 200;

    protected override Func<FieldCapabilitiesDescriptor, IFieldCapabilitiesRequest> Fluent => d => d
        .Fields(Fields<Project>(p => p.Name).And("*"));

    protected override HttpMethod HttpMethod => HttpMethod.GET;

    protected override FieldCapabilitiesRequest Initializer => new FieldCapabilitiesRequest(Index<Project>().And<Developer>())
    {
        Fields = Fields<Project>(p => p.Name).And("*"),
    };

    protected override string UrlPath => "/project%2Cdevs/_field_caps?fields=name%2C%2A";

    protected override LazyResponses ClientUsage() => Calls(
        (c, f) => c.FieldCapabilities(Index<Project>().And<Developer>(), f),
        (c, f) => c.FieldCapabilitiesAsync(Index<Project>().And<Developer>(), f),
        (c, r) => c.FieldCapabilities(r),
        (c, r) => c.FieldCapabilitiesAsync(r)
    );

    protected override void ExpectResponse(FieldCapabilitiesResponse response)
    {

        var sourceField = response.Fields.First(kv => kv.Value.Source != null).Value.Source;
        sourceField.Aggregatable.Should().BeFalse();
        sourceField.Searchable.Should().BeFalse();

        response.Fields.Should().ContainKey("_index");
        var indexField = response.Fields["_index"].Index;
        indexField.Should().NotBeNull();

        indexField.Aggregatable.Should().BeTrue();
        indexField.Searchable.Should().BeTrue();

        response.Fields.Should().ContainKey("jobTitle.keyword");
        var jobTitleCapabilities = response.Fields["jobTitle.keyword"].Keyword;
        jobTitleCapabilities.Aggregatable.Should().BeTrue();
        jobTitleCapabilities.Searchable.Should().BeTrue();

        jobTitleCapabilities = response.Fields[Field<Developer>(p => p.JobTitle.Suffix("keyword"))].Keyword;
        jobTitleCapabilities.Aggregatable.Should().BeTrue();
        jobTitleCapabilities.Searchable.Should().BeTrue();
    }
}

public class FieldCapabilitiesIndexFilterApiTests
    : ApiIntegrationTestBase<ReadOnlyCluster, FieldCapabilitiesResponse, IFieldCapabilitiesRequest, FieldCapabilitiesDescriptor,
        FieldCapabilitiesRequest>
{
    public FieldCapabilitiesIndexFilterApiTests(ReadOnlyCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

    protected override object ExpectJson => new
    {
        index_filter = new
        {
            term = new
            {
                versionControl = new
                {
                    value = "git"
                }
            }
        }
    };

    protected override bool SupportsDeserialization { get; } = false;

    protected override bool ExpectIsValid => true;

    protected override int ExpectStatusCode => 200;

    protected override Func<FieldCapabilitiesDescriptor, IFieldCapabilitiesRequest> Fluent => d => d
        .Fields("*")
        .IndexFilter<Project>(q => q
            .Term(t => t
                .Field(f => f.VersionControl)
                .Value(Project.VersionControlConstant)
            )
        );

    protected override HttpMethod HttpMethod => HttpMethod.POST;

    protected override FieldCapabilitiesRequest Initializer => new FieldCapabilitiesRequest(Index<Project>().And<Developer>())
    {
        Fields = "*",
        IndexFilter = new TermQuery
        {
            Field = Field<Project>(f => f.VersionControl),
            Value = Project.VersionControlConstant
        }
    };

    protected override string UrlPath => "/project%2Cdevs/_field_caps?fields=%2A";

    protected override LazyResponses ClientUsage() => Calls(
        (c, f) => c.FieldCapabilities(Index<Project>().And<Developer>(), f),
        (c, f) => c.FieldCapabilitiesAsync(Index<Project>().And<Developer>(), f),
        (c, r) => c.FieldCapabilities(r),
        (c, r) => c.FieldCapabilitiesAsync(r)
    );

    protected override void ExpectResponse(FieldCapabilitiesResponse response) => response.ShouldBeValid();
}
