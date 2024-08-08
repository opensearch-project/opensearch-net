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
using OpenSearch.Net.Utf8Json;

namespace OpenSearch.Net
{
    public class LowLevelRequestResponseSerializer : IOpenSearchSerializer, IInternalSerializer
    {
        public static readonly LowLevelRequestResponseSerializer Instance = new LowLevelRequestResponseSerializer();

        public object Deserialize(Type type, Stream stream) =>
            JsonSerializer.NonGeneric.Deserialize(type, stream, OpenSearchNetFormatterResolver.Instance);

        public T Deserialize<T>(Stream stream) =>
            JsonSerializer.Deserialize<T>(stream, OpenSearchNetFormatterResolver.Instance);

        public Task<object> DeserializeAsync(Type type, Stream stream, CancellationToken cancellationToken = default) =>
            JsonSerializer.NonGeneric.DeserializeAsync(type, stream, OpenSearchNetFormatterResolver.Instance);

        public Task<T> DeserializeAsync<T>(Stream stream, CancellationToken cancellationToken = default) =>
            JsonSerializer.DeserializeAsync<T>(stream, OpenSearchNetFormatterResolver.Instance);

        public void Serialize<T>(T data, Stream writableStream, SerializationFormatting formatting = SerializationFormatting.None) =>
            JsonSerializer.Serialize(writableStream, data, OpenSearchNetFormatterResolver.Instance);

        public Task SerializeAsync<T>(T data, Stream writableStream, SerializationFormatting formatting,
            CancellationToken cancellationToken = default
        ) =>
            JsonSerializer.SerializeAsync(writableStream, data, OpenSearchNetFormatterResolver.Instance);

        bool IInternalSerializer.TryGetJsonFormatter(out IJsonFormatterResolver formatterResolver)
        {
            formatterResolver = OpenSearchNetFormatterResolver.Instance;
            return true;
        }
    }
}
