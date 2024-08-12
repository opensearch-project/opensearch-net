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

namespace Tests.Reproduce;

public class TimeSpanSerialization
{
    [U]
    public void SerializeMaxTimeSpansAsTicksAndStrings()
    {
        var timeSpans = new TimeSpans(TimeSpan.MaxValue);
        var client = new OpenSearchClient();

        var json = client.RequestResponseSerializer.SerializeToString(timeSpans);

        json.Should()
            .Be("{\"default\":9223372036854775807,\"defaultNullable\":9223372036854775807,\"string\":\"10675199.02:48:05.4775807\",\"stringNullable\":\"10675199.02:48:05.4775807\"}");

        TimeSpans deserialized;
        using (var stream = client.ConnectionSettings.MemoryStreamFactory.Create(Encoding.UTF8.GetBytes(json)))
            deserialized = client.RequestResponseSerializer.Deserialize<TimeSpans>(stream);

        timeSpans.Default.Should().Be(deserialized.Default);
        timeSpans.DefaultNullable.Should().Be(deserialized.DefaultNullable);
        timeSpans.String.Should().Be(deserialized.String);
        timeSpans.StringNullable.Should().Be(deserialized.StringNullable);
    }

    [U]
    public void SerializeTimeSpansAsTicksAndStrings()
    {
        var timeSpans = new TimeSpans(TimeSpan.FromSeconds(902312));
        var client = new OpenSearchClient();

        var json = client.RequestResponseSerializer.SerializeToString(timeSpans);

        json.Should()
            .Be("{\"default\":9023120000000,\"defaultNullable\":9023120000000,\"string\":\"10.10:38:32\",\"stringNullable\":\"10.10:38:32\"}");

        TimeSpans deserialized;
        using (var stream = client.ConnectionSettings.MemoryStreamFactory.Create(Encoding.UTF8.GetBytes(json)))
            deserialized = client.RequestResponseSerializer.Deserialize<TimeSpans>(stream);

        timeSpans.Default.Should().Be(deserialized.Default);
        timeSpans.DefaultNullable.Should().Be(deserialized.DefaultNullable);
        timeSpans.String.Should().Be(deserialized.String);
        timeSpans.StringNullable.Should().Be(deserialized.StringNullable);
    }

    private class TimeSpans
    {
        public TimeSpans(TimeSpan timeSpan)
        {
            Default = timeSpan;
            DefaultNullable = timeSpan;
            String = timeSpan;
            StringNullable = timeSpan;
        }

        public TimeSpans()
        {
        }

        public TimeSpan Default { get; set; }

        public TimeSpan DefaultNullable { get; set; }

        [StringTimeSpan]
        public TimeSpan String { get; set; }

        [StringTimeSpan]
        public TimeSpan? StringNullable { get; set; }
    }
}
