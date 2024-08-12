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
using System.Linq.Expressions;
using System.Reflection;

namespace OpenSearch.Net.Extensions;

internal static class TypeExtensions
{
    public delegate T ObjectActivator<out T>(params object[] args);

    private static readonly MethodInfo GetActivatorMethodInfo =
        typeof(TypeExtensions).GetMethod(nameof(GetActivator), BindingFlags.Static | BindingFlags.NonPublic);

    private static readonly ConcurrentDictionary<string, ObjectActivator<object>> CachedActivators =
        new ConcurrentDictionary<string, ObjectActivator<object>>();

    internal static object CreateInstance(this Type t, params object[] args)
    {
        var argKey = args.Length;
        var key = argKey + "--" + t.FullName;
        if (CachedActivators.TryGetValue(key, out var activator))
            return activator(args);

        var generic = GetActivatorMethodInfo.MakeGenericMethod(t);
        var constructors = from c in t.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                           let p = c.GetParameters()
                           where p.Length == args.Length
                           select c;

        var ctor = constructors.FirstOrDefault() ?? throw new Exception($"Cannot create an instance of {t.FullName} because it has no constructor taking {args.Length} arguments");
        activator = (ObjectActivator<object>)generic.Invoke(null, new object[] { ctor });
        CachedActivators.TryAdd(key, activator);
        return activator(args);
    }

    //do not remove this is referenced through GetActivatorMethod
    private static ObjectActivator<T> GetActivator<T>(ConstructorInfo ctor)
    {
        var paramsInfo = ctor.GetParameters();

        //create a single param of type object[]
        var param = Expression.Parameter(typeof(object[]), "args");

        var argsExp = new Expression[paramsInfo.Length];

        //pick each arg from the params array
        //and create a typed expression of them
        for (var i = 0; i < paramsInfo.Length; i++)
        {
            var index = Expression.Constant(i);
            var paramType = paramsInfo[i].ParameterType;

            var paramAccessorExp = Expression.ArrayIndex(param, index);

            var paramCastExp = Expression.Convert(paramAccessorExp, paramType);

            argsExp[i] = paramCastExp;
        }

        //make a NewExpression that calls the
        //ctor with the args we just created
        var newExp = Expression.New(ctor, argsExp);

        //create a lambda with the New
        //Expression as body and our param object[] as arg
        var lambda = Expression.Lambda(typeof(ObjectActivator<T>), newExp, param);

        //compile it
        var compiled = (ObjectActivator<T>)lambda.Compile();
        return compiled;
    }

    private static readonly ConcurrentDictionary<Type, Func<object>> CachedDefaultValues =
        new ConcurrentDictionary<Type, Func<object>>();

    internal static object DefaultValue(this Type type) =>
        type.IsValueType
            ? CachedDefaultValues.GetOrAdd(type, t =>
                    Expression.Lambda<Func<object>>(
                            Expression.Convert(Expression.Default(type), typeof(object))
                        )
                        .Compile()
                )
                .Invoke()
            : null;
}
