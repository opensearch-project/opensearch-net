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
using System.Runtime.Serialization;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Domain;
using Tests.Framework;
using Tests.Framework.DocumentationTests;
using Xunit;
using static OpenSearch.Client.Infer;
using static Tests.Core.Serialization.SerializationTestHelper;

namespace Tests.ClientConcepts.HighLevel.Inference;

/**[[types-and-relations-inference]]
	*=== Relation names inference
	*
	*/
public class TypesAndRelationsInference : DocumentationTestBase
{
    /**
		* [[relation-names]]
		* [float]
		* === Relation names
		*
		* The general guideline has always been to use a single type per index. This is also enforced.
		* Some features still need to store multiple types in a single index such as Parent/Child join relations.
		*
		* Both `Parent` and `Child` will need to have resolve to the same typename to be indexed into the same index.
		*
		* Therefore we need a different type that translates a CLR type to a join relation. This can be configured seperately
		* using `.RelationName()`
		*/
    [U]
    public void RelationNameConfiguration()
    {
        var settings = new ConnectionSettings()
            .DefaultMappingFor<CommitActivity>(m => m
                .IndexName("projects-and-commits")
                .RelationName("commits")
            )
            .DefaultMappingFor<Project>(m => m
                .IndexName("projects-and-commits")
                .RelationName("projects")
            );

        var resolver = new RelationNameResolver(settings);
        var relation = resolver.Resolve<Project>();
        relation.Should().Be("projects");

        relation = resolver.Resolve<CommitActivity>();
        relation.Should().Be("commits");
    }

    /**
		* `RelationName` uses the `DefaultTypeNameInferrer` to translate CLR types to a string representation.
		*
		* Explicit `TypeName` configuration does not affect how the default relation for the CLR type
		* is represented though
		*/
    [U]
    public void TypeNameExplicitConfigurationDoesNotAffectRelationName()
    {
        var settings = new ConnectionSettings()
            .DefaultMappingFor<Project>(m => m
                .IndexName("projects-and-commits")
            );

        var resolver = new RelationNameResolver(settings);
        var relation = resolver.Resolve<Project>();
        relation.Should().Be("project");
    }

    //hide
    [U]
    public void RoundTripSerializationPreservesCluster() => Expect("project").WhenSerializing(Relation<Project>());

    //hide
    [U]
    public void EqualsValidation()
    {
        var relation = (RelationName)"projects";
        var relationOther = (RelationName)"p";

        relation.Should().NotBe(relationOther);
        relation.Should().Be("projects");
        relation.Should().Be((RelationName)"projects");

        Relation<Project>().Should().Be(Relation<Project>());
        Relation<Project>().Should().NotBe(Relation<Developer>());
    }

    //hide
    [U]
    public void GetHashCodeValidation()
    {
        var relation = (RelationName)"projects";

        relation.GetHashCode().Should().NotBe(0);

        Relation<Project>().GetHashCode().Should().Be(Relation<Project>().GetHashCode()).And.NotBe(0);
        Relation<Project>().GetHashCode().Should().NotBe(Relation<Developer>().GetHashCode()).And.NotBe(0);

    }
}
