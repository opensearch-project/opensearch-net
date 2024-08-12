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
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Domain;
using Tests.Framework.EndpointTests.TestState;

namespace Tests.Mapping.Types.Core.Text;

public class TextPropertyIndexPhrasesTests : PropertyTestsBase
{
    public TextPropertyIndexPhrasesTests(WritableCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

    protected override object ExpectJson => new
    {
        properties = new
        {
            name = new
            {
                type = "text",
                index_phrases = true
            }
        }
    };


    protected override Func<PropertiesDescriptor<Project>, IPromise<IProperties>> FluentProperties => f => f
        .Text(s => s
            .Name(p => p.Name)
            .IndexPhrases()
        );


    protected override IProperties InitializerProperties => new Properties
    {
        { "name", new TextProperty { IndexPhrases = true } }
    };
}

public class TextPropertyIndexPrefixesTests : PropertyTestsBase
{
    public TextPropertyIndexPrefixesTests(WritableCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

    protected override object ExpectJson => new
    {
        properties = new
        {
            name = new
            {
                type = "text",
                index_prefixes = new
                {
                    min_chars = 1,
                    max_chars = 10
                }
            }
        }
    };


    protected override Func<PropertiesDescriptor<Project>, IPromise<IProperties>> FluentProperties => f => f
        .Text(s => s
            .Name(p => p.Name)
            .IndexPrefixes(i => i
                .MinCharacters(1)
                .MaxCharacters(10)
            )
        );


    protected override IProperties InitializerProperties => new Properties
    {
        {
            "name", new TextProperty
            {
                IndexPrefixes = new TextIndexPrefixes
                {
                    MinCharacters = 1,
                    MaxCharacters = 10
                }
            }
        }
    };
}

public class TextPropertyTests : PropertyTestsBase
{
    public TextPropertyTests(WritableCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

    protected override object ExpectJson => new
    {
        properties = new
        {
            name = new
            {
                type = "text",
                analyzer = "standard",
                boost = 1.2,
                copy_to = new[] { "other_field" },
                eager_global_ordinals = true,
                fielddata = true,
                fielddata_frequency_filter = new
                {
                    min = 1.0,
                    max = 100.00,
                    min_segment_size = 2
                },
                fields = new
                {
                    raw = new
                    {
                        type = "keyword",
                        ignore_above = 100
                    }
                },
                index = true,
                index_options = "offsets",
                position_increment_gap = 5,
                search_analyzer = "standard",
                search_quote_analyzer = "standard",
                similarity = "BM25",
                store = true,
                norms = false,
                term_vector = "with_positions_offsets"
            }
        }
    };


    protected override Func<PropertiesDescriptor<Project>, IPromise<IProperties>> FluentProperties => f => f
        .Text(s => s
            .Name(p => p.Name)
            .Analyzer("standard")
            .Boost(1.2)
            .CopyTo(c => c
                .Field("other_field")
            )
            .EagerGlobalOrdinals()
            .Fielddata()
            .FielddataFrequencyFilter(ff => ff
                .Min(1)
                .Max(100)
                .MinSegmentSize(2)
            )
            .Fields(fd => fd
                .Keyword(k => k
                    .Name("raw")
                    .IgnoreAbove(100)
                )
            )
            .Index()
            .IndexOptions(IndexOptions.Offsets)
            .PositionIncrementGap(5)
            .SearchAnalyzer("standard")
            .SearchQuoteAnalyzer("standard")
            .Similarity("BM25")
            .Store()
            .Norms(false)
            .TermVector(TermVectorOption.WithPositionsOffsets)
        );


    protected override IProperties InitializerProperties => new Properties
    {
        {
            "name", new TextProperty
            {
                Analyzer = "standard",
                Boost = 1.2,
                CopyTo = "other_field",
                EagerGlobalOrdinals = true,
                Fielddata = true,
                FielddataFrequencyFilter = new FielddataFrequencyFilter
                {
                    Min = 1,
                    Max = 100,
                    MinSegmentSize = 2
                },
                Fields = new Properties
                {
                    {
                        "raw", new KeywordProperty
                        {
                            IgnoreAbove = 100
                        }
                    }
                },
                Index = true,
                IndexOptions = IndexOptions.Offsets,
                PositionIncrementGap = 5,
                SearchAnalyzer = "standard",
                SearchQuoteAnalyzer = "standard",
                Similarity = "BM25",
                Store = true,
                Norms = false,
                TermVector = TermVectorOption.WithPositionsOffsets
            }
        }
    };
}
