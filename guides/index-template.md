# Index Template
Index templates allow you to define default settings, mappings, and aliases for one or more indices during their creation. This guide will teach you how to create index templates and apply them to indices using the OpenSearch .NET client.

## Setup
**This guide is designed specifically for the OpenSearch.Net low-level client. At the time this guide was written, none of the methods referenced in the examples below exist yet in the high-level OpenSearch.Client. These examples can be used as a model for the time at which the high-level client implementations are being developed.**


Assuming you have OpenSearch running locally on port 9200, you can create a client instance with the following code:

```csharp
using OpenSearch.Client;
using OpenSearch.Net;

var node = new Uri("https://localhost:9200");
var config = new ConnectionConfiguration(node)
    .ServerCertificateValidationCallback(CertificateValidations.AllowAll)
    .BasicAuthentication("admin", "admin");
var client = new OpenSearchLowLevelClient(config);;
```


# Index Template API Actions


## Create an Index Template
You can create an index template to define default settings and mappings for indices of certain patterns. The following example creates an index template named `books` with default settings and mappings for indices of the `books-*` pattern:

```csharp
var createResponse = client.Indices.PutTemplateForAll<StringResponse>("books", PostData.String(@"{
    ""index_patterns"": [""books-*""],
    ""settings"": {
        ""number_of_shards"": 3,
        ""number_of_replicas"": 0
    },
    ""mappings"": {
        ""properties"": {
            ""title"": { ""type"": ""text"" },
            ""author"": { ""type"": ""text"" },
            ""published_on"": { ""type"": ""date"" },
            ""pages"": { ""type"": ""integer"" }
        }
    }
}")).Body;

Console.WriteLine($"Create response: {createResponse}") // Create response: {"acknowledged":true}
```

Now, when you create an index that matches the `books-*` pattern, OpenSearch will automatically apply the template's settings and mappings to the index. Let's create an index named books-nonfiction and verify that its settings and mappings match those of the template:

```csharp
var createResponse = client.Indices.Create<StringResponse>("books-nonfiction", PostData.String(@"{
    ""settings"": {
        ""number_of_shards"": 3,
        ""number_of_replicas"": 0
    },
    ""mappings"": {
        ""properties"": {
            ""title"": { ""type"": ""text"" },
            ""author"": { ""type"": ""text"" },
            ""published_on"": { ""type"": ""date"" },
            ""pages"": { ""type"": ""integer"" }
        }
    }
}")).Body;

Console.WriteLine($"Create response: {createResponse}"); // Create response: {"acknowledged":true,"shards_acknowledged":true,"index":"books-nonfiction"}
```


## Multiple Index Templates
If multiple index templates match the index's name, OpenSearch will apply the template with the highest `priority`. The following example creates two index templates named `books-*` and `books-fiction-*` with different settings:

```csharp
var createResponseOne = client.Indices.PutTemplateV2ForAll<StringResponse>("books", PostData.String(@"{
    ""index_patterns"": [""books-*""],
    // ""priority"": 1,
    ""settings"": {
        ""number_of_shards"": 3,
        ""number_of_replicas"": 0
    }
}")).Body;

Console.WriteLine($"Create response: {createResponseOne}"); // Create response: {"acknowledged":true}

var createResponseTwo = client.Indices.PutTemplateV2ForAll<StringResponse>("books", PostData.String(@"{
    ""index_patterns"": [""books-fiction-*""],
    ""priority"": 2,
    ""settings"": {
        ""number_of_shards"": 3,
        ""number_of_replicas"": 0
    }
}")).Body;

Console.WriteLine($"Create response: {createResponseTwo}"); // Create response: {"acknowledged":true}
```

When we create an index named `books-fiction-romance`, OpenSearch will apply the `books-fiction-*` template's settings to the index:

```csharp  
var createResponse = client.Indices.Create<StringResponse>("books-fiction-romance", PostData.String(@"{
    ""settings"": {
        ""number_of_shards"": 3,
        ""number_of_replicas"": 0
    },
    ""mappings"": {
        ""properties"": {
            ""title"": { ""type"": ""text"" },
            ""author"": { ""type"": ""text"" },
            ""published_on"": { ""type"": ""date"" },
            ""pages"": { ""type"": ""integer"" }
        }
    }
}")).Body;

Console.WriteLine($"Create response: {createResponse}") // Create response: {"acknowledged":true,"shards_acknowledged":true,"index":"books-fiction-romance"}
```


# Composable Index Templates
Composable index templates are a new type of index template that allow you to define multiple component templates and compose them into a final template. The following example creates a component template named `books_mappings` with default mappings for indices of the `books-*` and `books-fiction-*` patterns:

```csharp
var createResponse = client.Indices.PutComponentTemplate<StringResponse>("books_mappings", PostData.String(@"{
    ""index_patterns"": [""books-*"", ""books-fiction-*""],
    ""mappings"": {
        ""properties"": {
            ""title"": { ""type"": ""text"" },
            ""author"": { ""type"": ""text"" },
            ""published_on"": { ""type"": ""date"" },
            ""pages"": { ""type"": ""integer"" }
        }
    }
}")).Body;

Console.WriteLine($"Create response: {createResponse}"); // Create response: {"acknowledged":true}

// book-*
var createResponse = client.Indices.PutTemplateV2ForAll<StringResponse>("books", PostData.String(@"{
    ""index_patterns"": [""books-*""],
    ""template"": {
        ""settings"": {
            ""number_of_shards"": 3,
            ""number_of_replicas"": 0
        }
    },
    ""priority"": 1
}")).Body;

Console.WriteLine($"Create response: {createResponse}"); // Create response: {"acknowledged":true}

var createResponseTwo = client.Indices.PutTemplateV2ForAll<StringResponse>("books", PostData.String(@"{
    ""index_patterns"": [""books-fiction-*""],
    ""template"": {
        ""settings"": {
            ""number_of_shards"": 3,
            ""number_of_replicas"": 0
        }
    },
    ""priority"": 2
}")).Body;

Console.WriteLine($"Create response: {createResponseTwo}"); // Create response: {"acknowledged":true}
```


```csharp
var createResponse = client.Indices.Create<StringResponse>("books-fiction-horror", PostData.String(@"{
    ""settings"": {
        ""number_of_shards"": 3,
        ""number_of_replicas"": 0
    },
    ""mappings"": {
        ""properties"": {
            ""title"": { ""type"": ""text"" },
            ""author"": { ""type"": ""text"" },
            ""published_on"": { ""type"": ""date"" },
            ""pages"": { ""type"": ""integer"" }
        }
    }
}")).Body;

Console.WriteLine($"Create response: {createResponse}") // Create response: {"acknowledged":true,"shards_acknowledged":true,"index":"books-fiction-horror"}
```


# Composable Index Templates
Composable index templates are a new type of index template that allow you to define multiple component templates and compose them into a final template. The following example creates a component template named books_mappings with default mappings for indices of the `books-*` and `books-fiction-*` patterns:

```csharp
var componentTemplateCreateResponse = cluster.PutComponentTemplate<StringResponse>("books_mappings", PostData.String(@"{
            ""template"": {
                ""mappings"": {
                    ""properties"": {
                        ""title"": { ""type"": ""text"" },
                        ""author"": { ""type"": ""text"" },
                        ""published_on"": { ""type"": ""date"" },
                        ""pages"": { ""type"": ""integer"" }
                    }
                }
            }
        }")).Body;
        
        Console.WriteLine($"Create response: {componentTemplateCreateResponse}"); // Create response: {"acknowledged":true}

        var createResponse = client.Indices.PutTemplateV2ForAll<StringResponse>("books", PostData.String(@"{
            ""index_patterns"": [""books-*""],
            ""composed_of"": [""books_mappings""],
            ""template"": {
                ""settings"": {
                    ""number_of_shards"": 3,
                    ""number_of_replicas"": 0
                }
            },
            ""priority"": 1,
            ""_meta"": {
                ""description"": ""Using component template books_mappings""
            }
        }")).Body;

        Console.WriteLine($"Create response: {createResponse}"); // Create response: {"acknowledged":true}

        var createResponseTwo = client.Indices.PutTemplateV2ForAll<StringResponse>("books", PostData.String(@"{
            ""index_patterns"": [""books-fiction-*""],
            ""composed_of"": [""books_mappings""],
            ""template"": {
                ""settings"": {
                    ""number_of_shards"": 3,
                    ""number_of_replicas"": 0
                }
            },
            ""priority"": 2,
            ""_meta"": {
                ""description"": ""Using component template books_mappings""
            }
        }")).Body;

        Console.WriteLine($"Create response: {createResponseTwo}"); // Create response: {"acknowledged":true}
```

When we create an index named `books-fiction-horror`, OpenSearch will apply the `books-fiction-*` template's settings, and `books_mappings` template mappings to the index:

```csharp
var createResponse = client.Indices.Create<StringResponse>("books-fiction-horror", PostData.String(@"{
            ""settings"": {
                ""number_of_shards"": 3,
                ""number_of_replicas"": 0
            },
            ""mappings"": {
                ""properties"": {
                    ""title"": { ""type"": ""text"" },
                    ""author"": { ""type"": ""text"" },
                    ""published_on"": { ""type"": ""date"" },
                    ""pages"": { ""type"": ""integer"" }
                }
            }
        }")).Body;

        Console.WriteLine($"Create response: {createResponse}"); // Create response: {"acknowledged":true,"shards_acknowledged":true,"index":"books-fiction-horror"}
```

# Get an Index Template
You can get an index template with the `get_index_template` API action. The following example gets the `books` index template:

```csharp
var getResponse = client.Indices.GetTemplateForAll<StringResponse>("books").Body;

Console.WriteLine($"Get response: {getResponse}"); // Get response: {"books":{"order":0,"index_patterns":["books-*"],"settings":{"index":{"number_of_shards":"3","number_of_replicas":"0"}},"mappings":{},"aliases":{}}}
```

# Delete an Index Template
You can delete an index template with the `delete_index_template` API action. The following example deletes the `books` index template:

```csharp
var deleteResponse = client.Indices.DeleteTemplateForAll<StringResponse>("books").Body;

Console.WriteLine($"Delete response: {deleteResponse}"); // Delete response: {"acknowledged":true}
```


# Cleanup
You can delete the index we created earlier with the following code:

```csharp
// delete indices
var deleteIndexResponse = client.Indices.Delete<StringResponse>("books-fiction-horror").Body;

Console.WriteLine($"Delete index response: {deleteIndexResponse}"); // Delete index response: {"acknowledged":true}


// delete index template
var deleteTemplateResponse = client.Indices.DeleteTemplateForAll<StringResponse>("books_mappings").Body;

Console.WriteLine($"Delete template response: {deleteTemplateResponse}"); // Delete template response: {"acknowledged":true}


// delete component template
var deleteTemplateResponse2 = client.Indices.DeleteTemplateForAll<StringResponse>("books").Body;

Console.WriteLine($"Delete template response: {deleteTemplateResponse2}"); // Delete template response: {"acknowledged":true}
```