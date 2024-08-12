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
using OpenSearch.Net;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Core.Client;

namespace Tests.Reproduce;

public class GitHubIssue3719
{
    [U]
    public void SerializeDateMathWithMinimumThreeDecimalPlacesWhenTens()
    {
        DateMath dateMath = new DateTime(2019, 5, 7, 12, 0, 0, 20);

        var json = TestClient.Default.RequestResponseSerializer.SerializeToString(dateMath, RecyclableMemoryStreamFactory.Default);
        json.Should().Be("\"2019-05-07T12:00:00.020\"");
    }

    [U]
    public void SerializeDateMathWithMinimumThreeDecimalPlacesWhenHundreds()
    {
        DateMath dateMath = new DateTime(2019, 5, 7, 12, 0, 0, 200);

        var json = TestClient.Default.RequestResponseSerializer.SerializeToString(dateMath, RecyclableMemoryStreamFactory.Default);
        json.Should().Be("\"2019-05-07T12:00:00.200\"");
    }
}
