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
using System.Runtime.Serialization;
using OpenSearch.Net.Utf8Json;

namespace OpenSearch.Client;

[MapsApi("render_search_template.json")]
public partial interface IRenderSearchTemplateRequest
{
    [DataMember(Name = "file")]
    string File { get; set; }

    [DataMember(Name = "params")]
    [JsonFormatter(typeof(VerbatimDictionaryKeysBaseFormatter<Dictionary<string, object>, string, object>))]
    Dictionary<string, object> Params { get; set; }

    [DataMember(Name = "source")]
    string Source { get; set; }
}

public partial class RenderSearchTemplateRequest
{
    public string File { get; set; }

    public Dictionary<string, object> Params { get; set; }
    public string Source { get; set; }
}

public partial class RenderSearchTemplateDescriptor
{
    string IRenderSearchTemplateRequest.File { get; set; }

    Dictionary<string, object> IRenderSearchTemplateRequest.Params { get; set; }
    string IRenderSearchTemplateRequest.Source { get; set; }

    public RenderSearchTemplateDescriptor Source(string source) => Assign(source, (a, v) => a.Source = v);

    public RenderSearchTemplateDescriptor File(string file) => Assign(file, (a, v) => a.File = v);

    public RenderSearchTemplateDescriptor Params(Dictionary<string, object> scriptParams) => Assign(scriptParams, (a, v) => a.Params = v);

    public RenderSearchTemplateDescriptor Params(Func<FluentDictionary<string, object>, FluentDictionary<string, object>> paramsSelector) =>
        Assign(paramsSelector, (a, v) => a.Params = v?.Invoke(new FluentDictionary<string, object>()));
}
