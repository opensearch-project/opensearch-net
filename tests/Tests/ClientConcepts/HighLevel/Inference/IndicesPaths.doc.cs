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
using OpenSearch.Net;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Core.Client;
using Tests.Domain;
using Tests.Framework;
using Tests.Framework.DocumentationTests;
using static OpenSearch.Client.Indices;

namespace Tests.ClientConcepts.HighLevel.Inference;

public class IndicesPaths : DocumentationTestBase
{
    /**[[indices-paths]]
		* === Indices paths
		*
		* Some APIs in OpenSearch take an index name, a collection of index names,
		* or the special `_all` marker (used to specify all indices), in the URI path of the request, to specify the indices that
		* the request should execute against.
		*
		* In OSC, these index names can be specified using the `Indices` type.
		*
		* ==== Implicit Conversion
		*
		* To make working with `Indices` easier, several types implicitly convert to it:
		*
		* - `string`
		* - comma separated `string`
		* - `string` array
		* - a CLR type, <<index-name-inference, where a default index name or index name for the type has been specified on `ConnectionSettings`>>
		* - `IndexName`
		* - `IndexName` array
		*
		* Here are some examples of how implicit conversions can be used to specify index names
		*/
    [U]
    public void ImplicitConversions()
    {
        OpenSearch.Client.Indices singleIndexFromString = "name";
        OpenSearch.Client.Indices multipleIndicesFromString = "name1, name2";
        OpenSearch.Client.Indices multipleIndicesFromStringArray = new[] { "name1", "name2" };
        OpenSearch.Client.Indices allFromString = "_all";

        OpenSearch.Client.Indices allWithOthersFromString = "_all, name2"; //<1> `_all` will override any specific index names here

        OpenSearch.Client.Indices singleIndexFromType = typeof(Project); //<2> The `Project` type has been mapped to a specific index name using <<index-name-type-mapping,`.DefaultMappingFor<Project>`>>

        OpenSearch.Client.Indices singleIndexFromIndexName = IndexName.From<Project>();

        singleIndexFromString.Match(
            all => all.Should().BeNull(),
            many => many.Indices.Should().HaveCount(1).And.Contain("name")
        );

        multipleIndicesFromString.Match(
            all => all.Should().BeNull(),
            many => many.Indices.Should().HaveCount(2).And.Contain("name2")
        );

        allFromString.Match(
            all => all.Should().NotBeNull(),
            many => many.Indices.Should().BeNull()
        );

        allWithOthersFromString.Match(
            all => all.Should().NotBeNull(),
            many => many.Indices.Should().BeNull()
        );

        multipleIndicesFromStringArray.Match(
            all => all.Should().BeNull(),
            many => many.Indices.Should().HaveCount(2).And.Contain("name2")
        );

        singleIndexFromType.Match(
            all => all.Should().BeNull(),
            many => many.Indices.Should().HaveCount(1).And.Contain(typeof(Project))
        );

        singleIndexFromIndexName.Match(
            all => all.Should().BeNull(),
            many => many.Indices.Should().HaveCount(1).And.Contain(typeof(Project))
        );
    }

    /**
		* [[osc-indices]]
		*==== Using OpenSearch.Client.Indices methods
		* To make creating `IndexName` or `Indices` instances easier, `OpenSearch.Client.Indices` also contains several static methods
		* that can be used to construct them.
		*
		*===== Single index
		*
		* A single index can be specified using a CLR type or a string, and the `.Index()` method.
		*
		* [TIP]
		* ====
		* This example uses the static import `using static OpenSearch.Client.Indices;` in the using directives to shorthand `OpenSearch.Client.Indices.Index()`
		* to simply `Index()`. Be sure to include this static import if copying any of these examples.
		* ====
		*/
    [U]
    public void UsingStaticPropertyField()
    {

        var client = TestClient.Default;

        var singleString = Index("name1"); // <1> specifying a single index using a string
        var singleTyped = Index<Project>(); //<2> specifying a single index using a type

        ISearchRequest singleStringRequest = new SearchDescriptor<Project>().Index(singleString);
        ISearchRequest singleTypedRequest = new SearchDescriptor<Project>().Index(singleTyped);

        ((IUrlParameter)singleStringRequest.Index).GetString(Client.ConnectionSettings).Should().Be("name1");
        ((IUrlParameter)singleTypedRequest.Index).GetString(Client.ConnectionSettings).Should().Be("project");

        var invalidSingleString = Index("name1, name2"); //<3> an **invalid** single index name
    }

    /**===== Multiple indices
		*
		* Similarly to a single index, multiple indices can be specified using multiple CLR types or multiple strings
		*/
    [U]
    public void MultipleIndices()
    {
        var manyStrings = Index("name1", "name2"); //<1> specifying multiple indices using strings
        var manyTypes = Index<Project>().And<Developer>(); //<2> specifying multiple indices using types
        var client = TestClient.Default;

        ISearchRequest manyStringRequest = new SearchDescriptor<Project>().Index(manyStrings);
        ISearchRequest manyTypedRequest = new SearchDescriptor<Project>().Index(manyTypes);

        ((IUrlParameter)manyStringRequest.Index).GetString(Client.ConnectionSettings).Should().Be("name1,name2");
        ((IUrlParameter)manyTypedRequest.Index).GetString(Client.ConnectionSettings).Should().Be("project,devs"); // <3> The index names here come from the Connection Settings passed to `TestClient`. See the documentation on <<index-name-inference, Index Name Inference>> for more details.

        manyStringRequest = new SearchDescriptor<Project>().Index(new[] { "name1", "name2" });
        ((IUrlParameter)manyStringRequest.Index).GetString(Client.ConnectionSettings).Should().Be("name1,name2");
    }

    /**===== All Indices
		*
		* OpenSearch allows searching across multiple indices using the special `_all` marker.
		*
		* OSC exposes the `_all` marker with `Indices.All` and `Indices.AllIndices`. Why expose it in two ways, you ask?
		* Well, you may be using both `OpenSearch.Client.Indices` and `OpenSearch.Client.Types` in the same file and you may also be using C#6
		* static imports too; in this scenario, the `All` property becomes ambiguous between `Indices.All` and `Types.All`, so the
		* `_all` marker for indices is exposed as `Indices.AllIndices`, to alleviate this ambiguity
		*/
    [U]
    public void IndicesAllAndAllIndicesSpecifiedWhenUsingStaticUsingDirective()
    {
        var indicesAll = All;
        var allIndices = AllIndices;

        ISearchRequest indicesAllRequest = new SearchDescriptor<Project>().Index(indicesAll);
        ISearchRequest allIndicesRequest = new SearchDescriptor<Project>().Index(allIndices);

        ((IUrlParameter)indicesAllRequest.Index).GetString(Client.ConnectionSettings).Should().Be("_all");
        ((IUrlParameter)allIndicesRequest.Index).GetString(Client.ConnectionSettings).Should().Be("_all");
    }
}
