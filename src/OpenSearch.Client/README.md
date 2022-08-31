`OpenSearch.Client` is a high level OpenSearch .NET client that maps closely to the original OpenSearch API. All requests and responses are exposed through types, making it easy for users to get up and running quickly.

Under the covers, `OpenSearch.Client` uses the `OpenSearch.Net` low level client to dispatch requests and responses, using and extending many of the types within `OpenSearch.Net`. The low level client is exposed on the high level client through the `.LowLevel` property.
