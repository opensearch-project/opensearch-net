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
using System.Collections.Concurrent;
using System.Reflection;
using OpenSearch.Net;

namespace OpenSearch.Client;

public class PropertyWalker
{
    private readonly int _maxRecursion;
    private readonly ConcurrentDictionary<Type, int> _seenTypes;
    private readonly Type _type;
    private readonly IPropertyVisitor _visitor;

    public PropertyWalker(Type type, IPropertyVisitor visitor, int maxRecursion = 0)
    {
        _type = GetUnderlyingType(type);
        _visitor = visitor ?? new NoopPropertyVisitor();
        _maxRecursion = maxRecursion;
        _seenTypes = new ConcurrentDictionary<Type, int>();
        _seenTypes.TryAdd(_type, 0);
    }

    private PropertyWalker(Type type, IPropertyVisitor visitor, int maxRecursion, ConcurrentDictionary<Type, int> seenTypes)
    {
        _type = type;
        _visitor = visitor;
        _maxRecursion = maxRecursion;
        _seenTypes = seenTypes;
    }

    public IProperties GetProperties(ConcurrentDictionary<Type, int> seenTypes = null, int maxRecursion = 0)
    {
        var properties = new Properties();

        if (seenTypes != null && seenTypes.TryGetValue(_type, out var seen) && seen > maxRecursion)
            return properties;

        foreach (var propertyInfo in _type.GetAllProperties())
        {
            var attribute = OpenSearchPropertyAttributeBase.From(propertyInfo);
            if (attribute != null && attribute.Ignore) continue;
            if (_visitor.SkipProperty(propertyInfo, attribute)) continue;

            var property = GetProperty(propertyInfo, attribute);
            if (property is IPropertyWithClrOrigin withCLrOrigin)
                withCLrOrigin.ClrOrigin = propertyInfo;
            properties.Add(propertyInfo, property);
        }

        return properties;
    }

    private IProperty GetProperty(PropertyInfo propertyInfo, OpenSearchPropertyAttributeBase attribute)
    {
        var property = _visitor.Visit(propertyInfo, attribute);
        if (property != null) return property;

        if (propertyInfo.GetMethod.IsStatic)
            return null;

        property = attribute ?? InferProperty(propertyInfo);

        if (property is IObjectProperty objectProperty)
        {
            var type = GetUnderlyingType(propertyInfo.PropertyType);
            var seenTypes = new ConcurrentDictionary<Type, int>(_seenTypes);
            seenTypes.AddOrUpdate(type, 0, (t, i) => ++i);
            var walker = new PropertyWalker(type, _visitor, _maxRecursion, seenTypes);
            objectProperty.Properties = walker.GetProperties(seenTypes, _maxRecursion);
        }

        _visitor.Visit(property, propertyInfo, attribute);

        return property;
    }

    private static IProperty InferProperty(PropertyInfo propertyInfo)
    {
        var type = GetUnderlyingType(propertyInfo.PropertyType);

        if (type == typeof(string))
            return new TextProperty
            {
                Fields = new Properties
                {
                    {
                        "keyword", new KeywordProperty
                        {
                            IgnoreAbove = 256
                        }
                    }
                }
            };

        if (type.IsEnum)
        {
            if (type.GetCustomAttribute<StringEnumAttribute>() != null
                || propertyInfo.GetCustomAttribute<StringEnumAttribute>() != null)
                return new KeywordProperty();

            return new NumberProperty(NumberType.Integer);
        }

        if (type.IsValueType)
        {
            switch (type.Name)
            {
                case "Int32":
                case "UInt16":
                    return new NumberProperty(NumberType.Integer);
                case "Int16":
                case "Byte":
                    return new NumberProperty(NumberType.Short);
                case "SByte":
                    return new NumberProperty(NumberType.Byte);
                case "Int64":
                case "UInt32":
                case "TimeSpan":
                    return new NumberProperty(NumberType.Long);
                case "Single":
                    return new NumberProperty(NumberType.Float);
                case "Decimal":
                case "Double":
                case "UInt64":
                    return new NumberProperty(NumberType.Double);
                case "DateTime":
                case "DateTimeOffset":
                    return new DateProperty();
                case "Boolean":
                    return new BooleanProperty();
                case "Char":
                case "Guid":
                    return new KeywordProperty();
            }
        }

        if (type == typeof(GeoLocation))
            return new GeoPointProperty();

        if (type == typeof(CompletionField))
            return new CompletionProperty();

        if (type == typeof(DateRange))
            return new DateRangeProperty();

        if (type == typeof(DoubleRange))
            return new DoubleRangeProperty();

        if (type == typeof(FloatRange))
            return new FloatRangeProperty();

        if (type == typeof(IntegerRange))
            return new IntegerRangeProperty();

        if (type == typeof(LongRange))
            return new LongRangeProperty();

        if (type == typeof(IpAddressRange))
            return new IpRangeProperty();

        if (type == typeof(QueryContainer))
            return new PercolatorProperty();

        if (type == typeof(IGeoShape))
            return new GeoShapeProperty();

        return new ObjectProperty();
    }

    /// <summary>
    /// Gets the underlying type when the type is a generic collection that implements <see cref="IEnumerable"/> or a nullable type
    /// </summary>
    private static Type GetUnderlyingType(Type type)
    {
        if (type.IsArray)
            return type.GetElementType();

        if (ShouldUnwrapType(type))
        {
            var returnType = type.GetGenericArguments()[0];
            if (ShouldUnwrapType(returnType)) // This is needed for types like IEnumerable<int?>.
            {
                return returnType.GetGenericArguments()[0];
            }
            return returnType;
        }

        return type;
    }

    private static bool ShouldUnwrapType(Type ty) =>
                ty.IsGenericType &&
                ty.GetGenericArguments().Length == 1 &&
                (
                    ty.GetInterfaces().HasAny(t => t == typeof(IEnumerable)) ||
                    Nullable.GetUnderlyingType(ty) != null
                );
}
