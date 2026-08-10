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
| D3 | **Both layers gained an STJ path, and both now have an independent opt-in switch.** `ConnectionConfiguration.UseSystemTextJson()` (low level) mirrors `ConnectionSettings.UseSystemTextJson()` (high level); Utf8Json stays the default for both. | The migration target is the high level (#388), so Utf8Json remains the low-level default — this was previously the low-level default too, briefly, then deliberately reverted (see history note below) once it caused real YAML failures, and the switch exists so the STJ path is reachable without repeating that regression for every caller. The two switches read the same `OSC_USE_STJ` variable but select their engines independently, matching how the two layers already have independent config entry points. See [§5](#5-known-trade-offs--limitations) for what this switch does and does not change, including the residual risk it reopens. |
| D4 | **Parity is verified by running the entire existing unit suite against both engines** rather than by writing a parallel STJ-only test suite. | Zero new assertions to drift; the existing suite already pins the expected JSON. See [§6](#6-verification). |
| D5 | **Reuse the existing hand-written domain model and its markers.** New STJ converters are additive; no `[JsonFormatter]`/`[DataMember]` attributes were stripped from the domain model. | Minimizes blast radius and keeps the two engines reading the same annotated types. |

D3 history: the low-level default engine changed twice before landing on today's opt-in switch —
`SystemTextJsonSerializer` became the low-level default early in this PR, was reverted back to
Utf8Json after the YAML suite (which drives a real low-level client) surfaced number-formatting
and exception-shape regressions, and this PR now adds the switch so the same STJ path is
reachable on demand without reintroducing that regression as anyone's default. See [§5](#5-known-trade-offs--limitations).

## 3. Two-layer architecture

The client is two stacked layers, each with its **own** serializer and its **own**
configuration entry point — which is why they can default to different engines:

| Layer | Assembly | Handles | Config entry point | Default engine |
|-------|----------|---------|--------------------|----------------|
| **Low-level** | `OpenSearch.Net` | Raw JSON, `DynamicResponse`, dynamic dictionaries, sniff, exceptions | `ConnectionConfiguration` | Utf8Json (opt-in STJ) |
| **High-level** | `OpenSearch.Client` | Strongly-typed requests/responses, the fluent DSL | `ConnectionSettings` (`: ConnectionSettingsBase`) | Utf8Json (opt-in STJ) |

The high-level client internally owns a low-level client to actually send bytes.
So a high-level request is: *typed object → high-level serializer → JSON → low-level
`DoRequest`*.

A high-level client's `OpenSearchClient.LowLevel` shares the parent's `Transport`/
`ConnectionSettings` — it is not a second, independently-defaulted low-level client. Calling
`settings.UseSystemTextJson()` on the high level already switches `client.LowLevel` to STJ too,
because both read the same `IConnectionConfigurationValues.RequestResponseSerializer`. The
low-level `ConnectionConfiguration.UseSystemTextJson()` toggle described below matters for a
**standalone** low-level client — one constructed directly from `ConnectionConfiguration`, with
no high-level `ConnectionSettings` involved at all.

### Engine selection precedence (each layer independently)

High level, resolved in `ConnectionSettingsBase` (`BuildHighLevelSerializers()`):

1. Programmatic `settings.UseSystemTextJson(true|false)` — highest.
2. Environment: `OSC_USE_STJ=true` → STJ; `OSC_USE_STJ=false` → Utf8Json.
3. Not set → **Utf8Json** (the default).

Low level, resolved in `ConnectionConfiguration<T>` (`BuildRequestResponseSerializer()`):

1. An explicit serializer passed to a `ConnectionConfiguration` constructor — highest;
   overrides both the toggle and the environment variable, and is retained across later
   `UseSystemTextJson()` calls (matching the pre-existing constructor behavior).
2. Programmatic `connectionConfiguration.UseSystemTextJson(true|false)`.
3. Environment: same `OSC_USE_STJ` variable as the high level, read
   independently.
4. Not set → **Utf8Json** (the default).

The two layers read the same environment variables but resolve and apply them
independently — setting `OSC_USE_STJ=true` switches both by default, but the two
`UseSystemTextJson()` calls (high-level `ConnectionSettings`, low-level `ConnectionConfiguration`)
are separate method calls on separate objects, so a program can select different engines per
layer if it constructs its low-level and high-level clients separately.

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

- **The low level's STJ path is opt-in, matching the high level, not the default (D3).**
  `ConnectionConfiguration.UseSystemTextJson()` activates it for a standalone low-level
  client, exercised by the dual-engine unit suite (§6). This is not the first time the
  low level ran on STJ: an earlier revision of this PR made it the low-level default
  outright, then reverted specifically because the YAML integration suite (which drives a
  real low-level `OpenSearchLowLevelClient`, see below) failed on `search.backpressure`
  (`heap_variance`), a `flat_object` case, and `strict_allow_templates` — low-level
  dynamic number formatting and exception-shape gaps in the STJ converters that the
  mature Utf8Json path didn't have. Those gaps have since been closed by the low-level
  converters added over this PR (real-number formatting in `ObjectConverter` /
  `DynamicDictionaryConverter`, the exception converter, the relaxed encoder, and the
  `InterfaceDataContractResolver` registration), and the `test-yaml-stj` CI leg (§6) now
  runs the YAML suite under `OSC_USE_STJ=true` against a real cluster — a representative
  version per major line (1.3.14 / 2.16.0 / 3.6.0) — where those historical failures no
  longer reproduce. So the low-level switch is integration-validated for those
  representative versions, not merely unit-tested; it is not, however, run under STJ
  across the *full* version matrix (only the default Utf8Json config is), so treat the
  full-matrix guarantee as still Utf8Json-only.
- **`DynamicResponse` and `ServerError` now honor the configured engine.** Both used to
  hardcode `LowLevelRequestResponseSerializer.Instance` regardless of which engine was
  configured, silently keeping dynamic responses and server-error parsing on Utf8Json
  even when the caller had opted into STJ everywhere else — a real behavioral gap fixed
  as part of adding the low-level toggle above (`ResponseBuilder.SetSpecialTypes`,
  `ServerError.Create`/`TryCreate`/`CreateAsync`).
- **The low-level `SystemTextJsonSerializer` now registers `InterfaceDataContractResolver`
  as its `TypeInfoResolver`.** It previously registered none, so STJ's default resolver
  silently left any `[DataMember]` property exposed through a non-public setter at its
  default value on read — e.g. `ServerError.Status`/`Error` are `{ get; internal set; }`,
  so every STJ-parsed server error had `Status == -1` and `Error == null` regardless of
  the actual response body, discovered while testing the `ServerError` fix above.
  `InterfaceDataContractResolver` (already used by the high level's
  `HighLevelContractResolver`, a subclass) wires those setters via reflection; the base
  resolver has no dependency on `IConnectionSettingsValues`, so it applies here unchanged.
- **The YAML test runner now exercises the low-level STJ path.** It builds an
  `OpenSearchLowLevelClient` over `ConnectionConfiguration` (see
  `tests/Tests.YamlRunner/Program.fs`); since that config now honors `OSC_USE_STJ`
  (`ConnectionConfiguration.UseSystemTextJson`), the runner drives the low-level STJ
  engine end-to-end when the variable is set. `integration-yaml-tests.yml` has a
  `test-yaml-stj` leg that sets `OSC_USE_STJ=true` for a representative version per
  major line — this is the residual-risk validation described above (an earlier
  low-level-STJ-by-default revision failed the YAML suite on `search.backpressure`
  `heap_variance`, a `flat_object` case, and `strict_allow_templates`). If that leg
  surfaces failures, they are the remaining low-level STJ gaps to close, not a
  regression in the default (Utf8Json) configuration, which the full matrix still
  covers.
- **Code-generation templates are unchanged.** ApiGenerator (`.cshtml`) still emits
  `using OpenSearch.Net.Utf8Json;` and `[SerializationConstructor]` on generated
  request classes in `_Generated/`. STJ's resolver already honors those markers, so
  regenerating does not *break* STJ — it just re-introduces the Utf8Json `using`.
  Updating the templates is a follow-up (§8), consistent with #982/#996 which also
  left templates untouched.
- **The vendored Utf8Json library is retained.** Removing it is a later-major-version
  follow-up (§8), matching #982/#996.

## 6. Verification

- **Unit suite, both engines, high level** (`.github/workflows/test-jobs.yml`): a matrix leg for
  `utf8json` (`OSC_USE_STJ` unset) and `stj` (`OSC_USE_STJ=true`) runs the full unit
  suite twice. Parity is asserted by the existing suite (D4).
- **Integration, both engines, high level** (`.github/workflows/integration.yml`): integration tests
  build the high-level client via `TestConnectionSettings : ConnectionSettings`, which
  honors `OSC_USE_STJ`. The matrix has an `engine` dimension (`utf8json` / `stj`), so
  every server version runs the full integration suite under both engines end-to-end
  against a real cluster.
- **YAML** stays single-engine by necessity (§5) — it drives the low-level client, which
  stays on the default (Utf8Json unless `ConnectionConfiguration.UseSystemTextJson()` or
  `OSC_USE_STJ` is set) in every CI job; no CI job sets either. This means, unlike the
  high level, the low-level STJ opt-in described in §7 is **not** exercised end-to-end
  against a real cluster by this PR — see the D3 history note and §5's residual-risk
  caveat, and the follow-up below.
- **Recorded-response `[U]` tests** in `tests/Tests.Reproduce` and the new
  deserialization/serialization tests reproduce each fixed bug against recorded JSON.

## 7. How to opt in

High-level client (also switches the shared `client.LowLevel`, since they use the same
`ConnectionSettings`):

```csharp
var settings = new ConnectionSettings(pool)
    .UseSystemTextJson();          // high-level STJ; UseSystemTextJson(false) forces Utf8Json
var client = new OpenSearchClient(settings);
```

Standalone low-level client (no high-level `ConnectionSettings` involved):

```csharp
var config = new ConnectionConfiguration(pool)
    .UseSystemTextJson();          // low-level STJ; UseSystemTextJson(false) forces Utf8Json
var lowLevelClient = new OpenSearchLowLevelClient(config);
```

or, without touching code — sets the default for whichever of the two above is
constructed without an explicit `UseSystemTextJson()` call:

```bash
OSC_USE_STJ=true dotnet run     # honored by both the high- and low-level client independently
```

## 8. Follow-ups (out of scope for this PR)

- Update ApiGenerator templates to emit STJ-friendly markers instead of
  `using OpenSearch.Net.Utf8Json;` so regeneration does not re-introduce the
  dependency. (#982 attached a PoC; not done here — see §5.)
- Flip the *default* engine (high level, low level, or both) to STJ, with a
  documented Utf8Json removal timeline, as #996 proposes for 3.0.0. Both layers
  already have an opt-in `UseSystemTextJson()` switch (D3); this follow-up is
  specifically about changing what happens when no one calls it.
- Decouple the STJ path from Utf8Json markers/types it currently reuses (tracked with
  `TODO(utf8json-decoupling)` in code): the response-dictionary converter factory reads the
  legacy `[JsonFormatter(...)]` attribute to discover its generic arguments, and
  `HighLevelContractResolver` holds a Utf8Json `IJsonFormatterResolver` to invoke type-level
  `ShouldSerialize(IJsonFormatterResolver)` conventions. Reusing them keeps the STJ mapping in
  lock-step with the legacy one and avoids re-annotating the domain model, but must be replaced
  with STJ-native equivalents before the vendored library can be removed. The `LazyDocument`
  model type (e.g. on `ScriptedMetricAggregate`/`TopHitsAggregate`) similarly still takes a
  Utf8Json resolver.
- Remove the vendored `OpenSearch.Net.Utf8Json` library once the default has moved.
- Consider a `PolymorphicInterfaceConverter<T>` and a `[JsonFormatter]`→converter
  bridge (as in #982) to reduce the number of hand-written converters.
- AOT / source-generator (`JsonSerializerContext`) support — unsolved across all
  three PRs; a future direction.
- Add a YAML/integration CI leg that opts the low-level client into STJ (e.g. an
  `engine` dimension on `integration-yaml-tests.yml`, or a low-level-specific
  integration job), so the low-level switch introduced here gets the same
  real-cluster validation the high-level switch already has (§6), rather than only
  unit-level coverage. This is the direct way to close the residual-risk caveat in §5.
