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
using static OpenSearch.Net.HttpMethod;

// ReSharper disable InterpolatedStringExpressionIsNotIFormattable
// ReSharper disable once CheckNamespace
// ReSharper disable InterpolatedStringExpressionIsNotIFormattable
// ReSharper disable RedundantExtendsListEntry
namespace OpenSearch.Net.Specification.CatApi
{
    /// <summary>
    /// Cat APIs.
    /// <para>Not intended to be instantiated directly. Use the <see cref="IOpenSearchLowLevelClient.Cat"/> property
    /// on <see cref="IOpenSearchLowLevelClient"/>.
    /// </para>
    /// </summary>
    public partial class LowLevelCatNamespace : NamespacedClientProxy
    {
        internal LowLevelCatNamespace(OpenSearchLowLevelClient client)
            : base(client) { }

        protected override string ContentType => "text/plain";

        /// <summary>GET on /_cat/aliases <para>https://opensearch.org/docs/latest/api-reference/cat/cat-aliases/</para></summary>
        /// <param name="requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
        public TResponse Aliases<TResponse>(CatAliasesRequestParameters requestParameters = null)
            where TResponse : class, IOpenSearchResponse, new() =>
            DoRequest<TResponse>(GET, "_cat/aliases", null, RequestParams(requestParameters));

        /// <summary>GET on /_cat/aliases <para>https://opensearch.org/docs/latest/api-reference/cat/cat-aliases/</para></summary>
        /// <param name="requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
        [MapsApi("cat.aliases", "")]
        public Task<TResponse> AliasesAsync<TResponse>(
            CatAliasesRequestParameters requestParameters = null,
            CancellationToken ctx = default
        )
            where TResponse : class, IOpenSearchResponse, new() =>
            DoRequestAsync<TResponse>(
                GET,
                "_cat/aliases",
                ctx,
                null,
                RequestParams(requestParameters)
            );

        /// <summary>GET on /_cat/aliases/{name} <para>https://opensearch.org/docs/latest/api-reference/cat/cat-aliases/</para></summary>
        /// <param name="name">Comma-separated list of alias names.</param>
        /// <param name="requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
        public TResponse Aliases<TResponse>(
            string name,
            CatAliasesRequestParameters requestParameters = null
        )
            where TResponse : class, IOpenSearchResponse, new() =>
            DoRequest<TResponse>(
                GET,
                Url($"_cat/aliases/{name:name}"),
                null,
                RequestParams(requestParameters)
            );

        /// <summary>GET on /_cat/aliases/{name} <para>https://opensearch.org/docs/latest/api-reference/cat/cat-aliases/</para></summary>
        /// <param name="name">Comma-separated list of alias names.</param>
        /// <param name="requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
        [MapsApi("cat.aliases", "name")]
        public Task<TResponse> AliasesAsync<TResponse>(
            string name,
            CatAliasesRequestParameters requestParameters = null,
            CancellationToken ctx = default
        )
            where TResponse : class, IOpenSearchResponse, new() =>
            DoRequestAsync<TResponse>(
                GET,
                Url($"_cat/aliases/{name:name}"),
                ctx,
                null,
                RequestParams(requestParameters)
            );

        ///<summary>GET on /_cat/allocation <para>https://opensearch.org/docs/latest/api-reference/cat/cat-allocation/</para></summary>
        ///<param name="requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
        public TResponse Allocation<TResponse>(
            CatAllocationRequestParameters requestParameters = null
        )
            where TResponse : class, IOpenSearchResponse, new() =>
            DoRequest<TResponse>(GET, "_cat/allocation", null, RequestParams(requestParameters));

        ///<summary>GET on /_cat/allocation <para>https://opensearch.org/docs/latest/api-reference/cat/cat-allocation/</para></summary>
        ///<param name="requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
        [MapsApi("cat.allocation", "")]
        public Task<TResponse> AllocationAsync<TResponse>(
            CatAllocationRequestParameters requestParameters = null,
            CancellationToken ctx = default
        )
            where TResponse : class, IOpenSearchResponse, new() =>
            DoRequestAsync<TResponse>(
                GET,
                "_cat/allocation",
                ctx,
                null,
                RequestParams(requestParameters)
            );

        ///<summary>GET on /_cat/allocation/{node_id} <para>https://opensearch.org/docs/latest/api-reference/cat/cat-allocation/</para></summary>
        ///<param name="nodeId">Comma-separated list of node IDs or names to limit the returned information.</param>
        ///<param name="requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
        public TResponse Allocation<TResponse>(
            string nodeId,
            CatAllocationRequestParameters requestParameters = null
        )
            where TResponse : class, IOpenSearchResponse, new() =>
            DoRequest<TResponse>(
                GET,
                Url($"_cat/allocation/{nodeId:nodeId}"),
                null,
                RequestParams(requestParameters)
            );

        ///<summary>GET on /_cat/allocation/{node_id} <para>https://opensearch.org/docs/latest/api-reference/cat/cat-allocation/</para></summary>
        ///<param name="nodeId">Comma-separated list of node IDs or names to limit the returned information.</param>
        ///<param name="requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
        [MapsApi("cat.allocation", "node_id")]
        public Task<TResponse> AllocationAsync<TResponse>(
            string nodeId,
            CatAllocationRequestParameters requestParameters = null,
            CancellationToken ctx = default
        )
            where TResponse : class, IOpenSearchResponse, new() =>
            DoRequestAsync<TResponse>(
                GET,
                Url($"_cat/allocation/{nodeId:nodeId}"),
                ctx,
                null,
                RequestParams(requestParameters)
            );
    }
}
