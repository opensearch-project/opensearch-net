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

using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OpenSearch.Client;

[DataContract]
public class ClusterNodesStats
{
    [DataMember(Name = "count")]
    public ClusterNodeCount Count { get; internal set; }

    [DataMember(Name = "discovery_types")]
    public IReadOnlyDictionary<string, int> DiscoveryTypes { get; internal set; }

    [DataMember(Name = "fs")]
    public ClusterFileSystem FileSystem { get; internal set; }

    [DataMember(Name = "jvm")]
    public ClusterJvm Jvm { get; internal set; }

    [DataMember(Name = "network_types")]
    public ClusterNetworkTypes NetworkTypes { get; internal set; }

    [DataMember(Name = "os")]
    public ClusterOperatingSystemStats OperatingSystem { get; internal set; }

    [DataMember(Name = "packaging_types")]
    public IReadOnlyCollection<NodePackagingType> PackagingTypes { get; internal set; }

    [DataMember(Name = "plugins")]
    public IReadOnlyCollection<PluginStats> Plugins { get; internal set; }

    [DataMember(Name = "process")]
    public ClusterProcess Process { get; internal set; }

    [DataMember(Name = "versions")]
    public IReadOnlyCollection<string> Versions { get; internal set; }

    [DataMember(Name = "ingest")]
    public ClusterIngestStats Ingest { get; internal set; }
}

public class NodePackagingType
{
    [DataMember(Name = "count")]
    public int Count { get; internal set; }

    [DataMember(Name = "flavor")]
    public string Flavor { get; internal set; }

    [DataMember(Name = "type")]
    public string Type { get; internal set; }
}

[DataContract]
public class ClusterNetworkTypes
{
    [DataMember(Name = "http_types")]
    public IReadOnlyDictionary<string, int> HttpTypes { get; internal set; }

    [DataMember(Name = "transport_types")]
    public IReadOnlyDictionary<string, int> TransportTypes { get; internal set; }
}

[DataContract]
public class ClusterFileSystem
{
    [DataMember(Name = "available_in_bytes")]
    public long AvailableInBytes { get; internal set; }

    [DataMember(Name = "free_in_bytes")]
    public long FreeInBytes { get; internal set; }

    [DataMember(Name = "total_in_bytes")]
    public long TotalInBytes { get; internal set; }
}

[DataContract]
public class ClusterJvm
{
    [DataMember(Name = "max_uptime_in_millis")]
    public long MaxUptimeInMilliseconds { get; internal set; }

    [DataMember(Name = "mem")]
    public ClusterJvmMemory Memory { get; internal set; }

    [DataMember(Name = "threads")]
    public long Threads { get; internal set; }

    [DataMember(Name = "versions")]
    public IReadOnlyCollection<ClusterJvmVersion> Versions { get; internal set; }
}

[DataContract]
public class ClusterJvmVersion
{
    [DataMember(Name = "bundled_jdk")]
    public bool BundledJdk { get; internal set; }

    [DataMember(Name = "count")]
    public int Count { get; internal set; }

    [DataMember(Name = "using_bundled_jdk")]
    public bool? UsingBundledJdk { get; internal set; }

    [DataMember(Name = "version")]
    public string Version { get; internal set; }

    [DataMember(Name = "vm_name")]
    public string VmName { get; internal set; }

    [DataMember(Name = "vm_vendor")]
    public string VmVendor { get; internal set; }

    [DataMember(Name = "vm_version")]
    public string VmVersion { get; internal set; }
}

[DataContract]
public class ClusterJvmMemory
{
    [DataMember(Name = "heap_max_in_bytes")]
    public long HeapMaxInBytes { get; internal set; }

    [DataMember(Name = "heap_used_in_bytes")]
    public long HeapUsedInBytes { get; internal set; }
}

[DataContract]
public class ClusterProcess
{
    [DataMember(Name = "cpu")]
    public ClusterProcessCpu Cpu { get; internal set; }

    [DataMember(Name = "open_file_descriptors")]
    public ClusterProcessOpenFileDescriptors OpenFileDescriptors { get; internal set; }
}

[DataContract]
public class ClusterProcessCpu
{
    [DataMember(Name = "percent")]
    public int Percent { get; internal set; }
}

[DataContract]
public class ClusterProcessOpenFileDescriptors
{
    [DataMember(Name = "avg")]
    public long Avg { get; internal set; }

    [DataMember(Name = "max")]
    public long Max { get; internal set; }

    [DataMember(Name = "min")]
    public long Min { get; internal set; }
}

[DataContract]
public class ClusterOperatingSystemStats
{
    [DataMember(Name = "allocated_processors")]
    public int AllocatedProcessors { get; internal set; }

    [DataMember(Name = "available_processors")]
    public int AvailableProcessors { get; internal set; }

    [DataMember(Name = "mem")]
    public OperatingSystemMemoryInfo Memory { get; internal set; }

    [DataMember(Name = "names")]
    public IReadOnlyCollection<ClusterOperatingSystemName> Names { get; internal set; }

    [DataMember(Name = "pretty_names")]
    public IReadOnlyCollection<ClusterOperatingSystemPrettyName> PrettyNames { get; internal set; }

    [DataMember(Name = "architectures")]
    public IReadOnlyCollection<ArchitectureStats> Architectures { get; internal set; }
}

[DataContract]
public class OperatingSystemMemoryInfo
{
    [DataMember(Name = "free_in_bytes")]
    public long FreeBytes { get; internal set; }

    [DataMember(Name = "free_percent")]
    public int FreePercent { get; internal set; }

    [DataMember(Name = "total_in_bytes")]
    public long TotalBytes { get; internal set; }

    [DataMember(Name = "used_in_bytes")]
    public long UsedBytes { get; internal set; }

    [DataMember(Name = "used_percent")]
    public int UsedPercent { get; internal set; }
}

[DataContract]
public class ClusterOperatingSystemName
{
    [DataMember(Name = "count")]
    public int Count { get; internal set; }

    [DataMember(Name = "name")]
    public string Name { get; internal set; }
}

[DataContract]
public class ClusterOperatingSystemPrettyName
{
    [DataMember(Name = "count")]
    public int Count { get; internal set; }

    [DataMember(Name = "pretty_name")]
    public string PrettyName { get; internal set; }
}

[DataContract]
public class ClusterNodeCount
{
    ///<remarks>Introduced in OpenSearch 2.0 instead of <see cref="Master"/></remarks>
    [DataMember(Name = "cluster_manager")]
    public int ClusterManager { get; internal set; }

    [DataMember(Name = "coordinating_only")]
    public int CoordinatingOnly { get; internal set; }

    [DataMember(Name = "data")]
    public int Data { get; internal set; }

    [DataMember(Name = "ingest")]
    public int Ingest { get; internal set; }

    ///<remarks>Deprecated as of OpenSearch 2.0, use <see cref="ClusterManager"/> instead</remarks>
    [DataMember(Name = "master")]
    public int Master { get; internal set; }

    [DataMember(Name = "total")]
    public int Total { get; internal set; }

    [DataMember(Name = "voting_only")]
    public int VotingOnly { get; internal set; }
}

[DataContract]
public class ClusterIngestStats
{
    [DataMember(Name = "number_of_pipelines")]
    public int NumberOfPipelines { get; internal set; }

    [DataMember(Name = "processor_stats")]
    public IReadOnlyDictionary<string, ClusterProcessorStats> ProcessorStats { get; internal set; }
}

[DataContract]
public class ClusterProcessorStats
{
    [DataMember(Name = "count")]
    public long Count { get; set; }

    [DataMember(Name = "current")]
    public long Current { get; set; }

    [DataMember(Name = "failed")]
    public long Failed { get; set; }

    [DataMember(Name = "time_in_millis")]
    public long TimeInMilliseconds { get; set; }
}

[DataContract]
public class ArchitectureStats
{
    [DataMember(Name = "arch")]
    public string Architecture { get; set; }

    [DataMember(Name = "count")]
    public int Count { get; set; }
}
