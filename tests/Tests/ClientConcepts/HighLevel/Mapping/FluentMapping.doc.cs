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
	* [[fluent-mapping]]
	* === Fluent mapping
	*
	* Fluent mapping POCO properties to fields within an OpenSearch type mapping
	* offers the most control over the process. With fluent mapping, each property of
	* the POCO is explicitly mapped to an OpenSearch type field mapping.
	*/

public class FluentMapping
{
    private readonly IOpenSearchClient _client = TestClient.DisabledStreaming;

    /**
		* To demonstrate, we'll define two POCOs
		*
		* - `Company`, which has a name and a collection of Employees
		* - `Employee` which has various properties of different types and has itself a collection of `Employee` types.
		*/
    public class Company
    {
        public string Name { get; set; }
        public List<Employee> Employees { get; set; }
    }

    public class Employee
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Salary { get; set; }
        public DateTime Birthday { get; set; }
        public bool IsManager { get; set; }
        public List<Employee> Employees { get; set; }
        public TimeSpan Hours { get; set; }
    }

    [U]
    public void MappingManually()
    {
        /**==== Manual mapping
			 * To create a mapping for our Company type, we can use the fluent API
			 * and map each property explicitly
			 */
        var createIndexResponse = _client.Indices.Create("myindex", c => c
            .Map<Company>(m => m
                .Properties(ps => ps
                    .Text(s => s
                        .Name(n => n.Name)
                    )
                    .Object<Employee>(o => o
                        .Name(n => n.Employees)
                        .Properties(eps => eps
                            .Text(s => s
                                .Name(e => e.FirstName)
                            )
                            .Text(s => s
                                .Name(e => e.LastName)
                            )
                            .Number(n => n
                                .Name(e => e.Salary)
                                .Type(NumberType.Integer)
                            )
                        )
                    )
                )
            )
        );

        /**
			 * Here, the Name property of the `Company` type has been mapped as a {ref_current}/text.html[text datatype] and
			 * the `Employees` property mapped as an {ref_current}/object.html[object datatype]. Within this object mapping,
			 * only the `FirstName`, `LastName` and `Salary` properties of the `Employee` type have been mapped.
			 *
			 * The json mapping for this example looks like
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
                        type = "text"
                    },
                    employees = new
                    {
                        type = "object",
                        properties = new
                        {
                            firstName = new
                            {
                                type = "text"
                            },
                            lastName = new
                            {
                                type = "text"
                            },
                            salary = new
                            {
                                type = "integer"
                            }
                        }
                    }
                }
            }
        };

        // hide
        Expect(expected).FromRequest(createIndexResponse);
    }

    /** Manual mapping in this way is powerful but can become verbose and unwieldy for
		* large POCOs. The majority of the time you simply want to map *all* the properties of a POCO in a single go
		* without having to specify the mapping for each property,
		* particularly when there is <<auto-map,inferred mapping>> from CLR types to OpenSearch types.
		*
		* This is where the fluent mapping in conjunction with auto mapping comes in.
		*
		* [[auto-map-with-overrides]]
		* ==== Auto mapping with fluent overrides
		*
		* In most cases, you'll want to map more than just the vanilla datatypes and also provide
		* various options for your properties, such as the analyzer to use, whether to enable `doc_values`, etc.
		*
		* In this case, it's possible to use `.AutoMap()` in conjunction with explicitly mapped properties.
		*/
    [U]
    public void OverridingAutoMappedProperties()
    {
        /**
			* Here we are using `.AutoMap()` to automatically infer the mapping of our `Company` type from the
			* CLR property types, but then we're overriding the `Employees` property to make it a
			* {ref_current}/nested.html[nested datatype], since by default `.AutoMap()` will infer the
			* `List<Employee>` property as an `object` datatype
			*/
        var createIndexResponse = _client.Indices.Create("myindex", c => c
            .Map<Company>(m => m
                .AutoMap()
                .Properties(ps => ps
                    .Nested<Employee>(n => n
                        .Name(nn => nn.Employees)
                    )
                )
            )
        );

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
                    },
                    employees = new
                    {
                        type = "nested",
                    }
                }
            }
        };

        //hide
        Expect(expected).FromRequest(createIndexResponse);

        /**
			 * `.AutoMap()` __**is idempotent**__ therefore calling it _before_ or _after_
			 * manually mapped properties will still yield the same result. The next example
			 * generates the same mapping as the previous
			 */
        createIndexResponse = _client.Indices.Create("myindex", c => c
            .Map<Company>(m => m
                .Properties(ps => ps
                    .Nested<Employee>(n => n
                        .Name(nn => nn.Employees)
                    )
                )
                .AutoMap()
            )
        );

        //hide
        Expect(expected).FromRequest(createIndexResponse);
    }

    /**
		 * ==== Auto mapping overrides down the object graph
		 *
		 * Just as we were able to override the inferred properties from auto mapping in the previous example,
		 * fluent mapping also takes precedence over <<attribute-mapping, Attribute Mapping>>.
		 * In this way, fluent, attribute and auto mapping can be combined. We'll demonstrate with an example.
		 *
		 * Consider the following two POCOS
		 */
    [OpenSearchType(RelationName = "company")]
    public class CompanyWithAttributes
    {
        [Keyword(NullValue = "null", Similarity = "BM25")]
        public string Name { get; set; }

        [Text(Name = "office_hours")]
        public TimeSpan? HeadOfficeHours { get; set; }

        [Object(Store = false)]
        public List<EmployeeWithAttributes> Employees { get; set; }
    }

    [OpenSearchType(RelationName = "employee")]
    public class EmployeeWithAttributes
    {
        [Text(Name = "first_name")]
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
    }

    /**
		 * Now when mapping, `AutoMap()` is called to infer the mapping from the POCO property types and
		 * attributes, and inferred mappings are overridden with fluent mapping
		 */
    [U]
    public void OverridingAutoMappedAttributes()
    {
        var createIndexResponse = _client.Indices.Create("myindex", c => c
            .Map<CompanyWithAttributes>(m => m
                .AutoMap() // <1> Automap company
                .Properties(ps => ps // <2> Override company inferred mappings
                    .Nested<EmployeeWithAttributes>(n => n
                        .Name(nn => nn.Employees)
                        .AutoMap() // <3> Automap nested employee type
                        .Properties(pps => pps // <4> Override employee inferred mappings
                            .Text(s => s
                                .Name(e => e.FirstName)
                                .Fields(fs => fs
                                    .Keyword(ss => ss
                                        .Name("firstNameRaw")
                                    )
                                    .TokenCount(t => t
                                        .Name("length")
                                        .Analyzer("standard")
                                    )
                                )
                            )
                            .Number(nu => nu
                                .Name(e => e.Salary)
                                .Type(NumberType.Double)
                                .IgnoreMalformed(false)
                            )
                            .Date(d => d
                                .Name(e => e.Birthday)
                                .Format("MM-dd-yy")
                            )
                        )
                    )
                )
            )
        );

        //json
        var expected = new
        {
            mappings = new
            {
                properties = new
                {
                    employees = new
                    {
                        type = "nested",
                        properties = new
                        {
                            birthday = new
                            {
                                format = "MM-dd-yy",
                                type = "date"
                            },
                            empl = new
                            {
                                properties = new
                                {
                                    birthday = new
                                    {
                                        type = "date"
                                    },
                                    employees = new
                                    {
                                        properties = new { },
                                        type = "object"
                                    },
                                    firstName = new
                                    {
                                        fields = new
                                        {
                                            keyword = new
                                            {
                                                type = "keyword",
                                                ignore_above = 256
                                            }
                                        },
                                        type = "text"
                                    },
                                    hours = new
                                    {
                                        type = "long"
                                    },
                                    isManager = new
                                    {
                                        type = "boolean"
                                    },
                                    lastName = new
                                    {
                                        fields = new
                                        {
                                            keyword = new
                                            {
                                                type = "keyword",
                                                ignore_above = 256
                                            }
                                        },
                                        type = "text"
                                    },
                                    salary = new
                                    {
                                        type = "integer"
                                    }
                                },
                                type = "nested"
                            },
                            first_name = new
                            {
                                fields = new
                                {
                                    firstNameRaw = new
                                    {
                                        type = "keyword"
                                    },
                                    length = new
                                    {
                                        analyzer = "standard",
                                        type = "token_count"
                                    }
                                },
                                type = "text"
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
                            salary = new
                            {
                                ignore_malformed = false,
                                type = "double"
                            }
                        }
                    },
                    name = new
                    {
                        null_value = "null",
                        similarity = "BM25",
                        type = "keyword"
                    },
                    office_hours = new
                    {
                        type = "text"
                    }
                }
            }
        };

        /**
			* As demonstrated, by calling `.AutoMap()` inside of the `.Nested<Employee>` mapping, it is possible to auto map the
			* `Employee` nested properties and again, override any inferred mapping from the automapping process,
			* through manual mapping
			*/
        // hide
        Expect(expected).FromRequest(createIndexResponse);
    }

    /** 
		* [[mapping-runtime-fields]]
		* ==== Mapping runtime fields
		*
		* A {ref_current}/runtime.html[runtime field] is a field that is evaluated at query time. Runtime fields may 
		* be defined in the mapping of an index.
		*
		* In this example, we'll define a `CompanyRuntimeFields` class with a single property which we may then use in 
		* the strongly-typed runtime field mapping.
		*/

    public class CompanyRuntimeFields
    {
        public string BirthDayOfWeek { get; set; }
    }

    [U]
    public void MappingRuntimeFields()
    {
        var createIndexResponse = _client.Indices.Create("myindex", c => c
            .Map<Company>(m => m
                .RuntimeFields<CompanyRuntimeFields>(rtf => rtf //<1> Use the `CompanyRuntimeFields` class as the generic argument
                    .RuntimeField(f => f.BirthDayOfWeek, FieldType.Keyword, f => f.Script("emit(doc['@timestamp'].value.dayOfWeekEnum.getDisplayName(TextStyle.FULL, Locale.ROOT))"))) //<2> Use the `BirthDayOfWeek` property as the runtime field name
            )
        );

        //json
        var expected = new
        {
            mappings = new
            {
                runtime = new
                {
                    birthDayOfWeek = new
                    {
                        type = "keyword",
                        script = new
                        {
                            lang = "painless",
                            source = "emit(doc['@timestamp'].value.dayOfWeekEnum.getDisplayName(TextStyle.FULL, Locale.ROOT))"
                        }
                    }
                }
            }
        };

        //hide
        Expect(expected).FromRequest(createIndexResponse);

        /**
			 * It's not necessary to define a type for the runtime field mapping. Runtime fields can optionally be defined 
			 * by providing a `string` name.
			 */
        createIndexResponse = _client.Indices.Create("myindex", c => c
            .Map<Company>(m => m
                .RuntimeFields(rtf => rtf
                    .RuntimeField("birthDayOfWeek", FieldType.Keyword, f => f.Script("emit(doc['@timestamp'].value.dayOfWeekEnum.getDisplayName(TextStyle.FULL, Locale.ROOT))")))
            )
        );

        //hide
        Expect(expected).FromRequest(createIndexResponse);
    }
}
