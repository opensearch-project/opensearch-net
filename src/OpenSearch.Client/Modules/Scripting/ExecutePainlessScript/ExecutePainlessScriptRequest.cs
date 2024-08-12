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
using System.Runtime.Serialization;

namespace OpenSearch.Client;

/// <summary> The Painless execute API allows an arbitrary script to be executed and a result to be returned. </summary>
[MapsApi("scripts_painless_execute.json")]
public partial interface IExecutePainlessScriptRequest
{
    /// <summary> The context the script should be executed in </summary>
    [DataMember(Name = "context")]
    string Context { get; set; }

    /// <inheritdoc cref="IPainlessContextSetup" />
    [DataMember(Name = "context_setup")]
    IPainlessContextSetup ContextSetup { get; set; }

    /// <summary> The script to execute </summary>
    [DataMember(Name = "script")]
    IInlineScript Script { get; set; }
}

/// <inheritdoc cref="IExecutePainlessScriptRequest" />
public partial class ExecutePainlessScriptRequest
{
    /// <inheritdoc cref="IExecutePainlessScriptRequest.Context" />
    public string Context { get; set; }

    /// <inheritdoc cref="IExecutePainlessScriptRequest.ContextSetup" />
    public IPainlessContextSetup ContextSetup { get; set; }

    /// <inheritdoc cref="IExecutePainlessScriptRequest.Script" />
    public IInlineScript Script { get; set; }
}

/// <inheritdoc cref="IExecutePainlessScriptRequest" />
public partial class ExecutePainlessScriptDescriptor
{
    string IExecutePainlessScriptRequest.Context { get; set; }
    IPainlessContextSetup IExecutePainlessScriptRequest.ContextSetup { get; set; }
    IInlineScript IExecutePainlessScriptRequest.Script { get; set; }

    /// <inheritdoc cref="IExecutePainlessScriptRequest.Script" />
    public ExecutePainlessScriptDescriptor Script(Func<InlineScriptDescriptor, IInlineScript> selector) =>
        Assign(selector, (a, v) => a.Script = v?.Invoke(new InlineScriptDescriptor()));

    /// <inheritdoc cref="IExecutePainlessScriptRequest.ContextSetup" />
    public ExecutePainlessScriptDescriptor ContextSetup(Func<PainlessContextSetupDescriptor, IPainlessContextSetup> selector) =>
        Assign(selector, (a, v) => a.ContextSetup = v?.Invoke(new PainlessContextSetupDescriptor()));

    /// <inheritdoc cref="IExecutePainlessScriptRequest.Context" />
    public ExecutePainlessScriptDescriptor Context(string context) => Assign(context, (a, v) => a.Context = v);
}
