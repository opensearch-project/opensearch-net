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
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenSearch.Client;

/// <summary>
/// Provides GetMany extensions that make it easier to get many documents given a list of ids
/// </summary>
public static class GetManyExtensions
{
    private static Func<MultiGetOperationDescriptor<T>, string, IMultiGetOperation> Lookup<T>(IndexName index)
        where T : class
    {
        if (index == null) return null;

        return (d, id) => d.Index(index);
    }

    /// <summary>
    /// Multi GET API allows to get multiple documents based on an index, type (optional) and id (and possibly routing).
    /// The response includes a docs array with all the fetched documents, each element similar in structure to a document
    /// provided by the get API.
    /// <para> </para>
    /// https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/multi-get/
    /// </summary>
    /// <typeparam name="T">The type used to infer the default index and typename</typeparam>
    /// <param name="client"></param>
    /// <param name="ids">IEnumerable of ids as string for the documents to fetch</param>
    /// <param name="index">Optionally override the default inferred index name for T</param>
    /// <param name="type">Optionally overiide the default inferred typename for T</param>
    public static IEnumerable<IMultiGetHit<T>> GetMany<T>(this IOpenSearchClient client, IEnumerable<string> ids, IndexName index = null)
        where T : class
    {
        var result = client.MultiGet(s => s
            .Index(index)
            .RequestConfiguration(r => r.ThrowExceptions())
            .GetMany<T>(ids, Lookup<T>(index))
        );
        return result.GetMany<T>(ids);
    }

    /// <summary>
    /// Multi GET API allows to get multiple documents based on an index, type (optional) and id (and possibly routing).
    /// The response includes a docs array with all the fetched documents, each element similar in structure to a document
    /// provided by the get API.
    /// <para> </para>
    /// https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/multi-get/
    /// </summary>
    /// <typeparam name="T">The type used to infer the default index and typename</typeparam>
    /// <param name="client"></param>
    /// <param name="ids">IEnumerable of ids as ints for the documents to fetch</param>
    /// <param name="index">Optionally override the default inferred index name for T</param>
    /// <param name="type">Optionally overiide the default inferred typename for T</param>
    public static IEnumerable<IMultiGetHit<T>> GetMany<T>(this IOpenSearchClient client, IEnumerable<long> ids, IndexName index = null)
        where T : class => client.GetMany<T>(ids.Select(i => i.ToString(CultureInfo.InvariantCulture)), index);

    /// <summary>
    /// Multi GET API allows to get multiple documents based on an index, type (optional) and id (and possibly routing).
    /// The response includes a docs array with all the fetched documents, each element similar in structure to a document
    /// provided by the get API.
    /// <para> </para>
    /// https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/multi-get/
    /// </summary>
    /// <typeparam name="T">The type used to infer the default index and typename</typeparam>
    /// <param name="client"></param>
    /// <param name="ids">IEnumerable of ids as string for the documents to fetch</param>
    /// <param name="index">Optionally override the default inferred index name for T</param>
    /// <param name="type">Optionally overiide the default inferred typename for T</param>
    public static async Task<IEnumerable<IMultiGetHit<T>>> GetManyAsync<T>(
        this IOpenSearchClient client, IEnumerable<string> ids, IndexName index = null, CancellationToken cancellationToken = default)
        where T : class
    {
        var response = await client.MultiGetAsync(s => s
                    .Index(index)
                    .RequestConfiguration(r => r.ThrowExceptions())
                    .GetMany<T>(ids, Lookup<T>(index)),
                cancellationToken
            )
            .ConfigureAwait(false);
        return response.GetMany<T>(ids);
    }

    /// <summary>
    /// Multi GET API allows to get multiple documents based on an index, type (optional) and id (and possibly routing).
    /// The response includes a docs array with all the fetched documents, each element similar in structure to a document
    /// provided by the get API.
    /// <para> </para>
    /// https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/multi-get/
    /// </summary>
    /// <typeparam name="T">The type used to infer the default index and typename</typeparam>
    /// <param name="client"></param>
    /// <param name="ids">IEnumerable of ids as ints for the documents to fetch</param>
    /// <param name="index">Optionally override the default inferred index name for T</param>
    /// <param name="type">Optionally overiide the default inferred typename for T</param>
    public static Task<IEnumerable<IMultiGetHit<T>>> GetManyAsync<T>(
        this IOpenSearchClient client, IEnumerable<long> ids, IndexName index = null, CancellationToken cancellationToken = default
    )
        where T : class => client.GetManyAsync<T>(ids.Select(i => i.ToString(CultureInfo.InvariantCulture)), index, cancellationToken);
}
