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

using System.Linq;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Domain;
using Tests.Framework.EndpointTests.TestState;
using Tests.QueryDsl.BoolDsl;

namespace Tests.QueryDsl.Compound.Bool;

public class BoolDslComplexQueryUsageTests : BoolQueryUsageTests
{
    protected static readonly TermQuery NullQuery = null;
    protected static readonly TermQuery Query = new TermQuery { Field = "x", Value = "y" };

    public BoolDslComplexQueryUsageTests(ReadOnlyCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

    protected override QueryContainer QueryInitializer =>
        //first bool
        Query && Query
        //second bool
        || (+Query || +Query || !Query && (!Query && !ConditionlessQuery))
        // simple nested or
        && (Query || Query || Query)
        //all conditionless bool
        && (NullQuery || +ConditionlessQuery || !ConditionlessQuery)
        // actual bool query
        && base.QueryInitializer;

    protected override object QueryJson => new
    {
        @bool = new
        {
            should = new object[]
            {
					//first bool
					new
                {
                    @bool = new
                    {
                        must = new object[]
                        {
                            new { term = new { x = new { value = "y" } } },
                            new { term = new { x = new { value = "y" } } }
                        }
                    }
                },
                new
                {
                    @bool = new
                    {
							// must be typed to object[] for documentation
							must = new object[]
                        {
                            new
                            {
                                @bool = new
                                {
										//complex nested bool
										should = new object[]
                                    {
                                        new
                                        {
                                            @bool = new
                                            {
                                                filter = new object[] { new { term = new { x = new { value = "y" } } } }
                                            }
                                        },
                                        new
                                        {
                                            @bool = new
                                            {
                                                filter = new object[] { new { term = new { x = new { value = "y" } } } }
                                            }
                                        },
                                        new
                                        {
                                            @bool = new
                                            {
                                                must_not = new object[]
                                                {
                                                    new { term = new { x = new { value = "y" } } },
                                                    new { term = new { x = new { value = "y" } } }
                                                }
                                            }
                                        }
                                    }
                                }
                            },
								//simple nested or
								new
                            {
                                @bool = new
                                {
                                    should = new object[]
                                    {
                                        new { term = new { x = new { value = "y" } } },
                                        new { term = new { x = new { value = "y" } } },
                                        new { term = new { x = new { value = "y" } } }
                                    }
                                }
                            },
								//actual (locked) locked query
								base.QueryJson,
                        }
                    }
                }
            }
        }
    };

    protected override QueryContainer QueryFluent(QueryContainerDescriptor<Project> q) =>
        //first bool
        q.Query() && q.Query()
        //second bool
        || (+q.Query() || +q.Query() || !q.Query() && (!q.Query() && !q.ConditionlessQuery()))
        // simple nested or
        && (q.Query() || q.Query() || q.Query())
        //all conditionless bool
        && (q.NullQuery() || +q.ConditionlessQuery() || !q.ConditionlessQuery())
        // actual bool query
        && base.QueryFluent(q);

    //hide
    [U]
    protected void AsssertShape() => AssertShape(QueryInitializer);

    //hide
    private void AssertShape(IQueryContainer container)
    {
        //top level bool
        container.Bool.Should().NotBeNull();
        container.Bool.Should.Should().HaveCount(2);
        container.Bool.MustNot.Should().BeNull();
        container.Bool.Filter.Should().BeNull();
        container.Bool.Must.Should().BeNull();

        //first bool
        var firstBool = (container.Bool.Should.First() as IQueryContainer)?.Bool;
        firstBool.Should().NotBeNull();
        firstBool.Must.Should().HaveCount(2);
        firstBool.MustNot.Should().BeNull();
        firstBool.Filter.Should().BeNull();
        firstBool.Should.Should().BeNull();

        //second bool
        var secondBool = (container.Bool.Should.Last() as IQueryContainer)?.Bool;
        secondBool.Should().NotBeNull();
        secondBool.Must.Should().HaveCount(3); //the last bool query was all conditionless
        secondBool.MustNot.Should().BeNull();
        secondBool.Filter.Should().BeNull();
        secondBool.Should.Should().BeNull();

        //complex nested bool
        var complexNestedBool = (secondBool.Must.First() as IQueryContainer)?.Bool;
        complexNestedBool.Should().NotBeNull();
        complexNestedBool.Should.Should().HaveCount(3);

        //inner must nots
        var mustNotsBool = complexNestedBool.Should.Cast<IQueryContainer>().FirstOrDefault(q => q.Bool != null && q.Bool.MustNot != null)?.Bool;
        mustNotsBool.Should().NotBeNull();
        mustNotsBool.MustNot.Should().HaveCount(2); //one of the three must nots was conditionless
    }
}
