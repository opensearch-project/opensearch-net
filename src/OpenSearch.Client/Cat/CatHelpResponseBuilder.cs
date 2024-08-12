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

using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OpenSearch.Net;
using OpenSearch.Net.Extensions;

namespace OpenSearch.Client;

internal class CatHelpResponseBuilder : CustomResponseBuilderBase
{
    public static CatHelpResponseBuilder Instance { get; } = new CatHelpResponseBuilder();

    public override object DeserializeResponse(IOpenSearchSerializer builtInSerializer, IApiCallDetails response, Stream stream)
    {
        var catResponse = new CatResponse<CatHelpRecord>();

        if (!response.Success)
            return catResponse;

        using (stream)
        using (var ms = response.ConnectionConfiguration.MemoryStreamFactory.Create())
        {
            stream.CopyTo(ms);
            var body = ms.ToArray().Utf8String();
            Parse(catResponse, body);
        }

        return catResponse;
    }

    public override async Task<object> DeserializeResponseAsync(IOpenSearchSerializer builtInSerializer, IApiCallDetails response, Stream stream,
        CancellationToken ctx = default)
    {
        var catResponse = new CatResponse<CatHelpRecord>();

        if (!response.Success)
            return catResponse;

        using (stream)
        using (var ms = response.ConnectionConfiguration.MemoryStreamFactory.Create())
        {
            await stream.CopyToAsync(ms, 81920, ctx).ConfigureAwait(false);
            var body = ms.ToArray().Utf8String();
            Parse(catResponse, body);
        }

        return catResponse;
    }

    private static void Parse(CatResponse<CatHelpRecord> catResponse, string body) =>
        catResponse.Records = body.Split('\n')
            .Skip(1)
            .Select(f => new CatHelpRecord { Endpoint = f.Trim() })
            .ToList();
}
