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
using System.Text;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using OpenSearch.Net;
using OpenSearch.Net.Utf8Json;
using OpenSearch.Net.Specification.NodesApi;

// ReSharper disable RedundantBaseConstructorCall
// ReSharper disable UnusedTypeParameter
// ReSharper disable PartialMethodWithSinglePart
// ReSharper disable RedundantNameQualifier
namespace OpenSearch.Client.Specification.NodesApi
{
    [InterfaceDataContract]
    public partial interface INodesHotThreadsRequest : IRequest<NodesHotThreadsRequestParameters>
    {
        [IgnoreDataMember]
        NodeIds NodeId { get; }
    }

    ///<summary>Request for HotThreads <para>https://opensearch.org/docs/latest/api-reference/nodes-apis/nodes-hot-threads/</para></summary>
    public partial class NodesHotThreadsRequest
        : PlainRequestBase<NodesHotThreadsRequestParameters>,
            INodesHotThreadsRequest
    {
        protected INodesHotThreadsRequest Self => this;
        internal override ApiUrls ApiUrls => ApiUrlsLookups.NodesHotThreads;

        ///<summary>/_nodes/hot_threads</summary>
        public NodesHotThreadsRequest()
            : base() { }

        ///<summary>/_nodes/{node_id}/hot_threads</summary>
        ///<param name="nodeId">Optional, accepts null</param>
        public NodesHotThreadsRequest(NodeIds nodeId)
            : base(r => r.Optional("node_id", nodeId)) { }

        // values part of the url path
        [IgnoreDataMember]
        NodeIds INodesHotThreadsRequest.NodeId => Self.RouteValues.Get<NodeIds>("node_id");

        // Request parameters
        ///<summary>Don't show threads that are in known-idle places, such as waiting on a socket select or pulling from an empty task queue.</summary>
        public bool? IgnoreIdleThreads
        {
            get => Q<bool?>("ignore_idle_threads");
            set => Q("ignore_idle_threads", value);
        }

        ///<summary>The interval for the second sampling of threads.</summary>
        public Time Interval
        {
            get => Q<Time>("interval");
            set => Q("interval", value);
        }

        ///<summary>The type to sample.</summary>
        public SampleType? SampleType
        {
            get => Q<SampleType?>("type");
            set => Q("type", value);
        }

        ///<summary>Number of samples of thread stacktrace.</summary>
        public long? Snapshots
        {
            get => Q<long?>("snapshots");
            set => Q("snapshots", value);
        }

        ///<summary>Specify the number of threads to provide information for.</summary>
        public long? Threads
        {
            get => Q<long?>("threads");
            set => Q("threads", value);
        }

        ///<summary>Operation timeout.</summary>
        public Time Timeout
        {
            get => Q<Time>("timeout");
            set => Q("timeout", value);
        }
    }

    [InterfaceDataContract]
    public partial interface INodesInfoRequest : IRequest<NodesInfoRequestParameters>
    {
        [IgnoreDataMember]
        NodeIds NodeId { get; }

        [IgnoreDataMember]
        Metrics Metric { get; }
    }

    ///<summary>Request for Info <para>https://opensearch.org/docs/latest/api-reference/nodes-apis/nodes-info/</para></summary>
    public partial class NodesInfoRequest
        : PlainRequestBase<NodesInfoRequestParameters>,
            INodesInfoRequest
    {
        protected INodesInfoRequest Self => this;
        internal override ApiUrls ApiUrls => ApiUrlsLookups.NodesInfo;

        ///<summary>/_nodes</summary>
        public NodesInfoRequest()
            : base() { }

        ///<summary>/_nodes/{node_id}</summary>
        ///<param name="nodeId">Optional, accepts null</param>
        public NodesInfoRequest(NodeIds nodeId)
            : base(r => r.Optional("node_id", nodeId)) { }

        ///<summary>/_nodes/{metric}</summary>
        ///<param name="metric">Optional, accepts null</param>
        public NodesInfoRequest(Metrics metric)
            : base(r => r.Optional("metric", metric)) { }

        ///<summary>/_nodes/{node_id}/{metric}</summary>
        ///<param name="nodeId">Optional, accepts null</param>
        ///<param name="metric">Optional, accepts null</param>
        public NodesInfoRequest(NodeIds nodeId, Metrics metric)
            : base(r => r.Optional("node_id", nodeId).Optional("metric", metric)) { }

        // values part of the url path
        [IgnoreDataMember]
        NodeIds INodesInfoRequest.NodeId => Self.RouteValues.Get<NodeIds>("node_id");

        [IgnoreDataMember]
        Metrics INodesInfoRequest.Metric => Self.RouteValues.Get<Metrics>("metric");

        // Request parameters
        ///<summary>Return settings in flat format.</summary>
        public bool? FlatSettings
        {
            get => Q<bool?>("flat_settings");
            set => Q("flat_settings", value);
        }

        ///<summary>Operation timeout.</summary>
        public Time Timeout
        {
            get => Q<Time>("timeout");
            set => Q("timeout", value);
        }
    }

    [InterfaceDataContract]
    public partial interface IReloadSecureSettingsRequest
        : IRequest<ReloadSecureSettingsRequestParameters>
    {
        [IgnoreDataMember]
        NodeIds NodeId { get; }
    }

    ///<summary>Request for ReloadSecureSettings <para>https://opensearch.org/docs/latest/api-reference/nodes-apis/nodes-reload-secure/</para></summary>
    public partial class ReloadSecureSettingsRequest
        : PlainRequestBase<ReloadSecureSettingsRequestParameters>,
            IReloadSecureSettingsRequest
    {
        protected IReloadSecureSettingsRequest Self => this;
        internal override ApiUrls ApiUrls => ApiUrlsLookups.NodesReloadSecureSettings;

        ///<summary>/_nodes/reload_secure_settings</summary>
        public ReloadSecureSettingsRequest()
            : base() { }

        ///<summary>/_nodes/{node_id}/reload_secure_settings</summary>
        ///<param name="nodeId">Optional, accepts null</param>
        public ReloadSecureSettingsRequest(NodeIds nodeId)
            : base(r => r.Optional("node_id", nodeId)) { }

        // values part of the url path
        [IgnoreDataMember]
        NodeIds IReloadSecureSettingsRequest.NodeId => Self.RouteValues.Get<NodeIds>("node_id");

        // Request parameters
        ///<summary>Operation timeout.</summary>
        public Time Timeout
        {
            get => Q<Time>("timeout");
            set => Q("timeout", value);
        }
    }
}
