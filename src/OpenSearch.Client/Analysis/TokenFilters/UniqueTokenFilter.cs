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

/// <summary>
/// The unique token filter can be used to only index unique tokens during analysis. By default it is applied on all the
/// token stream
/// </summary>
public interface IUniqueTokenFilter : ITokenFilter
{
    /// <summary>
    ///  If only_on_same_position is set to true, it will only remove duplicate tokens on the same position.
    /// </summary>
    [DataMember(Name = "only_on_same_position")]
    [JsonFormatter(typeof(NullableStringBooleanFormatter))]
    bool? OnlyOnSamePosition { get; set; }
}

/// <inheritdoc />
public class UniqueTokenFilter : TokenFilterBase, IUniqueTokenFilter
{
    public UniqueTokenFilter() : base("unique") { }

    /// <inheritdoc />
    public bool? OnlyOnSamePosition { get; set; }
}

/// <inheritdoc />
public class UniqueTokenFilterDescriptor
    : TokenFilterDescriptorBase<UniqueTokenFilterDescriptor, IUniqueTokenFilter>, IUniqueTokenFilter
{
    protected override string Type => "unique";

    bool? IUniqueTokenFilter.OnlyOnSamePosition { get; set; }

    /// <inheritdoc />
    public UniqueTokenFilterDescriptor OnlyOnSamePosition(bool? samePositionOnly = true) => Assign(samePositionOnly, (a, v) => a.OnlyOnSamePosition = v);
}
