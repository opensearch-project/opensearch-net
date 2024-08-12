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
using Newtonsoft.Json;
using OpenSearch.Client;
using OpenSearch.Net;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Framework;
using static Tests.Core.Serialization.SerializationTestHelper;

namespace Tests.ClientConcepts.HighLevel.Mapping;

/**[[ignoring-properties]]
	* === Ignoring properties
	* Properties on a POCO can be ignored for mapping purposes in a few ways:
	*
	* - Using the `Ignore` property on a derived `OpenSearchPropertyAttributeBase` type, such as `TextAttribute`, applied to
	* the property that should be ignored on the POCO
	*
	* - Using the `Ignore` property on `PropertyNameAttribute` applied to a property that should be ignored on the POCO
	*
	* - Using the `.DefaultMappingFor<TDocument>(Func<ClrTypeMappingDescriptor<TDocument>, IClrTypeMapping<TDocument>>
	* selector)` on `ConnectionSettings`
	*
	* - Using an ignore attribute applied to the POCO property that is understood by
	* the `IOpenSearchSerializer` used, and inspected inside of the `CreatePropertyMapping()` on
	* the serializer. Using the builtin `SourceSerializer` this would be the `IgnoreProperty`
	*
	* This example demonstrates all ways, using the `Ignore` property on the attribute to ignore the property
	* `PropertyToIgnore`, the infer mapping to ignore the property `AnotherPropertyToIgnore` and the
	* json serializer specific attribute  to ignore the property either `IgnoreProperty` or `JsonIgnoredProperty` depending on which
	* `SourceSerializer` we configured.
	*/
public class IgnoringProperties
{
    [OpenSearchType(RelationName = "company")]
    public class CompanyWithAttributesAndPropertiesToIgnore
    {
        public string Name { get; set; }

        [Text(Ignore = true)]
        public string PropertyToIgnore { get; set; }

        [PropertyName("anotherPropertyToIgnore", Ignore = true)]
        public string AnotherPropertyToIgnore { get; set; }

        public string FluentMappingPropertyToIgnore { get; set; }

        [Ignore, JsonIgnore]
        public string JsonIgnoredProperty { get; set; }
    }

    [U]
    public void Ignoring()
    {
        /** All of the properties except `Name` have been ignored in the mapping */
        var connectionSettings = new ConnectionSettings(new InMemoryConnection()) // <1> we're using an in-memory connection, but in your application, you'll want to use an `IConnection` that actually sends a request.
            .DisableDirectStreaming() // <2> we disable direct streaming here to capture the request and response bytes. In a production application, you would likely not call this as it adds overhead to each call.
            .DefaultMappingFor<CompanyWithAttributesAndPropertiesToIgnore>(m => m
                .Ignore(p => p.FluentMappingPropertyToIgnore)
            );

        var client = new OpenSearchClient(connectionSettings);

        var createIndexResponse = client.Indices.Create("myindex", c => c
            .Map<CompanyWithAttributesAndPropertiesToIgnore>(m => m
                .AutoMap()
            )
        );

        /**
			 * The JSON output for the mapping does not contain the ignored properties
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

    /**==== Ignoring inherited properties
		 *
		 * By using the `DefaultMappingFor<T>()` configuration for a POCO on the `ConnectionSettings`, it is possible to
		 * ignore inherited properties too
		 */
    public class Parent
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public string IgnoreMe { get; set; }
    }

    public class Child : Parent { }

    [U]
    public void IgnoringInheritedProperties()
    {
        var connectionSettings = new ConnectionSettings(new InMemoryConnection())
            .DisableDirectStreaming()
            .DefaultMappingFor<Child>(m => m
                .PropertyName(p => p.Description, "desc")
                .Ignore(p => p.IgnoreMe)
            );

        var client = new OpenSearchClient(connectionSettings);

        var createIndexResponse = client.Indices.Create("myindex", c => c
            .Map<Child>(m => m
                .AutoMap()
            )
        );

        /** The property `IgnoreMe` has been ignored for the child mapping */
        //json
        var expected = new
        {
            mappings = new
            {
                properties = new
                {
                    id = new
                    {
                        type = "integer"
                    },
                    desc = new
                    {
                        fields = new
                        {
                            keyword = new
                            {
                                ignore_above = 256,
                                type = "keyword"
                            }
                        },
                        type = "text"
                    }
                }
            }
        };

        //hide
        Expect(expected).FromRequest(createIndexResponse);
    }

    /**==== Overriding inherited properties
		 *
		 * `DefaultMappingFor<T>()` configuration for a POCO on the `ConnectionSettings` can also be
		 * used to override inherited properties.
		 *
		 * In the following example, the `Id` property is shadowed in `ParentWithStringId` as
		 * a `string` type, resulting in OSC's automapping inferring the mapping as the default
		 * `text` with `keyword` multi-field field datatype mapping for a `string` type.
		 *
		 */
    public class ParentWithStringId : Parent
    {
        public new string Id { get; set; }
    }

    [U]
    public void OverridingInheritedProperties()
    {
        var connectionSettings = new ConnectionSettings(new InMemoryConnection()) // <1> we're using an _in memory_ connection for this example. In your production application though, you'll want to use an `IConnection` that actually sends a request.
            .DisableDirectStreaming() // <2> we disable direct streaming here to capture the request and response bytes. In your production application however, you'll likely not want to do this, since it causes the request and response bytes to be buffered in memory.
            .DefaultMappingFor<ParentWithStringId>(m => m
                .Ignore(p => p.Description)
                .Ignore(p => p.IgnoreMe)
            );

        var client = new OpenSearchClient(connectionSettings);

        var createIndexResponse = client.Indices.Create("myindex", c => c
            .Map<ParentWithStringId>(m => m
                .AutoMap()
            )
        );

        // json
        var expected = new
        {
            mappings = new
            {
                properties = new
                {
                    id = new
                    {
                        type = "text",
                        fields = new
                        {
                            keyword = new
                            {
                                ignore_above = 256,
                                type = "keyword"
                            }
                        }
                    }
                }
            }
        };

        // hide
        Expect(expected).FromRequest(createIndexResponse);
    }
}
