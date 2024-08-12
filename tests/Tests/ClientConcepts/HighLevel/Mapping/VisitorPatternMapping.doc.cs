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

/**[[visitor-pattern-mapping]]
	 * === Applying conventions through the Visitor pattern
	 * It is also possible to apply a transformation on all or specific properties.
	 *
	 * `.AutoMap()` internally implements the https://en.wikipedia.org/wiki/Visitor_pattern[visitor pattern].
	 * The default visitor, `NoopPropertyVisitor`, does nothing and acts as a blank canvas for you
	 * to implement your own visiting methods.
	 *
	 * For instance, let's create a custom visitor that disables doc values for numeric and boolean types -
	 * __This is not really a good idea in practice, but let's do it anyway for the sake of a clear example.__
	 */
public class VisitorPattern
{
    private IOpenSearchClient client = TestClient.DisabledStreaming;

    /**
		* Using the following POCO
		*/
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

    /**
		 * We first define a visitor; it's easiest to inherit from `NoopPropertyVisitor` and override
		 * the `Visit` methods to implement your conventions
		 */
    public class DisableDocValuesPropertyVisitor : NoopPropertyVisitor
    {
        public override void Visit(
            INumberProperty type,
            PropertyInfo propertyInfo,
            OpenSearchPropertyAttributeBase attribute) //<1> Override the `Visit` method on `INumberProperty` and set `DocValues = false`
=> type.DocValues = false;

        public override void Visit(
            IBooleanProperty type,
            PropertyInfo propertyInfo,
            OpenSearchPropertyAttributeBase attribute) //<2> Similarily, override the `Visit` method on `IBooleanProperty` and set `DocValues = false`
=> type.DocValues = false;
    }

    [U]
    public void UsingACustomPropertyVisitor()
    {
        /** Now we can pass an instance of our custom visitor to `.AutoMap()` */
        var createIndexResponse = client.Indices.Create("myindex", c => c
            .Map<Employee>(m => m.AutoMap(new DisableDocValuesPropertyVisitor()))
        );

        /** and any time the client maps a property of the POCO (Employee in this example) as a number (INumberProperty) or boolean (IBooleanProperty),
			 * it will apply the transformation defined in each `Visit()` call respectively, which in this example
			 * disables {ref_current}/doc-values.html[doc_values].
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
                        type = "date"
                    },
                    employees = new
                    {
                        properties = new { },
                        type = "object"
                    },
                    firstName = new
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
                    isManager = new
                    {
                        doc_values = false,
                        type = "boolean"
                    },
                    lastName = new
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
                    salary = new
                    {
                        doc_values = false,
                        type = "integer"
                    },
                    hours = new
                    {
                        doc_values = false,
                        type = "long"
                    }
                }
            }
        };

        // hide
        Expect(expected).FromRequest(createIndexResponse);
    }

    /**
		 * ==== Visiting on PropertyInfo
		 * You can even take the visitor approach a step further, and instead of visiting on `IProperty` types, visit
		 * directly on your POCO reflected `PropertyInfo` properties.
		 *
		 * As an example, let's create a visitor that maps all CLR types to an OpenSearch text datatype (`ITextProperty`).
		 */
    public class EverythingIsATextPropertyVisitor : NoopPropertyVisitor
    {
        public override IProperty Visit(PropertyInfo propertyInfo, OpenSearchPropertyAttributeBase attribute) => new TextProperty();
    }

    [U]
    public void UsingACustomPropertyVisitorOnPropertyInfo()
    {
        var createIndexResponse = client.Indices.Create("myindex", c => c
            .Map<Employee>(m => m.AutoMap(new EverythingIsATextPropertyVisitor()))
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
                        type = "text"
                    },
                    employees = new
                    {
                        type = "text"
                    },
                    firstName = new
                    {
                        type = "text"
                    },
                    isManager = new
                    {
                        type = "text"
                    },
                    lastName = new
                    {
                        type = "text"
                    },
                    salary = new
                    {
                        type = "text"
                    },
                    hours = new
                    {
                        type = "text"
                    }
                }
            }
        };

        // hide
        Expect(expected).FromRequest(createIndexResponse);
    }
    /**
		 * ==== Skip properties
		 *
		 * Through implementing `SkipProperty` on the visitor, you can prevent certain properties from being mapped.
		 *
		 * In this example, we skip the inherited properties of the type from which `DictionaryDocument` is derived
		 *
		 */
    public class DictionaryDocument : SortedDictionary<string, dynamic>
    {
        public int Id { get; set; }
    }

    public class IgnoreInheritedPropertiesVisitor<T> : NoopPropertyVisitor
    {
        public override bool SkipProperty(PropertyInfo propertyInfo, OpenSearchPropertyAttributeBase attribute) => propertyInfo?.DeclaringType != typeof(T);
    }

    [U]
    public void HidesInheritedMembers()
    {
        var createIndexResponse = client.Indices.Create("myindex", c => c
            .Map<DictionaryDocument>(m => m.AutoMap(new IgnoreInheritedPropertiesVisitor<DictionaryDocument>()))
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
                    id = new
                    {
                        type = "integer"
                    }
                }
            }
        };

        // hide
        Expect(expected).FromRequest(createIndexResponse);
    }
}
