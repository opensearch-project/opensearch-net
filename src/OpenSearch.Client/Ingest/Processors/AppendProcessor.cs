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
using System.Linq.Expressions;
using System.Runtime.Serialization;
using OpenSearch.Net.Utf8Json;

namespace OpenSearch.Client;

[InterfaceDataContract]
public interface IAppendProcessor : IProcessor
{
    [DataMember(Name = "field")]
    Field Field { get; set; }

    [DataMember(Name = "value")]
    IEnumerable<object> Value { get; set; }

    [DataMember(Name = "allow_duplicates")]
    bool? AllowDuplicates { get; set; }
}

public class AppendProcessor : ProcessorBase, IAppendProcessor
{
    public Field Field { get; set; }
    public IEnumerable<object> Value { get; set; }
    public bool? AllowDuplicates { get; set; }

    protected override string Name => "append";
}

public class AppendProcessorDescriptor<T> : ProcessorDescriptorBase<AppendProcessorDescriptor<T>, IAppendProcessor>, IAppendProcessor
    where T : class
{
    protected override string Name => "append";
    Field IAppendProcessor.Field { get; set; }
    IEnumerable<object> IAppendProcessor.Value { get; set; }
    bool? IAppendProcessor.AllowDuplicates { get; set; }

    public AppendProcessorDescriptor<T> Field(Field field) => Assign(field, (a, v) => a.Field = v);

    public AppendProcessorDescriptor<T> Field<TValue>(Expression<Func<T, TValue>> objectPath) =>
        Assign(objectPath, (a, v) => a.Field = v);

    public AppendProcessorDescriptor<T> Value<TValue>(IEnumerable<TValue> values) => Assign(values, (a, v) => a.Value = v?.Cast<object>());

    public AppendProcessorDescriptor<T> Value<TValue>(params TValue[] values) => Assign(values, (a, v) =>
    {
        if (v?.Length == 1 && typeof(IEnumerable).IsAssignableFrom(typeof(TValue)) && typeof(TValue) != typeof(string))
            a.Value = (v.First() as IEnumerable)?.Cast<object>();
        else a.Value = v?.Cast<object>();
    });

    public AppendProcessorDescriptor<T> AllowDuplicates(bool? allowDuplicates = true) => Assign(allowDuplicates, (a, v) => a.AllowDuplicates = v);
}
