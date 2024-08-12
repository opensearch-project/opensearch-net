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

// Licensed to Elasticsearch B.V under one or more agreements.
// Elasticsearch B.V licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information

using OpenSearch.Net;

namespace OpenSearch.Client;

/// <summary>
/// Block type for an index.
/// </summary>
public class IndexBlock : IUrlParameter
{
    private IndexBlock(string value) => Value = value;

    public string Value { get; }

    public string GetString(IConnectionConfigurationValues settings) => Value;

    /// <summary>
    /// Disable metadata changes, such as closing the index.
    /// </summary>
    public static IndexBlock Metadata { get; } = new IndexBlock("metadata");

    /// <summary>
    /// Disable read operations.
    /// </summary>
    public static IndexBlock Read { get; } = new IndexBlock("read");

    /// <summary>
    /// Disable write operations and metadata changes.
    /// </summary>
    public static IndexBlock ReadOnly { get; } = new IndexBlock("read_only");

    /// <summary>
    /// Disable write operations. However, metadata changes are still allowed.
    /// </summary>
    public static IndexBlock Write { get; } = new IndexBlock("write");
}
