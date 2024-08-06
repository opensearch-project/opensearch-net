/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

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
