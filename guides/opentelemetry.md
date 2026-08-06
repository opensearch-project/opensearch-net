- [OpenTelemetry Tracing](#opentelemetry-tracing)
  - [Enabling Tracing](#enabling-tracing)
  - [Emitted Spans](#emitted-spans)
  - [Span Attributes](#span-attributes)
  - [Relationship to the Legacy `DiagnosticSource`](#relationship-to-the-legacy-diagnosticsource)

# OpenTelemetry Tracing

The client natively emits [OpenTelemetry](https://opentelemetry.io/) traces for every request it sends to OpenSearch. It does this using [`System.Diagnostics.ActivitySource`](https://learn.microsoft.com/en-us/dotnet/core/diagnostics/distributed-tracing), the standard .NET tracing primitive that the OpenTelemetry SDK observes directly. The client does **not** take a dependency on the OpenTelemetry SDK — it only produces the spans, and any OpenTelemetry-aware backend can collect them.

## Enabling Tracing

Add the client's activity source, by name, to your tracer provider. The source name is exposed as a constant on `OpenSearchClientActivitySource.ActivitySourceName`.

```csharp
using OpenTelemetry;
using OpenTelemetry.Trace;

using var tracerProvider = Sdk.CreateTracerProviderBuilder()
    .AddSource("OpenSearch.Net.RequestPipeline")
    .AddOtlpExporter() // or any other exporter (Jaeger, Zipkin, console, ...)
    .Build();
```

That single `AddSource` call is all that is required. From then on, every request made through either the high-level (`OpenSearch.Client`) or low-level (`OpenSearch.Net`) client automatically produces a span — no per-request code changes are needed.

When no listener is subscribed to the source, the client skips span creation entirely, so there is virtually no overhead if you are not collecting traces.

## Emitted Spans

One span of kind `Client` is emitted per API call to OpenSearch (the call the request pipeline makes on your behalf). The span name is the OpenSearch REST API operation name when it is known (for example `search` or `indices.create`), and falls back to the HTTP method (for example `POST`) for requests that do not map to a named operation, such as raw low-level `DoRequest` calls.

Internal pipeline traffic such as pings and cluster sniffs is not traced through this `ActivitySource`; it continues to be observable only via the legacy `DiagnosticSource` mechanism described below.

The span status is set to `Ok` on a successful response and `Error` when the request fails or throws.

## Span Attributes

Attributes follow the [OpenTelemetry semantic conventions](https://opentelemetry.io/docs/specs/semconv/database/) so that any backend can interpret them without extra configuration.

| Attribute | Example | Notes |
| --- | --- | --- |
| `db.system` | `opensearch` | Always set. |
| `db.operation` | `search` | Set only when the operation name is known. |
| `http.request.method` | `POST` | |
| `http.response.status_code` | `200` | Set once a response is received. |
| `server.address` | `localhost` | Host of the node the request was sent to. |
| `server.port` | `9200` | Port of the node the request was sent to. |
| `url.full` | `http://localhost:9200/my-index/_search` | |

## Relationship to the Legacy `DiagnosticSource`

The client has long supported observing requests through the `DiagnosticSource` / `DiagnosticListener` mechanism (see [`OpenSearch.Net.Diagnostics.DiagnosticSources`](../src/OpenSearch.Net/Diagnostics/DiagnosticSources.cs)). That mechanism is unchanged and remains available for backwards compatibility.

The OpenTelemetry `ActivitySource` described here is a separate, additive mechanism. Prefer it when integrating with OpenTelemetry, since the OpenTelemetry SDK observes `ActivitySource` directly and does not require any glue code to bridge from `DiagnosticListener`.
