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
using System.Linq;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Core;
using Tests.Core.Client;
using Tests.Core.Extensions;
using Tests.Framework;

namespace Tests.ClientConcepts.HighLevel.CovariantHits;

public class CovariantSearchResults
{
    /**=== Covariant search results
		 *
		 * OSC used to have a feature that allowed you to map multiple types in an index back into a covariant list.
		 *
		 * Since types are removed in OpenSearch this feature is no longer supported. Because you can
		 * now explicitly inject a serializer for user types only (_source, fields etcetera) please rely on a JsonConverter that
		 * can do this out of the box e.g `TypeNameHandling.All` from `Json.NET`
		 *
		 * https://www.newtonsoft.com/json/help/html/SerializeTypeNameHandling.htm
		 */
    public class C
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
    private readonly IOpenSearchClient _client = FixedResponseClient.Create(SearchResultMock.Json);

    //hide
    [U]
    public void CanDeserializeHits()
    {
        var result = _client.Search<C>(s => s
            .Size(100)
        );
        result.ApiCall.Should().NotBeNull();
        result.ShouldBeValid();
        result.HitsMetadata.Should().NotBeNull();
        result.HitsMetadata.Hits.Should().NotBeNull();
        result.HitsMetadata.MaxScore.Should().BeGreaterThan(1.0);
        result.HitsMetadata.Total.Value.Should().Be(100);

        result.Hits.Should().OnlyContain(hit => hit.Index == "project", "_index on hit");
        result.Hits.Should().OnlyContain(hit => !string.IsNullOrEmpty(hit.Type), "_type on hit");
        result.Hits.Should().OnlyContain(hit => !string.IsNullOrEmpty(hit.Id), "_id on hit");
        result.Hits.Should().OnlyContain(hit => hit.Score.HasValue, "_score on hit");
        result.Hits.Should().OnlyContain(hit => hit.Source != null, "_source on hit");

        result.Documents.Count.Should().Be(100);
        result.Documents.Should().OnlyContain(d => d.Id > 0, "id on _source");
        result.Documents.Should().OnlyContain(d => !string.IsNullOrEmpty(d.Name), "name on _source");


    }
}

internal static class SearchResultMock
{
    public static object Json = new
    {
        took = 1,
        timed_out = false,
        _shards = new
        {
            total = 2,
            successful = 2,
            failed = 0
        },
        hits = new
        {
            total = new { value = 100 },
            max_score = 1.1,
            hits = Enumerable.Range(1, 25).Select(i => (object)new
            {
                _index = "project",
                _type = "a",
                _id = i.ToString(),
                _score = 1.0,
                _source = new { name = "A object", id = i }
            }).Concat(Enumerable.Range(26, 25).Select(i => (object)new
            {
                _index = "project",
                _type = "b",
                _id = i.ToString(),
                _score = 1.0,
                _source = new { name = "B object", id = i }
            })).Concat(Enumerable.Range(51, 50).Select(i => new
            {
                _index = "project",
                _type = "c",
                _id = i.ToString(),
                _score = 1.0,
                _source = new { name = "C object", id = i }
            })).ToArray()
        }
    };
}
