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

namespace OpenSearch.Client;

/// <summary>
/// OpenSearch 2.0 has Lucene upgraded up to version 9.0 which doesn't provide memory info for segments.
/// All fields except `count` might be zeroed.
/// </summary>
[DataContract]
public class SegmentsStats
{
    [DataMember(Name = "count")]
    public long Count { get; set; }

    [DataMember(Name = "doc_values_memory_in_bytes")]
    public long DocValuesMemoryInBytes { get; set; }

    [DataMember(Name = "fixed_bit_set_memory_in_bytes")]
    public long FixedBitSetMemoryInBytes { get; set; }

    [DataMember(Name = "index_writer_max_memory_in_bytes")]
    public long IndexWriterMaxMemoryInBytes { get; set; }

    [DataMember(Name = "index_writer_memory_in_bytes")]
    public long IndexWriterMemoryInBytes { get; set; }

    [DataMember(Name = "max_unsafe_auto_id_timestamp")]
    public long MaximumUnsafeAutoIdTimestamp { get; set; }

    [DataMember(Name = "memory_in_bytes")]
    public long MemoryInBytes { get; set; }

    [DataMember(Name = "norms_memory_in_bytes")]
    public long NormsMemoryInBytes { get; set; }

    [DataMember(Name = "points_memory_in_bytes")]
    public long PointsMemoryInBytes { get; set; }

    [DataMember(Name = "stored_fields_memory_in_bytes")]
    public long StoredFieldsMemoryInBytes { get; set; }

    [DataMember(Name = "terms_memory_in_bytes")]
    public long TermsMemoryInBytes { get; set; }

    [DataMember(Name = "term_vectors_memory_in_bytes")]
    public long TermVectorsMemoryInBytes { get; set; }

    [DataMember(Name = "version_map_memory_in_bytes")]
    public long VersionMapMemoryInBytes { get; set; }
}
