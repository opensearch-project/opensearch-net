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
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;

namespace Tests.CodeStandards.Parity;

public class ParityTests
{
    [U]
    public void FieldTypeHasAllNumberTypes()
    {
        var numberTypes = Enum.GetNames(typeof(NumberType));
        var fieldTypes = Enum.GetNames(typeof(FieldType));

        fieldTypes.Should().Contain(numberTypes);
    }

    [U]
    public void PropertyVisitorHasVisitMethodForAllPropertyTypes()
    {
        var interfaceType = typeof(IProperty);

        var excludeInterfaceTypes = new HashSet<Type>
        {
            interfaceType,
            typeof(IDocValuesProperty),
            typeof(ICoreProperty),
            typeof(IRangeProperty),
            typeof(IGenericProperty)
        };

        var propertyTypes = interfaceType.Assembly
            .GetTypes()
            .Where(t => t.IsInterface && interfaceType.IsAssignableFrom(t) && !excludeInterfaceTypes.Contains(t));

        var visitMethodTypes = typeof(IPropertyVisitor).GetMethods()
            .Where(m => m.ReturnType == typeof(void))
            .Select(m => m.GetParameters()[0].ParameterType);

        propertyTypes.Except(visitMethodTypes).Should().BeEmpty();
    }
}
