# Model Code Generation — Design Reference

---

## Overall Architecture Flow

```
┌──────────────────────────────────────────────────────────────────────────────┐
│                         opensearch-openapi.yaml                              │
└─────────────────────────────────┬────────────────────────────────────────────┘
                                  │  NSwag / NJsonSchema parse
                                  │  + requestBody $ref inline
                                  ▼
┌──────────────────────────────────────────────────────────────────────────────┐
│  RestApiSpec   (existing — unchanged)                                        │
│    endpoint list, low-level descriptors, request parameters                  │
└───────────┬──────────────────────────────────────────────────────────────────┘
            │
            ├──────────────────────────────────────────────────────────────────►
            │  existing Razor generators (LowLevel, Descriptors, Requests, ...) 
            │
            ▼
┌──────────────────────────────────────────────────────────────────────────────┐
│  SchemaCatalog                                                               │
│                                                                              │
│  Built once from doc.Components.Schemas immediately after NSwag parsing.    │
│  Provides three canonical maps:                                              │
│    component schema ID  ──► canonical JsonSchema                             │
│    JsonSchema wrapper   ──► component schema ID                              │
│    JsonSchema actual    ──► component schema ID  (resolves NSwag inlining)  │
│                                                                              │
│  All later stages look up identity here; no stage rescans Components.Schemas │
│  with reference equality or maintains its own _enumSchemaIds map.            │
└─────────────────────────────────┬────────────────────────────────────────────┘
                                  │
                                  ▼
┌──────────────────────────────────────────────────────────────────────────────┐
│  SchemaNormalizer                                                            │
│                                                                              │
│  Ordered passes — each pass inspects parsed NSwag schema objects and writes   │
│  normalized facts into immutable records; no C# names or template decisions: │
│                                                                              │
│  1. AllOfPropertyCollectionPass                                              │
│       allOf: [$ref, {properties}]  ──►  base ref + owned property set       │
│  2. CompositionPreservationPass                                              │
│       keeps oneOf / anyOf / allOf semantics intact for UnionClassifier       │
│  3. DependencyCollectionPass                                                 │
│       recursive component + operation + inline schema discovery              │
│       assigns deterministic synthetic IDs to anonymous inline schemas        │
│  4. RequiredFieldPropagationPass                                             │
│       propagates required[] through allOf inheritance chains                 │
│                                                                              │
│  Output: immutable per-schema record with normalized properties,             │
│  required fields, composition, discriminator, and dependency facts.          │
└─────────────────────────────────┬────────────────────────────────────────────┘
                                  │
                                  ▼
┌──────────────────────────────────────────────────────────────────────────────┐
│  Semantic IR                                                                 │
│                                                                              │
│  OperationGroupModel                                                         │
│    Aggregates all operations sharing the same x-operation-group value.       │
│    ├── Variants        (GET /foo, POST /foo, …)                               │
│    ├── PathParameters  (intersection of required params across all paths)    │
│    ├── QueryParameters (union across all variants)                           │
│    ├── RequestBody     (application/json schema ref)                         │
│    └── SuccessResponses (all 2xx, with incompatibility diagnostic)           │
│                                                                              │
│  UnionClassifier                                                             │
│    Inspects normalized schema facts and classifies into one of:              │
│                                                                              │
│    WrapperKeyOneOf ── oneOf where every variant has exactly one              │
│    │                  required property whose value is an object schema.     │
│    │                  Wire format: {"<key>": { ...body... }}                 │
│    │                  Used by: search_pipeline processor unions               │
│    │                                                                         │
│    FlatWrapperKey  ── plain object with optional properties +                │
│    │                  minProperties:1, maxProperties:1.                      │
│    │                  Wire format: same as above.                            │
│    │                  Used by: ingest ProcessorContainer                     │
│    │                                                                         │
│    InternalDiscriminator ── oneOf/anyOf with a shared discriminator field    │
│    │                  whose value selects the concrete schema.               │
│    │                                                                         │
│    TypedKeys       ── object whose additional-properties value schemas       │
│                       are distinct named types keyed by a string identifier. │
│                                                                              │
│  ReferenceGraph                                                              │
│    Marks operation request/response models and explicit public roots.        │
│    Follows TypeRef dependencies to build the reachable set.                  │
│    Schemas mapped to existing C# types are external leaves.                  │
│    Unreachable schemas are never emitted.                                    │
│    Replaces traversal-order-dependent duplicate filtering.                   │
└────────────┬───────────────────────────────────────────────────────────┬─────┘
             │                                                           │
             ▼                                                           │
┌────────────────────────────────────────────┐                          │
│  ModelTypeResolver                         │                          │
│                                            │                          │
│  Projects a JsonSchema into a TypeRef.     │                          │
│  Resolution is memoized by canonical       │                          │
│  schema ID from SchemaCatalog.             │                          │
│  Resolution order:                         │                          │
│   1. MappedCsharpType override             │                          │
│   2. String enum $ref  → C# enum           │                          │
│   3. string  → string                      │                          │
│   4. boolean → bool?                       │                          │
│   5. integer → long? / int?                │                          │
│   6. number  → double? / float?            │                          │
│   7. array   → IList<I<T>> or IList<T>     │                          │
│   8. named object → I<TypeName>            │                          │
│   9. additionalProperties schema           │                          │
│         → IDictionary<string, I<V>>        │                          │
│  10. additionalProperties:true             │                          │
│         → IDictionary<string, object>      │                          │
│  11. bare $ref → I<TypeName>               │                          │
│  12. fallback → object  (+diagnostic)      │                          │
└────────────────────┬───────────────────────┘                          │
                     │                                                   │
                     ▼                                                   │
┌────────────────────────────────────────────────────────────────────────────┐
│  ModelsGenerator                                                           │
│                                                                            │
│  For each IModelOverrides plugin in EnabledPlugins:                        │
│                                                                            │
│  Pass 1 — Shared models (NamespaceModel.Build)                             │
│    Collects all named object schemas and string enums reachable within     │
│    the plugin's namespace. Emits one .g.cs per type via Model.cshtml.      │
│    WrapperKeyUnions are detected here and body schema IDs are recorded     │
│    to prevent double-emission.                                             │
│                                                                            │
│  Pass 2 — Body operations (when GenerateBodyOps = true)                    │
│    For each x-operation-group with an application/json request body:       │
│      OperationModel.Build() → RequestBodyPartial.cshtml (request body)    │
│                             → ResponseType.cshtml      (response POCO)    │
│      Any referenced enums not already emitted in Pass 1 are added.        │
│                                                                            │
│  Pass 3 — Non-body operations (when GenerateNonBodyOps = true)             │
│    GET/DELETE operations have no request body but still return typed        │
│    responses. Emits ResponseType.cshtml only (no request partial).         │
│                                                                            │
│  Union rendering (UnionRenderingPolicy)                                    │
│    Each UnionModel carries a policy that selects the correct template.     │
│    WrapperKeyOneOf / FlatWrapperKey → WrapperKeyUnion.cshtml               │
│    (future) InternalDiscriminator  → dedicated template                    │
│    SuppressedUnionSchemaIds prevents emission for suppressed unions        │
│    (e.g. ProcessorContainer while handwritten code remains authoritative). │
└─────────────────────────────────┬──────────────────────────────────────────┘
                                  │
                                  ▼
┌──────────────────────────────────────────────────────────────────────────────┐
│  Razor Templates                                                             │
│                                                                              │
│  Model.cshtml             — interface + class + descriptor for ObjectModel;  │
│                             also handles EnumModel via @if branching         │
│  RequestBodyPartial.cshtml— body-only partial class (properties + fluent)   │
│  ResponseType.cshtml      — response POCO                                   │
│  WrapperKeyUnion.cshtml   — full union: base interface, per-variant types,  │
│                             Utf8Json formatter, STJ converter, descriptor   │
│                             builder with one typed fluent method/variant    │
│                                                                              │
│  NOTE: Razor treats <T> inside <text> as HTML. All sections containing      │
│  generic <> are pre-built as StringBuilder and emitted via @Raw(...).        │
└─────────────────────────────────┬────────────────────────────────────────────┘
                                  │
                                  ▼
┌──────────────────────────────────────────────────────────────────────────────┐
│  _Generated/<OutputFolder>/                                                  │
│                                                                              │
│  Example — ml namespace (OutputFolder = "ML"):                                │
│    ML/<SharedModel>.g.cs          interface + class + descriptor             │
│    ML/<EnumName>.g.cs             [StringEnum] enum                          │
│    ML/<Op>Request.g.cs            request body partial                       │
│    ML/<Op>Response.g.cs           response POCO                              │
│    Descriptors.ML.cs              high-level descriptor extensions           │
│    Requests.ML.cs                 typed request / route-value wrappers       │
│    OpenSearchClient.ML.cs         IOpenSearchClient.ML fluent entry point    │
│                                                                              │
│  Example — search_pipeline (OutputFolder = "SearchPipeline/Generated"):      │
│    SearchPipeline/Generated/RequestProcessor.g.cs   (union — 5 variants)    │
│    SearchPipeline/Generated/ResponseProcessor.g.cs  (union — 9 variants)    │
│    SearchPipeline/Generated/PhaseResultsProcessor.g.cs (union — 2 variants) │
│    SearchPipeline/Generated/<Op>Request.g.cs                                 │
│    SearchPipeline/Generated/<Op>Response.g.cs                                │
│    SearchPipeline/Generated/<SharedModel>.g.cs  (ObjectModel / enum)        │
│    SearchPipeline/OpenSearchClient.SearchPipeline.cs                         │
│    SearchPipeline/Requests.SearchPipeline.cs                                 │
│    SearchPipeline/Descriptors.SearchPipeline.cs                              │
│                                                                              │
│  All files carry the auto-generated header warning and are deleted and       │
│  recreated on each codegen run.                                              │
└──────────────────────────────────────────────────────────────────────────────┘
```

---

## Design Principles

### OpenAPI is the structural source of truth
Properties, required fields, wire names, variants, discriminators, composition, paths,
and version metadata come from the spec. C# source is the output target, not a secondary
source for wire structure.

### Overrides are target-language policy
`IModelOverrides` resolves C# naming collisions, maps schemas to existing client types,
excludes unsupported operations, and selects output locations. It does not reconstruct
schema structure that belongs in OpenAPI.

### Normalize before projecting
Equivalent OpenAPI encodings (e.g. `allOf: [$ref, {inline properties}]`) are reduced to
a canonical form before C# projection. Normalization preserves wire semantics and contains
no C# names.

### Renderers do not interpret OpenAPI
Razor templates consume a complete semantic model. They never inspect raw NSwag schemas
or decide whether a schema is an object, union, dictionary, or inherited type.

### Serializer backends are adapters
Generated model semantics are shared across serializers. A union has one
`WrapperKeyUnionModel`, one variant list, and one wire-key mapping. Utf8Json and
System.Text.Json support are emitted as thin serializer-specific adapters over that
metadata, not as separate classification or naming logic.

For generated wrapper-key unions, the model interface carries both serializer attributes:

```csharp
[JsonFormatter(typeof(RequestProcessorFormatter))]
[System.Text.Json.Serialization.JsonConverter(typeof(RequestProcessorConverter))]
public interface IRequestProcessor
{
    string Name { get; }
}
```

The Utf8Json adapter implements `IJsonFormatter<T>`. The STJ adapter implements
`System.Text.Json.Serialization.JsonConverter<T>`. Both write the same wrapper-key
object shape and dispatch from the same generated variant metadata:

```json
{ "filter_query": { ... } }
```

Type-level `[JsonConverter]` is used for these generated unions because the converters
are stateless and type-specific. If a future union encoding requires
`IConnectionSettingsValues` or registration-order-sensitive behavior, that encoding
must use generated centralized STJ registration instead of a type-level attribute.

### Reachability drives emission
Operation requests and responses are generation roots. The generator traces `TypeRef`
dependencies to find every required model. Schemas mapped to an existing C# type are
external leaves. Unreachable schemas are never emitted — no traversal-order dependency,
no manual suppression lists (except `SuppressedUnionSchemaIds` for handwritten code).

### Migration is incremental
Each infrastructure phase must compile and pass tests independently. Unless a phase
explicitly introduces a new schema pattern, regenerated output must remain unchanged.

---

## Key Components

### `IModelOverrides` / `ModelOverridesBase`

`src/ApiGenerator/Configuration/Overrides/IModelOverrides.cs`

| Property | Purpose |
|---|---|
| `Namespace` | Spec namespace prefix, e.g. `"ml"` |
| `OutputFolder` | Subfolder under `_Generated/`, e.g. `"Ml"` |
| `GenerateBodyOps` | Emit request/response pairs for operations with a JSON body |
| `GenerateNonBodyOps` | Emit response-only types for operations without a JSON body |
| `IncludeOperationSchemasInReachability` | Whether operation schemas seed shared-model reachability |
| `ExcludedOps` | `x-operation-group` values to skip (e.g. streaming endpoints) |
| `OpNameOverrides` | Per-operation rename: spec operation group → C# base name |
| `RenamedTypes` | Per-schema rename: spec schema id → C# type name |
| `MappedCsharpType(id)` | Return an existing OSC type in place of generation |
| `SuppressLowLevelApiImport` | Emit per-type qualified aliases instead of namespace import |
| `SuppressedUnionSchemaIds` | Prevent union emission while handwritten code is authoritative |

`ModelOverridesBase` provides sensible defaults (empty collections, `false` flags).

### `SchemaCatalog`

`src/ApiGenerator/Domain/Code/HighLevel/Models/SchemaCatalog.cs`

Built once immediately after NSwag parsing. All later identity lookups go through here
instead of maintaining separate `_enumSchemaIds` / `_objectSchemaIds` maps or rescanning
`Components.Schemas` by reference equality.

### `SchemaNormalizer`

`src/ApiGenerator/Domain/Code/HighLevel/Models/SchemaNormalizer.cs`

Document-scoped. Runs four ordered passes that produce immutable normalized facts
consumed by `NamespaceModel`, `UnionClassifier`, and `ReferenceGraphBuilder`.
`NamespaceModel` no longer performs ad hoc OpenAPI flattening.

### `OperationGroupModel`

`src/ApiGenerator/Domain/Code/HighLevel/Models/OperationGroupModel.cs`

Aggregates all operations with the same `x-operation-group`. Resolves all 2xx responses,
not just status 200. Emits an incompatibility diagnostic when response schemas differ
across status codes.

### `UnionClassifier`

`src/ApiGenerator/Domain/Code/HighLevel/Models/UnionClassifier.cs`

Inspects normalized schema facts and returns a `UnionModel` with a `UnionEncoding`
discriminant (`WrapperKeyOneOf`, `FlatWrapperKey`, `InternalDiscriminator`, `TypedKeys`).
No plugin name checks. Detection results are independent of rendering.

### `WrapperKeyUnionModel` / `WrapperKeyVariant`

`src/ApiGenerator/Domain/Code/HighLevel/Models/ModelType.cs`

```
WrapperKeyUnionModel
  SchemaId        e.g. "search_pipeline._common___RequestProcessor"
  CsharpName      e.g. "RequestProcessor"
  FormatterName   e.g. "RequestProcessorFormatter" (Utf8Json)
  ConverterName   e.g. "RequestProcessorConverter" (System.Text.Json)
  BaseProperties  shared envelope props present in ALL variants
  Variants[]
    Key               wire discriminator key, e.g. "neural_query_enricher"
    CsharpName        C# type name, e.g. "NeuralQueryEnricher"
    VersionAdded      from x-version-added on the oneOf entry, or null
    BodyProperties    properties of the body schema
    FluentMethodName  PascalCase method name for the descriptor builder
```

### `UnionRenderingPolicy`

`src/ApiGenerator/Configuration/Overrides/UnionRenderingPolicy.cs`

Each `UnionModel` carries a policy that selects the correct Razor template.
`SuppressedUnionSchemaIds` on the plugin overrides prevents the union from becoming a
generation root, so neither it nor its dependent models are emitted — preserving
handwritten code without special-casing the template layer.

`WrapperKeyUnion.cshtml` emits both serializer adapters from the same
`WrapperKeyUnionModel`:

- `{CsharpName}Formatter : IJsonFormatter<I{CsharpName}>` for the legacy Utf8Json
  serializer path.
- `{CsharpName}Converter : JsonConverter<I{CsharpName}>` for the System.Text.Json
  serializer path.

The generated interface is attributed with both adapters, so adding a new generated
union does not require manual registration in `SystemTextJsonHighLevelSerializer`.

### `ReferenceGraph` / `ReferenceGraphBuilder`

`src/ApiGenerator/Domain/Code/HighLevel/Models/ReferenceGraph.cs`

Records dependencies through `TypeRef` as models are resolved. Marks operation
request/response models and explicit public roots. Provides the reachable set to
`ModelsGenerator`, replacing traversal-order-dependent duplicate filtering.

### `ModelTypeResolver`

`src/ApiGenerator/Generator/ModelTypeResolver.cs`

Projects a `JsonSchema` into a C# type string or `TypeRef`. Resolution is memoized by
canonical schema ID from `SchemaCatalog`. Uses `MappedCsharpType` overrides and
`SchemaCatalog` identity maps; no direct reference-equality scanning.

---

## Plugin Implementations

### `MlModelOverrides`

`src/ApiGenerator/Configuration/Overrides/Plugins/MlModelOverrides.cs`

- **Streaming ops excluded** — `ml.predict_model_stream` / `ml.execute_agent_stream`
  require chunked/SSE transport not yet available in the high-level client.
- **Op rename** — `ml.get_task` → `GetMLTask` (avoids collision with `Tasks.GetTask`).
- **Type renames** — eight schema ids prefixed with `Ml` to avoid collisions with BCL
  (`Task`, `Action`), OpenSearch.Net (`Node`), and OSC types (`Aggregation`, `Result`,
  `TaskState`, `IndexSettings`, `Metadata`).

### `SearchPipelineModelOverrides`

`src/ApiGenerator/Configuration/Overrides/Plugins/SearchPipelineModelOverrides.cs`

- `GenerateBodyOps = true` — `PutSearchPipelineRequest.g.cs` from `SearchPipelineStructure`.
- `GenerateNonBodyOps = true` — `GetSearchPipelineResponse.g.cs`, `DeleteSearchPipelineResponse.g.cs`.
- `SuppressLowLevelApiImport = true` — avoids `CS0104` from generic low-level names
  (`DeleteRequestParameters`, `GetRequestParameters`) that clash with top-level
  `OpenSearch.Net` classes. Templates emit per-type qualified `using` aliases instead.
- `OpNameOverrides` — aligns `put` / `get` / `delete` to `PutSearchPipeline` etc.
- `MappedTypes` — maps three processor union schema IDs to `IRequestProcessor`,
  `IResponseProcessor`, `IPhaseResultsProcessor` so array-item resolution emits
  `IList<IRequestProcessor>` rather than falling back to `IList<object>`.
- `RenamedTypes` — avoids collisions with ingest types: `SortResponseProcessor`,
  `SplitResponseProcessor` retain their names; `SearchPipelineStructure` → `SearchPipeline`;
  `RerankContext` → `SearchPipelineRerankContext`; `MLOpenSearchReranker` →
  `SearchPipelineMLOpenSearchReranker`.

### `IngestModelOverrides` (infrastructure only)

`src/ApiGenerator/Configuration/Overrides/Plugins/IngestModelOverrides.cs`

- `SuppressedUnionSchemaIds` contains `ProcessorContainer` — the `FlatWrapperKey` union
  is classified but never emitted.
- Handwritten `ProcessorFormatter.cs`, `ProcessorsDescriptor.cs`, and all per-processor
  types in `src/OpenSearch.Client/Ingest/Processors/` remain authoritative.
- Running production codegen does **not** delete, overwrite, or conflict with handwritten
  ingest files. The generic machinery is purely additive.

---

## `SuppressLowLevelApiImport` Principle

Plugin-model codegen is **purely high-level**. Any coupling to low-level generated code
(e.g. `OpenSearch.Net.Specification.*Api.*RequestParameters`) should be avoided. When a
low-level namespace uses generic parameter class names that clash with top-level
`OpenSearch.Net` types, the correct fix is to suppress the low-level import at the
high-level template layer, not to rename the low-level classes.

- Default `false` — existing namespaces (ML, ingest) that have no naming conflicts import
  the low-level namespace normally.
- `true` — Requests and Descriptors templates derive the set of `*RequestParameters`
  class names from the endpoint list and emit fully-qualified `using` aliases. Generated
  behavior is identical; there is no namespace-level import.

---

## Implementation Phase Status

| Phase | Description | Status |
|---|---|---|
| 0–1 | `SchemaCatalog`, identity tests, baseline validation | ✅ Complete |
| 2 | `OperationGroupModel`, parameter aggregation, all 2xx responses, incompatibility diagnostics | ✅ Complete |
| 3 | `UnionClassifier`, `UnionModel` IR, all four union encoding types, 25 tests | ✅ Complete |
| 4 | `ReferenceGraph` reachability, ownership/output roles, deterministic emission, 23 tests | ✅ Complete |
| 5 | `UnionRenderingPolicy` + `FlatWrapperKey` classification of `ProcessorContainer`; handwritten ingest preserved via `SuppressedUnionSchemaIds`; 21 tests | ✅ Complete (infrastructure) |
| 6 | `SchemaNormalizer` with four ordered passes; `NamespaceModel` / `ReferenceGraphBuilder` no longer interpret raw `allOf`; 33 tests (120 total) | ✅ Complete |
| 7 | Structured diagnostics; `--strict-model-codegen`; CI strict mode | 🔲 Not started |

---

## Diagnostics (Phase 7 — planned)

Diagnostics include the schema ID, JSON pointer when available, and reason.

| Category | Meaning |
|---|---|
| `unresolved-schema-ref` | A `$ref` target was not found in the catalog |
| `unsupported-union-encoding` | Union did not match any known encoding |
| `conflicting-operation-variants` | Two variants of an operation group disagree on parameter shapes |
| `incompatible-success-responses` | 2xx responses within one operation group have incompatible schemas |
| `unknown-schema-format` | `format` value is not handled by `ModelTypeResolver` |
| `object-fallback` | A schema resolved to `object` due to unrecognized structure (information loss) |

Initial mode: warnings. `--strict-model-codegen` makes structural information loss a CI
failure. The check **Ensure Generated Code Up To Date** re-runs codegen and fails if any
`.g.cs` file differs from what is committed.

---

## Adding a New Plugin

1. Create `src/ApiGenerator/Configuration/Overrides/Plugins/<Name>ModelOverrides.cs`
   extending `ModelOverridesBase`. Set `Namespace`, `OutputFolder`, and any needed
   renames or exclusions.
2. Add an instance to `ModelsGenerator.EnabledPlugins`.
3. Run codegen (see below).
4. Commit the generated files alongside the overrides class.

### Rename Checklist

All generated types land in the flat `OpenSearch.Client` namespace. Before committing,
verify no collision with:

- BCL types: `System.Action`, `System.Threading.Tasks.Task`, `System.Net.Node`, …
- `OpenSearch.Net` types: `Node`, …
- OSC types: `IAggregation`/`Aggregation`, `Result`, `TaskState`, `IndexSettings`, …
- Test project types (e.g. `Tests.Domain.Metadata` caused `CS0104` for `ml.Metadata`).

Use `RenamedTypes` in the overrides class to apply `<Plugin><Name>` prefixes as needed.

### Union Array-Item Resolution

If the plugin has schemas whose instances appear as array items and those schemas are
`oneOf` unions (not plain objects), add them to `MappedTypes`:

```csharp
public override string? MappedCsharpType(string schemaId) => schemaId switch
{
    "myplugin._common___MyUnion" => "IMyUnion",
    _ => null
};
```

Without this, `ModelTypeResolver` cannot discover the C# name from `_objectSchemaIds`
alone and falls back to `IList<object>`.

> **Planned improvement (Phase 7 / follow-up):** auto-register
> `{unionSchemaId} → I{Name}` inside `NamespaceModel.Build` after a successful
> `TryBuildWrapperKeyUnion` call, making `MappedTypes` optional for union namespaces.

---

## Ingest Processor Migration Roadmap

The ingest namespace has 36 processor types in `ProcessorContainer`. Currently all are
hand-written. Phase 5 classified `ProcessorContainer` as `FlatWrapperKey` but suppressed
emission to preserve handwritten code. The migration path when ready:

1. Remove `ProcessorContainer` from `SuppressedUnionSchemaIds` in `IngestModelOverrides`.
2. Verify the `WrapperKeyUnion.cshtml` output matches the hand-written `ProcessorFormatter`
   and `ProcessorsDescriptor` behavior.
3. Delete the 36 hand-written processor files and `ProcessorFormatter.cs` /
   `ProcessorsDescriptor.cs`.
4. Retain `InferenceProcessorBase.cs` and `InferenceProcessorDescriptorBase` — they
   provide the reusable `model_id` + `field_map` abstraction for all ML inference
   processors and have no spec equivalent.

---

## Running Code Generation

```bash
# From the repo root — downloads the latest spec and regenerates everything
dotnet run --project src/ApiGenerator -- --download --include-high-level

# Skip download if opensearch-openapi.yaml is already present
dotnet run --project src/ApiGenerator -- --include-high-level
```

### Validation Gates (every PR)

1. `dotnet clean` and `dotnet build` for `ApiGenerator`.
2. Focused `ApiGenerator.Tests` tests.
3. `./build.sh codegen --include-high-level` without downloading a new spec.
4. `git diff --exit-code` after regeneration (infrastructure-only phases must produce no diff).
5. Full repository build and test per `DEVELOPER_GUIDE.md`.

## Design Notes

### Property-level type overrides (field-level override)

**Status:** ✅ Implemented

`PropertyTypeOverrides` in `IModelOverrides` allows per-property C# type overrides
keyed by `"{operationGroup}.{propertyName}"` or `"{schemaId}.{propertyName}"`.
`GlobalPropertyTypeOverrides` in `ModelOverridesBase` provides schema-scoped overrides
shared across all plugins (e.g. `TypeMapping.properties` → `IProperties`).

Resolution order in `ResolvePropertyTypeOverride`:
1. Per-plugin operation-scoped key (`"{operationGroup}.{wireName}"`)
2. Global schema-scoped key (`"{schemaId}.{wireName}"`)

This handles inline schemas that have no named component ID (e.g. `aliases` in
`indices.create`). The hand-written code uses `IAliases` — a wrapper dictionary type
with `IndexName` keys and `VerbatimDictionaryKeysFormatter`. The codegen override maps
the inline schema property to the correct wrapper type.
