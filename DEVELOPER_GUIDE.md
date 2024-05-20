- [Developer Guide](#developer-guide)
  - [Prerequisites](#prerequisites)
  - [Install Docker Image](#install-docker-image)
  - [Running Tests](#running-tests)
  - [Client Code Generator](#client-code-generator)

# Developer Guide

## Prerequisites

The .NET 6.0 SDK is required:

```
$ dotnet --list-sdks
6.0.xxx
```


## Running Tests

The integration tests will download opensearch and run a local cluster for you. To run the full suite of tests, all you need to do is call the below script.

```
.\build.bat
```

```
build.sh
```

## Client Code Generator

OpenSearch publishes an [OpenAPI specification](https://github.com/opensearch-project/opensearch-api-specification/releases/download/main/opensearch-openapi.yaml) in the [opensearch-api-specification](https://github.com/opensearch-project/opensearch-api-specification) repository, which is used to auto-generate the less interesting parts of the client.

```
./build.sh codegen --branch main
```
or
```
./build.bat codegen --branch main
```
