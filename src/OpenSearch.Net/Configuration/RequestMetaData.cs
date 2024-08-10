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

namespace OpenSearch.Net;

/// <summary>
/// Holds meta data about a client request.
/// </summary>
public sealed class RequestMetaData
{
    /// <summary>
    /// Reserved key for a meta data entry which identifies the helper which produced the request.
    /// </summary>
    internal const string HelperKey = "helper";

    private Dictionary<string, string> _metaDataItems;

    internal bool TryAddMetaData(string key, string value)
    {
        _metaDataItems ??= new Dictionary<string, string>();

#if NETSTANDARD2_1 || NET6_0_OR_GREATER
			return _metaDataItems.TryAdd(key, value);
#else
        if (_metaDataItems.ContainsKey(key))
            return false;

        _metaDataItems.Add(key, value);
        return true;
#endif
    }

    public IReadOnlyDictionary<string, string> Items => _metaDataItems ?? EmptyReadOnly<string, string>.Dictionary;
}
