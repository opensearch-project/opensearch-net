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
using OpenSearch.Client;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Core.Client;
using Tests.Framework;
using static Tests.Core.Serialization.SerializationTestHelper;

namespace Tests.ClientConcepts.HighLevel.Mapping;

/**
	* [[attribute-mapping]]
	* === Attribute mapping
	*
	* In <<auto-map, Auto mapping>>, you saw that the type mapping for a POCO can be inferred from the
	* properties of the POCO, using `.AutoMap()`. But what do you do when you want to map differently
	* to the inferred mapping? This is where attribute mapping can help.
	*
	* It is possible to define your mappings using attributes on your POCO type and properties. With
	* attributes on properties and calling `.AutoMap()`, OSC will infer the mappings from the POCO property
	* types **and** take into account the mapping attributes.
	*
	* [IMPORTANT]
	* --
	* When you use attributes, you *must* also call `.AutoMap()` for the attributes to be applied.
	* --
	*
	* Here we define an `Employee` type and use attributes to define the mappings.
	*/
public class AttributeMapping
{
    private readonly IOpenSearchClient _client = TestClient.DisabledStreaming;

    [OpenSearchType(RelationName = "employee")]
    public class Employee
    {
        [Text(Name = "first_name", Norms = false, Similarity = "LMDirichlet")]
        public string FirstName { get; set; }

        [Text(Name = "last_name")]
        public string LastName { get; set; }

        [Number(DocValues = false, IgnoreMalformed = true, Coerce = true)]
        public int Salary { get; set; }

        [Date(Format = "MMddyyyy")]
        public DateTime Birthday { get; set; }

        [Boolean(NullValue = false, Store = true)]
        public bool IsManager { get; set; }

        [Nested]
        [PropertyName("empl")]
        public List<Employee> Employees { get; set; }

        [Text(Name = "office_hours")]
        public TimeSpan? OfficeHours { get; set; }

        [Object]
        public List<Skill> Skills { get; set; }
    }

    public class Skill
    {
        [Text]
        public string Name { get; set; }

        [Number(NumberType.Byte, Name = "level")]
        public int Proficiency { get; set; }
    }

    /**Then we map the types by calling `.AutoMap()` */
    [U]
    public void UsingAutoMapWithAttributes()
    {
        var createIndexResponse = _client.Indices.Create("myindex", c => c
            .Map<Employee>(m => m.AutoMap())
        );

        /**
			 */
        // json
        var expected = new
        {
            mappings = new
            {
                properties = new
                {
                    birthday = new
                    {
                        format = "MMddyyyy",
                        type = "date"
                    },
                    empl = new
                    {
                        properties = new { },
                        type = "nested"
                    },
                    first_name = new
                    {
                        type = "text",
                        norms = false,
                        similarity = "LMDirichlet"
                    },
                    isManager = new
                    {
                        null_value = false,
                        store = true,
                        type = "boolean"
                    },
                    last_name = new
                    {
                        type = "text"
                    },
                    office_hours = new
                    {
                        type = "text"
                    },
                    salary = new
                    {
                        coerce = true,
                        doc_values = false,
                        ignore_malformed = true,
                        type = "float"
                    },
                    skills = new
                    {
                        properties = new
                        {
                            level = new
                            {
                                type = "byte"
                            },
                            name = new
                            {
                                type = "text"
                            }
                        },
                        type = "object"
                    }
                }
            }
        };

        // hide
        Expect(expected).FromRequest(createIndexResponse);
    }
    /**
		 * Attribute mapping can be a convenient way to control how POCOs are mapped with minimal code, however
		 * there are some mapping features that cannot be expressed with attributes, for example, <<multi-fields, Multi fields>>.
		 * In order to have the full power of mapping in OSC at your disposal,
		 * take a look at <<fluent-mapping, Fluent Mapping>> next.
		 */
}
