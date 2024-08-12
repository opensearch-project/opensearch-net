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
using OpenSearch.Client;

namespace OpenSearch.Client;

/// <summary> Settings to control where, when, and how shards are allocated to nodes. </summary>
public interface IClusterModuleSettings
{
    /// <summary></summary>
    IAllocationAwarenessSettings AllocationAwareness { get; set; }

    /// <summary></summary>
    IAllocationFilteringSettings AllocationFiltering { get; set; }

    /// <summary></summary>
    IDiskBasedShardAllocationSettings DiskBasedShardAllocation { get; set; }

    /// <summary></summary>
    IDictionary<string, LogLevel> Logger { get; set; }

    /// <summary></summary>
    bool? ReadOnly { get; set; }

    /// <summary></summary>
    IShardAllocationSettings ShardAllocation { get; set; }

    /// <summary></summary>
    IShardBalancingHeuristicsSettings ShardBalancingHeuristics { get; set; }

    /// <summary></summary>
    IShardRebalancingSettings ShardRebalancing { get; set; }
}

/// <inheritdoc />
public class ClusterModuleSettings : IClusterModuleSettings
{
    /// <inheritdoc />
    public IAllocationAwarenessSettings AllocationAwareness { get; set; }

    /// <inheritdoc />
    public IAllocationFilteringSettings AllocationFiltering { get; set; }

    /// <inheritdoc />
    public IDiskBasedShardAllocationSettings DiskBasedShardAllocation { get; set; }

    /// <inheritdoc />
    public IDictionary<string, LogLevel> Logger { get; set; }

    /// <inheritdoc />
    public bool? ReadOnly { get; set; }

    /// <inheritdoc />
    public IShardAllocationSettings ShardAllocation { get; set; }

    /// <inheritdoc />
    public IShardBalancingHeuristicsSettings ShardBalancingHeuristics { get; set; }

    /// <inheritdoc />
    public IShardRebalancingSettings ShardRebalancing { get; set; }
}

/// <inheritdoc />
public class ClusterModuleSettingsDescriptor : DescriptorBase<ClusterModuleSettingsDescriptor, IClusterModuleSettings>, IClusterModuleSettings
{
    IAllocationAwarenessSettings IClusterModuleSettings.AllocationAwareness { get; set; }

    IAllocationFilteringSettings IClusterModuleSettings.AllocationFiltering { get; set; }

    IDiskBasedShardAllocationSettings IClusterModuleSettings.DiskBasedShardAllocation { get; set; }

    IDictionary<string, LogLevel> IClusterModuleSettings.Logger { get; set; }
    bool? IClusterModuleSettings.ReadOnly { get; set; }

    IShardAllocationSettings IClusterModuleSettings.ShardAllocation { get; set; }

    IShardBalancingHeuristicsSettings IClusterModuleSettings.ShardBalancingHeuristics { get; set; }

    IShardRebalancingSettings IClusterModuleSettings.ShardRebalancing { get; set; }

    /// <inheritdoc />
    public ClusterModuleSettingsDescriptor ShardRebalancing(bool? readOnly = true) => Assign(readOnly, (a, v) => a.ReadOnly = v);

    /// <inheritdoc />
    public ClusterModuleSettingsDescriptor Logger(IDictionary<string, LogLevel> logger) => Assign(logger, (a, v) => a.Logger = v);

    /// <inheritdoc />
    public ClusterModuleSettingsDescriptor Logger(Func<FluentDictionary<string, LogLevel>, FluentDictionary<string, LogLevel>> selector) =>
        Assign(selector, (a, v) => a.Logger = v?.Invoke(new FluentDictionary<string, LogLevel>()));

    /// <inheritdoc />
    public ClusterModuleSettingsDescriptor AllocationAwareness(Func<AllocationAwarenessSettings, IAllocationAwarenessSettings> selector) =>
        Assign(selector, (a, v) => a.AllocationAwareness = v?.Invoke(new AllocationAwarenessSettings()));

    /// <inheritdoc />
    public ClusterModuleSettingsDescriptor
        AllocationFiltering(Func<AllocationFilteringSettingsDescriptor, IAllocationFilteringSettings> selector) =>
        Assign(selector, (a, v) => a.AllocationFiltering = v?.Invoke(new AllocationFilteringSettingsDescriptor()));

    /// <inheritdoc />
    public ClusterModuleSettingsDescriptor DiskBasedShardAllocation(
        Func<DiskBasedShardAllocationSettingsDescriptor, IDiskBasedShardAllocationSettings> selector
    ) =>
        Assign(selector, (a, v) => a.DiskBasedShardAllocation = v?.Invoke(new DiskBasedShardAllocationSettingsDescriptor()));

    /// <inheritdoc />
    public ClusterModuleSettingsDescriptor ShardAllocation(Func<ShardAllocationSettingsDescriptor, IShardAllocationSettings> selector) =>
        Assign(selector, (a, v) => a.ShardAllocation = v?.Invoke(new ShardAllocationSettingsDescriptor()));

    /// <inheritdoc />
    public ClusterModuleSettingsDescriptor ShardBalancingHeuristics(
        Func<ShardBalancingHeuristicsSettingsDescriptor, IShardBalancingHeuristicsSettings> selector
    ) =>
        Assign(selector, (a, v) => a.ShardBalancingHeuristics = v?.Invoke(new ShardBalancingHeuristicsSettingsDescriptor()));

    /// <inheritdoc />
    public ClusterModuleSettingsDescriptor ShardRebalancing(Func<ShardRebalancingSettingsDescriptor, IShardRebalancingSettings> selector) =>
        Assign(selector, (a, v) => a.ShardRebalancing = v?.Invoke(new ShardRebalancingSettingsDescriptor()));
}
