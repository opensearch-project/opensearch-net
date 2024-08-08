/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OpenSearch.Client;

[DataContract]
public class GetComponentTemplateResponse : ResponseBase
{
    [DataMember(Name = "component_templates")]
    public IReadOnlyCollection<NamedComponentTemplate> ComponentTemplates { get; internal set; }
}

[DataContract]
public class NamedComponentTemplate
{
    [DataMember(Name = "name")]
    public string Name { get; internal set; }

    [DataMember(Name = "component_template")]
    public ComponentTemplate ComponentTemplate { get; internal set; }
}
