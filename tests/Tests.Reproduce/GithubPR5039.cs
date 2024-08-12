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
using System.Runtime.Serialization;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Core.Client;
using static Tests.Core.Serialization.SerializationTestHelper;

namespace Tests.Reproduce;

public class GithubPR5039
{
    public class MyCustomTokenizer : ITokenizer
    {
        public string Type => "my_custom_tok";
        public string Version { get; set; }

        public string Y { get; set; }
    }

    [U]
    public void CustomTokenizer()
    {
        var tokenizer = Object(new MyCustomTokenizer() { Version = "x", Y = "z" })
            .RoundTrips(new { type = "my_custom_tok", version = "x", y = "z" });
        tokenizer.Type.Should().Be("my_custom_tok");
        tokenizer.Version.Should().Be("x");
        tokenizer.Y.Should().Be("z");
    }

    public class DynamicSynonymTokenFilter : ITokenFilter
    {
        public bool? Expand { get; set; }
        public SynonymFormat? Format { get; set; }
        public bool? Lenient { get; set; }
        public IEnumerable<string> Synonyms { get; set; }

        [DataMember(Name = "synonyms_path")]
        public string SynonymsPath { get; set; }

        public string Tokenizer { get; set; }
        public bool? Updateable { get; set; }
        public string Type { get; } = "dynamic_synonym";
        public string Version { get; set; }
        public int? Interval { get; set; }
    }

    [U]
    public void CustomTokenFilter()
    {
        var tokenizer = Object(new DynamicSynonymTokenFilter() { Version = "x", SynonymsPath = "/root/access" })
            .RoundTrips(new { type = "dynamic_synonym", version = "x", synonyms_path = "/root/access" });
        tokenizer.Type.Should().Be("dynamic_synonym");
        tokenizer.Version.Should().Be("x");
        tokenizer.SynonymsPath.Should().Be("/root/access");
    }

    [U]
    public void CreateIndex()
    {
        var client = TestClient.DefaultInMemoryClient;

        var response = client.Indices.Create("my-index", i => i
            .Settings(s => s
                .Analysis(a => a
                    .TokenFilters(t => t
                        .UserDefined("mytf",
                            new DynamicSynonymTokenFilter
                            {
                                SynonymsPath = "https://my-synonym-server-url-that-not-is-relevant",
                                Updateable = true,
                                Lenient = true,
                                Interval = 60
                            })
                    )
                    .Tokenizers(t => t
                        .UserDefined("myt", new MyCustomTokenizer { Y = "yy" })
                    )
                )
            )
        );

        Expect(new
        {
            settings = new
            {
                analysis = new
                {
                    filter = new
                    {
                        mytf = new
                        {
                            lenient = true,
                            synonyms_path = "https://my-synonym-server-url-that-not-is-relevant",
                            updateable = true,
                            type = "dynamic_synonym",
                            interval = 60
                        }
                    },
                    tokenizer = new { myt = new { type = "my_custom_tok", y = "yy" } }
                }
            }
        })
            .NoRoundTrip()
            .FromRequest(response);
    }
}
