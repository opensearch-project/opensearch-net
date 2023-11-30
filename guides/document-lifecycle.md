# Document Lifecycle
This guide covers OpenSearch .NET Client API actions for Document Lifecycle. You'll learn how to create, read, update, and delete documents in your OpenSearch cluster. Whether you're new to OpenSearch or an experienced user, this guide provides the information you need to manage your document lifecycle effectively.

## Setup
Assuming you have OpenSearch running locally on port 9200, you can create a client instance with the following code:
```csharp
var node = new Uri("https://localhost:9200");
var config = new ConnectionSettings(node)
    .ServerCertificateValidationCallback(CertificateValidations.AllowAll)
    .BasicAuthentication("admin", "admin")
    .DisableDirectStreaming();
var client = new OpenSearchClient(config);

class Movie
{
    public string Title { get; set; }
    public string Director { get; set; }
    public int? Year { get; set; }
    public override string ToString()
    {
        return $"{nameof(Title)}: {Title}, {nameof(Director)}: {Director}, {nameof(Year)}: {Year}";
    }
}
```

Next, create an index named `movies` with the default settings:
```csharp
var createIndexResponse = client.Indices.Create("movies", c => c
    .Map<Movie>(m => m
        .Properties(p => p
            .Text(t => t
                .Name(o => o.Title))
            .Text(t => t
                .Name(o => o.Director))
            .Number(n => n
                .Name(o => o.Year)
                .Type(NumberType.Integer)))));
Debug.Assert(createIndexResponse.IsValid, createIndexResponse.DebugInformation);
```


## Document API Actions

### Create a new document with specified ID
To create a new document, use the `Create` or `Index` API action. The following code creates two new documents with IDs of `1` and `2`:
```csharp
var createResponse = client.Create(new Movie
{
    Title = "The Godfather",
    Director = "Francis Ford Coppola",
    Year = 1972
}, i => i.Index("movies").Id(1));
Debug.Assert(createResponse.IsValid, createResponse.DebugInformation);

var indexResponse = client.Index(new Movie
{
    Title = "The Godfather: Part II",
    Director = "Francis Ford Coppola",
    Year = 1974
}, i => i.Index("movies").Id(2));
Debug.Assert(indexResponse.IsValid, indexResponse.DebugInformation);
```

Note that the `Create` action is NOT idempotent. If you try to create a document with an ID that already exists, the request will fail:
```csharp
var createResponse2 = client.Create(new Movie
{
    Title = "The Godfather: Part II",
    Director = "Francis Ford Coppola",
    Year = 1974
}, i => i.Index("movies").Id(2));
Debug.Assert(createResponse2.IsValid, createResponse2.DebugInformation); // Will fail because the document with ID 2 already exists
```

The `Index` action, on the other hand, is idempotent. If you try to index a document with an existing ID, the request will succeed and overwrite the existing document. Note that no new document will be created in this case. You can think of the `Index` action as an upsert:

```csharp
var indexResponse2 = client.Index(new Movie
{
    Title = "The Godfather: Part III",
    Director = "Francis Ford Coppola",
    Year = 1974
}, i => i.Index("movies").Id(2));
Debug.Assert(indexResponse2.IsValid, indexResponse2.DebugInformation);

// Succeeds and overwrites the existing document
var indexResponse3 = client.Index(new Movie
{
    Title = "The Godfather: Part IV",
    Director = "Francis Ford Coppola",
    Year = 1974
}, i => i.Index("movies").Id(2));
Debug.Assert(indexResponse3.IsValid, indexResponse3.DebugInformation);
```


### Create a new document with auto-generated ID
You can also create a new document with an auto-generated ID by omitting the `Id` parameter. The following code creates documents with an auto-generated IDs in the `movies` index:
```csharp
var indexResponse4 = client.Index(new Movie
{
    Title = "The Godfather: Part V",
    Director = "Francis Ford Coppola",
    Year = 1974
}, i => i.Index("movies"));
Debug.Assert(indexResponse4.IsValid, indexResponse4.DebugInformation);
Console.WriteLine("Auto generated id: " + indexResponse4.Id); // Auto generated id: NwLwaYsBxGGvdhiDoN5-
```

### Get a document
To get a document, use the `Get` API action. The following code gets the document with ID `1` from the `movies` index:
```csharp
var getResponse = client.Get<Movie>(1, g => g.Index("movies"));
Console.WriteLine(getResponse.Source);
// -> Title: The Godfather, Director: Francis Ford Coppola, Year: 1972
```

You can also use `SourceIncludes` and `SourceExcludes` parameters to specify which fields to include or exclude in the response:
```csharp
var getResponse2 = client.Get<Movie>(1, g => g
    .Index("movies")
    .SourceIncludes(m => m.Title));
Console.WriteLine(getResponse2.Source);
// -> Title: The Godfather, Director: , Year:

var getResponse3 = client.Get<Movie>(1, g => g
    .Index("movies")
    .SourceExcludes(m => m.Title));
Console.WriteLine(getResponse3.Source);
// -> Title: , Director: Francis Ford Coppola, Year: 1972
```


### Get multiple documents
To get multiple documents, use the `MultiGet` API action or the `GetMany` helper. The following code gets the documents with IDs `1` and `2` from the `movies` index:
```csharp
var mgIds = new long[] { 1, 2 };

var multiGetResponse = client.MultiGet(mg => mg
    .GetMany<Movie>(mgIds, (g, id) => g.Index("movies")));
Console.WriteLine(string.Join('\n', multiGetResponse.SourceMany<Movie>(mgIds)));

// OR:

var multiGetHits = client.GetMany<Movie>(mgIds, "movies");
Console.WriteLine(string.Join('\n', multiGetHits.Select(h => h.Source)));

// -> Title: The Godfather, Director: Francis Ford Coppola, Year: 1972
// -> Title: The Godfather: Part IV, Director: Francis Ford Coppola, Year: 1974
```

### Check if a document exists
To check if a document exists, use the `DocumentExists` API action. The following code checks if the document with ID `1` exists in the `movies` index:
```csharp
var existsResponse = client.DocumentExists<Movie>(1, d => d.Index("movies"));
Debug.Assert(existsResponse.Exists, existsResponse.DebugInformation);
```


### Update a document
To update a document, use the `Update` API action. The following code updates the `Year` field of the document with ID `1` in the `movies`  index:
```csharp
var updateResponse = client.Update<Movie>(1, u => u
    .Index("movies")
    .Doc(new Movie { Year = 2023 }));
Debug.Assert(updateResponse.IsValid, updateResponse.DebugInformation);
Console.WriteLine(client.Get<Movie>(1, g => g.Index("movies")).Source);
// -> Title: The Godfather, Director: Francis Ford Coppola, Year: 2023
```


Alternatively, you can use the `Script` parameter to update a document using a script. The following code increments the `Year` field of the document with ID `1` by 5 using painless script, the default scripting language in OpenSearch:
```csharp
var updateResponse2 = client.Update<Movie>(1, u => u
    .Index("movies")
    .Script(s => s
        .Source("ctx._source.year += params.count")
        .Params(p => p
            .Add("count", 5))));
Debug.Assert(updateResponse2.IsValid, updateResponse2.DebugInformation);
Console.WriteLine(client.Get<Movie>(1, g => g.Index("movies")).Source);
// -> Title: The Godfather, Director: Francis Ford Coppola, Year: 2028
```

Note that while both `Update` and `Index` actions perform updates, they are not the same. The `Update` action is a partial update, while the `Index` action is a full update. The `Update` action only updates the fields that are specified in the request body, while the `Index` action overwrites the entire document with the new document.


### Update multiple documents by query
To update documents that match a query, use the `UpdateByQuery` API action. The following code decreases the `Year` field of all documents with `Year` greater than 2023:
```csharp
var updateByQueryResponse = client.UpdateByQuery<Movie>(u => u
    .Index("movies")
    .Query(q => q
        .Range(r => r
            .Field(f => f.Year)
            .GreaterThan(2023)
        )
    )
    .Script(s => s
        .Source("ctx._source.year -= params.count")
        .Params(p => p
            .Add("count", 1)))
);
Debug.Assert(updateByQueryResponse.IsValid && updateByQueryResponse.Updated == 1, updateByQueryResponse.DebugInformation);
Console.WriteLine(client.Get<Movie>(1, g => g.Index("movies")).Source);
// -> Title: The Godfather, Director: Francis Ford Coppola, Year: 2027
```


### Delete a document
To delete a document, use the `Delete` API action. The following code deletes the document with ID `1`: 
```csharp
var deleteResponse = client.Delete<Movie>(1, d => d.Index("movies"));
Debug.Assert(deleteResponse.IsValid, deleteResponse.DebugInformation);
```


### Delete multiple documents by query
To delete documents that match a query, use the `DeleteByQuery` API action. The following code deletes all documents with `Year` greater than 1965:
```csharp
var deleteByQueryResponse = client.DeleteByQuery<Movie>(d => d
    .Index("movies")
    .Query(q => q
        .Range(r => r
            .Field(f => f.Year)
            .GreaterThan(1965)
        )
    )
);
Debug.Assert(
    deleteByQueryResponse.IsValid && deleteByQueryResponse.Deleted == 2,
    deleteByQueryResponse.DebugInformation);
```


## Cleanup
To clean up the resources created in this guide, delete the `movies` index:
```csharp
var deleteIndexResponse = client.Indices.Delete("movies");
Debug.Assert(deleteIndexResponse.IsValid, deleteIndexResponse.DebugInformation);
```
