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

using System.Collections.Generic;
using System.Runtime.Serialization;
using OpenSearch.Net.Utf8Json;

namespace OpenSearch.Client;

[JsonFormatter(typeof(ResolvableDictionaryResponseFormatter<GetMappingResponse, IndexName, IndexMappings>))]
public class GetMappingResponse : DictionaryResponseBase<IndexName, IndexMappings>
{
    [IgnoreDataMember]
    public IReadOnlyDictionary<IndexName, IndexMappings> Indices => Self.BackingDictionary;

    public void Accept(IMappingVisitor visitor)
    {
        var walker = new MappingWalker(visitor);
        walker.Accept(this);
    }
}

public class IndexMappings
{
    [DataMember(Name = "mappings")]
    public TypeMapping Mappings { get; internal set; }
}


public static class GetMappingResponseExtensions
{
    public static ITypeMapping GetMappingFor<T>(this GetMappingResponse response) => response.GetMappingFor(typeof(T));

    public static ITypeMapping GetMappingFor(this GetMappingResponse response, IndexName index)
    {
        if (index.IsNullOrEmpty()) return null;

        return response.Indices.TryGetValue(index, out var indexMappings) ? indexMappings.Mappings : null;
    }
}
