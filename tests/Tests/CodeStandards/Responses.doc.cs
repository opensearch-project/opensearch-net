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
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.Net;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Framework;

namespace Tests.CodeStandards;

public class Responses
{
    /**
		* Every collection property on a Response type should be either IReadOnlyCollection or IReadOnlyDictionary
		*/
    [U]
    public void ResponsePropertyCollectionsShouldBeReadOnly()
    {
        var exceptions = new HashSet<PropertyInfo>
        {
            typeof(ITypeMapping).GetProperty(nameof(ITypeMapping.DynamicDateFormats)),
            typeof(ITypeMapping).GetProperty(nameof(ITypeMapping.Meta)),
            typeof(TypeMapping).GetProperty(nameof(TypeMapping.DynamicDateFormats)),
            typeof(TypeMapping).GetProperty(nameof(TypeMapping.Meta)),
            typeof(DynamicDictionary).GetProperty(nameof(DynamicDictionary.Keys)),
            typeof(DynamicDictionary).GetProperty(nameof(DynamicDictionary.Values)),
            typeof(BulkResponse).GetProperty(nameof(BulkResponse.ItemsWithErrors)),
            typeof(FieldCapabilitiesResponse).GetProperty(nameof(FieldCapabilitiesResponse.Fields)),
            typeof(MultiSearchResponse).GetProperty(nameof(MultiSearchResponse.AllResponses)),
            typeof(DynamicDictionary).GetProperty(nameof(DynamicDictionary.Keys)),
            typeof(DynamicDictionary).GetProperty(nameof(DynamicDictionary.Values)),
        };

        var responseInterfaceTypes = from t in typeof(IResponse).Assembly.Types()
                                     where t.IsInterface && typeof(IResponse).IsAssignableFrom(t)
                                     select t;

        var ruleBreakers = new List<string>();
        var seenTypes = new HashSet<Type>();
        foreach (var responseType in responseInterfaceTypes)
        {
            FindPropertiesBreakingRule(responseType, exceptions, seenTypes, ruleBreakers);
        }

        ruleBreakers.Should().BeEmpty();
    }

    [U]
    public void ResponsesShouldNotHaveInterfaceUnlessThatInterfaceIsCovariant()
    {
        var responses = from t in typeof(IResponse).Assembly.ExportedTypes
                        where t.IsClass && typeof(IResponse).IsAssignableFrom(t)
                        select t;

        var offenders = new List<string>();
        foreach (var r in responses)
        {
            var interfaces = r.GetInterfaces();
            var sameNamedInterface = interfaces.FirstOrDefault(i => i.Name.StartsWith("I" + r.Name));
            if (sameNamedInterface != null)
            {
                if (!sameNamedInterface.IsGenericType)
                {
                    offenders.Add(sameNamedInterface.Name + " is not generic and thus can not be an allow covariant interface");
                    continue;
                }
                else
                {
                    var generic = sameNamedInterface.GetGenericTypeDefinition();
                    var genericArg = generic.GetGenericArguments()
                        .FirstOrDefault(a => a.GenericParameterAttributes.HasFlag(GenericParameterAttributes.Covariant));
                    if (genericArg == null)
                        offenders.Add(sameNamedInterface.Name + " is generic but not of its type arguments are covariant");
                }
            }
        }
        offenders.Should().BeEmpty("Responses may only have a same named interface if that interface is used to provide covariance");


    }


    private static readonly Type[] ResponseDictionaries = { typeof(AggregateDictionary) };

    private static void FindPropertiesBreakingRule(Type type, HashSet<PropertyInfo> exceptions, HashSet<Type> seenTypes, List<string> ruleBreakers)
    {
        if (!seenTypes.Add(type)) return;

        var properties = type.GetProperties();
        foreach (var propertyInfo in properties)
        {
            if (exceptions.Contains(propertyInfo))
                continue;

            if (typeof(IDictionary).IsAssignableFrom(propertyInfo.PropertyType) ||
                typeof(ICollection).IsAssignableFrom(propertyInfo.PropertyType))
            {
                ruleBreakers.Add($"{type.FullName}.{propertyInfo.Name} is of type {propertyInfo.PropertyType.Name}");
            }
            else if (propertyInfo.PropertyType.IsGenericType)
            {
                var genericTypeDefinition = propertyInfo.PropertyType.GetGenericTypeDefinition();
                if (genericTypeDefinition == typeof(IDictionary<,>) ||
                    genericTypeDefinition == typeof(Dictionary<,>) ||
                    genericTypeDefinition == typeof(IEnumerable<>) ||
                    genericTypeDefinition == typeof(IList<>) ||
                    genericTypeDefinition == typeof(ICollection<>))
                {
                    ruleBreakers.Add($"{type.FullName}.{propertyInfo.Name} is of type {propertyInfo.PropertyType.Name}");
                }
            }
            else if (propertyInfo.PropertyType.IsClass &&
                     (propertyInfo.PropertyType.Namespace.StartsWith("OpenSearch.Client") || propertyInfo.PropertyType.Namespace.StartsWith("OpenSearch.Net"))
                     //Do not traverse known response dictionaries
                     && !ResponseDictionaries.Contains(propertyInfo.PropertyType)
            )
            {
                FindPropertiesBreakingRule(propertyInfo.PropertyType, exceptions, seenTypes, ruleBreakers);
            }
        }
    }
}
