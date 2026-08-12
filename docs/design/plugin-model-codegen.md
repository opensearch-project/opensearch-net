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
