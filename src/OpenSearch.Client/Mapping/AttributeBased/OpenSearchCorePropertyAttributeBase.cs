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
using System.Reflection;
using System.Runtime.Serialization;

namespace OpenSearch.Client;

[AttributeUsage(AttributeTargets.Property)]
[DataContract]
public abstract class OpenSearchCorePropertyAttributeBase : OpenSearchPropertyAttributeBase, ICoreProperty
{
    protected OpenSearchCorePropertyAttributeBase(FieldType type) : base(type) { }

    /// <inheritdoc cref="ICoreProperty" />
    public string Similarity
    {
        set => Self.Similarity = value;
        get => Self.Similarity;
    }

    /// <inheritdoc cref="ICoreProperty" />
    public bool Store
    {
        get => Self.Store.GetValueOrDefault();
        set => Self.Store = value;
    }

    Fields ICoreProperty.CopyTo { get; set; }

    IProperties ICoreProperty.Fields { get; set; }

    private ICoreProperty Self => this;

    string ICoreProperty.Similarity { get; set; }

    bool? ICoreProperty.Store { get; set; }

    public static new OpenSearchCorePropertyAttributeBase From(MemberInfo memberInfo) =>
        memberInfo.GetCustomAttribute<OpenSearchCorePropertyAttributeBase>(true);
}
