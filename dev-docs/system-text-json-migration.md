# System.Text.Json Migration

> Design & decision record for the migration of the high-level `OpenSearch.Client`
> serializer from the bundled Utf8Json engine to `System.Text.Json` (STJ).
>
> Tracking issue: [#388](https://github.com/opensearch-project/opensearch-net/issues/388).
> Related PRs: [#982](https://github.com/opensearch-project/opensearch-net/pull/982),
> [#996](https://github.com/opensearch-project/opensearch-net/pull/996).

## 1. Motivation

The client ships a vendored copy of [Utf8Json](https://github.com/neuecc/Utf8Json)
(under `OpenSearch.Net.Utf8Json`) as its serialization engine. Utf8Json is
unmaintained, incompatible with modern .NET trimming/AOT direction, and forces us
to carry ~10k lines of third-party code. `System.Text.Json` is the in-box,
actively-maintained serializer that the wider .NET ecosystem has standardized on.

The goal of this work is to give the high-level client a fully-functional STJ
serialization path that is byte-for-byte compatible with the Utf8Json output the
server already accepts, so that Utf8Json can eventually be removed (a later major
version — see [Follow-ups](#8-follow-ups)).

## 2. Decision summary

| # | Decision | Rationale |
|---|----------|-----------|
| D1 | **STJ is opt-in; Utf8Json stays the default.** | Switching the default engine in an already-released 2.x line risks introducing an unknown serialization breaking change. Callers opt in when ready; the default is unchanged for existing users. |
| D2 | **Engine selection is configurable in code**, not only via environment variable. | Reviewers ([Xtansia](https://github.com/opensearch-project/opensearch-net/pull/1002), [Hailong-am](https://github.com/opensearch-project/opensearch-net/pull/1002)) asked for a discoverable, deterministic switch. `settings.UseSystemTextJson()` is IntelliSense-discoverable and testable, and takes precedence over the env vars. |
| D3 | **Both layers gained an STJ path, but only the high level activates it.** The low-level `OpenSearch.Net` client stays on Utf8Json unconditionally. | The migration target is the high level (#388). The low level owns raw dynamic/number/exception formatting whose Utf8Json behavior is mature; keeping it on Utf8Json avoided a class of low-level regressions (see [§5](#5-known-trade-offs--limitations)). |
| D4 | **Parity is verified by running the entire existing unit suite against both engines** rather than by writing a parallel STJ-only test suite. | Zero new assertions to drift; the existing suite already pins the expected JSON. See [§6](#6-verification). |
| D5 | **Reuse the existing hand-written domain model and its markers.** New STJ converters are additive; no `[JsonFormatter]`/`[DataMember]` attributes were stripped from the domain model. | Minimizes blast radius and keeps the two engines reading the same annotated types. |

## 3. Two-layer architecture

The client is two stacked layers, each with its **own** serializer and its **own**
configuration entry point — which is why they can default to different engines:

| Layer | Assembly | Handles | Config entry point | Default engine |
|-------|----------|---------|--------------------|----------------|
| **Low-level** | `OpenSearch.Net` | Raw JSON, `DynamicResponse`, dynamic dictionaries, sniff, exceptions | `ConnectionConfiguration` | **Utf8Json (fixed)** |
| **High-level** | `OpenSearch.Client` | Strongly-typed requests/responses, the fluent DSL | `ConnectionSettings` (`: ConnectionSettingsBase`) | Utf8Json (opt-in STJ) |

The high-level client internally owns a low-level client to actually send bytes.
So a high-level request is: *typed object → high-level serializer → JSON → low-level
`DoRequest`*.

### Engine selection precedence (high level only)

Resolved in `ConnectionSettingsBase` (`BuildHighLevelSerializers()`):

1. Programmatic `settings.UseSystemTextJson(true|false)` — highest.
2. Environment: `OSC_USE_STJ=true` (or legacy `OSC_USE_UTF8JSON=false`) → STJ;
   `OSC_USE_STJ=false` / `OSC_USE_UTF8JSON=true` → Utf8Json.
3. Neither set → **Utf8Json** (the default).

`ConnectionConfiguration` (low level) does **not** read these variables — it always
constructs `LowLevelRequestResponseSerializer` (Utf8Json). This is intentional (D3).

## 4. Scope of the change

- ~334 files changed vs `main` (~+32.7k lines), dominated by newly-added STJ converters.
- New STJ infrastructure under `OpenSearch.Net/Serialization` (low level) and
  `OpenSearch.Client/CommonAbstractions/SerializationBehavior` (high level):
  - `SystemTextJsonHighLevelSerializer` — high-level engine; registers 134 converters.
  - `HighLevelContractResolver : InterfaceDataContractResolver` — rebuilds the STJ
    contract so interface `[DataMember]` names / non-public members / contract
    suppression match Utf8Json.
  - Per-type converters mirroring the legacy Utf8Json formatters (polymorphic
    property/query/aggregation dispatch, resolvable dictionaries, promises, etc.).
- `UseSystemTextJson()` fluent opt-in on `ConnectionSettingsBase`.
- `UPGRADING.md` / `CHANGELOG.md` describe STJ as an opt-in addition (not a breaking change).

## 5. Known trade-offs & limitations

- **The low level is not migrated (D3).** Low-level dynamic number formatting (e.g.
  preserving `3.0` rather than `3` for boxed doubles), sniff parsing, and exception
  shapes are served by the mature Utf8Json path. STJ low-level converters
  (`ObjectConverter`, `DynamicDictionaryConverter` real-number handling, the
  9-field exception converter) exist and are correct, but are **not on the default
  path**. They activate only if a future change opts the low level into STJ.
- **The YAML test runner cannot exercise the STJ path.** It builds an
  `OpenSearchLowLevelClient` over `ConnectionConfiguration` (see
  `tests/Tests.YamlRunner/Program.fs`), which is Utf8Json-only and ignores
  `OSC_USE_STJ`. Adding an STJ leg to `integration-yaml-tests.yml` would just re-run
  Utf8Json, so it is deliberately **not** dual-engine. STJ is instead covered by the
  dual-engine unit suite and the STJ integration leg (§6).
- **Code-generation templates are unchanged.** ApiGenerator (`.cshtml`) still emits
  `using OpenSearch.Net.Utf8Json;` and `[SerializationConstructor]` on generated
  request classes in `_Generated/`. STJ's resolver already honors those markers, so
  regenerating does not *break* STJ — it just re-introduces the Utf8Json `using`.
  Updating the templates is a follow-up (§8), consistent with #982/#996 which also
  left templates untouched.
- **The vendored Utf8Json library is retained.** Removing it is a later-major-version
  follow-up (§8), matching #982/#996.

## 6. Verification

- **Unit suite, both engines** (`.github/workflows/test-jobs.yml`): a matrix leg for
  `utf8json` (`OSC_USE_STJ` unset) and `stj` (`OSC_USE_STJ=true`) runs the full unit
  suite twice. Parity is asserted by the existing suite (D4).
- **Integration, STJ leg** (`.github/workflows/integration.yml`): integration tests
  build the high-level client via `TestConnectionSettings : ConnectionSettings`, which
  honors `OSC_USE_STJ`. A representative subset of server versions runs under STJ in
  addition to the full default (Utf8Json) matrix, so the opt-in STJ path is exercised
  end-to-end against a real cluster without doubling the (already large) matrix.
- **YAML** stays single-engine by necessity (§5).
- **Recorded-response `[U]` tests** in `tests/Tests.Reproduce` and the new
  deserialization/serialization tests reproduce each fixed bug against recorded JSON.

## 7. How to opt in

```csharp
var settings = new ConnectionSettings(pool)
    .UseSystemTextJson();          // high-level STJ; UseSystemTextJson(false) forces Utf8Json
var client = new OpenSearchClient(settings);
```

or, without touching code:

```bash
OSC_USE_STJ=true dotnet run     # honored by the high-level client only
```

## 8. Follow-ups (out of scope for this PR)

- Update ApiGenerator templates to emit STJ-friendly markers instead of
  `using OpenSearch.Net.Utf8Json;` so regeneration does not re-introduce the
  dependency. (#982 attached a PoC; not done here — see §5.)
- Optionally migrate the low level to STJ and flip the default engine (with a
  documented Utf8Json removal timeline, as #996 proposes for 3.0.0).
- Remove the vendored `OpenSearch.Net.Utf8Json` library once the default has moved.
- Consider a `PolymorphicInterfaceConverter<T>` and a `[JsonFormatter]`→converter
  bridge (as in #982) to reduce the number of hand-written converters.
- AOT / source-generator (`JsonSerializerContext`) support — unsolved across all
  three PRs; a future direction.
