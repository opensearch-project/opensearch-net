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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OpenSearch.Client;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Core.Client;
using Tests.Domain;
using Tests.Framework;
using Xunit;
using static Tests.Core.Serialization.SerializationTestHelper;

namespace Tests.Search;

/**[[returned-fields]]
	 * === Selecting fields to return
	 *
	 * Sometimes you don't need to return all of the fields of a document from a search query; for example, when showing
	 * most recent posts on a blog, you may only need the title of the blog to be returned from the
	 * query that finds the most recent posts.
	 *
	 * There are two approaches that you can take to return only some of the fields from a document i.e. a _partial_
	 * document (we use this term _loosely_ here); using stored fields and source filtering. Both are quite different
	 * in how they work.
	 */
public class ReturnedFields
{
    private readonly IOpenSearchClient _client = TestClient.DisabledStreaming;

    /** [[stored-fields]]
		 * ==== Stored fields
		 *
		 * When indexing a document, by default, OpenSearch stores the originally sent JSON document in a special
		 * field called {ref_current}/mapping-source-field.html[_source]. Documents returned from
		 * a search query are materialized from the `_source` field returned from OpenSearch for each hit.
		 *
		 * It is also possible to store a field from the JSON document _separately_ within OpenSearch
		 * by using {ref_current}/mapping-store.html[store] on the mapping. Why would you ever want to do this?
		 * Well, you may disable `_source` so that the source is not stored and select to store only specific fields.
		 * Another possibility is that the `_source` contains a field with large values, for example, the body of
		 * a blog post, but typically only another field is needed, for example, the title of the blog post.
		 * In this case, we don't want to pay the cost of OpenSearch deserializing the entire `_soure` just to
		 * get a small field.
		 *
		 * [IMPORTANT]
		 * --
		 * Opting to disable source for a type mapping means that the original JSON document sent to OpenSearch
		 * is *not* stored and hence can never be retrieved. Whilst you may save disk space in doing so, certain
		 * features are not going to work when source is disabled such as the Reindex API or on the fly
		 * highlighting.
		 *
		 * Seriously consider whether disabling source is what you really want to do for your use case.
		 * --
		 */
    [U]
    public void StoredFields()
    {
        /**
			 * When storing fields in this manner, the individual field values to return can be specified using
			 * `.StoredFields` on the search request
			 */
        var searchResponse = _client.Search<Project>(s => s
            .StoredFields(sf => sf
                .Fields(
                    f => f.Name,
                    f => f.StartedOn,
                    f => f.Branches
                )
            )
            .Query(q => q
                .MatchAll()
            )
        );

        /**
			 * And retrieving them is possible using `.Fields` on the response
			 */
        foreach (var fieldValues in searchResponse.Fields)
        {
            var document = new // <1> Construct a partial document as an anonymous type from the stored fields requested
            {
                Name = fieldValues.ValueOf<Project, string>(p => p.Name),
                StartedOn = fieldValues.Value<DateTime>(Infer.Field<Project>(p => p.StartedOn)),
                Branches = fieldValues.Values<Project, string>(p => p.Branches.First())
            };
        }
    }

    /**
		 * This works when storing fields separately. A much more common scenario however is to return
		 * only a selection of fields from the `_source`; this is where source filtering comes in.
		 *
		 * [[source-filtering]]
		 * ==== Source filtering
		 *
		 * Only some of the fields of a document can be returned from a search query
		 * using source filtering
		 */
    [U]
    public void SourceFiltering()
    {
        var searchResponse = _client.Search<Project>(s => s
            .Source(sf => sf
                .Includes(i => i // <1> **Include** the following fields
                    .Fields(
                        f => f.Name,
                        f => f.StartedOn,
                        f => f.Branches
                    )
                )
                .Excludes(e => e // <2> **Exclude** the following fields
                    .Fields("num*") // <3> Fields can be included or excluded through wildcard patterns
                )
            )
            .Query(q => q
                .MatchAll()
            )
        );

        /**
			 * With source filtering specified on the request, `.Documents` will
			 * now contain _partial_ documents, materialized from the source fields specified to include
			 */
        var partialProjects = searchResponse.Documents;

        /**
			 * It's possible to exclude `_source` from being returned altogether from a query with
			 */
        searchResponse = _client.Search<Project>(s => s
            .Source(false)
            .Query(q => q
                .MatchAll()
            )
        );
    }
}
