- [Making Raw JSON REST Requests](#making-raw-json-rest-requests)
  - [HTTP Methods](#http-methods)
    - [GET](#get)
    - [PUT](#put)
    - [POST](#post)
    - [DELETE](#delete)
  - [Using Different Types Of PostData](#using-different-types-of-postdata)
    - [PostData.String](#postdatastring)
    - [PostData.Bytes](#postdatabytes)
    - [PostData.Serializable](#postdataserializable)
    - [PostData.MultiJson](#postdatamultijson)

# Making Raw JSON REST Requests
The OpenSearch client implements many high-level REST DSLs that invoke OpenSearch APIs. However you may find yourself in a situation that requires you to invoke an API that is not supported by the client. You can use `client.LowLevel.DoRequest` to do so. See [samples/Samples/RawJson/RawJsonSample.cs](../samples/Samples/RawJson/RawJsonSample.cs) for a complete working sample.

## HTTP Methods

### GET
The following example returns the server version information via `GET /`.

```csharp
var info = await client.LowLevel.DoRequestAsync<DynamicResponse>(HttpMethod.GET, "/", CancellationToken.None);
Console.WriteLine($"Welcome to {info.Body.version.distribution} {info.Body.version.number}!");
```

### PUT
The following example creates an index.

```csharp
var indexBody = new { settings = new { index = new { number_of_shards = 4 } } };

var createIndex = await client.LowLevel.DoRequestAsync<DynamicResponse>(HttpMethod.PUT, "/movies", CancellationToken.None, PostData.Serializable(indexBody));
Debug.Assert(createIndex.Success && (bool)createIndex.Body.acknowledged, createIndex.DebugInformation);
```

### POST
The following example searches for a document.

```csharp
const string q = "miller";

var query = new
{
	size = 5,
	query = new { multi_match = new { query = q, fields = new[] { "title^2", "director" } } }
};

var search = await client.LowLevel.DoRequestAsync<DynamicResponse>(HttpMethod.POST, $"/{indexName}/_search", CancellationToken.None, PostData.Serializable(query));
Debug.Assert(search.Success, search.DebugInformation);

foreach (var hit in search.Body.hits.hits) Console.WriteLine(hit["_source"]["title"]);
```

### DELETE
The following example deletes an index.

```csharp
var deleteDocument = await client.LowLevel.DoRequestAsync<DynamicResponse>(HttpMethod.DELETE, $"/{indexName}/_doc/{id}", CancellationToken.None);
Debug.Assert(deleteDocument.Success, deleteDocument.DebugInformation);
```

## Using Different Types Of PostData
The OpenSearch .NET client provides a `PostData` class that is used to provide the request body for a request. The `PostData` class has several static methods that can be used to create a `PostData` object from different types of data.

### PostData.String
The following example shows how to use the `PostData.String` method to create a `PostData` object from a string.

```csharp
string indexBody = @"
{{
    ""settings"": {
        ""index"": {
            ""number_of_shards"": 4
        }
    }
}}";

await client.LowLevel.DoRequestAsync<DynamicResponse>(HttpMethod.PUT, "/movies", CancellationToken.None, PostData.String(indexBody));
```

### PostData.Bytes
The following example shows how to use the `PostData.Bytes` method to create a `PostData` object from a byte array.

```csharp
byte[] indexBody = Encoding.UTF8.GetBytes(@"
{{
    ""settings"": {
        ""index"": {
            ""number_of_shards"": 4
        }
    }
}}");

await client.LowLevel.DoRequestAsync<DynamicResponse>(HttpMethod.PUT, "/movies", CancellationToken.None, PostData.Bytes(indexBody));
```

### PostData.Serializable
The following example shows how to use the `PostData.Serializable` method to create a `PostData` object from a serializable object.

```csharp
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

await client.LowLevel.DoRequestAsync<DynamicResponse>(HttpMethod.PUT, "/movies", CancellationToken.None, PostData.Serializable(indexBody));
```

### PostData.MultiJson
The following example shows how to use the `PostData.MultiJson` method to create a `PostData` object from a collection of serializable objects. 
The `PostData.MultiJson` method is useful when you want to send multiple documents in a bulk request.

```csharp
var bulkBody = new object[]
{ 
    new { index = new { _index = "movies", _id = "1" } },
    new { title = "The Godfather", director = "Francis Ford Coppola", year = 1972 },
    new { index = new { _index = "movies", _id = "2" } },
    new { title = "The Godfather: Part II", director = "Francis Ford Coppola", year = 1974 }
};

await client.LowLevel.DoRequestAsync<DynamicResponse>(HttpMethod.POST, "/_bulk", CancellationToken.None, PostData.MultiJson(bulkBody));
```
