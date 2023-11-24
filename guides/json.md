- [Making Raw JSON REST Requests](#making-raw-json-rest-requests)
  - [HTTP Methods](#http-methods)
    - [GET](#get)
    - [PUT](#put)
    - [POST](#post)
    - [DELETE](#delete)
  - [Request Bodies](#request-bodies)
    - [String](#string)
    - [Bytes](#bytes)
    - [Serializable](#serializable)
    - [Multi Json](#multi-json)
  - [Response Bodies](#response-bodies)

# Making Raw JSON REST Requests
The OpenSearch client implements many high-level REST DSLs that invoke OpenSearch APIs. However you may find yourself in a situation that requires you to invoke an API that is not supported by the client. You can use the methods defined within `client.Http` or `client.LowLevel.Http` to do so. See [samples/Samples/RawJson/RawJsonHighLevelSample.cs](../samples/Samples/RawJson/RawJsonHighLevelSample.cs) and [samples/Samples/RawJson/RawJsonLowLevelSample.cs](../samples/Samples/RawJson/RawJsonLowLevelSample.cs) for complete working samples. 

Older versions of the client that do not support the `client.Http` namespace can use the `client.LowLevel.DoRequest` method instead.

## HTTP Methods

### GET
The following example returns the server version information via `GET /`.

```csharp
var info = await client.Http.GetAsync<DynamicResponse>("/");
// OR
var info = await client.LowLevel.Http.GetAsync<DynamicResponse>("/");

Console.WriteLine($"Welcome to {info.Body.version.distribution} {info.Body.version.number}!");
```

### HEAD
The following example checks if an index exists via `HEAD /movies`.

```csharp
var indexExists = await client.Http.HeadAsync<VoidResponse>("/movies");
// OR
var indexExists = await client.LowLevel.Http.HeadAsync<VoidResponse>("/movies");

Console.WriteLine($"Index Exists: {indexExists.HttpStatusCode == 200}");
```

### PUT
The following example creates an index.

```csharp
var indexBody = new { settings = new { index = new { number_of_shards = 4 } } };

var createIndex = await client.Http.PutAsync<DynamicResponse>("/movies", d => d.SerializableBody(indexBody));
// OR
var createIndex = await client.LowLevel.Http.PutAsync<DynamicResponse>("/movies", PostData.Serializable(indexBody));

Console.WriteLine($"Create Index: {createIndex.Success && (bool)createIndex.Body.acknowledged}");
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

var search = await client.Http.PostAsync<DynamicResponse>("/movies/_search", d => d.SerializableBody(query));
// OR
var search = await client.LowLevel.Http.PostAsync<DynamicResponse>("/movies/_search", PostData.Serializable(query));

foreach (var hit in search.Body.hits.hits) Console.WriteLine($"Search Hit: {hit["_source"]["title"]}");
```

### DELETE
The following example deletes an index.

```csharp
var deleteIndex = await client.Http.DeleteAsync<DynamicResponse>("/movies");
// OR
var deleteIndex = await client.LowLevel.Http.DeleteAsync<DynamicResponse>("/movies");

Console.WriteLine($"Delete Index: {deleteIndex.Success && (bool)deleteIndex.Body.acknowledged}");
```

## Request Bodies
For the methods that take a request body (PUT/POST/PATCH) it is possible use several different types to specify the body. The high-level methods provided in `OpenSearch.Client` provide overloaded fluent-methods for setting the body. While the lower level methods in `OpenSearch.Net` accept instances of `PostData`.  
The `PostData` class has several static methods that can be used to create a `PostData` object from different types of data.

### String
The following example shows how to pass a string as a request body:

```csharp
string indexBody = @"
{{
    ""settings"": {
        ""index"": {
            ""number_of_shards"": 4
        }
    }
}}";

await client.Http.PutAsync<DynamicResponse>("/movies", d => d.Body(indexBody));
// OR
await client.LowLevel.Http.PutAsync<DynamicResponse>("/movies", PostData.String(indexBody));
```

### Bytes
The following example shows how to pass a byte array as a request body:

```csharp
byte[] indexBody = Encoding.UTF8.GetBytes(@"
{{
    ""settings"": {
        ""index"": {
            ""number_of_shards"": 4
        }
    }
}}");

await client.Http.PutAsync<DynamicResponse>("/movies", d => d.Body(indexBody));
// OR
await client.LowLevel.Http.PutAsync<DynamicResponse>("/movies", PostData.Bytes(indexBody));
```

### Serializable
The following example shows how to pass an object that will be serialized to JSON as a request body:

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

await client.Http.PutAsync<DynamicResponse>("/movies", d => d.SerializableBody(indexBody));
// OR
await client.LowLevel.Http.PutAsync<DynamicResponse>("/movies", PostData.Serializable(indexBody));
```

### Multi JSON
The following example shows how to pass a collection of objects (or strings) that will be serialized to JSON and then newline-delimited as a request body.
This formatting is primarily used when you want to make a bulk request.

```csharp
var bulkBody = new object[]
{ 
    new { index = new { _index = "movies", _id = "1" } },
    new { title = "The Godfather", director = "Francis Ford Coppola", year = 1972 },
    new { index = new { _index = "movies", _id = "2" } },
    new { title = "The Godfather: Part II", director = "Francis Ford Coppola", year = 1974 }
};

await client.Http.PostAsync<DynamicResponse>("/_bulk", d => d.MultiJsonBody(indexBody));
// OR
await client.LowLevel.Http.PostAsync<DynamicResponse>("/_bulk", PostData.MultiJson(indexBody));
```

## Response Bodies
There are a handful of response type implementations that can be used to retrieve the response body. These are specified as the generic argument to the request methods. The content of the body will then be available via the `Body` property of the response object. The following response types are available:

- `VoidResponse`: The response body will not be read. Useful when you only care about the response status code, such as a `HEAD` request to check an index exists.
- `StringResponse`: The response body will be read into a string.
- `BytesResponse`: The response body will be read into a byte array.
- `DynamicResponse`: The response body will be deserialized as JSON into a dynamic object.
