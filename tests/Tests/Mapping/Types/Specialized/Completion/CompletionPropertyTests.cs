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
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Domain;
using Tests.Framework.EndpointTests.TestState;

namespace Tests.Mapping.Types.Specialized.Completion;

public class CompletionPropertyTests : PropertyTestsBase
{
    public CompletionPropertyTests(WritableCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

    protected override object ExpectJson => new
    {
        properties = new
        {
            suggest = new
            {
                type = "completion",
                search_analyzer = "standard",
                analyzer = "standard",
                preserve_separators = true,
                preserve_position_increments = true,
                max_input_length = 20
            }
        }
    };

    protected override Func<PropertiesDescriptor<Project>, IPromise<IProperties>> FluentProperties => f => f
        .Completion(s => s
            .Name(p => p.Suggest)
            .SearchAnalyzer("standard")
            .Analyzer("standard")
            .PreserveSeparators()
            .PreservePositionIncrements()
            .MaxInputLength(20)
        );


    protected override IProperties InitializerProperties => new Properties
    {
        {
            "suggest", new CompletionProperty
            {
                SearchAnalyzer = "standard",
                Analyzer = "standard",
                PreserveSeparators = true,
                PreservePositionIncrements = true,
                MaxInputLength = 20
            }
        }
    };
}
