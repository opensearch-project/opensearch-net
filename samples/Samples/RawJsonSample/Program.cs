/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using OpenSearch.Client;
using OpenSearch.Net;
using HttpMethod = OpenSearch.Net.HttpMethod;


public class Program
{
    public static async Task Main(string[] args)
    {
        var node = new Uri("https://localhost:9200");
        var config = new ConnectionSettings(node)
            .ServerCertificateValidationCallback(CertificateValidations.AllowAll)
            .BasicAuthentication("admin", "admin")
            .DisableDirectStreaming();
        
        var client = new OpenSearchClient(config);
        

        // Sample Code: Making Raw JSON Requests
        
       // GET
        var versionResponse = await client.LowLevel.DoRequestAsync<DynamicResponse>(HttpMethod.GET, "/", CancellationToken.None);
        Console.WriteLine(versionResponse.Body["version"]["distribution"].ToString() + " " + versionResponse.Body["version"]["number"].ToString());

        // PUT
        string indexBody = @"
        {{
            ""settings"": {
                ""index"": {
                    ""number_of_shards"": 4
                }
            }
        }}";

        var putResponse = await client.LowLevel.DoRequestAsync<DynamicResponse>(HttpMethod.PUT, "/movies", CancellationToken.None, PostData.String(indexBody));
        
        // POST
        string q = "miller";

        string query = $@"
        {{
            ""size"": 5,
            ""query"": {{
                ""multi_match"": {{
                    ""query"": ""{q}"",
                    ""fields"": [""title^2"", ""director""]
                }}
            }}
        }}";

        var postResponse = await client.LowLevel.DoRequestAsync<DynamicResponse>(HttpMethod.POST, "/movies/_search", CancellationToken.None, PostData.String(query));
        
        // DELETE
        var deleteResponse = await client.LowLevel.DoRequestAsync<DynamicResponse>(HttpMethod.DELETE, "/movies", CancellationToken.None);
    }
}
