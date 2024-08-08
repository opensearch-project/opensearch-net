/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Collections.Generic;

namespace OpenSearch.Client;

[MapsApi("cluster.put_component_template")]
public partial interface IPutComponentTemplateRequest : IComponentTemplate { }

public partial class PutComponentTemplateRequest
{
    public ITemplate Template { get; set; }
    public long? Version { get; set; }
    public IDictionary<string, object> Meta { get; set; }
}

public partial class PutComponentTemplateDescriptor
{
    ITemplate IComponentTemplate.Template { get; set; }
    long? IComponentTemplate.Version { get; set; }
    IDictionary<string, object> IComponentTemplate.Meta { get; set; }

    public PutComponentTemplateDescriptor Version(long? version) => Assign(version, (a, v) => a.Version = v);

    public PutComponentTemplateDescriptor Meta(Func<FluentDictionary<string, object>, FluentDictionary<string, object>> metaSelector) =>
        Assign(metaSelector(new FluentDictionary<string, object>()), (a, v) => a.Meta = v);

    public PutComponentTemplateDescriptor Meta(Dictionary<string, object> metaDictionary) => Assign(metaDictionary, (a, v) => a.Meta = v);

    public PutComponentTemplateDescriptor Template(Func<TemplateDescriptor, ITemplate> selector) =>
        Assign(selector, (a, v) => a.Template = v?.Invoke(new TemplateDescriptor()));
}
