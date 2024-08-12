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
using System.Collections.Concurrent;
using System.Linq;

namespace OpenSearch.Client;

/// <summary>
/// Applied to a CLR type to override the name of a CLR type and the property from which an _id is inferred
/// when serializing an instance of the type.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class OpenSearchTypeAttribute : Attribute
{
    private static readonly ConcurrentDictionary<Type, OpenSearchTypeAttribute> CachedTypeLookups =
        new ConcurrentDictionary<Type, OpenSearchTypeAttribute>();

    /// <summary>
    /// The property on CLR type to use as the _id of the document
    /// </summary>
    public string IdProperty { get; set; }

    /// <summary>
    /// The name of the CLR type for serialization
    /// </summary>
    public string RelationName { get; set; }

    /// <summary>
    /// Gets the first <see cref="OpenSearchTypeAttribute"/> from a given CLR type
    /// </summary>
    public static OpenSearchTypeAttribute From(Type type)
    {
        if (CachedTypeLookups.TryGetValue(type, out var attr))
            return attr;

        var attributes = type.GetCustomAttributes(typeof(OpenSearchTypeAttribute), true);
        if (attributes.HasAny())
            attr = (OpenSearchTypeAttribute)attributes.First();
        CachedTypeLookups.TryAdd(type, attr);
        return attr;
    }
}
