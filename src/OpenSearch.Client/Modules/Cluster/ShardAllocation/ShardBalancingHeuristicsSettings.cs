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

namespace OpenSearch.Client;

public interface IShardBalancingHeuristicsSettings
{
    /// <summary>
    /// Defines a factor to the number of shards per index allocated on a specific node (float). Defaults to 0.55f.
    /// Raising this raises the tendency to equalize the number of shards per index across all nodes in the cluster.
    /// </summary>
    float? BalanceIndex { get; set; }

    /// <summary>
    /// Defines the weight factor for shards allocated on a node (float). Defaults to 0.45f.
    ///  Raising this raises the tendency to equalize the number of shards across all nodes in the cluster.
    /// </summary>
    float? BalanceShard { get; set; }

    /// <summary>
    /// Minimal optimization value of operations that should be performed (non negative float). Defaults to 1.0f.
    ///  Raising this will cause the cluster to be less aggressive about optimizing the shard balance
    /// </summary>
    float? BalanceThreshold { get; set; }
}

public class ShardBalancingHeuristicsSettings : IShardBalancingHeuristicsSettings
{
    /// <inheritdoc />
    public float? BalanceIndex { get; set; }

    /// <inheritdoc />
    public float? BalanceShard { get; set; }

    /// <inheritdoc />
    public float? BalanceThreshold { get; set; }
}

public class ShardBalancingHeuristicsSettingsDescriptor
    : DescriptorBase<ShardBalancingHeuristicsSettingsDescriptor, IShardBalancingHeuristicsSettings>, IShardBalancingHeuristicsSettings
{
    float? IShardBalancingHeuristicsSettings.BalanceIndex { get; set; }
    float? IShardBalancingHeuristicsSettings.BalanceShard { get; set; }

    float? IShardBalancingHeuristicsSettings.BalanceThreshold { get; set; }

    /// <inheritdoc />
    public ShardBalancingHeuristicsSettingsDescriptor BalanceShard(float? balance) => Assign(balance, (a, v) => a.BalanceShard = v);

    /// <inheritdoc />
    public ShardBalancingHeuristicsSettingsDescriptor BalanceIndex(float? balance) => Assign(balance, (a, v) => a.BalanceIndex = v);

    /// <inheritdoc />
    public ShardBalancingHeuristicsSettingsDescriptor BalanceThreshold(float? balance) => Assign(balance, (a, v) => a.BalanceThreshold = v);
}
