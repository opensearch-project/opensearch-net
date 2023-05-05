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
var movies = 'movies';
var books = 'books';
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

As you can see, each bulk operation is comprised of two objects. The first object contains the operation type and the target document's `_index` and `_id`. The second object contains the document's data. As a result, the body of the request above contains six objects for three index actions.

Alternatively, the `bulk` method can accept an array of hashes where each hash represents a single operation. The following code is equivalent to the previous example:

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

We will use this format for the rest of the examples in this guide.

### Creating multiple documents

Similarly, instead of calling the `create` method for each document, you can use the `bulk` API to create multiple documents in a single request. The following code creates three documents in the `movies` index and one in the `books` index:

```cs
var response = await client.BulkAsync(b => b
    .Index<object>(i => i
        .Index(movies)
        .Document(new { Title = "Beauty and the Beast 2", Year = 2030 })
    )
    .Index<object>(i => i
        .Index(movies)
        .Document(new { Title = "Beauty and the Beast 3", Year = 2031 })
    )
    .Index<object>(i => i
        .Index(movies)
        .Document(new { Title = "Beauty and the Beast 4", Year = 2049 })
    )
    .Index<object>(i => i
        .Index(books)
        .Document(new { Title = "The Lion King 2", Year = 1998 })
    )
    .Index<object>(i => i
        .Index(books)
        .Document(new { Title = "The Lion King 2", Year = 1998 })
    ));
```

Note that we specified only the `_index` for the last document in the request body. This is because the `bulk` method accepts an `index` parameter that specifies the default `_index` for all bulk operations in the request body. Moreover, we omit the `_id` for each document and let OpenSearch generate them for us in this example, just like we can with the `create` method.

### Updating multiple documents

```cs
var response = await client.BulkAsync(b => b
    .Update<object>(i => i
        .Index(movies)
        .Id(1)
        .Document(new { Year = 1992 })
    )
    .Update<object>(i => i
        .Index(movies)
        .Id(2)
        .Document(new { Year = 2018 })
    ));
```

Note that the updated data is specified in the `doc` field of the `data` object.

### Deleting multiple documents

```cs
var response = await client.BulkAsync(b => b
    .Delete<object>(i => i
        .Index(movies)
        .Id(1)
    )
    .Delete<object>(i => i
        .Index(movies)
        .Id(2)
    ));
```

### Mix and match operations

You can mix and match the different operations in a single request. The following code creates two documents, updates one document, and deletes another document:

```cs
var response = await client.BulkAsync(b => b
    .Index<object>(i => i
        .Index(movies)
        .Document(new { Title = "Beauty and the Beast 5", Year = 2050 })
    )
    .Index<object>(i => i
        .Index(movies)
        .Document(new { Title = "Beauty and the Beast 6", Year = 2051 })
    )
    .Update<object>(i => i
        .Index(movies)
        .Id(3)
        .Document(new { Year = 2052 })
    )
    .Delete<object>(i => i
        .Index(movies)
        .Id(4)
    ));
```

### Handling errors

The `bulk` API returns an array of responses for each operation in the request body. Each response contains a `status` field that indicates whether the operation was successful or not. If the operation was successful, the `status` field is set to a `2xx` code. Otherwise, the response contains an error message in the `error` field.

The following code shows how to look for errors in the response:

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
        .Document(new { Title = "Beauty and the Beast 2", Year = 2030 })
    )
    .Index<object>(i => i
        .Index(movies)
        .Id(1)
        .Document(new { Title = "Beauty and the Beast 3", Year = 2031 })
    )
    .Index<object>(i => i
        .Index(movies)
        .Id(2)
        .Document(new { Title = "Beauty and the Beast 4", Year = 2049 })
    ));

foreach (var item in response.body["items"]) {
  if(!Enumerable.Range(200,299).Contains(item["create"]["status"])) {
    Console.WriteLine(item["create"]["error"]["reason"]);
  }
}
```

## Cleanup

To clean up the resources created in this guide, delete the `movies` and `books` indices:

```cs
await client.Indices().DeleteAsync([movies, books])
```
