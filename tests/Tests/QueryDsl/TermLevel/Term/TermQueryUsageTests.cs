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

using System.IO;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Domain;
using Tests.Framework.EndpointTests.TestState;

namespace Tests.QueryDsl.TermLevel.Term;

public class TermQueryUsageTests : QueryDslUsageTestsBase
{
    public TermQueryUsageTests(ReadOnlyCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

    protected override ConditionlessWhen ConditionlessWhen => new ConditionlessWhen<ITermQuery>(q => q.Term)
    {
        q => q.Field = null,
        q => q.Value = "  ",
        q => q.Value = null
    };

    protected override QueryContainer QueryInitializer => new TermQuery
    {
        Name = "named_query",
        Boost = 1.1,
        Field = "description",
        Value = "project description"
    };

    protected override object QueryJson => new
    {
        term = new
        {
            description = new
            {
                _name = "named_query",
                boost = 1.1,
                value = "project description"
            }
        }
    };

    protected override QueryContainer QueryFluent(QueryContainerDescriptor<Project> q) => q
        .Term(c => c
            .Name("named_query")
            .Boost(1.1)
            .Field(p => p.Description)
            .Value("project description")
        );

    //hide
    [U]
    public void DeserializeShortForm()
    {
        using var stream = new MemoryStream(ShortFormQuery);
        var query = Client.RequestResponseSerializer.Deserialize<ITermQuery>(stream);
        query.Should().NotBeNull();
        query.Field.Should().Be(new Field("description"));
        query.Value.Should().Be("project description");
    }
}

/**[float]
	*== Verbatim term query
	 *
	 * By default an empty term is conditionless so will be rewritten. Sometimes sending an empty term to
	 * match nothing makes sense. You can either use the `ConditionlessQuery` construct from OSC to provide a fallback or make the
	 * query verbatim as followed:
	*/
public class VerbatimTermQueryUsageTests : TermQueryUsageTests
{
    public VerbatimTermQueryUsageTests(ReadOnlyCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

    protected override ConditionlessWhen ConditionlessWhen => null;

    protected override QueryContainer QueryInitializer => new TermQuery
    {
        IsVerbatim = true,
        Field = "description",
        Value = "",
    };

    protected override object QueryJson => new
    {
        term = new
        {
            description = new
            {
                value = ""
            }
        }
    };

    //when reading back the json the notion of is conditionless is lost
    protected override bool SupportsDeserialization => false;

    protected override QueryContainer QueryFluent(QueryContainerDescriptor<Project> q) => q
        .Term(c => c
            .Verbatim()
            .Field(p => p.Description)
            .Value(string.Empty)
        );
}

public class TermQueryWithCaseInsensitiveUsageTests : QueryDslUsageTestsBase
{
    public TermQueryWithCaseInsensitiveUsageTests(ReadOnlyCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

    protected override ConditionlessWhen ConditionlessWhen => new ConditionlessWhen<ITermQuery>(q => q.Term)
    {
        q => q.Field = null,
        q => q.Value = "  ",
        q => q.Value = null
    };

    protected override QueryContainer QueryInitializer => new TermQuery
    {
        Name = "named_query",
        Boost = 1.1,
        Field = "description",
        Value = "project description",
        CaseInsensitive = true
    };

    protected override object QueryJson => new
    {
        term = new
        {
            description = new
            {
                _name = "named_query",
                boost = 1.1,
                value = "project description",
                case_insensitive = true
            }
        }
    };

    protected override QueryContainer QueryFluent(QueryContainerDescriptor<Project> q) => q
        .Term(c => c
            .Name("named_query")
            .Boost(1.1)
            .Field(p => p.Description)
            .Value("project description")
            .CaseInsensitive(true)
        );

    //hide
    [U]
    public void DeserializeShortForm()
    {
        using var stream = new MemoryStream(ShortFormQuery);
        var query = Client.RequestResponseSerializer.Deserialize<ITermQuery>(stream);
        query.Should().NotBeNull();
        query.Field.Should().Be(new Field("description"));
        query.Value.Should().Be("project description");
    }
}
