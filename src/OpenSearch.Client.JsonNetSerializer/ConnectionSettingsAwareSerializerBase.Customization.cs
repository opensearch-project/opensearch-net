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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OpenSearch.Net;

namespace OpenSearch.Client.JsonNetSerializer;

public abstract partial class ConnectionSettingsAwareSerializerBase : IOpenSearchSerializer
{
    // Default buffer size of StreamWriter, which is private :(
    internal const int DefaultBufferSize = 1024;

    private static readonly Task CompletedTask = Task.CompletedTask;

    internal static readonly Encoding ExpectedEncoding = new UTF8Encoding(false);
    private readonly JsonSerializer _collapsedSerializer;

    private readonly JsonSerializer _serializer;
    protected virtual int BufferSize => DefaultBufferSize;

    public T Deserialize<T>(Stream stream)
    {
        using (var streamReader = new StreamReader(stream))
        using (var jsonTextReader = new JsonTextReader(streamReader))
            return _serializer.Deserialize<T>(jsonTextReader);
    }

    public object Deserialize(Type type, Stream stream)
    {
        using (var streamReader = new StreamReader(stream))
        using (var jsonTextReader = new JsonTextReader(streamReader))
            return _serializer.Deserialize(jsonTextReader, type);
    }

    public virtual async Task<T> DeserializeAsync<T>(Stream stream, CancellationToken cancellationToken = default)
    {
        using (var streamReader = new StreamReader(stream))
        using (var jsonTextReader = new JsonTextReader(streamReader))
        {
            var token = await jsonTextReader.ReadTokenWithDateParseHandlingNoneAsync(cancellationToken).ConfigureAwait(false);
            return token.ToObject<T>(_serializer);
        }
    }

    public virtual async Task<object> DeserializeAsync(Type type, Stream stream, CancellationToken cancellationToken = default(CancellationToken))
    {
        using (var streamReader = new StreamReader(stream))
        using (var jsonTextReader = new JsonTextReader(streamReader))
        {
            var token = await jsonTextReader.ReadTokenWithDateParseHandlingNoneAsync(cancellationToken).ConfigureAwait(false);
            return token.ToObject(type, _serializer);
        }
    }

    public void Serialize<T>(T data, Stream stream, SerializationFormatting formatting = SerializationFormatting.None)
    {
        using (var writer = new StreamWriter(stream, ExpectedEncoding, BufferSize, true))
        using (var jsonWriter = new JsonTextWriter(writer))
        {
            var serializer = formatting == SerializationFormatting.Indented ? _serializer : _collapsedSerializer;
            serializer.Serialize(jsonWriter, data);
        }
    }

    public Task SerializeAsync<T>(T data, Stream stream, SerializationFormatting formatting = SerializationFormatting.None,
        CancellationToken cancellationToken = default
    )
    {
        Serialize(data, stream, formatting);
        return CompletedTask;
    }
}
