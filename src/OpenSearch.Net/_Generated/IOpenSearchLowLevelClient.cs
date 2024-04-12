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
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OpenSearch.Net;
using OpenSearch.Net.Specification.CatApi;
using OpenSearch.Net.Specification.ClusterApi;
using OpenSearch.Net.Specification.DanglingIndicesApi;
using OpenSearch.Net.Specification.HttpApi;
using OpenSearch.Net.Specification.IndicesApi;
using OpenSearch.Net.Specification.IngestApi;
using OpenSearch.Net.Specification.NodesApi;
using OpenSearch.Net.Specification.SnapshotApi;
using OpenSearch.Net.Specification.TasksApi;

namespace OpenSearch.Net
{
    /// <summary>
    /// OpenSearch low level client
    /// </summary>
    public partial interface IOpenSearchLowLevelClient
    {
        /// <summary>Cat APIs</summary>
        LowLevelCatNamespace Cat { get; }

        /// <summary>Cluster APIs</summary>
        LowLevelClusterNamespace Cluster { get; }

        /// <summary>Dangling Indices APIs</summary>
        LowLevelDanglingIndicesNamespace DanglingIndices { get; }

        /// <summary>Indices APIs</summary>
        LowLevelIndicesNamespace Indices { get; }

        /// <summary>Ingest APIs</summary>
        LowLevelIngestNamespace Ingest { get; }

        /// <summary>Nodes APIs</summary>
        LowLevelNodesNamespace Nodes { get; }

        /// <summary>Http APIs</summary>
        LowLevelHttpNamespace Http { get; }

        /// <summary>Snapshot APIs</summary>
        LowLevelSnapshotNamespace Snapshot { get; }

        /// <summary>Tasks APIs</summary>
        LowLevelTasksNamespace Tasks { get; }

        /// <summary>POST on /{index}/_search/point_in_time <para>https://opensearch.org/docs/latest/search-plugins/point-in-time-api/#create-a-pit</para></summary>
        /// <param name="index">Comma-separated list of indices; use the special string `_all` or Indices.All to perform the operation on all indices.</param>
        /// <param name="requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
        /// <remarks>Supported by OpenSearch servers of version 2.4.0 or greater.</remarks>
        TResponse CreatePit<TResponse>(
            string index,
            CreatePitRequestParameters requestParameters = null
        )
            where TResponse : class, IOpenSearchResponse, new();

        /// <summary>POST on /{index}/_search/point_in_time <para>https://opensearch.org/docs/latest/search-plugins/point-in-time-api/#create-a-pit</para></summary>
        /// <param name="index">Comma-separated list of indices; use the special string `_all` or Indices.All to perform the operation on all indices.</param>
        /// <param name="requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
        /// <remarks>Supported by OpenSearch servers of version 2.4.0 or greater.</remarks>
        Task<TResponse> CreatePitAsync<TResponse>(
            string index,
            CreatePitRequestParameters requestParameters = null,
            CancellationToken ctx = default
        )
            where TResponse : class, IOpenSearchResponse, new();

        /// <summary>DELETE on /_search/point_in_time/_all <para>https://opensearch.org/docs/latest/search-plugins/point-in-time-api/#delete-pits</para></summary>
        /// <param name="requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
        /// <remarks>Supported by OpenSearch servers of version 2.4.0 or greater.</remarks>
        TResponse DeleteAllPits<TResponse>(DeleteAllPitsRequestParameters requestParameters = null)
            where TResponse : class, IOpenSearchResponse, new();

        /// <summary>DELETE on /_search/point_in_time/_all <para>https://opensearch.org/docs/latest/search-plugins/point-in-time-api/#delete-pits</para></summary>
        /// <param name="requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
        /// <remarks>Supported by OpenSearch servers of version 2.4.0 or greater.</remarks>
        Task<TResponse> DeleteAllPitsAsync<TResponse>(
            DeleteAllPitsRequestParameters requestParameters = null,
            CancellationToken ctx = default
        )
            where TResponse : class, IOpenSearchResponse, new();

        /// <summary>DELETE on /_search/point_in_time <para>https://opensearch.org/docs/latest/search-plugins/point-in-time-api/#delete-pits</para></summary>
        /// <param name="body">The point-in-time ids to be deleted.</param>
        /// <param name="requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
        /// <remarks>Supported by OpenSearch servers of version 2.4.0 or greater.</remarks>
        TResponse DeletePit<TResponse>(
            PostData body,
            DeletePitRequestParameters requestParameters = null
        )
            where TResponse : class, IOpenSearchResponse, new();

        /// <summary>DELETE on /_search/point_in_time <para>https://opensearch.org/docs/latest/search-plugins/point-in-time-api/#delete-pits</para></summary>
        /// <param name="body">The point-in-time ids to be deleted.</param>
        /// <param name="requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
        /// <remarks>Supported by OpenSearch servers of version 2.4.0 or greater.</remarks>
        Task<TResponse> DeletePitAsync<TResponse>(
            PostData body,
            DeletePitRequestParameters requestParameters = null,
            CancellationToken ctx = default
        )
            where TResponse : class, IOpenSearchResponse, new();

        /// <summary>GET on /_search/point_in_time/_all <para>https://opensearch.org/docs/latest/search-plugins/point-in-time-api/#list-all-pits</para></summary>
        /// <param name="requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
        /// <remarks>Supported by OpenSearch servers of version 2.4.0 or greater.</remarks>
        TResponse GetAllPits<TResponse>(GetAllPitsRequestParameters requestParameters = null)
            where TResponse : class, IOpenSearchResponse, new();

        /// <summary>GET on /_search/point_in_time/_all <para>https://opensearch.org/docs/latest/search-plugins/point-in-time-api/#list-all-pits</para></summary>
        /// <param name="requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
        /// <remarks>Supported by OpenSearch servers of version 2.4.0 or greater.</remarks>
        Task<TResponse> GetAllPitsAsync<TResponse>(
            GetAllPitsRequestParameters requestParameters = null,
            CancellationToken ctx = default
        )
            where TResponse : class, IOpenSearchResponse, new();
    }
}
