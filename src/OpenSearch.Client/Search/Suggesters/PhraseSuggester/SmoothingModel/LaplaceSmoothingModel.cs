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
using OpenSearch.Net.Utf8Json;

namespace OpenSearch.Client;

[InterfaceDataContract]
[ReadAs(typeof(LaplaceSmoothingModel))]
public interface ILaplaceSmoothingModel : ISmoothingModel
{
    [DataMember(Name = "alpha")]
    double? Alpha { get; set; }
}

public class LaplaceSmoothingModel : SmoothingModelBase, ILaplaceSmoothingModel
{
    public double? Alpha { get; set; }

    internal override void WrapInContainer(ISmoothingModelContainer container) => container.Laplace = this;
}

public class LaplaceSmoothingModelDescriptor : DescriptorBase<LaplaceSmoothingModelDescriptor, ILaplaceSmoothingModel>, ILaplaceSmoothingModel
{
    double? ILaplaceSmoothingModel.Alpha { get; set; }

    public LaplaceSmoothingModelDescriptor Alpha(double? alpha) => Assign(alpha, (a, v) => a.Alpha = v);
}
