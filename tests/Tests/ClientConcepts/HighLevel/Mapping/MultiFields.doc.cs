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
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using OpenSearch.Client;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Core.Client;
using Tests.Framework;
using static Tests.Core.Serialization.SerializationTestHelper;

namespace Tests.ClientConcepts.HighLevel.Mapping;

/**[[multi-fields]]
	* === Multi fields
	*
	* It is often useful to index the same field in OpenSearch in different ways, to
	* serve different purposes, for example, mapping a POCO `string` property as a
	* `text` datatype for full text search as well as mapping as a `keyword` datatype for
	* structured search, sorting and aggregations. Another example is mapping a POCO `string`
	* property to use different analyzers, to serve different full text search needs.
	*
	* Let's look at a few examples. for each, we use the following simple POCO
	*/

public class MultiFields
{
    private readonly IOpenSearchClient _client = TestClient.DisabledStreaming;

    public class Person
    {
        public string Name { get; set; }
    }

    /**
		* ==== Default mapping for String properties
		*
		* When using <<auto-map, Auto Mapping>>, the inferred mapping for a `string`
		* POCO type is a `text` datatype with multi fields including a `keyword` sub field
		*/
    [U]
    public void DefaultMultiFields()
    {
        var createIndexResponse = _client.Indices.Create("myindex", c => c
            .Map<Person>(m => m
                .AutoMap()
            )
        );

        /**
			 * This results in the following JSON request
			 */
        //json
        var expected = new
        {
            mappings = new
            {
                properties = new
                {
                    name = new
                    {
                        type = "text",
                        fields = new
                        {
                            keyword = new
                            {
                                type = "keyword",
                                ignore_above = 256
                            }
                        }
                    }
                }
            }
        };

        //hide
        Expect(expected).FromRequest(createIndexResponse);
    }

    /**
		 * This is useful because the property can be used for both full text search
		 * as well as for structured search, sorting and aggregations
		 */
    [U]
    public void Searching()
    {
        var searchResponse = _client.Search<Person>(s => s
            .Query(q => q
                .Match(m => m
                    .Field(f => f.Name)
                    .Query("Russ")
                )
            )
            .Sort(ss => ss
                .Descending(f => f.Name.Suffix("keyword")) // <1> Use the keyword subfield on `Name`
            )
            .Aggregations(a => a
                .Terms("peoples_names", t => t
                    .Field(f => f.Name.Suffix("keyword"))
                )
            )
        );

        /**
			 */
        //json
        var expected = new
        {
            query = new
            {
                match = new
                {
                    name = new
                    {
                        query = "Russ"
                    }
                }
            },
            sort = new object[]
            {
                new JObject
                {
                    { "name.keyword", new JObject { { "order", "desc" } } }
                }
            },
            aggs = new
            {
                peoples_names = new
                {
                    terms = new
                    {
                        field = "name.keyword"
                    }
                }
            }
        };

        // hide
        Expect(expected).FromRequest(searchResponse);
    }


    /**
		* [NOTE]
		* --
		* Multi fields do not change the original `_source` field in OpenSearch; they affect only how
		* a field is indexed.
		*
		* New multi fields can be added to existing fields using the Put Mapping API.
		* --
		*
		* ==== Creating Multi fields
		*
		* Multi fields can be created on a mapping using the `.Fields()` method within a field mapping
		*/
    [U]
    public void CreatingMultiFields()
    {
        var createIndexResponse = _client.Indices.Create("myindex", c => c
            .Map<Person>(m => m
                .Properties(p => p
                    .Text(t => t
                        .Name(n => n.Name)
                        .Fields(ff => ff
                            .Text(tt => tt
                                .Name("stop") // <1> Use the stop analyzer on this sub field
                                .Analyzer("stop")
                            )
                            .Text(tt => tt
                                .Name("shingles")
                                .Analyzer("name_shingles") // <2> Use a custom analyzer named "named_shingles" that is configured in the index
                            )
                            .Keyword(k => k
                                .Name("keyword") // <3> Index as not analyzed
                                .IgnoreAbove(256)
                            )
                        )
                    )
                )
            )
        );

        /**
			 */
        //json
        var expected = new
        {
            mappings = new
            {
                properties = new
                {
                    name = new
                    {
                        type = "text",
                        fields = new
                        {
                            stop = new
                            {
                                type = "text",
                                analyzer = "stop"
                            },
                            shingles = new
                            {
                                type = "text",
                                analyzer = "name_shingles"
                            },
                            keyword = new
                            {
                                type = "keyword",
                                ignore_above = 256
                            }
                        }
                    }
                }
            }
        };

        //hide
        Expect(expected).FromRequest(createIndexResponse);
    }
}
