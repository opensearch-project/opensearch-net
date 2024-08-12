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
using System.Reflection;

namespace OpenSearch.Client;

public class IdResolver
{
    private static readonly ConcurrentDictionary<Type, Func<object, string>> IdDelegates = new ConcurrentDictionary<Type, Func<object, string>>();

    private static readonly MethodInfo MakeDelegateMethodInfo =
        typeof(IdResolver).GetMethod(nameof(MakeDelegate), BindingFlags.Static | BindingFlags.NonPublic);

    private readonly IConnectionSettingsValues _connectionSettings;
    private readonly ConcurrentDictionary<Type, Func<object, string>> _localIdDelegates = new ConcurrentDictionary<Type, Func<object, string>>();

    public IdResolver(IConnectionSettingsValues connectionSettings) => _connectionSettings = connectionSettings;

    private PropertyInfo GetPropertyCaseInsensitive(Type type, string fieldName)
        => type.GetProperty(fieldName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

    internal Func<T, string> CreateIdSelector<T>() where T : class
    {
        Func<T, string> idSelector = Resolve;
        return idSelector;
    }

    internal static Func<object, object> MakeDelegate<T, TReturn>(MethodInfo @get)
    {
        var f = (Func<T, TReturn>)@get.CreateDelegate(typeof(Func<T, TReturn>));
        return t => f((T)t);
    }

    public string Resolve<T>(T @object) where T : class =>
        _connectionSettings.DefaultDisableIdInference || @object == null ? null : Resolve(@object.GetType(), @object);

    public string Resolve(Type type, object @object)
    {
        if (type == null || @object == null) return null;
        if (_connectionSettings.DefaultDisableIdInference || _connectionSettings.DisableIdInference.Contains(type))
            return null;

        var preferLocal = _connectionSettings.IdProperties.TryGetValue(type, out _);

        if (_localIdDelegates.TryGetValue(type, out var cachedLookup))
            return cachedLookup(@object);

        if (!preferLocal && IdDelegates.TryGetValue(type, out cachedLookup))
            return cachedLookup(@object);

        var idProperty = GetInferredId(type);
        if (idProperty == null) return null;

        var getMethod = idProperty.GetMethod;
        var generic = MakeDelegateMethodInfo.MakeGenericMethod(type, getMethod.ReturnType);
        var func = (Func<object, object>)generic.Invoke(null, new object[] { getMethod });
        cachedLookup = o =>
        {
            var v = func(o);
            return v?.ToString();
        };
        if (preferLocal)
            _localIdDelegates.TryAdd(type, cachedLookup);
        else
            IdDelegates.TryAdd(type, cachedLookup);
        return cachedLookup(@object);
    }


    private PropertyInfo GetInferredId(Type type)
    {
        // if the type specifies through OpenSearchAttribute what the id prop is
        // use that no matter what

        _connectionSettings.IdProperties.TryGetValue(type, out var propertyName);
        if (!propertyName.IsNullOrEmpty())
            return GetPropertyCaseInsensitive(type, propertyName);

        var opensearchTypeAtt = OpenSearchTypeAttribute.From(type);
        propertyName = opensearchTypeAtt?.IdProperty.IsNullOrEmpty() ?? true ? "Id" : opensearchTypeAtt.IdProperty;

        return GetPropertyCaseInsensitive(type, propertyName);
    }
}
