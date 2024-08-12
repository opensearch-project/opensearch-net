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
using System.Linq.Expressions;
using System.Reflection;
using OpenSearch.Net;
using OpenSearch.Net.Utf8Json;

namespace OpenSearch.Client;

[JsonFormatter(typeof(FieldFormatter))]
[DebuggerDisplay("{DebugDisplay,nq}")]
public class Field : IEquatable<Field>, IUrlParameter
{
    private readonly object _comparisonValue;
    private readonly Type _type;

    public Field(string name, double? boost = null, string format = null)
    {
        name.ThrowIfNullOrEmpty(nameof(name));
        Name = ParseFieldName(name, out var b);
        Boost = b ?? boost;
        Format = format;
        _comparisonValue = Name;
    }

    public Field(Expression expression, double? boost = null, string format = null)
    {
        Expression = expression ?? throw new ArgumentNullException(nameof(expression));
        Boost = boost;
        Format = format;
        _comparisonValue = expression.ComparisonValueFromExpression(out var type, out var cachable);
        _type = type;
        CachableExpression = cachable;
    }

    public Field(PropertyInfo property, double? boost = null, string format = null)
    {
        Property = property ?? throw new ArgumentNullException(nameof(property));
        Boost = boost;
        Format = format;
        _comparisonValue = property;
        _type = property.DeclaringType;
    }

    /// <summary>
    /// A boost to apply to the field
    /// </summary>
    public double? Boost { get; set; }

    /// <summary>
    /// A format to apply to the field.
    /// </summary>
    public string Format { get; set; }

    public bool CachableExpression { get; }

    /// <summary>
    /// An expression from which the name of the field can be inferred
    /// </summary>
    public Expression Expression { get; }

    /// <summary>
    /// The name of the field
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// A property from which the name of the field can be inferred
    /// </summary>
    public PropertyInfo Property { get; }

    internal string DebugDisplay =>
        $"{Expression?.ToString() ?? PropertyDebug ?? Name}{(Boost.HasValue ? "^" + Boost.Value : string.Empty)}"
        + $"{(!string.IsNullOrEmpty(Format) ? " format: " + Format : string.Empty)}"
        + $"{(_type == null ? string.Empty : " typeof: " + _type.Name)}";

    public override string ToString() => DebugDisplay;

    private string PropertyDebug => Property == null ? null : $"PropertyInfo: {Property.Name}";

    public bool Equals(Field other) => _type != null
        ? other != null && _type == other._type && _comparisonValue.Equals(other._comparisonValue)
        : other != null && _comparisonValue.Equals(other._comparisonValue);

    string IUrlParameter.GetString(IConnectionConfigurationValues settings)
    {
        if (!(settings is IConnectionSettingsValues oscSettings))
            throw new ArgumentNullException(nameof(settings),
                $"Can not resolve {nameof(Field)} if no {nameof(IConnectionSettingsValues)} is provided");

        return oscSettings.Inferrer.Field(this);
    }

    public Fields And(Field field) => new Fields(new[] { this, field });

    public Fields And<T, TValue>(Expression<Func<T, TValue>> field, double? boost = null, string format = null) where T : class =>
        new Fields(new[] { this, new Field(field, boost, format) });

    public Fields And<T>(Expression<Func<T, object>> field, double? boost = null, string format = null) where T : class =>
        new Fields(new[] { this, new Field(field, boost, format) });

    public Fields And(string field, double? boost = null, string format = null) =>
        new Fields(new[] { this, new Field(field, boost, format) });

    public Fields And(PropertyInfo property, double? boost = null, string format = null) =>
        new Fields(new[] { this, new Field(property, boost, format) });

    private static string ParseFieldName(string name, out double? boost)
    {
        boost = null;
        if (name == null) return null;

        var caretIndex = name.IndexOf('^');
        if (caretIndex == -1)
            return name;

        var parts = name.Split(new[] { '^' }, 2, StringSplitOptions.RemoveEmptyEntries);
        name = parts[0];
        boost = double.Parse(parts[1], CultureInfo.InvariantCulture);
        return name;
    }

    public static implicit operator Field(string name) => name.IsNullOrEmpty() ? null : new Field(name);

    public static implicit operator Field(Expression expression) => expression == null ? null : new Field(expression);

    public static implicit operator Field(PropertyInfo property) => property == null ? null : new Field(property);

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = _comparisonValue?.GetHashCode() ?? 0;
            hashCode = (hashCode * 397) ^ (_type?.GetHashCode() ?? 0);
            return hashCode;
        }
    }

    public override bool Equals(object obj)
    {
        switch (obj)
        {
            case string s: return Equals(s);
            case PropertyInfo p: return Equals(p);
            case Field f: return Equals(f);
            default: return false;
        }
    }

    public static bool operator ==(Field x, Field y) => Equals(x, y);

    public static bool operator !=(Field x, Field y) => !Equals(x, y);
}
