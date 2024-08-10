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
using OpenSearch.Net;
using OpenSearch.Net.Utf8Json;

namespace OpenSearch.Client;

[JsonFormatter(typeof(IndexRequestFormatter<>))]
[MapsApi("index.json")]
public partial interface IIndexRequest<TDocument> : IProxyRequest, IDocumentRequest where TDocument : class
{
    TDocument Document { get; set; }
}

public partial class IndexRequest<TDocument>
    where TDocument : class
{
    public TDocument Document { get; set; }

    protected override HttpMethod HttpMethod => GetHttpMethod(this);

    void IProxyRequest.WriteJson(IOpenSearchSerializer sourceSerializer, Stream stream, SerializationFormatting formatting) =>
        sourceSerializer.Serialize(Document, stream, formatting);

    internal static HttpMethod GetHttpMethod(IIndexRequest<TDocument> request) =>
        request.Id?.StringOrLongValue != null || request.RouteValues.ContainsId ? HttpMethod.PUT : HttpMethod.POST;

    partial void DocumentFromPath(TDocument document) => Document = document;
}

public partial class IndexDescriptor<TDocument> where TDocument : class
{
    protected override HttpMethod HttpMethod => IndexRequest<TDocument>.GetHttpMethod(this);
    TDocument IIndexRequest<TDocument>.Document { get; set; }

    void IProxyRequest.WriteJson(IOpenSearchSerializer sourceSerializer, Stream stream, SerializationFormatting formatting) =>
        sourceSerializer.Serialize(Self.Document, stream, formatting);

    partial void DocumentFromPath(TDocument document) => Assign(document, (a, v) => a.Document = v);
}
