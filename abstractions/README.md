![OpenSearch logo](../OpenSearch.svg)

- [OpenSearch .NET abstractions](#opensearch-net-abstractions)
    - [OpenSearch.OpenSearch.Managed](#opensearchopensearchmanaged)
    - [OpenSearch.OpenSearch.Ephemeral](#opensearchopensearchephemeral)
    - [OpenSearch.OpenSearch.Xunit](#opensearchopensearchxunit)
    - [OpenSearch.Stack.ArtifactsApi](#opensearchstackartifactsapi)

## Welcome!

# OpenSearch .NET abstractions

You've reached the home for several auxiliary projects from the .NET team within OpenSearch.

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
