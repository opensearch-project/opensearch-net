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
using System.IO;
using System.Text;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.Client.JsonNetSerializer;
using OpenSearch.Net;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;

namespace Tests.Reproduce;

public class DateSerialization
{
    [U]
    public void ShouldRoundtripDateTimeAndDateTimeOffsetWithSameKindAndOffset()
    {
        var dates = new Dates
        {
            DateTimeUtcKind = new DateTime(2016, 1, 1, 1, 1, 1, DateTimeKind.Utc),
            DateTimeOffset = new DateTimeOffset(1999, 1, 1, 1, 1, 1, 1, TimeSpan.FromHours(5)),
            DateTimeOffsetUtc = new DateTimeOffset(1999, 1, 1, 1, 1, 1, 1, TimeSpan.Zero)
        };

        var client = new OpenSearchClient();
        var serializedDates =
            client.SourceSerializer.SerializeToString(dates, client.ConnectionSettings.MemoryStreamFactory, SerializationFormatting.None);

        serializedDates.Should()
            .Contain("2016-01-01T01:01:01Z")
            .And.Contain("1999-01-01T01:01:01.0010000+05:00")
            .And.Contain("1999-01-01T01:01:01.0010000+00:00");

        using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(serializedDates)))
        {
            var deserializedDates = client.RequestResponseSerializer.Deserialize<Dates>(stream);

            deserializedDates.DateTimeUtcKind.Should().Be(dates.DateTimeUtcKind);
            deserializedDates.DateTimeUtcKind.Kind.Should().Be(dates.DateTimeUtcKind.Kind);

            deserializedDates.DateTimeOffset.Should().Be(dates.DateTimeOffset);
            deserializedDates.DateTimeOffset.Offset.Should().Be(dates.DateTimeOffset.Offset);
            deserializedDates.DateTimeOffset.Date.Kind.Should().Be(dates.DateTimeOffset.Date.Kind);

            deserializedDates.DateTimeOffsetUtc.Should().Be(dates.DateTimeOffsetUtc);
            deserializedDates.DateTimeOffsetUtc.Offset.Should().Be(dates.DateTimeOffsetUtc.Offset);
            deserializedDates.DateTimeOffsetUtc.Date.Kind.Should().Be(dates.DateTimeOffsetUtc.Date.Kind);
        }
    }

    [U]
    public void ShouldRoundtripDateTimeAndDateTimeOffsetWithSameKindAndOffsetNewtonsoft()
    {
        var dates = new Dates
        {
            DateTimeUtcKind = new DateTime(2016, 1, 1, 1, 1, 1, DateTimeKind.Utc),
            DateTimeOffset = new DateTimeOffset(1999, 1, 1, 1, 1, 1, 1, TimeSpan.FromHours(5)),
            DateTimeOffsetUtc = new DateTimeOffset(1999, 1, 1, 1, 1, 1, 1, TimeSpan.Zero)
        };

        var sett = new ConnectionSettings(new SingleNodeConnectionPool(new Uri("http://localhost:9200")), JsonNetSerializer.Default);
        var client = new OpenSearchClient(sett);
        var serializedDates =
            client.SourceSerializer.SerializeToString(dates, client.ConnectionSettings.MemoryStreamFactory, SerializationFormatting.None);

        serializedDates.Should()
            .Contain("2016-01-01T01:01:01Z")
            .And.Contain("1999-01-01T01:01:01.001+05:00")
            .And.Contain("1999-01-01T01:01:01.001+00:00");

        using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(serializedDates)))
        {
            var deserializedDates = client.RequestResponseSerializer.Deserialize<Dates>(stream);

            deserializedDates.DateTimeUtcKind.Should().Be(dates.DateTimeUtcKind);
            deserializedDates.DateTimeUtcKind.Kind.Should().Be(dates.DateTimeUtcKind.Kind);

            deserializedDates.DateTimeOffset.Should().Be(dates.DateTimeOffset);
            deserializedDates.DateTimeOffset.Offset.Should().Be(dates.DateTimeOffset.Offset);
            deserializedDates.DateTimeOffset.Date.Kind.Should().Be(dates.DateTimeOffset.Date.Kind);

            deserializedDates.DateTimeOffsetUtc.Should().Be(dates.DateTimeOffsetUtc);
            deserializedDates.DateTimeOffsetUtc.Offset.Should().Be(dates.DateTimeOffsetUtc.Offset);
            deserializedDates.DateTimeOffsetUtc.Date.Kind.Should().Be(dates.DateTimeOffsetUtc.Date.Kind);
        }
    }

    private class Dates
    {
        public DateTimeOffset DateTimeOffset { get; set; }
        public DateTimeOffset DateTimeOffsetUtc { get; set; }
        public DateTime DateTimeUtcKind { get; set; }
    }
}
