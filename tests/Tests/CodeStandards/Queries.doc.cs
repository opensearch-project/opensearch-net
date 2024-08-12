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
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Core.Xunit;
using Tests.Framework;

namespace Tests.CodeStandards;

[ProjectReferenceOnly]
public class QueriesStandards
{
    protected static PropertyInfo[] QueryProperties = typeof(IQueryContainer).GetProperties();
    protected static PropertyInfo[] QueryPlaceHolderProperties = typeof(IQueryContainer).GetProperties()
        .Where(a => !a.GetCustomAttributes<IgnoreDataMemberAttribute>().Any()).ToArray();

    /*
		* All properties must be either marked with IgnoreDataMemberAttribute or DataMemberAttribute
		*/
    [U]
    public void InterfacePropertiesMustBeMarkedExplicitly()
    {
        var properties = from p in QueryProperties
                         let a = p.GetCustomAttributes<IgnoreDataMemberAttribute>()
                             .Concat<Attribute>(p.GetCustomAttributes<DataMemberAttribute>())
                         where a.Count() != 1
                         select p;
        properties.Should().BeEmpty();
    }

    [U]
    public void StaticQueryExposesAll()
    {
        var staticProperties = from p in typeof(Query<>).GetMethods()
                               let name = p.Name.StartsWith("GeoShape") ? "GeoShape" : p.Name
                               select name;

        var placeHolders = QueryPlaceHolderProperties.Select(p => p.Name.StartsWith("GeoShape") ? "GeoShape" : p.Name);
        staticProperties.Distinct().Should().Contain(placeHolders.Distinct());
    }

    [U]
    public void FluentDescriptorExposesAll()
    {
        var fluentMethods = from p in typeof(QueryContainerDescriptor<>).GetMethods()
                            let name = p.Name.StartsWith("GeoShape") ? "GeoShape" : p.Name
                            select name;

        var placeHolders = QueryPlaceHolderProperties.Select(p => p.Name.StartsWith("GeoShape") ? "GeoShape" : p.Name);
        fluentMethods.Distinct().Should().Contain(placeHolders.Distinct());
    }

    [U]
    public void VisitorVisitsAll()
    {
        var skipQueryImplementations = new[]
        {
            typeof(IFieldNameQuery),
            typeof(IFuzzyQuery<,>),
            typeof(IConditionlessQuery),
            typeof(ISpanGapQuery)
        };
        var queries = typeof(IQuery).Assembly.ExportedTypes
            .Where(t => t.IsInterface && typeof(IQuery).IsAssignableFrom(t))
            .Where(t => !skipQueryImplementations.Contains(t))
            .ToList();
        queries.Should().NotBeEmpty();

        var visitMethods = typeof(IQueryVisitor).GetMethods().Where(m => m.Name == "Visit");
        visitMethods.Should().NotBeEmpty();
        var missingTypes = from q in queries
                           let visitMethod = visitMethods.FirstOrDefault(m => m.GetParameters().First().ParameterType == q)
                           where visitMethod == null
                           select q;
        missingTypes.Should().BeEmpty();
    }
}
