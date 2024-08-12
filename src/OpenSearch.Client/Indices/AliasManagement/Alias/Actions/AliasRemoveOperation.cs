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

using System.Collections.Generic;
using System.Runtime.Serialization;
using OpenSearch.Net.Utf8Json;

namespace OpenSearch.Client;

public class AliasRemoveOperation
{
    /// <summary>
    /// An alias to remove.
    /// Multiple aliases can be specified with <see cref="Aliases"/>
    /// </summary>
    [DataMember(Name = "alias")]
    public string Alias { get; set; }

    /// <summary>
    /// A collection of aliases to remove
    /// </summary>
    [DataMember(Name = "aliases")]
    public IEnumerable<string> Aliases { get; set; }

    /// <summary>
    /// The index to which to remove the alias.
    /// Multiple indices can be specified with <see cref="Indices"/>
    /// </summary>
    [DataMember(Name = "index")]
    public IndexName Index { get; set; }

    /// <summary>
    /// The indices to which to remove the alias
    /// </summary>
    [DataMember(Name = "indices")]
    [JsonFormatter(typeof(IndicesFormatter))]
    public Indices Indices { get; set; }

    /// <summary>
    /// If <c>true</c>, the alias to remove must exist. Defaults to <c>false</c>.
    /// </summary>
    [DataMember(Name = "must_exist")]
    public bool? MustExist { get; set; }
}
