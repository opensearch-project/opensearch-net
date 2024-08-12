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
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace OpenSearch.Net;

public interface IOpenSearchSerializer
{
    /// <summary> Deserialize <paramref name="stream"/> to an instance of <paramref name="type"/> </summary>
    object Deserialize(Type type, Stream stream);

    /// <summary> Deserialize <paramref name="stream"/> to an instance of <typeparamref name="T" /></summary>
    T Deserialize<T>(Stream stream);

    /// <inheritdoc cref="Deserialize"/>
    Task<object> DeserializeAsync(Type type, Stream stream, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="Deserialize{T}"/>
    Task<T> DeserializeAsync<T>(Stream stream, CancellationToken cancellationToken = default);

    /// <summary>
    /// Serialize an instance of <typeparamref name="T"/> to <paramref name="stream"/> using <paramref name="formatting"/>.
    /// </summary>
    /// <param name="data">The instance of <typeparamref name="T"/> that we want to serialize</param>
    /// <param name="stream">The stream to serialize to</param>
    /// <param name="formatting">
    /// Formatting hint, note no all implementations of <see cref="IOpenSearchSerializer"/> are able to
    /// satisfy this hint.
    /// </param>
    void Serialize<T>(T data, Stream stream, SerializationFormatting formatting = SerializationFormatting.None);

    /// <inheritdoc cref="Serialize{T}"/>
    Task SerializeAsync<T>(
        T data,
        Stream stream,
        SerializationFormatting formatting = SerializationFormatting.None,
        CancellationToken cancellationToken = default
    );
}
