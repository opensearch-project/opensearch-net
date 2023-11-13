/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.Diagnostics;
using OpenSearch.Client;
using OpenSearch.Net;
using HttpMethod = OpenSearch.Net.HttpMethod;

public class Program
{
    public static async Task Main(string[] args)
    {
        var node = new Uri("http://localhost:9200");
        var config = new ConnectionSettings(node)
            .ServerCertificateValidationCallback(CertificateValidations.AllowAll)
            .BasicAuthentication("admin", "admin")
            .DisableDirectStreaming();

        var client = new OpenSearchClient(config);

        var info = await client.LowLevel.DoRequestAsync<DynamicResponse>(HttpMethod.GET, "/", CancellationToken.None);
        Console.WriteLine($"Welcome to {info.Body.version.distribution} {info.Body.version.number}!");

		// Create an index

		const string indexName = "movies";

		var indexBody = new { settings = new { index = new { number_of_shards = 4 } } };

        var createIndex = await client.LowLevel.DoRequestAsync<DynamicResponse>(HttpMethod.PUT, $"/{indexName}", CancellationToken.None, PostData.Serializable(indexBody));
		Debug.Assert(createIndex.Success && (bool)createIndex.Body.acknowledged, createIndex.DebugInformation);

		// Add a document to the index
		var document = new { title = "Moneyball", director = "Bennett Miller", year = 2011};

		const string id = "1";

		var addDocument = await client.LowLevel.DoRequestAsync<DynamicResponse>(HttpMethod.PUT, $"/{indexName}/_doc/{id}", CancellationToken.None, PostData.Serializable(document));
		Debug.Assert(addDocument.Success, addDocument.DebugInformation);

		// Refresh the index
		var refresh = await client.LowLevel.DoRequestAsync<DynamicResponse>(HttpMethod.POST, $"/{indexName}/_refresh", CancellationToken.None);
		Debug.Assert(refresh.Success, refresh.DebugInformation);

        // Search for a document
        const string q = "miller";

		var query = new
		{
			size = 5,
			query = new { multi_match = new { query = q, fields = new[] { "title^2", "director" } } }
		};

        var search = await client.LowLevel.DoRequestAsync<DynamicResponse>(HttpMethod.POST, $"/{indexName}/_search", CancellationToken.None, PostData.Serializable(query));
		Debug.Assert(search.Success, search.DebugInformation);

		foreach (var hit in search.Body.hits.hits) Console.WriteLine(hit["_source"]["title"]);

		// Delete the document
		var deleteDocument = await client.LowLevel.DoRequestAsync<DynamicResponse>(HttpMethod.DELETE, $"/{indexName}/_doc/{id}", CancellationToken.None);
		Debug.Assert(deleteDocument.Success, deleteDocument.DebugInformation);

		// Delete the index
        var deleteIndex = await client.LowLevel.DoRequestAsync<DynamicResponse>(HttpMethod.DELETE, $"/{indexName}", CancellationToken.None);
		Debug.Assert(deleteIndex.Success && (bool)deleteIndex.Body.acknowledged, deleteIndex.DebugInformation);
    }
}
