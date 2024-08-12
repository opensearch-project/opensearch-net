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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using OpenSearch.Net;
using OpenSearch.Net.Utf8Json;

namespace OpenSearch.Client;

[JsonFormatter(typeof(FieldsFormatter))]
[DebuggerDisplay("{DebugDisplay,nq}")]
public class Fields : IUrlParameter, IEnumerable<Field>, IEquatable<Fields>
{
    internal readonly List<Field> ListOfFields;

    internal Fields() => ListOfFields = new List<Field>();

    internal Fields(IEnumerable<Field> fieldNames) => ListOfFields = fieldNames.ToList();

    private string DebugDisplay =>
        $"Count: {ListOfFields.Count} [" + string.Join(",", ListOfFields.Select((t, i) => $"({i + 1}: {t?.DebugDisplay ?? "NULL"})")) + "]";

    public override string ToString() => DebugDisplay;

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public IEnumerator<Field> GetEnumerator() => ListOfFields.GetEnumerator();

    public bool Equals(Fields other) => EqualsAllFields(ListOfFields, other.ListOfFields);

    string IUrlParameter.GetString(IConnectionConfigurationValues settings)
    {
        if (!(settings is IConnectionSettingsValues oscSettings))
            throw new ArgumentNullException(nameof(settings),
                $"Can not resolve {nameof(Fields)} if no {nameof(IConnectionSettingsValues)} is provided");

        return string.Join(",", ListOfFields.Where(f => f != null).Select(f => ((IUrlParameter)f).GetString(oscSettings)));
    }

    public static implicit operator Fields(string[] fields) => fields.IsEmpty() ? null : new Fields(fields.Select(f => new Field(f)));

    public static implicit operator Fields(string field) => field.IsNullOrEmptyCommaSeparatedList(out var split)
        ? null
        : new Fields(split.Select(f => new Field(f)));

    public static implicit operator Fields(Expression[] fields) => fields.IsEmpty() ? null : new Fields(fields.Select(f => new Field(f)));

    public static implicit operator Fields(Expression field) => field == null ? null : new Fields(new[] { new Field(field) });

    public static implicit operator Fields(Field field) => field == null ? null : new Fields(new[] { field });

    public static implicit operator Fields(PropertyInfo field) => field == null ? null : new Fields(new Field[] { field });

    public static implicit operator Fields(PropertyInfo[] fields) => fields.IsEmpty() ? null : new Fields(fields.Select(f => new Field(f)));

    public static implicit operator Fields(Field[] fields) => fields.IsEmpty() ? null : new Fields(fields);

    public Fields And<T, TValue>(Expression<Func<T, TValue>> field, double? boost = null, string format = null) where T : class
    {
        ListOfFields.Add(new Field(field, boost, format));
        return this;
    }

    public Fields And(string field, double? boost = null, string format = null)
    {
        ListOfFields.Add(new Field(field, boost, format));
        return this;
    }

    public Fields And(PropertyInfo property, double? boost = null)
    {
        ListOfFields.Add(new Field(property, boost));
        return this;
    }

    public Fields And<T>(params Expression<Func<T, object>>[] fields) where T : class
    {
        ListOfFields.AddRange(fields.Select(f => new Field(f)));
        return this;
    }

    public Fields And(params string[] fields)
    {
        ListOfFields.AddRange(fields.Select(f => new Field(f)));
        return this;
    }

    public Fields And(params PropertyInfo[] properties)
    {
        ListOfFields.AddRange(properties.Select(f => new Field(f)));
        return this;
    }

    public Fields And(params Field[] fields)
    {
        ListOfFields.AddRange(fields);
        return this;
    }

    public static bool operator ==(Fields left, Fields right) => Equals(left, right);

    public static bool operator !=(Fields left, Fields right) => !Equals(left, right);

    public override bool Equals(object obj)
    {
        switch (obj)
        {
            case Fields f: return Equals(f);
            case string s: return Equals(s);
            case Field fn: return Equals(fn);
            case Field[] fns: return Equals(fns);
            case Expression e: return Equals(e);
            case Expression[] opensearch: return Equals(opensearch);
            default: return false;
        }
    }

    private static bool EqualsAllFields(IReadOnlyList<Field> thisTypes, IReadOnlyList<Field> otherTypes)
    {
        if (thisTypes == null && otherTypes == null) return true;
        if (thisTypes == null || otherTypes == null) return false;
        if (thisTypes.Count != otherTypes.Count) return false;

        return thisTypes.Count == otherTypes.Count && !thisTypes.Except(otherTypes).Any();
    }

    public override int GetHashCode() => ListOfFields.GetHashCode();
}
