# Bulk

In this guide, you'll learn how to use the OpenSearch .NET Client API to perform bulk operations. You'll learn how to index, update, and delete multiple documents in a single request.

## Setup

First, create a client instance with the following code:

```cs
var nodeAddress = new Uri("http://myserver:9200");
var client = new OpenSearchClient(nodeAddress);
```

Next, create an index named `movies` and another named `books` with the default settings:

```cs
var movies = "movies";
var books = "books";
if (!(await client.Indices.ExistsAsync(movies)).Exists) {
  await client.Indices.CreateAsync(movies);
}

if (!(await client.Indices.ExistsAsync(books)).Exists) {
  await client.Indices.CreateAsync(books);
}
```

## Bulk API

The `bulk` API action allows you to perform document operations in a single request. The body of the request is an array of objects that contains the bulk operations and the target documents to index, create, update, or delete.

### Indexing multiple documents

The following code creates two documents in the `movies` index and one document in the `books` index:

```cs
var response = await client.BulkAsync(b => b
    .Index<object>(i => i
        .Index(movies)
        .Id(1)
        .Document(new { Title = "Beauty and the Beast", Year = 1991 })
    )
    .Index<object>(i => i
        .Index(movies)
        .Id(2)
        .Document(new { Title = "Beauty and the Beast - Live Action", Year = 2017 })
    )
    .Index<object>(i => i
        .Index(books)
        .Id(1)
        .Document(new { Title = "The Lion King", Year = 1994 })
    ));
```

### Creating multiple documents

Similarly, instead of calling the `create` method for each document, you can use the `bulk` API to create multiple documents in a single request. The following code creates three documents in the `movies` index and one in the `books` index:

```cs
var response = await client.BulkAsync(b => b
    .Index(movies)
    .CreateMany(new[]
    {
        new { Title = "Beauty and the Beast 2", Year = 2030 },
        new { Title = "Beauty and the Beast 3", Year = 2031 },
        new { Title = "Beauty and the Beast 4", Year = 2049 }
    })
    .Create<object>(i => i
        .Index(books)
        .Document(new { Title = "The Lion King 2", Year = 1998 })
    ));
```

Note that we specified only the `_index` for the last document in the request body. This is because the `bulk` method accepts an `index` parameter that specifies the default `_index` for all bulk operations in the request body. Moreover, we omit the `_id` for each document and let OpenSearch generate them for us in this example, just like we can with the `create` method.

### Updating multiple documents

```cs
var response = await client.BulkAsync(b => b
    .Index(movies)
    .Update<object>(i => i
        .Id(1)
        .Doc(new { Year = 1992 })
    )
    .Update<object>(i => i
        .Id(2)
        .Doc(new { Year = 2018 })
    ));
```

### Deleting multiple documents

```cs
var response = await client.BulkAsync(b => b
    .Index(movies)
    .DeleteMany<object>(new long[] { 1, 2 }));
```

### Mix and match operations

You can mix and match the different operations in a single request. The following code creates two documents, updates one document, and deletes another document:

```cs
var response = await client.BulkAsync(b => b
    .Index(movies)
    .CreateMany(new[]
    {
        new { Title = "Beauty and the Beast 5", Year = 2050 },
        new { Title = "Beauty and the Beast 6", Year = 2051 }
    })
    .Update<object>(i => i
        .Id(3)
        .Doc(new { Year = 2052 })
    )
    .Delete<object>(i => i.Id(4)));
```

### Handling errors

The `bulk` API returns an array of responses for each operation in the request body. Each response contains a `status` field that indicates whether the operation was successful or not. If the operation was successful, the `status` field is set to a `2xx` code. Otherwise, the response contains an error message in the `error` field.

The following code shows how to look for errors in the response:

```cs
var response = await client.BulkAsync(b => b
    .Index(movies)
    .Create<object>(i => i
        .Id(1)
        .Document(new { Title = "Beauty and the Beast", Year = 1991 })
    )
    .Create<object>(i => i
        .Id(2)
        .Document(new { Title = "Beauty and the Beast 2", Year = 2030 })
    )
    .Create<object>(i => i // document already exists error
        .Id(1)
        .Document(new { Title = "Beauty and the Beast 3", Year = 2031 })
    )
    .Create<object>(i => i // document already exists error
        .Id(2)
        .Document(new { Title = "Beauty and the Beast 4", Year = 2049 })
    ));

foreach (var item in response.ItemsWithErrors) {
    Console.WriteLine(item.Error.Reason);
}
```

## BulkAll

The `bulk` API sends a single request. When you need to index a large or streamed set of documents, the `BulkAll` helper partitions the documents into batches, sends each batch to `_bulk`, and transparently retries transient failures. It returns an observable you subscribe to:

```cs
var documents = GetDocuments(); // IEnumerable<MyDocument>, ideally lazily evaluated

var observable = client.BulkAll(documents, b => b
    .Index(movies)
    .Size(1000)                 // documents per batch
    .MaxDegreeOfParallelism(4)  // batches in flight
    .BackOffRetries(2)          // retries per batch on HTTP 429
    .BackOffTime(TimeSpan.FromSeconds(5)));

observable.Wait(TimeSpan.FromMinutes(15), response =>
{
    // called once per successful batch
    Console.WriteLine($"Indexed page {response.Page}");
});
```

### Setting the document `_id`

By default `BulkAll` infers each document's `_id` the same way the rest of the client does. This inference is unchanged since the fork from Elasticsearch and resolves in the following order:

1. A property named `Id` on the document type is used as the `_id`:

   ```cs
   public class Person
   {
       public string Id { get; set; }
       public string FirstName { get; set; }
       public string LastName { get; set; }
   }
   ```

2. A different property can be configured as the id on `ConnectionSettings`:

   ```cs
   var settings = new ConnectionSettings()
       .DefaultMappingFor<Person>(m => m.IdProperty(p => p.FirstName));
   ```

3. The `[OpenSearchType]` attribute can specify the id property on the type itself:

   ```cs
   [OpenSearchType(IdProperty = nameof(Person.LastName))]
   public class Person
   {
       public string FirstName { get; set; }
       public string LastName { get; set; }
   }
   ```

If you only need to set the `_id` from the document for a particular bulk operation — without changing the type or the global mapping — use `DocumentId`. The function can return any string, whether a single field or a value computed from the document:

```cs
var observable = client.BulkAll(documents, b => b
    .Index(movies)
    .DocumentId(d => d.FirstName)); // each document is indexed with _id = d.FirstName
```

`DocumentId` applies to the default bulk operation. If you supply your own `BufferToBulk` callback you take complete control of how each batch is translated into bulk operations, so `DocumentId` is ignored and you set the `_id` yourself:

```cs
var observable = client.BulkAll(documents, b => b
    .Index(movies)
    .BufferToBulk((descriptor, buffer) =>
        descriptor.IndexMany(buffer, (op, document) => op.Id(document.FirstName))));
```

## Cleanup

To clean up the resources created in this guide, delete the `movies` and `books` indices:

```cs
await client.Indices.DeleteAsync(new[] { movies, books });
```
