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

using System.Collections.Generic;
using System.Linq;
using OpenSearch.Net.Utf8Json;

namespace OpenSearch.Client;

public class TopHitsAggregate : MetricAggregateBase
{
    private readonly IJsonFormatterResolver _formatterResolver;
    private readonly IList<LazyDocument> _hits;

    public TopHitsAggregate() { }

    internal TopHitsAggregate(IList<LazyDocument> hits, IJsonFormatterResolver formatterResolver)
    {
        _hits = hits;
        _formatterResolver = formatterResolver;
    }

    public double? MaxScore { get; set; }

    public TotalHits Total { get; set; }

    private IEnumerable<IHit<TDocument>> ConvertHits<TDocument>()
        where TDocument : class
    {
        var formatter = _formatterResolver.GetFormatter<IHit<TDocument>>();
        return _hits.Select(h =>
        {
            var reader = new JsonReader(h.Bytes);
            return formatter.Deserialize(ref reader, _formatterResolver);
        });
    }

    public IReadOnlyCollection<IHit<TDocument>> Hits<TDocument>()
        where TDocument : class =>
        ConvertHits<TDocument>().ToList().AsReadOnly();

    public IReadOnlyCollection<TDocument> Documents<TDocument>() where TDocument : class =>
        ConvertHits<TDocument>().Select(h => h.Source).ToList().AsReadOnly();
}
