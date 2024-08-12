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
using System.Linq.Expressions;
using System.Reflection;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Domain;

namespace Tests.ClientConcepts.HighLevel.Inference;

public class ImplicitConversionTests
{
    private static T Implicit<T>(T i) => i;

    [U]
    public void Ids()
    {
        Implicit<Ids>(null).Should().BeNull();
        Implicit<Ids>("").Should().BeNull();
        Implicit<Ids>("   ").Should().BeNull();
        Implicit<Ids>(",, ,,").Should().BeNull();
        Implicit<Ids>(new string[] { }).Should().BeNull();
        Implicit<Ids>(new string[] { null, null }).Should().BeNull();
    }

    [U] public void LongId() => Implicit<LongId>(null).Should().BeNull();

    [U]
    public void DocumentPath()
    {
        Implicit<DocumentPath<Project>>((Project)null).Should().BeNull();
        Implicit<DocumentPath<Project>>((Id)null).Should().BeNull();
        Implicit<DocumentPath<Project>>((string)null).Should().BeNull();
        Implicit<DocumentPath<Project>>("").Should().BeNull();
        Implicit<DocumentPath<Project>>("   ").Should().BeNull();
    }

    [U]
    public void Field()
    {
        Implicit<Field>((string)null).Should().BeNull();
        Implicit<Field>((Expression)null).Should().BeNull();
        Implicit<Field>((PropertyInfo)null).Should().BeNull();
        Implicit<Field>("").Should().BeNull();
        Implicit<Field>("   ").Should().BeNull();
    }

    [U]
    public void Fields()
    {
        Implicit<Fields>((Expression)null).Should().BeNull();
        Implicit<Fields>((Field)null).Should().BeNull();
        Implicit<Fields>((string)null).Should().BeNull();
        Implicit<Fields>((PropertyInfo)null).Should().BeNull();
        Implicit<Fields>((string[])null).Should().BeNull();
        Implicit<Fields>((Expression[])null).Should().BeNull();
        Implicit<Fields>((PropertyInfo[])null).Should().BeNull();
        Implicit<Fields>((Field[])null).Should().BeNull();
        Implicit<Fields>("").Should().BeNull();
        Implicit<Fields>("   ").Should().BeNull();
        Implicit<Fields>(new string[] { }).Should().BeNull();
        Implicit<Fields>(new Expression[] { }).Should().BeNull();
        Implicit<Fields>(new PropertyInfo[] { }).Should().BeNull();
        Implicit<Fields>(new Field[] { }).Should().BeNull();
        Implicit<Fields>(new Expression[] { null, null }).Should().BeNull();
        Implicit<Fields>(new PropertyInfo[] { null, null }).Should().BeNull();
        Implicit<Fields>(new Field[] { null, null }).Should().BeNull();
    }

    [U]
    public void Id()
    {
        Implicit<Id>(null).Should().BeNull();
        Implicit<Id>("").Should().BeNull();
        Implicit<Id>("   ").Should().BeNull();
    }

    [U]
    public void IndexName()
    {
        Implicit<IndexName>((string)null).Should().BeNull();
        Implicit<IndexName>((Type)null).Should().BeNull();
        Implicit<IndexName>("").Should().BeNull();
        Implicit<IndexName>("   ").Should().BeNull();
    }

    [U]
    public void Indices()
    {
        Implicit<OpenSearch.Client.Indices>((string)null).Should().BeNull();
        Implicit<OpenSearch.Client.Indices>((OpenSearch.Client.Indices.ManyIndices)null).Should().BeNull();
        Implicit<OpenSearch.Client.Indices>((string[])null).Should().BeNull();
        Implicit<OpenSearch.Client.Indices>((IndexName)null).Should().BeNull();
        Implicit<OpenSearch.Client.Indices>((IndexName[])null).Should().BeNull();
        Implicit<OpenSearch.Client.Indices>((IndexName)null).Should().BeNull();
        Implicit<OpenSearch.Client.Indices>("").Should().BeNull();
        Implicit<OpenSearch.Client.Indices>("    ").Should().BeNull();
        Implicit<OpenSearch.Client.Indices>(",, ,,    ").Should().BeNull();
        Implicit<OpenSearch.Client.Indices>(new string[] { }).Should().BeNull();
        Implicit<OpenSearch.Client.Indices>(new IndexName[] { }).Should().BeNull();
        Implicit<OpenSearch.Client.Indices>(new string[] { null, null }).Should().BeNull();
        Implicit<OpenSearch.Client.Indices>(new IndexName[] { null, null }).Should().BeNull();
    }

    [U]
    public void Names()
    {
        Implicit<Names>((string)null).Should().BeNull();
        Implicit<Names>((string[])null).Should().BeNull();
        Implicit<Names>("").Should().BeNull();
        Implicit<Names>(",,").Should().BeNull();
        Implicit<Names>(",   ,").Should().BeNull();
        Implicit<Names>("   ").Should().BeNull();
        Implicit<Names>(new string[] { }).Should().BeNull();
        Implicit<Names>(new string[] { null, null }).Should().BeNull();
    }

    [U]
    public void Routing()
    {
        Implicit<Routing>((string)null).Should().BeNull();
        Implicit<Routing>((string[])null).Should().BeNull();
        Implicit<Routing>("").Should().BeNull();
        Implicit<Routing>(",,").Should().BeNull();
        Implicit<Routing>(",   ,").Should().BeNull();
        Implicit<Routing>("   ").Should().BeNull();
        Implicit<Routing>(new string[] { }).Should().BeNull();
        Implicit<Routing>(new string[] { null, null }).Should().BeNull();
    }

    [U] public void Metrics() => Implicit<Metrics>(null).Should().BeNull();

    [U] public void IndexMetrics() => Implicit<IndexMetrics>(null).Should().BeNull();

    [U]
    public void Name()
    {
        Implicit<Name>(null).Should().BeNull();
        Implicit<Name>("").Should().BeNull();
        Implicit<Name>("   ").Should().BeNull();
    }

    [U]
    public void NodeId()
    {
        Implicit<NodeIds>((string)null).Should().BeNull();
        Implicit<NodeIds>((string[])null).Should().BeNull();
        Implicit<NodeIds>("").Should().BeNull();
        Implicit<NodeIds>("  ").Should().BeNull();
        Implicit<NodeIds>("  ,, , ,,").Should().BeNull();
        Implicit<NodeIds>(new string[] { }).Should().BeNull();
        Implicit<NodeIds>(new string[] { null, null }).Should().BeNull();
    }

    [U]
    public void PropertyName()
    {
        Implicit<PropertyName>((Expression)null).Should().BeNull();
        Implicit<PropertyName>((PropertyInfo)null).Should().BeNull();
        Implicit<PropertyName>((string)null).Should().BeNull();
        Implicit<PropertyName>("").Should().BeNull();
        Implicit<PropertyName>("  ").Should().BeNull();
    }

    [U]
    public void RelationName()
    {
        Implicit<RelationName>((string)null).Should().BeNull();
        Implicit<RelationName>((Type)null).Should().BeNull();
        Implicit<RelationName>("   ").Should().BeNull();
    }

    [U]
    public void TaskId()
    {
        Implicit<TaskId>((string)null).Should().BeNull();
        Implicit<TaskId>("    ").Should().BeNull();
        Implicit<TaskId>("").Should().BeNull();
    }
}
