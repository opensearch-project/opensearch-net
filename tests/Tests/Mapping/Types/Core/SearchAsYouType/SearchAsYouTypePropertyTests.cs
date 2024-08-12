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

namespace Tests.Mapping.Types.Core.SearchAsYouType;

public class SearchAsYouTypePropertyTests : PropertyTestsBase
{
    public SearchAsYouTypePropertyTests(WritableCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

    protected override object ExpectJson => new
    {
        properties = new
        {
            name = new
            {
                max_shingle_size = 4,
                type = "search_as_you_type",
                analyzer = "standard",
                copy_to = new[] { "other_field" },
                index = true,
                index_options = "offsets",
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
        .SearchAsYouType(s => s
            .MaxShingleSize(4)
            .Name(p => p.Name)
            .Analyzer("standard")
            .CopyTo(c => c
                .Field("other_field")
            )
            .Index()
            .IndexOptions(IndexOptions.Offsets)
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
            "name", new SearchAsYouTypeProperty
            {
                MaxShingleSize = 4,
                Analyzer = "standard",
                CopyTo = "other_field",
                Index = true,
                IndexOptions = IndexOptions.Offsets,
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
