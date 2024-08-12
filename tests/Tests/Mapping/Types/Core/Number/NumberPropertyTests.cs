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

namespace Tests.Mapping.Types.Core.Number;

public class NumberPropertyTests : PropertyTestsBase
{
    public NumberPropertyTests(WritableCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

    protected override object ExpectJson => new
    {
        properties = new
        {
            numberOfCommits = new
            {
                type = "integer",
                doc_values = true,
                similarity = "BM25",
                store = true,
                index = false,
                null_value = 0.0,
                ignore_malformed = true,
                coerce = true
            }
        }
    };

    protected override Func<PropertiesDescriptor<Project>, IPromise<IProperties>> FluentProperties => f => f
        .Number(n => n
            .Name(p => p.NumberOfCommits)
            .Type(NumberType.Integer)
            .DocValues()
            .Similarity("BM25")
            .Store()
            .Index(false)
            .NullValue(0.0)
            .IgnoreMalformed()
            .Coerce()
        );


    protected override IProperties InitializerProperties => new Properties
    {
        {
            "numberOfCommits", new NumberProperty(NumberType.Integer)
            {
                DocValues = true,
                Similarity = "BM25",
                Store = true,
                Index = false,
                NullValue = 0.0,
                IgnoreMalformed = true,
                Coerce = true
            }
        }
    };
}

public class ScaledFloatNumberPropertyTests : PropertyTestsBase
{
    public ScaledFloatNumberPropertyTests(WritableCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

    protected override object ExpectJson => new
    {
        properties = new
        {
            numberOfCommits = new
            {
                type = "scaled_float",
                scaling_factor = 10.0,
                doc_values = true,
                similarity = "BM25",
                store = true,
                index = false,
                null_value = 0.0,
                ignore_malformed = true,
                coerce = true
            }
        }
    };

    protected override Func<PropertiesDescriptor<Project>, IPromise<IProperties>> FluentProperties => f => f
        .Number(n => n
            .Name(p => p.NumberOfCommits)
            .Type(NumberType.ScaledFloat)
            .ScalingFactor(10)
            .DocValues()
            .Similarity("BM25")
            .Store()
            .Index(false)
            .NullValue(0.0)
            .IgnoreMalformed()
            .Coerce()
        );


    protected override IProperties InitializerProperties => new Properties
    {
        {
            "numberOfCommits", new NumberProperty(NumberType.ScaledFloat)
            {
                ScalingFactor = 10,
                DocValues = true,
                Similarity = "BM25",
                Store = true,
                Index = false,
                NullValue = 0.0,
                IgnoreMalformed = true,
                Coerce = true
            }
        }
    };
}
