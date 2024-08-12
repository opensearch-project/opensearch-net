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
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;

namespace Tests.Reproduce;

public class GitHubIssue4876
{
    private readonly OpenSearchClient _client;

    public GitHubIssue4876() => _client = new OpenSearchClient();

    [U]
    public void CanDeserializeExtendedFormatOffsetIso8601DateTime() =>
        AssertDateTime("2020-07-31T12:29:29.4425068+10:00", 637317593694425068);

    [U]
    public void CanDeserializeBasicFormatOffsetWithMinutesIso8601DateTime() =>
        AssertDateTime("2020-07-31T12:29:29.4425068+1000", 637317593694425068);

    [U]
    public void CanDeserializeBasicFormatOffsetIso8601DateTime() =>
        AssertDateTime("2020-07-31T12:29:29.4425068+10", 637317593694425068);

    [U]
    public void ThrowExceptionWhenInvalidBasicFormatOffset()
    {
        Action action = () => AssertDateTime("2020-07-31T12:29:29.4425068+100", 637317593694425068);
        action.Should().Throw<InvalidOperationException>();
    }

    [U]
    public void ThrowExceptionWhenInvalidBasicFormatOffset2()
    {
        Action action = () => AssertDateTime("2020-07-31T12:29:29.4425068-10000", 637317593694425068);
        action.Should().Throw<InvalidOperationException>();
    }

    [U]
    public void CanDeserializeExtendedFormatOffsetIso8601DateTimeOffset() =>
        AssertDateTimeOffset("2020-07-31T12:29:29.4425068+10:00", 637317593694425068);

    [U]
    public void CanDeserializeBasicFormatOffsetWithMinutesIso8601DateTimeOffset() =>
        AssertDateTimeOffset("2020-07-31T12:29:29.4425068+1000", 637317593694425068);

    [U]
    public void CanDeserializeBasicFormatOffsetIso8601DateTimeOffset() =>
        AssertDateTimeOffset("2020-07-31T12:29:29.4425068+10", 637317593694425068);

    [U]
    public void CanDeserializeNoFractionalSeconds()
    {
        var ticks = 637321081960000000;
        AssertDateTime("2020-08-04T13:23:16+10:00", ticks);
        AssertDateTime("2020-08-04T13:23:16+1000", ticks);
        AssertDateTime("2020-08-04T13:23:16+10", ticks);
        AssertDateTimeOffset("2020-08-04T13:23:16+10:00", ticks);
        AssertDateTimeOffset("2020-08-04T13:23:16+1000", ticks);
        AssertDateTimeOffset("2020-08-04T13:23:16+10", ticks);
    }

    [U]
    public void CanDeserializeOneFractionalSecond()
    {
        var ticks = 637321081967000000;
        AssertDateTime("2020-08-04T13:23:16.7+10:00", ticks);
        AssertDateTime("2020-08-04T13:23:16.7+1000", ticks);
        AssertDateTime("2020-08-04T13:23:16.7+10", ticks);
        AssertDateTimeOffset("2020-08-04T13:23:16.7+10:00", ticks);
        AssertDateTimeOffset("2020-08-04T13:23:16.7+1000", ticks);
        AssertDateTimeOffset("2020-08-04T13:23:16.7+10", ticks);
    }

    [U]
    public void CanDeserializeTwoFractionalSeconds()
    {
        var ticks = 637321081967200000;
        AssertDateTime("2020-08-04T13:23:16.72+10:00", ticks);
        AssertDateTime("2020-08-04T13:23:16.72+1000", ticks);
        AssertDateTime("2020-08-04T13:23:16.72+10", ticks);
        AssertDateTimeOffset("2020-08-04T13:23:16.72+10:00", ticks);
        AssertDateTimeOffset("2020-08-04T13:23:16.72+1000", ticks);
        AssertDateTimeOffset("2020-08-04T13:23:16.72+10", ticks);
    }

    [U]
    public void CanDeserializeThreeFractionalSeconds()
    {
        var ticks = 637321081967220000;
        AssertDateTime("2020-08-04T13:23:16.722+10:00", ticks);
        AssertDateTime("2020-08-04T13:23:16.722+1000", ticks);
        AssertDateTime("2020-08-04T13:23:16.722+10", ticks);
        AssertDateTimeOffset("2020-08-04T13:23:16.722+10:00", ticks);
        AssertDateTimeOffset("2020-08-04T13:23:16.722+1000", ticks);
        AssertDateTimeOffset("2020-08-04T13:23:16.722+10", ticks);
    }

    [U]
    public void CanDeserializeFourFractionalSeconds()
    {
        var ticks = 637321081967220000;
        AssertDateTime("2020-08-04T13:23:16.7220+10:00", ticks);
        AssertDateTime("2020-08-04T13:23:16.7220+1000", ticks);
        AssertDateTime("2020-08-04T13:23:16.7220+10", ticks);
        AssertDateTimeOffset("2020-08-04T13:23:16.7220+10:00", ticks);
        AssertDateTimeOffset("2020-08-04T13:23:16.7220+1000", ticks);
        AssertDateTimeOffset("2020-08-04T13:23:16.7220+10", ticks);
    }

    [U]
    public void CanDeserializeFiveFractionalSeconds()
    {
        var ticks = 637321081967220100;
        AssertDateTime("2020-08-04T13:23:16.72201+10:00", ticks);
        AssertDateTime("2020-08-04T13:23:16.72201+1000", ticks);
        AssertDateTime("2020-08-04T13:23:16.72201+10", ticks);
        AssertDateTimeOffset("2020-08-04T13:23:16.72201+10:00", ticks);
        AssertDateTimeOffset("2020-08-04T13:23:16.72201+1000", ticks);
        AssertDateTimeOffset("2020-08-04T13:23:16.72201+10", ticks);
    }

    [U]
    public void CanDeserializeSixFractionalSeconds()
    {
        var ticks = 637321081967220110;
        AssertDateTime("2020-08-04T13:23:16.722011+10:00", ticks);
        AssertDateTime("2020-08-04T13:23:16.722011+1000", ticks);
        AssertDateTime("2020-08-04T13:23:16.722011+10", ticks);
        AssertDateTimeOffset("2020-08-04T13:23:16.722011+10:00", ticks);
        AssertDateTimeOffset("2020-08-04T13:23:16.722011+1000", ticks);
        AssertDateTimeOffset("2020-08-04T13:23:16.722011+10", ticks);
    }

    [U]
    public void CanDeserializeSevenFractionalSeconds()
    {
        var ticks = 637321081967220111;
        AssertDateTime("2020-08-04T13:23:16.7220111+10:00", ticks);
        AssertDateTime("2020-08-04T13:23:16.7220111+1000", ticks);
        AssertDateTime("2020-08-04T13:23:16.7220111+10", ticks);
        AssertDateTimeOffset("2020-08-04T13:23:16.7220111+10:00", ticks);
        AssertDateTimeOffset("2020-08-04T13:23:16.7220111+1000", ticks);
        AssertDateTimeOffset("2020-08-04T13:23:16.7220111+10", ticks);
    }

    [U]
    public void CanDeserializeMoreThanSevenFractionalSeconds()
    {
        var ticks = 637321081967220111;
        var digits = new Random().Next();
        AssertDateTime($"2020-08-04T13:23:16.7220111{digits}+10:00", ticks);
        AssertDateTime($"2020-08-04T13:23:16.7220111{digits}+1000", ticks);
        AssertDateTime($"2020-08-04T13:23:16.7220111{digits}+10", ticks);
        AssertDateTimeOffset($"2020-08-04T13:23:16.7220111{digits}+10:00", ticks);
        AssertDateTimeOffset($"2020-08-04T13:23:16.7220111{digits}+1000", ticks);
        AssertDateTimeOffset($"2020-08-04T13:23:16.7220111{digits}+10", ticks);
    }

    private void AssertDateTime(string json, long expectedTicks)
    {
        var stream = new MemoryStream(Encoding.UTF8.GetBytes($"{{\"timestamp\":\"{json}\"}}"));
        var document = _client.SourceSerializer.Deserialize<Document>(stream);

        document.Timestamp.Should().Be(
            new DateTime(expectedTicks, DateTimeKind.Utc).ToLocalTime());
    }

    private void AssertDateTimeOffset(string json, long expectedTicks)
    {
        var stream = new MemoryStream(Encoding.UTF8.GetBytes($"{{\"timestamp\":\"{json}\"}}"));
        var document = _client.SourceSerializer.Deserialize<Document2>(stream);

        document.Timestamp.Should().Be(
            new DateTimeOffset(expectedTicks, TimeSpan.Zero));
    }

    public class Document
    {
        public DateTime Timestamp { get; set; }
    }

    public class Document2
    {
        public DateTimeOffset Timestamp { get; set; }
    }
}
