# OpenAPI Model Code Generator Generalization Plan

## Status

- **State:** Approved for incremental implementation
- **Baseline:** The plugin model generator introduced by PR #1017
- **Phase 0-1:** ✅ Complete — SchemaCatalog, identity tests, baseline validation
- **Phase 2:** ✅ Complete — OperationGroupModel, parameter aggregation, all 2xx responses, incompatibility diagnostics
- **Phase 3:** ✅ Complete — UnionClassifier, UnionModel IR, WrapperKeyOneOf/FlatWrapperKey/InternalDiscriminator/TypedKeys detection, 25 focused tests
- **Phase 4:** ✅ Complete — production ReferenceGraph reachability, explicit ownership/output roles, deterministic emission, and explicit public roots; 23 focused tests (66 total)
- **Phase 5:** ✅ Complete — production ingest generation enabled; generic `UnionRenderingPolicy` and dedicated policy-aware union renderer preserve `ProcessorBase`, generic descriptors, `Field`/`Fields` expression overloads, aliases, convenience overloads, and retained behavioral variants; 33 hand-written processor variants plus formatter/list descriptor replaced by generated output; 21 focused tests (87 total).
- **Phase 6:** ✅ Complete — document-scoped `SchemaNormalizer` with ordered passes (`AllOfPropertyCollectionPass`, `CompositionPreservationPass`, `DependencyCollectionPass`, `RequiredFieldPropagationPass`); recursive component/operation/inline schema discovery with deterministic synthetic identities; immutable normalized properties, required fields, composition, discriminator, and dependency facts; `NamespaceModel` and `ReferenceGraphBuilder` no longer interpret raw `allOf`; 33 focused tests (120 total), zero ML/SearchPipeline generated diff, and full build passing.

## Context

The plugin model generator currently converts the OpenAPI document parsed by NSwag into ML and search-pipeline high-level client models. It already provides useful target-language abstractions such as `ModelType`, `TypeRef`, plugin-specific overrides, and wrapper-key union generation.

The next step is not to copy the opensearch-java generator. It is to adopt the compiler-style separation that makes its generation pipeline predictable while retaining the .NET client's existing public API, partial-class structure, formatter conventions, and plugin migration model.

The intended pipeline is:

```text
OpenAPI YAML
    -> NSwag parser
    -> SchemaCatalog
    -> specification normalization
    -> semantic intermediate representation
    -> C# type projection
    -> reachability and validation
    -> Razor rendering
```

## Goal

Make model generation structural and spec-driven. Adding an operation, schema, or union variant that uses a supported OpenAPI pattern should require only a spec update and regeneration, without a new C# special case or hand-written model.

## Non-goals

- Reimplement the opensearch-java generator in C#.
- Replace NSwag in the initial phases.
- Change the existing public API as part of infrastructure-only phases.
- Move OpenAPI structural facts into plugin overrides.
- Migrate every OpenSearch namespace in one change.
- Rewrite Razor templates before the semantic model requires it.

## Design principles

### OpenAPI is the structural source of truth

The specification determines properties, required fields, wire names, variants, discriminators, composition, paths, and version metadata. C# source is the output target, not a secondary source for wire structure.

### Overrides are target-language policy

`IModelOverrides` may resolve C# naming collisions, map schemas to existing client types, exclude operations unsupported by the transport, and select output locations. It must not reconstruct schema structure that should have come from OpenAPI.

### Normalize before projecting

Equivalent OpenAPI encodings should be normalized before they are projected into C# types. Normalization preserves wire semantics and must not contain C# names or template decisions.

### Renderers do not interpret OpenAPI

Razor templates consume a complete semantic model. They must not inspect NSwag schemas or decide whether a schema is an object, union, dictionary, or inherited type.

### Unsupported structure is visible

A schema explicitly allowing arbitrary JSON may map to `object`. A schema that maps to `object` because the generator lost a reference or does not understand a union must produce a diagnostic and fail in strict CI mode.

### Migration is incremental

Each infrastructure phase must compile and pass tests independently. Unless a phase explicitly introduces support for a new schema pattern, regenerated output must remain unchanged.

## Target architecture

### SchemaCatalog

`SchemaCatalog` owns canonical schema identity immediately after NSwag parsing.

```text
component schema ID -> canonical JsonSchema
JsonSchema wrapper instance -> component schema ID
JsonSchema ActualSchema instance -> component schema ID
```

All later stages use this catalog instead of maintaining separate enum/object maps or rescanning `Components.Schemas` with reference equality.

### Specification normalization

Normalization is an ordered sequence of small transformations. Initial passes will cover:

1. `allOf: [$ref, object]` as base schema plus owned properties.
2. Missing type inference for composition schemas.
3. Success-response collection across all 2xx status codes.
4. Required-field calculation for supported composition forms.

Union information must remain intact until union classification.

### Semantic intermediate representation

The schema model will evolve toward:

```text
SchemaModel
├── ObjectModel
├── EnumModel
├── UnionModel
├── ArrayModel
├── DictionaryModel
└── AliasModel

OperationGroupModel
├── Variants
├── PathParameters
├── QueryParameters
├── RequestBody
└── SuccessResponses
```

`UnionModel` records wire encoding independently from plugin or template names:

```text
UnionEncoding
├── InternalDiscriminator
├── WrapperKeyOneOf
├── FlatWrapperKey
└── TypedKeys
```

Search-pipeline processor unions and ingest processor containers then become two encodings of the same semantic concept.

### C# type projection

`ModelTypeResolver` projects semantic schema references into structured `TypeRef` values. Resolution is memoized by canonical schema identity and records dependencies for reachability analysis.

### Reachability

Operation requests and responses are generation roots. The generator follows `TypeRef` dependencies to find every required model. A schema mapped to an existing C# type is an external leaf. Unreachable schemas are not emitted.

This replaces traversal-order-dependent suppression such as consumed union-body sets, operation-owned response sets, and second-pass duplicate removal.

### Diagnostics

Diagnostics include the schema ID, JSON pointer when available, and reason. Initial diagnostic categories are:

- unresolved schema reference;
- unsupported union encoding;
- conflicting operation variants;
- incompatible successful response schemas;
- unknown schema format;
- unexpected fallback to `object`.

The initial mode reports warnings. CI later enables strict mode for structural information loss.

## Implementation phases

### Phase 0 -- Baseline

1. Run code generation twice and verify deterministic output.
2. Record representative fixtures for named object references, enum references, composition, wrapper-key unions, flat wrapper-key objects, recursive schemas, and 2xx responses.
3. Establish an automated no-generated-diff check for infrastructure-only phases.

**Exit criteria:** Existing code generation completes with no tracked changes, and the affected projects build cleanly.

### Phase 1 -- Canonical schema identity

1. Add `SchemaCatalog`.
2. Route enum, object, array-item, dictionary-value, and union-body identity through the catalog.
3. Remove `_enumSchemaIds`, `_objectSchemaIds`, and component rescans from model construction.
4. Keep generated output unchanged.

**Exit criteria:** Identity tests pass for wrapper and resolved schema instances; regeneration produces no tracked changes.

### Phase 2 -- Operation groups

1. Add `OperationGroupModel` containing every operation with the same `x-operation-group`.
2. Aggregate path and query parameters across variants.
3. Calculate required path parameters from the intersection of supported paths.
4. Resolve all successful 2xx responses rather than only status 200.
5. Diagnose incompatible response schemas.

**Exit criteria:** Multiple paths, methods, and success status codes are represented without selecting an arbitrary first operation.

### Phase 3 -- General schema IR and union classification

1. Extract wrapper-key detection into `UnionClassifier`.
2. Represent union encoding and variants independently from rendering.
3. Migrate existing search-pipeline unions without changing generated behavior.
4. Add tests for internal-discriminator, wrapper-key-oneOf, flat-wrapper-key, and typed-key classifications.

**Exit criteria:** Union recognition has no plugin-name checks, and existing search-pipeline output remains stable.

### Phase 4 -- Reference graph and reachability

1. Record dependencies between semantic models through `TypeRef`.
2. Mark operation request/response models and explicit public models as roots.
3. Emit only reachable models.
4. Remove traversal-order-dependent duplicate filtering.

**Exit criteria:** Schema declaration order does not affect generated output, no duplicate type is emitted, and every generated model has a path from a root.

### Phase 5 -- Ingest processor migration

1. Classify `ProcessorContainer` as `FlatWrapperKey` using `minProperties: 1` and `maxProperties: 1`.
2. Reuse the union renderer to generate processor interfaces, variants, formatters, and fluent descriptors.
3. Add `IngestModelOverrides` containing only C# policy.
4. Remove hand-written processor artifacts replaced by generated output while retaining genuine behavioral base classes such as `InferenceProcessorBase`.

**Exit criteria:** Adding a processor to the spec and regenerating creates its model, formatter branch, and fluent descriptor method without C# generator changes.

### Phase 6 -- Specification normalization

1. Add small ordered normalization passes.
2. Move `allOf` property collection and inheritance interpretation out of `NamespaceModel`.
3. Preserve union and composition semantics in the intermediate representation.

**Exit criteria:** Model construction no longer performs ad hoc OpenAPI flattening.

### Phase 7 -- Strict diagnostics

1. Introduce structured diagnostics.
2. Distinguish intentional free-form JSON from unsupported or unresolved structure.
3. Add `--strict-model-codegen` and enable it in generated-code CI.

**Exit criteria:** Structural type loss cannot silently enter committed generated code.

## Delivery sequence

| Change | Scope | Expected generated output |
|---|---|---|
| PR A | Phase 0-1: baseline and `SchemaCatalog` | No change |
| PR B | Phase 2: operation grouping and all 2xx responses | Only correctness fixes |
| PR C | Phase 3-4: general union IR and reachability | No semantic change |
| PR D | Phase 5: ingest processor generation | Intentional replacement of hand-written code |
| PR E | Phase 6-7: normalization and strict diagnostics | No change except explicit correctness fixes |

## Validation gates

Every PR must run:

1. `dotnet clean` and `dotnet build` for `ApiGenerator`.
2. Focused `ApiGenerator.Tests` tests.
3. `./build.sh codegen --branch main --include-high-level` without downloading a different spec.
4. `git diff --exit-code` for infrastructure-only phases after regeneration.
5. The repository build and test command documented in `DEVELOPER_GUIDE.md` before handoff.

## First implementation boundary

Implementation starts with PR A only. It introduces the baseline checks, `SchemaCatalog`, resolver integration, and identity tests. It does not change union classification, operation behavior, Razor templates, or generated public APIs.
