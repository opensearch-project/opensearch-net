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

public class IndexNameEqualityTests
{
    [U]
    public void Eq()
    {
        IndexName types = "foo";
        IndexName[] equal = { "foo" };
        foreach (var t in equal)
        {
            (t == types).ShouldBeTrue(t);
            t.Should().Be(types);
        }
    }

    [U]
    public void NotEq()
    {
        IndexName types = "foo";
        IndexName[] notEqual = { "bar", "foo  ", "  foo   ", "x", "", "   ", typeof(Project) };
        foreach (var t in notEqual)
        {
            (t != types).ShouldBeTrue(t);
            t.Should().NotBe(types);
        }
    }

    [U]
    public void EqCluster()
    {
        IndexName types = "c:foo";
        IndexName[] equal = { "c:foo" };
        foreach (var t in equal)
        {
            (t == types).ShouldBeTrue(t);
            t.Should().Be(types);
        }
    }

    [U]
    public void NotEqCluster()
    {
        IndexName types = "c:foo";
        IndexName[] notEqual = { "c1:bar", "c:foo  ", "  c:foo   ", "c:foo1", "x", "", "   ", typeof(Project) };
        foreach (var t in notEqual)
        {
            (t != types).ShouldBeTrue(t);
            t.Should().NotBe(types);
        }
    }

    [U]
    public void IndexdEq()
    {
        IndexName t1 = typeof(Project), t2 = typeof(Project);
        (t1 == t2).ShouldBeTrue(t2);
    }

    [U]
    public void IndexdNotEq()
    {
        IndexName t1 = typeof(Project), t2 = typeof(CommitActivity);
        (t1 != t2).ShouldBeTrue(t2);
    }

    [U]
    public void Null()
    {
        IndexName value = typeof(Project);
        (value == null).Should().BeFalse();
        (null == value).Should().BeFalse();
    }
}
