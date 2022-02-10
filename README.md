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
```
<Project>
  ...
  <ItemGroup>
    <ProjectReference Include="..\opensearch-net\src\Osc\Osc.csproj" />
  </ItemGroup>
</Project>
```

**Connecting to a single node**
```csharp
var node = new Uri("http://myserver:9200");
var settings = new ConnectionSettings(node);
var client = new OpenSearchClient(settings);
```

# [OpenSearch.Net](src/OpenSearch.Net)

A low-level, dependency free client that has no opinions how you build and represent your requests and responses.

## Getting Started
Include OpenSearch.Net in your .csproj file.
```
<Project>
  ...
  <ItemGroup>
    <ProjectReference Include="..\opensearch-net\src\OpenSearch.Net\OpenSearch.Net.csproj" />
  </ItemGroup>
</Project>
```

**Connecting**
```csharp
var node = new Uri("http://myserver:9200");
var config = new ConnectionConfiguration(node);
var client = new OpenSearchLowLevelClient(config);
```

## Code of Conduct

This project has adopted the [Amazon Open Source Code of Conduct](CODE_OF_CONDUCT.md). For more information see the [Code of Conduct FAQ](https://aws.github.io/code-of-conduct-faq), or contact [opensource-codeofconduct@amazon.com](mailto:opensource-codeofconduct@amazon.com) with any additional questions or comments.

## License

This project is licensed under the [Apache v2.0 License](LICENSE.txt).

## Copyright

Copyright OpenSearch Contributors. See [NOTICE](./NOTICE.txt) for details.
