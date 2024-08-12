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
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Common;
using OpenSearch.Client;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Framework;
// ReSharper disable SuggestVarOrType_Elsewhere
// ReSharper disable SuggestVarOrType_BuiltInTypes
// ReSharper disable SuggestVarOrType_SimpleTypes

namespace Tests.CodeStandards;

public class OpenSearchClientStandards
{
    /*
		* Fluent methods on IOpenSearchClient (Func<Descriptor, Interface>) should be named `selector`.
		*/
    [U]
    public void ConsistentFluentParameterNames()
    {
        var fluentParametersNotNamedSelector =
            from m in typeof(IOpenSearchClient).GetMethods()
            from p in m.GetParameters()
            where p.ParameterType.BaseType == typeof(MulticastDelegate)
            where !p.Name.Equals("selector") && !p.Name.Equals("mapper")
            select $"method '{nameof(IOpenSearchClient)}.{m.Name}' should have parameter name of 'selector' or 'mapper' but has a name of '{p.Name}'";

        fluentParametersNotNamedSelector.Should().BeEmpty();
    }

    /*
		* Similarly, OIS methods on IOpenSearchClient (IRequest) should be named `request`.
		*/
    [U]
    public void ConsistentInitializerParameterNames()
    {
        var requestParametersNotNamedRequest =
            from m in typeof(IOpenSearchClient).GetMethods()
            from p in m.GetParameters()
            where typeof(IRequest).IsAssignableFrom(p.ParameterType)
            where !p.Name.Equals("request")
            select $"method '{nameof(IOpenSearchClient)}.{m.Name}' should have parameter name of 'request' but has a name of '{p.Name}'";

        requestParametersNotNamedRequest.Should().BeEmpty();
    }

    /*
		* Request objects on OIS methods are always required, and so they shouldn't be nullable
		*/
    [U]
    public void InitializerRequestsAreNotOptional()
    {
        var requestParameters =
            (from m in typeof(IOpenSearchClient).GetMethods()
             from p in m.GetParameters()
             where typeof(IRequest).IsAssignableFrom(p.ParameterType)
             select p).ToList();

        foreach (var requestParameter in requestParameters)
            requestParameter.HasDefaultValue.Should().BeFalse();
    }

    //TODO ensure xml docs on all IOpenSearchClient methods

    /*
		* The parameter names to OpenSearchClient methods and whether they are optional should match
		* the interface method definitions
		*/
    [U]
    public void ConcreteClientOptionalParametersMatchInterfaceClient()
    {
        var concreteMethodParametersDoNotMatchInterface = new List<string>();
        var interfaceMap = typeof(OpenSearchClient).GetInterfaceMap(typeof(IOpenSearchClient));

        foreach (var interfaceMethodInfo in typeof(IOpenSearchClient).GetMethods())
        {
            var indexOfInterfaceMethod = Array.IndexOf(interfaceMap.InterfaceMethods, interfaceMethodInfo);
            var concreteMethod = interfaceMap.TargetMethods[indexOfInterfaceMethod];

            var concreteParameters = concreteMethod.GetParameters();
            var interfaceParameters = interfaceMethodInfo.GetParameters();

            for (var i = 0; i < concreteParameters.Length; i++)
            {
                var parameterInfo = concreteParameters[i];
                var interfaceParameter = interfaceParameters[i];

                parameterInfo.Name.Should().Be(
                    interfaceParameter.Name,
                    $"{nameof(OpenSearchClient)}.{interfaceMethodInfo.Name} should have parameter named {interfaceParameter.Name}");

                if (parameterInfo.HasDefaultValue != interfaceParameter.HasDefaultValue)
                    concreteMethodParametersDoNotMatchInterface.Add(
                        $"'{interfaceParameter.Name}' parameter on concrete implementation of '{nameof(OpenSearchClient)}.{interfaceMethodInfo.Name}' to {(interfaceParameter.HasDefaultValue ? string.Empty : "NOT")} be optional");
            }
        }

        concreteMethodParametersDoNotMatchInterface.Should().BeEmpty();
    }

    /*
		* Synchronous and Asynchronous methods should be consistent
		* with regards to optional parameters
		*/
    [U]
    public void ConsistentOptionalParametersForSyncAndAsyncMethods()
    {
        var methodGroups =
            from methodInfo in typeof(IOpenSearchClient).GetMethods()
            where
                typeof(IResponse).IsAssignableFrom(methodInfo.ReturnType) ||
                (methodInfo.ReturnType.IsGenericType
                 && typeof(Task<>) == methodInfo.ReturnType.GetGenericTypeDefinition()
                 && typeof(IResponse).IsAssignableFrom(methodInfo.ReturnType.GetGenericArguments()[0]))
            where !methodInfo.Name.Contains("CreateDocument")
            where !methodInfo.Name.Contains("IndexDocument")
            let method = new MethodWithRequestParameter(methodInfo)
            group method by method.Name into methodGroup
            select methodGroup;

        foreach (var methodGroup in methodGroups)
        {
            foreach (var asyncMethod in methodGroup.Where(g => g.IsAsync))
            {
                var parameters = asyncMethod.MethodInfo.GetParameters().Where(p => p.ParameterType != typeof(CancellationToken)).ToArray();

                var syncMethod = methodGroup.First(g =>
                    !g.IsAsync
                    && g.MethodType == asyncMethod.MethodType
                    && g.MethodInfo.GetParameters().Length == parameters.Length
                    && (!asyncMethod.MethodInfo.IsGenericMethod ||
                        g.MethodInfo.GetGenericArguments().Length == asyncMethod.MethodInfo.GetGenericArguments().Length));

                asyncMethod.Parameter.HasDefaultValue.Should().Be(syncMethod.Parameter.HasDefaultValue,
                    $"sync and async versions of {asyncMethod.MethodType} '{nameof(OpenSearchClient)}{methodGroup.Key}' should match");
            }
        }
    }

    private enum ClientMethodType
    {
        Fluent,
        Initializer
    }

    private class MethodWithRequestParameter
    {
        public string Name { get; }

        public MethodInfo MethodInfo { get; }

        public bool IsAsync { get; }

        public ClientMethodType MethodType { get; }

        public ParameterInfo Parameter { get; }

        public MethodWithRequestParameter(MethodInfo methodInfo)
        {
            Name = methodInfo.Name.EndsWith("Async")
                ? methodInfo.Name.Substring(0, methodInfo.Name.Length - "Async".Length)
                : methodInfo.Name;

            IsAsync = methodInfo.ReturnType.IsGenericType &&
                      methodInfo.ReturnType.GetGenericTypeDefinition() == typeof(Task<>);

            MethodInfo = methodInfo;

            var parameterInfo = methodInfo.GetParameters()
                .FirstOrDefault(p => typeof(IRequest).IsAssignableFrom(p.ParameterType));

            if (parameterInfo != null)
            {
                Parameter = parameterInfo;
                MethodType = ClientMethodType.Initializer;
            }
            else
            {
                Parameter = methodInfo.GetParameters()
                    .First(p => p.ParameterType.BaseType == typeof(MulticastDelegate));
                MethodType = ClientMethodType.Fluent;
            }
        }
    }
}
