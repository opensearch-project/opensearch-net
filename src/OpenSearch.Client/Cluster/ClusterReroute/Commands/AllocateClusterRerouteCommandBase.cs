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

using System.Runtime.Serialization;

namespace OpenSearch.Client;

public interface IAllocateClusterRerouteCommand : IClusterRerouteCommand
{
    [DataMember(Name = "index")]
    IndexName Index { get; set; }

    [DataMember(Name = "node")]
    string Node { get; set; }

    [DataMember(Name = "shard")]
    int? Shard { get; set; }
}

public interface IAllocateReplicaClusterRerouteCommand : IAllocateClusterRerouteCommand { }

public interface IAllocateEmptyPrimaryRerouteCommand : IAllocateClusterRerouteCommand
{
    [DataMember(Name = "accept_data_loss")]
    bool? AcceptDataLoss { get; set; }
}

public interface IAllocateStalePrimaryRerouteCommand : IAllocateClusterRerouteCommand
{
    [DataMember(Name = "accept_data_loss")]
    bool? AcceptDataLoss { get; set; }
}

public abstract class AllocateClusterRerouteCommandBase : IAllocateClusterRerouteCommand
{
    public IndexName Index { get; set; }
    public abstract string Name { get; }

    public string Node { get; set; }

    public int? Shard { get; set; }
}

public class AllocateReplicaClusterRerouteCommand : AllocateClusterRerouteCommandBase, IAllocateReplicaClusterRerouteCommand
{
    public override string Name => "allocate_replica";
}

public class AllocateEmptyPrimaryRerouteCommand : AllocateClusterRerouteCommandBase, IAllocateEmptyPrimaryRerouteCommand
{
    public bool? AcceptDataLoss { get; set; }
    public override string Name => "allocate_empty_primary";
}

public class AllocateStalePrimaryRerouteCommand : AllocateClusterRerouteCommandBase, IAllocateStalePrimaryRerouteCommand
{
    public bool? AcceptDataLoss { get; set; }
    public override string Name => "allocate_stale_primary";
}

public abstract class AllocateClusterRerouteCommandDescriptorBase<TDescriptor, TInterface>
    : DescriptorBase<TDescriptor, TInterface>, IAllocateClusterRerouteCommand
    where TDescriptor : AllocateClusterRerouteCommandDescriptorBase<TDescriptor, TInterface>, TInterface, IAllocateClusterRerouteCommand
    where TInterface : class, IAllocateClusterRerouteCommand
{
    public abstract string Name { get; }

    IndexName IAllocateClusterRerouteCommand.Index { get; set; }
    string IClusterRerouteCommand.Name => Name;

    string IAllocateClusterRerouteCommand.Node { get; set; }

    int? IAllocateClusterRerouteCommand.Shard { get; set; }

    public TDescriptor Index(IndexName index) => Assign(index, (a, v) => a.Index = v);

    public TDescriptor Index<T>() where T : class => Assign(typeof(T), (a, v) => a.Index = v);

    public TDescriptor Shard(int? shard) => Assign(shard, (a, v) => a.Shard = v);

    public TDescriptor Node(string node) => Assign(node, (a, v) => a.Node = v);
}

public class AllocateReplicaClusterRerouteCommandDescriptor
    : AllocateClusterRerouteCommandDescriptorBase<AllocateReplicaClusterRerouteCommandDescriptor, IAllocateReplicaClusterRerouteCommand>,
        IAllocateReplicaClusterRerouteCommand
{
    public override string Name => "allocate_replica";
}

public class AllocateEmptyPrimaryRerouteCommandDescriptor
    : AllocateClusterRerouteCommandDescriptorBase<AllocateEmptyPrimaryRerouteCommandDescriptor, IAllocateEmptyPrimaryRerouteCommand>,
        IAllocateEmptyPrimaryRerouteCommand
{
    public override string Name => "allocate_empty_primary";

    bool? IAllocateEmptyPrimaryRerouteCommand.AcceptDataLoss { get; set; }

    public AllocateEmptyPrimaryRerouteCommandDescriptor AcceptDataLoss(bool? acceptDataLoss = true) =>
        Assign(acceptDataLoss, (a, v) => a.AcceptDataLoss = v);
}

public class AllocateStalePrimaryRerouteCommandDescriptor
    : AllocateClusterRerouteCommandDescriptorBase<AllocateStalePrimaryRerouteCommandDescriptor, IAllocateStalePrimaryRerouteCommand>,
        IAllocateStalePrimaryRerouteCommand
{
    public override string Name => "allocate_stale_primary";

    bool? IAllocateStalePrimaryRerouteCommand.AcceptDataLoss { get; set; }

    public AllocateStalePrimaryRerouteCommandDescriptor AcceptDataLoss(bool? acceptDataLoss = true) =>
        Assign(acceptDataLoss, (a, v) => a.AcceptDataLoss = v);
}
