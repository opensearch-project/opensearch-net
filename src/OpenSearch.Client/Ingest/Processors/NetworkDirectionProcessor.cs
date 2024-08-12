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
using System.Linq.Expressions;
using System.Runtime.Serialization;
using OpenSearch.Net.Utf8Json;

namespace OpenSearch.Client;

[InterfaceDataContract]
public interface INetworkDirectionProcessor : IProcessor
{
    [DataMember(Name = "destination_ip")]
    Field DestinationIp { get; set; }

    [DataMember(Name = "ignore_missing")]
    bool? IgnoreMissing { get; set; }

    [DataMember(Name = "internal_networks")]
    IEnumerable<string> InternalNetworks { get; set; }

    [DataMember(Name = "source_ip")]
    Field SourceIp { get; set; }

    [DataMember(Name = "target_field")]
    Field TargetField { get; set; }
}

public class NetworkDirectionProcessor : ProcessorBase, INetworkDirectionProcessor
{
    protected override string Name => "network_direction";

    /// <inheritdoc />
    public Field DestinationIp { get; set; }
    /// <inheritdoc />
    public bool? IgnoreMissing { get; set; }
    /// <inheritdoc />
    public IEnumerable<string> InternalNetworks { get; set; }
    /// <inheritdoc />
    public Field SourceIp { get; set; }
    /// <inheritdoc />
    public Field TargetField { get; set; }
}

/// <inheritdoc cref="IFingerprintProcessor" />
public class NetworkDirectionProcessorDescriptor<T>
    : ProcessorDescriptorBase<NetworkDirectionProcessorDescriptor<T>, INetworkDirectionProcessor>, INetworkDirectionProcessor
    where T : class
{
    protected override string Name => "network_direction";

    Field INetworkDirectionProcessor.DestinationIp { get; set; }
    bool? INetworkDirectionProcessor.IgnoreMissing { get; set; }
    IEnumerable<string> INetworkDirectionProcessor.InternalNetworks { get; set; }
    Field INetworkDirectionProcessor.SourceIp { get; set; }
    Field INetworkDirectionProcessor.TargetField { get; set; }

    /// <inheritdoc cref="INetworkDirectionProcessor.DestinationIp" />
    public NetworkDirectionProcessorDescriptor<T> DestinationIp(Field field) => Assign(field, (a, v) => a.DestinationIp = v);

    /// <inheritdoc cref="INetworkDirectionProcessor.DestinationIp" />
    public NetworkDirectionProcessorDescriptor<T> DestinationIp<TValue>(Expression<Func<T, TValue>> objectPath) =>
        Assign(objectPath, (a, v) => a.DestinationIp = v);

    /// <inheritdoc cref="INetworkDirectionProcessor.IgnoreMissing" />
    public NetworkDirectionProcessorDescriptor<T> IgnoreMissing(bool? ignoreMissing = true) => Assign(ignoreMissing, (a, v) => a.IgnoreMissing = v);

    /// <inheritdoc cref="INetworkDirectionProcessor.InternalNetworks" />
    public NetworkDirectionProcessorDescriptor<T> InternalNetworks(IEnumerable<string> internalNetworks) => Assign(internalNetworks, (a, v) => a.InternalNetworks = v);

    /// <inheritdoc cref="INetworkDirectionProcessor.InternalNetworks" />
    public NetworkDirectionProcessorDescriptor<T> InternalNetworks(params string[] internalNetworks) => Assign(internalNetworks, (a, v) => a.InternalNetworks = v);

    /// <inheritdoc cref="INetworkDirectionProcessor.SourceIp" />
    public NetworkDirectionProcessorDescriptor<T> SourceIp(Field field) => Assign(field, (a, v) => a.SourceIp = v);

    /// <inheritdoc cref="INetworkDirectionProcessor.SourceIp" />
    public NetworkDirectionProcessorDescriptor<T> SourceIp<TValue>(Expression<Func<T, TValue>> objectPath) =>
        Assign(objectPath, (a, v) => a.SourceIp = v);

    /// <inheritdoc cref="INetworkDirectionProcessor.TargetField" />
    public NetworkDirectionProcessorDescriptor<T> TargetField(Field field) => Assign(field, (a, v) => a.TargetField = v);

    /// <inheritdoc cref="INetworkDirectionProcessor.TargetField" />
    public NetworkDirectionProcessorDescriptor<T> TargetField<TValue>(Expression<Func<T, TValue>> objectPath) =>
        Assign(objectPath, (a, v) => a.TargetField = v);
}
