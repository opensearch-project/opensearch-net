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
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.Net;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Core.Extensions;
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Domain;
using Tests.Framework.EndpointTests;
using Tests.Framework.EndpointTests.TestState;

namespace Tests.Indices.IndexManagement.RolloverIndex;

using OpenSearch.Client;

public class RolloverIndexApiTests
    : ApiIntegrationTestBase<WritableCluster, RolloverIndexResponse, IRolloverIndexRequest, RolloverIndexDescriptor, RolloverIndexRequest>
{
    public RolloverIndexApiTests(WritableCluster cluster, EndpointUsage usage)
        : base(cluster, usage) { }

    protected override bool ExpectIsValid => true;
    protected override int ExpectStatusCode => 200;
    protected override HttpMethod HttpMethod => HttpMethod.POST;
    protected override bool SupportsDeserialization => false;
    protected override string UrlPath => $"/{CallIsolatedValue}-alias/_rollover/{CallIsolatedValue}-new";

    protected override object ExpectJson => new
    {
        conditions = new
        {
            max_age = "7d",
            max_docs = 1000
        },
        settings = new Dictionary<string, object>
        {
            { "index.number_of_shards", 1 },
            { "index.number_of_replicas", 1 }
        },
        mappings = new
        {
            properties = new
            {
                branches = new
                {
                    type = "text",
                    fields = new
                    {
                        keyword = new
                        {
                            type = "keyword",
                            ignore_above = 256
                        }
                    }
                }
            }
        },
        aliases = new Dictionary<string, object>
        {
            { CallIsolatedValue + "-new_projects",  new { } }
        }
    };

    protected override Func<RolloverIndexDescriptor, IRolloverIndexRequest> Fluent => f => f
        .NewIndex(CallIsolatedValue + "-new")
        .Conditions(c => c
            .MaxAge("7d")
            .MaxDocs(1000)
        )
        .Settings(s => s
            .NumberOfShards(1)
            .NumberOfReplicas(1)
        )
        .Map<Project>(p => p
            .Properties(pp => pp
                .Text(t => t
                    .Name(n => n.Branches)
                    .Fields(pf => pf
                        .Keyword(k => k
                            .Name("keyword")
                            .IgnoreAbove(256)
                        )
                    )
                )
            )
        )
        .Aliases(a => a
            .Alias(CallIsolatedValue + "-new_projects")
        );

    protected override RolloverIndexRequest Initializer => new(CallIsolatedValue + "-alias", CallIsolatedValue + "-new")
    {
        Conditions = new RolloverConditions
        {
            MaxAge = "7d",
            MaxDocs = 1000
        },
        Settings = new IndexSettings
        {
            NumberOfShards = 1,
            NumberOfReplicas = 1
        },
        Mappings = new TypeMapping
        {
            Properties = new Properties<Project>
            {
                {
                    p => p.Branches, new TextProperty
                    {
                        Fields = new Properties
                        {
                            {
                                "keyword", new KeywordProperty
                                {
                                    IgnoreAbove = 256
                                }
                            }
                        }
                    }
                }
            }
        },
        Aliases = new Aliases
        {
            { CallIsolatedValue + "-new_projects", new Alias() }
        }
    };

    protected override void OnBeforeCall(IOpenSearchClient client)
    {
        var create = client.Indices.Create(CallIsolatedValue, c => c
            .Aliases(a => a
                .Alias(CallIsolatedValue + "-alias")
            )
        );
        create.ShouldBeValid();
        var someDocs = client.Bulk(b => b
            .Index(CallIsolatedValue)
            .Refresh(Refresh.True)
            .IndexMany(Project.Generator.Generate(1200))
        );
        someDocs.ShouldBeValid();

    }

    protected override LazyResponses ClientUsage() => Calls(
        (client, f) => client.Indices.Rollover(CallIsolatedValue + "-alias", f),
        (client, f) => client.Indices.RolloverAsync(CallIsolatedValue + "-alias", f),
        (client, r) => client.Indices.Rollover(r),
        (client, r) => client.Indices.RolloverAsync(r)
    );

    protected override RolloverIndexDescriptor NewDescriptor() => new(CallIsolatedValue + "-alias");

    protected override void ExpectResponse(RolloverIndexResponse response)
    {
        response.ShouldBeValid();
        response.OldIndex.Should().NotBeNullOrEmpty();
        response.NewIndex.Should().NotBeNullOrEmpty();
        response.RolledOver.Should().BeTrue();
        response.ShardsAcknowledged.Should().BeTrue();
        response.Conditions.Should().NotBeNull().And.HaveCount(2);
        response.Conditions["[max_age: 7d]"].Should().BeFalse();
        response.Conditions["[max_docs: 1000]"].Should().BeTrue();
    }
}
