/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenSearch.Client;

[MapsApi("indices.put_index_template")]
public partial interface IPutComposableIndexTemplateRequest : IComposableIndexTemplate { }

public partial class PutComposableIndexTemplateRequest
{
	public IReadOnlyCollection<string> ComposedOf { get; set; }
	public IDataStreamTemplate DataStream { get; set; }
	public IReadOnlyCollection<string> IndexPatterns { get; set; }
	public long? Priority { get; set; }
	public ITemplate Template { get; set; }
	public long? Version { get; set; }
	public IDictionary<string, object> Meta { get; set; }
}

public partial class PutComposableIndexTemplateDescriptor
{
	IReadOnlyCollection<string> IComposableIndexTemplate.ComposedOf { get; set; }
	IDataStreamTemplate IComposableIndexTemplate.DataStream { get; set; }
	IReadOnlyCollection<string> IComposableIndexTemplate.IndexPatterns { get; set; }
	long? IComposableIndexTemplate.Priority { get; set; }
	ITemplate IComposableIndexTemplate.Template { get; set; }
	long? IComposableIndexTemplate.Version { get; set; }
	IDictionary<string, object> IComposableIndexTemplate.Meta { get; set; }

	public PutComposableIndexTemplateDescriptor ComposedOf(params string[] composedOf) => Assign(composedOf, (a, v) => a.ComposedOf = v);

	public PutComposableIndexTemplateDescriptor ComposedOf(IEnumerable<string> composedOf) => Assign(composedOf?.ToArray(), (a, v) => a.ComposedOf = v);

	public PutComposableIndexTemplateDescriptor DataStream(Func<DataStreamTemplateDescriptor, IDataStreamTemplate> dataStreamSelector) =>
		Assign(dataStreamSelector, (a, v) => a.DataStream = v?.Invoke(new DataStreamTemplateDescriptor()));

	public PutComposableIndexTemplateDescriptor IndexPatterns(params string[] patterns) => Assign(patterns, (a, v) => a.IndexPatterns = v);

	public PutComposableIndexTemplateDescriptor IndexPatterns(IEnumerable<string> patterns) => Assign(patterns?.ToArray(), (a, v) => a.IndexPatterns = v);

	public PutComposableIndexTemplateDescriptor Priority(long? priority) => Assign(priority, (a, v) => a.Priority = v);

	public PutComposableIndexTemplateDescriptor Template(Func<TemplateDescriptor, ITemplate> selector) =>
		Assign(selector, (a, v) => a.Template = v?.Invoke(new TemplateDescriptor()));

	public PutComposableIndexTemplateDescriptor Version(long? version) => Assign(version, (a, v) => a.Version = v);

	public PutComposableIndexTemplateDescriptor Meta(Func<FluentDictionary<string, object>, FluentDictionary<string, object>> selector) =>
		Assign(selector, (a, v) => a.Meta = v?.Invoke(new FluentDictionary<string, object>()));

	public PutComposableIndexTemplateDescriptor Meta(Dictionary<string, object> meta) => Assign(meta, (a, v) => a.Meta = v);
}
