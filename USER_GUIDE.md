- [User Guide](#user-guide)
    - [Getting Started](#getting-started)
    - [Connecting](#connecting)
    - [Indexing](#indexing)
    - [Getting a document](#getting-a-document)
    - [Searching for documents](#searching-for-documents)
    - [Falling back to OpenSearch.Net](#falling-back-to-opensearchnet)
  - [OpenSearch.Net](#opensearchnet)
    - [Getting Started](#getting-started-1)
    - [Connecting](#connecting-1)
# User Guide

This user guide specifies how to include and use the .NET client in your application.

### Getting Started

Include OpenSearch.Client in your .csproj file.
```xml
<Project>
  ...
  <ItemGroup>
    <PackageReference Include="OpenSearch.Client" Version="1.0.0" />
  </ItemGroup>
</Project>
```

### Connecting

You can connect to your OpenSearch cluster via a single node, or by specifying multiple nodes using a node pool. Using a node pool has a few advantages over a single node, such as load balancing and cluster failover support.

**Connecting to a single node**
```csharp
var node = new Uri("http://myserver:9200");
var config = new ConnectionSettings(node);
var client = new OpenSearchClient(config);
```

**Connecting to multiple nodes using a connection pool**
```csharp
var nodes = new Uri[]
{
    new Uri("http://myserver1:9200"),
    new Uri("http://myserver2:9200"),
    new Uri("http://myserver3:9200")
};

var pool = new StaticConnectionPool(nodes);
var settings = new ConnectionSettings(pool);
var client = new OpenSearchClient(settings);
```

### Indexing

Indexing a document is as simple as:

```csharp
var tweet = new Tweet
{
    Id = 1,
    User = "kimchy",
    PostDate = new DateTime(2009, 11, 15),
    Message = "Trying out OSC, so far so good?"
};

var indexingResponse = client.Index(tweet, idx => idx.Index("mytweetindex")); //or specify index via settings.DefaultIndex("mytweetindex");
```

All the calls have async variants:

```csharp
var indexingResponseTask = client.IndexAsync(tweet, idx => idx.Index("mytweetindex")); // returns a Task<IndexResponse>

// Or, in an async-context
var indexingResponse = await client.IndexAsync(tweet, idx => idx.Index("mytweetindex")); // awaits a Task<IndexResponse>
```

### Getting a document

```csharp
var getResponse = client.Get<Tweet>(indexingResponse.Id, idx => idx.Index("mytweetindex")); // returns an IGetResponse mapped 1-to-1 with the OpenSearch JSON response
var tweet = getResponse.Source; // the original document
```

### Searching for documents

OpenSearch.Client exposes a fluent interface and a [powerful query DSL](https://opensearch.org/docs/latest/opensearch/query-dsl/index/)

```csharp
var searchResponse = client.Search<Tweet>(s => s
    .Index("mytweetindex") //or specify index via settings.DefaultIndex("mytweetindex");
    .From(0)
    .Size(10)
    .Query(q => q
        .Term(t => t.User, "kimchy") || q
        .Match(mq => mq.Field(f => f.User).Query("osc"))
    )
);
```

As well as an object initializer syntax:

```csharp
var request = new SearchRequest
{
    From = 0,
    Size = 10,
    Query = new TermQuery { Field = "user", Value = "kimchy" } || 
            new MatchQuery { Field = "description", Query = "OSC" }
};

var searchResponse = client.Search<Tweet>(request);
```
### Falling back to OpenSearch.Net

OpenSearch.Client also includes and exposes the low-level [OpenSearch.Net](https://github.com/opensearch-project/opensearch-net/tree/main/src/OpenSearch.Net) client that you can fall back to in case anything is missing:

```csharp
IOpenSearchLowLevelClient lowLevelClient = client.LowLevel;
// Generic parameter of Search<> is the type of .Body on response
var response = lowLevelClient.Search<SearchResponse<Tweet>>("mytweetindex", PostData.Serializable(new
{
    from = 0,
    size = 10,
    fields = new [] {"id", "name"},
    query = new {
        term = new {
            name = new {
                value= "OSC"
            }
        }
    }
}));
```

## [OpenSearch.Net.Auth.AwsSigV4](src/OpenSearch.Net.Auth.AwsSigV4)

An implementation of AWS SigV4 request signing for performing IAM authentication against the managed Amazon OpenSearch Service.
It can be used with both the low-level OpenSearch.Net client as well as the higher-level OpenSearch.Client client.

### Getting Started
Include OpenSearch.Net.Auth.AwsSigV4 along with your preferred client in your .csproj file.
```xml
<Project>
  ...
  <ItemGroup>
    <PackageReference Include="OpenSearch.Client" Version="1.0.0" />
    <PackageReference Include="OpenSearch.Net.Auth.AwsSigV4" Version="1.0.0" />
  </ItemGroup>
</Project>
```

### Connecting
The only wiring required is to use the `AwsSigV4HttpConnection` implementation of `IConnection`.

**With OpenSearch.Client**
```csharp
var endpoint = new Uri("https://example-aaabbbcccddd111222333.us-east-1.es.amazonaws.com");
var connection = new AwsSigV4HttpConnection();
var config = new ConnectionSettings(endpoint, connection);
var client = new OpenSearchClient(config);
```

**With OpenSearch.Net**
```csharp
var endpoint = new Uri("https://example-aaabbbcccddd111222333.us-east-1.es.amazonaws.com");
var connection = new AwsSigV4HttpConnection();
var config = new ConnectionConfiguration(endpoint, connection);
var client = new OpenSearchLowLevelClient(config);
```

### Configuring Region & Credentials
By default, `AwsSigV4HttpConnection` will use the same default logic as the [AWS SDK for .NET](https://docs.aws.amazon.com/sdk-for-net/v3/developer-guide/creds-assign.html) to determine the credentials and region to use.
However, you may explicitly specify one or both to override this logic, for example:

**Explicitly setting the region, but the credentials coming from the environment**
```shell
export AWS_ACCESS_KEY_ID="..."
export AWS_SECRET_ACCESS_KEY="..."
export AWS_SESSION_TOKEN="..."
```
```csharp
var endpoint = new Uri("https://example-aaabbbcccddd111222333.ap-southeast-2.es.amazonaws.com");
var connection = new AwsSigV4HttpConnection(RegionEndpoint.APSoutheast2);
var config = new ConnectionSettings(endpoint, connection);
var client = new OpenSearchClient(config);
```

**Explicitly setting credentials, such as to assume a role, but the region coming from the environment**
```shell
export AWS_REGION="ap-southeast-2"
```
```csharp
var endpoint = new Uri("https://example-aaabbbcccddd111222333.ap-southeast-2.es.amazonaws.com");
var credentials = new AssumeRoleAWSCredentials(
				FallbackCredentialsFactory.GetCredentials(),
				"arn:aws:iam::123456789012:role/my-open-search-ingest-role",
				"my-ingest-application");
var connection = new AwsSigV4HttpConnection(credentials);
var config = new ConnectionSettings(endpoint, connection);
var client = new OpenSearchClient(config);
```

**Explicitly setting credentials and region, such as to assume a role**
```csharp
var endpoint = new Uri("https://example-aaabbbcccddd111222333.ap-southeast-2.es.amazonaws.com");
var credentials = new AssumeRoleAWSCredentials(
				FallbackCredentialsFactory.GetCredentials(),
				"arn:aws:iam::123456789012:role/my-open-search-ingest-role",
				"my-ingest-application");
var connection = new AwsSigV4HttpConnection(credentials, RegionEndpoint.APSoutheast2);
var config = new ConnectionSettings(endpoint, connection);
var client = new OpenSearchClient(config);
```

## [OpenSearch.Net](src/OpenSearch.Net)

A low-level, dependency free client that is a simple .NET wrapper for the REST API. It allows you to build and represent your own requests and responses according to you needs.

### Getting Started
Include OpenSearch.Net in your .csproj file.
```xml
<Project>
  ...
  <ItemGroup>
    <PackageReference Include="OpenSearch.Net" Version="1.0.0" />
  </ItemGroup>
</Project>
```

### Connecting

Connecting using the low-level client is very similar to how you would connect using OpenSearch.Client. In fact, the connection constructs that OpenSearch.Client use are actually OpenSearch.Net constructs. Thus, single node connections and connection pooling still apply when using OpenSearch.Net.

```csharp
var node = new Uri("http://myserver:9200");
var config = new ConnectionConfiguration(node);
var client = new OpenSearchLowLevelClient(config);
```

Note the main difference here is that we are instantiating an `OpenSearchLowLevelClient` rather than `OpenSearchClient`, and `ConnectionConfiguration` instead of `ConnectionSettings`.
