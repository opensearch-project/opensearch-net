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

using System.IO;
using System.Linq;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.Net;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Core.Client;
using Tests.Framework.DocumentationTests;

namespace Tests.Document.Multiple.Bulk;

public class BulkResponseParstingTests : DocumentationTestBase
{
    [U]
    public void CanDeserialize()
    {
        var client = TestClient.DefaultInMemoryClient;
        var count = 100000;
        var bytes = client.RequestResponseSerializer.SerializeToBytes(ReturnBulkResponse(count));
        var x = Deserialize(bytes, client);
        x.Items.Should().HaveCount(count).And.NotContain(i => i == null);
    }

    private BulkResponse Deserialize(byte[] response, IOpenSearchClient client)
    {
        using (var ms = new MemoryStream(response))
            return client.RequestResponseSerializer.Deserialize<BulkResponse>(ms);
    }

    private static object BulkItemResponse() => new
    {
        index = new
        {
            _index = "osc-52cfd7aa",
            _type = "project",
            _id = "Kuhn LLC",
            _version = 1,
            _shards = new
            {
                total = 2,
                successful = 1,
                failed = 0
            },
            created = true,
            status = 201
        }
    };


    private static object ReturnBulkResponse(int numberOfItems) => new
    {
        took = 276,
        errors = false,
        items = Enumerable.Range(0, numberOfItems)
            .Select(i => BulkItemResponse())
            .ToArray()
    };
}
