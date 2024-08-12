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
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using OpenSearch.Net.Diagnostics;
using OpenSearch.Net.Utf8Json;

namespace OpenSearch.Net;

public class SerializerRegistrationInformation
{
    private readonly string _stringRepresentation;

    public SerializerRegistrationInformation(Type type, string purpose)
    {
        TypeInformation = type;
        Purpose = purpose;
        _stringRepresentation = $"{Purpose}: {TypeInformation.FullName}";
    }


    public Type TypeInformation { get; }

    /// <summary>
    /// A string describing the purpose of the serializer emitting this events.
    /// <para>In `Elastisearch.Net` this will always be "request/response"</para>
    /// <para>Using `OpenSearch.Client` this could also be `source` allowing you to differentiate between the internal and configured source serializer</para>
    /// </summary>
    public string Purpose { get; }

    public override string ToString() => _stringRepresentation;
}

/// <summary>
/// Wraps configured serializer so that we can emit diagnostics per configured serializer.
/// </summary>
internal class DiagnosticsSerializerProxy : IOpenSearchSerializer, IInternalSerializer
{
    private readonly IOpenSearchSerializer _serializer;
    private readonly bool _wrapsUtf8JsonSerializer;
    private readonly SerializerRegistrationInformation _state;
    private readonly IJsonFormatterResolver _formatterResolver;
    private static DiagnosticSource DiagnosticSource { get; } = new DiagnosticListener(DiagnosticSources.Serializer.SourceName);

    public DiagnosticsSerializerProxy(IOpenSearchSerializer serializer, string purpose = "request/response")
    {
        _serializer = serializer;
        _state = new SerializerRegistrationInformation(serializer.GetType(), purpose);
        if (serializer is IInternalSerializer s && s.TryGetJsonFormatter(out var formatterResolver))
        {
            _formatterResolver = formatterResolver;
            _wrapsUtf8JsonSerializer = true;
        }
        else
        {
            _formatterResolver = null;
            _wrapsUtf8JsonSerializer = false;
        }
    }

    public bool TryGetJsonFormatter(out IJsonFormatterResolver formatterResolver)
    {
        formatterResolver = _formatterResolver;
        return _wrapsUtf8JsonSerializer;
    }

    public object Deserialize(Type type, Stream stream)
    {
        using (DiagnosticSource.Diagnose(DiagnosticSources.Serializer.Deserialize, _state))
            return _serializer.Deserialize(type, stream);
    }


    public T Deserialize<T>(Stream stream)
    {
        using (DiagnosticSource.Diagnose(DiagnosticSources.Serializer.Deserialize, _state))
            return _serializer.Deserialize<T>(stream);
    }

    public Task<object> DeserializeAsync(Type type, Stream stream, CancellationToken cancellationToken = default)
    {
        using (DiagnosticSource.Diagnose(DiagnosticSources.Serializer.Deserialize, _state))
            return _serializer.DeserializeAsync(type, stream, cancellationToken);
    }

    public Task<T> DeserializeAsync<T>(Stream stream, CancellationToken cancellationToken = default)
    {
        using (DiagnosticSource.Diagnose(DiagnosticSources.Serializer.Deserialize, _state))
            return _serializer.DeserializeAsync<T>(stream, cancellationToken);
    }

    public void Serialize<T>(T data, Stream stream, SerializationFormatting formatting = SerializationFormatting.None)
    {
        using (DiagnosticSource.Diagnose(DiagnosticSources.Serializer.Serialize, _state))
            _serializer.Serialize(data, stream, formatting);
    }

    public Task SerializeAsync<T>(T data, Stream stream, SerializationFormatting formatting = SerializationFormatting.None,
        CancellationToken cancellationToken = default
    )
    {
        using (DiagnosticSource.Diagnose(DiagnosticSources.Serializer.Serialize, _state))
            return _serializer.SerializeAsync(data, stream, formatting, cancellationToken);
    }

}
