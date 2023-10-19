# Search
OpenSearch provides a powerful search API that allows you to search for documents in an index. The search API supports a number of parameters that allow you to customize the search operation. In this guide, we will explore the search API and its parameters.

## Setup
Let's start by creating an index and adding some documents to it:

```csharp
using OpenSearch.Client;
using OpenSearch.Net;

var node = new Uri("https://localhost:9200");
var config = new ConnectionSettings(node)
    .ThrowExceptions()
    .ServerCertificateValidationCallback(CertificateValidations.AllowAll)
    .BasicAuthentication("admin", "admin");
var client = new OpenSearchClient(config);

class Movie
{
    public string Title { get; set; }
    public string Director { get; set; }
    public int Year { get; set; }
    public override string ToString()
    {
        return $"{nameof(Title)}: {Title}, {nameof(Director)}: {Director}, {nameof(Year)}: {Year}";
    }
}

await client.Indices.CreateAsync("movies", c => c
    .Settings(s => s
        .NumberOfShards(1)
        .NumberOfReplicas(0)
    )
    .Map<Movie>(m => m
        .Properties(p => p
            .Text(t => t
                .Name(o => o.Title))
            .Text(t => t
                .Name(o => o.Director))
            .Number(n => n
                .Name(o => o.Year)
                .Type(NumberType.Integer)))));

var movies = new List<Movie>
{
    new Movie { Title = "The Godfather", Director = "Francis Ford Coppola", Year = 1972 },
    new Movie { Title = "The Shawshank Redemption", Director = "Frank Darabont", Year = 1994 },
};

for (var i = 0; i < 10; ++i)
    movies.Add(new Movie { Title = "The Dark Knight " + i, Director = "Christopher Nolan", Year = 2008 + i });

// Index the movies
var indexResponse = await client.BulkAsync(b => b
    .Index("movies")
    .IndexMany(movies)
    .Refresh(Refresh.WaitFor));
Console.WriteLine($"Indexed {indexResponse.Items.Count} movies"); // Indexed 12 movies
```


## Search API

### Basic Search
The search API allows you to search for documents in an index. The following example searches for ALL documents in the `movies` index:

```csharp
var searchResponse = await client.SearchAsync<Movie>(s => s
    .Index("movies")
    .Query(q => q.MatchAll()));
Console.WriteLine(string.Join('\n', searchResponse.Documents));
```

You can also search for documents that match a specific query. The following example searches for documents that match the query `dark knight`:
```csharp
var searchResponse = await client.SearchAsync<Movie>(s => s
    .Index("movies")
    .Query(q => q
        .Match(m => m
            .Field(f => f.Title)
            .Query("dark knight"))));
Console.WriteLine(string.Join('\n', searchResponse.Documents));
```

OpenSearch query DSL allows you to specify complex queries. Check out the [OpenSearch query DSL documentation](https://opensearch.org/docs/latest/query-dsl/) for more information.


### Basic Pagination
The search API allows you to paginate through the search results. The following example searches for documents that match the query `dark knight`, sorted by `year` in in ascending order, and returns the first 2 results after skipping the first 5 results:

```csharp
var query = Query<Movie>.Match(m => m
    .Field(f => f.Title)
    .Query("dark knight"));
var sort = new SortDescriptor<Movie>()
    .Ascending(f => f.Year);
var searchResponse = await client.SearchAsync<Movie>(s => s
    .Index("movies")
    .Query(_ => query)
    .Sort(_ => sort)
    .From(5)
    .Size(2));
Console.WriteLine(string.Join('\n', searchResponse.Documents));
```

With sorting, you can also use the `SearchAfter` parameter to paginate through the search results. Let's say you have already displayed the first page of results, and you want to display the next page. You can use the `SearchAfter` parameter to paginate through the search results. The following example will demonstrate how to get the first 3 pages of results using the search query of the previous example:

```csharp
var page1 = await client.SearchAsync<Movie>(s => s
    .Index("movies")
    .Query(_ => query)
    .Sort(_ => sort)
    .Size(2));
var page2 = await client.SearchAsync<Movie>(s => s
    .Index("movies")
    .Query(_ => query)
    .Sort(_ => sort)
    .Size(2)
    .SearchAfter(page1.Hits.Last().Sorts));
var page3 = await client.SearchAsync<Movie>(s => s
    .Index("movies")
    .Query(_ => query)
    .Sort(_ => sort)
    .Size(2)
    .SearchAfter(page2.Hits.Last().Sorts));
Console.WriteLine(string.Join('\n', page3.Documents));
```

### Pagination with Scroll API
When retrieving large amounts of non-real-time data, you can use the `scroll` parameter to paginate through the search results:

```csharp
var page1 = await client.SearchAsync<Movie>(s => s
    .Index("movies")
    .Query(_ => query)
    .Sort(_ => sort)
    .Size(2)
    .Scroll("1m"));
var page2 = await client.ScrollAsync<Movie>("1m", page1.ScrollId);
var page3 = await client.ScrollAsync<Movie>("1m", page2.ScrollId);
Console.WriteLine(string.Join('\n', page3.Documents));
```

### Pagination with Point in Time
The scroll example above has one weakness: if the index is updated while you are scrolling through the results, they will be paginated inconsistently. To avoid this, you should use the "Point in Time" feature. The following example demonstrates how to use the `point_in_time` and `pit_id` parameters to paginate through the search results:

```csharp
var pitResp = await client.CreatePitAsync("movies", p => p.KeepAlive("1m"));

var page1 = await client.SearchAsync<Movie>(s => s
    .Query(_ => query)
    .Sort(_ => sort)
    .Size(2)
    .PointInTime(p => p.PitId(pitResp.PitId).KeepAlive("1m")));
var page2 = await client.SearchAsync<Movie>(s => s
    .Query(_ => query)
    .Sort(_ => sort)
    .Size(2)
    .PointInTime(p => p.PitId(pitResp.PitId).KeepAlive("1m"))
    .SearchAfter(page1.Hits.Last().Sorts));
var page3 = await client.SearchAsync<Movie>(s => s
    .Query(_ => query)
    .Sort(_ => sort)
    .Size(2)
    .PointInTime(p => p.PitId(pitResp.PitId).KeepAlive("1m"))
    .SearchAfter(page2.Hits.Last().Sorts));

foreach (var doc in page1.Documents.Concat(page2.Documents).Concat(page3.Documents))
{
    Console.WriteLine(doc.Title);
}

await client.DeletePitAsync(p => p.PitIds(pitResp.PitId));
```

Note that a point-in-time is associated with an index or a set of index. So, when performing a search with a point-in-time, you DO NOT specify the index in the search.

## Cleanup
```csharp
await client.Indices.DeleteAsync("movies");
```
