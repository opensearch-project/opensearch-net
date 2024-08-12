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

using System.Collections.Generic;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Core.Extensions;
using static Tests.Core.Serialization.SerializationTestHelper;
using Name = Bogus.DataSets.Name;

namespace Tests.Search.Hits;

public class HitsSerializationTests
{
    [U]
    public void CanDeserializeNestedNestedTopHits()
    {
        var json = @"{
				""took"" : 6,
				""timed_out"" : false,
				""_shards"" : {
				""total"" : 1,
				""successful"" : 1,
				""failed"" : 0
				},
				""hits"" : {
				""total"" : { ""value"" : 4 },
				""max_score"" : 0.0,
				""hits"" : [ ]
				},
				""aggregations"" : {
				""children"" : {
					""doc_count"" : 3,
					""grand_children"" : {
					""doc_count"" : 4,
					""grand_children_top_hits"" : {
						""hits"" : {
						""total"" : { ""value"" : 4 },
						""max_score"" : 1.4E-45,
						""hits"" : [ {
							""_index"" : ""people"",
							""_type"" : ""person"",
							""_id"" : ""AVhGTilue0LQGr8lV0AP"",
							""_nested"" : {
							""field"" : ""children"",
							""offset"" : 2,
							""_nested"" : {
								""field"" : ""children"",
								""offset"" : 1
							}
							},
							""_score"" : 0.0,
							""inner_hits"" : {
							""great_grand_children_inner_hits"" : {
								""hits"" : {
								""total"" : { ""value"" : 0 },
								""max_score"" : null,
								""hits"" : [ ]
								}
							},
							""children_inner_hits"" : {
								""hits"" : {
								""total"" : { ""value"" : 0 },
								""max_score"" : null,
								""hits"" : [ ]
								}
							},
							""grand_children_inner_hits"" : {
								""hits"" : {
								""total"" : { ""value"" : 0 },
								""max_score"" : null,
								""hits"" : [ ]
								}
							}
							}
						}, {
							""_index"" : ""people"",
							""_type"" : ""person"",
							""_id"" : ""AVhGTilue0LQGr8lV0AP"",
							""_nested"" : {
							""field"" : ""children"",
							""offset"" : 2,
							""_nested"" : {
								""field"" : ""children"",
								""offset"" : 0
							}
							},
							""_score"" : 0.0,
							""inner_hits"" : {
							""great_grand_children_inner_hits"" : {
								""hits"" : {
								""total"" : { ""value"" : 2 },
								""max_score"" : 2.178655,
								""hits"" : [ {
									""_index"" : ""people"",
									""_type"" : ""person"",
									""_id"" : ""AVhGTilue0LQGr8lV0AP"",
									""_nested"" : {
									""field"" : ""children"",
									""offset"" : 2,
									""_nested"" : {
										""field"" : ""children"",
										""offset"" : 0,
										""_nested"" : {
										""field"" : ""children"",
										""offset"" : 1
										}
									}
									},
									""_score"" : 2.178655,
									""_source"" : {
									""name"" : ""Ivy"",
									""age"" : 1,
									""gender"" : 1
									}
								}, {
									""_index"" : ""people"",
									""_type"" : ""person"",
									""_id"" : ""AVhGTilue0LQGr8lV0AP"",
									""_nested"" : {
									""field"" : ""children"",
									""offset"" : 2,
									""_nested"" : {
										""field"" : ""children"",
										""offset"" : 0,
										""_nested"" : {
										""field"" : ""children"",
										""offset"" : 0
										}
									}
									},
									""_score"" : 2.178655,
									""_source"" : {
									""name"" : ""Helen"",
									""age"" : 3,
									""gender"" : 1
									}
								} ]
								}
							},
							""children_inner_hits"" : {
								""hits"" : {
								""total"" : { ""value"" : 0 },
								""max_score"" : null,
								""hits"" : [ ]
								}
							},
							""grand_children_inner_hits"" : {
								""hits"" : {
								""total"" : { ""value"" : 0 },
								""max_score"" : null,
								""hits"" : [ ]
								}
							}
							}
						}, {
							""_index"" : ""people"",
							""_type"" : ""person"",
							""_id"" : ""AVhGTilue0LQGr8lV0AP"",
							""_nested"" : {
							""field"" : ""children"",
							""offset"" : 0,
							""_nested"" : {
								""field"" : ""children"",
								""offset"" : 1
							}
							},
							""_score"" : 0.0,
							""inner_hits"" : {
							""great_grand_children_inner_hits"" : {
								""hits"" : {
								""total"" : { ""value"" : 0 },
								""max_score"" : null,
								""hits"" : [ ]
								}
							},
							""children_inner_hits"" : {
								""hits"" : {
								""total"" : { ""value"" : 0 },
								""max_score"" : 0.0,
								""hits"" : [ {
									""_index"" : ""people"",
									""_type"" : ""person"",
									""_id"" : ""AVhGTilue0LQGr8lV0AP"",
									""_nested"" : {
									""field"" : ""children"",
									""offset"" : 2
									},
									""_score"" : 0.0
								} ]
								}
							},
							""grand_children_inner_hits"" : {
								""hits"" : {
								""total"" : { ""value"" : 0 },
								""max_score"" : null,
								""hits"" : [ ]
								}
							}
							}
						}, {
							""_index"" : ""people"",
							""_type"" : ""person"",
							""_id"" : ""AVhGTilue0LQGr8lV0AP"",
							""_nested"" : {
							""field"" : ""children"",
							""offset"" : 0,
							""_nested"" : {
								""field"" : ""children"",
								""offset"" : 0
							}
							},
							""_score"" : 0.0,
							""inner_hits"" : {
							""great_grand_children_inner_hits"" : {
								""hits"" : {
								""total"" : { ""value"" : 1 },
								""max_score"" : 2.178655,
								""hits"" : [ {
									""_index"" : ""people"",
									""_type"" : ""person"",
									""_id"" : ""AVhGTilue0LQGr8lV0AP"",
									""_nested"" : {
									""field"" : ""children"",
									""offset"" : 0,
									""_nested"" : {
										""field"" : ""children"",
										""offset"" : 0,
										""_nested"" : {
										""field"" : ""children"",
										""offset"" : 0
										}
									}
									},
									""_score"" : 2.178655,
									""_source"" : {
									""name"" : ""Daisy"",
									""age"" : 10,
									""gender"" : 1
									}
								} ]
								}
							},
							""children_inner_hits"" : {
								""hits"" : {
								""total"" : { ""value"" : 1 },
								""max_score"" : 0.0,
								""hits"" : [ {
									""_index"" : ""people"",
									""_type"" : ""person"",
									""_id"" : ""AVhGTilue0LQGr8lV0AP"",
									""_nested"" : {
									""field"" : ""children"",
									""offset"" : 2
									},
									""_score"" : 0.0
								} ]
								}
							},
							""grand_children_inner_hits"" : {
								""hits"" : {
								""total"" : { ""value"" : 0 },
								""max_score"" : null,
								""hits"" : [ ]
								}
							}
							}
						} ]
						}
					}
					}
				}
				}
			}
			";
        var response = Expect(json).NoRoundTrip().DeserializesTo<SearchResponse<Person>>();

        var nestedChildrenAggregation = response.Aggregations.Nested("children");
        nestedChildrenAggregation.Should().NotBeNull();

        var nestedGrandChildrenAggregation = nestedChildrenAggregation.Nested("grand_children");
        nestedGrandChildrenAggregation.Should().NotBeNull();

        var grandChildrenTopHits = nestedGrandChildrenAggregation.TopHits("grand_children_top_hits");
        grandChildrenTopHits.Should().NotBeNull();

        foreach (var hit in grandChildrenTopHits.Hits<Person>())
        {
            // top hits is applied to grandchildren so each result should have two levels
            // of nested identities i.e. grandparent -> parent (1st level) -> child (2nd level)
            var nestedIdentity = hit.Nested;

            nestedIdentity.Should().NotBeNull();
            nestedIdentity.Field.Should().NotBeNull();
            nestedIdentity.Offset.Should().BeGreaterOrEqualTo(0);

            var nestedNestedIdentity = nestedIdentity.Nested;
            nestedNestedIdentity.Should().NotBeNull();
            nestedNestedIdentity.Field.Should().NotBeNull();
            nestedNestedIdentity.Offset.Should().BeGreaterOrEqualTo(0);
        }
    }

    public class Person
    {
        public byte Age { get; set; }

        public List<Person> Children { get; set; }

        public Name.Gender Gender { get; set; }
        public string Name { get; set; }
    }
}
