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
using System.Threading;
using System.Threading.Tasks;
using OpenSearch.Net.Specification.ClusterApi;

// ReSharper disable once CheckNamespace
// ReSharper disable RedundantTypeArgumentsOfMethod
namespace OpenSearch.Client.Specification.ClusterApi
{
    /// <summary>
    /// Cluster APIs.
    /// <para>Not intended to be instantiated directly. Use the <see cref="IOpenSearchClient.Cluster"/> property
    /// on <see cref="IOpenSearchClient"/>.
    /// </para>
    /// </summary>
    public partial class ClusterNamespace : NamespacedClientProxy
    {
        internal ClusterNamespace(OpenSearchClient client)
            : base(client) { }

        /// <summary>
        /// <c>POST</c> request to the <c>cluster.allocation_explain</c> API, read more about this API online:
        /// <para></para>
        /// <a href="https://opensearch.org/docs/latest/api-reference/cluster-api/cluster-allocation/">https://opensearch.org/docs/latest/api-reference/cluster-api/cluster-allocation/</a>
        /// </summary>
        public ClusterAllocationExplainResponse AllocationExplain(
            Func<ClusterAllocationExplainDescriptor, IClusterAllocationExplainRequest> selector =
                null
        ) => AllocationExplain(selector.InvokeOrDefault(new ClusterAllocationExplainDescriptor()));

        /// <summary>
        /// <c>POST</c> request to the <c>cluster.allocation_explain</c> API, read more about this API online:
        /// <para></para>
        /// <a href="https://opensearch.org/docs/latest/api-reference/cluster-api/cluster-allocation/">https://opensearch.org/docs/latest/api-reference/cluster-api/cluster-allocation/</a>
        /// </summary>
        public Task<ClusterAllocationExplainResponse> AllocationExplainAsync(
            Func<ClusterAllocationExplainDescriptor, IClusterAllocationExplainRequest> selector =
                null,
            CancellationToken ct = default
        ) =>
            AllocationExplainAsync(
                selector.InvokeOrDefault(new ClusterAllocationExplainDescriptor()),
                ct
            );

        /// <summary>
        /// <c>POST</c> request to the <c>cluster.allocation_explain</c> API, read more about this API online:
        /// <para></para>
        /// <a href="https://opensearch.org/docs/latest/api-reference/cluster-api/cluster-allocation/">https://opensearch.org/docs/latest/api-reference/cluster-api/cluster-allocation/</a>
        /// </summary>
        public ClusterAllocationExplainResponse AllocationExplain(
            IClusterAllocationExplainRequest request
        ) =>
            DoRequest<IClusterAllocationExplainRequest, ClusterAllocationExplainResponse>(
                request,
                request.RequestParameters
            );

        /// <summary>
        /// <c>POST</c> request to the <c>cluster.allocation_explain</c> API, read more about this API online:
        /// <para></para>
        /// <a href="https://opensearch.org/docs/latest/api-reference/cluster-api/cluster-allocation/">https://opensearch.org/docs/latest/api-reference/cluster-api/cluster-allocation/</a>
        /// </summary>
        public Task<ClusterAllocationExplainResponse> AllocationExplainAsync(
            IClusterAllocationExplainRequest request,
            CancellationToken ct = default
        ) =>
            DoRequestAsync<IClusterAllocationExplainRequest, ClusterAllocationExplainResponse>(
                request,
                request.RequestParameters,
                ct
            );

        /// <summary>
        /// <c>DELETE</c> request to the <c>cluster.delete_component_template</c> API, read more about this API online:
        /// <para></para>
        /// <a href="https://opensearch.org/docs/latest">https://opensearch.org/docs/latest</a>
        /// </summary>
        public DeleteComponentTemplateResponse DeleteComponentTemplate(
            Name name,
            Func<DeleteComponentTemplateDescriptor, IDeleteComponentTemplateRequest> selector = null
        ) =>
            DeleteComponentTemplate(
                selector.InvokeOrDefault(new DeleteComponentTemplateDescriptor(name: name))
            );

        /// <summary>
        /// <c>DELETE</c> request to the <c>cluster.delete_component_template</c> API, read more about this API online:
        /// <para></para>
        /// <a href="https://opensearch.org/docs/latest">https://opensearch.org/docs/latest</a>
        /// </summary>
        public Task<DeleteComponentTemplateResponse> DeleteComponentTemplateAsync(
            Name name,
            Func<DeleteComponentTemplateDescriptor, IDeleteComponentTemplateRequest> selector =
                null,
            CancellationToken ct = default
        ) =>
            DeleteComponentTemplateAsync(
                selector.InvokeOrDefault(new DeleteComponentTemplateDescriptor(name: name)),
                ct
            );

        /// <summary>
        /// <c>DELETE</c> request to the <c>cluster.delete_component_template</c> API, read more about this API online:
        /// <para></para>
        /// <a href="https://opensearch.org/docs/latest">https://opensearch.org/docs/latest</a>
        /// </summary>
        public DeleteComponentTemplateResponse DeleteComponentTemplate(
            IDeleteComponentTemplateRequest request
        ) =>
            DoRequest<IDeleteComponentTemplateRequest, DeleteComponentTemplateResponse>(
                request,
                request.RequestParameters
            );

        /// <summary>
        /// <c>DELETE</c> request to the <c>cluster.delete_component_template</c> API, read more about this API online:
        /// <para></para>
        /// <a href="https://opensearch.org/docs/latest">https://opensearch.org/docs/latest</a>
        /// </summary>
        public Task<DeleteComponentTemplateResponse> DeleteComponentTemplateAsync(
            IDeleteComponentTemplateRequest request,
            CancellationToken ct = default
        ) =>
            DoRequestAsync<IDeleteComponentTemplateRequest, DeleteComponentTemplateResponse>(
                request,
                request.RequestParameters,
                ct
            );

        /// <summary>
        /// <c>DELETE</c> request to the <c>cluster.delete_voting_config_exclusions</c> API, read more about this API online:
        /// <para></para>
        /// <a href="https://opensearch.org/docs/latest">https://opensearch.org/docs/latest</a>
        /// </summary>
        public DeleteVotingConfigExclusionsResponse DeleteVotingConfigExclusions(
            Func<
                DeleteVotingConfigExclusionsDescriptor,
                IDeleteVotingConfigExclusionsRequest
            > selector = null
        ) =>
            DeleteVotingConfigExclusions(
                selector.InvokeOrDefault(new DeleteVotingConfigExclusionsDescriptor())
            );

        /// <summary>
        /// <c>DELETE</c> request to the <c>cluster.delete_voting_config_exclusions</c> API, read more about this API online:
        /// <para></para>
        /// <a href="https://opensearch.org/docs/latest">https://opensearch.org/docs/latest</a>
        /// </summary>
        public Task<DeleteVotingConfigExclusionsResponse> DeleteVotingConfigExclusionsAsync(
            Func<
                DeleteVotingConfigExclusionsDescriptor,
                IDeleteVotingConfigExclusionsRequest
            > selector = null,
            CancellationToken ct = default
        ) =>
            DeleteVotingConfigExclusionsAsync(
                selector.InvokeOrDefault(new DeleteVotingConfigExclusionsDescriptor()),
                ct
            );

        /// <summary>
        /// <c>DELETE</c> request to the <c>cluster.delete_voting_config_exclusions</c> API, read more about this API online:
        /// <para></para>
        /// <a href="https://opensearch.org/docs/latest">https://opensearch.org/docs/latest</a>
        /// </summary>
        public DeleteVotingConfigExclusionsResponse DeleteVotingConfigExclusions(
            IDeleteVotingConfigExclusionsRequest request
        ) =>
            DoRequest<IDeleteVotingConfigExclusionsRequest, DeleteVotingConfigExclusionsResponse>(
                request,
                request.RequestParameters
            );

        /// <summary>
        /// <c>DELETE</c> request to the <c>cluster.delete_voting_config_exclusions</c> API, read more about this API online:
        /// <para></para>
        /// <a href="https://opensearch.org/docs/latest">https://opensearch.org/docs/latest</a>
        /// </summary>
        public Task<DeleteVotingConfigExclusionsResponse> DeleteVotingConfigExclusionsAsync(
            IDeleteVotingConfigExclusionsRequest request,
            CancellationToken ct = default
        ) =>
            DoRequestAsync<
                IDeleteVotingConfigExclusionsRequest,
                DeleteVotingConfigExclusionsResponse
            >(request, request.RequestParameters, ct);

        /// <summary>
        /// <c>HEAD</c> request to the <c>cluster.exists_component_template</c> API, read more about this API online:
        /// <para></para>
        /// <a href="https://opensearch.org/docs/latest">https://opensearch.org/docs/latest</a>
        /// </summary>
        public ExistsResponse ComponentTemplateExists(
            Name name,
            Func<ComponentTemplateExistsDescriptor, IComponentTemplateExistsRequest> selector = null
        ) =>
            ComponentTemplateExists(
                selector.InvokeOrDefault(new ComponentTemplateExistsDescriptor(name: name))
            );

        /// <summary>
        /// <c>HEAD</c> request to the <c>cluster.exists_component_template</c> API, read more about this API online:
        /// <para></para>
        /// <a href="https://opensearch.org/docs/latest">https://opensearch.org/docs/latest</a>
        /// </summary>
        public Task<ExistsResponse> ComponentTemplateExistsAsync(
            Name name,
            Func<ComponentTemplateExistsDescriptor, IComponentTemplateExistsRequest> selector =
                null,
            CancellationToken ct = default
        ) =>
            ComponentTemplateExistsAsync(
                selector.InvokeOrDefault(new ComponentTemplateExistsDescriptor(name: name)),
                ct
            );

        /// <summary>
        /// <c>HEAD</c> request to the <c>cluster.exists_component_template</c> API, read more about this API online:
        /// <para></para>
        /// <a href="https://opensearch.org/docs/latest">https://opensearch.org/docs/latest</a>
        /// </summary>
        public ExistsResponse ComponentTemplateExists(IComponentTemplateExistsRequest request) =>
            DoRequest<IComponentTemplateExistsRequest, ExistsResponse>(
                request,
                request.RequestParameters
            );

        /// <summary>
        /// <c>HEAD</c> request to the <c>cluster.exists_component_template</c> API, read more about this API online:
        /// <para></para>
        /// <a href="https://opensearch.org/docs/latest">https://opensearch.org/docs/latest</a>
        /// </summary>
        public Task<ExistsResponse> ComponentTemplateExistsAsync(
            IComponentTemplateExistsRequest request,
            CancellationToken ct = default
        ) =>
            DoRequestAsync<IComponentTemplateExistsRequest, ExistsResponse>(
                request,
                request.RequestParameters,
                ct
            );

        /// <summary>
        /// <c>GET</c> request to the <c>cluster.get_component_template</c> API, read more about this API online:
        /// <para></para>
        /// <a href="https://opensearch.org/docs/latest">https://opensearch.org/docs/latest</a>
        /// </summary>
        public GetComponentTemplateResponse GetComponentTemplate(
            Names name = null,
            Func<GetComponentTemplateDescriptor, IGetComponentTemplateRequest> selector = null
        ) =>
            GetComponentTemplate(
                selector.InvokeOrDefault(new GetComponentTemplateDescriptor().Name(name: name))
            );

        /// <summary>
        /// <c>GET</c> request to the <c>cluster.get_component_template</c> API, read more about this API online:
        /// <para></para>
        /// <a href="https://opensearch.org/docs/latest">https://opensearch.org/docs/latest</a>
        /// </summary>
        public Task<GetComponentTemplateResponse> GetComponentTemplateAsync(
            Names name = null,
            Func<GetComponentTemplateDescriptor, IGetComponentTemplateRequest> selector = null,
            CancellationToken ct = default
        ) =>
            GetComponentTemplateAsync(
                selector.InvokeOrDefault(new GetComponentTemplateDescriptor().Name(name: name)),
                ct
            );

        /// <summary>
        /// <c>GET</c> request to the <c>cluster.get_component_template</c> API, read more about this API online:
        /// <para></para>
        /// <a href="https://opensearch.org/docs/latest">https://opensearch.org/docs/latest</a>
        /// </summary>
        public GetComponentTemplateResponse GetComponentTemplate(
            IGetComponentTemplateRequest request
        ) =>
            DoRequest<IGetComponentTemplateRequest, GetComponentTemplateResponse>(
                request,
                request.RequestParameters
            );

        /// <summary>
        /// <c>GET</c> request to the <c>cluster.get_component_template</c> API, read more about this API online:
        /// <para></para>
        /// <a href="https://opensearch.org/docs/latest">https://opensearch.org/docs/latest</a>
        /// </summary>
        public Task<GetComponentTemplateResponse> GetComponentTemplateAsync(
            IGetComponentTemplateRequest request,
            CancellationToken ct = default
        ) =>
            DoRequestAsync<IGetComponentTemplateRequest, GetComponentTemplateResponse>(
                request,
                request.RequestParameters,
                ct
            );

        /// <summary>
        /// <c>GET</c> request to the <c>cluster.get_settings</c> API, read more about this API online:
        /// <para></para>
        /// <a href="https://opensearch.org/docs/latest/api-reference/cluster-api/cluster-settings/">https://opensearch.org/docs/latest/api-reference/cluster-api/cluster-settings/</a>
        /// </summary>
        public ClusterGetSettingsResponse GetSettings(
            Func<ClusterGetSettingsDescriptor, IClusterGetSettingsRequest> selector = null
        ) => GetSettings(selector.InvokeOrDefault(new ClusterGetSettingsDescriptor()));

        /// <summary>
        /// <c>GET</c> request to the <c>cluster.get_settings</c> API, read more about this API online:
        /// <para></para>
        /// <a href="https://opensearch.org/docs/latest/api-reference/cluster-api/cluster-settings/">https://opensearch.org/docs/latest/api-reference/cluster-api/cluster-settings/</a>
        /// </summary>
        public Task<ClusterGetSettingsResponse> GetSettingsAsync(
            Func<ClusterGetSettingsDescriptor, IClusterGetSettingsRequest> selector = null,
            CancellationToken ct = default
        ) => GetSettingsAsync(selector.InvokeOrDefault(new ClusterGetSettingsDescriptor()), ct);

        /// <summary>
        /// <c>GET</c> request to the <c>cluster.get_settings</c> API, read more about this API online:
        /// <para></para>
        /// <a href="https://opensearch.org/docs/latest/api-reference/cluster-api/cluster-settings/">https://opensearch.org/docs/latest/api-reference/cluster-api/cluster-settings/</a>
        /// </summary>
        public ClusterGetSettingsResponse GetSettings(IClusterGetSettingsRequest request) =>
            DoRequest<IClusterGetSettingsRequest, ClusterGetSettingsResponse>(
                request,
                request.RequestParameters
            );

        /// <summary>
        /// <c>GET</c> request to the <c>cluster.get_settings</c> API, read more about this API online:
        /// <para></para>
        /// <a href="https://opensearch.org/docs/latest/api-reference/cluster-api/cluster-settings/">https://opensearch.org/docs/latest/api-reference/cluster-api/cluster-settings/</a>
        /// </summary>
        public Task<ClusterGetSettingsResponse> GetSettingsAsync(
            IClusterGetSettingsRequest request,
            CancellationToken ct = default
        ) =>
            DoRequestAsync<IClusterGetSettingsRequest, ClusterGetSettingsResponse>(
                request,
                request.RequestParameters,
                ct
            );

        /// <summary>
        /// <c>GET</c> request to the <c>cluster.health</c> API, read more about this API online:
        /// <para></para>
        /// <a href="https://opensearch.org/docs/latest/api-reference/cluster-api/cluster-health/">https://opensearch.org/docs/latest/api-reference/cluster-api/cluster-health/</a>
        /// </summary>
        public ClusterHealthResponse Health(
            Indices index = null,
            Func<ClusterHealthDescriptor, IClusterHealthRequest> selector = null
        ) => Health(selector.InvokeOrDefault(new ClusterHealthDescriptor().Index(index: index)));

        /// <summary>
        /// <c>GET</c> request to the <c>cluster.health</c> API, read more about this API online:
        /// <para></para>
        /// <a href="https://opensearch.org/docs/latest/api-reference/cluster-api/cluster-health/">https://opensearch.org/docs/latest/api-reference/cluster-api/cluster-health/</a>
        /// </summary>
        public Task<ClusterHealthResponse> HealthAsync(
            Indices index = null,
            Func<ClusterHealthDescriptor, IClusterHealthRequest> selector = null,
            CancellationToken ct = default
        ) =>
            HealthAsync(
                selector.InvokeOrDefault(new ClusterHealthDescriptor().Index(index: index)),
                ct
            );

        /// <summary>
        /// <c>GET</c> request to the <c>cluster.health</c> API, read more about this API online:
        /// <para></para>
        /// <a href="https://opensearch.org/docs/latest/api-reference/cluster-api/cluster-health/">https://opensearch.org/docs/latest/api-reference/cluster-api/cluster-health/</a>
        /// </summary>
        public ClusterHealthResponse Health(IClusterHealthRequest request) =>
            DoRequest<IClusterHealthRequest, ClusterHealthResponse>(
                request,
                request.RequestParameters
            );

        /// <summary>
        /// <c>GET</c> request to the <c>cluster.health</c> API, read more about this API online:
        /// <para></para>
        /// <a href="https://opensearch.org/docs/latest/api-reference/cluster-api/cluster-health/">https://opensearch.org/docs/latest/api-reference/cluster-api/cluster-health/</a>
        /// </summary>
        public Task<ClusterHealthResponse> HealthAsync(
            IClusterHealthRequest request,
            CancellationToken ct = default
        ) =>
            DoRequestAsync<IClusterHealthRequest, ClusterHealthResponse>(
                request,
                request.RequestParameters,
                ct
            );

        /// <summary>
        /// <c>GET</c> request to the <c>cluster.pending_tasks</c> API, read more about this API online:
        /// <para></para>
        /// <a href="https://opensearch.org/docs/latest">https://opensearch.org/docs/latest</a>
        /// </summary>
        public ClusterPendingTasksResponse PendingTasks(
            Func<ClusterPendingTasksDescriptor, IClusterPendingTasksRequest> selector = null
        ) => PendingTasks(selector.InvokeOrDefault(new ClusterPendingTasksDescriptor()));

        /// <summary>
        /// <c>GET</c> request to the <c>cluster.pending_tasks</c> API, read more about this API online:
        /// <para></para>
        /// <a href="https://opensearch.org/docs/latest">https://opensearch.org/docs/latest</a>
        /// </summary>
        public Task<ClusterPendingTasksResponse> PendingTasksAsync(
            Func<ClusterPendingTasksDescriptor, IClusterPendingTasksRequest> selector = null,
            CancellationToken ct = default
        ) => PendingTasksAsync(selector.InvokeOrDefault(new ClusterPendingTasksDescriptor()), ct);

        /// <summary>
        /// <c>GET</c> request to the <c>cluster.pending_tasks</c> API, read more about this API online:
        /// <para></para>
        /// <a href="https://opensearch.org/docs/latest">https://opensearch.org/docs/latest</a>
        /// </summary>
        public ClusterPendingTasksResponse PendingTasks(IClusterPendingTasksRequest request) =>
            DoRequest<IClusterPendingTasksRequest, ClusterPendingTasksResponse>(
                request,
                request.RequestParameters
            );

        /// <summary>
        /// <c>GET</c> request to the <c>cluster.pending_tasks</c> API, read more about this API online:
        /// <para></para>
        /// <a href="https://opensearch.org/docs/latest">https://opensearch.org/docs/latest</a>
        /// </summary>
        public Task<ClusterPendingTasksResponse> PendingTasksAsync(
            IClusterPendingTasksRequest request,
            CancellationToken ct = default
        ) =>
            DoRequestAsync<IClusterPendingTasksRequest, ClusterPendingTasksResponse>(
                request,
                request.RequestParameters,
                ct
            );

        /// <summary>
        /// <c>PUT</c> request to the <c>cluster.put_component_template</c> API, read more about this API online:
        /// <para></para>
        /// <a></a>
        /// </summary>
        public PutComponentTemplateResponse PutComponentTemplate(
            Name name,
            Func<PutComponentTemplateDescriptor, IPutComponentTemplateRequest> selector
        ) =>
            PutComponentTemplate(
                selector.InvokeOrDefault(new PutComponentTemplateDescriptor(name: name))
            );

        /// <summary>
        /// <c>PUT</c> request to the <c>cluster.put_component_template</c> API, read more about this API online:
        /// <para></para>
        /// <a></a>
        /// </summary>
        public Task<PutComponentTemplateResponse> PutComponentTemplateAsync(
            Name name,
            Func<PutComponentTemplateDescriptor, IPutComponentTemplateRequest> selector,
            CancellationToken ct = default
        ) =>
            PutComponentTemplateAsync(
                selector.InvokeOrDefault(new PutComponentTemplateDescriptor(name: name)),
                ct
            );

        /// <summary>
        /// <c>PUT</c> request to the <c>cluster.put_component_template</c> API, read more about this API online:
        /// <para></para>
        /// <a></a>
        /// </summary>
        public PutComponentTemplateResponse PutComponentTemplate(
            IPutComponentTemplateRequest request
        ) =>
            DoRequest<IPutComponentTemplateRequest, PutComponentTemplateResponse>(
                request,
                request.RequestParameters
            );

        /// <summary>
        /// <c>PUT</c> request to the <c>cluster.put_component_template</c> API, read more about this API online:
        /// <para></para>
        /// <a></a>
        /// </summary>
        public Task<PutComponentTemplateResponse> PutComponentTemplateAsync(
            IPutComponentTemplateRequest request,
            CancellationToken ct = default
        ) =>
            DoRequestAsync<IPutComponentTemplateRequest, PutComponentTemplateResponse>(
                request,
                request.RequestParameters,
                ct
            );
    }
}
