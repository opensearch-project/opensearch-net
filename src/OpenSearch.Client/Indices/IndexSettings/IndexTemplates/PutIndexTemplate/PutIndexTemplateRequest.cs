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

using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenSearch.Client;

[MapsApi("indices.put_template.json")]
public partial interface IPutIndexTemplateRequest : ITemplateMapping { }

public partial class PutIndexTemplateRequest
{
    public IAliases Aliases { get; set; }
    public IReadOnlyCollection<string> IndexPatterns { get; set; }

    public ITypeMapping Mappings { get; set; }

    public int? Order { get; set; }

    public IIndexSettings Settings { get; set; }

    public int? Version { get; set; }
}

public partial class PutIndexTemplateDescriptor
{
    IAliases ITemplateMapping.Aliases { get; set; }

    IReadOnlyCollection<string> ITemplateMapping.IndexPatterns { get; set; }

    ITypeMapping ITemplateMapping.Mappings { get; set; }
    int? ITemplateMapping.Order { get; set; }

    IIndexSettings ITemplateMapping.Settings { get; set; }

    int? ITemplateMapping.Version { get; set; }

    public PutIndexTemplateDescriptor Order(int? order) => Assign(order, (a, v) => a.Order = v);

    public PutIndexTemplateDescriptor Version(int? version) => Assign(version, (a, v) => a.Version = v);

    public PutIndexTemplateDescriptor IndexPatterns(params string[] patterns) => Assign(patterns, (a, v) => a.IndexPatterns = v);

    public PutIndexTemplateDescriptor IndexPatterns(IEnumerable<string> patterns) => Assign(patterns?.ToArray(), (a, v) => a.IndexPatterns = v);

    public PutIndexTemplateDescriptor Settings(Func<IndexSettingsDescriptor, IPromise<IIndexSettings>> settingsSelector) =>
        Assign(settingsSelector, (a, v) => a.Settings = v?.Invoke(new IndexSettingsDescriptor())?.Value);

    public PutIndexTemplateDescriptor Map<T>(Func<TypeMappingDescriptor<T>, ITypeMapping> selector) where T : class =>
        Assign(selector, (a, v) => a.Mappings = v?.Invoke(new TypeMappingDescriptor<T>()));

    public PutIndexTemplateDescriptor Map(Func<TypeMappingDescriptor<object>, ITypeMapping> selector) =>
        Assign(selector, (a, v) => a.Mappings = v?.Invoke(new TypeMappingDescriptor<object>()));

    [Obsolete("Mappings is no longer a dictionary, please use the simplified Map() method on this descriptor instead")]
    public PutIndexTemplateDescriptor Mappings(Func<MappingsDescriptor, ITypeMapping> mappingSelector) =>
        Assign(mappingSelector, (a, v) => a.Mappings = v?.Invoke(new MappingsDescriptor()));

    public PutIndexTemplateDescriptor Aliases(Func<AliasesDescriptor, IPromise<IAliases>> aliasDescriptor) =>
        Assign(aliasDescriptor, (a, v) => a.Aliases = v?.Invoke(new AliasesDescriptor())?.Value);
}
