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
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;

namespace OpenSearch.Client;

public class RoutingResolver
{
    private static readonly ConcurrentDictionary<Type, Func<object, JoinField>> PropertyGetDelegates =
        new ConcurrentDictionary<Type, Func<object, JoinField>>();

    private static readonly MethodInfo MakeDelegateMethodInfo =
        typeof(RoutingResolver).GetMethod(nameof(MakeDelegate), BindingFlags.Static | BindingFlags.NonPublic);


    private readonly IConnectionSettingsValues _connectionSettings;
    private readonly IdResolver _idResolver;

    private readonly ConcurrentDictionary<Type, Func<object, string>>
        _localRouteDelegates = new ConcurrentDictionary<Type, Func<object, string>>();

    public RoutingResolver(IConnectionSettingsValues connectionSettings, IdResolver idResolver)
    {
        _connectionSettings = connectionSettings;
        _idResolver = idResolver;
    }

    private PropertyInfo GetPropertyCaseInsensitive(Type type, string fieldName) =>
        type.GetProperty(fieldName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

    internal static Func<object, object> MakeDelegate<T, TReturn>(MethodInfo @get)
    {
        var f = (Func<T, TReturn>)@get.CreateDelegate(typeof(Func<T, TReturn>));
        return t => f((T)t);
    }

    public string Resolve<T>(T @object) => @object == null ? null : Resolve(@object.GetType(), @object);

    public string Resolve(Type type, object @object)
    {
        if (TryConnectionSettingsRoute(type, @object, out var route)) return route;

        var joinField = GetJoinFieldFromObject(type, @object);
        return joinField?.Match(p => _idResolver.Resolve(@object), c => ResolveId(c.ParentId, _connectionSettings));
    }

    private bool TryConnectionSettingsRoute(Type type, object @object, out string route)
    {
        route = null;
        if (!_connectionSettings.RouteProperties.TryGetValue(type, out var propertyName))
            return false;

        if (_localRouteDelegates.TryGetValue(type, out var cachedLookup))
        {
            route = cachedLookup(@object);
            return true;
        }
        var property = GetPropertyCaseInsensitive(type, propertyName);
        var func = CreateGetterFunc(type, property);
        cachedLookup = o =>
        {
            var v = func(o);
            return v?.ToString();
        };
        _localRouteDelegates.TryAdd(type, cachedLookup);
        route = cachedLookup(@object);
        return true;
    }

    private string ResolveId(Id id, IConnectionSettingsValues oscSettings) =>
        id.Document != null ? oscSettings.Inferrer.Id(id.Document) : id.StringOrLongValue;

    private static JoinField GetJoinFieldFromObject(Type type, object @object)
    {
        if (type == null || @object == null) return null;

        if (PropertyGetDelegates.TryGetValue(type, out var cachedLookup)) return cachedLookup(@object);

        var joinProperty = GetJoinFieldProperty(type);
        if (joinProperty == null)
        {
            PropertyGetDelegates.TryAdd(type, o => null);
            return null;
        }

        var func = CreateGetterFunc(type, joinProperty);
        cachedLookup = o =>
        {
            var v = func(o);
            return v as JoinField;
        };
        PropertyGetDelegates.TryAdd(type, cachedLookup);
        return cachedLookup(@object);
    }

    private static Func<object, object> CreateGetterFunc(Type type, PropertyInfo joinProperty)
    {
        var getMethod = joinProperty.GetMethod;
        var generic = MakeDelegateMethodInfo.MakeGenericMethod(type, getMethod.ReturnType);
        var func = (Func<object, object>)generic.Invoke(null, new object[] { getMethod });
        return func;
    }

    private static PropertyInfo GetJoinFieldProperty(Type type)
    {
        var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
        try
        {
            var joinField = properties.SingleOrDefault(p => p.PropertyType == typeof(JoinField));
            return joinField;
        }
        catch (InvalidOperationException e)
        {
            throw new ArgumentException($"{type.Name} has more than one JoinField property", e);
        }
    }
}
