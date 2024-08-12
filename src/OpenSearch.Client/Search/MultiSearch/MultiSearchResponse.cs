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
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using OpenSearch.Net;
using OpenSearch.Net.Utf8Json;

namespace OpenSearch.Client;

[DataContract]
[JsonFormatter(typeof(MultiSearchResponseFormatter))]
public class MultiSearchResponse : ResponseBase
{
    public MultiSearchResponse() => Responses = new Dictionary<string, IResponse>();

    public long Took { get; set; }

    public IEnumerable<IResponse> AllResponses => _allResponses<IResponse>();

    public override bool IsValid => base.IsValid && AllResponses.All(b => b.IsValid);

    public int TotalResponses => Responses.HasAny() ? Responses.Count() : 0;

    [JsonFormatter(typeof(VerbatimDictionaryInterfaceKeysFormatter<string, IResponse>))]
    internal IDictionary<string, IResponse> Responses { get; set; }

    public IEnumerable<IResponse> GetInvalidResponses() => _allResponses<IResponse>().Where(r => !r.IsValid);

    public ISearchResponse<T> GetResponse<T>(string name) where T : class
    {
        if (!Responses.TryGetValue(name, out var response))
            return null;

        if (response is IOpenSearchResponse OpenSearchResponse)
            OpenSearchResponse.ApiCall = ApiCall;

        return response as ISearchResponse<T>;
    }

    public IEnumerable<ISearchResponse<T>> GetResponses<T>() where T : class => _allResponses<SearchResponse<T>>();

    protected override void DebugIsValid(StringBuilder sb)
    {
        sb.AppendLine($"# Invalid searches (inspect individual response.DebugInformation for more detail):");
        foreach (var i in AllResponses.Select((item, i) => new { item, i }).Where(i => !i.item.IsValid))
            sb.AppendLine($"  search[{i.i}]: {i.item}");
    }

    private IEnumerable<T> _allResponses<T>() where T : class, IResponse, IOpenSearchResponse
    {
        foreach (var r in Responses.Values.OfType<T>())
        {
            r.ApiCall = ApiCall;
            yield return r;
        }
    }
}
