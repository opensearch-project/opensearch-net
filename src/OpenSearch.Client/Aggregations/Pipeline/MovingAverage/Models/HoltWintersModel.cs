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

using System.Runtime.Serialization;
using OpenSearch.Net;
using OpenSearch.Net.Utf8Json;

namespace OpenSearch.Client;

[StringEnum]
public enum HoltWintersType
{
    [EnumMember(Value = "add")]
    Additive,

    [EnumMember(Value = "mult")]
    Multiplicative
}

[InterfaceDataContract]
[ReadAs(typeof(HoltWintersModel))]
public interface IHoltWintersModel : IMovingAverageModel
{
    [DataMember(Name = "alpha")]
    float? Alpha { get; set; }

    [DataMember(Name = "beta")]
    float? Beta { get; set; }

    [DataMember(Name = "gamma")]
    float? Gamma { get; set; }

    [DataMember(Name = "pad")]
    bool? Pad { get; set; }

    [DataMember(Name = "period")]
    int? Period { get; set; }

    [DataMember(Name = "type")]
    HoltWintersType? Type { get; set; }
}

public class HoltWintersModel : IHoltWintersModel
{
    public float? Alpha { get; set; }
    public float? Beta { get; set; }
    public float? Gamma { get; set; }
    public bool? Pad { get; set; }
    public int? Period { get; set; }
    public HoltWintersType? Type { get; set; }
    string IMovingAverageModel.Name { get; } = "holt_winters";
}

public class HoltWintersModelDescriptor
    : DescriptorBase<HoltWintersModelDescriptor, IHoltWintersModel>, IHoltWintersModel
{
    float? IHoltWintersModel.Alpha { get; set; }
    float? IHoltWintersModel.Beta { get; set; }
    float? IHoltWintersModel.Gamma { get; set; }
    string IMovingAverageModel.Name { get; } = "holt_winters";
    bool? IHoltWintersModel.Pad { get; set; }
    int? IHoltWintersModel.Period { get; set; }
    HoltWintersType? IHoltWintersModel.Type { get; set; }

    public HoltWintersModelDescriptor Alpha(float? alpha) => Assign(alpha, (a, v) => a.Alpha = v);

    public HoltWintersModelDescriptor Beta(float? beta) => Assign(beta, (a, v) => a.Beta = v);

    public HoltWintersModelDescriptor Gamma(float? gamma) => Assign(gamma, (a, v) => a.Gamma = v);

    public HoltWintersModelDescriptor Pad(bool? pad = true) => Assign(pad, (a, v) => a.Pad = v);

    public HoltWintersModelDescriptor Period(int? period) => Assign(period, (a, v) => a.Period = v);

    public HoltWintersModelDescriptor Type(HoltWintersType? type) => Assign(type, (a, v) => a.Type = v);
}
