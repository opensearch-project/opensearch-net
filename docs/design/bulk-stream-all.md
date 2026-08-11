# Design: BulkStreamAll — High-Level Streaming Bulk Ingestion Helper

## Status
**Draft** — for review

## Problem Statement

Customers consuming high-throughput event streams (Kafka, Kinesis, change feeds) need to bulk-ingest documents into OpenSearch with:
- Automatic batching (size and byte-count thresholds)
- Retry with exponential backoff for transient failures (429, transport errors)
- Backpressure so producers don't overwhelm the pipeline
- Progress reporting for observability
- Document-ID affinity to guarantee ordering for same-document operations
- Flush-without-close semantics for long-lived reusable instances (Lambda warm starts, etc.)

The existing `BulkAllObservable<T>` partially addresses this but has critical limitations. Customers maintain custom wrappers to work around them.

## Context: What Exists Today

### PR #935 — Low-Level Bulk Stream API (assumes landed)

Adds the `_bulk/stream` endpoint to the client:
- `IBulkStreamRequest` / `BulkStreamRequest` / `BulkStreamDescriptor` — request types holding `BulkOperationsCollection<IBulkOperation>`
- `BulkStreamResponse` — response with `Errors`, `Items`, `ItemsWithErrors`, `Took`
- `BulkStreamRequestFormatter` — ndjson serialization
- URL patterns: `_bulk/stream`, `{index}/_bulk/stream`

### `BulkAllObservable<T>` — Existing High-Level Helper

| Feature | Status | Limitation |
|---------|--------|-----------|
| Batching by count | ✅ Size property | No byte-size threshold |
| Retry with backoff | ✅ BackOffRetries + BackOffTime | Fixed delay, no jitter/exponential |
| Backpressure | ✅ ProducerConsumerBackPressure | Semaphore-only, no channel integration |
| Progress reporting | ✅ IObservable<BulkAllResponse> | Only successful pages, no per-item |
| Document-ID affinity | ❌ | Round-robin across workers |
| Flush without close | ❌ | Dispose is the only drain mechanism |
| IAsyncEnumerable source | ❌ | IEnumerable<T> only, eager .ToList() |
| Uses _bulk/stream | ❌ | Uses standard _bulk endpoint |

## Decision: New Type vs. Extend Existing

**Recommendation: Create a new `BulkStreamAllObservable<T>` alongside the existing `BulkAllObservable<T>`.**

Rationale:
1. **Breaking change avoidance** — `BulkAllObservable<T>` is a public API with users depending on its exact behavior. Changing its internals risks regressions.
2. **Different wire protocol** — The new helper targets `_bulk/stream` (PR #935), which may have different server-side semantics (streaming response, kept-alive connection). Mixing the two muddies the abstraction.
3. **Clean API boundary** — New type can be designed from scratch with `IAsyncEnumerable<T>`, `Channel<T>`, and modern C# patterns without compromising the old type's compatibility.
4. **Migration path** — Keep `BulkAllObservable<T>` for existing users; mark it as legacy in docs. New users adopt `BulkStreamAll`. Eventually deprecate when `_bulk/stream` is universally available.
5. **Naming alignment** — `BulkStream` (PR #935's primitive) → `BulkStreamAll` (high-level orchestrator). Clear layering.

## Architecture

```
┌─────────────────────────────────────────────────────────────────────┐
│                        User Code                                     │
│   IAsyncEnumerable<T> / IEnumerable<T> source                       │
└──────────────────────────┬──────────────────────────────────────────┘
                           │ documents
                           ▼
┌─────────────────────────────────────────────────────────────────────┐
│                   BulkStreamAllObservable<T>                          │
│                                                                      │
│  ┌────────────┐    ┌────────────────────────────────────────────┐   │
│  │  Ingestion │    │         Worker Pool (N workers)             │   │
│  │   Loop     │    │                                             │   │
│  │            │    │  ┌─────────┐ ┌─────────┐ ... ┌─────────┐  │   │
│  │ reads src  │───▶│  │Worker[0]│ │Worker[1]│     │Worker[N]│  │   │
│  │ routes by  │    │  │Channel  │ │Channel  │     │Channel  │  │   │
│  │ doc-ID hash│    │  │  ▼      │ │  ▼      │     │  ▼      │  │   │
│  │            │    │  │ Batch   │ │ Batch   │     │ Batch   │  │   │
│  └────────────┘    │  │ Buffer  │ │ Buffer  │     │ Buffer  │  │   │
│                    │  │  ▼      │ │  ▼      │     │  ▼      │  │   │
│                    │  │BulkStream│ │BulkStream│    │BulkStream│  │   │
│                    │  │ Request │ │ Request │     │ Request │  │   │
│                    │  └─────────┘ └─────────┘     └─────────┘  │   │
│                    └────────────────────────────────────────────┘   │
│                                                                      │
│  ┌─────────────────────────────────────────────────────────────┐    │
│  │              Response Processing & Retry                     │    │
│  │  • Inspect BulkStreamResponse.Items                          │    │
│  │  • Retry 429s with exponential backoff + jitter              │    │
│  │  • Route dropped docs to callback                            │    │
│  │  • Report progress via IObservable<BulkStreamAllResponse>    │    │
│  └─────────────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────────────┘
```

### Key Components

1. **Ingestion Loop** — Reads from the source (`IAsyncEnumerable<T>` or `IEnumerable<T>`), applies the document-ID routing function to select a worker channel, writes the item into the appropriate bounded channel.

2. **Worker Pool** — N workers, each owning a `Channel<T>` (bounded, provides backpressure). Each worker accumulates items into a batch buffer until a flush trigger fires.

3. **Flush Triggers** — A batch is sent when ANY of:
   - Item count reaches `Size` (default 1000)
   - Accumulated serialized byte size reaches `MaxBatchBytes` (default 5MB)
   - `FlushInterval` timer fires (default 5s)
   - Explicit `FlushAsync()` is called

4. **Bulk Dispatch** — Each worker constructs a `BulkStreamRequest`, dispatches via `client.BulkStreamAsync(...)`, and processes the `BulkStreamResponse`.

5. **Retry Engine** — Per-batch retry with:
   - Exponential backoff: `baseDelay * 2^attempt` (default base 1s)
   - Jitter: ±25% randomization to prevent thundering herd
   - Max retries configurable (default 3)
   - Retries only the failed items within a batch (not the whole batch)
   - Predicate-based: user can control what's retryable (default: status 429)

6. **Progress Reporting** — `IObservable<BulkStreamAllResponse>` emitted per successful batch, containing page number, items, retries, and timing.

## API Surface

### Configuration — `IBulkStreamAllRequest<T>`

```csharp
public interface IBulkStreamAllRequest<T> where T : class
{
    // === Source ===
    /// The documents to ingest. Supports lazy evaluation.
    IEnumerable<T> Documents { get; }

    /// Async document source for true streaming (preferred over Documents).
    IAsyncEnumerable<T> DocumentsAsync { get; }

    // === Batching ===
    /// Max documents per bulk request (default 1000)
    int? Size { get; set; }

    /// Max serialized bytes per bulk request (default 5MB). Null = no byte limit.
    long? MaxBatchBytes { get; set; }

    /// Time-based flush interval. Null = no timer-based flush.
    TimeSpan? FlushInterval { get; set; }

    // === Parallelism & Backpressure ===
    /// Number of parallel workers/channels (default 4)
    int? MaxDegreeOfParallelism { get; set; }

    /// Max items buffered per worker channel before backpressure (default Size * 4)
    int? ChannelCapacity { get; set; }

    // === Retry ===
    /// Max retry attempts per batch (default 3)
    int? MaxRetries { get; set; }

    /// Base delay for exponential backoff (default 1s)
    TimeSpan? RetryBaseDelay { get; set; }

    /// Max delay cap for backoff (default 30s)
    TimeSpan? RetryMaxDelay { get; set; }

    /// Predicate to decide if a failed item is retryable (default: status == 429)
    Func<BulkResponseItemBase, T, bool> RetryDocumentPredicate { get; set; }

    // === Document Routing ===
    /// Function to extract a routing key from a document for worker affinity.
    /// Documents with the same key always go to the same worker, preserving order.
    /// Null = round-robin (no ordering guarantee).
    Func<T, string> DocumentAffinityKey { get; set; }

    // === Target ===
    IndexName Index { get; set; }
    string Pipeline { get; set; }
    Routing Routing { get; set; }
    Time Timeout { get; set; }
    int? WaitForActiveShards { get; set; }

    // === Behavior ===
    /// How to map each T to a bulk operation. Default: IndexMany.
    Action<BulkStreamDescriptor, IList<T>> BufferToBulk { get; set; }

    /// Called for items that fail and are not retryable.
    Action<BulkResponseItemBase, T> DroppedDocumentCallback { get; set; }

    /// If true, continue processing after non-retryable failures (default true).
    bool ContinueAfterDroppedDocuments { get; set; }

    /// Refresh target indices after all processing completes.
    bool RefreshOnCompleted { get; set; }
    Indices RefreshIndices { get; set; }

    // === Callbacks ===
    /// Called for every bulk response (including retries). For observability.
    Action<BulkStreamResponse> BulkResponseCallback { get; set; }
}
```

### Response — `BulkStreamAllResponse`

```csharp
public class BulkStreamAllResponse
{
    /// Batch sequence number (0-based, per worker)
    public long Page { get; internal set; }

    /// Which worker processed this batch
    public int WorkerIndex { get; internal set; }

    /// Number of retry attempts for this batch
    public int Retries { get; internal set; }

    /// Items from the bulk response
    public IReadOnlyCollection<BulkResponseItemBase> Items { get; internal set; }

    /// Server-side time in milliseconds
    public long Took { get; internal set; }
}
```

### Observable & Observer

```csharp
public class BulkStreamAllObservable<T> : IObservable<BulkStreamAllResponse>, IAsyncDisposable, IDisposable
    where T : class
{
    public BulkStreamAllObservable(IOpenSearchClient client, IBulkStreamAllRequest<T> request,
        CancellationToken cancellationToken = default);

    public IDisposable Subscribe(IObserver<BulkStreamAllResponse> observer);
    public IDisposable Subscribe(BulkStreamAllObserver observer);

    /// Flush all worker buffers without closing the instance.
    /// Returns when all pending items have been sent and responses received.
    public Task FlushAsync(CancellationToken cancellationToken = default);

    /// Gracefully shut down: flush + close channels + await workers.
    public ValueTask DisposeAsync();
    public void Dispose();
}

public class BulkStreamAllObserver : CoordinatedRequestObserverBase<BulkStreamAllResponse>
{
    public long TotalNumberOfFailedBuffers { get; }
    public long TotalNumberOfRetries { get; }
    public long TotalDocumentsProcessed { get; }
}
```

### Client Extension

```csharp
public partial class OpenSearchClient
{
    public BulkStreamAllObservable<T> BulkStreamAll<T>(
        IEnumerable<T> documents,
        Func<BulkStreamAllDescriptor<T>, IBulkStreamAllRequest<T>> selector,
        CancellationToken cancellationToken = default) where T : class;

    public BulkStreamAllObservable<T> BulkStreamAll<T>(
        IAsyncEnumerable<T> documents,
        Func<BulkStreamAllDescriptor<T>, IBulkStreamAllRequest<T>> selector,
        CancellationToken cancellationToken = default) where T : class;

    public BulkStreamAllObservable<T> BulkStreamAll<T>(
        IBulkStreamAllRequest<T> request,
        CancellationToken cancellationToken = default) where T : class;
}
```

### Blocking Extension (convenience)

```csharp
public static class BulkStreamAllExtensions
{
    public static BulkStreamAllObserver Wait<T>(
        this BulkStreamAllObservable<T> observable,
        TimeSpan maximumRunTime,
        Action<BulkStreamAllResponse> onNext) where T : class;
}
```

## Document-ID Affinity (Solving Go #464)

When `DocumentAffinityKey` is set, the ingestion loop hashes the key to select a worker:

```csharp
int workerIndex = (int)(MurmurHash3(affinityKey) % (uint)numWorkers);
await workers[workerIndex].Channel.Writer.WriteAsync(document, ct);
```

This guarantees:
- All operations for the same document go to the same worker
- Within a worker, operations are processed in FIFO order
- Bulk requests from a single worker maintain document ordering
- Create→Update→Delete sequences for the same ID are never reordered

When `DocumentAffinityKey` is null, items are distributed round-robin for maximum throughput (no ordering guarantee — same as existing `BulkAllObservable<T>`).

## Flush Without Close (Solving Go #336)

```csharp
public async Task FlushAsync(CancellationToken cancellationToken = default)
{
    // Signal each worker to flush its current buffer immediately
    foreach (var worker in _workers)
        worker.FlushSignal.Set();

    // Await until all workers report their current buffers are drained
    await Task.WhenAll(_workers.Select(w => w.WaitForFlushComplete(cancellationToken)));
}
```

Key design points:
- Does NOT close channels — the instance remains usable for more `Add` operations
- Does NOT cancel in-flight retries — only drains what's currently buffered
- Is safe to call concurrently (idempotent)
- The blocking `Wait()` extension implicitly flushes on completion

This enables patterns like:
```csharp
// Lambda handler — BulkStreamAll instance lives across invocations
await _bulkStreamAll.FlushAsync(); // drain current batch
return response; // Lambda returns, instance stays warm
```

## Retry Strategy

```
Delay = min(RetryMaxDelay, RetryBaseDelay * 2^attempt * jitter)
where jitter ∈ [0.75, 1.25]
```

Per-item retry (not whole-batch):
1. Send batch → get `BulkStreamResponse`
2. Partition response items into: succeeded, retryable (per predicate), dropped
3. Invoke `DroppedDocumentCallback` for dropped items
4. If retryable items remain AND attempts < MaxRetries:
   - Wait backoff delay
   - Re-send only the retryable items as a new smaller batch
5. If retries exhausted: either throw or continue (per `ContinueAfterDroppedDocuments`)

## Backpressure via Bounded Channels

Each worker owns a `Channel<T>.CreateBounded(ChannelCapacity)`:
- When the channel is full, the ingestion loop's `WriteAsync` naturally blocks
- This propagates backpressure to the source (`IAsyncEnumerable` stops yielding)
- No semaphore needed — the channel itself is the throttle
- `ChannelCapacity` defaults to `Size * 4` (4 batches worth of buffering per worker)

## Usage Examples

### Basic — Index documents from a collection
```csharp
var observable = client.BulkStreamAll(documents, b => b
    .Index("my-index")
    .Size(500)
    .MaxDegreeOfParallelism(4)
    .MaxRetries(3)
    .DroppedDocumentCallback((item, doc) => logger.Warn($"Dropped: {item.Id}"))
);

var observer = observable.Wait(TimeSpan.FromMinutes(10), response =>
    logger.Info($"Page {response.Page} indexed {response.Items.Count} docs"));
```

### Streaming — Kafka consumer with document affinity
```csharp
async IAsyncEnumerable<OrderEvent> ConsumeKafka([EnumeratorCancellation] CancellationToken ct)
{
    await foreach (var msg in consumer.ConsumeAsync(ct))
        yield return msg.Value;
}

var observable = client.BulkStreamAll(ConsumeKafka(cts.Token), b => b
    .Index("orders")
    .Size(1000)
    .MaxBatchBytes(5_000_000)
    .FlushInterval(TimeSpan.FromSeconds(5))
    .MaxDegreeOfParallelism(8)
    .DocumentAffinityKey(order => order.OrderId)  // same order always same worker
    .BufferToBulk((descriptor, batch) =>
    {
        foreach (var order in batch)
            descriptor.Index<OrderEvent>(i => i.Document(order).Id(order.OrderId));
    })
    .RetryDocumentPredicate((item, doc) => item.Status == 429 || item.Status == 503)
    .BulkResponseCallback(r => metrics.RecordBulkLatency(r.Took))
);
```

### Lambda — Flush without close
```csharp
// In main() — shared across invocations
_bulkStream = client.BulkStreamAll<MyEvent>(events, b => b
    .Index("events")
    .Size(500)
    .FlushInterval(TimeSpan.FromSeconds(2))
);
_bulkStream.Subscribe(new BulkStreamAllObserver(onNext: r => { }));

// In handler — called per invocation
public async Task Handle(KinesisEvent kinesisEvent)
{
    foreach (var record in kinesisEvent.Records)
        _events.Add(Deserialize(record));  // feed the source enumerable

    await _bulkStream.FlushAsync();  // drain without destroying
}
```

## File Layout

```
src/OpenSearch.Client/Document/Multiple/BulkStreamAll/
├── IBulkStreamAllRequest.cs          // Interface + defaults
├── BulkStreamAllRequest.cs           // POCO implementation
├── BulkStreamAllDescriptor.cs        // Fluent descriptor
├── BulkStreamAllObservable.cs        // Core orchestrator
├── BulkStreamAllObserver.cs          // Observer with counters
├── BulkStreamAllResponse.cs          // Per-batch response DTO
├── BulkStreamAllWorker.cs            // Per-worker channel + batch logic
├── OpenSearchClient-BulkStreamAll.cs // Client extension methods
└── RetryStrategy.cs                  // Exponential backoff + jitter
```

## Migration Path

| Scenario | Recommendation |
|----------|---------------|
| New code, server supports `_bulk/stream` | Use `BulkStreamAll` |
| New code, server does NOT support `_bulk/stream` | Use `BulkStreamAll` with fallback (see below) |
| Existing code using `BulkAllObservable<T>` | Keep working, migrate at your pace |
| Custom implementations wrapping `BulkAll` | Replace with `BulkStreamAll` |

### Fallback for servers without `_bulk/stream`

`BulkStreamAllObservable<T>` should detect a 404/400 on first `_bulk/stream` call and transparently fall back to standard `_bulk` requests (using `BulkRequest` internally). This is a single if-branch in the worker dispatch path. Log a warning on fallback.

## Testing Plan

1. **Unit tests** — Mock `IOpenSearchClient`, verify:
   - Batching by count and bytes
   - Document affinity routing (same key → same worker)
   - Retry logic (429 items retried, dropped items callbacked)
   - FlushAsync drains without closing
   - Backpressure (producer blocks when channels full)
   - Cancellation propagation

2. **Integration tests** — Against a real cluster:
   - End-to-end ingestion of N documents
   - Verify document ordering with affinity enabled
   - Verify retry on simulated 429s
   - Verify fallback to `_bulk` on older clusters

3. **Benchmark** — Compare throughput/latency vs. existing `BulkAllObservable<T>`:
   - 100k, 1M, 10M documents
   - With/without affinity
   - Various parallelism levels

## Open Questions

1. **Should `BulkStreamAll` also accept `IBulkOperation` directly** (not just `T`)?
   This would support mixed create/update/delete in a single stream. The `BufferToBulk` callback partially covers this, but a first-class `IAsyncEnumerable<IBulkOperation>` overload could be cleaner.

2. **Completion semantics for infinite streams** — When the source is infinite (`IAsyncEnumerable` from Kafka), `OnCompleted` never fires. Should we add a `DrainAndComplete()` method? Or is `FlushAsync()` + `DisposeAsync()` sufficient?

3. **Metrics/OpenTelemetry integration** — Should `BulkStreamAllObservable` emit OTel spans/metrics natively, or rely on the callback pattern?

4. **Server-Sent Events from `_bulk/stream`** — If the streaming endpoint sends incremental responses, the worker dispatch logic may need to handle a streaming response rather than a single JSON blob. Need to confirm PR #935's response semantics.
