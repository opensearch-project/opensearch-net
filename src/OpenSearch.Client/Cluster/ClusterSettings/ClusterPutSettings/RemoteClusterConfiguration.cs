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
using System.Linq;

namespace OpenSearch.Client;

/// <summary>
/// Simplifies the creation of remote cluster configuration, can be combined with a dictionary using the overloaded + operator
/// </summary>
public class RemoteClusterConfiguration : IsADictionaryBase<string, object>
{
    private readonly Dictionary<string, object> _remoteDictionary =
        new Dictionary<string, object>();

    public RemoteClusterConfiguration() =>
        BackingDictionary["cluster"] = new Dictionary<string, object>
        {
            { "remote", _remoteDictionary }
        };

    /// <summary>
    /// Adds seeds for the remote cluster specified by name
    /// </summary>
    public void Add(string name, params Uri[] seeds) =>
        Add(name, seeds.Select(u => $"{u.Host}:{u.Port}").ToArray());

    /// <summary>
    /// Adds seeds for the remote cluster specified by name
    /// </summary>
    public void Add(string name, params string[] seeds) =>
        Add(name, new Dictionary<string, object>
        {
            { "seeds", seeds }
        });

    /// <summary>
    /// Adds settings for the remote cluster specified by name
    /// </summary>
    public void Add(string name, Dictionary<string, object> remoteSettings) =>
        _remoteDictionary[name] = remoteSettings;

    public static Dictionary<string, object> operator +(RemoteClusterConfiguration left, IDictionary<string, object> right) => Combine(left, right);
    public static Dictionary<string, object> operator +(IDictionary<string, object> left, RemoteClusterConfiguration right) => Combine(left, right);

    private static Dictionary<string, object> Combine(IDictionary<string, object> left, IDictionary<string, object> right)
    {
        if (left == null && right == null) return null;
        if (left == null) return new Dictionary<string, object>(right);
        if (right == null) return new Dictionary<string, object>(left);

        foreach (var kv in right) left[kv.Key] = kv.Value;
        return new Dictionary<string, object>(left);
    }
}
