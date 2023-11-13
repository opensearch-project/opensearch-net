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

using OpenSearch.Net;
using OpenSearch.Net.Utf8Json;
using OpenSearch.Net.Specification.ClusterApi;

// ReSharper disable RedundantBaseConstructorCall
// ReSharper disable UnusedTypeParameter
// ReSharper disable PartialMethodWithSinglePart
// ReSharper disable RedundantNameQualifier
namespace OpenSearch.Client
{
    /// <summary>Descriptor for DeleteComponentTemplate <para>https://opensearch.org/docs/latest</para></summary>
    public partial class DeleteComponentTemplateDescriptor
        : RequestDescriptorBase<
            DeleteComponentTemplateDescriptor,
            DeleteComponentTemplateRequestParameters,
            IDeleteComponentTemplateRequest
        >,
            IDeleteComponentTemplateRequest
    {
        internal override ApiUrls ApiUrls => ApiUrlsLookups.ClusterDeleteComponentTemplate;

        /// <summary>/_component_template/{name}</summary>
        /// <param name="name">this parameter is required</param>
        public DeleteComponentTemplateDescriptor(Name name)
            : base(r => r.Required("name", name)) { }

        /// <summary>Used for serialization purposes, making sure we have a parameterless constructor</summary>
        [SerializationConstructor]
        protected DeleteComponentTemplateDescriptor()
            : base() { }

        // values part of the url path
        Name IDeleteComponentTemplateRequest.Name => Self.RouteValues.Get<Name>("name");

        // Request parameters
        /// <summary>Operation timeout for connection to cluster-manager node.</summary>
        /// <remarks>Supported by OpenSearch servers of version 2.0.0 or greater.</remarks>
        public DeleteComponentTemplateDescriptor ClusterManagerTimeout(
            Time clustermanagertimeout
        ) => Qs("cluster_manager_timeout", clustermanagertimeout);

        /// <summary>Operation timeout for connection to master node.</summary>
        [Obsolete(
            "Deprecated as of: 2.0.0, reason: To promote inclusive language, use 'cluster_manager_timeout' instead."
        )]
        public DeleteComponentTemplateDescriptor MasterTimeout(Time mastertimeout) =>
            Qs("master_timeout", mastertimeout);

        /// <summary>Operation timeout.</summary>
        public DeleteComponentTemplateDescriptor Timeout(Time timeout) => Qs("timeout", timeout);
    }

    /// <summary>Descriptor for ComponentTemplateExists <para>https://opensearch.org/docs/latest</para></summary>
    public partial class ComponentTemplateExistsDescriptor
        : RequestDescriptorBase<
            ComponentTemplateExistsDescriptor,
            ComponentTemplateExistsRequestParameters,
            IComponentTemplateExistsRequest
        >,
            IComponentTemplateExistsRequest
    {
        internal override ApiUrls ApiUrls => ApiUrlsLookups.ClusterComponentTemplateExists;

        /// <summary>/_component_template/{name}</summary>
        /// <param name="name">this parameter is required</param>
        public ComponentTemplateExistsDescriptor(Name name)
            : base(r => r.Required("name", name)) { }

        /// <summary>Used for serialization purposes, making sure we have a parameterless constructor</summary>
        [SerializationConstructor]
        protected ComponentTemplateExistsDescriptor()
            : base() { }

        // values part of the url path
        Name IComponentTemplateExistsRequest.Name => Self.RouteValues.Get<Name>("name");

        // Request parameters
        /// <summary>Operation timeout for connection to cluster-manager node.</summary>
        /// <remarks>Supported by OpenSearch servers of version 2.0.0 or greater.</remarks>
        public ComponentTemplateExistsDescriptor ClusterManagerTimeout(
            Time clustermanagertimeout
        ) => Qs("cluster_manager_timeout", clustermanagertimeout);

        /// <summary>Return local information, do not retrieve the state from cluster-manager node.</summary>
        public ComponentTemplateExistsDescriptor Local(bool? local = true) => Qs("local", local);

        /// <summary>Operation timeout for connection to master node.</summary>
        [Obsolete(
            "Deprecated as of: 2.0.0, reason: To promote inclusive language, use 'cluster_manager_timeout' instead."
        )]
        public ComponentTemplateExistsDescriptor MasterTimeout(Time mastertimeout) =>
            Qs("master_timeout", mastertimeout);
    }

    /// <summary>Descriptor for GetComponentTemplate <para>https://opensearch.org/docs/latest</para></summary>
    public partial class GetComponentTemplateDescriptor
        : RequestDescriptorBase<
            GetComponentTemplateDescriptor,
            GetComponentTemplateRequestParameters,
            IGetComponentTemplateRequest
        >,
            IGetComponentTemplateRequest
    {
        internal override ApiUrls ApiUrls => ApiUrlsLookups.ClusterGetComponentTemplate;

        /// <summary>/_component_template</summary>
        public GetComponentTemplateDescriptor()
            : base() { }

        /// <summary>/_component_template/{name}</summary>
        /// <param name="name">Optional, accepts null</param>
        public GetComponentTemplateDescriptor(Names name)
            : base(r => r.Optional("name", name)) { }

        // values part of the url path
        Names IGetComponentTemplateRequest.Name => Self.RouteValues.Get<Names>("name");

        /// <summary>The Comma-separated names of the component templates.</summary>
        public GetComponentTemplateDescriptor Name(Names name) =>
            Assign(name, (a, v) => a.RouteValues.Optional("name", v));

        // Request parameters
        /// <summary>Operation timeout for connection to cluster-manager node.</summary>
        /// <remarks>Supported by OpenSearch servers of version 2.0.0 or greater.</remarks>
        public GetComponentTemplateDescriptor ClusterManagerTimeout(Time clustermanagertimeout) =>
            Qs("cluster_manager_timeout", clustermanagertimeout);

        /// <summary>Return local information, do not retrieve the state from cluster-manager node.</summary>
        public GetComponentTemplateDescriptor Local(bool? local = true) => Qs("local", local);

        /// <summary>Operation timeout for connection to master node.</summary>
        [Obsolete(
            "Deprecated as of: 2.0.0, reason: To promote inclusive language, use 'cluster_manager_timeout' instead."
        )]
        public GetComponentTemplateDescriptor MasterTimeout(Time mastertimeout) =>
            Qs("master_timeout", mastertimeout);
    }

    /// <summary>Descriptor for PutComponentTemplate</summary>
    public partial class PutComponentTemplateDescriptor
        : RequestDescriptorBase<
            PutComponentTemplateDescriptor,
            PutComponentTemplateRequestParameters,
            IPutComponentTemplateRequest
        >,
            IPutComponentTemplateRequest
    {
        internal override ApiUrls ApiUrls => ApiUrlsLookups.ClusterPutComponentTemplate;

        /// <summary>/_component_template/{name}</summary>
        /// <param name="name">this parameter is required</param>
        public PutComponentTemplateDescriptor(Name name)
            : base(r => r.Required("name", name)) { }

        /// <summary>Used for serialization purposes, making sure we have a parameterless constructor</summary>
        [SerializationConstructor]
        protected PutComponentTemplateDescriptor()
            : base() { }

        // values part of the url path
        Name IPutComponentTemplateRequest.Name => Self.RouteValues.Get<Name>("name");

        // Request parameters
        /// <summary>Operation timeout for connection to cluster-manager node.</summary>
        /// <remarks>Supported by OpenSearch servers of version 2.0.0 or greater.</remarks>
        public PutComponentTemplateDescriptor ClusterManagerTimeout(Time clustermanagertimeout) =>
            Qs("cluster_manager_timeout", clustermanagertimeout);

        /// <summary>Whether the index template should only be added if new or can also replace an existing one.</summary>
        public PutComponentTemplateDescriptor Create(bool? create = true) => Qs("create", create);

        /// <summary>Operation timeout for connection to master node.</summary>
        [Obsolete(
            "Deprecated as of: 2.0.0, reason: To promote inclusive language, use 'cluster_manager_timeout' instead."
        )]
        public PutComponentTemplateDescriptor MasterTimeout(Time mastertimeout) =>
            Qs("master_timeout", mastertimeout);

        /// <summary>Operation timeout.</summary>
        public PutComponentTemplateDescriptor Timeout(Time timeout) => Qs("timeout", timeout);
    }
}
