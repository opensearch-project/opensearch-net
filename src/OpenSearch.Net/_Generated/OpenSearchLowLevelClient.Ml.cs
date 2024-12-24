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
namespace OpenSearch.Net.Specification.MlApi
{
    /// <summary>
    /// Ml APIs.
    /// <para>Not intended to be instantiated directly. Use the <see cref="IOpenSearchLowLevelClient.Ml"/> property
    /// on <see cref="IOpenSearchLowLevelClient"/>.
    /// </para>
    /// </summary>
    public partial class LowLevelMlNamespace : NamespacedClientProxy
    {
        internal LowLevelMlNamespace(OpenSearchLowLevelClient client)
            : base(client) { }

        /// <summary>POST on /_plugins/_ml/connectors/_create</summary>
        /// <param name="body"></param>
        /// <param name="requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
        public TResponse CreateConnector<TResponse>(
            PostData body,
            CreateConnectorRequestParameters requestParameters = null
        )
            where TResponse : class, IOpenSearchResponse, new() =>
            DoRequest<TResponse>(
                POST,
                "_plugins/_ml/connectors/_create",
                body,
                RequestParams(requestParameters)
            );

        /// <summary>POST on /_plugins/_ml/connectors/_create</summary>
        /// <param name="body"></param>
        /// <param name="requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
        [MapsApi("ml.create_connector", "body")]
        public Task<TResponse> CreateConnectorAsync<TResponse>(
            PostData body,
            CreateConnectorRequestParameters requestParameters = null,
            CancellationToken ctx = default
        )
            where TResponse : class, IOpenSearchResponse, new() =>
            DoRequestAsync<TResponse>(
                POST,
                "_plugins/_ml/connectors/_create",
                ctx,
                body,
                RequestParams(requestParameters)
            );

        /// <summary>DELETE on /_plugins/_ml/agents/{agent_id}</summary>
        /// <param name="agentId"></param>
        /// <param name="requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
        public TResponse DeleteAgent<TResponse>(
            string agentId,
            DeleteAgentRequestParameters requestParameters = null
        )
            where TResponse : class, IOpenSearchResponse, new() =>
            DoRequest<TResponse>(
                DELETE,
                Url($"_plugins/_ml/agents/{agentId:agentId}"),
                null,
                RequestParams(requestParameters)
            );

        /// <summary>DELETE on /_plugins/_ml/agents/{agent_id}</summary>
        /// <param name="agentId"></param>
        /// <param name="requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
        [MapsApi("ml.delete_agent", "agent_id")]
        public Task<TResponse> DeleteAgentAsync<TResponse>(
            string agentId,
            DeleteAgentRequestParameters requestParameters = null,
            CancellationToken ctx = default
        )
            where TResponse : class, IOpenSearchResponse, new() =>
            DoRequestAsync<TResponse>(
                DELETE,
                Url($"_plugins/_ml/agents/{agentId:agentId}"),
                ctx,
                null,
                RequestParams(requestParameters)
            );

        /// <summary>DELETE on /_plugins/_ml/connectors/{connector_id}</summary>
        /// <param name="connectorId"></param>
        /// <param name="requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
        public TResponse DeleteConnector<TResponse>(
            string connectorId,
            DeleteConnectorRequestParameters requestParameters = null
        )
            where TResponse : class, IOpenSearchResponse, new() =>
            DoRequest<TResponse>(
                DELETE,
                Url($"_plugins/_ml/connectors/{connectorId:connectorId}"),
                null,
                RequestParams(requestParameters)
            );

        /// <summary>DELETE on /_plugins/_ml/connectors/{connector_id}</summary>
        /// <param name="connectorId"></param>
        /// <param name="requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
        [MapsApi("ml.delete_connector", "connector_id")]
        public Task<TResponse> DeleteConnectorAsync<TResponse>(
            string connectorId,
            DeleteConnectorRequestParameters requestParameters = null,
            CancellationToken ctx = default
        )
            where TResponse : class, IOpenSearchResponse, new() =>
            DoRequestAsync<TResponse>(
                DELETE,
                Url($"_plugins/_ml/connectors/{connectorId:connectorId}"),
                ctx,
                null,
                RequestParams(requestParameters)
            );

        /// <summary>DELETE on /_plugins/_ml/models/{model_id}</summary>
        /// <param name="modelId"></param>
        /// <param name="requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
        public TResponse DeleteModel<TResponse>(
            string modelId,
            DeleteModelRequestParameters requestParameters = null
        )
            where TResponse : class, IOpenSearchResponse, new() =>
            DoRequest<TResponse>(
                DELETE,
                Url($"_plugins/_ml/models/{modelId:modelId}"),
                null,
                RequestParams(requestParameters)
            );

        /// <summary>DELETE on /_plugins/_ml/models/{model_id}</summary>
        /// <param name="modelId"></param>
        /// <param name="requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
        [MapsApi("ml.delete_model", "model_id")]
        public Task<TResponse> DeleteModelAsync<TResponse>(
            string modelId,
            DeleteModelRequestParameters requestParameters = null,
            CancellationToken ctx = default
        )
            where TResponse : class, IOpenSearchResponse, new() =>
            DoRequestAsync<TResponse>(
                DELETE,
                Url($"_plugins/_ml/models/{modelId:modelId}"),
                ctx,
                null,
                RequestParams(requestParameters)
            );

        /// <summary>DELETE on /_plugins/_ml/model_groups/{model_group_id}</summary>
        /// <param name="modelGroupId"></param>
        /// <param name="requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
        public TResponse DeleteModelGroup<TResponse>(
            string modelGroupId,
            DeleteModelGroupRequestParameters requestParameters = null
        )
            where TResponse : class, IOpenSearchResponse, new() =>
            DoRequest<TResponse>(
                DELETE,
                Url($"_plugins/_ml/model_groups/{modelGroupId:modelGroupId}"),
                null,
                RequestParams(requestParameters)
            );

        /// <summary>DELETE on /_plugins/_ml/model_groups/{model_group_id}</summary>
        /// <param name="modelGroupId"></param>
        /// <param name="requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
        [MapsApi("ml.delete_model_group", "model_group_id")]
        public Task<TResponse> DeleteModelGroupAsync<TResponse>(
            string modelGroupId,
            DeleteModelGroupRequestParameters requestParameters = null,
            CancellationToken ctx = default
        )
            where TResponse : class, IOpenSearchResponse, new() =>
            DoRequestAsync<TResponse>(
                DELETE,
                Url($"_plugins/_ml/model_groups/{modelGroupId:modelGroupId}"),
                ctx,
                null,
                RequestParams(requestParameters)
            );

        /// <summary>DELETE on /_plugins/_ml/tasks/{task_id}</summary>
        /// <param name="taskId"></param>
        /// <param name="requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
        public TResponse DeleteTask<TResponse>(
            string taskId,
            DeleteTaskRequestParameters requestParameters = null
        )
            where TResponse : class, IOpenSearchResponse, new() =>
            DoRequest<TResponse>(
                DELETE,
                Url($"_plugins/_ml/tasks/{taskId:taskId}"),
                null,
                RequestParams(requestParameters)
            );

        /// <summary>DELETE on /_plugins/_ml/tasks/{task_id}</summary>
        /// <param name="taskId"></param>
        /// <param name="requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
        [MapsApi("ml.delete_task", "task_id")]
        public Task<TResponse> DeleteTaskAsync<TResponse>(
            string taskId,
            DeleteTaskRequestParameters requestParameters = null,
            CancellationToken ctx = default
        )
            where TResponse : class, IOpenSearchResponse, new() =>
            DoRequestAsync<TResponse>(
                DELETE,
                Url($"_plugins/_ml/tasks/{taskId:taskId}"),
                ctx,
                null,
                RequestParams(requestParameters)
            );

        /// <summary>POST on /_plugins/_ml/models/{model_id}/_deploy</summary>
        /// <param name="modelId"></param>
        /// <param name="requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
        public TResponse DeployModel<TResponse>(
            string modelId,
            DeployModelRequestParameters requestParameters = null
        )
            where TResponse : class, IOpenSearchResponse, new() =>
            DoRequest<TResponse>(
                POST,
                Url($"_plugins/_ml/models/{modelId:modelId}/_deploy"),
                null,
                RequestParams(requestParameters)
            );

        /// <summary>POST on /_plugins/_ml/models/{model_id}/_deploy</summary>
        /// <param name="modelId"></param>
        /// <param name="requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
        [MapsApi("ml.deploy_model", "model_id")]
        public Task<TResponse> DeployModelAsync<TResponse>(
            string modelId,
            DeployModelRequestParameters requestParameters = null,
            CancellationToken ctx = default
        )
            where TResponse : class, IOpenSearchResponse, new() =>
            DoRequestAsync<TResponse>(
                POST,
                Url($"_plugins/_ml/models/{modelId:modelId}/_deploy"),
                ctx,
                null,
                RequestParams(requestParameters)
            );

        /// <summary>GET on /_plugins/_ml/model_groups/{model_group_id}</summary>
        /// <param name="modelGroupId"></param>
        /// <param name="requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
        public TResponse GetModelGroup<TResponse>(
            string modelGroupId,
            GetModelGroupRequestParameters requestParameters = null
        )
            where TResponse : class, IOpenSearchResponse, new() =>
            DoRequest<TResponse>(
                GET,
                Url($"_plugins/_ml/model_groups/{modelGroupId:modelGroupId}"),
                null,
                RequestParams(requestParameters)
            );

        /// <summary>GET on /_plugins/_ml/model_groups/{model_group_id}</summary>
        /// <param name="modelGroupId"></param>
        /// <param name="requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
        [MapsApi("ml.get_model_group", "model_group_id")]
        public Task<TResponse> GetModelGroupAsync<TResponse>(
            string modelGroupId,
            GetModelGroupRequestParameters requestParameters = null,
            CancellationToken ctx = default
        )
            where TResponse : class, IOpenSearchResponse, new() =>
            DoRequestAsync<TResponse>(
                GET,
                Url($"_plugins/_ml/model_groups/{modelGroupId:modelGroupId}"),
                ctx,
                null,
                RequestParams(requestParameters)
            );

        /// <summary>GET on /_plugins/_ml/tasks/{task_id}</summary>
        /// <param name="taskId"></param>
        /// <param name="requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
        public TResponse GetTask<TResponse>(
            string taskId,
            GetTaskRequestParameters requestParameters = null
        )
            where TResponse : class, IOpenSearchResponse, new() =>
            DoRequest<TResponse>(
                GET,
                Url($"_plugins/_ml/tasks/{taskId:taskId}"),
                null,
                RequestParams(requestParameters)
            );

        /// <summary>GET on /_plugins/_ml/tasks/{task_id}</summary>
        /// <param name="taskId"></param>
        /// <param name="requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
        [MapsApi("ml.get_task", "task_id")]
        public Task<TResponse> GetTaskAsync<TResponse>(
            string taskId,
            GetTaskRequestParameters requestParameters = null,
            CancellationToken ctx = default
        )
            where TResponse : class, IOpenSearchResponse, new() =>
            DoRequestAsync<TResponse>(
                GET,
                Url($"_plugins/_ml/tasks/{taskId:taskId}"),
                ctx,
                null,
                RequestParams(requestParameters)
            );

        /// <summary>POST on /_plugins/_ml/agents/_register</summary>
        /// <param name="body"></param>
        /// <param name="requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
        public TResponse RegisterAgents<TResponse>(
            PostData body,
            RegisterAgentsRequestParameters requestParameters = null
        )
            where TResponse : class, IOpenSearchResponse, new() =>
            DoRequest<TResponse>(
                POST,
                "_plugins/_ml/agents/_register",
                body,
                RequestParams(requestParameters)
            );

        /// <summary>POST on /_plugins/_ml/agents/_register</summary>
        /// <param name="body"></param>
        /// <param name="requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
        [MapsApi("ml.register_agents", "body")]
        public Task<TResponse> RegisterAgentsAsync<TResponse>(
            PostData body,
            RegisterAgentsRequestParameters requestParameters = null,
            CancellationToken ctx = default
        )
            where TResponse : class, IOpenSearchResponse, new() =>
            DoRequestAsync<TResponse>(
                POST,
                "_plugins/_ml/agents/_register",
                ctx,
                body,
                RequestParams(requestParameters)
            );

        /// <summary>POST on /_plugins/_ml/models/_register</summary>
        /// <param name="body"></param>
        /// <param name="requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
        public TResponse RegisterModel<TResponse>(
            PostData body,
            RegisterModelRequestParameters requestParameters = null
        )
            where TResponse : class, IOpenSearchResponse, new() =>
            DoRequest<TResponse>(
                POST,
                "_plugins/_ml/models/_register",
                body,
                RequestParams(requestParameters)
            );

        /// <summary>POST on /_plugins/_ml/models/_register</summary>
        /// <param name="body"></param>
        /// <param name="requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
        [MapsApi("ml.register_model", "body")]
        public Task<TResponse> RegisterModelAsync<TResponse>(
            PostData body,
            RegisterModelRequestParameters requestParameters = null,
            CancellationToken ctx = default
        )
            where TResponse : class, IOpenSearchResponse, new() =>
            DoRequestAsync<TResponse>(
                POST,
                "_plugins/_ml/models/_register",
                ctx,
                body,
                RequestParams(requestParameters)
            );

        /// <summary>POST on /_plugins/_ml/model_groups/_register</summary>
        /// <param name="body"></param>
        /// <param name="requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
        public TResponse RegisterModelGroup<TResponse>(
            PostData body,
            RegisterModelGroupRequestParameters requestParameters = null
        )
            where TResponse : class, IOpenSearchResponse, new() =>
            DoRequest<TResponse>(
                POST,
                "_plugins/_ml/model_groups/_register",
                body,
                RequestParams(requestParameters)
            );

        /// <summary>POST on /_plugins/_ml/model_groups/_register</summary>
        /// <param name="body"></param>
        /// <param name="requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
        [MapsApi("ml.register_model_group", "body")]
        public Task<TResponse> RegisterModelGroupAsync<TResponse>(
            PostData body,
            RegisterModelGroupRequestParameters requestParameters = null,
            CancellationToken ctx = default
        )
            where TResponse : class, IOpenSearchResponse, new() =>
            DoRequestAsync<TResponse>(
                POST,
                "_plugins/_ml/model_groups/_register",
                ctx,
                body,
                RequestParams(requestParameters)
            );

        /// <summary>GET on /_plugins/_ml/models/_search</summary>
        /// <param name="body"></param>
        /// <param name="requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
        public TResponse SearchModels<TResponse>(
            PostData body,
            SearchModelsRequestParameters requestParameters = null
        )
            where TResponse : class, IOpenSearchResponse, new() =>
            DoRequest<TResponse>(
                GET,
                "_plugins/_ml/models/_search",
                body,
                RequestParams(requestParameters)
            );

        /// <summary>GET on /_plugins/_ml/models/_search</summary>
        /// <param name="body"></param>
        /// <param name="requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
        [MapsApi("ml.search_models", "body")]
        public Task<TResponse> SearchModelsAsync<TResponse>(
            PostData body,
            SearchModelsRequestParameters requestParameters = null,
            CancellationToken ctx = default
        )
            where TResponse : class, IOpenSearchResponse, new() =>
            DoRequestAsync<TResponse>(
                GET,
                "_plugins/_ml/models/_search",
                ctx,
                body,
                RequestParams(requestParameters)
            );

        /// <summary>POST on /_plugins/_ml/models/{model_id}/_undeploy</summary>
        /// <param name="modelId"></param>
        /// <param name="requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
        public TResponse UndeployModel<TResponse>(
            string modelId,
            UndeployModelRequestParameters requestParameters = null
        )
            where TResponse : class, IOpenSearchResponse, new() =>
            DoRequest<TResponse>(
                POST,
                Url($"_plugins/_ml/models/{modelId:modelId}/_undeploy"),
                null,
                RequestParams(requestParameters)
            );

        /// <summary>POST on /_plugins/_ml/models/{model_id}/_undeploy</summary>
        /// <param name="modelId"></param>
        /// <param name="requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
        [MapsApi("ml.undeploy_model", "model_id")]
        public Task<TResponse> UndeployModelAsync<TResponse>(
            string modelId,
            UndeployModelRequestParameters requestParameters = null,
            CancellationToken ctx = default
        )
            where TResponse : class, IOpenSearchResponse, new() =>
            DoRequestAsync<TResponse>(
                POST,
                Url($"_plugins/_ml/models/{modelId:modelId}/_undeploy"),
                ctx,
                null,
                RequestParams(requestParameters)
            );
    }
}