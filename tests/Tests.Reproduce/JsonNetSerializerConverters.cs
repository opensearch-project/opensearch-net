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
using System.Text;
using FluentAssertions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using OpenSearch.Client;
using OpenSearch.Client.JsonNetSerializer;
using OpenSearch.Net;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;


namespace Tests.Reproduce;

// from https://stackoverflow.com/questions/49224866/elasticsearch-nest-6-storing-enums-as-string
public class JsonNetSerializerConverters
{
    public enum ProductType
    {
        Example
    }

    [U]
    public void JsonConvertersInJsonSerializerSettingsAreHonoured()
    {
        var pool = new SingleNodeConnectionPool(new Uri("http://localhost:9200"));
        var connectionSettings = new ConnectionSettings(pool, new InMemoryConnection(), (builtin, settings) =>
            new JsonNetSerializer(builtin, settings,
                () => new JsonSerializerSettings
                {
                    Converters = new List<JsonConverter> { new StringEnumConverter() }
                })).DisableDirectStreaming();

        var client = new OpenSearchClient(connectionSettings);

        var indexResponse = client.Index(new Product { ProductType = ProductType.Example }, i => i.Index("examples"));
        Encoding.UTF8.GetString(indexResponse.ApiCall.RequestBodyInBytes).Should().Contain("\"productType\":\"Example\"");
    }

    public class Product
    {
        public ProductType ProductType { get; set; }
    }
}
