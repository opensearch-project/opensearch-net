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
using OpenSearch.Net.Utf8Json;

namespace OpenSearch.Client;

/// <summary>
/// A query allowing to define a script to execute as a query
/// </summary>
[ReadAs(typeof(ScriptQuery))]
[InterfaceDataContract]
public interface IScriptQuery : IQuery
{
    /// <summary>
    /// The script to execute
    /// </summary>
    [DataMember(Name = "script")]
    IScript Script { get; set; }
}

/// <inheritdoc cref="IScriptQuery"/>
public class ScriptQuery : QueryBase, IScriptQuery
{
    /// <inheritdoc />
    public IScript Script { get; set; }

    protected override bool Conditionless => IsConditionless(this);

    internal override void InternalWrapInContainer(IQueryContainer c) => c.Script = this;

    internal static bool IsConditionless(IScriptQuery q)
    {
        if (q.Script == null)
            return true;

        switch (q.Script)
        {
            case IInlineScript inlineScript:
                return inlineScript.Source.IsNullOrEmpty();
            case IIndexedScript indexedScript:
                return indexedScript.Id.IsNullOrEmpty();
        }

        return false;
    }
}

/// <inheritdoc cref="IScriptQuery"/>
public class ScriptQueryDescriptor<T>
    : QueryDescriptorBase<ScriptQueryDescriptor<T>, IScriptQuery>
        , IScriptQuery where T : class
{
    protected override bool Conditionless => ScriptQuery.IsConditionless(this);

    IScript IScriptQuery.Script { get; set; }

    /// <inheritdoc cref="IScriptQuery.Script"/>
    public ScriptQueryDescriptor<T> Script(Func<ScriptDescriptor, IScript> selector) =>
        Assign(selector, (a, v) => a.Script = v?.Invoke(new ScriptDescriptor()));
}
