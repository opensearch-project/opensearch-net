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
using System.Threading;
using System.Threading.Tasks;
using OpenSearch.Net;
using OpenSearch.Net.Utf8Json;

namespace OpenSearch.Client;

/// <summary>
/// A lazily deserialized document
/// </summary>
[JsonFormatter(typeof(LazyDocumentInterfaceFormatter))]
public interface ILazyDocument
{
    /// <summary>
    /// Creates an instance of <typeparamref name="T" /> from this
    /// <see cref="ILazyDocument" /> instance
    /// </summary>
    /// <typeparam name="T">The type</typeparam>
    T As<T>();

    /// <summary>
    /// Creates an instance of <paramref name="objectType" /> from this
    /// <see cref="ILazyDocument" /> instance
    /// </summary>
    /// <param name="objectType">The type</param>
    object As(Type objectType);

    /// <summary>
    /// Creates an instance of <typeparamref name="T" /> from this
    /// <see cref="ILazyDocument" /> instance
    /// </summary>
    /// <typeparam name="T">The type</typeparam>
    Task<T> AsAsync<T>(CancellationToken ct = default);

    /// <summary>
    /// Creates an instance of <paramref name="objectType" /> from this
    /// <see cref="ILazyDocument" /> instance
    /// </summary>
    /// <param name="objectType">The type</param>
    Task<object> AsAsync(Type objectType, CancellationToken ct = default);
}

/// <inheritdoc />
[JsonFormatter(typeof(LazyDocumentFormatter))]
public class LazyDocument : ILazyDocument
{
    private readonly IOpenSearchSerializer _sourceSerializer;
    private readonly IOpenSearchSerializer _requestResponseSerializer;
    private readonly IMemoryStreamFactory _memoryStreamFactory;

    internal LazyDocument(byte[] bytes, IJsonFormatterResolver formatterResolver)
    {
        Bytes = bytes;
        var settings = formatterResolver.GetConnectionSettings();
        _sourceSerializer = settings.SourceSerializer;
        _requestResponseSerializer = settings.RequestResponseSerializer;
        _memoryStreamFactory = settings.MemoryStreamFactory;
    }

    internal byte[] Bytes { get; }

    internal T AsUsingRequestResponseSerializer<T>()
    {
        using (var ms = _memoryStreamFactory.Create(Bytes))
            return _requestResponseSerializer.Deserialize<T>(ms);
    }

    /// <inheritdoc />
    public T As<T>()
    {
        using (var ms = _memoryStreamFactory.Create(Bytes))
            return _sourceSerializer.Deserialize<T>(ms);
    }

    /// <inheritdoc />
    public object As(Type objectType)
    {
        using (var ms = _memoryStreamFactory.Create(Bytes))
            return _sourceSerializer.Deserialize(objectType, ms);
    }

    /// <inheritdoc />
    public Task<T> AsAsync<T>(CancellationToken ct = default)
    {
        using (var ms = _memoryStreamFactory.Create(Bytes))
            return _sourceSerializer.DeserializeAsync<T>(ms, ct);
    }

    /// <inheritdoc />
    public Task<object> AsAsync(Type objectType, CancellationToken ct = default)
    {
        using (var ms = _memoryStreamFactory.Create(Bytes))
            return _sourceSerializer.DeserializeAsync(objectType, ms, ct);
    }
}
