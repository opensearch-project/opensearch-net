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

namespace Tests.Mapping.Types.Core.DateNanos;

public class DateNanosTest
{
    [DateNanos(
        DocValues = true,
        Store = true,
        Index = false,
        Boost = 1.2,
        IgnoreMalformed = true,
        Format = "yyyy-MM-dd'T'HH:mm[:ss][.S]")]
    public DateTime Full { get; set; }

    [DateNanos]
    public DateTime Minimal { get; set; }
}

public class DateNanosAttributeTests : AttributeTestsBase<DateNanosTest>
{
    protected override object ExpectJson => new
    {
        properties = new
        {
            full = new
            {
                type = "date_nanos",
                doc_values = true,
                store = true,
                index = false,
                boost = 1.2,
                ignore_malformed = true,
                format = "yyyy-MM-dd'T'HH:mm[:ss][.S]"
            },
            minimal = new
            {
                type = "date_nanos"
            }
        }
    };
}
