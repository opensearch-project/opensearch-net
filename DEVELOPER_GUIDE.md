  - [Developer Guide](#developer-guide)
  - [Prerequisites](#prerequisites)
  - [Install Docker Image](#install-docker-image)
  - [Running Tests](#running-tests)
  - [Linter](#linter)
  - [Documentation](#documentation)
  - [Client Code Generator](#client-code-generator)

# Developer Guide

## Prerequisites

dotnet 6.0 sdk is required

```
$ dotnet --list-sdks
6.0.xxx
```



## Install Docker Image

Integration tests require [docker](https://opensearch.org/docs/latest/install-and-configure/install-opensearch/docker/).

Run the following commands to install the docker image:

```
docker pull opensearchproject/opensearch:latest
```

Integration tests will auto-start the docker image. To start it manually:

```
docker run -d -p 9200:9200 -p 9600:9600 -e "discovery.type=single-node" opensearchproject/opensearch:latest
```

## Running Tests

Tests require a live instance of OpenSearch running in docker.

If you have one running, the build.bat or build.sh commands will run a full run of unit and integration tests.

```
.\build.bat
```

```
build.sh
```

To run tests in a specific test file.



Note that integration tests require docker to be installed and running, and downloads quite a bit of data from over the internet and hence take few minutes to complete.


## Client Code Generator

OpenSearch publishes an [OpenAPI specification](https://github.com/opensearch-project/opensearch-api-specification/releases/download/main/opensearch-openapi.yaml) in the [opensearch-api-specification](https://github.com/opensearch-project/opensearch-api-specification) repository, which is used to auto-generate the less interesting parts of the client.

```
nox -rs generate
```
