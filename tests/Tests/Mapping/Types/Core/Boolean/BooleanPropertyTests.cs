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

namespace Tests.Mapping.Types.Core.Boolean;

public class BooleanPropertyTests : PropertyTestsBase
{
    public BooleanPropertyTests(WritableCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

    protected override object ExpectJson => new
    {
        properties = new
        {
            name = new
            {
                type = "boolean",
                boost = 1.3,
                doc_values = false,
                store = true,
                index = false,
                null_value = false,
            }
        }
    };

    protected override Func<PropertiesDescriptor<Project>, IPromise<IProperties>> FluentProperties => f => f
        .Boolean(b => b
            .Name(p => p.Name)
            .Boost(1.3)
            .DocValues(false)
            .Store()
            .Index(false)
            .NullValue(false)
        );

    protected override IProperties InitializerProperties => new Properties
    {
        {
            "name", new BooleanProperty
            {
                DocValues = false,
                Boost = 1.3,
                Store = true,
                Index = false,
                NullValue = false
            }
        }
    };
}
