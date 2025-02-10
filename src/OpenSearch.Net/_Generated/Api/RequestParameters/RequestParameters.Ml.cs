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

// ReSharper disable once CheckNamespace
namespace OpenSearch.Net.Specification.MlApi
{
    /// <summary>Request options for ChunkModel</summary>
    public partial class ChunkModelRequestParameters
        : RequestParameters<ChunkModelRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.POST;
        public override bool SupportsBody => true;
    }

    /// <summary>Request options for CreateConnector</summary>
    public partial class CreateConnectorRequestParameters
        : RequestParameters<CreateConnectorRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.POST;
        public override bool SupportsBody => true;
    }

    /// <summary>Request options for CreateController</summary>
    public partial class CreateControllerRequestParameters
        : RequestParameters<CreateControllerRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.POST;
        public override bool SupportsBody => true;
    }

    /// <summary>Request options for CreateMemory</summary>
    public partial class CreateMemoryRequestParameters
        : RequestParameters<CreateMemoryRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.POST;
        public override bool SupportsBody => true;
    }

    /// <summary>Request options for CreateMessage</summary>
    public partial class CreateMessageRequestParameters
        : RequestParameters<CreateMessageRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.POST;
        public override bool SupportsBody => true;
    }

    /// <summary>Request options for CreateModelMeta</summary>
    public partial class CreateModelMetaRequestParameters
        : RequestParameters<CreateModelMetaRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.POST;
        public override bool SupportsBody => true;
    }

    /// <summary>Request options for DeleteAgent</summary>
    public partial class DeleteAgentRequestParameters
        : RequestParameters<DeleteAgentRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.DELETE;
        public override bool SupportsBody => false;
    }

    /// <summary>Request options for DeleteConnector</summary>
    public partial class DeleteConnectorRequestParameters
        : RequestParameters<DeleteConnectorRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.DELETE;
        public override bool SupportsBody => false;
    }

    /// <summary>Request options for DeleteController</summary>
    public partial class DeleteControllerRequestParameters
        : RequestParameters<DeleteControllerRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.DELETE;
        public override bool SupportsBody => false;
    }

    /// <summary>Request options for DeleteMemory</summary>
    public partial class DeleteMemoryRequestParameters
        : RequestParameters<DeleteMemoryRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.DELETE;
        public override bool SupportsBody => false;
    }

    /// <summary>Request options for DeleteModel</summary>
    public partial class DeleteModelRequestParameters
        : RequestParameters<DeleteModelRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.DELETE;
        public override bool SupportsBody => false;
    }

    /// <summary>Request options for DeleteModelGroup</summary>
    public partial class DeleteModelGroupRequestParameters
        : RequestParameters<DeleteModelGroupRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.DELETE;
        public override bool SupportsBody => false;
    }

    /// <summary>Request options for DeleteTask</summary>
    public partial class DeleteTaskRequestParameters
        : RequestParameters<DeleteTaskRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.DELETE;
        public override bool SupportsBody => false;
    }

    /// <summary>Request options for DeployModel</summary>
    public partial class DeployModelRequestParameters
        : RequestParameters<DeployModelRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.POST;
        public override bool SupportsBody => false;
    }

    /// <summary>Request options for ExecuteAgent</summary>
    public partial class ExecuteAgentRequestParameters
        : RequestParameters<ExecuteAgentRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.POST;
        public override bool SupportsBody => true;
    }

    /// <summary>Request options for ExecuteAlgorithm</summary>
    public partial class ExecuteAlgorithmRequestParameters
        : RequestParameters<ExecuteAlgorithmRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.POST;
        public override bool SupportsBody => true;
    }

    /// <summary>Request options for GetAgent</summary>
    public partial class GetAgentRequestParameters : RequestParameters<GetAgentRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.GET;
        public override bool SupportsBody => false;
    }

    /// <summary>Request options for GetAllMemories</summary>
    public partial class GetAllMemoriesRequestParameters
        : RequestParameters<GetAllMemoriesRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.GET;
        public override bool SupportsBody => false;

        /// <summary>
        /// The maximum number of results to return. If there are fewer memories than the number set in `max_results`, the response returns only the
        /// number of memories that exist. Default is `10`.
        /// </summary>
        public long? MaxResults
        {
            get => Q<long?>("max_results");
            set => Q("max_results", value);
        }

        /// <summary>
        /// The index of the first memory in the sorted list of memories to return. Memories are ordered by `create_time`. For example, if memories
        /// `A`, `B`, and `C` exist, `next_token=1` returns memories `B` and `C`. Default is `0` (return all memories).
        /// </summary>
        public long? NextToken
        {
            get => Q<long?>("next_token");
            set => Q("next_token", value);
        }
    }

    /// <summary>Request options for GetAllMessages</summary>
    public partial class GetAllMessagesRequestParameters
        : RequestParameters<GetAllMessagesRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.GET;
        public override bool SupportsBody => false;
        public long? MaxResults
        {
            get => Q<long?>("max_results");
            set => Q("max_results", value);
        }
        public long? NextToken
        {
            get => Q<long?>("next_token");
            set => Q("next_token", value);
        }
    }

    /// <summary>Request options for GetAllTools</summary>
    public partial class GetAllToolsRequestParameters
        : RequestParameters<GetAllToolsRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.GET;
        public override bool SupportsBody => false;
    }

    /// <summary>Request options for GetConnector</summary>
    public partial class GetConnectorRequestParameters
        : RequestParameters<GetConnectorRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.GET;
        public override bool SupportsBody => false;
    }

    /// <summary>Request options for GetController</summary>
    public partial class GetControllerRequestParameters
        : RequestParameters<GetControllerRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.GET;
        public override bool SupportsBody => false;
    }

    /// <summary>Request options for GetMemory</summary>
    public partial class GetMemoryRequestParameters : RequestParameters<GetMemoryRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.GET;
        public override bool SupportsBody => false;
    }

    /// <summary>Request options for GetMessage</summary>
    public partial class GetMessageRequestParameters
        : RequestParameters<GetMessageRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.GET;
        public override bool SupportsBody => false;
    }

    /// <summary>Request options for GetMessageTraces</summary>
    public partial class GetMessageTracesRequestParameters
        : RequestParameters<GetMessageTracesRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.GET;
        public override bool SupportsBody => false;
        public long? MaxResults
        {
            get => Q<long?>("max_results");
            set => Q("max_results", value);
        }
        public long? NextToken
        {
            get => Q<long?>("next_token");
            set => Q("next_token", value);
        }
    }

    /// <summary>Request options for GetModel</summary>
    public partial class GetModelRequestParameters : RequestParameters<GetModelRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.GET;
        public override bool SupportsBody => false;
    }

    /// <summary>Request options for GetModelGroup</summary>
    public partial class GetModelGroupRequestParameters
        : RequestParameters<GetModelGroupRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.GET;
        public override bool SupportsBody => false;
    }

    /// <summary>Request options for GetProfile</summary>
    public partial class GetProfileRequestParameters
        : RequestParameters<GetProfileRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.GET;
        public override bool SupportsBody => true;
    }

    /// <summary>Request options for GetProfileModels</summary>
    public partial class GetProfileModelsRequestParameters
        : RequestParameters<GetProfileModelsRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.GET;
        public override bool SupportsBody => true;
    }

    /// <summary>Request options for GetProfileTasks</summary>
    public partial class GetProfileTasksRequestParameters
        : RequestParameters<GetProfileTasksRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.GET;
        public override bool SupportsBody => true;
    }

    /// <summary>Request options for GetStats</summary>
    public partial class GetStatsRequestParameters : RequestParameters<GetStatsRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.GET;
        public override bool SupportsBody => false;
    }

    /// <summary>Request options for GetTask</summary>
    public partial class GetTaskRequestParameters : RequestParameters<GetTaskRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.GET;
        public override bool SupportsBody => false;
    }

    /// <summary>Request options for GetTool</summary>
    public partial class GetToolRequestParameters : RequestParameters<GetToolRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.GET;
        public override bool SupportsBody => false;
    }

    /// <summary>Request options for LoadModel</summary>
    public partial class LoadModelRequestParameters : RequestParameters<LoadModelRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.POST;
        public override bool SupportsBody => false;
    }

    /// <summary>Request options for Predict</summary>
    public partial class PredictRequestParameters : RequestParameters<PredictRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.POST;
        public override bool SupportsBody => true;
    }

    /// <summary>Request options for PredictModel</summary>
    public partial class PredictModelRequestParameters
        : RequestParameters<PredictModelRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.POST;
        public override bool SupportsBody => true;
    }

    /// <summary>Request options for RegisterAgents</summary>
    public partial class RegisterAgentsRequestParameters
        : RequestParameters<RegisterAgentsRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.POST;
        public override bool SupportsBody => true;
    }

    /// <summary>Request options for RegisterModel</summary>
    public partial class RegisterModelRequestParameters
        : RequestParameters<RegisterModelRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.POST;
        public override bool SupportsBody => true;
    }

    /// <summary>Request options for RegisterModelGroup</summary>
    public partial class RegisterModelGroupRequestParameters
        : RequestParameters<RegisterModelGroupRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.POST;
        public override bool SupportsBody => true;
    }

    /// <summary>Request options for RegisterModelMeta</summary>
    public partial class RegisterModelMetaRequestParameters
        : RequestParameters<RegisterModelMetaRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.POST;
        public override bool SupportsBody => true;
    }

    /// <summary>Request options for SearchAgents</summary>
    public partial class SearchAgentsRequestParameters
        : RequestParameters<SearchAgentsRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.POST;
        public override bool SupportsBody => true;
    }

    /// <summary>Request options for SearchConnectors</summary>
    public partial class SearchConnectorsRequestParameters
        : RequestParameters<SearchConnectorsRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.POST;
        public override bool SupportsBody => true;
    }

    /// <summary>Request options for SearchMemory</summary>
    public partial class SearchMemoryRequestParameters
        : RequestParameters<SearchMemoryRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.POST;
        public override bool SupportsBody => true;
    }

    /// <summary>Request options for SearchMessage</summary>
    public partial class SearchMessageRequestParameters
        : RequestParameters<SearchMessageRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.POST;
        public override bool SupportsBody => true;
    }

    /// <summary>Request options for SearchModelGroup</summary>
    public partial class SearchModelGroupRequestParameters
        : RequestParameters<SearchModelGroupRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.POST;
        public override bool SupportsBody => true;
    }

    /// <summary>Request options for SearchModels</summary>
    public partial class SearchModelsRequestParameters
        : RequestParameters<SearchModelsRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.POST;
        public override bool SupportsBody => true;
    }

    /// <summary>Request options for SearchTasks</summary>
    public partial class SearchTasksRequestParameters
        : RequestParameters<SearchTasksRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.POST;
        public override bool SupportsBody => true;
    }

    /// <summary>Request options for Train</summary>
    public partial class TrainRequestParameters : RequestParameters<TrainRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.POST;
        public override bool SupportsBody => true;
    }

    /// <summary>Request options for TrainPredict</summary>
    public partial class TrainPredictRequestParameters
        : RequestParameters<TrainPredictRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.POST;
        public override bool SupportsBody => true;
    }

    /// <summary>Request options for UndeployModel</summary>
    public partial class UndeployModelRequestParameters
        : RequestParameters<UndeployModelRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.POST;
        public override bool SupportsBody => true;
    }

    /// <summary>Request options for UnloadModel</summary>
    public partial class UnloadModelRequestParameters
        : RequestParameters<UnloadModelRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.POST;
        public override bool SupportsBody => true;
    }

    /// <summary>Request options for UpdateConnector</summary>
    public partial class UpdateConnectorRequestParameters
        : RequestParameters<UpdateConnectorRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.PUT;
        public override bool SupportsBody => true;
    }

    /// <summary>Request options for UpdateController</summary>
    public partial class UpdateControllerRequestParameters
        : RequestParameters<UpdateControllerRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.PUT;
        public override bool SupportsBody => true;
    }

    /// <summary>Request options for UpdateMemory</summary>
    public partial class UpdateMemoryRequestParameters
        : RequestParameters<UpdateMemoryRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.PUT;
        public override bool SupportsBody => true;
    }

    /// <summary>Request options for UpdateMessage</summary>
    public partial class UpdateMessageRequestParameters
        : RequestParameters<UpdateMessageRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.PUT;
        public override bool SupportsBody => true;
    }

    /// <summary>Request options for UpdateModel</summary>
    public partial class UpdateModelRequestParameters
        : RequestParameters<UpdateModelRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.PUT;
        public override bool SupportsBody => true;
    }

    /// <summary>Request options for UpdateModelGroup</summary>
    public partial class UpdateModelGroupRequestParameters
        : RequestParameters<UpdateModelGroupRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.PUT;
        public override bool SupportsBody => true;
    }

    /// <summary>Request options for UploadChunk</summary>
    public partial class UploadChunkRequestParameters
        : RequestParameters<UploadChunkRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.POST;
        public override bool SupportsBody => true;
    }

    /// <summary>Request options for UploadModel</summary>
    public partial class UploadModelRequestParameters
        : RequestParameters<UploadModelRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.POST;
        public override bool SupportsBody => true;
    }
}
