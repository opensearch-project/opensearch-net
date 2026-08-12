# Plugin Model Code Generation

This document describes the design of the plugin-scoped model code generator introduced
to produce typed high-level client types (request bodies, response classes, shared models,
and enums) for OpenSearch plugin namespaces directly from the
[opensearch-api-specification](https://github.com/opensearch-project/opensearch-api-specification).

## Motivation

The existing ApiGenerator covers core OpenSearch APIs end-to-end (low-level requests,
high-level descriptors, enumerations). Plugin namespaces such as `ml` were previously
hand-maintained, meaning new endpoints required manual POCO authoring and were prone to
falling out of sync with the specification.

The plugin code generator extends the existing pipeline to produce the same classes for
any plugin namespace automatically, while giving each plugin the flexibility to apply
renames, exclusions, and type mappings that make the generated output consistent with
the rest of the high-level client.

## Architecture Overview

```
opensearch-openapi.yaml
        |
        v
  ApiGenerator.CreateRestApiSpecModel()
        |  (NSwag/NJsonSchema parse + requestBody ref inline)
        v
    RestApiSpec
        |
        +---> existing Razor generators (low-level, descriptors, requests, ...)
        |
        +---> ModelsGenerator  <-- NEW
                    |
                    for each IModelOverrides plugin:
                    |
                    +-- NamespaceModel.Build()   (shared model/enum types)
                    |
                    +-- OperationModel.Build()   (per-op request + response)
                    |
                    +-- ModelTypeResolver        (schema -> C# type)
                    |
                    v
             _Generated/<OutputFolder>/*.g.cs
```

## Key Components

### `IModelOverrides` / `ModelOverridesBase`

`src/ApiGenerator/Configuration/Overrides/IModelOverrides.cs`

Defines the contract each plugin must implement:

| Property | Purpose |
|---|---|
| `Namespace` | Spec namespace prefix, e.g. `"ml"` |
| `OutputFolder` | Subfolder under `_Generated/`, e.g. `"Ml"` |
| `GenerateBodyOps` | Emit request/response pairs for operations with a JSON body |
| `GenerateNonBodyOps` | Emit response-only types for operations without a JSON body |
| `UseObjectSchemaIds` | Enable reverse-lookup of object schemas by instance identity (needed when NSwag inlines `$ref`s) |
| `ExcludedOps` | Set of `x-operation-group` values to skip (e.g. streaming endpoints) |
| `OpNameOverrides` | Per-operation rename map: spec operation group -> C# base name |
| `RenamedTypes` | Per-schema rename map: spec schema id -> C# type name |
| `MappedCsharpType(id)` | Return an existing OSC type to use in place of generation |

`ModelOverridesBase` provides sensible defaults (empty collections, `false` flags).

### `MlModelOverrides`

`src/ApiGenerator/Configuration/Overrides/Plugins/MlModelOverrides.cs`

The concrete overrides for the `ml` namespace. Key decisions:

- **Streaming ops excluded** -- `ml.predict_model_stream` and `ml.execute_agent_stream`
  require chunked/SSE transport not yet available in the high-level client.
- **Op rename** -- `ml.get_task` -> `GetMlTask` to avoid a flat-namespace collision with
  `Tasks.GetTask`.
- **Type renames** -- eight schema ids are prefixed with `Ml` to avoid collisions with
  BCL types (`Task`, `Action`), OpenSearch.Net types (`Node`), and OSC types
  (`Aggregation`, `Result`, `TaskState`, `IndexSettings`, `Metadata`).

To add a new plugin, create a class that extends `ModelOverridesBase`, add it to
`ModelsGenerator.EnabledPlugins`, and run code generation.

### `ModelsGenerator`

`src/ApiGenerator/Generator/Razor/ModelsGenerator.cs`

Orchestrates generation for all enabled plugins in two passes:

1. **Shared models pass** -- calls `NamespaceModel.Build()` to collect all named object
   schemas and string enums referenced within the plugin's namespace, then emits one
   `.g.cs` file per type using `Model.cshtml`.

2. **Body operations pass** (when `GenerateBodyOps = true`) -- for each operation group
   that has a `application/json` request body, calls `OperationModel.Build()` and emits:
   - `<BaseName>Request.g.cs` via `RequestBodyPartial.cshtml`
   - `<BaseName>Response.g.cs` via `ResponseType.cshtml`
   - Any enums referenced by the request/response that were not already emitted in pass 1.

3. **Non-body operations pass** (when `GenerateNonBodyOps = true`) -- for operations
   without a JSON body, emits response-only files via `ResponseType.cshtml`.

### `ModelTypeResolver`

`src/ApiGenerator/Generator/ModelTypeResolver.cs`

Translates a `JsonSchema` into a C# type string or `TypeRef` using these rules (in order):

1. String enum `$ref` -> generated C# enum (nullable value type), respecting
   `RenamedTypes` and `MappedCsharpType`.
2. `string` -> `string`
3. `boolean` -> `bool?`
4. `integer` (int64) -> `long?`, otherwise `int?`
5. `number` (double) -> `double?`, otherwise `float?`
6. `array` -> `IList<I<ElementType>>` for object refs, `IList<T>` for scalars.
7. Named object with properties -> `I<TypeName>` (interface reference), resolved via
   direct `$ref` or the reverse schema-instance lookup (see below).
8. Object with `additionalProperties` schema -> `IDictionary<string, I<ValueType>>`
9. Object with `additionalProperties: true` -> `IDictionary<string, object>`
10. Bare `$ref` -> `I<TypeName>`
11. Fallback -> `object`

**Reverse schema-instance lookup** addresses an NSwag behaviour: when NSwag resolves a
`$ref`, it inlines the target schema, losing the original `Reference.Id`. Two reverse
maps are built at startup:

- `_enumSchemaIds` -- `JsonSchema instance -> schema id` for enum types.
- `_objectSchemaIds` -- `JsonSchema instance -> schema id` for named object types
  (enabled only when `UseObjectSchemaIds = true`).

These maps restore the schema id from the actual resolved instance, enabling the
resolver to emit the correct named C# type instead of falling back to `string` or
`IDictionary<string, object>`.

### Output Layout

For the `ml` namespace with `OutputFolder = "Ml"`:

```
src/OpenSearch.Client/_Generated/
  Ml/
    <SharedModel>.g.cs      -- interface + class + descriptor triple for each shared type
    <EnumName>.g.cs         -- [StringEnum] enum with [EnumMember] wire values
    <Op>Request.g.cs        -- interface + class + descriptor for the request body
    <Op>Response.g.cs       -- response POCO
  Descriptors.Ml.cs         -- high-level descriptor extensions (Razor-generated)
  Requests.Ml.cs            -- typed request/route-values wrappers (Razor-generated)
  OpenSearchClient.Ml.cs    -- IOpenSearchClient.Ml fluent entry point (Razor-generated)
```

All files carry the auto-generated header warning and are deleted and recreated on each
codegen run.

## Running Code Generation

```bash
# From the repo root -- downloads the latest spec and regenerates everything
dotnet run --project src/ApiGenerator -- --branch main --include-high-level --download

# Skip download if opensearch-openapi.yaml is already present
dotnet run --project src/ApiGenerator -- --branch main --include-high-level
```

The `--branch` value selects the GitHub release tag to download:
`https://github.com/opensearch-project/opensearch-api-specification/releases/download/<branch>-latest/opensearch-openapi.yaml`.

The CI check **Ensure Generated Code Up To Date** re-runs codegen and fails if any
`.g.cs` file differs from what is committed, ensuring generated files are never stale.

## Adding a New Plugin

1. Create `src/ApiGenerator/Configuration/Overrides/Plugins/<Name>ModelOverrides.cs`
   extending `ModelOverridesBase`. Set `Namespace`, `OutputFolder`, and any needed
   renames or exclusions.
2. Add an instance to `ModelsGenerator.EnabledPlugins`.
3. Run codegen (see above).
4. Commit the generated files alongside the overrides class.

### Rename Checklist

All generated types land in the flat `OpenSearch.Client` namespace. Before committing,
verify no collision exists with:

- BCL types: `System.Action`, `System.Threading.Tasks.Task`, `System.Net.Node`, ...
- `OpenSearch.Net` types: `Node`, ...
- OSC types: `IAggregation`/`Aggregation`, `Result`, `TaskState`, `IndexSettings`, ...
- Test project types (e.g. `Tests.Domain.Metadata` caused `CS0104` for `ml.Metadata`).

Use the `RenamedTypes` dictionary in the overrides class to apply `Ml<Name>` prefixes as
needed (see `MlModelOverrides` for examples).

## Wrapper-Key Discriminated Unions

Some OpenAPI spec namespaces (e.g. `search_pipeline`) model their extensible type lists
as a **wrapper-key discriminated union** rather than a flat object:

```yaml
# spec: search_pipeline._common.yaml
RequestProcessor:
  oneOf:
    - title: neural_query_enricher
      properties:
        neural_query_enricher:
          $ref: '#/components/schemas/NeuralQueryEnricherRequestProcessor'
      required: [neural_query_enricher]
    - title: filter_query
      properties:
        filter_query:
          $ref: '#/components/schemas/FilterQueryRequestProcessor'
      required: [filter_query]
    - ...
```

The wire format is `{"<key>": { ...body... }}` — a single-property object whose key
names the processor type. This is structurally different from the `type`-discriminated
unions handled by the flat `ObjectModel` path and requires dedicated codegen support.

### Detection

`NamespaceModel.TryBuildWrapperKeyUnion()` detects this pattern by checking that:

1. The schema has a non-empty `oneOf`.
2. Every variant has exactly one required property.
3. That property's value is an object schema (either inline or via `$ref`).

When detection succeeds, a `WrapperKeyUnionModel` is returned instead of an
`ObjectModel`, and the body schema IDs are recorded in `unionBodySchemaIds` to prevent
them from also being emitted as standalone `ObjectModel` files (which would cause
duplicate-type compile errors).

**NSwag `$ref` inlining** — when NSwag resolves a `$ref` it may inline the target
schema and drop `Reference.Id`. The detection code recovers the body schema ID by
scanning `doc.Components.Schemas` for the same schema instance by reference equality.

### `WrapperKeyUnionModel` / `WrapperKeyVariant`

`src/ApiGenerator/Domain/Code/HighLevel/Models/ModelType.cs`

```
WrapperKeyUnionModel
  SchemaId        e.g. "search_pipeline._common___RequestProcessor"
  CsharpName      e.g. "RequestProcessor"
  BaseProperties  shared envelope props present in ALL variants (tag, description, ...)
  Variants[]
    Key             wire discriminator key, e.g. "neural_query_enricher"
    CsharpName      C# type name for this variant, e.g. "NeuralQueryEnricher"
    VersionAdded    from x-version-added on the oneOf entry, or null
    BodyProperties  properties of the body schema (excluding shared base props)
    FluentMethodName  PascalCase method name for the descriptor builder
```

### `WrapperKeyUnion.cshtml`

`src/ApiGenerator/Views/HighLevel/WrapperKeyUnion.cshtml`

A single template renders the entire union — four artifacts in one file:

| Artifact | Description |
|---|---|
| Base interface | `[JsonFormatter(typeof({Name}Formatter))] public interface I{Name}` with `string Name { get; }` and shared base properties |
| Per-variant types | `interface I{Variant} : I{Name}`, `class {Variant} : I{Variant}`, `class {Variant}Descriptor` with fluent setter methods |
| Formatter | `internal class {Name}Formatter : IJsonFormatter<I{Name}>` using `AutomataDictionary` for O(1) key dispatch |
| Descriptor builder | `public class {Name}sDescriptor : DescriptorPromiseBase<..., IList<I{Name}>>` with one typed fluent method per variant |

**Razor `<>` as HTML** — Razor treats `<TypeArg>` inside `<text>` blocks as HTML tags,
corrupting generic type arguments. All code sections that contain generic `<>` (formatter
class declaration, switch cases, fluent builder methods) are pre-built as `StringBuilder`
strings and emitted via `@Raw(...)` to bypass Razor's HTML parser.

### `SearchPipelineModelOverrides`

`src/ApiGenerator/Configuration/Overrides/Plugins/SearchPipelineModelOverrides.cs`

Plugs the `search_pipeline` namespace into `ModelsGenerator.EnabledPlugins` and generates
**all** typed artifacts from the spec — processor hierarchies, request/response partials,
and response-only types — without any hand-written files.

Key settings:

- `GenerateBodyOps = true` — generates `PutSearchPipelineRequest.g.cs` (body properties
  and descriptor fluent methods) from `SearchPipelineStructure`.
- `GenerateNonBodyOps = true` — generates `GetSearchPipelineResponse.g.cs` and
  `DeleteSearchPipelineResponse.g.cs`.
- `SuppressLowLevelApiImport = true` — tells the Requests and Descriptors Razor
  generators to omit `using OpenSearch.Net.Specification.SearchPipelineApi;` and instead
  emit per-type `using Alias = FullyQualified.Type;` aliases. This is necessary because
  the low-level search pipeline namespace uses generic names (`DeleteRequestParameters`,
  `GetRequestParameters`) that are identical to top-level `OpenSearch.Net` classes,
  causing CS0104 ambiguity. Setting this flag keeps the high-level client completely
  independent of the low-level namespace.
- `OpNameOverrides` — aligns codegen names (`put` → `PutSearchPipeline`, etc.) with the
  existing high-level method naming.
- `MappedTypes` — maps the three processor union schema IDs to their interface names
  (`IRequestProcessor`, `IResponseProcessor`, `IPhaseResultsProcessor`). Required because
  union container schemas are `oneOf` (not plain object schemas with properties), so
  `ModelTypeResolver` cannot discover their C# names from `_objectSchemaIds` alone.
  Without this mapping, array item resolution falls back to `IList<object>`.
- `RenamedTypes` — avoids collisions with existing OSC types: `SortResponseProcessor`
  → `SearchPipelineSort`, `SearchScriptRequestProcessor` → `SearchScript`, etc.

`CodeConfiguration.HighLevelOnlyApiNameOverrides` carries entries for
`search_pipeline.put/get/delete` to align the Requests/Descriptors generators with the
ModelsGenerator operation names. No low-level API renaming is required or applied.

### `SuppressLowLevelApiImport` — Principle

Plugin-model code generation is **purely high-level**. Any coupling to low-level
generated code (e.g. `OpenSearch.Net.Specification.*Api.*RequestParameters`) should be
avoided. When a low-level namespace uses generic parameter class names that clash with
top-level `OpenSearch.Net` types, the correct fix is to suppress the low-level import
at the high-level template layer, not to rename the low-level classes.

The `SuppressLowLevelApiImport` flag on `IModelOverrides` encodes this intent:

- Default `false` — existing namespaces (ML, ingest, ...) that do not have naming
  conflicts continue to import the low-level namespace normally.
- `true` — the Requests and Descriptors templates iterate the endpoint list, derive the
  set of `*RequestParameters` class names, and emit fully-qualified `using` aliases
  instead of the blanket import. The generated code is identical in behaviour but does
  not depend on a namespace-level import.

### Output Layout

### Output Layout

For the `search_pipeline` namespace with `OutputFolder = "SearchPipeline/Generated"`:

```
src/OpenSearch.Client/_Generated/
  SearchPipeline/
    Generated/
      RequestProcessor.g.cs            -- IRequestProcessor + 5 variant types + formatter + builder
      ResponseProcessor.g.cs           -- IResponseProcessor + 9 variant types + formatter + builder
      PhaseResultsProcessor.g.cs       -- IPhaseResultsProcessor + 2 variant types + formatter + builder
      PutSearchPipelineRequest.g.cs    -- body partial: Description, RequestProcessors, ...
      PutSearchPipelineResponse.g.cs   -- AcknowledgedResponseBase subclass
      GetSearchPipelineResponse.g.cs   -- GetSearchPipelineResponse POCO
      DeleteSearchPipelineResponse.g.cs
      ScoreCombination.g.cs            -- shared body schema (ObjectModel)
      ScoreNormalization.g.cs
      ScoreRankerCombination.g.cs
      ScoreCombinationTechnique.g.cs   -- [StringEnum] enum
      ScoreNormalizationTechnique.g.cs
      ScoreRankerCombinationTechnique.g.cs
      SearchPipelineMLOpenSearchReranker.g.cs
      SearchPipelineRerankContext.g.cs
      SearchPipelineStructure.g.cs     -- top-level pipeline body schema (ObjectModel)
    OpenSearchClient.SearchPipeline.cs -- generated client namespace
    Requests.SearchPipeline.cs
    Descriptors.SearchPipeline.cs
```

No hand-written files remain for the `search_pipeline` namespace.

### Adding a New Search Pipeline Processor

When OpenSearch adds a new processor to the spec:

1. Add the variant to `RequestProcessor` / `ResponseProcessor` / `PhaseResultsProcessor`
   `oneOf` in `spec/schemas/search_pipeline._common.yaml`.
2. Add the body schema (e.g. `MyNewRequestProcessor`) to the same file.
3. Run codegen — the new variant is generated automatically:
   - A new `interface IMyNew : IRequestProcessor` + class + descriptor
   - A new `case "my_new":` branch in `RequestProcessorFormatter`
   - A new `MyNew(Func<MyNewDescriptor, IMyNew> selector)` method in `RequestProcessorsDescriptor`
4. If the new type name collides with an existing OSC type, add a rename to
   `SearchPipelineModelOverrides.RenamedTypes`.

### Generality Assessment

The infrastructure added for `search_pipeline` is largely general, with one known
limitation that requires per-plugin manual steps today:

| Component | General? | Notes |
|---|---|---|
| `WrapperKeyUnion` detection + codegen | ✅ Fully general | Any `oneOf` wrapper-key union in any namespace auto-generates |
| `ModelTypeResolver` array-item reverse lookup | ✅ Fully general | All namespaces benefit from the `_objectSchemaIds` + `MappedCsharpType` fix |
| `BuildObjectSchemaIds` oneOf tracking | ✅ Fully general | All oneOf unions are now in the reverse map |
| `WrapperKeyUnionModel` emit-filter fix | ✅ Fully general | Union schemas with `MappedTypes` entries are never suppressed from generation |
| `SuppressLowLevelApiImport` flag | ✅ Fully general | Any plugin that does not need the low-level namespace import can set this. The templates automatically emit per-type qualified aliases from the endpoint list — no hardcoding required. |
| `MappedTypes` for union array-item resolution | ⚠️ Manual per plugin | Each plugin must list `{unionSchemaId} → I{Name}` for every union whose instances appear as array items. A future improvement would auto-register these during `TryBuildWrapperKeyUnion` so plugins don't need to specify them. |

**Design principle**: plugin-model codegen is **purely high-level**. No modifications to
the low-level API (`LowLevelApiNameMapping`, `RequestParameters` class renaming, etc.)
should be needed. If a naming conflict arises between a low-level namespace and the
top-level `OpenSearch.Net` namespace, the fix belongs in the high-level template layer
(`SuppressLowLevelApiImport`), not in the low-level generator.

**Remaining improvement path**: auto-register `{unionSchemaId} → I{Name}` in
`NamespaceModel.Build` after a successful `TryBuildWrapperKeyUnion` call, injecting the
mapping into the resolver directly. This would make `MappedTypes` optional for union
namespaces.


## Ingest Processor Codegen Roadmap

The ingest namespace has 36 processor types defined in `ProcessorContainer` in the spec.
Currently all of them are hand-written. The spec now includes schemas for the three neural
search processors (`sparse_encoding`, `text_image_embedding`, `text_chunking`) added in
the same PR as this codegen infrastructure.

### Current State

| File | Lines | Status |
|---|---|---|
| `InferenceProcessorBase.cs` | 119 | Hand-written (base class, stays) |
| `TextEmbeddingProcessor.cs` | 32 | Hand-written |
| `SparseEncodingProcessor.cs` | 31 | Hand-written — spec schema added |
| `TextImageEmbeddingProcessor.cs` | 31 | Hand-written — spec schema added |
| `TextChunkingProcessor.cs` | 257 | Hand-written — spec schema added |
| `ProcessorFormatter.cs` | ~380 | Hand-written (dispatcher) |
| `ProcessorsDescriptor.cs` | ~200 | Hand-written (fluent builder) |
| 29 other processor files | ~2000 | Hand-written |

### Migration Path

The `ProcessorContainer` schema uses optional `properties` (not `oneOf`) with
`minProperties: 1, maxProperties: 1`, where each property key is the processor name.
This is structurally equivalent to the wrapper-key union pattern used by search pipeline:

```yaml
ProcessorContainer:
  type: object
  properties:
    text_embedding:
      $ref: '#/components/schemas/TextEmbeddingProcessor'
    sparse_encoding:
      $ref: '#/components/schemas/SparseEncodingProcessor'
    ...
  minProperties: 1
  maxProperties: 1
```

To auto-generate ingest processors:

1. **Extend `TryBuildWrapperKeyUnion`** to detect the flat-property pattern
   (`minProperties: 1, maxProperties: 1` object) in addition to the `oneOf` pattern.
   The resulting `WrapperKeyUnionModel` with `CsharpName = "Processor"` would drive
   generation of `IProcessor`, `ProcessorFormatter`, and `ProcessorsDescriptor`.

2. **Create `IngestModelOverrides`** (namespace `"ingest"`, `OutputFolder = "Ingest/Generated"`).
   Map `IProcessor` to the existing type so it doesn't generate a second base interface.
   Use `SuppressLowLevelApiImport = true`.

3. **Delete the 36 hand-written processor files** once the generated output matches.

4. **Update `ProcessorFormatter.cs`** to be generated — this is the largest change since
   the formatter today uses `value.Name` dispatch from the `IProcessor.Name` property,
   which the `WrapperKeyUnion.cshtml` template already emits correctly.

The `InferenceProcessorBase.cs` and `InferenceProcessorDescriptorBase` would remain
hand-written as they provide the reusable `model_id` + `field_map` abstraction for all
ML inference processors.

### Why Not in PR #1017

This migration touches all 36 processor files (~2600 lines), the formatter, and the
descriptor — representing a significant refactor separate from the ML high-level client
work that is the primary focus of PR #1017. It is tracked as a follow-up.
