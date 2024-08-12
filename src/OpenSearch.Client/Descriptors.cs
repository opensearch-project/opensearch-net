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

using System.Collections.Generic;

namespace OpenSearch.Client;

// ReSharper disable UnusedTypeParameter
public abstract partial class RequestDescriptorBase<TDescriptor, TParameters, TInterface>
{
    ///<summary>Include the stack trace of returned errors.</summary>
    public TDescriptor ErrorTrace(bool? errortrace = true) => Qs("error_trace", errortrace);
    ///<summary>A comma-separated list of filters used to reduce the response.<para>Use of response filtering can result in a response from OpenSearch that cannot be correctly deserialized to the respective response type for the request. In such situations, use the low level client to issue the request and handle response deserialization</para></summary>
    public TDescriptor FilterPath(params string[] filterpath) => Qs("filter_path", filterpath);
    ///<summary>A comma-separated list of filters used to reduce the response.<para>Use of response filtering can result in a response from OpenSearch that cannot be correctly deserialized to the respective response type for the request. In such situations, use the low level client to issue the request and handle response deserialization</para></summary>
    public TDescriptor FilterPath(IEnumerable<string> filterpath) => Qs("filter_path", filterpath);
    ///<summary>Return human readable values for statistics.</summary>
    public TDescriptor Human(bool? human = true) => Qs("human", human);
    ///<summary>Pretty format the returned JSON response.</summary>
    public TDescriptor Pretty(bool? pretty = true) => Qs("pretty", pretty);
    ///<summary>The URL-encoded request definition. Useful for libraries that do not accept a request body for non-POST requests.</summary>
    public TDescriptor SourceQueryString(string sourcequerystring) => Qs("source", sourcequerystring);
}
