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

namespace OpenSearch.Client;

[StringEnum]
public enum NodeRole
{
    ///<remarks>Deprecated as of OpenSearch 2.0, use <see cref="ClusterManager"/> instead</remarks>
    [EnumMember(Value = "master")]
    Master,

    ///<remarks>Introduced in OpenSearch 2.0 instead of <see cref="Master"/></remarks>
    [EnumMember(Value = "cluster_manager")]
    ClusterManager,

    [EnumMember(Value = "data")]
    Data,

    [EnumMember(Value = "data_cold")]
    DataCold,

    [EnumMember(Value = "data_frozen")]
    DataFrozen,

    [EnumMember(Value = "data_content")]
    DataContent,

    [EnumMember(Value = "data_hot")]
    DataHot,

    [EnumMember(Value = "data_warm")]
    DataWarm,

    [EnumMember(Value = "client")]
    Client,

    [EnumMember(Value = "ingest")]
    Ingest,

    [EnumMember(Value = "voting_only")]
    VotingOnly,

    [EnumMember(Value = "transform")]
    Transform,

    [EnumMember(Value = "remote_cluster_client")]
    RemoteClusterClient,

    [EnumMember(Value = "coordinating_only")]
    CoordinatingOnly,
}
