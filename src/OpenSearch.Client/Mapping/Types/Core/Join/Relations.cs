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
using OpenSearch.Net.Utf8Json;

namespace OpenSearch.Client;

[JsonFormatter(typeof(VerbatimDictionaryKeysFormatter<Relations, IRelations, RelationName, Children>))]
public interface IRelations : IIsADictionary<RelationName, Children> { }

public class Relations : IsADictionaryBase<RelationName, Children>, IRelations
{
    public Relations() { }

    public Relations(IDictionary<RelationName, Children> container) : base(container) { }

    public Relations(Dictionary<RelationName, Children> container) : base(container) { }

    public void Add(RelationName type, Children children)
    {
        if (BackingDictionary.ContainsKey(type))
            throw new ArgumentException($"{type} is already mapped as parent, you have to map all it's children as a single entry");

        BackingDictionary.Add(type, children);
    }

    public void Add(RelationName type, RelationName child, params RelationName[] moreChildren)
    {
        if (BackingDictionary.ContainsKey(type))
            throw new ArgumentException($"{type} is already mapped as parent, you have to map all it's children as a single entry");

        BackingDictionary.Add(type, new Children(child, moreChildren));
    }
}

public class RelationsDescriptor : IsADictionaryDescriptorBase<RelationsDescriptor, IRelations, RelationName, Children>
{
    public RelationsDescriptor() : base(new Relations()) { }

    internal RelationsDescriptor(IRelations relations) : base(relations) { }

    public RelationsDescriptor Join(RelationName parent, RelationName child, params RelationName[] moreChildren) =>
        Assign(parent, new Children(child, moreChildren));

    public RelationsDescriptor Join<TParent>(RelationName child, params RelationName[] moreChildren)
    {
        if (PromisedValue.ContainsKey(typeof(TParent))) throw new ArgumentException(Message(typeof(TParent)));

        return Assign(typeof(TParent), new Children(child, moreChildren));
    }

    public RelationsDescriptor Join<TParent, TChild>()
    {
        if (PromisedValue.ContainsKey(typeof(TParent))) throw new ArgumentException(Message(typeof(TParent)));

        return Assign(typeof(TParent), typeof(TChild));
    }

    private static string Message(Type t) =>
        $"{t.Name} is already mapped. Use Join<TParent>(typeof(ChildA), typeof(ChildB), ..) to add multiple children in one go";
}
