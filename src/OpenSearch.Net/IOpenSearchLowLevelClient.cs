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
* 	http://www.apache.org/licenses/LICENSE-2.0
*
*  Unless required by applicable law or agreed to in writing,
*  software distributed under the License is distributed on an
*  "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
*  KIND, either express or implied.  See the License for the
*  specific language governing permissions and limitations
*  under the License.
*/

using System.Threading;
using System.Threading.Tasks;

namespace OpenSearch.Net;

public partial interface IOpenSearchLowLevelClient
{
    /// <summary>
    /// Perform any request you want over the configured IConnection synchronously while taking advantage of the cluster failover.
    /// </summary>
    /// <typeparam name="TResponse">The type representing the response JSON</typeparam>
    /// <param name="method">the HTTP Method to use</param>
    /// <param name="path">The path of the the url that you would like to hit</param>
    /// <param name="data">The body of the request, string and byte[] are posted as is other types will be serialized to JSON</param>
    /// <param name="requestParameters">Optionally configure request specific timeouts, headers</param>
    /// <returns>An OpenSearchResponse of T where T represents the JSON response body</returns>
    TResponse DoRequest<TResponse>(HttpMethod method, string path, PostData data = null, IRequestParameters requestParameters = null)
        where TResponse : class, IOpenSearchResponse, new();

    /// <summary>
    /// Perform any request you want over the configured IConnection asynchronously while taking advantage of the cluster failover.
    /// </summary>
    /// <typeparam name="TResponse">The type representing the response JSON</typeparam>
    /// <param name="method">the HTTP Method to use</param>
    /// <param name="path">The path of the the url that you would like to hit</param>
    /// <param name="data">The body of the request, string and byte[] are posted as is other types will be serialized to JSON</param>
    /// <param name="requestParameters">Optionally configure request specific timeouts, headers</param>
    /// <returns>A task of OpenSearchResponse of T where T represents the JSON response body</returns>
    Task<TResponse> DoRequestAsync<TResponse>(HttpMethod method, string path, CancellationToken cancellationToken, PostData data = null,
        IRequestParameters requestParameters = null
    )
        where TResponse : class, IOpenSearchResponse, new();

}
