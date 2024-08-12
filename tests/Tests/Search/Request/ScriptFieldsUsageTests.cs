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
using FluentAssertions;
using OpenSearch.Client;
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Domain;
using Tests.Framework.EndpointTests.TestState;

namespace Tests.Search.Request;

/**
	 * Allows to return a script evaluation (based on different fields) for each hit.
	 *
	 * Script fields can work on fields that are not stored, and allow to return custom values to
	 * be returned (the evaluated value of the script).
	 *
	 * Script fields can also access the actual `_source` document and extract specific elements to
	 * be returned from it by using `params['_source']`.
	 *
	 * Script fields can be accessed on the response using <<returned-fields,`.Fields`>>, similarly to stored fields.
	 *
	 * See the OpenSearch documentation on {ref_current}/search-request-body.html#request-body-search-script-fields[script fields]
	 * for more detail.
	 */
public class ScriptFieldsUsageTests : SearchUsageTestBase
{
    public ScriptFieldsUsageTests(ReadOnlyCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

    protected override object ExpectJson => new
    {
        script_fields = new
        {
            test1 = new
            {
                script = new
                {
                    source = "doc['numberOfCommits'].value * 2",
                }
            },
            test2 = new
            {
                script = new
                {
                    source = "doc['numberOfCommits'].value * params.factor",
                    @params = new
                    {
                        factor = 2.0
                    }
                }
            }
        }
    };

    protected override Func<SearchDescriptor<Project>, ISearchRequest> Fluent => s => s
        .ScriptFields(sf => sf
            .ScriptField("test1", sc => sc
                .Source("doc['numberOfCommits'].value * 2")
            )
            .ScriptField("test2", sc => sc
                .Source("doc['numberOfCommits'].value * params.factor")
                .Params(p => p
                    .Add("factor", 2.0)
                )
            )
        );

    protected override SearchRequest<Project> Initializer =>
        new SearchRequest<Project>
        {
            ScriptFields = new ScriptFields
            {
                { "test1", new ScriptField { Script = new InlineScript("doc['numberOfCommits'].value * 2") } },
                {
                    "test2", new InlineScript("doc['numberOfCommits'].value * params.factor")
                    {
                        Params = new FluentDictionary<string, object>
                        {
                            { "factor", 2.0 }
                        }
                    }
                }
            }
        };


    protected override void ExpectResponse(ISearchResponse<Project> response)
    {
        foreach (var fields in response.Fields)
        {
            fields.Value<int>("test1").Should().BeGreaterOrEqualTo(0);
            fields.Value<double>("test2").Should().BeGreaterOrEqualTo(0);
        }
    }
}
