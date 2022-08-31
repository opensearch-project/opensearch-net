![OpenSearch logo](OpenSearch.svg)

- [OpenSearch .NET abstractions](#opensearch-net-abstractions)
    - [OpenSearch.OpenSearch.Managed](#opensearchopensearchmanaged)
    - [OpenSearch.OpenSearch.Ephemeral](#opensearchopensearchephemeral)
    - [OpenSearch.OpenSearch.Xunit](#opensearchopensearchxunit)
    - [OpenSearch.Stack.ArtifactsApi](#opensearchstackartifactsapi)
- [Project Resources](#project-resources)
- [Code of Conduct](#code-of-conduct)
- [Security](#security)
- [License](#license)
- [Copyright](#copyright)

## Welcome!

# OpenSearch .NET abstractions

You've reached the home repository for several auxiliary projects from the .NET team within OpenSearch.

Current projects:

### [OpenSearch.OpenSearch.Managed](src/OpenSearch.OpenSearch.Managed/README.md)

Provides an easy to start/stop one or more OpenSearch instances that exists on disk already
 
### [OpenSearch.OpenSearch.Ephemeral](src/OpenSearch.OpenSearch.Ephemeral/README.md)
 
Bootstrap (download, install, configure) and run OpenSearch clusters with ease.
Started nodes are run in a new ephemeral location each time they are started and will clean up after they 
are disposed.
 
### [OpenSearch.OpenSearch.Xunit](src/OpenSearch.OpenSearch.Xunit/README.md)

Write integration tests against OpenSearch.
Works with `.NET Core` and `.NET 4.6` and up.

Supports `dotnet xunit`, `dotnet test`, `xunit.console.runner` and tests will be runnable in your IDE through VSTest and jetBrains Rider.

### [OpenSearch.Stack.ArtifactsApi](src/OpenSearch.Stack.ArtifactsApi/README.md)

Library to fetch the url and metadata for released artifacts.

Supports:

1. Snapshots builds
    * `latest-MAJOR` where `MAJOR` is a single integer representing the major you want
    * `latest` latest greatest 

2. Released versions
    * `MAJOR.MINOR.PATH` where `MAJOR` is still supported as defined by the EOL policy of OpenSearch.
    * Note if the version exists but is not yet released it will resolve as a build candidate
    
## Project Resources

* [Project Website](https://opensearch.org/)
* Need help? Try [Forums](https://discuss.opendistrocommunity.dev/)
* [Project Principles](https://opensearch.org/#principles)
* [Contributing to OpenSearch](CONTRIBUTING.md)
* [Maintainer Responsibilities](MAINTAINERS.md)
* [Release Management](RELEASING.md)
* [Admin Responsibilities](ADMINS.md)
* [Security](SECURITY.md)

## Code of Conduct

This project has adopted the [Amazon Open Source Code of Conduct](CODE_OF_CONDUCT.md). For more information see the [Code of Conduct FAQ](https://aws.github.io/code-of-conduct-faq), or contact [opensource-codeofconduct@amazon.com](mailto:opensource-codeofconduct@amazon.com) with any additional questions or comments.

## Security
If you discover a potential security issue in this project we ask that you notify AWS/Amazon Security via our [vulnerability reporting page](http://aws.amazon.com/security/vulnerability-reporting/) or directly via email to aws-security@amazon.com. Please do **not** create a public GitHub issue.

## License

This project is licensed under the [Apache v2.0 License](LICENSE.txt).

## Copyright

Copyright OpenSearch Contributors. See [NOTICE](NOTICE.txt) for details.
