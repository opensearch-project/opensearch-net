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
namespace OpenSearch.Net.Specification.SmApi
{
    /// <summary>
    /// Sm APIs.
    /// <para>Not intended to be instantiated directly. Use the <see cref="IOpenSearchLowLevelClient.Sm"/> property
    /// on <see cref="IOpenSearchLowLevelClient"/>.
    /// </para>
    /// </summary>
    public partial class LowLevelSmNamespace : NamespacedClientProxy
    {
        internal LowLevelSmNamespace(OpenSearchLowLevelClient client)
            : base(client) { }

        /// <summary>POST on /_plugins/_sm/policies/{policy_name}</summary>
        /// <param name="policyName">The snapshot management policy name.</param>
        /// <param name="body"></param>
        /// <param name="requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
        /// <remarks>Supported by OpenSearch servers of version 2.1.0 or greater.</remarks>
        public TResponse CreatePolicy<TResponse>(
            string policyName,
            PostData body,
            CreatePolicyRequestParameters requestParameters = null
        )
            where TResponse : class, IOpenSearchResponse, new() =>
            DoRequest<TResponse>(
                POST,
                Url($"_plugins/_sm/policies/{policyName:policyName}"),
                body,
                RequestParams(requestParameters)
            );

        /// <summary>POST on /_plugins/_sm/policies/{policy_name}</summary>
        /// <param name="policyName">The snapshot management policy name.</param>
        /// <param name="body"></param>
        /// <param name="requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
        /// <remarks>Supported by OpenSearch servers of version 2.1.0 or greater.</remarks>
        [MapsApi("sm.create_policy", "policy_name, body")]
        public Task<TResponse> CreatePolicyAsync<TResponse>(
            string policyName,
            PostData body,
            CreatePolicyRequestParameters requestParameters = null,
            CancellationToken ctx = default
        )
            where TResponse : class, IOpenSearchResponse, new() =>
            DoRequestAsync<TResponse>(
                POST,
                Url($"_plugins/_sm/policies/{policyName:policyName}"),
                ctx,
                body,
                RequestParams(requestParameters)
            );

        /// <summary>DELETE on /_plugins/_sm/policies/{policy_name}</summary>
        /// <param name="policyName">The snapshot management policy name.</param>
        /// <param name="requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
        /// <remarks>Supported by OpenSearch servers of version 2.1.0 or greater.</remarks>
        public TResponse DeletePolicy<TResponse>(
            string policyName,
            DeletePolicyRequestParameters requestParameters = null
        )
            where TResponse : class, IOpenSearchResponse, new() =>
            DoRequest<TResponse>(
                DELETE,
                Url($"_plugins/_sm/policies/{policyName:policyName}"),
                null,
                RequestParams(requestParameters)
            );

        /// <summary>DELETE on /_plugins/_sm/policies/{policy_name}</summary>
        /// <param name="policyName">The snapshot management policy name.</param>
        /// <param name="requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
        /// <remarks>Supported by OpenSearch servers of version 2.1.0 or greater.</remarks>
        [MapsApi("sm.delete_policy", "policy_name")]
        public Task<TResponse> DeletePolicyAsync<TResponse>(
            string policyName,
            DeletePolicyRequestParameters requestParameters = null,
            CancellationToken ctx = default
        )
            where TResponse : class, IOpenSearchResponse, new() =>
            DoRequestAsync<TResponse>(
                DELETE,
                Url($"_plugins/_sm/policies/{policyName:policyName}"),
                ctx,
                null,
                RequestParams(requestParameters)
            );

        /// <summary>GET on /_plugins/_sm/policies/{policy_name}/_explain</summary>
        /// <param name="policyName">The snapshot management policy name.</param>
        /// <param name="requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
        /// <remarks>Supported by OpenSearch servers of version 2.1.0 or greater.</remarks>
        public TResponse ExplainPolicy<TResponse>(
            string policyName,
            ExplainPolicyRequestParameters requestParameters = null
        )
            where TResponse : class, IOpenSearchResponse, new() =>
            DoRequest<TResponse>(
                GET,
                Url($"_plugins/_sm/policies/{policyName:policyName}/_explain"),
                null,
                RequestParams(requestParameters)
            );

        /// <summary>GET on /_plugins/_sm/policies/{policy_name}/_explain</summary>
        /// <param name="policyName">The snapshot management policy name.</param>
        /// <param name="requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
        /// <remarks>Supported by OpenSearch servers of version 2.1.0 or greater.</remarks>
        [MapsApi("sm.explain_policy", "policy_name")]
        public Task<TResponse> ExplainPolicyAsync<TResponse>(
            string policyName,
            ExplainPolicyRequestParameters requestParameters = null,
            CancellationToken ctx = default
        )
            where TResponse : class, IOpenSearchResponse, new() =>
            DoRequestAsync<TResponse>(
                GET,
                Url($"_plugins/_sm/policies/{policyName:policyName}/_explain"),
                ctx,
                null,
                RequestParams(requestParameters)
            );

        /// <summary>GET on /_plugins/_sm/policies</summary>
        /// <param name="requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
        /// <remarks>Supported by OpenSearch servers of version 2.1.0 or greater.</remarks>
        public TResponse GetPolicies<TResponse>(
            GetPoliciesRequestParameters requestParameters = null
        )
            where TResponse : class, IOpenSearchResponse, new() =>
            DoRequest<TResponse>(
                GET,
                "_plugins/_sm/policies",
                null,
                RequestParams(requestParameters)
            );

        /// <summary>GET on /_plugins/_sm/policies</summary>
        /// <param name="requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
        /// <remarks>Supported by OpenSearch servers of version 2.1.0 or greater.</remarks>
        [MapsApi("sm.get_policies", "")]
        public Task<TResponse> GetPoliciesAsync<TResponse>(
            GetPoliciesRequestParameters requestParameters = null,
            CancellationToken ctx = default
        )
            where TResponse : class, IOpenSearchResponse, new() =>
            DoRequestAsync<TResponse>(
                GET,
                "_plugins/_sm/policies",
                ctx,
                null,
                RequestParams(requestParameters)
            );

        /// <summary>GET on /_plugins/_sm/policies/{policy_name}</summary>
        /// <param name="policyName">The snapshot management name.</param>
        /// <param name="requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
        /// <remarks>Supported by OpenSearch servers of version 2.1.0 or greater.</remarks>
        public TResponse GetPolicy<TResponse>(
            string policyName,
            GetPolicyRequestParameters requestParameters = null
        )
            where TResponse : class, IOpenSearchResponse, new() =>
            DoRequest<TResponse>(
                GET,
                Url($"_plugins/_sm/policies/{policyName:policyName}"),
                null,
                RequestParams(requestParameters)
            );

        /// <summary>GET on /_plugins/_sm/policies/{policy_name}</summary>
        /// <param name="policyName">The snapshot management name.</param>
        /// <param name="requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
        /// <remarks>Supported by OpenSearch servers of version 2.1.0 or greater.</remarks>
        [MapsApi("sm.get_policy", "policy_name")]
        public Task<TResponse> GetPolicyAsync<TResponse>(
            string policyName,
            GetPolicyRequestParameters requestParameters = null,
            CancellationToken ctx = default
        )
            where TResponse : class, IOpenSearchResponse, new() =>
            DoRequestAsync<TResponse>(
                GET,
                Url($"_plugins/_sm/policies/{policyName:policyName}"),
                ctx,
                null,
                RequestParams(requestParameters)
            );

        /// <summary>POST on /_plugins/_sm/policies/{policy_name}/_start</summary>
        /// <param name="policyName">The snapshot management policy name.</param>
        /// <param name="requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
        /// <remarks>Supported by OpenSearch servers of version 2.1.0 or greater.</remarks>
        public TResponse StartPolicy<TResponse>(
            string policyName,
            StartPolicyRequestParameters requestParameters = null
        )
            where TResponse : class, IOpenSearchResponse, new() =>
            DoRequest<TResponse>(
                POST,
                Url($"_plugins/_sm/policies/{policyName:policyName}/_start"),
                null,
                RequestParams(requestParameters)
            );

        /// <summary>POST on /_plugins/_sm/policies/{policy_name}/_start</summary>
        /// <param name="policyName">The snapshot management policy name.</param>
        /// <param name="requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
        /// <remarks>Supported by OpenSearch servers of version 2.1.0 or greater.</remarks>
        [MapsApi("sm.start_policy", "policy_name")]
        public Task<TResponse> StartPolicyAsync<TResponse>(
            string policyName,
            StartPolicyRequestParameters requestParameters = null,
            CancellationToken ctx = default
        )
            where TResponse : class, IOpenSearchResponse, new() =>
            DoRequestAsync<TResponse>(
                POST,
                Url($"_plugins/_sm/policies/{policyName:policyName}/_start"),
                ctx,
                null,
                RequestParams(requestParameters)
            );

        /// <summary>POST on /_plugins/_sm/policies/{policy_name}/_stop</summary>
        /// <param name="policyName">The snapshot management policy name.</param>
        /// <param name="requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
        /// <remarks>Supported by OpenSearch servers of version 2.1.0 or greater.</remarks>
        public TResponse StopPolicy<TResponse>(
            string policyName,
            StopPolicyRequestParameters requestParameters = null
        )
            where TResponse : class, IOpenSearchResponse, new() =>
            DoRequest<TResponse>(
                POST,
                Url($"_plugins/_sm/policies/{policyName:policyName}/_stop"),
                null,
                RequestParams(requestParameters)
            );

        /// <summary>POST on /_plugins/_sm/policies/{policy_name}/_stop</summary>
        /// <param name="policyName">The snapshot management policy name.</param>
        /// <param name="requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
        /// <remarks>Supported by OpenSearch servers of version 2.1.0 or greater.</remarks>
        [MapsApi("sm.stop_policy", "policy_name")]
        public Task<TResponse> StopPolicyAsync<TResponse>(
            string policyName,
            StopPolicyRequestParameters requestParameters = null,
            CancellationToken ctx = default
        )
            where TResponse : class, IOpenSearchResponse, new() =>
            DoRequestAsync<TResponse>(
                POST,
                Url($"_plugins/_sm/policies/{policyName:policyName}/_stop"),
                ctx,
                null,
                RequestParams(requestParameters)
            );

        /// <summary>PUT on /_plugins/_sm/policies/{policy_name}</summary>
        /// <param name="policyName">The snapshot management policy name.</param>
        /// <param name="body"></param>
        /// <param name="requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
        /// <remarks>Supported by OpenSearch servers of version 2.1.0 or greater.</remarks>
        public TResponse UpdatePolicy<TResponse>(
            string policyName,
            PostData body,
            UpdatePolicyRequestParameters requestParameters = null
        )
            where TResponse : class, IOpenSearchResponse, new() =>
            DoRequest<TResponse>(
                PUT,
                Url($"_plugins/_sm/policies/{policyName:policyName}"),
                body,
                RequestParams(requestParameters)
            );

        /// <summary>PUT on /_plugins/_sm/policies/{policy_name}</summary>
        /// <param name="policyName">The snapshot management policy name.</param>
        /// <param name="body"></param>
        /// <param name="requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
        /// <remarks>Supported by OpenSearch servers of version 2.1.0 or greater.</remarks>
        [MapsApi("sm.update_policy", "policy_name, body")]
        public Task<TResponse> UpdatePolicyAsync<TResponse>(
            string policyName,
            PostData body,
            UpdatePolicyRequestParameters requestParameters = null,
            CancellationToken ctx = default
        )
            where TResponse : class, IOpenSearchResponse, new() =>
            DoRequestAsync<TResponse>(
                PUT,
                Url($"_plugins/_sm/policies/{policyName:policyName}"),
                ctx,
                body,
                RequestParams(requestParameters)
            );
    }
}
