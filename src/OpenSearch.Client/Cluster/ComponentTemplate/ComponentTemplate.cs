/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using OpenSearch.Net.Utf8Json;

namespace OpenSearch.Client;

[ReadAs(typeof(ComponentTemplate))]
public interface IComponentTemplate
{
	[DataMember(Name = "template")]
	ITemplate Template { get; set; }

	[DataMember(Name = "version")]
	long? Version { get; set; }

	[DataMember(Name = "_meta")]
	[JsonFormatter(typeof(VerbatimDictionaryInterfaceKeysFormatter<string, object>))]
	IDictionary<string, object> Meta { get; set; }
}

public class ComponentTemplate : IComponentTemplate
{
	public ITemplate Template { get; set; }
	public long? Version { get; set; }
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

public class TemplateDescriptor : DescriptorBase<TemplateDescriptor, ITemplate>, ITemplate
{
	IAliases ITemplate.Aliases { get; set; }
	ITypeMapping ITemplate.Mappings { get; set; }
	IIndexSettings ITemplate.Settings { get; set; }

	public TemplateDescriptor Aliases(Func<AliasesDescriptor, IPromise<IAliases>> aliasDescriptor) =>
		Assign(aliasDescriptor, (a, v) => a.Aliases = v?.Invoke(new AliasesDescriptor())?.Value);

	public TemplateDescriptor Map<T>(Func<TypeMappingDescriptor<T>, ITypeMapping> selector) where T : class =>
		Assign(selector, (a, v) => a.Mappings = v?.Invoke(new TypeMappingDescriptor<T>()));

	public TemplateDescriptor Map(Func<TypeMappingDescriptor<object>, ITypeMapping> selector) =>
		Assign(selector, (a, v) => a.Mappings = v?.Invoke(new TypeMappingDescriptor<object>()));

	public TemplateDescriptor Settings(Func<IndexSettingsDescriptor, IPromise<IIndexSettings>> settingsSelector) =>
		Assign(settingsSelector, (a, v) => a.Settings = v?.Invoke(new IndexSettingsDescriptor())?.Value);
}
