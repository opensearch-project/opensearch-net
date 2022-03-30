![OpenSearch logo](OpenSearch.svg)

OpenSearch .NET Client

- [Welcome!](#welcome)
- [Project Resources](#project-resources)
- [Code of Conduct](#code-of-conduct)
- [License](#license)
- [Copyright](#copyright)

## Welcome!

**opensearch-net** is [a community-driven, open source fork](https://aws.amazon.com/blogs/opensource/introducing-opensearch/) of elasticsearch-net licensed under the [Apache v2.0 License](LICENSE.txt). For more information, see [opensearch.org](https://opensearch.org/).

**OSC** is [a community-driven, open source fork](https://aws.amazon.com/blogs/opensource/introducing-opensearch/) of elasticsearch-net high level client NEST licensed under the [Apache v2.0 License](LICENSE.txt). For more information, see [opensearch.org](https://opensearch.org/).

## Project Resources

* [Project Website](https://opensearch.org/)
* [Downloads](https://opensearch.org/downloads.html).
* [Documentation](https://opensearch.org/docs/)
* Need help? Try [Forums](https://discuss.opendistrocommunity.dev/)
* [Project Principles](https://opensearch.org/#principles)
* [Contributing to OpenSearch](CONTRIBUTING.md)
* [Maintainer Responsibilities](MAINTAINERS.md)
* [Release Management](RELEASING.md)
* [Admin Responsibilities](ADMINS.md)
* [Security](SECURITY.md)

# [OSC](https://github.com/opensearch-project/opensearch-net/tree/main/src/Osc)

OSC is the official high-level .NET client of [OpenSearch](https://github.com/opensearch-project/OpenSearch).

## Getting Started
Include OSC in your .csproj file.
```xml
<Project>
  ...
  <ItemGroup>
    <ProjectReference Include="..\opensearch-net\src\Osc\Osc.csproj" />
  </ItemGroup>
</Project>
```

### Connecting

You can connect to your OpenSearch cluster via a single node, or by specifying multiple nodes using a node pool. Using a node pool has a few advantages over a single node, such as load balancing and cluster failover support.

**Connecting to a single node**
```csharp
var node = new Uri("http://myserver:9200");
var config = new ConnectionConfiguration(node);
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

var pool = new StaticNodePool(nodes);
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

OSC exposes a fluent interface and a [powerful query DSL](https://opensearch.org/docs/latest/opensearch/query-dsl/index/)

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

OSC also includes and exposes the low-level [OpenSearch.Net](https://github.com/opensearch-project/opensearch-net/tree/main/src/OpenSearch.Net) client that you can fall back to in case anything is missing:

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

# [OpenSearch.Net](src/OpenSearch.Net)

A low-level, dependency free client is a just a .Net wrapper for the REST API. It allows you to build and represent your own requests and responses according to you needs.

## Getting Started
Include OpenSearch.Net in your .csproj file.
```xml
<Project>
  ...
  <ItemGroup>
    <ProjectReference Include="..\opensearch-net\src\OpenSearch.Net\OpenSearch.Net.csproj" />
  </ItemGroup>
</Project>
```

### Connecting

Connecting using the low-level client is very similar to how you would connect using OSC. In fact, the connection constructs that OSC use are actually OpenSearch.Net constructs. Thus, single node connections and connection pooling still apply when using OpenSearch.Net.

```csharp
var node = new Uri("http://myserver:9200");
var config = new ConnectionConfiguration(node);
var client = new OpenSearchLowLevelClient(config);
```

Note the main difference here is that we are instantiating an `OpenSearchLowLevelClient` rather than `OpenSearchClient`, and `ConnectionConfiguration` instead of `ConnectionSettings`.

## Code of Conduct

This project has adopted the [Amazon Open Source Code of Conduct](CODE_OF_CONDUCT.md). For more information see the [Code of Conduct FAQ](https://aws.github.io/code-of-conduct-faq), or contact [opensource-codeofconduct@amazon.com](mailto:opensource-codeofconduct@amazon.com) with any additional questions or comments.

## License

This project is licensed under the [Apache v2.0 License](LICENSE.txt).

## Copyright

Copyright OpenSearch Contributors. See [NOTICE](./NOTICE.txt) for details.
