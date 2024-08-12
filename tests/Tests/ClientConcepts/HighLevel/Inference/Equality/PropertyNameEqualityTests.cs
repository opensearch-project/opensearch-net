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

public class PropertyNameEqualityTests
{
    [U]
    public void Eq()
    {
        PropertyName name = "foo";
        PropertyName[] equal = { "foo" };
        foreach (var t in equal)
        {
            (t == name).ShouldBeTrue(t);
            t.Should().Be(name);
        }
    }

    [U]
    public void NotEq()
    {
        PropertyName name = "foo";
        PropertyName[] notEqual = { "bar", "foo  ", "  foo   ", "x", "", "   ", Infer.Property<Project>(p => p.Name) };
        foreach (var t in notEqual)
        {
            (t != name).ShouldBeTrue(t);
            t.Should().NotBe(name);
        }
    }

    [U]
    public void TypedEq()
    {
        PropertyName t1 = Infer.Property<Project>(p => p.Name), t2 = Infer.Property<Project>(p => p.Name);
        (t1 == t2).ShouldBeTrue(t2);
    }

    [U]
    public void TypedNotEq()
    {
        PropertyName t1 = Infer.Property<Developer>(p => p.Id), t2 = Infer.Property<CommitActivity>(p => p.Id);
        (t1 != t2).ShouldBeTrue(t2);
    }

    [U]
    public void Null()
    {
        PropertyName value = "foo";
        (value == null).Should().BeFalse();
        (null == value).Should().BeFalse();
    }
}
