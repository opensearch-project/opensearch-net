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
- **Polymorphic dispatch via `type` discriminator.** A single reusable
  `PolymorphicInterfaceConverter<TInterface>` replaces the per-family
  hand-written `*Formatter` dispatchers: write serializes the concrete runtime
  type (no recursion, since it is not `TInterface`); read parses the `type`
  string and dispatches to the concrete type. Each family supplies only a
  discriminator → concrete-type table — the exact shape the converter generator
  emits. `TokenizerInterfaceConverter`'s table is a **strict superset** of
  `TokenizerFormatter` (it also registers `keyword`/`letter`/`lowercase`,
  closing a latent read-dispatch drift).
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

## 12. Second namespace: char filters (generalization)

The `analysis/charfilters` family (`html_strip`, `mapping`, `pattern_replace`,
plus the `kuromoji`/`icu` plugin filters) uses the identical `type`-discriminator
pattern. Wiring it required **no new dispatch code**: the tokenizer converter was
refactored onto a reusable `PolymorphicInterfaceConverter<TInterface>` (in
`OpenSearch.Net`), and `CharFilterInterfaceConverter` is a thin subclass
supplying only its discriminator table. Same for the eventual token
filters/analyzers/normalizers families.

Harness `poc/StjAnalysisParity` covers both families — **10/10 write + round-trip
read parity** — re-validating the tokenizer refactor and adding char filters. The
`mapping` filter's `a=>b` rules additionally confirm the relaxed encoder is
required beyond `+`: STJ's default encoder escapes `>` to `\u003E`, which
`UnsafeRelaxedJsonEscaping` avoids to match Utf8Json.

**Takeaway for sizing:** after the tokenizer slice paid the one-time
infrastructure cost (interface-aware resolver, enum factory, encoder default,
generic converter), a new `type`-discriminated family costs ~one table. This is
the measured per-namespace rate that makes the generation approach (vs.
hand-writing ~81 converters as in PR #980) the sustainable path.

## 13. Third namespace: token filters (the largest family) and dependency graph

`analysis/tokenfilters` is the biggest polymorphic family — **46 discriminators**
(including the `delimited_payload`/`delimited_payload_filter` alias). Wiring it was
again just a table (`TokenFilterInterfaceConverter`). Harness
`poc/StjTokenFilterParity` runs 35 representative cases: **33/35 full parity**,
covering scalar/string/`int?`/`bool?` props, `[StringEnum]` enums
(`KeepTypesMode`, `DelimitedPayloadEncoding`, `EdgeNGramSide`, `SynonymFormat`),
`char?`, string-list props, and the `version` base field.

The remaining two are **cross-type dependencies**, not converter bugs — exactly the
edges a generator must track:

- `stop` embeds a `StopWords` value type (`Union<string, IEnumerable<string>>`,
  written as either a string or a string array). Closed here with a small
  `StopWordsConverter`; this is a **shared** value type reused by the stop/standard
  analyzers and keep-words, so migrating it once unblocks several components.
- `condition` and `predicate_token_filter` embed an `IScript`. Full parity for
  these needs the **`script`** namespace converter — a separate slice. The
  dispatch is registered now; the two round-trips will pass once `script` lands.

Reusable finding: `NullableStringBooleanFormatter` (used 45× here) writes bare
`true`/`false`, matching STJ's default `bool?`, so no per-property converter is
needed for it on the write path.

## 14. Analyzers + normalizers — finishing the analysis area

The last two analysis families introduced two patterns the shared base did not yet
cover, both now handled generically:

- **Fallback dispatch.** `AnalyzerFormatter` doesn't rely solely on `type`: an
  unrecognized/absent type means `CustomAnalyzer` when a `tokenizer` field is
  present, otherwise `LanguageAnalyzer`. `PolymorphicInterfaceConverter` gained a
  `protected virtual ResolveType(discriminator, JsonElement)` seam; the default is
  the table lookup, and `AnalyzerInterfaceConverter` overrides it to reproduce the
  fallback. `NormalizerInterfaceConverter` uses the same seam to always resolve
  `CustomNormalizer` (OpenSearch ships no built-in normalizers).
- **Data-driven discriminant via a non-public setter.** `LanguageAnalyzer.Type`
  is the language name (e.g. `english`) and is exposed only through a
  `protected set`. STJ's default resolver won't write non-public setters, so the
  discriminant was lost on read (write was fine). `DataContractResolver` now wires
  a reflection setter for any `[DataMember]` property whose only setter is
  non-public — mirroring Utf8Json's `allowPrivate || dm != null` rule. This is a
  **general** fix: every `*Base.Type` (protected set) across the client is now
  correctly rehydrated on deserialize.

`SingleOrEnumerableFormatter<T>` (on `CustomAnalyzer`/`CustomNormalizer` list
props) writes a plain array, so there is no write gap; only its read tolerance of
a bare scalar differs (a response-only concern, like `NullableStringInt`).

Harness `poc/StjAnalyzerParity`: **12/12 write + round-trip read parity** across
named analyzers, the custom/language fallback, the `snowball` enum, `StopWords`,
and both normalizer shapes.

**Analysis area status:** tokenizers, char filters, token filters, analyzers and
normalizers are all migrated and validated. The only residual cross-namespace
edges are `condition`/`predicate_token_filter` → `IScript` (the `script`
namespace), whose dispatch is already registered and will round-trip once
`script` lands.

## 15. Script namespace — dispatch without a discriminator, and constructor-less types

`IScript` (`InlineScript`, `IndexedScript`) has **no `type` discriminator**; the
concrete type is inferred from which field is present (`source`/`inline` →
inline, `id` → indexed). `ScriptInterfaceConverter` reuses the `ResolveType` seam
to inspect the buffered object instead of a discriminator — no new base machinery
needed. Write serializes the concrete runtime type as usual.

This slice surfaced a second general read-side gap and one caveat:

- **Constructor-less deserialization.** `InlineScript` exposes only
  `InlineScript(string script)`; the parameter `script` binds to no property (the
  property is `Source`), so STJ refused to construct it (`IndexedScript(string id)`
  worked only by the `id`→`Id` coincidence). `DataContractResolver` now, for a
  non-abstract type with **no** parameterless constructor, sets `CreateObject` to
  build an uninitialized instance and populate via property setters — the
  data-contract-serializer semantic. It is **gated**: types that have a
  parameterless constructor are untouched, so constructor-set defaults (e.g.
  `TokenizerBase` setting `Type`) still run. (`RuntimeHelpers.GetUninitializedObject`
  on net6+/netstandard2.1; `FormatterServices` on netstandard2.0.)
- **`params` read caveat.** `IScript.Params` is `Dictionary<string, object>`.
  Writes match byte-for-byte (real boxed values). On read, STJ materializes
  `object` values as `JsonElement`, which does not round-trip through the Utf8Json
  oracle — the well-known STJ `object` behavior that also affects `_source`,
  aggregation `meta`, etc. Tracked as a cross-cutting concern for a shared
  `object`/`JsonElement` policy, not a per-namespace fix.

Harness `poc/StjScriptParity`: **7/7** (writes for all; round-trip reads for the
non-`params` cases), and it confirms the previously blocked `condition` and
`predicate_token_filter` token filters now reach **full write + read parity** once
the script converter is registered — closing the dependency noted in §13.

## 16. The `object` / `JsonElement` policy (cross-cutting)

Dynamic payloads typed as `object` — script `params`, aggregation `meta`,
`_source` fragments, and every `Dictionary<string, object>` value — are pervasive.
STJ deserializes `object` to `JsonElement`, which neither matches the CLR shapes
the client's code and tests expect nor round-trips through the pipeline. The new
`ObjectConverter` (registered by default in `SystemTextJsonSerializer`) reproduces
the Utf8Json `PrimitiveObjectFormatter` mapping:

| JSON | CLR |
|---|---|
| object | `Dictionary<string, object>` |
| array | `List<object>` |
| integral number | `long` |
| other number | `double` |
| string / bool / null | `string` / `bool` / `null` |

Writing delegates to the value's runtime type. A subtle but important bug caught
here: `TryGetInt64(...) ? l : GetDouble()` unifies the ternary to `double` and
loses the integral case — every integer became `5.0`. Boxing `long` and `double`
on separate statements fixes it.

This retroactively closes the `params` read caveat from §15 (script params now
round-trip) and is the shared policy for `meta`/`_source`/dynamic fields going
forward. Harness `poc/StjObjectParity`: **9/9** — script `params` round-trips
(ints, doubles, strings, bools, nested objects/arrays) plus direct `object`
shape assertions.

Note: date-like strings remain strings (Utf8Json does not parse dates for
`object`), matching the oracle.

## 17. Query DSL — `QueryContainer` dispatch (first slice)

The query DSL is the largest and most-used polymorphic area, and `QueryContainer`
is the hardest serializer in the client. It is not a discriminated union: each
verb (`bool`, `term`, `match`, …) is a nullable property on `IQueryContainer`, and
a container holds exactly one. `QueryContainerConverter` writes the container as
its interface so null-omission leaves only the populated verb (`{ "bool": {…} }`),
with a raw query written through verbatim; on read the single key selects the
property and the query interface's `[ReadAs]` attribute maps it to the concrete
type. Nested `bool` clause arrays recurse back through the converter.

Making this byte-exact required three general capabilities, all reusable across
the rest of the DSL and the client:

- **`ShouldSerialize<Member>()` support** in `DataContractResolver` — the
  Utf8Json/Json.NET convention `bool` uses to omit empty `must`/`should`/`filter`
  clause arrays. Wired to STJ's `JsonPropertyInfo.ShouldSerialize`.
- **`[ReadAs]` interface→concrete mapping** on read (e.g. `IBoolQuery` →
  `BoolQuery`), since a verb property is typed as an interface.
- **Number formatting.** Utf8Json writes integral doubles with a trailing `.0`
  (`boost: 2.0`), while STJ's shortest form emits `2`. `DoubleFormatConverter`/
  `SingleFormatConverter` append `.0` to integral values in fixed notation;
  scientific-notation values (`1E+20`, `1E-10`) already match. `boost` and scored
  float fields depend on this. Also added a `MinimumShouldMatchConverter` for the
  `Union<int?, string>` value type.

Harness `poc/StjQueryContainerParity`: **8/8 write + round-trip read parity** —
`match_all`/`match_none`, `boost`/`_name`, recursive `bool` (must/should/must_not/
filter), `minimum_should_match`, and nested `bool`.

**Deliberately deferred to following slices** (this slice covers queries needing
no field-name inference): field-name-keyed leaf queries (`term`/`match`/`range`,
serialized as `{ "<field>": {…} }`), which require threading `ConnectionSettings`
(the field inferrer) into the options (decision D1); and conditionless-element
filtering within clause arrays. The dispatch for those verbs is already registered
in the container converter.

## 18. Settings threading + field-name inference (decision D1)

Field/property serialization depends on `ConnectionSettings` (the inferrer maps a
`Field`/`PropertyName` — including typed lambda expressions like `p => p.StockQuantity`
— to a wire name). Utf8Json reached settings through
`IJsonFormatterResolverWithSettings`; STJ has no equivalent, so per decision D1 the
settings are **threaded through the converters**: `FieldConverter` and
`PropertyNameConverter` are constructed with `IConnectionSettingsValues` and call
`settings.Inferrer.Field`/`.PropertyName`. Both also implement STJ's
`ReadAsPropertyName`/`WriteAsPropertyName` so a `Field` can be used as an object key.

This is the last foundational piece, so it also introduces
`SystemTextJsonOptionsFactory.Create(IConnectionSettingsValues)` — the single place
that assembles the `JsonSerializerOptions`: the resolver, encoder, null-omission,
the stateless converters (object, number, enum), the settings-bearing
`Field`/`PropertyName`, and every migrated value/polymorphic/query converter. This
is the integration seam the high-level client will use, and it makes future work a
one-line registration.

Harness `poc/StjFieldInferenceParity` (built entirely from the factory): **8/8** —
`exists` with a string field (verbatim), with expression fields (`p => p.Name` →
`name`, `p => p.StockQuantity` → `stockQuantity`), plus a re-validation that a
tokenizer, token filter, analyzer, script, and `bool`/`match_all` query all still
round-trip through the one consolidated options instance.

Still deferred: field-name-**keyed** leaf queries (`term`/`match`/`range` as
`{ "<field>": {…} }`) — the `FieldNameQuery` wrapper — now unblocked since the
inference plumbing exists.

## 19. Field-name-keyed queries

The everyday term- and full-text queries serialize the field as the object key:
`{ "term": { "<field>": { "value": … } } }` — three levels of nesting (container →
verb → field key → body). `FieldNameQueryConverter<TConcrete, TInterface>` (mirroring
Utf8Json's `FieldNameQueryFormatter<T, TInterface>`, both type parameters) writes
the inferred field name as the key and the body as the **concrete** type (so
`Field`, which is `[IgnoreDataMember]`, is excluded and there is no recursion). It
is constructed with the settings for inference and registered per query interface
in the factory (term, prefix, wildcard, regexp, match, match_phrase,
match_phrase_prefix, match_bool_prefix).

The two type parameters matter: a first cut generic only over the interface
recursed infinitely on read (the interface has no `[ReadAs]`, so it re-dispatched
into itself). Supplying the concrete type resolves the body directly. The
`QueryContainer` read already routed correctly — verbs with `[ReadAs]` (bool,
match_all) deserialize to their concrete type, while field-name verbs (no
`[ReadAs]`) deserialize via the interface, hitting these converters.

Harness `poc/StjFieldNameQueryParity`: **10/10 write + round-trip read** — term
(string/int/bool + `case_insensitive`), expression-inferred fields, prefix,
wildcard, regexp, match, match_phrase, and a `bool` composing `term` + `match`
(exercising all three nesting levels at once).

Deferred within the DSL: the range family (`IRangeQuery` needs its own dispatch to
date/long/numeric/term-range concretes before the field-name wrapper) and the
specialized field-name queries (knn, neural, intervals, span_term, terms_set).

## 20. Range family — two-stage dispatch

`range` is unique: `IRangeQuery` (the container's `Range` verb) has no
discriminator, so `RangeQueryInterfaceConverter` **infers** the concrete type from
the bound values inside the field-keyed body — `format`/`time_zone` or a date-like
bound → date; a non-integral number → numeric; an integral number → long;
otherwise a term (string) range — mirroring Utf8Json's `RangeQueryFormatter`. It
then delegates to the concrete's `FieldNameQueryConverter`, so the two stages
(type inference, then field-name wrapping) compose cleanly:
`{ "range": { "<field>": { "gte": 1.5, "lte": 99.0 } } }`.

This reused the number-formatting insight from §17: a `NumericRangeQuery` with
`GreaterThan = 1` writes `1.0` (double), which both round-trips and lets the read
sniff distinguish it from a `LongRangeQuery` (`1`). Date ranges also needed a small
`DateMathConverter` (`DateMath` writes as its string form; reads anchor a plain
date-time or parse the expression).

Harness `poc/StjRangeQueryParity`: **6/6 write + round-trip read** — numeric
(double bounds), numeric with the `relation` enum, long (integral bounds), term
(string bounds), date (`DateMath` + `format`), and a `bool` filtering on a numeric
range. This completes the everyday query DSL; remaining are the specialized
field-name queries (knn, neural, intervals, span_term, terms_set) and compound/
span/specialized queries.

## 21. Bulk of the query DSL — triage-driven

Rather than convert each remaining query type blindly, a triage harness
(`poc/StjQueryDslTriage`) ran a representative instance of each remaining verb
through the consolidated factory and reported write/read parity. This showed most
compound/full-text queries (`dis_max`, `constant_score`, `function_score`,
`nested`, `query_string`, `script`, `script_score`) already worked — they are
plain objects the resolver + `[ReadAs]` handle. The failures drove three general
resolver capabilities and a few value converters:

- **Explicit interface implementations.** The fluent descriptors (the `[ReadAs]`
  targets for `boosting`, `simple_query_string`, …) implement their interfaces
  explicitly, which STJ ignores. The resolver now surfaces interface `[DataMember]`
  members not otherwise present, reading/writing through the interface (Utf8Json's
  `allowPrivate`).
- **Name-matched interface attributes.** A concrete type may expose a public helper
  property (e.g. `SpanQuery.IsWritable`) parallel to an explicit
  `IQuery.IsWritable` marked `[IgnoreDataMember]`; the interface map points at the
  explicit method, so the resolver also matches interface properties by name to
  inherit `[IgnoreDataMember]`/`[DataMember]`.
- **Generic `[ReadAs]`.** `ReadAsConverterFactory` honors `[ReadAs]` for any
  interface used as a nested property (e.g. `ISpanQuery` inside `span_first`),
  which STJ otherwise cannot instantiate.

Plus value converters for `Id` (`ids`), `Fields` (`multi_match`/`simple_query_string`),
`RelationName` (`has_child`/`has_parent`/`parent_id`), the bespoke flattened
`terms` query, and registrations for the field-name-keyed `terms_set`, `span_term`,
and `knn`. Result: **28/30** in the triage (incl. an analysis/script regression
cross-check through the same factory), covering compound, joining, span, term-level,
and full-text queries.

## 22. Query DSL tail — bespoke formatters (start)

The remaining queries have hand-written Utf8Json formatters (flattened shapes,
nested polymorphic pieces, value-type coordinates). Two common ones are done, and
they contributed reusable value converters:

- **`geo_distance`.** Flattens `_name`/`boost`/`validation_method`/`distance`/
  `distance_type` alongside the field key whose value is the `GeoLocation`. Needed a
  `DistanceConverter` (`"12km"` string form) and a `GeoLocationConverter`
  (`{ "lat": …, "lon": … }`; a converter is required on read because lat/lon are
  get-only, set through the constructor) — both reusable across the rest of geo.
  The `validation_method`/`distance_type` `[StringEnum]`s are already handled.
- **`rank_feature`.** Writes `field` as a value plus one of the polymorphic scoring
  functions (`saturation`/`log`/`sigmoid`/`linear`), dispatched on the concrete
  function type and read back by key.

Harness `poc/StjQueryTailParity`: **5/5** — geo_distance (with/without
validation/type/boost) and rank_feature (field-only, +boost, +saturation function).

`geo_polygon` is also done (`GeoPolygonQueryConverter`, reusing `GeoLocation`):
flattened `_name`/`boost`/`validation_method` + `{ "<field>": { "points": [ … ] } }`
— `poc/StjGeoPolygonParity` **2/2**.

Still deferred (a focused follow-up): `geo_bounding_box`/`geo_shape`/`shape` (the
`IBoundingBox` and geo-shape geometry unions), `distance_feature`,
`more_like_this`, `percolate`, `neural`, `intervals`, and `fuzzy`.

## 23. Aggregations — request side

`AggregationContainer` mirrors `QueryContainer` (verb-named nullable properties,
public implementations, `[ReadAs]`), so the resolver + `ReadAsConverterFactory`
handle it. The one new piece is `AggregationDictionaryConverter` for the user-named
`aggs` map (`{ "<name>": { <aggregation> }, … }`), an `IsADictionaryBase` with an
explicit `IDictionary` implementation STJ can't handle on its own.

This surfaced the last general resolver gap: **`[InterfaceDataContract]` opt-in**.
`IAggregation` is `[InterfaceDataContract]`, so `Meta`/`Name` (no `[DataMember]`)
and `BucketAggregationBase.Aggregations` must be excluded from an aggregation's
body (they live at the container level). Utf8Json treats implementing an
`[InterfaceDataContract]` interface as opt-in; the resolver now does the same
(`[DataContract]` on the type OR any implemented interface marked
`[InterfaceDataContract]` → only `[DataMember]` members serialize). This is a
general correctness improvement — it also cleanly excludes query helper properties
(`IsWritable`, …) that the earlier name-match rule handled case-by-case — and it
caused **no regression** (the full query triage stayed green at 30/30).

Harness `poc/StjAggregationParity`: **11/11 write + round-trip read** — metric
(avg/max/min/sum/value_count/cardinality/stats), bucket (terms, terms+size,
histogram), and a `terms > avg` nested sub-aggregation. Deferred: the response side
(`Aggregate`/`AggregateDictionary`, a large bespoke typed-response reader) and
aggregations with their own formatters (composite, filters, percentiles, …).

## 24. Mappings / properties

The mapping properties are a `type`-discriminated union (`text`/`keyword`/numeric/
`date`/`object`/`nested`/…) inside a named `properties` dictionary. Two converters:
`PropertyInterfaceConverter` dispatches `IProperty` on `type` (the eight numeric
types collapse to `NumberProperty` with its wire `Type` preserved; a missing or
unknown type falls back to `ObjectProperty`, matching `PropertyFormatter`), and
`PropertiesConverter` handles the `PropertyName` → `IProperty` map with inferred
keys. Multi-fields (`fields`) and `object`/`nested` sub-properties recurse.

Harness `poc/StjMappingParity`: **11/11 write + round-trip read** — text, keyword,
integer/float numbers, date, boolean, geo_point, ip, text with multi-fields, and
object/nested with sub-properties. This landed with no new resolver work — the
foundations covered the property bodies. (The per-property dedup/`ClrOrigin` HACK
used by expression-based `AutoMap` is deferred; direct name-based mappings are a
passthrough.)

## 25. Aggregation response reader (first slice)

The response side (`IAggregate`/`AggregateDictionary`) is the client's largest
bespoke reader — a ~1,100-line Utf8Json heuristic that peeks at property names to
infer the aggregate result type. The tractable STJ port buffers each aggregate
object to a `JsonElement` and applies the same heuristics on it (rather than
mirroring the streaming reader). `AggregateConverter` (read-only) dispatches:
`value` → `ValueAggregate`; `count`+min/max/avg/sum → `StatsAggregate`; `buckets`
→ `BucketAggregate` of keyed buckets; `doc_count` (no buckets) →
`SingleBucketAggregate`. Non-reserved object properties in a bucket are named
sub-aggregations and recurse. `AggregateResponseDictionaryConverter` reads the
`aggregations` map (typed-key `type#name` resolution is handled by
`AggregateDictionary`).

Validation is by value (the type is read-only): sample response JSON is
deserialized with both the Utf8Json oracle and the STJ stack and the extracted
values compared. Harness `poc/StjAggregateResponseParity`: **5/5** — avg,
value_count, stats, single-bucket (filter) with a sub-avg, and a terms
multi-bucket with per-bucket sub-avg.

Deferred (subsequent slices of the reader): geo (bounds/centroid/line), top_hits,
matrix_stats, composite/significant-terms buckets, and scripted-metric.

**Slice 2** adds percentiles (object and array `values`), extended-stats
(`sum_of_squares`/`variance`/`std_deviation`/bounds), and bucket-type dispatch —
range/IP-range buckets (`from`/`to`) and date-histogram buckets (`key_as_string`
first) alongside the keyed/terms buckets, mirroring `ReadBucket`'s first-property
dispatch. Harness `poc/StjAggResponse2Parity`: **7/7** (percentiles,
extended_stats, range buckets, date_histogram + sub-sum, plus avg/stats/terms
regression checks).

## 26. Request-side aggregation remainders (bespoke shapes)

A triage of the request-side aggregations (`poc/StjAggReqTriage`) surfaced nine types whose
Utf8Json formatters emit shapes the default `DataContractResolver` cannot reproduce — either
because the significant properties are declared on the interface without `[DataMember]`, or
because the body is re-keyed. Each got a dedicated `System.Text.Json` converter:

- **`filter`** (`FilterAggregationConverter`) — the aggregation body IS the `Filter` query
  container, written inline (not wrapped in another `filter` object). Sub-aggregations and `meta`
  are emitted as siblings by the enclosing container, so the converter only writes the query.
- **`percentiles` / `percentile_ranks`** (`PercentilesAggregationConverter`,
  `PercentileRanksAggregationConverter`) — the `percents`/`values` arrays and the `tdigest`/`hdr`
  method object are interface-only members that the resolver drops; the converters write the full
  field/script/method/missing/array/keyed/format shape. The `tdigest`/`hdr` block is factored into
  a shared `PercentilesMethodConverter` write helper.
- **`terms`/`histogram` order** (`SortOrderConverter<TSortOrder>`) — serialized as
  `{ "<key>": "asc|desc" }`.
- **`terms` include/exclude** (`TermsIncludeConverter`/`TermsExcludeConverter`/
  `IncludeExcludeConverter`) — string pattern, string-array value list, or partition object.
- **`buckets_path`** (`BucketsPathConverter`) — single string, string array, or `{ name: path }`
  object for pipeline aggregations.
- **`calendar_interval`** (`UnionConverter<DateInterval?, DateMathTime>` +
  `DateMathTimeConverter`) — the field is a `Union` of an interval enum and a date-math string; the
  generic union converter writes whichever arm is set and reads first-arm-then-second, mirroring the
  original best-effort formatter. `fixed_interval` reuses the existing `TimeConverter`.
- **`composite` sources** (`CompositeAggregationSourceConverter`) — each source is wrapped as
  `{ "<name>": { "<source_type>": { … } } }`. The inner body is serialized through the concrete
  runtime type (`JsonSerializer.Serialize(writer, value, value.GetType(), options)`), which the
  interface-scoped converter does not intercept, so there is no recursion and the `[DataMember]`
  body (incl. the `Union`/`Time` interval members) is reused as-is.

Harness `poc/StjAggReqTriage`: **9/9 full parity** (write + round-trip read) for filter,
percentiles, percentile_ranks, terms+order, terms+include, histogram+order, date_histogram
(calendar_interval), max_bucket (pipeline buckets_path), and composite (terms source).
