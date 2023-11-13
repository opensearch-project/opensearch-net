# Index Template
Index templates allow you to define default settings, mappings, and aliases for one or more indices during their creation. This guide will teach you how to create index templates and apply them to indices using the OpenSearch .NET client.

## Setup
**At the time of writing the API methods related to composable templates do not yet exist in the high-level client, as such this guide makes use of their low-level counterparts.**


Assuming you have OpenSearch running locally on port 9200, you can create a client instance with the following code:

```csharp
using OpenSearch.Client;
using OpenSearch.Net;

var node = new Uri("https://localhost:9200");
var config = new ConnectionSettings(node)
    .ServerCertificateValidationCallback(CertificateValidations.AllowAll)
    .BasicAuthentication("admin", "admin");

var client = new OpenSearchClient(config);;
```


## Index Template API Actions


### Create an Index Template
You can create an index template to define default settings and mappings for indices of certain patterns. The following example creates an index template named `books` with default settings and mappings for indices of the `books-*` pattern:

```csharp
client.LowLevel.Indices.PutTemplateV2ForAll<VoidResponse>("books", PostData.Serializable(new
{
    index_patterns = new[] { "books-*" },
    priority = 0,
    template = new
    {
        settings = new
        {
            index = new
            {
                number_of_shards = 3,
                number_of_replicas = 0
            }
        },
        mappings = new
        {
            properties = new
            {
                title = new { type = "text" },
                author = new { type = "text" },
                published_on = new { type = "date" },
                pages = new { type = "integer" }
            }
        }
    }
}));
```

Now, when you create an index that matches the `books-*` pattern, OpenSearch will automatically apply the template's settings and mappings to the index. Let's create an index named `books-nonfiction` and verify that its settings and mappings match those of the template:

```csharp
client.Indices.Create("books-nonfiction");
var getResponse = client.Indices.Get("books-nonfiction");
Console.WriteLine(getResponse.Indices["books-nonfiction"].Mappings.Properties["pages"].Type); // integer
```


### Multiple Index Templates

```csharp
var createResponseOne = client.LowLevel.Indices.PutTemplateV2ForAll<VoidResponse>("books", PostData.Serializable(new
{
    index_patterns = new[] { "books-*" },
    priority = 0,
    template = new
    {
        settings = new
        {
            index = new
            {
                number_of_shards = 3,
                number_of_replicas = 0
            }
        }
    }
}));

client.LowLevel.Indices.PutTemplateV2ForAll<VoidResponse>("books-fiction", PostData.Serializable(new
{
    index_patterns = new[] { "books-fiction-*" },
    priority = 1,  // higher priority than the `books` template
    template = new
    {
        settings = new
        {
            index = new
            {
                number_of_shards = 1,
                number_of_replicas = 1
            }
        }
    }
}));
```

When we create an index named `books-fiction-romance`, OpenSearch will apply the `books-fiction-*` template's settings to the index:

```csharp  
client.Indices.Create("books-fiction-romance");
var getResponse = client.Indices.Get("books-fiction-romance");
Console.WriteLine(getResponse.Indices["books-fiction-romance"].Settings.NumberOfShards); // 1
```


### Composable Index Templates
Composable index templates are a new type of index template that allow you to define multiple component templates and compose them into a final template. The following example creates a component template named `books_mappings` with default mappings for indices of the `books-*` and `books-fiction-*` patterns:

```csharp
// Create a component template
client.Cluster.PutComponentTemplate("books_mappings", ct => ct
    .Template(t => t
        .Map(m => m
            .Properties(p => p
                .Text(tp => tp
                    .Name("title"))
                .Text(tp => tp
                    .Name("author"))
                .Date(d => d
                    .Name("published_on"))
                .Number(n => n
                    .Name("pages")
                    .Type(NumberType.Integer))))));

// Create an index template for "books"
var createBooksTemplateResponse = client.LowLevel.Indices.PutTemplateV2ForAll<VoidResponse>("books", PostData.Serializable(new
{
    index_patterns = new[] { "books-*" },
    composed_of = new[] { "books_mappings" },
    priority = 0,
    template = new
    {
        settings = new
        {
            index = new
            {
                number_of_shards = 3,
                number_of_replicas = 0
            }
        }
    }
}));

// Create an index template for "books-fiction"
var createBooksFictionTemplateResponse = client.LowLevel.Indices.PutTemplateV2ForAll<VoidResponse>("books-fiction", PostData.Serializable(new
{
    index_patterns = new[] { "books-fiction-*" },
    composed_of = new[] { "books_mappings" },
    priority = 1,
    template = new
    {
        settings = new
        {
            index = new
            {
                number_of_shards = 1,
                number_of_replicas = 1
            }
        }
    }
}));
```

When we create an index named `books-fiction-horror`, OpenSearch will apply the `books-fiction-*` template's settings, and `books_mappings` template mappings to the index:

```csharp
client.Indices.Create("books-fiction-horror");
var getResponse = client.Indices.Get("books-fiction-horror");
Console.WriteLine(getResponse.Indices["books-fiction-horror"].Settings.NumberOfShards); // 1 Console.WriteLine(getResponse.Indices["books-fiction-horror"].Mappings.Properties["pages"].Type); // integer 
```

### Get an Index Template
You can get an index template with the `GetTemplateV2ForAll` API action. The following example gets the `books` index template:

```csharp
var getResponse = client.LowLevel.Indices.GetTemplateV2ForAll<StringResponse>("books").Body;
Console.WriteLine($"Get response: {getResponse}"); // Get response: {"books":{"order":0,"index_patterns":["books-*"],"settings":{"index":{"number_of_shards":"3","number_of_replicas":"0"}},"mappings":{},"aliases":{}}}
```

### Delete an Index Template
You can delete an index template with the `DeleteTemplateV2ForAll` API action. The following example deletes the `books` index template:

```csharp
var deleteResponse = client.LowLevel.Indices.DeleteTemplateV2ForAll<VoidResponse>("books");
Console.WriteLine($"Delete response: {deleteResponse}"); // Delete response: {"acknowledged":true}
```


## Cleanup
Let's delete all resources created in this guide:

```csharp
client.Indices.Delete("books-");
client.LowLevel.Indices.DeleteTemplateV2ForAll("books-fiction");
client.Cluster.DeleteComponentTemplate("books_mappings");
```
