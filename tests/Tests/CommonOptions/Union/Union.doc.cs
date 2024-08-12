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

using System.Runtime.Serialization;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.Net;
using Tests.Core.Client;
using Tests.Framework;

namespace Tests.CommonOptions.Union;

/**[[union]]
	 * === Union type
	 * Some API parameters within OpenSearch can accept more than one JSON data structure, for example, source filtering on
	 * a search request can accept
	 *
	 * - a `bool` value to disable `_source` retrieval
	 * - a `string` value representing a wildcard pattern to control what parts of `_source` to return
	 * - an array of `string` values representing multiple wildcard patterns to control what parts of `_source` to return
	 * - an `object` with `includes` and `excludes` properties that each accept an array of wildcard patterns to control
	 * what parts of `_source` to include and exclude, respectively.
	 *
	 * That's a lot of different flexibility! OSC includes a `Union<TFirst,TSecond>` type to make it easier to work with
	 * these kinds of parameters, forming the union of two types, `TFirst` and `TSecond`.
	 */
public class Union
{
    private IOpenSearchSerializer serializer = TestClient.DefaultInMemoryClient.RequestResponseSerializer;

    /**
		 * ==== Implicit conversion
		 *
		 * The `Union<TFirst,TSecond>` has implicit operators to convert from an instance of `TFirst` or `TSecond` to an
		 * instance of `Union<TFirst,TSecond>`. This is often the easiest way of construction an instance
		 */
    public void ImplicitConversion()
    {
        Union<bool, ISourceFilter> sourceFilterFalse = false;

        Union<bool, ISourceFilter> sourceFilterInterface = new SourceFilter
        {
            Includes = new[] { "foo.*" }
        };

        // hide
        sourceFilterFalse.Match(
            b => b.Should().BeFalse(),
            s => s.Should().BeNull());

        // hide
        sourceFilterInterface.Match(
            b => b.Should().BeFalse(),
            s => s.Should().NotBeNull());
    }

    /**
		 * ==== Constructor
		 *
		 * Sometimes, the constructor of `Union<TFirst,TSecond>` may be required in cases where the compiler is
		 * unable to infer the correct implicit conversion
		 */
    public void Constructor()
    {
        var sourceFilterTrue = new Union<bool, ISourceFilter>(true);

        var sourceFilterInterface = new Union<bool, ISourceFilter>(new SourceFilter
        {
            Includes = new[] { "foo.*" }
        });

        /**
			 * ==== Match
			 *
			 * The `Match` method can be used to operate on the value encapsulated by the instance of `Union<TFirst,TSecond>`.
			 * Two delegates are passed; one to operate on a `TFirst` value and the other to operate on a `TSecond` value.
			 */
        sourceFilterTrue.Match(
            b => b.Should().BeTrue(),
            s => s.Should().BeNull());

        sourceFilterInterface.Match(
            b => b.Should().BeFalse(),
            s => s.Should().NotBeNull());

        /**
			 * The delegates can also return a value
			 */
        var serializedFilterTrue = sourceFilterTrue.Match(
            b => serializer.SerializeToString(b),
            s => null);

        serializedFilterTrue.Should().Be("true");

        var serializedFilterInterface = sourceFilterTrue.Match(
            b => null,
            s => serializer.SerializeToString(s));

        serializedFilterInterface.Should().Be("{\"includes\":[\"foo.*\"]}");
    }
}
