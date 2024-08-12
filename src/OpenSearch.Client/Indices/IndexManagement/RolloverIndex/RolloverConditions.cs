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
/// Conditions that must be satisfied for a new index to be created
/// with the rollover index API
/// </summary>
[InterfaceDataContract]
[ReadAs(typeof(RolloverConditions))]
public interface IRolloverConditions
{
    /// <summary>
    /// The maximum age of the index
    /// </summary>
    [DataMember(Name = "max_age")]
    Time MaxAge { get; set; }

    /// <summary>
    /// The maximum number of documents
    /// </summary>
    [DataMember(Name = "max_docs")]
    long? MaxDocs { get; set; }

    /// <summary>
    /// The maximum size of the index e.g. "5gb"
    /// </summary>
    [DataMember(Name = "max_size")]
    string MaxSize { get; set; }
}

/// <inheritdoc />
public class RolloverConditions : IRolloverConditions
{
    /// <inheritdoc />
    public Time MaxAge { get; set; }

    /// <inheritdoc />
    public long? MaxDocs { get; set; }

    /// <inheritdoc />
    public string MaxSize { get; set; }

    /// <inheritdoc />
    public string MaxPrimaryShardSize { get; set; }
}

/// <inheritdoc cref="IRolloverConditions" />
public class RolloverConditionsDescriptor
    : DescriptorBase<RolloverConditionsDescriptor, IRolloverConditions>, IRolloverConditions
{
    Time IRolloverConditions.MaxAge { get; set; }
    long? IRolloverConditions.MaxDocs { get; set; }
    string IRolloverConditions.MaxSize { get; set; }

    /// <inheritdoc cref="IRolloverConditions.MaxAge" />
    public RolloverConditionsDescriptor MaxAge(Time maxAge) => Assign(maxAge, (a, v) => a.MaxAge = v);

    /// <inheritdoc cref="IRolloverConditions.MaxDocs" />
    public RolloverConditionsDescriptor MaxDocs(long? maxDocs) => Assign(maxDocs, (a, v) => a.MaxDocs = v);

    /// <inheritdoc cref="IRolloverConditions.MaxSize" />
    public RolloverConditionsDescriptor MaxSize(string maxSize) => Assign(maxSize, (a, v) => a.MaxSize = v);
}
