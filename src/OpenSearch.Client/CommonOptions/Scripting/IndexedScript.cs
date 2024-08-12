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
public interface IIndexedScript : IScript
{
    [DataMember(Name = "id")]
    string Id { get; set; }
}

public class IndexedScript : ScriptBase, IIndexedScript
{
    public IndexedScript(string id) => Id = id;

    public string Id { get; set; }
}

public class IndexedScriptDescriptor
    : ScriptDescriptorBase<IndexedScriptDescriptor, IIndexedScript>, IIndexedScript
{
    public IndexedScriptDescriptor() { }

    public IndexedScriptDescriptor(string id) => Self.Id = id;

    string IIndexedScript.Id { get; set; }

    public IndexedScriptDescriptor Id(string id) => Assign(id, (a, v) => a.Id = v);
}
