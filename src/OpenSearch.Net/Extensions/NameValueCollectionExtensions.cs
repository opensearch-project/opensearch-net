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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace OpenSearch.Net.Extensions;

internal static class NameValueCollectionExtensions
{
    internal static string ToQueryString(this NameValueCollection nv)
    {
        if (nv == null || nv.AllKeys.Length == 0) return string.Empty;

        // initialize with capacity for number of key/values with length 5 each
        var builder = new StringBuilder("?", nv.AllKeys.Length * 2 * 5);
        for (var i = 0; i < nv.AllKeys.Length; i++)
        {
            if (i != 0)
                builder.Append("&");

            var key = nv.AllKeys[i];
            builder.Append(Uri.EscapeDataString(key));
            var value = nv[key];
            if (!value.IsNullOrEmpty())
            {
                builder.Append("=");
                builder.Append(Uri.EscapeDataString(nv[key]));
            }
        }

        return builder.ToString();
    }

    internal static void UpdateFromDictionary(this NameValueCollection queryString, Dictionary<string, object> queryStringUpdates,
        OpenSearchUrlFormatter provider
    )
    {
        if (queryString == null || queryString.Count < 0) return;
        if (queryStringUpdates == null || queryStringUpdates.Count < 0) return;

        foreach (var kv in queryStringUpdates.Where(kv => !kv.Key.IsNullOrEmpty()))
        {
            if (kv.Value == null)
            {
                queryString.Remove(kv.Key);
                continue;
            }
            var resolved = provider.CreateString(kv.Value);
            if (resolved != null)
                queryString[kv.Key] = resolved;
            else
                queryString.Remove(kv.Key);
        }
    }
}
