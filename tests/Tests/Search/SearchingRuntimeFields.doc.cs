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

using OpenSearch.Client;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Core.Client;
using Tests.Domain;
using static Tests.Core.Serialization.SerializationTestHelper;

namespace Tests.Search;

/**
* === Searching runtime fields
*
* Runtime fields can be returned with search requests by specifying the fields using `.Fields`
* on the search request.
* 
* [WARNING]
* --
* This functionality is in beta and is subject to change. The design and code is less mature 
* than official GA features and is being provided as-is with no warranties. Beta features 
* are not subject to the support SLA of official GA features.
* --
*
*/
public class SearchingRuntimeFields
{
    private readonly IOpenSearchClient _client = TestClient.DisabledStreaming;

    [U]
    public void RetrievingRuntimeFields()
    {
        var searchResponse = _client.Search<Project>(s => s
            .Query(q => q
                .MatchAll()
            )
            .Fields<ProjectRuntimeFields>(fs => fs
                .Field(f => f.StartedOnDayOfWeek)
                .Field(f => f.ThirtyDaysFromStarted, format: DateFormat.basic_date)
            )
        );

        /**
			 * which serializes to the following JSON
			 */
        //json
        var expected = new
        {
            query = new
            {
                match_all = new { }
            },
            fields = new object[]
            {
                "runtime_started_on_day_of_week",
                new
                {
                    field = "runtime_thirty_days_after_started",
                    format = "basic_date"
                }
            },
        };

        //hide
        Expect(expected).FromRequest(searchResponse);

        /**
			 * The previous example used the Fluent API to express the query. OSC also exposes an
			 * Object Initializer syntax to compose queries
			 */
        var searchRequest = new SearchRequest<Project>
        {
            Query = new MatchAllQuery(),
            Fields = Infer.Field<ProjectRuntimeFields>(p => p.StartedOnDayOfWeek) //<1> Here we infer the field name from a property on a POCO class
                .And<ProjectRuntimeFields>(p => p.ThirtyDaysFromStarted, format: DateFormat.basic_date) //<2> For runtime fields which return a date, a format may be specified.
        };

        searchResponse = _client.Search<Project>(searchRequest);

        //hide
        Expect(expected).FromRequest(searchResponse);
    }

    /**==== Defining runtime fields
		 *
		 * You may define runtime fields that exist only as part of a query by specifying `.RuntimeFields` on
		 * the search request. You may return this field using `.Fields` or use it for an aggregation.
		 */
    [U]
    public void SearchQueryRuntimeFields()
    {
        var searchResponse = _client.Search<Project>(s => s
            .Query(q => q
                .MatchAll()
            )
            .Fields<ProjectRuntimeFields>(fs => fs
                .Field(f => f.StartedOnDayOfWeek)
                .Field(f => f.ThirtyDaysFromStarted, format: DateFormat.basic_date)
                .Field("search_runtime_field")
            )
            .RuntimeFields(rtf => rtf.RuntimeField("search_runtime_field", FieldType.Keyword, r => r
                .Script("if (doc['type'].size() != 0) {emit(doc['type'].value.toUpperCase())}")))
        );

        /**which yields the following query JSON
			 *
			 */
        // json
        var expected = new
        {
            query = new
            {
                match_all = new { }
            },
            fields = new object[]
            {
                "runtime_started_on_day_of_week",
                new
                {
                    field = "runtime_thirty_days_after_started",
                    format = "basic_date"
                },
                "search_runtime_field"
            },
            runtime_mappings = new
            {
                search_runtime_field = new
                {
                    script = new
                    {
                        lang = "painless",
                        source = "if (doc['type'].size() != 0) {emit(doc['type'].value.toUpperCase())}"
                    },
                    type = "keyword"
                }
            }
        };

        //hide
        Expect(expected).FromRequest(searchResponse);

        /**
			 * The previous example used the Fluent API to express the query. Here is the same query using the
			 * Object Initializer syntax.
			 */
        var searchRequest = new SearchRequest<Project>
        {
            Query = new MatchAllQuery(),
            Fields = Infer.Field<ProjectRuntimeFields>(p => p.StartedOnDayOfWeek)
                .And<ProjectRuntimeFields>(p => p.ThirtyDaysFromStarted, format: DateFormat.basic_date)
                .And("search_runtime_field"),
            RuntimeFields = new RuntimeFields
            {
                { "search_runtime_field", new RuntimeField
                    {
                        Type = FieldType.Keyword,
                        Script = new PainlessScript("if (doc['type'].size() != 0) {emit(doc['type'].value.toUpperCase())}")
                    }
                }
            }
        };

        searchResponse = _client.Search<Project>(searchRequest);

        //hide
        Expect(expected).FromRequest(searchResponse);
    }
}
