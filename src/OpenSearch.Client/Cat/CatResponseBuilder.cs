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
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using OpenSearch.Net;

namespace OpenSearch.Client;

internal class CatResponseBuilder<TCatRecord> : CustomResponseBuilderBase where TCatRecord : ICatRecord
{
    public static CatResponseBuilder<TCatRecord> Instance { get; } = new CatResponseBuilder<TCatRecord>();

    public override object DeserializeResponse(IOpenSearchSerializer builtInSerializer, IApiCallDetails response, Stream stream)
    {
        if (!response.Success || response.HttpStatusCode == 404)
            return builtInSerializer.Deserialize<CatResponse<TCatRecord>>(stream);

        var catResponse = new CatResponse<TCatRecord>();
        var records = builtInSerializer.Deserialize<IReadOnlyCollection<TCatRecord>>(stream);
        catResponse.Records = records;
        return catResponse;
    }

    public override async Task<object> DeserializeResponseAsync(IOpenSearchSerializer builtInSerializer, IApiCallDetails response, Stream stream, CancellationToken ctx = default)
    {
        if (!response.Success || response.HttpStatusCode == 404)
            return await builtInSerializer.DeserializeAsync<CatResponse<TCatRecord>>(stream, ctx).ConfigureAwait(false);

        var catResponse = new CatResponse<TCatRecord>();
        var records = await builtInSerializer.DeserializeAsync<IReadOnlyCollection<TCatRecord>>(stream, ctx).ConfigureAwait(false);
        catResponse.Records = records;
        return catResponse;
    }
}
