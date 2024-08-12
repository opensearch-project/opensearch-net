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
using OpenSearch.Net.Utf8Json;
using OpenSearch.Net.Utf8Json.Formatters;


namespace OpenSearch.Client;

internal static class DateTimeUtil
{
    public static readonly DateTimeOffset UnixEpoch = new DateTimeOffset(1970, 1, 1, 0, 0, 0, 0, TimeSpan.Zero);
}

/// <summary>
/// Signals that this date time property returns the date as epoch.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class EpochDateTimeAttribute : Attribute { }

internal class EpochDateTimeOffsetFormatter : IJsonFormatter<DateTimeOffset>
{
    public DateTimeOffset Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
    {
        var token = reader.GetCurrentJsonToken();

        if (token == JsonToken.String)
        {
            var formatter = formatterResolver.GetFormatter<DateTimeOffset>();
            return formatter.Deserialize(ref reader, formatterResolver);
        }
        if (token == JsonToken.Null)
        {
            reader.ReadNext();
            return default;
        }

        if (token == JsonToken.Number)
        {
            var millisecondsSinceEpoch = reader.ReadDouble();
            var dateTimeOffset = DateTimeUtil.UnixEpoch.AddMilliseconds(millisecondsSinceEpoch);
            return dateTimeOffset;
        }

        throw new Exception($"Cannot deserialize {nameof(DateTimeOffset)} from token {token}");
    }

    public virtual void Serialize(ref JsonWriter writer, DateTimeOffset value, IJsonFormatterResolver formatterResolver) =>
        ISO8601DateTimeOffsetFormatter.Default.Serialize(ref writer, value, formatterResolver);
}

internal class EpochDateTimeFormatter : IJsonFormatter<DateTime>
{
    public static readonly EpochDateTimeFormatter Instance = new EpochDateTimeFormatter();

    public DateTime Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
    {
        var token = reader.GetCurrentJsonToken();

        if (token == JsonToken.String)
        {
            var formatter = formatterResolver.GetFormatter<DateTime>();
            return formatter.Deserialize(ref reader, formatterResolver);
        }
        if (token == JsonToken.Null)
        {
            reader.ReadNext();
            return default;
        }

        if (token == JsonToken.Number)
        {
            var millisecondsSinceEpoch = reader.ReadDouble();
            var dateTimeOffset = DateTimeUtil.UnixEpoch.AddMilliseconds(millisecondsSinceEpoch);
            return dateTimeOffset.DateTime;
        }

        throw new Exception($"Cannot deserialize {nameof(DateTimeOffset)} from token {token}");
    }

    public void Serialize(ref JsonWriter writer, DateTime value, IJsonFormatterResolver formatterResolver) =>
        ISO8601DateTimeFormatter.Default.Serialize(ref writer, value, formatterResolver);
}
