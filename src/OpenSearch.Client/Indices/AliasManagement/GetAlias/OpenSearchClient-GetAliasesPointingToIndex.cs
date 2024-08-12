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
using System.Threading.Tasks;
using OpenSearch.Client.Specification.IndicesApi;
using OpenSearch.Net;

namespace OpenSearch.Client;

public static class AliasPointingToIndexExtensions
{
    /// <summary>
    /// Returns a dictionary of aliases that point to the specified index, simplified version of
    /// <see cref="IndicesNamespace.GetAlias(IGetAliasRequest)" />..
    /// </summary>
    /// <param name="index">The index name we want to know aliases of</param>
    public static IReadOnlyDictionary<string, AliasDefinition> GetAliasesPointingToIndex(this IOpenSearchClient client, IndexName index)
    {
        var response = client.Indices.GetAlias(index, a => a.RequestConfiguration(r => r.ThrowExceptions()));
        return AliasesPointingToIndex(index, response);
    }

    /// <summary>
    /// Returns a dictionary of aliases that point to the specified index, simplified version of
    /// <see cref="IndicesNamespace.GetAlias(IGetAliasRequest)" />..
    /// </summary>
    /// <param name="index">The index name we want to know aliases of</param>
    public static async Task<IReadOnlyDictionary<string, AliasDefinition>> GetAliasesPointingToIndexAsync(this IOpenSearchClient client,
        IndexName index
    )
    {
        var response = await client.Indices.GetAliasAsync(index, a => a.RequestConfiguration(r => r.ThrowExceptions())).ConfigureAwait(false);
        return AliasesPointingToIndex(index, response);
    }

    private static IReadOnlyDictionary<string, AliasDefinition> AliasesPointingToIndex(IndexName index, GetAliasResponse response)
    {
        if (!response.IsValid || !response.Indices.HasAny()) return EmptyReadOnly<string, AliasDefinition>.Dictionary;

        return response.Indices[index].Aliases;
    }
}
