/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.Collections.Generic;
using System.Runtime.Serialization;
using OpenSearch.Client.Specification.IndicesApi;
using OpenSearch.Net.Utf8Json;

namespace OpenSearch.Client.Specification.ClusterApi;

[ReadAs(typeof(ComponentTemplate))]
public interface IComponentTemplate
{
	[DataMember(Name = "template")]
	ITemplate Template { get; set; }

	[DataMember(Name = "version")]
	long Version { get; set; }

	[DataMember(Name = "_meta")]
	[JsonFormatter(typeof(VerbatimDictionaryInterfaceKeysFormatter<string, object>))]
	IDictionary<string, object> Meta { get; set; }
}

public class ComponentTemplate : IComponentTemplate
{
	public ITemplate Template { get; set; }
	public long Version { get; set; }
	public IDictionary<string, object> Meta { get; set; }
}

[ReadAs(typeof(Template))]
public interface ITemplate
{
	[DataMember(Name = "aliases")]
	IAliases Aliases { get; set; }

	[DataMember(Name = "mappings")]
	ITypeMapping Mappings { get; set; }

	[DataMember(Name = "settings")]
	IIndexSettings Settings { get; set; }
}

public class Template : ITemplate
{
	public IAliases Aliases { get; set; }
	public ITypeMapping Mappings { get; set; }
	public IIndexSettings Settings { get; set; }
}
