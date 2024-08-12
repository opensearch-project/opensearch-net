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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net.NetworkInformation;
using OpenSearch.Net.Diagnostics;

namespace OpenSearch.Net;

/// <summary>
/// Details about the API call
/// </summary>
public interface IApiCallDetails
{
    //TODO: Get rid of setter
    /// <summary>
    /// An audit trail of requests made to nodes within the cluster
    /// </summary>
    List<Audit> AuditTrail { get; set; }

    // TODO: Get rid of setter.
    /// <summary>
    /// Thread pool thread statistics collected when making a request
    /// </summary>
    ReadOnlyDictionary<string, ThreadPoolStatistics> ThreadPoolStats { get; set; }

    // TODO: Get rid of setter
    /// <summary>
    /// Active TCP connection statistics collected when making a request
    /// </summary>
    ReadOnlyDictionary<TcpState, int> TcpStats { get; set; }

    /// <summary>
    /// A human readable string representation of what happened during this request for both successful and failed requests.
    /// </summary>
    string DebugInformation { get; }

    /// <summary>
    /// Reference to the connection configuration that yielded this response
    /// </summary>
    IConnectionConfigurationValues ConnectionConfiguration { get; }

    /// <summary>
    /// A collection of deprecation warnings returned from OpenSearch.
    /// <para>Used to signal that the request uses an API feature that is marked as deprecated</para>
    /// </summary>
    IEnumerable<string> DeprecationWarnings { get; }

    /// <summary>
    /// The HTTP method used by the request
    /// </summary>
    HttpMethod HttpMethod { get; }

    /// <summary>
    /// The HTTP status code as returned by OpenSearch
    /// </summary>
    int? HttpStatusCode { get; }

    /// <summary>
    /// If <see cref="Success"/> is <c>false</c>, this will hold the original exception.
    /// This will be the originating CLR exception in most cases.
    /// </summary>
    Exception OriginalException { get; }

    /// <summary>
    /// The request body bytes.
    /// <para>NOTE: Only set when disable direct streaming is set for the request</para>
    /// </summary>
    [DebuggerDisplay("{RequestBodyInBytes != null ? System.Text.Encoding.UTF8.GetString(RequestBodyInBytes) : null,nq}")]
    byte[] RequestBodyInBytes { get; }

    /// <summary>
    /// The response body bytes.
    /// <para>NOTE: Only set when disable direct streaming is set for the request</para>
    /// </summary>
    [DebuggerDisplay("{ResponseBodyInBytes != null ? System.Text.Encoding.UTF8.GetString(ResponseBodyInBytes) : null,nq}")]
    byte[] ResponseBodyInBytes { get; }

    /// <summary>The response MIME type </summary>
    string ResponseMimeType { get; }

    /// <summary>
    /// The response status code is in the 200 range or is in the allowed list of status codes set on the request.
    /// </summary>
    bool Success { get; }

    /// <summary>
    /// The response is successful or has a response code between 400-599, the call should not be retried.
    /// Only on 502,503 and 504 will this return false;
    /// </summary>
    bool SuccessOrKnownError { get; }

    /// <summary>
    /// The url as requested
    /// </summary>
    Uri Uri { get; }
}
