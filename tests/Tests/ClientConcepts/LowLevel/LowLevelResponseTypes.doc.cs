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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.Net;
using OpenSearch.Net.Extensions;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Domain.Extensions;
using Tests.Framework;

// ReSharper disable SuggestVarOrType_Elsewhere
// ReSharper disable SuggestVarOrType_BuiltInTypes
// ReSharper disable SuggestVarOrType_SimpleTypes

namespace Tests.ClientConcepts.LowLevel;

public class LowLevelResponseTypes
{
    /**[[low-level-response-types]]
		 * === Low Level Client Response Types
		 *
		 */

    public static string Response() => @"{
				""boolean"" : true,
				""string"" : ""v"",
				""number"" : 29,
				""array"" : [1, 2, 3, 4],
				""object"" : {
					""first"" : ""value1"",
					""second"" : ""value2"",
					""nested"" : { ""x"" : ""value3"" }
				},
				""array_of_objects"" : [
					{
						""first"" : ""value11"",
						""second"" : ""value12"",
						""nested"" : { ""x"" : ""value4"", ""z"" : [{""id"": 1}] }
					},
					{
						""first"" : ""value21"",
						""second"" : ""value22"",
						""nested"" : { ""x"" : ""value5"", ""z"" : [{""id"": 3}, {""id"": 2}] },
						""complex.nested"" : { ""x"" : ""value6"" }
					}
				]
			}";

    public LowLevelResponseTypes()
    {
        var connection = new InMemoryConnection(Response().Utf8Bytes());
        Client = new OpenSearchClient(new ConnectionSettings(connection).ApplyDomainSettings());
    }

    private OpenSearchClient Client { get; }


    [U]
    public void DynamicResponse()
    {
        /**[float]
			* === DynamicResponse
			*
			*/

        var response = Client.LowLevel.Search<DynamicResponse>(PostData.Empty);

        response.Get<string>("object.first").Should()
            .NotBeEmpty()
            .And.Be("value1");

        response.Get<string>("object._arbitrary_key_").Should()
            .NotBeEmpty()
            .And.Be("first");

        response.Get<int>("array.1").Should().Be(2);
        response.Get<long>("array.1").Should().Be(2);
        response.Get<long>("number").Should().Be(29);
        response.Get<long?>("number").Should().Be(29);
        response.Get<long?>("number_does_not_exist").Should().Be(null);
        response.Get<long?>("number").Should().Be(29);

        response.Get<string>("array_of_objects.1.second").Should()
            .NotBeEmpty()
            .And.Be("value22");

        response.Get<string>("array_of_objects.1.complex\\.nested.x").Should()
            .NotBeEmpty()
            .And.Be("value6");

        /**
			 * You can project into arrays using the dot notation
			 */
        response.Get<string[]>("array_of_objects.first").Should()
            .NotBeEmpty()
            .And.HaveCount(2)
            .And.BeEquivalentTo(new[] { "value11", "value21" });

        /**
			 * You can even peek into array of arrays
			 */
        var nestedZs = response.Get<int[][]>("array_of_objects.nested.z.id")
            //.SelectMany(d=>d.Get<int[]>("id"))
            .Should().NotBeEmpty()
            .And.HaveCount(2)
            .And.BeEquivalentTo(new[] { new[] { 1 }, new[] { 3, 2 } });


    }

}
