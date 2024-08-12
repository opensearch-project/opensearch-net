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
using OpenSearch.Client;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Analysis;
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Domain;
using Tests.Framework.EndpointTests.TestState;
using static Tests.Framework.Extensions.Promisify;

namespace Tests.Mapping.Types.Core.Keyword;

public class KeywordPropertyTests : PropertyTestsBase
{
    public KeywordPropertyTests(WritableCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

    protected override object ExpectJson => new
    {
        properties = new
        {
            state = new
            {
                type = "keyword",
                doc_values = false,
                boost = 1.2,
                eager_global_ordinals = true,
                ignore_above = 50,
                index = false,
                index_options = "freqs",
                null_value = "null",
                norms = false,
                fields = new
                {
                    foo = new
                    {
                        type = "keyword",
                        ignore_above = 10
                    }
                },
                store = true,
                normalizer = "myCustom",
            }
        }
    };

    protected override Func<PropertiesDescriptor<Project>, IPromise<IProperties>> FluentProperties => f => f
        .Keyword(b => b
            .Name(p => p.State)
            .DocValues(false)
            .Boost(1.2)
            .EagerGlobalOrdinals()
            .IgnoreAbove(50)
            .Index(false)
            .IndexOptions(IndexOptions.Freqs)
            .NullValue("null")
            .Normalizer("myCustom")
            .Norms(false)
            .Store()
            .Fields(fs => fs
                .Keyword(k => k
                    .Name("foo")
                    .IgnoreAbove(10)
                )
            )
        );


    protected override IProperties InitializerProperties => new Properties
    {
        {
            "state", new KeywordProperty
            {
                DocValues = false,
                Boost = 1.2,
                EagerGlobalOrdinals = true,
                IgnoreAbove = 50,
                Index = false,
                IndexOptions = IndexOptions.Freqs,
                NullValue = "null",
                Normalizer = "myCustom",
                Norms = false,
                Store = true,
                Fields = new Properties
                {
                    { "foo", new KeywordProperty { IgnoreAbove = 10 } }
                }
            }
        }
    };

    protected override ICreateIndexRequest CreateIndexSettings(CreateIndexDescriptor create) => create
        .Settings(s => s
            .Analysis(a => a
                .CharFilters(t => Promise(AnalysisUsageTests.CharFiltersFluent.Analysis.CharFilters))
                .TokenFilters(t => Promise(AnalysisUsageTests.TokenFiltersFluent.Analysis.TokenFilters))
                .Normalizers(t => Promise(AnalysisUsageTests.NormalizersInitializer.Analysis.Normalizers))
            )
        );

    public class KeywordPropertySplitQueriesOnWhitespaceTests : PropertyTestsBase
    {
        public KeywordPropertySplitQueriesOnWhitespaceTests(WritableCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

        protected override object ExpectJson => new
        {
            properties = new
            {
                state = new
                {
                    type = "keyword",
                    split_queries_on_whitespace = true
                }
            }
        };

        protected override Func<PropertiesDescriptor<Project>, IPromise<IProperties>> FluentProperties => f => f
            .Keyword(b => b
                .Name(p => p.State)
                .SplitQueriesOnWhitespace()
            );


        protected override IProperties InitializerProperties => new Properties
        {
            { "state", new KeywordProperty { SplitQueriesOnWhitespace = true } }
        };
    }
}
