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
using OpenSearch.Net.Utf8Json;

namespace OpenSearch.Client;

/// <summary>
/// An item within a bulk response
/// </summary>
[JsonFormatter(typeof(BulkResponseItemFormatter))]
public abstract class BulkResponseItemBase
{
    /// <summary>
    /// The error associated with the bulk operation
    /// </summary>
    [DataMember(Name = "error")]
    public Error Error { get; internal set; }

    /// <summary>
    /// The id of the document for the bulk operation
    /// </summary>
    [DataMember(Name = "_id")]
    public string Id { get; internal set; }

    /// <summary>
    /// The index against which the bulk operation ran
    /// </summary>
    [DataMember(Name = "_index")]
    public string Index { get; internal set; }

    /// <summary> The type of bulk operation </summary>
    public abstract string Operation { get; }

    [DataMember(Name = "_primary_term")]
    public long PrimaryTerm { get; internal set; }

    [DataMember(Name = "get")]
    internal LazyDocument Get { get; set; }

    /// <summary>
    /// Deserialize the <see cref="Get"/> property as a <see cref="GetResponse{TDocument}"/> type, where <typeparamref name="TDocument"/> is the document type.
    /// </summary>
    public GetResponse<TDocument> GetResponse<TDocument>() where TDocument : class => Get?.AsUsingRequestResponseSerializer<GetResponse<TDocument>>();

    /// <summary> The result of the bulk operation</summary>
    [DataMember(Name = "result")]
    public string Result { get; internal set; }

    [DataMember(Name = "_seq_no")]
    public long SequenceNumber { get; internal set; }

    /// <summary>
    /// The shards associated with the bulk operation
    /// </summary>
    [DataMember(Name = "_shards")]
    public ShardStatistics Shards { get; internal set; }

    /// <summary> The status of the bulk operation </summary>
    [DataMember(Name = "status")]
    public int Status { get; internal set; }

    /// <summary>
    /// The type against which the bulk operation ran
    /// </summary>
    /// <remarks>Deprecated as of OpenSearch 2.0</remarks>
    [DataMember(Name = "_type")]
    public string Type { get; internal set; }

    /// <summary> The version of the document </summary>
    [DataMember(Name = "_version")]
    public long Version { get; internal set; }

    /// <summary>
    /// Specifies whether this particular bulk operation succeeded or not
    /// </summary>
    public bool IsValid
    {
        get
        {
            if (Error != null) return false;

            switch (Operation.ToLowerInvariant())
            {
                case "delete": return Status == 200 || Status == 404;
                case "update":
                case "index":
                case "create":
                    return Status == 200 || Status == 201;
                default:
                    return false;
            }
        }
    }

    public override string ToString() =>
        $"{Operation} returned {Status} _index: {Index} _type: {Type} _id: {Id} _version: {Version} error: {Error}";
}
