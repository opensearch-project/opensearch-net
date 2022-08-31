# OpenSearch.Stack.ArtifactsApi

Library to fetch the url and metadata for released artifacts.

Supports:

1. Snapshots builds
    * `latest-MAJOR` where `MAJOR` is a single integer representing the major you want a snapshot for
    * `latest` latest greatest 

3. Released versions
    * `MAJOR.MINOR.PATH` where `MAJOR` is still supported as defined by the EOL policy of OpenSearch.
    * Note if the version exists but is not yet released it will resolve as a build candidate
    

## Usage

First create an opensearch version

```csharp
var version = OpenSearchVersion.From(versionString);
```

Where `versionString` is a string in the aforementioned formats. `version.ArtifactBuildState` represents the type of version parsed.

```csharp
var version = OpenSearchVersion.From(versionString);
```

To go from a version to an artifact do the following

```csharp
var product = Product.From("opensearch");
var artifact = version.Artifact(product);
```
By first creating a `product` we can then pass that `product` to `version.Artifact` to get an artifact to that product's version.

A product can be a main product such as `opensearch` or a related product e.g

```csharp
var product = Product.From("opensearch-plugins", "analysis-icu");
var artifact = version.Artifact(product);
```

To aid with discoverability we ship with some statics so you do not need to guess the right monikers.

```csharp
Product.OpenSearch;
Product.OpenSearchDashboards;
Product.OpenSearchPlugin(OpenSearchPlugin.AnalysisIcu);
```






