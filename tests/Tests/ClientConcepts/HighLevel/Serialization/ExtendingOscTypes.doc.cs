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
using Tests.ClientConcepts.HighLevel.Mapping;
using Tests.Core.Client;
using Tests.Domain;
using Tests.Framework;
using static Tests.Core.Serialization.SerializationTestHelper;

namespace Tests.ClientConcepts.HighLevel.Serialization;

/**[[extending-osc-types]]
	 * === Extending OSC types
	 *
	 * Sometimes you might want to provide a custom implementation of a type, perhaps to work around an issue or because
	 * you're using a third-party plugin that extends the features of OpenSearch, and OSC does not provide support out of the box.
	 *
	 * OSC allows extending its types in some scenarios, discussed here.
	 *
	 * ==== Creating your own property mapping
	 *
	 * As an example, let's imagine we're using a third party plugin that provides support for additional data type
	 * for field mapping. We can implement a custom `IProperty` implementation so that we can use the field mapping
	 * type with OSC.
	 */
public class ExtendingOscTypes
{
    // keep field name as client, for documentation purposes
    private readonly IOpenSearchClient client = TestClient.DisabledStreaming;

    public class MyPluginProperty : IProperty
    {
        IDictionary<string, object> IProperty.LocalMetadata { get; set; }
        IDictionary<string, string> IProperty.Meta { get; set; }
        public string Type { get; set; } = "my_plugin_property";
        public PropertyName Name { get; set; }

        public MyPluginProperty(string name, string language)
        {
            Name = name;
            Language = language;
            Numeric = true;
        }

        [PropertyName("language")]
        public string Language { get; set; }

        [PropertyName("numeric")]
        public bool Numeric { get; set; }
    }

    [U]
    public void InjectACustomIPropertyImplementation()
    {
        /**
			 * `PropertyNameAttribute` can be used to mark properties that should be serialized. Without this attribute,
			 * OSC won't pick up the property for serialization.
			 *
			 * Now that we have our own `IProperty` implementation we can add it to our properties mapping when creating an index
			 */
        var createIndexResponse = client.Indices.Create("myindex", c => c
            .Map<Project>(m => m
                .Properties(props => props
                    .Custom(new MyPluginProperty("fieldName", "dutch"))
                )
            )
        );

        /**
			 * which will serialize to the following JSON request
			 */
        // json
        var expected = new
        {
            mappings = new
            {
                properties = new
                {
                    fieldName = new
                    {
                        type = "my_plugin_property",
                        language = "dutch",
                        numeric = true
                    }
                }
            }
        };

        /**
			 * Whilst OSC can _serialize_ our `my_plugin_property`, it does not know how to _deserialize_ it;
			 * We plan to make this more pluggable in the future.
			 */
        // hide
        Expect(expected).FromRequest(createIndexResponse);
    }
}
