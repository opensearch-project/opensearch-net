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
using System.Runtime.Serialization;
using System.Text;
using OpenSearch.Net;
using OpenSearch.Net.Specification.ClusterApi;
using OpenSearch.Net.Utf8Json;

// ReSharper disable RedundantBaseConstructorCall
// ReSharper disable UnusedTypeParameter
// ReSharper disable PartialMethodWithSinglePart
// ReSharper disable RedundantNameQualifier
namespace OpenSearch.Client
{
    [InterfaceDataContract]
    public partial interface IDeleteComponentTemplateRequest
        : IRequest<DeleteComponentTemplateRequestParameters>
    {
        [IgnoreDataMember]
        Name Name { get; }
    }

    /// <summary>Request for DeleteComponentTemplate <para>https://opensearch.org/docs/latest</para></summary>
    public partial class DeleteComponentTemplateRequest
        : PlainRequestBase<DeleteComponentTemplateRequestParameters>,
            IDeleteComponentTemplateRequest
    {
        protected IDeleteComponentTemplateRequest Self => this;
        internal override ApiUrls ApiUrls => ApiUrlsLookups.ClusterDeleteComponentTemplate;

        /// <summary>/_component_template/{name}</summary>
        /// <param name="name">this parameter is required</param>
        public DeleteComponentTemplateRequest(Name name)
            : base(r => r.Required("name", name)) { }

        /// <summary>Used for serialization purposes, making sure we have a parameterless constructor</summary>
        [SerializationConstructor]
        protected DeleteComponentTemplateRequest()
            : base() { }

        // values part of the url path
        [IgnoreDataMember]
        Name IDeleteComponentTemplateRequest.Name => Self.RouteValues.Get<Name>("name");

        // Request parameters
        /// <summary>Operation timeout for connection to cluster-manager node.</summary>
        /// <remarks>Supported by OpenSearch servers of version 2.0.0 or greater.</remarks>
        public Time ClusterManagerTimeout
        {
            get => Q<Time>("cluster_manager_timeout");
            set => Q("cluster_manager_timeout", value);
        }

        /// <summary>
        /// Period to wait for a connection to the master node. If no response is received before the timeout expires, the request fails and returns
        /// an error.
        /// </summary>
        [Obsolete(
            "Deprecated as of: 2.0.0, reason: To promote inclusive language, use 'cluster_manager_timeout' instead."
        )]
        public Time MasterTimeout
        {
            get => Q<Time>("master_timeout");
            set => Q("master_timeout", value);
        }

        /// <summary>Period to wait for a response. If no response is received before the timeout expires, the request fails and returns an error.</summary>
        public Time Timeout
        {
            get => Q<Time>("timeout");
            set => Q("timeout", value);
        }
    }

    [InterfaceDataContract]
    public partial interface IComponentTemplateExistsRequest
        : IRequest<ComponentTemplateExistsRequestParameters>
    {
        [IgnoreDataMember]
        Name Name { get; }
    }

    /// <summary>Request for ComponentTemplateExists <para>https://opensearch.org/docs/latest</para></summary>
    public partial class ComponentTemplateExistsRequest
        : PlainRequestBase<ComponentTemplateExistsRequestParameters>,
            IComponentTemplateExistsRequest
    {
        protected IComponentTemplateExistsRequest Self => this;
        internal override ApiUrls ApiUrls => ApiUrlsLookups.ClusterComponentTemplateExists;

        /// <summary>/_component_template/{name}</summary>
        /// <param name="name">this parameter is required</param>
        public ComponentTemplateExistsRequest(Name name)
            : base(r => r.Required("name", name)) { }

        /// <summary>Used for serialization purposes, making sure we have a parameterless constructor</summary>
        [SerializationConstructor]
        protected ComponentTemplateExistsRequest()
            : base() { }

        // values part of the url path
        [IgnoreDataMember]
        Name IComponentTemplateExistsRequest.Name => Self.RouteValues.Get<Name>("name");

        // Request parameters
        /// <summary>Operation timeout for connection to cluster-manager node.</summary>
        /// <remarks>Supported by OpenSearch servers of version 2.0.0 or greater.</remarks>
        public Time ClusterManagerTimeout
        {
            get => Q<Time>("cluster_manager_timeout");
            set => Q("cluster_manager_timeout", value);
        }

        /// <summary>
        /// If true, the request retrieves information from the local node only. Defaults to false, which means information is retrieved from the
        /// master node.
        /// </summary>
        public bool? Local
        {
            get => Q<bool?>("local");
            set => Q("local", value);
        }

        /// <summary>
        /// Period to wait for a connection to the master node. If no response is received before the timeout expires, the request fails and returns
        /// an error.
        /// </summary>
        [Obsolete(
            "Deprecated as of: 2.0.0, reason: To promote inclusive language, use 'cluster_manager_timeout' instead."
        )]
        public Time MasterTimeout
        {
            get => Q<Time>("master_timeout");
            set => Q("master_timeout", value);
        }
    }

    [InterfaceDataContract]
    public partial interface IGetComponentTemplateRequest
        : IRequest<GetComponentTemplateRequestParameters>
    {
        [IgnoreDataMember]
        Names Name { get; }
    }

    /// <summary>Request for GetComponentTemplate <para>https://opensearch.org/docs/latest</para></summary>
    public partial class GetComponentTemplateRequest
        : PlainRequestBase<GetComponentTemplateRequestParameters>,
            IGetComponentTemplateRequest
    {
        protected IGetComponentTemplateRequest Self => this;
        internal override ApiUrls ApiUrls => ApiUrlsLookups.ClusterGetComponentTemplate;

        /// <summary>/_component_template</summary>
        public GetComponentTemplateRequest()
            : base() { }

        /// <summary>/_component_template/{name}</summary>
        /// <param name="name">Optional, accepts null</param>
        public GetComponentTemplateRequest(Names name)
            : base(r => r.Optional("name", name)) { }

        // values part of the url path
        [IgnoreDataMember]
        Names IGetComponentTemplateRequest.Name => Self.RouteValues.Get<Names>("name");

        // Request parameters
        /// <summary>Operation timeout for connection to cluster-manager node.</summary>
        /// <remarks>Supported by OpenSearch servers of version 2.0.0 or greater.</remarks>
        public Time ClusterManagerTimeout
        {
            get => Q<Time>("cluster_manager_timeout");
            set => Q("cluster_manager_timeout", value);
        }

        /// <summary>If `true`, the request retrieves information from the local node only. If `false`, information is retrieved from the master node.</summary>
        public bool? Local
        {
            get => Q<bool?>("local");
            set => Q("local", value);
        }

        /// <summary>
        /// Period to wait for a connection to the master node. If no response is received before the timeout expires, the request fails and returns
        /// an error.
        /// </summary>
        [Obsolete(
            "Deprecated as of: 2.0.0, reason: To promote inclusive language, use 'cluster_manager_timeout' instead."
        )]
        public Time MasterTimeout
        {
            get => Q<Time>("master_timeout");
            set => Q("master_timeout", value);
        }
    }

    [InterfaceDataContract]
    public partial interface IPutComponentTemplateRequest
        : IRequest<PutComponentTemplateRequestParameters>
    {
        [IgnoreDataMember]
        Name Name { get; }
    }

    /// <summary>Request for PutComponentTemplate <para>https://opensearch.org/docs/latest/im-plugin/index-templates/#use-component-templates-to-create-an-index-template</para></summary>
    public partial class PutComponentTemplateRequest
        : PlainRequestBase<PutComponentTemplateRequestParameters>,
            IPutComponentTemplateRequest
    {
        protected IPutComponentTemplateRequest Self => this;
        internal override ApiUrls ApiUrls => ApiUrlsLookups.ClusterPutComponentTemplate;

        /// <summary>/_component_template/{name}</summary>
        /// <param name="name">this parameter is required</param>
        public PutComponentTemplateRequest(Name name)
            : base(r => r.Required("name", name)) { }

        /// <summary>Used for serialization purposes, making sure we have a parameterless constructor</summary>
        [SerializationConstructor]
        protected PutComponentTemplateRequest()
            : base() { }

        // values part of the url path
        [IgnoreDataMember]
        Name IPutComponentTemplateRequest.Name => Self.RouteValues.Get<Name>("name");

        // Request parameters
        /// <summary>Operation timeout for connection to cluster-manager node.</summary>
        /// <remarks>Supported by OpenSearch servers of version 2.0.0 or greater.</remarks>
        public Time ClusterManagerTimeout
        {
            get => Q<Time>("cluster_manager_timeout");
            set => Q("cluster_manager_timeout", value);
        }

        /// <summary>If `true`, this request cannot replace or update existing component templates.</summary>
        public bool? Create
        {
            get => Q<bool?>("create");
            set => Q("create", value);
        }

        /// <summary>
        /// Period to wait for a connection to the master node. If no response is received before the timeout expires, the request fails and returns
        /// an error.
        /// </summary>
        [Obsolete(
            "Deprecated as of: 2.0.0, reason: To promote inclusive language, use 'cluster_manager_timeout' instead."
        )]
        public Time MasterTimeout
        {
            get => Q<Time>("master_timeout");
            set => Q("master_timeout", value);
        }

        /// <summary>Operation timeout.</summary>
        public Time Timeout
        {
            get => Q<Time>("timeout");
            set => Q("timeout", value);
        }
    }
}
