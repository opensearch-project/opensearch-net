- [Making Raw JSON REST Requests](#making-raw-json-rest-requests)
  - [GET](#get)
  - [PUT](#put)
  - [POST](#post)
  - [DELETE](#delete)

# Making Raw JSON REST Requests
OpenSearch exposes a REST API that you can use to interact with OpenSearch. The OpenSearch .NET client provides a low-level API that allows you to send raw JSON requests to OpenSearch. This API is useful if you want to use a feature that is not yet supported by the OpenSearch .NET client, but it supported by the OpenSearch REST API.

## GET
The following example returns the server version information via `GET /`.

```csharp
var versionResponse = await client.LowLevel.DoRequestAsync<DynamicResponse>(HttpMethod.GET, "/", CancellationToken.None);
Console.WriteLine(versionResponse.Body["version"]["distribution"].ToString(), versionResponse.Body["version"]["number"].ToString()); // Distribution & Version number
```

# PUT
The following example creates an index.

```csharp
    string indexBody = @"
    {{
        ""settings"": {
            ""index"": {
                ""number_of_shards"": 4
            }
        }
    }}";

var putResponse = await client.LowLevel.DoRequestAsync<DynamicResponse>(HttpMethod.PUT, "/movies", CancellationToken.None, PostData.String(indexBody));
Console.WriteLine(putResponse.Body["acknowledged"].ToString());  // true
```

## POST
The following example searches for a document.

```csharp
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
```

# DELETE
The following example deletes an index.

```csharp
var deleteResponse = await client.LowLevel.DoRequestAsync<DynamicResponse>(HttpMethod.DELETE, "/movies", CancellationToken.None);
Console.WriteLine(deleteResponse.Body["acknowledged"].ToString()); // true
```

## Using Different Types Of PostData
The OpenSearch .NET client also provides a `PostData` class that you can use to construct JSON requests. The following example shows how to use `PostData` to create the same request as the previous example, the difference being that the request body is constructed using `PostData.Serializable()` instead of `PostData.String()`.

# PUT
```csharp
string q = "miller";

var indexBody = new
{
    settings = new
    {
        index = new
        {
            number_of_shards = 4
        }
    }
};

var putResponse = await client.LowLevel.DoRequestAsync<DynamicResponse>(HttpMethod.PUT, 
"/movies", CancellationToken.None, PostData.Serializable(indexBody));
Console.WriteLine(putResponse.Body["acknowledged"].ToString());
```

## POST
```csharp
string q = "miller";

var query = new
{
    size = 5,
    query = new
    {
        multi_match = new
        {
            query = q,
            fields = new[] { "title^2", "director" }
        }
    }
};

var postResponse = await client.LowLevel.DoRequestAsync<DynamicResponse>(HttpMethod.POST, "/movies/_search", CancellationToken.None, PostData.Serializable(query));
```

# Sample Code
[Making Raw JSON Requests](/samples/Samples/Program.cs)