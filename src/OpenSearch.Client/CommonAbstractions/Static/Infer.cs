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
using System.Linq.Expressions;
using System.Reflection;

namespace OpenSearch.Client;

public static class Infer
{
    public static readonly Indices AllIndices = Client.Indices.All;

    public static IndexName Index(IndexName index) => index;

    public static IndexName Index<T>() => typeof(T);

    public static IndexName Index<T>(string clusterName) => IndexName.From<T>(clusterName);

    public static Indices Indices<T>() => typeof(T);

    public static Indices Indices(params IndexName[] indices) => indices;

    public static Indices Indices(IEnumerable<IndexName> indices) => indices.ToArray();

    public static RelationName Relation(string type) => type;

    public static RelationName Relation(Type type) => type;

    public static RelationName Relation<T>() => typeof(T);

    public static Routing Route<T>(T instance) where T : class => Routing.From(instance);

    public static Names Names(params string[] names) => string.Join(",", names);

    public static Names Names(IEnumerable<string> names) => string.Join(",", names);

    public static Id Id<T>(T document) where T : class => Client.Id.From(document);

    public static Fields Fields<T>(params Expression<Func<T, object>>[] fields) where T : class =>
        new Fields(fields.Select(f => new Field(f)));

    public static Fields Fields(params string[] fields) => new Fields(fields.Select(f => new Field(f)));

    public static Fields Fields(params PropertyInfo[] properties) => new Fields(properties.Select(f => new Field(f)));

    /// <summary>
    /// Create a strongly typed string field name representation of the path to a property
    /// <para>e.g. p => p.Array.First().SubProperty.Field will return 'array.subProperty.field'</para>
    /// </summary>
    public static Field Field<T, TValue>(Expression<Func<T, TValue>> path, double? boost = null, string format = null)
        where T : class => new Field(path, boost, format);

    /// <inheritdoc cref="Field{T, TValue}" />
    public static Field Field<T>(Expression<Func<T, object>> path, double? boost = null, string format = null)
        where T : class => new Field(path, boost, format);

    public static Field Field(string field, double? boost = null, string format = null) =>
        new Field(field, boost, format);

    public static Field Field(PropertyInfo property, double? boost = null, string format = null) =>
        new Field(property, boost, format);

    public static PropertyName Property(string property) => property;

    public static PropertyName Property<T, TValue>(Expression<Func<T, TValue>> path) where T : class => path;

    public static PropertyName Property<T>(Expression<Func<T, object>> path) where T : class => path;
}
