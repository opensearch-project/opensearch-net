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
/// Provides convenience extension methods to get many _source's given a list of ids.
/// </summary>
public static class SourceManyExtensions
{

    private static Func<MultiGetOperationDescriptor<T>, string, IMultiGetOperation> Lookup<T>(IndexName index)
        where T : class
    {
        if (index == null) return null;

        return (d, id) => d.Index(index);
    }

    /// <summary>
    /// SourceMany allows you to get a list of T documents out of OpenSearch, internally it calls into MultiGet()
    /// <para>
    /// Multi GET API allows to get multiple documents based on an index, type (optional) and id (and possibly routing).
    /// The response includes a docs array with all the fetched documents, each element similar in structure to a document
    /// provided by the get API.
    /// </para>
    /// <para> </para>
    /// https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/multi-get/
    /// </summary>
    /// <typeparam name="T">The type used to infer the default index and typename</typeparam>
    /// <param name="client"></param>
    /// <param name="ids">A list of ids as string</param>
    /// <param name="index">Optionally override the default inferred indexname for T</param>
    public static IEnumerable<T> SourceMany<T>(this IOpenSearchClient client, IEnumerable<string> ids, string index = null)
        where T : class
    {
        var result = client.MultiGet(s => s
            .Index(index)
            .RequestConfiguration(r => r.ThrowExceptions())
            .GetMany<T>(ids, Lookup<T>(index))
        );
        return result.SourceMany<T>(ids);
    }

    /// <summary>
    /// SourceMany allows you to get a list of T documents out of OpenSearch, internally it calls into MultiGet()
    /// <para>
    /// Multi GET API allows to get multiple documents based on an index, type (optional) and id (and possibly routing).
    /// The response includes a docs array with all the fetched documents, each element similar in structure to a document
    /// provided by the get API.
    /// </para>
    /// <para> </para>
    /// https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/multi-get/
    /// </summary>
    /// <typeparam name="T">The type used to infer the default index and typename</typeparam>
    /// <param name="client"></param>
    /// <param name="ids">A list of ids as int</param>
    /// <param name="index">Optionally override the default inferred indexname for T</param>
    public static IEnumerable<T> SourceMany<T>(this IOpenSearchClient client, IEnumerable<long> ids, string index = null)
        where T : class => client.SourceMany<T>(ids.Select(i => i.ToString(CultureInfo.InvariantCulture)), index);

    /// <summary>
    /// SourceMany allows you to get a list of T documents out of OpenSearch, internally it calls into MultiGet()
    /// <para>
    /// Multi GET API allows to get multiple documents based on an index, type (optional) and id (and possibly routing).
    /// The response includes a docs array with all the fetched documents, each element similar in structure to a document
    /// provided by the get API.
    /// </para>
    /// <para> </para>
    /// https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/multi-get/
    /// </summary>
    /// <typeparam name="T">The type used to infer the default index and typename</typeparam>
    /// <param name="client"></param>
    /// <param name="ids">A list of ids as string</param>
    /// <param name="index">Optionally override the default inferred indexname for T</param>
    public static async Task<IEnumerable<T>> SourceManyAsync<T>(
        this IOpenSearchClient client, IEnumerable<string> ids, string index = null, CancellationToken cancellationToken = default
    )
        where T : class
    {
        var response = await client.MultiGetAsync(s => s
                .Index(index)
                .RequestConfiguration(r => r.ThrowExceptions())
                .GetMany<T>(ids, Lookup<T>(index)), cancellationToken)
            .ConfigureAwait(false);
        return response.SourceMany<T>(ids);
    }

    /// <summary>
    /// SourceMany allows you to get a list of T documents out of OpenSearch, internally it calls into MultiGet()
    /// <para>
    /// Multi GET API allows to get multiple documents based on an index, type (optional) and id (and possibly routing).
    /// The response includes a docs array with all the fetched documents, each element similar in structure to a document
    /// provided by the get API.
    /// </para>
    /// <para> </para>
    /// https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/multi-get/
    /// </summary>
    /// <typeparam name="T">The type used to infer the default index and typename</typeparam>
    /// <param name="client"></param>
    /// <param name="ids">A list of ids as int</param>
    /// <param name="index">Optionally override the default inferred indexname for T</param>
    public static Task<IEnumerable<T>> SourceManyAsync<T>(
        this IOpenSearchClient client, IEnumerable<long> ids, string index = null,
        CancellationToken cancellationToken = default
    )
        where T : class => client.SourceManyAsync<T>(ids.Select(i => i.ToString(CultureInfo.InvariantCulture)), index, cancellationToken);
}
