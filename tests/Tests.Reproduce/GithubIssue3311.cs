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
using Tests.Core.ManagedOpenSearch.Clusters;

namespace Tests.Reproduce;

public class GithubIssue3314 : IClusterFixture<WritableCluster>
{
    private readonly WritableCluster _cluster;
    public GithubIssue3314(WritableCluster cluster) => _cluster = cluster;

    [I]
    public void ValueAggregateWhenNullShouldStillReturn()
    {
        var indexName = "max-bucket-reproduce";

        var ec = _cluster.Client;
        var now = DateTime.Now;
        var testdata = Enumerable.Empty<DateTime>()
            .Concat(Enumerable.Repeat(now.AddMinutes(1), 3))
            .Concat(Enumerable.Repeat(now.AddMinutes(2), 4))
            .Concat(Enumerable.Repeat(now.AddMinutes(3), 1))
            .Select((d, i) => new MyClass { Id = i, Time = d, Message = "test", MessageType = 1 })
            .ToArray();
        ec.Indices.Delete(indexName);
        ec.IndexMany(testdata, indexName);
        ec.Indices.Refresh(indexName);

        var res = ec
            .Search<MyClass>(s => s
                .Index(indexName)
                .Query(q => q
                    .Term(t => t
                            .Field(ff => ff.MessageType)
                            .Value(2) //none of the docs has value 2 for messageType
                    )
                )
                .Aggregations(agg => agg
                    .DateHistogram("hist", dh => dh
                        .Field(ff => ff.Time)
                        .FixedInterval(new Time(TimeSpan.FromMinutes(1)))
                    )
                    .MaxBucket("max", mb => mb
                        .BucketsPath("hist>_count")
                    )
                )
            );

        var max = (KeyedValueAggregate)res.Aggregations["max"];
        max.Keys.Should().BeEmpty();
        max.Value.Should().BeNull();
    }

    public class MyClass
    {
        public int Id { get; set; }
        public DateTime Time { get; set; }
        public int MessageType { get; set; }
        public string Message { get; set; }
    }
}
