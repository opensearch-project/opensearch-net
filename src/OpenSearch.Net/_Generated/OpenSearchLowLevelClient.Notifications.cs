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
namespace OpenSearch.Net.Specification.NotificationsApi
{
    /// <summary>
    /// Notifications APIs.
    /// <para>Not intended to be instantiated directly. Use the <see cref="IOpenSearchLowLevelClient.Notifications"/> property
    /// on <see cref="IOpenSearchLowLevelClient"/>.
    /// </para>
    /// </summary>
    public partial class LowLevelNotificationsNamespace : NamespacedClientProxy
    {
        internal LowLevelNotificationsNamespace(OpenSearchLowLevelClient client)
            : base(client) { }

        /// <summary>POST on /_plugins/_notifications/configs <para>https://opensearch.org/docs/latest/observing-your-data/notifications/api/#create-channel-configuration</para></summary>
        /// <param name="body"></param>
        /// <param name="requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
        /// <remarks>Supported by OpenSearch servers of version 2.0.0 or greater.</remarks>
        public TResponse CreateConfig<TResponse>(
            PostData body,
            CreateConfigRequestParameters requestParameters = null
        )
            where TResponse : class, IOpenSearchResponse, new() =>
            DoRequest<TResponse>(
                POST,
                "_plugins/_notifications/configs",
                body,
                RequestParams(requestParameters)
            );

        /// <summary>POST on /_plugins/_notifications/configs <para>https://opensearch.org/docs/latest/observing-your-data/notifications/api/#create-channel-configuration</para></summary>
        /// <param name="body"></param>
        /// <param name="requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
        /// <remarks>Supported by OpenSearch servers of version 2.0.0 or greater.</remarks>
        [MapsApi("notifications.create_config", "body")]
        public Task<TResponse> CreateConfigAsync<TResponse>(
            PostData body,
            CreateConfigRequestParameters requestParameters = null,
            CancellationToken ctx = default
        )
            where TResponse : class, IOpenSearchResponse, new() =>
            DoRequestAsync<TResponse>(
                POST,
                "_plugins/_notifications/configs",
                ctx,
                body,
                RequestParams(requestParameters)
            );

        /// <summary>DELETE on /_plugins/_notifications/configs/{config_id} <para>https://opensearch.org/docs/latest/observing-your-data/notifications/api/#delete-channel-configuration</para></summary>
        /// <param name="configId">The ID of the channel configuration to delete.</param>
        /// <param name="requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
        /// <remarks>Supported by OpenSearch servers of version 2.0.0 or greater.</remarks>
        public TResponse DeleteConfig<TResponse>(
            string configId,
            DeleteConfigRequestParameters requestParameters = null
        )
            where TResponse : class, IOpenSearchResponse, new() =>
            DoRequest<TResponse>(
                DELETE,
                Url($"_plugins/_notifications/configs/{configId:configId}"),
                null,
                RequestParams(requestParameters)
            );

        /// <summary>DELETE on /_plugins/_notifications/configs/{config_id} <para>https://opensearch.org/docs/latest/observing-your-data/notifications/api/#delete-channel-configuration</para></summary>
        /// <param name="configId">The ID of the channel configuration to delete.</param>
        /// <param name="requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
        /// <remarks>Supported by OpenSearch servers of version 2.0.0 or greater.</remarks>
        [MapsApi("notifications.delete_config", "config_id")]
        public Task<TResponse> DeleteConfigAsync<TResponse>(
            string configId,
            DeleteConfigRequestParameters requestParameters = null,
            CancellationToken ctx = default
        )
            where TResponse : class, IOpenSearchResponse, new() =>
            DoRequestAsync<TResponse>(
                DELETE,
                Url($"_plugins/_notifications/configs/{configId:configId}"),
                ctx,
                null,
                RequestParams(requestParameters)
            );

        /// <summary>DELETE on /_plugins/_notifications/configs <para>https://opensearch.org/docs/latest/observing-your-data/notifications/api/#delete-channel-configuration</para></summary>
        /// <param name="requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
        /// <remarks>Supported by OpenSearch servers of version 2.2.0 or greater.</remarks>
        public TResponse DeleteConfigs<TResponse>(
            DeleteConfigsRequestParameters requestParameters = null
        )
            where TResponse : class, IOpenSearchResponse, new() =>
            DoRequest<TResponse>(
                DELETE,
                "_plugins/_notifications/configs",
                null,
                RequestParams(requestParameters)
            );

        /// <summary>DELETE on /_plugins/_notifications/configs <para>https://opensearch.org/docs/latest/observing-your-data/notifications/api/#delete-channel-configuration</para></summary>
        /// <param name="requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
        /// <remarks>Supported by OpenSearch servers of version 2.2.0 or greater.</remarks>
        [MapsApi("notifications.delete_configs", "")]
        public Task<TResponse> DeleteConfigsAsync<TResponse>(
            DeleteConfigsRequestParameters requestParameters = null,
            CancellationToken ctx = default
        )
            where TResponse : class, IOpenSearchResponse, new() =>
            DoRequestAsync<TResponse>(
                DELETE,
                "_plugins/_notifications/configs",
                ctx,
                null,
                RequestParams(requestParameters)
            );

        /// <summary>GET on /_plugins/_notifications/configs/{config_id}</summary>
        /// <param name="configId"></param>
        /// <param name="requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
        /// <remarks>Supported by OpenSearch servers of version 2.0.0 or greater.</remarks>
        public TResponse GetConfig<TResponse>(
            string configId,
            GetConfigRequestParameters requestParameters = null
        )
            where TResponse : class, IOpenSearchResponse, new() =>
            DoRequest<TResponse>(
                GET,
                Url($"_plugins/_notifications/configs/{configId:configId}"),
                null,
                RequestParams(requestParameters)
            );

        /// <summary>GET on /_plugins/_notifications/configs/{config_id}</summary>
        /// <param name="configId"></param>
        /// <param name="requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
        /// <remarks>Supported by OpenSearch servers of version 2.0.0 or greater.</remarks>
        [MapsApi("notifications.get_config", "config_id")]
        public Task<TResponse> GetConfigAsync<TResponse>(
            string configId,
            GetConfigRequestParameters requestParameters = null,
            CancellationToken ctx = default
        )
            where TResponse : class, IOpenSearchResponse, new() =>
            DoRequestAsync<TResponse>(
                GET,
                Url($"_plugins/_notifications/configs/{configId:configId}"),
                ctx,
                null,
                RequestParams(requestParameters)
            );

        /// <summary>GET on /_plugins/_notifications/configs <para>https://opensearch.org/docs/latest/observing-your-data/notifications/api/#list-all-notification-configurations</para></summary>
        /// <param name="body"></param>
        /// <param name="requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
        /// <remarks>Supported by OpenSearch servers of version 2.0.0 or greater.</remarks>
        public TResponse GetConfigs<TResponse>(
            PostData body,
            GetConfigsRequestParameters requestParameters = null
        )
            where TResponse : class, IOpenSearchResponse, new() =>
            DoRequest<TResponse>(
                GET,
                "_plugins/_notifications/configs",
                body,
                RequestParams(requestParameters)
            );

        /// <summary>GET on /_plugins/_notifications/configs <para>https://opensearch.org/docs/latest/observing-your-data/notifications/api/#list-all-notification-configurations</para></summary>
        /// <param name="body"></param>
        /// <param name="requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
        /// <remarks>Supported by OpenSearch servers of version 2.0.0 or greater.</remarks>
        [MapsApi("notifications.get_configs", "body")]
        public Task<TResponse> GetConfigsAsync<TResponse>(
            PostData body,
            GetConfigsRequestParameters requestParameters = null,
            CancellationToken ctx = default
        )
            where TResponse : class, IOpenSearchResponse, new() =>
            DoRequestAsync<TResponse>(
                GET,
                "_plugins/_notifications/configs",
                ctx,
                body,
                RequestParams(requestParameters)
            );

        /// <summary>GET on /_plugins/_notifications/channels <para>https://opensearch.org/docs/latest/observing-your-data/notifications/api/#list-all-notification-channels</para></summary>
        /// <param name="requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
        /// <remarks>Supported by OpenSearch servers of version 2.0.0 or greater.</remarks>
        public TResponse ListChannels<TResponse>(
            ListChannelsRequestParameters requestParameters = null
        )
            where TResponse : class, IOpenSearchResponse, new() =>
            DoRequest<TResponse>(
                GET,
                "_plugins/_notifications/channels",
                null,
                RequestParams(requestParameters)
            );

        /// <summary>GET on /_plugins/_notifications/channels <para>https://opensearch.org/docs/latest/observing-your-data/notifications/api/#list-all-notification-channels</para></summary>
        /// <param name="requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
        /// <remarks>Supported by OpenSearch servers of version 2.0.0 or greater.</remarks>
        [MapsApi("notifications.list_channels", "")]
        public Task<TResponse> ListChannelsAsync<TResponse>(
            ListChannelsRequestParameters requestParameters = null,
            CancellationToken ctx = default
        )
            where TResponse : class, IOpenSearchResponse, new() =>
            DoRequestAsync<TResponse>(
                GET,
                "_plugins/_notifications/channels",
                ctx,
                null,
                RequestParams(requestParameters)
            );

        /// <summary>GET on /_plugins/_notifications/features <para>https://opensearch.org/docs/latest/observing-your-data/notifications/api/#list-supported-channel-configurations</para></summary>
        /// <param name="requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
        /// <remarks>Supported by OpenSearch servers of version 2.0.0 or greater.</remarks>
        public TResponse ListFeatures<TResponse>(
            ListFeaturesRequestParameters requestParameters = null
        )
            where TResponse : class, IOpenSearchResponse, new() =>
            DoRequest<TResponse>(
                GET,
                "_plugins/_notifications/features",
                null,
                RequestParams(requestParameters)
            );

        /// <summary>GET on /_plugins/_notifications/features <para>https://opensearch.org/docs/latest/observing-your-data/notifications/api/#list-supported-channel-configurations</para></summary>
        /// <param name="requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
        /// <remarks>Supported by OpenSearch servers of version 2.0.0 or greater.</remarks>
        [MapsApi("notifications.list_features", "")]
        public Task<TResponse> ListFeaturesAsync<TResponse>(
            ListFeaturesRequestParameters requestParameters = null,
            CancellationToken ctx = default
        )
            where TResponse : class, IOpenSearchResponse, new() =>
            DoRequestAsync<TResponse>(
                GET,
                "_plugins/_notifications/features",
                ctx,
                null,
                RequestParams(requestParameters)
            );

        /// <summary>POST on /_plugins/_notifications/feature/test/{config_id} <para>https://opensearch.org/docs/latest/observing-your-data/notifications/api/#send-test-notification</para></summary>
        /// <param name="configId"></param>
        /// <param name="requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
        /// <remarks>Supported by OpenSearch servers of version 2.0.0 or greater.</remarks>
        [Obsolete("Deprecated in version 2.3.0: Use the POST method instead.")]
        public TResponse SendTest<TResponse>(
            string configId,
            SendTestRequestParameters requestParameters = null
        )
            where TResponse : class, IOpenSearchResponse, new() =>
            DoRequest<TResponse>(
                POST,
                Url($"_plugins/_notifications/feature/test/{configId:configId}"),
                null,
                RequestParams(requestParameters)
            );

        /// <summary>POST on /_plugins/_notifications/feature/test/{config_id} <para>https://opensearch.org/docs/latest/observing-your-data/notifications/api/#send-test-notification</para></summary>
        /// <param name="configId"></param>
        /// <param name="requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
        /// <remarks>Supported by OpenSearch servers of version 2.0.0 or greater.</remarks>
        [Obsolete("Deprecated in version 2.3.0: Use the POST method instead.")]
        [MapsApi("notifications.send_test", "config_id")]
        public Task<TResponse> SendTestAsync<TResponse>(
            string configId,
            SendTestRequestParameters requestParameters = null,
            CancellationToken ctx = default
        )
            where TResponse : class, IOpenSearchResponse, new() =>
            DoRequestAsync<TResponse>(
                POST,
                Url($"_plugins/_notifications/feature/test/{configId:configId}"),
                ctx,
                null,
                RequestParams(requestParameters)
            );

        /// <summary>PUT on /_plugins/_notifications/configs/{config_id} <para>https://opensearch.org/docs/latest/observing-your-data/notifications/api/#update-channel-configuration</para></summary>
        /// <param name="configId"></param>
        /// <param name="body"></param>
        /// <param name="requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
        /// <remarks>Supported by OpenSearch servers of version 2.0.0 or greater.</remarks>
        public TResponse UpdateConfig<TResponse>(
            string configId,
            PostData body,
            UpdateConfigRequestParameters requestParameters = null
        )
            where TResponse : class, IOpenSearchResponse, new() =>
            DoRequest<TResponse>(
                PUT,
                Url($"_plugins/_notifications/configs/{configId:configId}"),
                body,
                RequestParams(requestParameters)
            );

        /// <summary>PUT on /_plugins/_notifications/configs/{config_id} <para>https://opensearch.org/docs/latest/observing-your-data/notifications/api/#update-channel-configuration</para></summary>
        /// <param name="configId"></param>
        /// <param name="body"></param>
        /// <param name="requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
        /// <remarks>Supported by OpenSearch servers of version 2.0.0 or greater.</remarks>
        [MapsApi("notifications.update_config", "config_id, body")]
        public Task<TResponse> UpdateConfigAsync<TResponse>(
            string configId,
            PostData body,
            UpdateConfigRequestParameters requestParameters = null,
            CancellationToken ctx = default
        )
            where TResponse : class, IOpenSearchResponse, new() =>
            DoRequestAsync<TResponse>(
                PUT,
                Url($"_plugins/_notifications/configs/{configId:configId}"),
                ctx,
                body,
                RequestParams(requestParameters)
            );
    }
}
