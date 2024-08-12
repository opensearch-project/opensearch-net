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
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenSearch.Client;
using OpenSearch.Net;

namespace OpenSearch.Client.JsonNetSerializer.Converters;

public class HandleOscTypesOnSourceJsonConverter : JsonConverter
{
    private static readonly HashSet<Type> OscTypesThatCanAppearInSource = new HashSet<Type>
    {
        typeof(JoinField),
        typeof(QueryContainer),
        typeof(CompletionField),
        typeof(Attachment),
        typeof(ILazyDocument),
        typeof(LazyDocument),
        typeof(GeoCoordinate),
        typeof(GeoLocation),
        typeof(CartesianPoint),
    };

    private readonly IOpenSearchSerializer _builtInSerializer;
    private IMemoryStreamFactory _memoryStreamFactory;

    public HandleOscTypesOnSourceJsonConverter(IOpenSearchSerializer builtInSerializer, IMemoryStreamFactory memoryStreamFactory
    )
    {
        _builtInSerializer = builtInSerializer;
        _memoryStreamFactory = memoryStreamFactory;
    }

    public override bool CanRead => true;
    public override bool CanWrite => true;

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        var formatting = serializer.Formatting == Formatting.Indented
            ? SerializationFormatting.Indented
            : SerializationFormatting.None;

        using (var ms = _memoryStreamFactory.Create())
        using (var streamReader = new StreamReader(ms, ConnectionSettingsAwareSerializerBase.ExpectedEncoding))
        using (var reader = new JsonTextReader(streamReader))
        {
            _builtInSerializer.Serialize(value, ms, formatting);
            ms.Position = 0;
            var token = reader.ReadTokenWithDateParseHandlingNone();
            writer.WriteToken(token.CreateReader(), true);
        }
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        var token = reader.ReadTokenWithDateParseHandlingNone();
        //in place because JsonConverter.Deserialize() only works on full json objects.
        //even though we pass type JSON.NET won't try the registered converter for that type
        //even if it can handle string tokens :(
        if (objectType == typeof(JoinField) && token.Type == JTokenType.String)
            return JoinField.Root(token.Value<string>());

        using (var ms = token.ToStream(_memoryStreamFactory))
            return _builtInSerializer.Deserialize(objectType, ms);
    }

    public override bool CanConvert(Type objectType) =>
        OscTypesThatCanAppearInSource.Contains(objectType) ||
        typeof(IGeoShape).IsAssignableFrom(objectType) ||
        typeof(IGeometryCollection).IsAssignableFrom(objectType);
}
