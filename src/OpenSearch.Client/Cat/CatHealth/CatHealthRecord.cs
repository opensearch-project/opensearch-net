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

namespace OpenSearch.Client;

[DataContract]
public class CatHealthRecord : ICatRecord
{
    [DataMember(Name = "cluster")]
    public string Cluster { get; set; }

    [DataMember(Name = "epoch")]
    public string Epoch { get; set; }

    [DataMember(Name = "init")]
    public string Initializing { get; set; }

    [DataMember(Name = "node.data")]
    public string NodeData { get; set; }

    [DataMember(Name = "node.total")]
    public string NodeTotal { get; set; }

    [DataMember(Name = "pending_tasks")]
    public string PendingTasks { get; set; }

    [DataMember(Name = "pri")]
    public string Primary { get; set; }

    [DataMember(Name = "relo")]
    public string Relocating { get; set; }

    [DataMember(Name = "shards")]
    public string Shards { get; set; }

    [DataMember(Name = "status")]
    public string Status { get; set; }

    [DataMember(Name = "timestamp")]
    public string Timestamp { get; set; }

    [DataMember(Name = "unassign")]
    public string Unassigned { get; set; }
}
