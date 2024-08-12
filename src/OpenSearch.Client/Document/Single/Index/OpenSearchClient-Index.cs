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

namespace OpenSearch.Client;

public partial interface IOpenSearchClient
{
    /// <summary>
    /// Adds or updates a typed JSON document in a specific index, making it searchable.
    /// <para> </para>
    /// <a href="https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/index-document/">
    /// https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/index-document/</a>
    /// </summary>
    /// <typeparam name="TDocument">The document type used to infer the default index and id</typeparam>
    /// <param name="document">
    /// The document to be indexed. Id will be inferred from (in order):
    /// <para>1. Id property set up on <see cref="ConnectionSettings" /> for <typeparamref name="TDocument" /></para>
    /// <para>
    /// 2. <see cref="OpenSearchTypeAttribute.IdProperty" /> property on <see cref="OpenSearchTypeAttribute" /> applied to
    /// <typeparamref name="TDocument" />
    /// </para>
    /// <para>3. A property named Id on <typeparamref name="TDocument" /></para>
    /// </param>
    IndexResponse IndexDocument<TDocument>(TDocument document) where TDocument : class;

    /// <inheritdoc cref="IOpenSearchClient.IndexDocument{TDocument}" />
    Task<IndexResponse> IndexDocumentAsync<T>(T document, CancellationToken ct = default)
        where T : class;
}

public partial class OpenSearchClient
{
    /// <inheritdoc cref="IOpenSearchClient.IndexDocument{TDocument}" />
    public IndexResponse IndexDocument<TDocument>(TDocument document) where TDocument : class => Index(document, s => s);

    /// <inheritdoc cref="IOpenSearchClient.IndexDocument{TDocument}" />
    public Task<IndexResponse> IndexDocumentAsync<TDocument>(TDocument document, CancellationToken ct = default)
        where TDocument : class =>
        IndexAsync(document, s => s, ct);
}
