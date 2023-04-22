# Bulk

In this guide, you'll learn how to use the OpenSearch Ruby Client API to perform bulk operations. You'll learn how to index, update, and delete multiple documents in a single request.

## Setup

First, create a client instance with the following code:

```c#
var nodeAddress = new Uri("http://myserver:9200");
var client = new OpenSearchClient(nodeAddress);
```

Next, create an index named `movies` and another named `books` with the default settings:

```c#
movies = 'movies'
books = 'books'
if(!await client.Indices.ExistsAsync(movies)) {
    await client.Indices.CreateAsync(movies);
}

if(!await client.Indices.ExistsAsync(books)) {
    await client.Indices.CreateAsync(books);
}
```

## Bulk API

The `bulk` API action allows you to perform document operations in a single request. The body of the request is an array of objects that contains the bulk operations and the target documents to index, create, update, or delete.

### Indexing multiple documents

The following code creates two documents in the `movies` index and one document in the `books` index:

```c#
client.Bulk<StringResponse>(PostData.MultiJson(
  new object[]
  {
    new { index = new {_index = movies, _id = 1}},
    new { title = "Beauty and the Beast", year = 1991 },
    new { index = new { _index = movies, _id = 2 }},
    new { title = "Beauty and the Beast - Live Action", year = 2017 },
    new { index = new { _index = books, _id = 1 }},
    new { title = 'The Lion King', year = 1994 }
  }
));
```

As you can see, each bulk operation is comprised of two objects. The first object contains the operation type and the target document's `_index` and `_id`. The second object contains the document's data. As a result, the body of the request above contains six objects for three index actions.

Alternatively, the `bulk` method can accept an array of hashes where each hash represents a single operation. The following code is equivalent to the previous example:

```c#
client.Bulk<StringResponse>(PostData.MultiJson(
  new object[]
  {
    new { index = new { _index = movies, _id = 1, data = new { title = "Beauty and the Beast", year = 1991 }}},
    new { index = new { _index = movies, _id = 2, data = new { title = "Beauty and the Beast - Live Action", year = 2017 }}},
    new { index = new { _index = books, _id = 1, data = new { title = "The Lion King", year = 1994 }}}
  }
));
```

We will use this format for the rest of the examples in this guide.

### Creating multiple documents

Similarly, instead of calling the `create` method for each document, you can use the `bulk` API to create multiple documents in a single request. The following code creates three documents in the `movies` index and one in the `books` index:

```c#
client.Bulk<StringResponse>(PostData.MultiJson(
  new object[] {
    new { create = new { data = new { title = "Beauty and the Beast 2", year = 2030 }}},
    new { create = new { data = new { title = "Beauty and the Beast 3", year = 2031 }}},
    new { create = new { data = new { title: "Beauty and the Beast 4", year = 2049 }}},
    new { create = new { _index = books, data = new { title = "The Lion King 2", year = 1998 }}},
    new { create = new { _index = books, data = new { title: "The Lion King 2", year = 1998 }}}
  }
)).Index(movies);
```

Note that we specified only the `_index` for the last document in the request body. This is because the `bulk` method accepts an `index` parameter that specifies the default `_index` for all bulk operations in the request body. Moreover, we omit the `_id` for each document and let OpenSearch generate them for us in this example, just like we can with the `create` method.

### Updating multiple documents

```c#
client.Bulk<StringResponse>(PostData.MultiJson(
  new object[] {
    new { update = new { _id = 1, data = new { doc = new { year = 1992 } } } },
    new { update = new { _id = 2, data = new { doc = new { year = 2018 } } } }
  }
)).Index(movies);
```

Note that the updated data is specified in the `doc` field of the `data` object.

### Deleting multiple documents

```c#
client.Bulk<StringResponse>(PostData.MultiJson(
  new object[] {
    new { delete = new { _id = 1 } },
    new { delete = new { _id = 2 } }
  }
)).Index(movies);
```

### Mix and match operations

You can mix and match the different operations in a single request. The following code creates two documents, updates one document, and deletes another document:

```c#
client.Bulk<StringResponse>(PostData.MultiJson(
  new object[] {
    new { create = new { data = new { title = "Beauty and the Beast 5", year = 2050 } } },
    new { create = new { data = new { title = "Beauty and the Beast 6", year = 2051 } } },
    new { update = new { _id = 3, data = new { doc = new { year = 2052 } } } },
    new { delete = new { _id = 4 } }
  }
)).Index(movies);
```

### Handling errors

The `bulk` API returns an array of responses for each operation in the request body. Each response contains a `status` field that indicates whether the operation was successful or not. If the operation was successful, the `status` field is set to a `2xx` code. Otherwise, the response contains an error message in the `error` field.

The following code shows how to look for errors in the response:

```c#
response = client.Bulk<StringResponse>(PostData.MultiJson(
    new object[] {
      new { create = new { _id = 1, data = new { title = "Beauty and the Beast", year = 1991 } } },
      new { create = new { _id = 2, data = new { title = "Beauty and the Beast 2", year = 2030 } } },
      new { create = new { _id = 1, data = new { title = "Beauty and the Beast 3", year = 2031 } } }, // document already exists error
      new { create = new { _id = 2, data = new { title = "Beauty and the Beast 4", year = 2049 } } }  // document already exists error
    }
)).Index(movies);

foreach (var item in response.body["items"]) {
  if(!Enumerable.Range(200,299).Contains(item["create"]["status"])) {
    Console.WriteLine(item["create"]["error"]["reason"]);
  }
}
```

## Cleanup

To clean up the resources created in this guide, delete the `movies` and `books` indices:

```c#
await client.Indices().DeleteAsync([movies, books])
```
