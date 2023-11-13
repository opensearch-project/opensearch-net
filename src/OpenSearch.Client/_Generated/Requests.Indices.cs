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
using OpenSearch.Net.Specification.IndicesApi;

// ReSharper disable RedundantBaseConstructorCall
// ReSharper disable UnusedTypeParameter
// ReSharper disable PartialMethodWithSinglePart
// ReSharper disable RedundantNameQualifier
namespace OpenSearch.Client
{
    [InterfaceDataContract]
    public partial interface IDeleteComposableIndexTemplateRequest
        : IRequest<DeleteComposableIndexTemplateRequestParameters>
    {
        [IgnoreDataMember]
        Name Name { get; }
    }

    /// <summary>Request for DeleteComposableTemplate <para>https://opensearch.org/docs/latest/im-plugin/index-templates/#delete-a-template</para></summary>
    public partial class DeleteComposableIndexTemplateRequest
        : PlainRequestBase<DeleteComposableIndexTemplateRequestParameters>,
            IDeleteComposableIndexTemplateRequest
    {
        protected IDeleteComposableIndexTemplateRequest Self => this;
        internal override ApiUrls ApiUrls => ApiUrlsLookups.IndicesDeleteComposableTemplate;

        /// <summary>/_index_template/{name}</summary>
        /// <param name="name">this parameter is required</param>
        public DeleteComposableIndexTemplateRequest(Name name)
            : base(r => r.Required("name", name)) { }

        /// <summary>Used for serialization purposes, making sure we have a parameterless constructor</summary>
        [SerializationConstructor]
        protected DeleteComposableIndexTemplateRequest()
            : base() { }

        // values part of the url path
        [IgnoreDataMember]
        Name IDeleteComposableIndexTemplateRequest.Name => Self.RouteValues.Get<Name>("name");

        // Request parameters
        /// <summary>Operation timeout for connection to cluster-manager node.</summary>
        /// <remarks>Supported by OpenSearch servers of version 2.0.0 or greater.</remarks>
        public Time ClusterManagerTimeout
        {
            get => Q<Time>("cluster_manager_timeout");
            set => Q("cluster_manager_timeout", value);
        }

        /// <summary>Operation timeout for connection to master node.</summary>
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

    [InterfaceDataContract]
    public partial interface IComposableIndexTemplateExistsRequest
        : IRequest<ComposableIndexTemplateExistsRequestParameters>
    {
        [IgnoreDataMember]
        Name Name { get; }
    }

    /// <summary>Request for ComposableTemplateExists <para>https://opensearch.org/docs/latest/im-plugin/index-templates/</para></summary>
    public partial class ComposableIndexTemplateExistsRequest
        : PlainRequestBase<ComposableIndexTemplateExistsRequestParameters>,
            IComposableIndexTemplateExistsRequest
    {
        protected IComposableIndexTemplateExistsRequest Self => this;
        internal override ApiUrls ApiUrls => ApiUrlsLookups.IndicesComposableTemplateExists;

        /// <summary>/_index_template/{name}</summary>
        /// <param name="name">this parameter is required</param>
        public ComposableIndexTemplateExistsRequest(Name name)
            : base(r => r.Required("name", name)) { }

        /// <summary>Used for serialization purposes, making sure we have a parameterless constructor</summary>
        [SerializationConstructor]
        protected ComposableIndexTemplateExistsRequest()
            : base() { }

        // values part of the url path
        [IgnoreDataMember]
        Name IComposableIndexTemplateExistsRequest.Name => Self.RouteValues.Get<Name>("name");

        // Request parameters
        /// <summary>Operation timeout for connection to cluster-manager node.</summary>
        /// <remarks>Supported by OpenSearch servers of version 2.0.0 or greater.</remarks>
        public Time ClusterManagerTimeout
        {
            get => Q<Time>("cluster_manager_timeout");
            set => Q("cluster_manager_timeout", value);
        }

        /// <summary>Return settings in flat format.</summary>
        public bool? FlatSettings
        {
            get => Q<bool?>("flat_settings");
            set => Q("flat_settings", value);
        }

        /// <summary>Return local information, do not retrieve the state from cluster-manager node.</summary>
        public bool? Local
        {
            get => Q<bool?>("local");
            set => Q("local", value);
        }

        /// <summary>Operation timeout for connection to master node.</summary>
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
    public partial interface IGetComposableIndexTemplateRequest
        : IRequest<GetComposableIndexTemplateRequestParameters>
    {
        [IgnoreDataMember]
        Names Name { get; }
    }

    /// <summary>Request for GetComposableTemplate <para>https://opensearch.org/docs/latest/im-plugin/index-templates/</para></summary>
    public partial class GetComposableIndexTemplateRequest
        : PlainRequestBase<GetComposableIndexTemplateRequestParameters>,
            IGetComposableIndexTemplateRequest
    {
        protected IGetComposableIndexTemplateRequest Self => this;
        internal override ApiUrls ApiUrls => ApiUrlsLookups.IndicesGetComposableTemplate;

        /// <summary>/_index_template</summary>
        public GetComposableIndexTemplateRequest()
            : base() { }

        /// <summary>/_index_template/{name}</summary>
        /// <param name="name">Optional, accepts null</param>
        public GetComposableIndexTemplateRequest(Names name)
            : base(r => r.Optional("name", name)) { }

        // values part of the url path
        [IgnoreDataMember]
        Names IGetComposableIndexTemplateRequest.Name => Self.RouteValues.Get<Names>("name");

        // Request parameters
        /// <summary>Operation timeout for connection to cluster-manager node.</summary>
        /// <remarks>Supported by OpenSearch servers of version 2.0.0 or greater.</remarks>
        public Time ClusterManagerTimeout
        {
            get => Q<Time>("cluster_manager_timeout");
            set => Q("cluster_manager_timeout", value);
        }

        /// <summary>Return settings in flat format.</summary>
        public bool? FlatSettings
        {
            get => Q<bool?>("flat_settings");
            set => Q("flat_settings", value);
        }

        /// <summary>Return local information, do not retrieve the state from cluster-manager node.</summary>
        public bool? Local
        {
            get => Q<bool?>("local");
            set => Q("local", value);
        }

        /// <summary>Operation timeout for connection to master node.</summary>
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
    public partial interface IPutComposableIndexTemplateRequest
        : IRequest<PutComposableIndexTemplateRequestParameters>
    {
        [IgnoreDataMember]
        Name Name { get; }
    }

    /// <summary>Request for PutComposableTemplate</summary>
    public partial class PutComposableIndexTemplateRequest
        : PlainRequestBase<PutComposableIndexTemplateRequestParameters>,
            IPutComposableIndexTemplateRequest
    {
        protected IPutComposableIndexTemplateRequest Self => this;
        internal override ApiUrls ApiUrls => ApiUrlsLookups.IndicesPutComposableTemplate;

        /// <summary>/_index_template/{name}</summary>
        /// <param name="name">this parameter is required</param>
        public PutComposableIndexTemplateRequest(Name name)
            : base(r => r.Required("name", name)) { }

        /// <summary>Used for serialization purposes, making sure we have a parameterless constructor</summary>
        [SerializationConstructor]
        protected PutComposableIndexTemplateRequest()
            : base() { }

        // values part of the url path
        [IgnoreDataMember]
        Name IPutComposableIndexTemplateRequest.Name => Self.RouteValues.Get<Name>("name");

        // Request parameters
        /// <summary>User defined reason for creating/updating the index template.</summary>
        public string Cause
        {
            get => Q<string>("cause");
            set => Q("cause", value);
        }

        /// <summary>Operation timeout for connection to cluster-manager node.</summary>
        /// <remarks>Supported by OpenSearch servers of version 2.0.0 or greater.</remarks>
        public Time ClusterManagerTimeout
        {
            get => Q<Time>("cluster_manager_timeout");
            set => Q("cluster_manager_timeout", value);
        }

        /// <summary>Whether the index template should only be added if new or can also replace an existing one.</summary>
        public bool? Create
        {
            get => Q<bool?>("create");
            set => Q("create", value);
        }

        /// <summary>Operation timeout for connection to master node.</summary>
        [Obsolete(
            "Deprecated as of: 2.0.0, reason: To promote inclusive language, use 'cluster_manager_timeout' instead."
        )]
        public Time MasterTimeout
        {
            get => Q<Time>("master_timeout");
            set => Q("master_timeout", value);
        }
    }
}
