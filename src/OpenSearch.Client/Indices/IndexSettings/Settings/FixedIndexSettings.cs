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

namespace OpenSearch.Client;

/// <summary>
/// const string collection of known OpenSearch index settings that can only be provided at
/// index creation time
/// </summary>
public static class FixedIndexSettings
{
    public const string NumberOfRoutingShards = "index.number_of_routing_shards";
    public const string NumberOfShards = "index.number_of_shards";
    public const string RoutingPartitionSize = "index.routing_partition_size";

    /// <summary>
    /// Indicates whether the index should be hidden by default.
    /// Hidden indices are not returned by default when using a wildcard expression.
    /// </summary>
    public const string Hidden = "index.hidden";

    /// <summary>
    /// If a field referred to in a percolator query does not exist,
    /// it will be handled as a default text field so that adding the percolator query doesn't fail.
    /// Defaults to <c>false</c>
    /// </summary>
    public const string PercolatorMapUnmappedFieldsAsText = "index.percolator.map_unmapped_fields_as_text";
}
