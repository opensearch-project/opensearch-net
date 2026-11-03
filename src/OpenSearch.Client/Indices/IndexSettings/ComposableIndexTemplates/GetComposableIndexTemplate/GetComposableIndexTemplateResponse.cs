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
public class GetComposableIndexTemplateResponse : ResponseBase
{
    [DataMember(Name = "index_templates")]
    public IReadOnlyCollection<NamedComposableIndexTemplate> IndexTemplates { get; internal set; }
}

[DataContract]
public class NamedComposableIndexTemplate
{
    [DataMember(Name = "name")]
    public string Name { get; internal set; }

    [DataMember(Name = "index_template")]
    public ComposableIndexTemplate IndexTemplate { get; internal set; }
}
