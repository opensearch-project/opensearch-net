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
namespace OpenSearch.Net.Specification.IndicesApi
{
    /// <summary>
    /// Indices APIs.
    /// <para>Not intended to be instantiated directly. Use the <see cref="IOpenSearchLowLevelClient.Indices"/> property
    /// on <see cref="IOpenSearchLowLevelClient"/>.
    /// </para>
    /// </summary>
    public partial class LowLevelIndicesNamespace : NamespacedClientProxy
    {
        internal LowLevelIndicesNamespace(OpenSearchLowLevelClient client)
            : base(client) { }

        /// <summary>DELETE on /_index_template/{name} <para>https://opensearch.org/docs/latest/im-plugin/index-templates/#delete-a-template</para></summary>
        /// <param name="name">Name of the index template to delete. Wildcard (*) expressions are supported.</param>
        /// <param name="requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
        public TResponse DeleteComposableTemplateForAll<TResponse>(
            string name,
            DeleteComposableIndexTemplateRequestParameters requestParameters = null
        )
            where TResponse : class, IOpenSearchResponse, new() =>
            DoRequest<TResponse>(
                DELETE,
                Url($"_index_template/{name:name}"),
                null,
                RequestParams(requestParameters)
            );

        /// <summary>DELETE on /_index_template/{name} <para>https://opensearch.org/docs/latest/im-plugin/index-templates/#delete-a-template</para></summary>
        /// <param name="name">Name of the index template to delete. Wildcard (*) expressions are supported.</param>
        /// <param name="requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
        [MapsApi("indices.delete_index_template", "name")]
        public Task<TResponse> DeleteComposableTemplateForAllAsync<TResponse>(
            string name,
            DeleteComposableIndexTemplateRequestParameters requestParameters = null,
            CancellationToken ctx = default
        )
            where TResponse : class, IOpenSearchResponse, new() =>
            DoRequestAsync<TResponse>(
                DELETE,
                Url($"_index_template/{name:name}"),
                ctx,
                null,
                RequestParams(requestParameters)
            );

        /// <summary>HEAD on /_index_template/{name} <para>https://opensearch.org/docs/latest/im-plugin/index-templates/</para></summary>
        /// <param name="name">Name of the index template to check existence of. Wildcard (*) expressions are supported.</param>
        /// <param name="requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
        public TResponse ComposableTemplateExistsForAll<TResponse>(
            string name,
            ComposableIndexTemplateExistsRequestParameters requestParameters = null
        )
            where TResponse : class, IOpenSearchResponse, new() =>
            DoRequest<TResponse>(
                HEAD,
                Url($"_index_template/{name:name}"),
                null,
                RequestParams(requestParameters)
            );

        /// <summary>HEAD on /_index_template/{name} <para>https://opensearch.org/docs/latest/im-plugin/index-templates/</para></summary>
        /// <param name="name">Name of the index template to check existence of. Wildcard (*) expressions are supported.</param>
        /// <param name="requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
        [MapsApi("indices.exists_index_template", "name")]
        public Task<TResponse> ComposableTemplateExistsForAllAsync<TResponse>(
            string name,
            ComposableIndexTemplateExistsRequestParameters requestParameters = null,
            CancellationToken ctx = default
        )
            where TResponse : class, IOpenSearchResponse, new() =>
            DoRequestAsync<TResponse>(
                HEAD,
                Url($"_index_template/{name:name}"),
                ctx,
                null,
                RequestParams(requestParameters)
            );

        /// <summary>GET on /_index_template <para>https://opensearch.org/docs/latest/im-plugin/index-templates/</para></summary>
        /// <param name="requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
        public TResponse GetComposableTemplateForAll<TResponse>(
            GetComposableIndexTemplateRequestParameters requestParameters = null
        )
            where TResponse : class, IOpenSearchResponse, new() =>
            DoRequest<TResponse>(GET, "_index_template", null, RequestParams(requestParameters));

        /// <summary>GET on /_index_template <para>https://opensearch.org/docs/latest/im-plugin/index-templates/</para></summary>
        /// <param name="requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
        [MapsApi("indices.get_index_template", "")]
        public Task<TResponse> GetComposableTemplateForAllAsync<TResponse>(
            GetComposableIndexTemplateRequestParameters requestParameters = null,
            CancellationToken ctx = default
        )
            where TResponse : class, IOpenSearchResponse, new() =>
            DoRequestAsync<TResponse>(
                GET,
                "_index_template",
                ctx,
                null,
                RequestParams(requestParameters)
            );

        /// <summary>GET on /_index_template/{name} <para>https://opensearch.org/docs/latest/im-plugin/index-templates/</para></summary>
        /// <param name="name">Name of the index template to retrieve. Wildcard (*) expressions are supported.</param>
        /// <param name="requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
        public TResponse GetComposableTemplateForAll<TResponse>(
            string name,
            GetComposableIndexTemplateRequestParameters requestParameters = null
        )
            where TResponse : class, IOpenSearchResponse, new() =>
            DoRequest<TResponse>(
                GET,
                Url($"_index_template/{name:name}"),
                null,
                RequestParams(requestParameters)
            );

        /// <summary>GET on /_index_template/{name} <para>https://opensearch.org/docs/latest/im-plugin/index-templates/</para></summary>
        /// <param name="name">Name of the index template to retrieve. Wildcard (*) expressions are supported.</param>
        /// <param name="requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
        [MapsApi("indices.get_index_template", "name")]
        public Task<TResponse> GetComposableTemplateForAllAsync<TResponse>(
            string name,
            GetComposableIndexTemplateRequestParameters requestParameters = null,
            CancellationToken ctx = default
        )
            where TResponse : class, IOpenSearchResponse, new() =>
            DoRequestAsync<TResponse>(
                GET,
                Url($"_index_template/{name:name}"),
                ctx,
                null,
                RequestParams(requestParameters)
            );

        /// <summary>PUT on /_index_template/{name}</summary>
        /// <param name="name">Index or template name.</param>
        /// <param name="body">The template definition.</param>
        /// <param name="requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
        public TResponse PutComposableTemplateForAll<TResponse>(
            string name,
            PostData body,
            PutComposableIndexTemplateRequestParameters requestParameters = null
        )
            where TResponse : class, IOpenSearchResponse, new() =>
            DoRequest<TResponse>(
                PUT,
                Url($"_index_template/{name:name}"),
                body,
                RequestParams(requestParameters)
            );

        /// <summary>PUT on /_index_template/{name}</summary>
        /// <param name="name">Index or template name.</param>
        /// <param name="body">The template definition.</param>
        /// <param name="requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
        [MapsApi("indices.put_index_template", "name, body")]
        public Task<TResponse> PutComposableTemplateForAllAsync<TResponse>(
            string name,
            PostData body,
            PutComposableIndexTemplateRequestParameters requestParameters = null,
            CancellationToken ctx = default
        )
            where TResponse : class, IOpenSearchResponse, new() =>
            DoRequestAsync<TResponse>(
                PUT,
                Url($"_index_template/{name:name}"),
                ctx,
                body,
                RequestParams(requestParameters)
            );
    }
}
