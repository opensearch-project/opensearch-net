# Design: Migrate serialization from Utf8Json to System.Text.Json

**Issue:** [#388](https://github.com/opensearch-project/opensearch-net/issues/388)
(also relates to #370 trimming, #424 AOT, #318)
**Status:** Draft for review
**PoC / evidence:** This PR ships the production foundation (`SystemTextJsonSerializer`,
the `ConnectionSettings` seam) with an in-tree regression test
(`tests/Tests.Reproduce/SystemTextJsonSeamTests.cs`). The full converter
proof-of-concept and code generator — including wire-format parity vs. the real
Utf8Json client across queries and aggregations, recursion, field-name inference,
and round-trip — live on the `poc/stj-serializer-vertical-slice`
branch as referenced evidence.

---

## 1. Problem

`OpenSearch.Net` compiles a vendored, re-namespaced copy of the **abandoned**
Utf8Json library (57 files, ~20,500 LOC) and uses it as the **default internal
serializer** for all requests/responses. The high-level client is deeply coupled
to it: ~151 custom `IJsonFormatter<T>` implementations and ~135 files thread
Utf8Json's `IJsonFormatterResolver`. This blocks enterprise security reviews,
prevents trimming/AOT, and accrues maintenance cost (e.g. in-place patches like
PR #959). A source-serializer-only package does **not** solve this — the engine
must be replaced.

## 2. Goals / Non-goals

**Goals**
- Replace Utf8Json with **System.Text.Json** as the internal serializer.
- Preserve byte-for-byte request/response JSON (validated by the existing ~3,200
  serialization tests).
- Keep `netstandard2.0/2.1` support; enable trimming/AOT on `net6.0+`.
- Provide a clean migration path with a one-major-version opt-in fallback.

**Non-goals**
- Changing the public fluent/descriptor API surface beyond unavoidable breaks.
- Feature parity with Newtonsoft behaviors that OpenSearch doesn't need.

## 3. Decisions to lock (Step 0)

| # | Decision | Recommendation | Status |
|---|---|---|---|
| D1 | State threading (replacing `IJsonFormatterResolver`) | Settings-carrying `JsonSerializerOptions` + converters constructed with settings | **Validated by PoC** |
| D2 | Existing `[DataMember]`/`[ReadAs]`/`[StringEnum]` (3,703 attrs) | Honor via one `DefaultJsonTypeInfoResolver`/contract customization — not mass attribute rewrites | Proposed |
| D3 | Target frameworks | Reflection-mode STJ on `netstandard2.0/2.1`; add `net8.0+` for source-gen (AOT/trim) | Proposed |
| D4 | Backward compatibility | Utf8Json behind an opt-in switch for one major version, then delete | Proposed |
| D5 | Converter production | **Code-generate** the bulk from the OpenAPI model; hand-write complex converters | Proposed |

## 4. Architecture (validated by the PoC)

- **`OpenSearchJsonOptions`** owns the settings and a `JsonSerializerOptions`
  whose converters are constructed with those settings — every converter can
  reach connection settings (field-name inferrer, etc.). This is the direct
  replacement for `IJsonFormatterResolver`.
- **Polymorphic single-key dispatch** (`{ "<variant>": { ... } }`) is handled by
  hand-written `JsonConverter<T>` reading/writing the wrapper — proven for
  queries (`match_all`/`term`/`bool`) and aggregations (`terms`/`max`).
- **Recursion** (compound `bool` query) works by the converter re-entering
  `JsonSerializer` with the same options.
- **Named-map containers** (`aggs`) handled in the owning converter.
- New serializers implement the existing `IOpenSearchSerializer`, so they drop
  into `DefaultHighLevelSerializer` / `LowLevelRequestResponseSerializer` without
  changing call sites.

PoC evidence: xUnit tests cover exact-JSON parity, round-trip, recursion, the
second polymorphic family, state threading, and error handling — on a library
that **targets `netstandard2.0`** using the System.Text.Json NuGet package.

## 5. Code generation (the accelerator)

The client is already generated from the OpenSearch OpenAPI specification via
`src/ApiGenerator`. Extend it to emit STJ converters for generated model types
using the PoC's patterns. This converts the ~151-converter hand-effort into a
mostly-generated artifact (the approach Elastic's v8 client used). Complex
converters (geo shapes, `_source`, compound queries) remain hand-written and are
flagged by the generator.

**Measured evidence (PoC, validated against the real client's wire output):** a
generator with **4 shape templates** (Empty / FieldOnly / FieldValue / Compound)
reproduces the exact wire format byte-for-byte for **8 real query types**
(`match_all`, `exists`, `term`, `prefix`, `wildcard`, `regexp`, `match`, and the
recursive `bool`), including expression-based **field-name inference** threaded
through the converter. Of the leaf queries probed, only `ids` was irregular
(~88% generatable from metadata with a small template set). See the
`poc/stj-serializer-vertical-slice` branch.

## 6. Migration plan

| Step | Deliverable | Exit criteria |
|---|---|---|
| 1. Foundation | Settings options, contract resolver, `SystemTextJsonSerializer : IOpenSearchSerializer` | Low-level round-trips via STJ; `OpenSearch.Net` tests green |
| 2. Generator | `ApiGenerator` emits converters for one real type, then all routine types | Generated converters compile; complex ones flagged |
| 3. Migrate | STJ as internal default; remove resolver threading (135 files) | `OpenSearch.Client` compiles; Utf8Json off the hot path |
| 4. Parity | Fix STJ default divergences | ~3,200 unit tests green; integration green vs OpenSearch 3.x |
| 5. AOT/source | Source-serializer pkg; net8 source-gen | Trimming/AOT validated (#370/#424) |
| 6. Compat/release | Opt-in legacy, UPGRADING, benchmarks, delete engine | Released; #388/#370/#424/#318 closed |

## 7. Testing

The existing ~3,200 exact-JSON serialization tests are the oracle and run
continuously through Steps 2–4. Add: benchmark comparison vs Utf8Json; trimming/
AOT smoke tests; a compatibility test matrix for the opt-in legacy serializer.

## 8. Risks

| Risk | Mitigation |
|---|---|
| STJ stricter defaults cause subtle JSON drift | Exact-JSON test oracle catches it; tune via options/converters |
| Converter state design (sank the 2023 attempt) | **Resolved** — PoC validates the pattern |
| Hand-written reader positioning is error-prone | Generate converters; rely on the test oracle |
| `netstandard2.0` lacks source-gen | Reflection mode there; source-gen only on net6+ |
| Scope/effort | Code generation + phased rollout; opt-in legacy reduces pressure |

## 9. Effort

- Human: ~5–9 months solo / ~3–5 months with two engineers.
- The code generator (Step 2) is the primary lever; without it the converter
  hand-work dominates.

## 10. Open questions

1. Final version number / deprecation window for Utf8Json.
2. Whether to multi-target `net8.0` immediately or after parity.
3. Source serializer default: STJ vs. keeping Newtonsoft package as an option.

## 11. First fully-integrated namespace: analysis tokenizers

The `analysis/tokenizers` namespace is the first end-to-end slice wired through
the production stack (`SystemTextJsonSerializer` + `DataContractResolver` +
`StringEnumConverter` + a polymorphic `TokenizerInterfaceConverter`) and
validated **byte-for-byte against the real Utf8Json high-level client** as the
oracle. Harness: `poc/StjTokenizerParity` — **13/13 cases reach full write + round-trip
read parity** across every concrete `ITokenizer` type.

What this exercised and the reusable findings for the rest of the migration:

- **Interface-declared attributes.** The client declares `[DataMember]` on
  *interfaces* (e.g. `ICharGroupTokenizer.MaxTokenLength`), while the concrete
  property implements it implicitly with no attribute. The Utf8Json `MetaType`
  resolved names by walking interface maps; `DataContractResolver` now does the
  same, so concrete-type (de)serialization honors the interface names. This is a
  general fix, not tokenizer-specific.
- **Polymorphic dispatch via `type` discriminator.** `TokenizerInterfaceConverter`
  replaces the hand-written `TokenizerFormatter`: write serializes the concrete
  runtime type (no recursion, since it is not `ITokenizer`); read parses the
  `type` string and dispatches to the concrete type. Its table is a **strict
  superset** of `TokenizerFormatter` — it also registers `keyword`/`letter`/
  `lowercase`, closing a latent read-dispatch drift — matching the table the
  converter generator produces.
- **`[StringEnum]` enums.** STJ's built-in `JsonStringEnumConverter` emits the CLR
  member name and does not read `[EnumMember]` on the supported target
  frameworks. `StringEnumConverterFactory` (a `JsonConverterFactory`) reproduces the
  Utf8Json behavior, so `TokenChar.Whitespace` → `"whitespace"`. Verified via
  `token_chars`.
- **Minimal escaping.** STJ's default HTML-safe encoder escaped `+` as `\u002B`
  (diverging on `pattern` and `custom_token_chars`). Setting
  `Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping` matches Utf8Json's
  minimal escaping; this is now the serializer's default and is a **client-wide**
  requirement, not a tokenizer detail.
- **Null omission.** The high-level client serializes with `excludeNull: true`;
  parity requires `DefaultIgnoreCondition = WhenWritingNull`.
- **No `char?`/`NullableStringInt` write gap.** `char?` (`delimiter`,
  `replacement`) and the `NullableStringIntFormatter` properties matched on the
  write path; the only residual difference is the read path's tolerance of
  string-encoded integers, which is a response-only concern to revisit before
  cutover.
