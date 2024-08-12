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

namespace OpenSearch.Client;

public interface IDocumentPath
{
    Id Id { get; set; }
    IndexName Index { get; set; }
}

public class DocumentPath<T> : IEquatable<DocumentPath<T>>, IDocumentPath where T : class
{
    public DocumentPath(T document) : this(Client.Id.From(document)) => Document = document;

    public DocumentPath(Id id)
    {
        Self.Id = id;
        Self.Index = typeof(T);
    }

    internal T Document { get; set; }
    internal IDocumentPath Self => this;
    Id IDocumentPath.Id { get; set; }
    IndexName IDocumentPath.Index { get; set; }

    public bool Equals(DocumentPath<T> other)
    {
        IDocumentPath o = other, s = Self;
        return s.Index.NullOrEquals(o.Index) && s.Id.NullOrEquals(o.Id) && (Document?.Equals(other.Document) ?? true);
    }

    public static DocumentPath<T> Id(Id id) => new DocumentPath<T>(id);

    public static DocumentPath<T> Id(T @object) => new DocumentPath<T>(@object);

    public static implicit operator DocumentPath<T>(T @object) => @object == null ? null : new DocumentPath<T>(@object);

    public static implicit operator DocumentPath<T>(Id id) => id == null ? null : new DocumentPath<T>(id);

    public static implicit operator DocumentPath<T>(long id) => new DocumentPath<T>(id);

    public static implicit operator DocumentPath<T>(string id) => id.IsNullOrEmpty() ? null : new DocumentPath<T>(id);

    public static implicit operator DocumentPath<T>(Guid id) => new DocumentPath<T>(id);

    public DocumentPath<T> Index(IndexName index)
    {
        if (index == null) return this;

        Self.Index = index;
        return this;
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = Self.Index?.GetHashCode() ?? 0;
            hashCode = (hashCode * 397) ^ (Self.Id?.GetHashCode() ?? 0);
            return hashCode;
        }
    }

    public override bool Equals(object obj)
    {
        switch (obj)
        {
            case DocumentPath<T> d: return Equals(d);
            default: return false;
        }
    }

    public static bool operator ==(DocumentPath<T> x, DocumentPath<T> y) => Equals(x, y);

    public static bool operator !=(DocumentPath<T> x, DocumentPath<T> y) => !Equals(x, y);
}
