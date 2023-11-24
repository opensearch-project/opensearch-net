/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.Diagnostics;
using OpenSearch.Client;
using OpenSearch.Net;

namespace Samples.RawJson;

public class RawJsonHighLevelSample : Sample
{
	public RawJsonHighLevelSample() : base("raw-json-high-level", "A sample demonstrating how to use the high-level client to perform raw JSON requests") { }

    protected override async Task Run(IOpenSearchClient client)
    {
        var info = await client.Http.GetAsync<DynamicResponse>("/");
		Debug.Assert(info.Success, info.DebugInformation);
        Console.WriteLine($"Welcome to {info.Body.version.distribution} {info.Body.version.number}!");

		const string indexName = "movies";

		// Check if the index already exists

		var indexExists = await client.Http.HeadAsync<DynamicResponse>($"/{indexName}");
		Debug.Assert(indexExists.HttpStatusCode == 404, indexExists.DebugInformation);
		Console.WriteLine($"Index Exists: {indexExists.HttpStatusCode == 200}");

		// Create an index

		var indexBody = new { settings = new { index = new { number_of_shards = 4 } } };

        var createIndex = await client.Http.PutAsync<DynamicResponse>($"/{indexName}", d => d.SerializableBody(indexBody));
		Debug.Assert(createIndex.Success && (bool)createIndex.Body.acknowledged, createIndex.DebugInformation);
		Console.WriteLine($"Create Index: {createIndex.Success && (bool)createIndex.Body.acknowledged}");

		// Add a document to the index
		var document = new { title = "Moneyball", director = "Bennett Miller", year = 2011 };

		const string id = "1";

		var addDocument = await client.Http.PutAsync<DynamicResponse>(
			$"/{indexName}/_doc/{id}",
			d => d
				.SerializableBody(document)
				.QueryString(qs => qs.Add("refresh", true)));
		Debug.Assert(addDocument.Success, addDocument.DebugInformation);

        // Search for a document
        const string q = "miller";

		var query = new
		{
			size = 5,
			query = new { multi_match = new { query = q, fields = new[] { "title^2", "director" } } }
		};

        var search = await client.Http.PostAsync<DynamicResponse>($"/{indexName}/_search", d => d.SerializableBody(query));
		Debug.Assert(search.Success, search.DebugInformation);

		foreach (var hit in search.Body.hits.hits) Console.WriteLine($"Search Hit: {hit["_source"]["title"]}");

		// Delete the document
		var deleteDocument = await client.Http.DeleteAsync<DynamicResponse>($"/{indexName}/_doc/{id}");
		Debug.Assert(deleteDocument.Success, deleteDocument.DebugInformation);
		Console.WriteLine($"Delete Document: {deleteDocument.Success}");

		// Delete the index
        var deleteIndex = await client.Http.DeleteAsync<DynamicResponse>($"/{indexName}");
		Debug.Assert(deleteIndex.Success && (bool)deleteIndex.Body.acknowledged, deleteIndex.DebugInformation);
		Console.WriteLine($"Delete Index: {deleteIndex.Success && (bool)deleteIndex.Body.acknowledged}");
    }
}
