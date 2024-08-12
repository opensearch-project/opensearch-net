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

using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Core.Extensions;
using Tests.Domain;

namespace Tests.ClientConcepts.HighLevel.Inference.Equality;

public class FieldEqualityTests
{
    [U]
    public void Eq()
    {
        Field name = "foo";
        Field[] equal = { "foo" };
        foreach (var t in equal)
        {
            (t == name).ShouldBeTrue(t);
            t.Should().Be(name);
        }
    }

    [U]
    public void NotEq()
    {
        Field name = "foo";
        Field[] notEqual = { "bar", "x", "", "   ", " foo ", Infer.Field<Project>(p => p.Name) };
        foreach (var t in notEqual)
        {
            (t != name).ShouldBeTrue(t);
            t.Should().NotBe(name);
        }
    }

    [U]
    public void TypedEq()
    {
        Field t1 = Infer.Field<Project>(p => p.Name), t2 = Infer.Field<Project>(p => p.Name);
        (t1 == t2).ShouldBeTrue(t2);
        t1.Should().Be(t2);
    }

    [U]
    public void TypedNotEq()
    {
        Field t1 = Infer.Field<Developer>(p => p.Id), t2 = Infer.Field<CommitActivity>(p => p.Id);
        (t1 != t2).ShouldBeTrue(t2);
        t1.Should().NotBe(t2);
    }

    [U]
    public void ReflectedEq()
    {
        Field t1 = typeof(Project).GetProperty(nameof(Project.Name)),
            t2 = typeof(Project).GetProperty(nameof(Project.Name));
        (t1 == t2).ShouldBeTrue(t2);
        t1.Should().Be(t2);
    }

    [U]
    public void ReflectedNotEq()
    {
        Field t1 = typeof(CommitActivity).GetProperty(nameof(CommitActivity.Id)),
            t2 = typeof(Developer).GetProperty(nameof(Developer.Id));
        (t1 != t2).ShouldBeTrue(t2);
        t1.Should().NotBe(t2);
    }

    [U]
    public void Null()
    {
        Field value = "foo";
        (value == null).Should().BeFalse();
        (null == value).Should().BeFalse();
    }
}
