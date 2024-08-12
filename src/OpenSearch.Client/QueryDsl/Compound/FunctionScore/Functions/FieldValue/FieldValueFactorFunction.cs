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
using System.Linq.Expressions;
using System.Runtime.Serialization;
using OpenSearch.Net.Utf8Json;


namespace OpenSearch.Client;

[InterfaceDataContract]
public interface IFieldValueFactorFunction : IScoreFunction
{
    [DataMember(Name = "factor")]
    double? Factor { get; set; }

    [DataMember(Name = "field")]
    Field Field { get; set; }

    [DataMember(Name = "missing")]
    double? Missing { get; set; }

    [DataMember(Name = "modifier")]
    FieldValueFactorModifier? Modifier { get; set; }
}

public class FieldValueFactorFunction : FunctionScoreFunctionBase, IFieldValueFactorFunction
{
    public double? Factor { get; set; }
    public Field Field { get; set; }

    public double? Missing { get; set; }

    public FieldValueFactorModifier? Modifier { get; set; }
}

public class FieldValueFactorFunctionDescriptor<T>
    : FunctionScoreFunctionDescriptorBase<FieldValueFactorFunctionDescriptor<T>, IFieldValueFactorFunction, T>, IFieldValueFactorFunction
    where T : class
{
    double? IFieldValueFactorFunction.Factor { get; set; }
    Field IFieldValueFactorFunction.Field { get; set; }
    double? IFieldValueFactorFunction.Missing { get; set; }
    FieldValueFactorModifier? IFieldValueFactorFunction.Modifier { get; set; }

    public FieldValueFactorFunctionDescriptor<T> Field(Field field) => Assign(field, (a, v) => a.Field = v);

    public FieldValueFactorFunctionDescriptor<T> Field<TValue>(Expression<Func<T, TValue>> field) => Assign(field, (a, v) => a.Field = v);

    public FieldValueFactorFunctionDescriptor<T> Factor(double? factor) => Assign(factor, (a, v) => a.Factor = v);

    public FieldValueFactorFunctionDescriptor<T> Modifier(FieldValueFactorModifier? modifier) => Assign(modifier, (a, v) => a.Modifier = v);

    public FieldValueFactorFunctionDescriptor<T> Missing(double? missing) => Assign(missing, (a, v) => a.Missing = v);
}
