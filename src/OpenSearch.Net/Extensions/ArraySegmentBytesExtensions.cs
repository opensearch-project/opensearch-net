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
using System.Globalization;
using System.Runtime.CompilerServices;
using OpenSearch.Net.Utf8Json;
using OpenSearch.Net.Utf8Json.Formatters;
using OpenSearch.Net.Utf8Json.Internal;

namespace OpenSearch.Net.Extensions;

internal static class ArraySegmentBytesExtensions
{
    private const byte DateMathSeparator = (byte)'|';
    private const byte DecimalPoint = (byte)'.';

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsDouble(this ref ArraySegment<byte> arraySegment)
    {
        if (arraySegment.Array == null)
            return false;

        var i = 0;
        while (i < arraySegment.Count)
        {
            if (arraySegment.Array[arraySegment.Offset + i] == DecimalPoint)
                return true;

            i++;
        }

        return false;
    }

    private static readonly byte[] LongMaxValue = StringEncoding.UTF8.GetBytes(long.MaxValue.ToString(CultureInfo.InvariantCulture));

    private static readonly byte[] LongMinValue = StringEncoding.UTF8.GetBytes(long.MinValue.ToString(CultureInfo.InvariantCulture));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsLong(this ref ArraySegment<byte> arraySegment)
    {
        if (arraySegment.Array == null || arraySegment.Count == 0 || arraySegment.Count > 20)
            return false;

        var isNegative = arraySegment.Array[arraySegment.Offset] == '-';

        if (arraySegment.Count == 20 && !isNegative)
            return false;

        var longBytes = isNegative ? LongMinValue : LongMaxValue;

        // this doesn't handle positive values that are prefixed with + symbol.
        // OpenSearch does not return values with this prefix.
        var i = isNegative ? 1 : 0;
        var check = arraySegment.Count == longBytes.Length;
        while (i < arraySegment.Count)
        {
            var b = arraySegment.Array[arraySegment.Offset + i];
            if (!NumberConverter.IsNumber(b))
                return false;

            // only compare to long.MinValue or long.MaxValue if we're dealing with same number of bytes
            if (check)
            {
                // larger than long.MinValue or long.MaxValue, bail early.
                if (b > longBytes[i])
                    return false;

                // only continue to check bytes if current byte is equal to longBytes[i]
                if (b < longBytes[i])
                    check = false;
            }

            i++;
        }

        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool EqualsBytes(this ref ArraySegment<byte> arraySegment, byte[] bytes)
    {
        if (arraySegment == default || bytes == null || bytes.Length == 0)
            return false;

        if (arraySegment.Count != bytes.Length)
            return false;

        for (var i = 0; i < bytes.Length; i++)
        {
            if (arraySegment.Array != null && bytes[i] != arraySegment.Array[arraySegment.Offset + i])
                return false;
        }

        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsDateTime(this ref ArraySegment<byte> arraySegment, IJsonFormatterResolver formatterResolver, out DateTime dateTime)
    {
        dateTime = default;

        // TODO: Nicer way to do this
        var reader = new JsonReader(arraySegment.Array, arraySegment.Offset - 1); // include opening quote "
        try
        {
            dateTime = ISO8601DateTimeFormatter.Default.Deserialize(ref reader, formatterResolver);
            return true;
        }
        catch
        {
            return false;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ContainsDateMathSeparator(this ref ArraySegment<byte> segment)
    {
        var i = 0;
        while (i < segment.Count)
        {
            if (segment.Array != null && (segment.Array[segment.Offset + i] == DateMathSeparator &&
                i + 1 < segment.Count && segment.Array[segment.Offset + i + 1] == DateMathSeparator))
                return true;

            i++;
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string Utf8String(this ref ArraySegment<byte> segment) =>
        // segment.Array is never null
        // ReSharper disable once AssignNullToNotNullAttribute
        StringEncoding.UTF8.GetString(segment.Array, segment.Offset, segment.Count);
}
