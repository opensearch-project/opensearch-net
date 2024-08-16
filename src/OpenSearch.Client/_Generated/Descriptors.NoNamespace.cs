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
*   http://www.apache.org/licenses/LICENSE-2.0
*
*  Unless required by applicable law or agreed to in writing,
*  software distributed under the License is distributed on an
*  "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
*  KIND, either express or implied.  See the License for the
*  specific language governing permissions and limitations
*  under the License.
*/

// ███╗   ██╗ ██████╗ ████████╗██╗ ██████╗███████╗
// ████╗  ██║██╔═══██╗╚══██╔══╝██║██╔════╝██╔════╝
// ██╔██╗ ██║██║   ██║   ██║   ██║██║     █████╗
// ██║╚██╗██║██║   ██║   ██║   ██║██║     ██╔══╝
// ██║ ╚████║╚██████╔╝   ██║   ██║╚██████╗███████╗
// ╚═╝  ╚═══╝ ╚═════╝    ╚═╝   ╚═╝ ╚═════╝╚══════╝
// -----------------------------------------------
//
// This file is automatically generated
// Please do not edit these files manually
// Run the following in the root of the repos:
//
//      *NIX        :   ./build.sh codegen
//      Windows     :   build.bat codegen
//
// -----------------------------------------------
// ReSharper disable RedundantUsingDirective
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using OpenSearch.Net;
using OpenSearch.Net.Utf8Json;

// ReSharper disable RedundantBaseConstructorCall
// ReSharper disable UnusedTypeParameter
// ReSharper disable PartialMethodWithSinglePart
// ReSharper disable RedundantNameQualifier
namespace OpenSearch.Client
{
    /// <summary>Descriptor for CreatePit <para>https://opensearch.org/docs/latest/search-plugins/point-in-time-api/#create-a-pit</para></summary>
    public partial class CreatePitDescriptor
         : RequestDescriptorBase<CreatePitDescriptor, CreatePitRequestParameters, ICreatePitRequest>,
            ICreatePitRequest
    {
        internal override ApiUrls ApiUrls => ApiUrlsLookups.NoNamespaceCreatePit;
        /// <summary>/{index}/_search/point_in_time</summary>
        /// <param name="index">this parameter is required</param>
        public CreatePitDescriptor(Indices index)
            : base(r => r.Required("index", index)) { }
        /// <summary>Used for serialization purposes, making sure we have a parameterless constructor</summary>
        [SerializationConstructor]
        protected CreatePitDescriptor()
            : base() { }
        // values part of the url path
        Indices ICreatePitRequest.Index => Self.RouteValues.Get<Indices>("index");

        /// <summary>Comma-separated list of indices; use the special string `_all` or Indices.All to perform the operation on all indices.</summary>
        public CreatePitDescriptor Index(Indices index) => Assign(index, (a, v) => a.RouteValues.Required("index", v));

        /// <summary>a shortcut into calling Index(typeof(TOther))</summary>
        public CreatePitDescriptor Index<TOther>() where TOther : class => Assign(typeof(TOther), (a, v) => a.RouteValues.Required("index", (Indices)v));

        /// <summary>A shortcut into calling Index(Indices.All)</summary>
        public CreatePitDescriptor AllIndices() => Index(Indices.All);
        // Request parameters
        /// <summary>Allow if point in time can be created with partial failures.</summary>
        public CreatePitDescriptor AllowPartialPitCreation(bool? allowpartialpitcreation = true) => Qs("allow_partial_pit_creation", allowpartialpitcreation);
        /// <summary>Whether to expand wildcard expression to concrete indices that are open, closed or both.</summary>
        public CreatePitDescriptor ExpandWildcards(ExpandWildcards? expandwildcards) => Qs("expand_wildcards", expandwildcards);
        /// <summary>Specify the keep alive for point in time.</summary>
        public CreatePitDescriptor KeepAlive(Time keepalive) => Qs("keep_alive", keepalive);
        /// <summary>Specify the node or shard the operation should be performed on.</summary>
        public CreatePitDescriptor Preference(string preference) => Qs("preference", preference);
        /// <summary>
        /// A document is routed to a particular shard in an index using the following formula
        /// <para> shard_num = hash(_routing) % num_primary_shards</para>
        /// <para>OpenSearch will use the document id if not provided. </para>
        /// <para>For requests that are constructed from/for a document OpenSearch.Client will automatically infer the routing key
        /// if that document has a <see cref="OpenSearch.Client.JoinField" /> or a routing mapping on for its type exists on <see cref="OpenSearch.Client.ConnectionSettings" /></para> 
        /// </summary>
        public CreatePitDescriptor Routing(Routing routing) => Qs("routing", routing);
    }

    /// <summary>Descriptor for DeleteAllPits <para>https://opensearch.org/docs/latest/search-plugins/point-in-time-api/#delete-pits</para></summary>
    public partial class DeleteAllPitsDescriptor
         : RequestDescriptorBase<DeleteAllPitsDescriptor, DeleteAllPitsRequestParameters, IDeleteAllPitsRequest>,
            IDeleteAllPitsRequest
    {
        internal override ApiUrls ApiUrls => ApiUrlsLookups.NoNamespaceDeleteAllPits;
        // values part of the url path
        // Request parameters
    }

    /// <summary>Descriptor for DeletePit <para>https://opensearch.org/docs/latest/search-plugins/point-in-time-api/#delete-pits</para></summary>
    public partial class DeletePitDescriptor
         : RequestDescriptorBase<DeletePitDescriptor, DeletePitRequestParameters, IDeletePitRequest>,
            IDeletePitRequest
    {
        internal override ApiUrls ApiUrls => ApiUrlsLookups.NoNamespaceDeletePit;
        // values part of the url path
        // Request parameters
    }

    /// <summary>Descriptor for GetAllPits <para>https://opensearch.org/docs/latest/search-plugins/point-in-time-api/#list-all-pits</para></summary>
    public partial class GetAllPitsDescriptor
         : RequestDescriptorBase<GetAllPitsDescriptor, GetAllPitsRequestParameters, IGetAllPitsRequest>,
            IGetAllPitsRequest
    {
        internal override ApiUrls ApiUrls => ApiUrlsLookups.NoNamespaceGetAllPits;
        // values part of the url path
        // Request parameters
    }

}
