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
using System.Linq.Expressions;
using System.Runtime.Serialization;
using OpenSearch.Net;
using OpenSearch.Net.Utf8Json;
using static OpenSearch.Client.Infer;

namespace OpenSearch.Client;

[JsonFormatter(typeof(ResolvableDictionaryResponseFormatter<GetFieldMappingResponse, IndexName, TypeFieldMappings>))]
public class GetFieldMappingResponse : DictionaryResponseBase<IndexName, TypeFieldMappings>
{
    [IgnoreDataMember]
    public IReadOnlyDictionary<IndexName, TypeFieldMappings> Indices => Self.BackingDictionary;

    //if you call get mapping on an existing type and index but no fields match you still get back a 200.
    public override bool IsValid => base.IsValid && Indices.HasAny();

    public IFieldMapping GetMapping(IndexName index, Field property)
    {
        if (property == null) return null;

        var mappings = MappingsFor(index);
        if (mappings == null) return null;

        if (!mappings.TryGetValue(property, out var fieldMapping) || fieldMapping.Mapping == null) return null;

        return fieldMapping.Mapping.TryGetValue(property, out var field) ? field : null;
    }

    public IFieldMapping MappingFor<T>(Field property) => MappingFor<T>(property, null);

    public IFieldMapping MappingFor<T>(Field property, IndexName index) =>
        GetMapping(index ?? Index<T>(), property);

    public IFieldMapping MappingFor<T, TValue>(Expression<Func<T, TValue>> objectPath, IndexName index = null)
        where T : class =>
        GetMapping(index ?? Index<T>(), Field(objectPath));

    public IFieldMapping MappingFor<T>(Expression<Func<T, object>> objectPath, IndexName index = null)
        where T : class =>
        GetMapping(index ?? Index<T>(), Field(objectPath));


    private IReadOnlyDictionary<Field, FieldMapping> MappingsFor(IndexName index)
    {
        if (!Indices.TryGetValue(index, out var indexMapping) || indexMapping.Mappings == null) return null;

        return indexMapping.Mappings;
    }
}

public class TypeFieldMappings
{
    [DataMember(Name = "mappings")]
    [JsonFormatter(typeof(ResolvableReadOnlyDictionaryFormatter<Field, FieldMapping>))]
    public IReadOnlyDictionary<Field, FieldMapping> Mappings { get; internal set; } = EmptyReadOnly<Field, FieldMapping>.Dictionary;
}

public class FieldMapping
{
    [DataMember(Name = "full_name")]
    public string FullName { get; internal set; }

    [DataMember(Name = "mapping")]
    [JsonFormatter(typeof(FieldMappingFormatter))]
    public IReadOnlyDictionary<Field, IFieldMapping> Mapping { get; internal set; } = EmptyReadOnly<Field, IFieldMapping>.Dictionary;
}
