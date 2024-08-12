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
using System.Diagnostics;
using System.Globalization;
using OpenSearch.Net;
using OpenSearch.Net.Utf8Json;

namespace OpenSearch.Client;

[JsonFormatter(typeof(IdFormatter))]
[DebuggerDisplay("{DebugDisplay,nq}")]
public class Id : IEquatable<Id>, IUrlParameter
{
    public Id(string id)
    {
        Tag = 0;
        StringValue = id;
    }

    public Id(long id)
    {
        Tag = 1;
        LongValue = id;
    }

    public Id(object document)
    {
        Tag = 2;
        Document = document;
    }

    internal object Document { get; }
    internal long? LongValue { get; }
    internal string StringOrLongValue => StringValue ?? LongValue?.ToString(CultureInfo.InvariantCulture);
    internal string StringValue { get; }
    internal int Tag { get; }

    private string DebugDisplay => StringOrLongValue ?? "Id from instance typeof: " + Document?.GetType().Name;

    private static int TypeHashCode { get; } = typeof(Id).GetHashCode();

    public bool Equals(Id other)
    {
        if (Tag + other.Tag == 1)
            return StringOrLongValue == other.StringOrLongValue;
        else if (Tag != other.Tag) return false;

        switch (Tag)
        {
            case 0:
            case 1:
                return StringOrLongValue == other.StringOrLongValue;
            default:
                return Document?.Equals(other.Document) ?? false;
        }
    }

    string IUrlParameter.GetString(IConnectionConfigurationValues settings)
    {
        var oscSettings = (IConnectionSettingsValues)settings;
        return oscSettings.Inferrer.Id(Document) ?? StringOrLongValue;
    }

    public static implicit operator Id(string id) => id.IsNullOrEmpty() ? null : new Id(id);

    public static implicit operator Id(long id) => new Id(id);

    public static implicit operator Id(Guid id) => new Id(id.ToString("D"));

    public static Id From<T>(T document) where T : class => new Id(document);

    public override string ToString() => DebugDisplay;

    public override bool Equals(object obj)
    {
        switch (obj)
        {
            case Id r: return Equals(r);
            case string s: return Equals(s);
            case int l: return Equals(l);
            case long l: return Equals(l);
            case Guid g: return Equals(g);
        }
        return Equals(new Id(obj));
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var result = TypeHashCode;
            result = (result * 397) ^ (StringValue?.GetHashCode() ?? 0);
            result = (result * 397) ^ (LongValue?.GetHashCode() ?? 0);
            result = (result * 397) ^ (Document?.GetHashCode() ?? 0);
            return result;
        }
    }

    public static bool operator ==(Id left, Id right) => Equals(left, right);

    public static bool operator !=(Id left, Id right) => !Equals(left, right);
}
