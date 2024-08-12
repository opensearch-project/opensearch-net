/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.Runtime.Serialization;

namespace OpenSearch.Client;

[DataContract]
public class CatPitSegmentsRecord : ICatRecord
{
    [DataMember(Name = "committed")]
    public string Committed { get; set; }

    [DataMember(Name = "compound")]
    public string Compound { get; set; }

    [DataMember(Name = "docs.count")]
    public string DocsCount { get; set; }

    [DataMember(Name = "docs.deleted")]
    public string DocsDeleted { get; set; }

    [DataMember(Name = "generation")]
    public string Generation { get; set; }

    [DataMember(Name = "id")]
    public string Id { get; set; }

    [DataMember(Name = "index")]
    public string Index { get; set; }

    [DataMember(Name = "ip")]
    public string Ip { get; set; }

    [DataMember(Name = "prirep")]
    public string PrimaryOrReplica { get; set; }

    [DataMember(Name = "searchable")]
    public string Searchable { get; set; }

    [DataMember(Name = "segment")]
    public string Segment { get; set; }

    [DataMember(Name = "shard")]
    public string Shard { get; set; }

    [DataMember(Name = "size")]
    public string Size { get; set; }

    [DataMember(Name = "size.memory")]
    public string SizeMemory { get; set; }

    [DataMember(Name = "version")]
    public string Version { get; set; }
}
