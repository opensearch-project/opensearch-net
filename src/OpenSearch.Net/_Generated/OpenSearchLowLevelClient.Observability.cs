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
namespace OpenSearch.Net.Specification.ObservabilityApi
{
    /// <summary>
    /// Observability APIs.
    /// <para>Not intended to be instantiated directly. Use the <see cref="IOpenSearchLowLevelClient.Observability"/> property
    /// on <see cref="IOpenSearchLowLevelClient"/>.
    /// </para>
    /// </summary>
    public partial class LowLevelObservabilityNamespace : NamespacedClientProxy
    {
        internal LowLevelObservabilityNamespace(OpenSearchLowLevelClient client)
            : base(client) { }

        /// <summary>POST on /_plugins/_observability/object</summary>
        /// <param name="body"></param>
        /// <param name="requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
        /// <remarks>Supported by OpenSearch servers of version 1.1.0 or greater.</remarks>
        public TResponse CreateObject<TResponse>(
            PostData body,
            CreateObjectRequestParameters requestParameters = null
        )
            where TResponse : class, IOpenSearchResponse, new() =>
            DoRequest<TResponse>(
                POST,
                "_plugins/_observability/object",
                body,
                RequestParams(requestParameters)
            );

        /// <summary>POST on /_plugins/_observability/object</summary>
        /// <param name="body"></param>
        /// <param name="requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
        /// <remarks>Supported by OpenSearch servers of version 1.1.0 or greater.</remarks>
        [MapsApi("observability.create_object", "body")]
        public Task<TResponse> CreateObjectAsync<TResponse>(
            PostData body,
            CreateObjectRequestParameters requestParameters = null,
            CancellationToken ctx = default
        )
            where TResponse : class, IOpenSearchResponse, new() =>
            DoRequestAsync<TResponse>(
                POST,
                "_plugins/_observability/object",
                ctx,
                body,
                RequestParams(requestParameters)
            );

        /// <summary>DELETE on /_plugins/_observability/object/{object_id}</summary>
        /// <param name="objectId">The ID of the Observability Object.</param>
        /// <param name="requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
        /// <remarks>Supported by OpenSearch servers of version 1.1.0 or greater.</remarks>
        public TResponse DeleteObject<TResponse>(
            string objectId,
            DeleteObjectRequestParameters requestParameters = null
        )
            where TResponse : class, IOpenSearchResponse, new() =>
            DoRequest<TResponse>(
                DELETE,
                Url($"_plugins/_observability/object/{objectId:objectId}"),
                null,
                RequestParams(requestParameters)
            );

        /// <summary>DELETE on /_plugins/_observability/object/{object_id}</summary>
        /// <param name="objectId">The ID of the Observability Object.</param>
        /// <param name="requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
        /// <remarks>Supported by OpenSearch servers of version 1.1.0 or greater.</remarks>
        [MapsApi("observability.delete_object", "object_id")]
        public Task<TResponse> DeleteObjectAsync<TResponse>(
            string objectId,
            DeleteObjectRequestParameters requestParameters = null,
            CancellationToken ctx = default
        )
            where TResponse : class, IOpenSearchResponse, new() =>
            DoRequestAsync<TResponse>(
                DELETE,
                Url($"_plugins/_observability/object/{objectId:objectId}"),
                ctx,
                null,
                RequestParams(requestParameters)
            );

        /// <summary>DELETE on /_plugins/_observability/object</summary>
        /// <param name="requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
        /// <remarks>Supported by OpenSearch servers of version 1.1.0 or greater.</remarks>
        public TResponse DeleteObjects<TResponse>(
            DeleteObjectsRequestParameters requestParameters = null
        )
            where TResponse : class, IOpenSearchResponse, new() =>
            DoRequest<TResponse>(
                DELETE,
                "_plugins/_observability/object",
                null,
                RequestParams(requestParameters)
            );

        /// <summary>DELETE on /_plugins/_observability/object</summary>
        /// <param name="requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
        /// <remarks>Supported by OpenSearch servers of version 1.1.0 or greater.</remarks>
        [MapsApi("observability.delete_objects", "")]
        public Task<TResponse> DeleteObjectsAsync<TResponse>(
            DeleteObjectsRequestParameters requestParameters = null,
            CancellationToken ctx = default
        )
            where TResponse : class, IOpenSearchResponse, new() =>
            DoRequestAsync<TResponse>(
                DELETE,
                "_plugins/_observability/object",
                ctx,
                null,
                RequestParams(requestParameters)
            );

        /// <summary>GET on /_plugins/_observability/_local/stats</summary>
        /// <param name="requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
        /// <remarks>Supported by OpenSearch servers of version 2.5.0 or greater.</remarks>
        public TResponse GetLocalstats<TResponse>(
            GetLocalstatsRequestParameters requestParameters = null
        )
            where TResponse : class, IOpenSearchResponse, new() =>
            DoRequest<TResponse>(
                GET,
                "_plugins/_observability/_local/stats",
                null,
                RequestParams(requestParameters)
            );

        /// <summary>GET on /_plugins/_observability/_local/stats</summary>
        /// <param name="requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
        /// <remarks>Supported by OpenSearch servers of version 2.5.0 or greater.</remarks>
        [MapsApi("observability.get_localstats", "")]
        public Task<TResponse> GetLocalstatsAsync<TResponse>(
            GetLocalstatsRequestParameters requestParameters = null,
            CancellationToken ctx = default
        )
            where TResponse : class, IOpenSearchResponse, new() =>
            DoRequestAsync<TResponse>(
                GET,
                "_plugins/_observability/_local/stats",
                ctx,
                null,
                RequestParams(requestParameters)
            );

        /// <summary>GET on /_plugins/_observability/object/{object_id}</summary>
        /// <param name="objectId">The ID of the Observability Object.</param>
        /// <param name="requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
        /// <remarks>Supported by OpenSearch servers of version 1.1.0 or greater.</remarks>
        public TResponse GetObject<TResponse>(
            string objectId,
            GetObjectRequestParameters requestParameters = null
        )
            where TResponse : class, IOpenSearchResponse, new() =>
            DoRequest<TResponse>(
                GET,
                Url($"_plugins/_observability/object/{objectId:objectId}"),
                null,
                RequestParams(requestParameters)
            );

        /// <summary>GET on /_plugins/_observability/object/{object_id}</summary>
        /// <param name="objectId">The ID of the Observability Object.</param>
        /// <param name="requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
        /// <remarks>Supported by OpenSearch servers of version 1.1.0 or greater.</remarks>
        [MapsApi("observability.get_object", "object_id")]
        public Task<TResponse> GetObjectAsync<TResponse>(
            string objectId,
            GetObjectRequestParameters requestParameters = null,
            CancellationToken ctx = default
        )
            where TResponse : class, IOpenSearchResponse, new() =>
            DoRequestAsync<TResponse>(
                GET,
                Url($"_plugins/_observability/object/{objectId:objectId}"),
                ctx,
                null,
                RequestParams(requestParameters)
            );

        /// <summary>GET on /_plugins/_observability/object</summary>
        /// <param name="requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
        /// <remarks>Supported by OpenSearch servers of version 1.1.0 or greater.</remarks>
        public TResponse ListObjects<TResponse>(
            ListObjectsRequestParameters requestParameters = null
        )
            where TResponse : class, IOpenSearchResponse, new() =>
            DoRequest<TResponse>(
                GET,
                "_plugins/_observability/object",
                null,
                RequestParams(requestParameters)
            );

        /// <summary>GET on /_plugins/_observability/object</summary>
        /// <param name="requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
        /// <remarks>Supported by OpenSearch servers of version 1.1.0 or greater.</remarks>
        [MapsApi("observability.list_objects", "")]
        public Task<TResponse> ListObjectsAsync<TResponse>(
            ListObjectsRequestParameters requestParameters = null,
            CancellationToken ctx = default
        )
            where TResponse : class, IOpenSearchResponse, new() =>
            DoRequestAsync<TResponse>(
                GET,
                "_plugins/_observability/object",
                ctx,
                null,
                RequestParams(requestParameters)
            );

        /// <summary>PUT on /_plugins/_observability/object/{object_id}</summary>
        /// <param name="objectId">The ID of the Observability Object.</param>
        /// <param name="body"></param>
        /// <param name="requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
        /// <remarks>Supported by OpenSearch servers of version 1.1.0 or greater.</remarks>
        public TResponse UpdateObject<TResponse>(
            string objectId,
            PostData body,
            UpdateObjectRequestParameters requestParameters = null
        )
            where TResponse : class, IOpenSearchResponse, new() =>
            DoRequest<TResponse>(
                PUT,
                Url($"_plugins/_observability/object/{objectId:objectId}"),
                body,
                RequestParams(requestParameters)
            );

        /// <summary>PUT on /_plugins/_observability/object/{object_id}</summary>
        /// <param name="objectId">The ID of the Observability Object.</param>
        /// <param name="body"></param>
        /// <param name="requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
        /// <remarks>Supported by OpenSearch servers of version 1.1.0 or greater.</remarks>
        [MapsApi("observability.update_object", "object_id, body")]
        public Task<TResponse> UpdateObjectAsync<TResponse>(
            string objectId,
            PostData body,
            UpdateObjectRequestParameters requestParameters = null,
            CancellationToken ctx = default
        )
            where TResponse : class, IOpenSearchResponse, new() =>
            DoRequestAsync<TResponse>(
                PUT,
                Url($"_plugins/_observability/object/{objectId:objectId}"),
                ctx,
                body,
                RequestParams(requestParameters)
            );
    }
}
