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
using System.Text;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.Net;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Core.Client;

namespace Tests.Reproduce;

public class GithubIssue4703
{
    [U]
    public void NullableValueTupleDoesNotThrow()
    {
        var connectionSettings = new ConnectionSettings(new InMemoryConnection()).DisableDirectStreaming();
        var client = new OpenSearchClient(connectionSettings);

        Func<IndexResponse> action = () =>
            client.Index(
                new ExampleDoc
                {
                    tupleNullable = ("somestring", 42),
                }, i => i.Index("index"));

        var a = action.Should().NotThrow();
        var response = a.Subject;

        var json = Encoding.UTF8.GetString(response.ApiCall.RequestBodyInBytes);
        json.Should().Be(@"{""tupleNullable"":{""Item1"":""somestring"",""Item2"":42}}");
    }
}

public class ExampleDoc
{
    public (string info, int number)? tupleNullable { get; set; }
}
