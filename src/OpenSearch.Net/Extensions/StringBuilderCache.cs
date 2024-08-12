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

namespace OpenSearch.Net.Extensions;

/// <summary>Provide a cached reusable instance of stringbuilder per thread.</summary>
internal static class StringBuilderCache
{
    private const int DefaultCapacity = 16; // == StringBuilder.DefaultCapacity

    // The value 360 was chosen in discussion with performance experts as a compromise between using
    // as little memory per thread as possible and still covering a large part of short-lived
    // StringBuilder creations on the startup path of VS designers.
    private const int MaxBuilderSize = 360;

    // WARNING: We allow diagnostic tools to directly inspect this member (t_cachedInstance).
    // See https://github.com/dotnet/corert/blob/master/Documentation/design-docs/diagnostics/diagnostics-tools-contract.md for more details.
    // Please do not change the type, the name, or the semantic usage of this member without understanding the implication for tools.
    // Get in touch with the diagnostics team if you have questions.
    [ThreadStatic]
    private static StringBuilder _cachedInstance;

    /// <summary>Get a StringBuilder for the specified capacity.</summary>
    /// <remarks>If a StringBuilder of an appropriate size is cached, it will be returned and the cache emptied.</remarks>
    public static StringBuilder Acquire(int capacity = DefaultCapacity)
    {
        if (capacity <= MaxBuilderSize)
        {
            var sb = _cachedInstance;
            if (sb != null)
            {
                // Avoid stringbuilder block fragmentation by getting a new StringBuilder
                // when the requested size is larger than the current capacity
                if (capacity <= sb.Capacity)
                {
                    _cachedInstance = null;
                    sb.Clear();
                    return sb;
                }
            }
        }

        return new StringBuilder(capacity);
    }

    /// <summary>Place the specified builder in the cache if it is not too big.</summary>
    public static void Release(StringBuilder sb)
    {
        if (sb.Capacity <= MaxBuilderSize) _cachedInstance = sb;
    }

    /// <summary>ToString() the stringbuilder, Release it to the cache, and return the resulting string.</summary>
    public static string GetStringAndRelease(StringBuilder sb)
    {
        var result = sb.ToString();
        Release(sb);
        return result;
    }
}
