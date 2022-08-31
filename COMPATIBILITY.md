- [Compatibility with OpenSearch](#compatibility-with-opensearch)
- [Upgrading](#upgrading)

## Compatibility with OpenSearch

The below matrix shows the compatibility of the [`opensearch-net`](https://www.nuget.org/profiles/opensearchproject) with versions of [`OpenSearch`](https://opensearch.org/downloads.html#opensearch).

| OpenSearch Version | Client Version |
|--------------------|----------------|
| 1.x                | 1.0.0, 1.1.0   |
| 2.x                | 1.1.0          |

## Upgrading

Major versions of OpenSearch introduce breaking changes that require careful upgrades of the client. While `opensearch-net` client 1.1.0 works against the latest OpenSearch 1.x, certain deprecated features removed in OpenSearch 2.0 have also been removed from the client. Please refer to the [OpenSearch documentation](https://opensearch.org/docs/latest/clients/index/) for more information.
