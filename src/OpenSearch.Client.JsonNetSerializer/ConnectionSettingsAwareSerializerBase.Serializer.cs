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
using System.Linq;
using Newtonsoft.Json;
using OpenSearch.Client;
using OpenSearch.Client.JsonNetSerializer.Converters;
using OpenSearch.Net;

namespace OpenSearch.Client.JsonNetSerializer;

public abstract partial class ConnectionSettingsAwareSerializerBase
{
    protected ConnectionSettingsAwareSerializerBase(IOpenSearchSerializer builtinSerializer, IConnectionSettingsValues connectionSettings)
        : this(builtinSerializer, connectionSettings, null, null, null) { }

    internal ConnectionSettingsAwareSerializerBase(
        IOpenSearchSerializer builtinSerializer,
        IConnectionSettingsValues connectionSettings,
        Func<JsonSerializerSettings> jsonSerializerSettingsFactory,
        Action<ConnectionSettingsAwareContractResolver> modifyContractResolver,
        IEnumerable<JsonConverter> contractJsonConverters
    )
    {
        JsonSerializerSettingsFactory = jsonSerializerSettingsFactory;
        ModifyContractResolverCallback = modifyContractResolver;
        ContractJsonConverters = contractJsonConverters ?? Enumerable.Empty<JsonConverter>();

        ConnectionSettings = connectionSettings;
        BuiltinSerializer = builtinSerializer;
        Converters = new List<JsonConverter>
        {
            new HandleOscTypesOnSourceJsonConverter(BuiltinSerializer, connectionSettings.MemoryStreamFactory),
            new TimeSpanToStringConverter()
        };
        _serializer = CreateSerializer(SerializationFormatting.Indented);
        _collapsedSerializer = CreateSerializer(SerializationFormatting.None);
    }

    protected IOpenSearchSerializer BuiltinSerializer { get; }

    protected IConnectionSettingsValues ConnectionSettings { get; }
    protected IEnumerable<JsonConverter> ContractJsonConverters { get; }
    protected Func<JsonSerializerSettings> JsonSerializerSettingsFactory { get; }
    protected Action<ConnectionSettingsAwareContractResolver> ModifyContractResolverCallback { get; }

    private List<JsonConverter> Converters { get; }

    private JsonSerializer CreateSerializer(SerializationFormatting formatting)
    {
        var s = CreateJsonSerializerSettings() ?? new JsonSerializerSettings();
        var converters = CreateJsonConverters() ?? Enumerable.Empty<JsonConverter>();
        var contract = CreateContractResolver();
        s.Formatting = formatting == SerializationFormatting.Indented ? Formatting.Indented : Formatting.None;
        s.ContractResolver = contract;
        foreach (var converter in converters.Concat(Converters))
            s.Converters.Add(converter);

        return JsonSerializer.Create(s);
    }

    protected virtual ConnectionSettingsAwareContractResolver CreateContractResolver()
    {
        var contract = new ConnectionSettingsAwareContractResolver(ConnectionSettings);
        ModifyContractResolver(contract);
        return contract;
    }

    protected virtual JsonSerializerSettings CreateJsonSerializerSettings() => JsonSerializerSettingsFactory?.Invoke();

    protected virtual IEnumerable<JsonConverter> CreateJsonConverters() => ContractJsonConverters;

    protected virtual void ModifyContractResolver(ConnectionSettingsAwareContractResolver resolver) =>
        ModifyContractResolverCallback?.Invoke(resolver);
}
