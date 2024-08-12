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
using System.Diagnostics;
using System.Linq;
using OpenSearch.Net;

namespace OpenSearch.Client;

[DebuggerDisplay("{DebugDisplay,nq}")]
public class Ids : IUrlParameter, IEquatable<Ids>
{
    private readonly List<string> _ids;

    public Ids(IEnumerable<string> value) => _ids = value?.ToList();

    public Ids(string value)
    {
        if (!value.IsNullOrEmptyCommaSeparatedList(out var arr))
            _ids = arr.ToList();
    }

    private string DebugDisplay => ((IUrlParameter)this).GetString(null);

    public override string ToString() => DebugDisplay;

    public bool Equals(Ids other)
    {
        if (other == null) return false;
        if (_ids == null && other._ids == null) return true;
        if (_ids == null || other._ids == null) return false;

        return _ids.Count == other._ids.Count &&
            _ids.OrderBy(id => id).SequenceEqual(other._ids.OrderBy(id => id));
    }

    string IUrlParameter.GetString(IConnectionConfigurationValues settings) =>
        string.Join(",", _ids ?? Enumerable.Empty<string>());

    public static implicit operator Ids(string value) =>
        value.IsNullOrEmptyCommaSeparatedList(out var arr) ? null : new Ids(arr);

    public static implicit operator Ids(string[] value) =>
        value.IsEmpty() ? null : new Ids(value);

    public override bool Equals(object obj) => obj is Ids other && Equals(other);

    public override int GetHashCode()
    {
        if (_ids == null) return 0;
        unchecked
        {
            var hc = 0;
            foreach (var id in _ids.OrderBy(id => id))
                hc = hc * 17 + id.GetHashCode();
            return hc;
        }
    }

    public static bool operator ==(Ids left, Ids right) => Equals(left, right);

    public static bool operator !=(Ids left, Ids right) => !Equals(left, right);
}
