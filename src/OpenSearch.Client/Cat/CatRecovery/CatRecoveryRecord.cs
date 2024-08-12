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

[DataContract]
public class CatRecoveryRecord : ICatRecord
{
    [DataMember(Name = "bytes")]
    public string Bytes { get; set; }

    [DataMember(Name = "bytes_percent")]
    public string BytesPercent { get; set; }

    [DataMember(Name = "bytes_recovered")]
    public string BytesRecovered { get; set; }

    [DataMember(Name = "bytes_total")]
    public string BytesTotal { get; set; }

    [DataMember(Name = "files")]
    public string Files { get; set; }

    [DataMember(Name = "files_percent")]
    public string FilesPercent { get; set; }

    [DataMember(Name = "files_recovered")]
    public string FilesRecovered { get; set; }

    [DataMember(Name = "files_total")]
    public string FilesTotal { get; set; }

    [DataMember(Name = "index")]
    public string Index { get; set; }

    [DataMember(Name = "repository")]
    public string Repository { get; set; }

    [DataMember(Name = "shard")]
    public string Shard { get; set; }

    [DataMember(Name = "snapshot")]
    public string Snapshot { get; set; }

    [DataMember(Name = "source_host")]
    public string SourceHost { get; set; }

    [DataMember(Name = "source_node")]
    public string SourceNode { get; set; }

    [DataMember(Name = "stage")]
    public string Stage { get; set; }

    [DataMember(Name = "target_host")]
    public string TargetHost { get; set; }

    [DataMember(Name = "target_node")]
    public string TargetNode { get; set; }

    [DataMember(Name = "time")]
    public string Time { get; set; }

    [DataMember(Name = "translog_ops")]
    [JsonFormatter(typeof(NullableStringLongFormatter))]
    public long? TranslogOps { get; set; }

    [DataMember(Name = "translog_ops_percent")]
    public string TranslogOpsPercent { get; set; }

    [DataMember(Name = "translog_ops_recovered")]
    [JsonFormatter(typeof(NullableStringLongFormatter))]
    public long? TranslogOpsRecovered { get; set; }

    [DataMember(Name = "type")]
    public string Type { get; set; }
}
