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
using System.Runtime.Serialization;
using OpenSearch.Net.Utf8Json;

namespace OpenSearch.Client;

[InterfaceDataContract]
[ReadAs(typeof(Rescore))]
public interface IRescore
{
    [DataMember(Name = "query")]
    IRescoreQuery Query { get; set; }

    [DataMember(Name = "window_size")]
    int? WindowSize { get; set; }
}

public class Rescore : IRescore
{
    public IRescoreQuery Query { get; set; }
    public int? WindowSize { get; set; }
}

public class RescoringDescriptor<T> : DescriptorPromiseBase<RescoringDescriptor<T>, IList<IRescore>>
    where T : class
{
    public RescoringDescriptor() : base(new List<IRescore>()) { }

    public RescoringDescriptor<T> Rescore(Func<RescoreDescriptor<T>, IRescore> selector) =>
        AddRescore(selector?.Invoke(new RescoreDescriptor<T>()));

    private RescoringDescriptor<T> AddRescore(IRescore rescore) => rescore == null ? this : Assign(rescore, (a, v) => a.Add(v));
}

public class RescoreDescriptor<T> : DescriptorBase<RescoreDescriptor<T>, IRescore>, IRescore
    where T : class
{
    IRescoreQuery IRescore.Query { get; set; }
    int? IRescore.WindowSize { get; set; }

    public virtual RescoreDescriptor<T> RescoreQuery(Func<RescoreQueryDescriptor<T>, IRescoreQuery> rescoreQuerySelector) =>
        Assign(rescoreQuerySelector, (a, v) => a.Query = v?.Invoke(new RescoreQueryDescriptor<T>()));

    public virtual RescoreDescriptor<T> WindowSize(int? windowSize) => Assign(windowSize, (a, v) => a.WindowSize = v);
}
