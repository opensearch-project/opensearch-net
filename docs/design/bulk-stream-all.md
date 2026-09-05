# Design: BulkAll enhancements for high-throughput ingestion

## Status
**Implemented.** The capabilities originally prototyped as a separate `BulkStreamAll` helper have been
folded into the existing `BulkAll` helper as backward-compatible additions, following review feedback
(#1020). There is a single orchestrator (`BulkAllObservable<T>`) rather than two near-identical ones.

## Problem Statement

Customers consuming high-throughput event streams (Kafka, Kinesis, change feeds) need to bulk-ingest
documents into OpenSearch with retry/backoff for transient failures, backpressure, progress reporting,
and — for change feeds — ordering guarantees for operations that share a document id. The existing
`BulkAll` helper distributed work round-robin (no ordering guarantee) and used fixed-delay retries.

## What was added to `BulkAll`

All additions are opt-in and preserve existing behavior when unset:

- **`DocumentAffinityKey(Func<T,string>)`** — hash-routes documents sharing a key to the same worker.
  Each worker owns a bounded queue drained by a dedicated consumer that dispatches strictly FIFO with at
  most one batch in flight, so on-the-wire ordering is preserved per key. The producer only ever blocks
  on the one worker whose queue is full, so a slow or retrying worker holds up only its own key. When
  unset, dispatch remains round-robin via the shared `ForEachAsync`.
- **`RetryBaseDelay` / `RetryMaxDelay`** — when set, retries use exponential backoff with jitter
  (`min(RetryMaxDelay, RetryBaseDelay * 2^attempt * jitter)`, jitter ∈ [0.75, 1.25]) via `RetryStrategy`.
  When unset, the historical fixed `BackOffTime` delay is used. Retry count is still governed by
  `BackOffRetries`.
- **`WaitForActiveShards(int?)`** — a fluent builder for the pre-existing request property.
- **`TotalDocumentsProcessed`** — a counter on `BulkAllObserver`, and `WorkerIndex` on `BulkAllResponse`
  (meaningful under affinity routing).

`BulkAll` dispatches every batch to `_bulk`. A `UseStreamingEndpoint()` opt-in that routed batches through
`_bulk/stream` was prototyped but removed before merge (#1020): issuing one discrete request/response per
batch does not leverage the streaming endpoint's server-side batching, so it was the same shape as `_bulk`
with extra API surface. See Future Work for what a genuine streaming integration would require.

## Low-level `_bulk/stream` support (PR #935, retained)

`BulkStreamRequest` / `BulkStreamDescriptor` / `BulkStreamResponse` and `client.BulkStream[Async]` remain
the low-level transport for the endpoint. This PR added a `System.Text.Json` `BulkStreamRequestConverter`
(mirroring `BulkRequestConverter`) so the request body serializes correctly under both serializer engines,
and a `BulkStreamResponseBuilder` that splits and aggregates the newline-delimited response.

## Future Work

- **True streaming.** A genuine `_bulk/stream` integration would hold the connection open and write
  documents incrementally, leveraging the endpoint's server-side batching (`batch_size` / `batch_interval`)
  rather than issuing one discrete request/response per client-side batch. This requires low-level transport
  support (persistent connection + incremental writes) that the client does not yet provide; until then a
  batch-per-request wrapper adds no benefit over `_bulk`.
- **`IAsyncEnumerable<T>` source** for the document stream, once true streaming lands.
