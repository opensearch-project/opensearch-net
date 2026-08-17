/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using NJsonSchema;
using NSwag;

namespace ApiGenerator.Domain.Code.HighLevel.Models;

/// <summary>
/// Ordered normalization pipeline over the parsed OpenAPI document.
/// Runs small deterministic passes in sequence and produces immutable
/// <see cref="NormalizedSchema"/> facts keyed by canonical schema ID.
///
/// One instance is created per <see cref="OpenApiDocument"/> and its result
/// is shared across ALL plugin resolvers and downstream consumers.
///
/// The normalizer does NOT mutate the raw OpenAPI document.
///
/// Phase 6 closure: The normalizer recursively discovers and registers EVERY schema
/// instance reachable from document components AND inline operation request/response
/// schemas, following properties, item, additionalProperties, allOf, oneOf, anyOf.
/// Canonical component schemas use SchemaCatalog IDs; all other instances receive
/// stable deterministic synthetic IDs. The frozen NormalizationResult maps schema
/// instances (by reference equality) to their normalized ID, so TryGetForSchema
/// works for inline wrappers and ActualSchema instances.
/// </summary>
public sealed class SchemaNormalizer
{
    private readonly SchemaCatalog _catalog;
    private readonly IReadOnlyList<INormalizationPass> _passes;

    /// <summary>
    /// Creates a normalizer with the standard ordered pass sequence.
    /// </summary>
    public SchemaNormalizer(SchemaCatalog catalog)
        : this(catalog, DefaultPasses())
    {
    }

    /// <summary>
    /// Creates a normalizer with an explicit pass sequence (for testing).
    /// </summary>
    public SchemaNormalizer(SchemaCatalog catalog, IReadOnlyList<INormalizationPass> passes)
    {
        _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
        _passes = passes ?? throw new ArgumentNullException(nameof(passes));
    }

    /// <summary>
    /// Runs the normalization pipeline over the document and returns the result.
    /// The result is reusable across all consumers for this document.
    /// </summary>
    public NormalizationResult Normalize(OpenApiDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        var context = new NormalizationContext(_catalog, document);

        // Phase 6: Discover and register ALL reachable schema instances BEFORE
        // running normalization passes. This ensures passes operate over the complete
        // schema universe (component + inline), and that every schema has a stable ID.
        DiscoverAllSchemas(context, document);

        // Execute passes in declared order. Each pass writes to the context.
        foreach (var pass in _passes)
            pass.Execute(context);

        return context.Freeze();
    }

    /// <summary>Returns the default ordered pass sequence.</summary>
    public static IReadOnlyList<INormalizationPass> DefaultPasses() => new INormalizationPass[]
    {
        new AllOfPropertyCollectionPass(),
        new CompositionPreservationPass(),
        new DependencyCollectionPass(),
        new RequiredFieldPropagationPass(),
    };

    /// <summary>
    /// Recursively discovers and registers every schema instance reachable from:
    /// 1. Document.Components.Schemas (canonical component schemas)
    /// 2. Inline operation request/response schemas
    ///
    /// Uses reference-identity cycle protection. Component schemas get their
    /// SchemaCatalog ID; inline schemas get stable deterministic synthetic IDs
    /// based on deterministic traversal order (sorted paths, sorted status codes).
    /// </summary>
    private static void DiscoverAllSchemas(NormalizationContext context, OpenApiDocument document)
    {
        var visited = new HashSet<JsonSchema>(ReferenceEqualityComparer.Instance);

        // 1. Component schemas (deterministic: sorted by key)
        foreach (var (schemaId, schema) in document.Components.Schemas
            .OrderBy(kv => kv.Key, StringComparer.Ordinal))
        {
            var actual = schema.ActualSchema;
            // Register the component schema with its catalog ID
            context.RegisterSchema(actual, schemaId);
            // Traverse its structural children
            TraverseAndRegister(context, actual, visited);
        }

        // 2. Inline operation schemas (deterministic: sorted paths, sorted methods, sorted status codes)
        foreach (var (path, pathItem) in document.Paths
            .OrderBy(kv => kv.Key, StringComparer.Ordinal))
        {
            foreach (var (method, op) in pathItem
                .OrderBy(kv => kv.Key))
            {
                // Request body schemas
                if (op.ActualRequestBody?.Content != null)
                {
                    foreach (var (mediaType, content) in op.ActualRequestBody.Content
                        .OrderBy(kv => kv.Key, StringComparer.Ordinal))
                    {
                        if (content.Schema != null)
                            TraverseAndRegister(context, content.Schema.ActualSchema, visited);
                    }
                }

                // Response schemas
                if (op.ActualResponses != null)
                {
                    foreach (var (statusCode, response) in op.ActualResponses
                        .OrderBy(kv => kv.Key, StringComparer.Ordinal))
                    {
                        if (response.Content == null) continue;
                        foreach (var (mediaType, content) in response.Content
                            .OrderBy(kv => kv.Key, StringComparer.Ordinal))
                        {
                            if (content.Schema != null)
                                TraverseAndRegister(context, content.Schema.ActualSchema, visited);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Recursively traverses a schema's structural children (properties, item,
    /// additionalProperties, allOf, oneOf, anyOf) and registers each instance
    /// with a stable ID in the context. Uses reference-identity cycle protection.
    /// </summary>
    private static void TraverseAndRegister(
        NormalizationContext context, JsonSchema schema, HashSet<JsonSchema> visited)
    {
        if (schema == null) return;
        var actual = schema.ActualSchema;
        if (!visited.Add(actual)) return; // cycle protection via reference identity

        // Ensure this schema instance has an ID (catalog or synthetic)
        context.GetOrCreateSchemaId(actual);

        // Properties
        if (actual.Properties != null)
        {
            foreach (var (_, propSchema) in actual.Properties
                .OrderBy(kv => kv.Key, StringComparer.Ordinal))
            {
                TraverseAndRegister(context, propSchema.ActualSchema, visited);
            }
        }

        // Array items
        if (actual.Item != null)
            TraverseAndRegister(context, actual.Item.ActualSchema, visited);

        // Dictionary values
        if (actual.AdditionalPropertiesSchema != null)
            TraverseAndRegister(context, actual.AdditionalPropertiesSchema.ActualSchema, visited);

        // allOf members
        foreach (var sub in actual.AllOf)
            TraverseAndRegister(context, sub.ActualSchema, visited);

        // oneOf variants
        foreach (var variant in actual.OneOf)
            TraverseAndRegister(context, variant.ActualSchema, visited);

        // anyOf variants
        foreach (var variant in actual.AnyOf)
            TraverseAndRegister(context, variant.ActualSchema, visited);
    }
}

/// <summary>
/// A single deterministic normalization pass. Passes are executed in order
/// and may read/write to <see cref="NormalizationContext"/>.
/// </summary>
public interface INormalizationPass
{
    /// <summary>The stable name of this pass for diagnostics/testing.</summary>
    string Name { get; }

    /// <summary>Execute the pass, writing results to the context.</summary>
    void Execute(NormalizationContext context);
}

/// <summary>
/// Mutable context accumulated by normalization passes.
/// Frozen into an immutable <see cref="NormalizationResult"/> after all passes.
/// </summary>
public sealed class NormalizationContext
{
    public SchemaCatalog Catalog { get; }
    public OpenApiDocument Document { get; }

    /// <summary>
    /// Per-schema effective properties after allOf collection.
    /// Passes write here; the result is frozen into NormalizedSchema entries.
    /// </summary>
    public Dictionary<string, Dictionary<string, JsonSchema>> EffectiveProperties { get; } =
        new(StringComparer.Ordinal);

    /// <summary>Per-schema required property names after propagation.</summary>
    public Dictionary<string, HashSet<string>> RequiredProperties { get; } =
        new(StringComparer.Ordinal);

    /// <summary>Per-schema allOf composition members (ordered).</summary>
    public Dictionary<string, List<CompositionMember>> AllOfMembers { get; } =
        new(StringComparer.Ordinal);

    /// <summary>Per-schema oneOf variants (ordered).</summary>
    public Dictionary<string, List<CompositionMember>> OneOfVariants { get; } =
        new(StringComparer.Ordinal);

    /// <summary>Per-schema anyOf variants (ordered).</summary>
    public Dictionary<string, List<CompositionMember>> AnyOfVariants { get; } =
        new(StringComparer.Ordinal);

    /// <summary>Per-schema discriminator information.</summary>
    public Dictionary<string, DiscriminatorInfo> Discriminators { get; } =
        new(StringComparer.Ordinal);

    /// <summary>Per-schema inline property refs for dependency tracking.</summary>
    public Dictionary<string, List<InlinePropertyRef>> InlinePropertyRefs { get; } =
        new(StringComparer.Ordinal);

    /// <summary>
    /// Named component dependencies reachable through this schema's normalized structure.
    /// Includes properties, arrays, dictionaries, and all composition forms.
    /// </summary>
    public Dictionary<string, HashSet<string>> DependencySchemaIds { get; } =
        new(StringComparer.Ordinal);

    /// <summary>Tracks which passes have been executed for ordering verification.</summary>
    public List<string> ExecutedPasses { get; } = new();

    /// <summary>
    /// Schema instance -> stable ID map (reference equality).
    /// Contains both catalog-resolved and synthetic IDs.
    /// This is the authoritative instance-to-ID map that gets frozen into NormalizationResult.
    /// </summary>
    public Dictionary<JsonSchema, string> SchemaInstanceIds { get; } =
        new(ReferenceEqualityComparer.Instance);

    /// <summary>
    /// All registered schema IDs (component + inline). Passes iterate over this
    /// to process the complete schema universe.
    /// </summary>
    public Dictionary<string, JsonSchema> RegisteredSchemas { get; } =
        new(StringComparer.Ordinal);

    private int _inlineCounter;

    public NormalizationContext(SchemaCatalog catalog, OpenApiDocument document)
    {
        Catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
        Document = document ?? throw new ArgumentNullException(nameof(document));
    }

    /// <summary>
    /// Registers a schema instance with a known ID (used for component schemas).
    /// If already registered, no-op.
    /// </summary>
    public void RegisterSchema(JsonSchema schema, string schemaId)
    {
        SchemaInstanceIds.TryAdd(schema, schemaId);
        RegisteredSchemas.TryAdd(schemaId, schema);
    }

    /// <summary>
    /// Gets or creates a stable identity for a schema instance.
    /// If the schema has a catalog ID, returns that. Otherwise assigns a synthetic ID.
    /// Also registers the schema in RegisteredSchemas.
    /// </summary>
    public string GetOrCreateSchemaId(JsonSchema schema)
    {
        // Already registered by reference?
        if (SchemaInstanceIds.TryGetValue(schema, out var existingId))
            return existingId;

        // Prefer catalog identity
        if (Catalog.TryGetId(schema, out var catalogId))
        {
            SchemaInstanceIds[schema] = catalogId;
            RegisteredSchemas.TryAdd(catalogId, schema);
            return catalogId;
        }

        // Also check ActualSchema
        var actual = schema.ActualSchema;
        if (!ReferenceEquals(schema, actual))
        {
            if (SchemaInstanceIds.TryGetValue(actual, out existingId))
            {
                SchemaInstanceIds[schema] = existingId;
                return existingId;
            }
            if (Catalog.TryGetId(actual, out catalogId))
            {
                SchemaInstanceIds[schema] = catalogId;
                SchemaInstanceIds[actual] = catalogId;
                RegisteredSchemas.TryAdd(catalogId, actual);
                return catalogId;
            }
        }

        // Assign new synthetic ID (deterministic: counter increments in traversal order)
        var syntheticId = $"__inline_{_inlineCounter++}";
        SchemaInstanceIds[schema] = syntheticId;
        if (!ReferenceEquals(schema, actual))
            SchemaInstanceIds[actual] = syntheticId;
        RegisteredSchemas.TryAdd(syntheticId, schema);
        return syntheticId;
    }

    /// <summary>
    /// Attempts to get the registered ID for a schema instance without creating one.
    /// </summary>
    public bool TryGetSchemaId(JsonSchema schema, out string? id)
    {
        if (SchemaInstanceIds.TryGetValue(schema, out id!))
            return true;
        var actual = schema.ActualSchema;
        if (!ReferenceEquals(schema, actual) && SchemaInstanceIds.TryGetValue(actual, out id!))
            return true;
        if (Catalog.TryGetId(schema, out id!))
            return true;
        id = null;
        return false;
    }

    /// <summary>Freeze the mutable context into an immutable result.</summary>
    public NormalizationResult Freeze()
    {
        var schemas = new Dictionary<string, NormalizedSchema>(StringComparer.Ordinal);

        // Build NormalizedSchema for every registered schema that has normalization facts
        foreach (var schemaId in RegisteredSchemas.Keys)
        {
            var props = EffectiveProperties.TryGetValue(schemaId, out var p)
                ? (IReadOnlyDictionary<string, JsonSchema>)p
                : new Dictionary<string, JsonSchema>(StringComparer.Ordinal);

            var required = RequiredProperties.TryGetValue(schemaId, out var req)
                ? (IReadOnlySet<string>)req
                : new HashSet<string>(StringComparer.Ordinal);

            var allOf = AllOfMembers.TryGetValue(schemaId, out var aof)
                ? (IReadOnlyList<CompositionMember>)aof
                : Array.Empty<CompositionMember>();

            var oneOf = OneOfVariants.TryGetValue(schemaId, out var oof)
                ? (IReadOnlyList<CompositionMember>)oof
                : Array.Empty<CompositionMember>();

            var anyOf = AnyOfVariants.TryGetValue(schemaId, out var anof)
                ? (IReadOnlyList<CompositionMember>)anof
                : Array.Empty<CompositionMember>();

            Discriminators.TryGetValue(schemaId, out var discriminator);

            var inlineRefs = InlinePropertyRefs.TryGetValue(schemaId, out var refs)
                ? (IReadOnlyList<InlinePropertyRef>)refs
                : Array.Empty<InlinePropertyRef>();

            var dependencies = DependencySchemaIds.TryGetValue(schemaId, out var deps)
                ? (IReadOnlySet<string>)deps
                : new HashSet<string>(StringComparer.Ordinal);

            schemas[schemaId] = new NormalizedSchema(
                schemaId, props, required, allOf, oneOf, anyOf, discriminator, inlineRefs, dependencies);
        }

        // Build the frozen instance->ID map
        var frozenInstanceMap = new Dictionary<JsonSchema, string>(
            SchemaInstanceIds, ReferenceEqualityComparer.Instance);

        return new NormalizationResult(schemas, ExecutedPasses.ToList(), Catalog, frozenInstanceMap);
    }
}

/// <summary>
/// Immutable result of specification normalization. One instance is shared
/// across all downstream consumers for a given document + catalog.
/// </summary>
public sealed class NormalizationResult
{
    private readonly IReadOnlyDictionary<string, NormalizedSchema> _schemas;
    private readonly IReadOnlyDictionary<JsonSchema, string> _instanceToId;

    /// <summary>Names of passes that were executed, in order.</summary>
    public IReadOnlyList<string> PassesExecuted { get; }

    /// <summary>The shared catalog this normalization was built from.</summary>
    public SchemaCatalog Catalog { get; }

    public NormalizationResult(
        IReadOnlyDictionary<string, NormalizedSchema> schemas,
        IReadOnlyList<string> passesExecuted,
        SchemaCatalog catalog,
        IReadOnlyDictionary<JsonSchema, string> instanceToId)
    {
        _schemas = schemas ?? throw new ArgumentNullException(nameof(schemas));
        PassesExecuted = passesExecuted ?? throw new ArgumentNullException(nameof(passesExecuted));
        Catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
        _instanceToId = instanceToId ?? throw new ArgumentNullException(nameof(instanceToId));
    }

    /// <summary>Get normalized schema facts for a given schema ID.</summary>
    public bool TryGet(string schemaId, out NormalizedSchema normalized)
    {
        ArgumentNullException.ThrowIfNull(schemaId);
        return _schemas.TryGetValue(schemaId, out normalized!);
    }

    /// <summary>
    /// Get the normalized schema ID for a schema instance (reference equality lookup).
    /// Works for BOTH component schemas and inline schemas that were discovered during
    /// normalization traversal.
    /// </summary>
    public bool TryGetIdForSchema(JsonSchema schema, out string? id)
    {
        if (_instanceToId.TryGetValue(schema, out id!))
            return true;
        var actual = schema.ActualSchema;
        if (!ReferenceEquals(schema, actual) && _instanceToId.TryGetValue(actual, out id!))
            return true;
        // Final fallback: catalog
        if (Catalog.TryGetId(schema, out id!))
            return true;
        id = null;
        return false;
    }

    /// <summary>
    /// Get normalized schema facts for a schema by instance identity.
    /// Resolves the schema's ID via the frozen instance map, then looks up NormalizedSchema.
    /// Works for component schemas, inline schemas, and ActualSchema indirections.
    /// </summary>
    public bool TryGetForSchema(JsonSchema schema, out NormalizedSchema normalized)
    {
        if (TryGetIdForSchema(schema, out var id) && id != null && _schemas.TryGetValue(id, out normalized!))
            return true;
        normalized = null!;
        return false;
    }

    /// <summary>
    /// Get normalized schema facts for a schema. Throws if not found.
    /// Use in production code paths where normalization coverage is guaranteed.
    /// </summary>
    public NormalizedSchema GetForSchema(JsonSchema schema)
    {
        if (TryGetForSchema(schema, out var normalized))
            return normalized;
        throw new InvalidOperationException(
            $"Schema instance was not discovered during normalization. " +
            $"This indicates a gap in schema discovery traversal. " +
            $"Schema type={schema.Type}, HasProperties={schema.Properties?.Count > 0}, " +
            $"HasAllOf={schema.AllOf.Count > 0}");
    }

    /// <summary>All normalized schema IDs.</summary>
    public IEnumerable<string> SchemaIds => _schemas.Keys;

    /// <summary>Number of schemas normalized.</summary>
    public int Count => _schemas.Count;
}

/// <summary>
/// Pass 1: Collects effective properties from allOf compositions with true recursive
/// nested inline allOf support. Iterates over ALL registered schemas (component + inline).
///
/// For each schema that has allOf members:
/// - $ref members are recorded as base schemas (for dependency tracking) — their properties
///   are NOT inherited (they are base-class dependencies, not mixin properties)
/// - Inline object members have their properties merged recursively (nested allOf within
///   inline members is fully expanded)
/// - Direct properties of the schema are included (override allOf-inherited ones)
///
/// Does NOT mutate the document. Produces immutable property maps keyed by schema ID.
/// Handles recursive schemas safely via cycle detection with deterministic override precedence:
/// earlier allOf members are overridden by later ones; direct properties override all.
/// </summary>
public sealed class AllOfPropertyCollectionPass : INormalizationPass
{
    public string Name => "AllOfPropertyCollection";

    public void Execute(NormalizationContext context)
    {
        context.ExecutedPasses.Add(Name);

        // Process ALL registered schemas (component + inline), not just components
        foreach (var (schemaId, schema) in context.RegisteredSchemas
            .OrderBy(kv => kv.Key, StringComparer.Ordinal))
        {
            CollectForSchema(context, schemaId, schema.ActualSchema);
        }
    }

    private static void CollectForSchema(NormalizationContext context, string schemaId, JsonSchema actual)
    {
        // Avoid reprocessing. Per-chain cycle protection lives in the local `visited`
        // HashSet<JsonSchema> passed to CollectInlineAllOfProperties.
        if (context.EffectiveProperties.ContainsKey(schemaId))
            return;

        var effectiveProps = new Dictionary<string, JsonSchema>(StringComparer.Ordinal);
        var inlineRefs = new List<InlinePropertyRef>();

        // Gather properties from allOf inline members (recursive for nested inline allOf)
        foreach (var sub in actual.AllOf)
        {
            if (sub.HasReference)
                continue; // Skip $ref entries — they are base classes, not property mixins

            CollectInlineAllOfProperties(sub.ActualSchema, effectiveProps, inlineRefs, context,
                new HashSet<JsonSchema>(ReferenceEqualityComparer.Instance));
        }

        // Direct properties override allOf-inherited ones
        if (actual.Properties != null)
        {
            foreach (var kv in actual.Properties)
            {
                effectiveProps[kv.Key] = kv.Value; // Direct props override

                var propSchemaId = kv.Value.Reference?.Id;
                if (propSchemaId == null)
                    context.Catalog.TryGetId(kv.Value, out propSchemaId);
                if (propSchemaId != null && !inlineRefs.Any(r => r.PropertyName == kv.Key))
                    inlineRefs.Add(new InlinePropertyRef(kv.Key, propSchemaId));
            }
        }

        context.EffectiveProperties[schemaId] = effectiveProps;
        context.InlinePropertyRefs[schemaId] = inlineRefs;
    }

    /// <summary>
    /// Recursively collects properties from an inline allOf member, including nested
    /// inline allOf within it. Uses instance-identity cycle guard for inline schemas.
    /// </summary>
    private static void CollectInlineAllOfProperties(
        JsonSchema inlineSchema,
        Dictionary<string, JsonSchema> target,
        List<InlinePropertyRef> inlineRefs,
        NormalizationContext context,
        HashSet<JsonSchema> visited)
    {
        if (!visited.Add(inlineSchema)) return; // cycle guard for inline schemas

        // If this inline schema itself has nested allOf, recurse into its inline members
        foreach (var nestedSub in inlineSchema.AllOf)
        {
            if (nestedSub.HasReference) continue; // nested $ref = base dependency, skip
            CollectInlineAllOfProperties(nestedSub.ActualSchema, target, inlineRefs, context, visited);
        }

        // Collect this inline schema's own properties
        if (inlineSchema.Properties != null)
        {
            foreach (var kv in inlineSchema.Properties)
            {
                target.TryAdd(kv.Key, kv.Value); // First writer wins within allOf (later direct props override)

                var propSchemaId = kv.Value.Reference?.Id;
                if (propSchemaId == null)
                    context.Catalog.TryGetId(kv.Value, out propSchemaId);
                if (propSchemaId != null && !inlineRefs.Any(r => r.PropertyName == kv.Key))
                    inlineRefs.Add(new InlinePropertyRef(kv.Key, propSchemaId));
            }
        }
    }
}

/// <summary>
/// Pass 2: Preserves composition semantics (allOf members, oneOf/anyOf variants,
/// discriminator info) in the normalization IR without erasing structural information.
///
/// Iterates over ALL registered schemas (component + inline).
/// </summary>
public sealed class CompositionPreservationPass : INormalizationPass
{
    public string Name => "CompositionPreservation";

    public void Execute(NormalizationContext context)
    {
        context.ExecutedPasses.Add(Name);

        // Process ALL registered schemas (component + inline)
        foreach (var (schemaId, schema) in context.RegisteredSchemas
            .OrderBy(kv => kv.Key, StringComparer.Ordinal))
        {
            var actual = schema.ActualSchema;
            PreserveComposition(context, schemaId, actual);
        }
    }

    private static void PreserveComposition(NormalizationContext context, string schemaId, JsonSchema actual)
    {
        // allOf members (ordered)
        if (actual.AllOf.Count > 0)
        {
            var members = new List<CompositionMember>();
            foreach (var sub in actual.AllOf)
            {
                string? refId = null;
                bool isRef = sub.HasReference || !string.IsNullOrEmpty(sub.Reference?.Id);
                if (isRef)
                {
                    refId = sub.Reference?.Id;
                    if (refId == null)
                        context.Catalog.TryGetId(sub, out refId);
                }
                members.Add(new CompositionMember(isRef, refId, sub));
            }
            context.AllOfMembers[schemaId] = members;
        }

        // oneOf variants (ordered)
        if (actual.OneOf.Count > 0)
        {
            var variants = new List<CompositionMember>();
            foreach (var variant in actual.OneOf)
            {
                string? refId = null;
                bool isRef = variant.HasReference || !string.IsNullOrEmpty(variant.Reference?.Id);
                if (isRef)
                {
                    refId = variant.Reference?.Id;
                    if (refId == null)
                        context.Catalog.TryGetId(variant, out refId);
                }
                else
                {
                    context.Catalog.TryGetId(variant, out refId);
                }
                variants.Add(new CompositionMember(isRef, refId, variant));
            }
            context.OneOfVariants[schemaId] = variants;
        }

        // anyOf variants (ordered)
        if (actual.AnyOf.Count > 0)
        {
            var variants = new List<CompositionMember>();
            foreach (var variant in actual.AnyOf)
            {
                string? refId = null;
                bool isRef = variant.HasReference || !string.IsNullOrEmpty(variant.Reference?.Id);
                if (isRef)
                {
                    refId = variant.Reference?.Id;
                    if (refId == null)
                        context.Catalog.TryGetId(variant, out refId);
                }
                else
                {
                    context.Catalog.TryGetId(variant, out refId);
                }
                variants.Add(new CompositionMember(isRef, refId, variant));
            }
            context.AnyOfVariants[schemaId] = variants;
        }

        // Discriminator
        var discPropName = actual.DiscriminatorObject?.PropertyName
            ?? (!string.IsNullOrEmpty(actual.Discriminator) ? actual.Discriminator : null);
        if (!string.IsNullOrEmpty(discPropName))
        {
            var mapping = new Dictionary<string, string>(StringComparer.Ordinal);
            if (actual.DiscriminatorObject?.Mapping != null)
            {
                foreach (var kv in actual.DiscriminatorObject.Mapping)
                {
                    var mappedId = kv.Value?.Reference?.Id;
                    if (mappedId == null)
                        context.Catalog.TryGetId(kv.Value!, out mappedId);
                    if (mappedId != null)
                        mapping[kv.Key] = mappedId;
                }
            }
            context.Discriminators[schemaId] = new DiscriminatorInfo(discPropName, mapping);
        }
    }
}

/// <summary>
/// Pass 3: Collects named component dependencies from normalized schema structure.
/// This is the sole composition-aware dependency interpretation layer; downstream
/// reference-graph construction consumes only the resulting schema IDs.
/// </summary>
public sealed class DependencyCollectionPass : INormalizationPass
{
    public string Name => "DependencyCollection";

    public void Execute(NormalizationContext context)
    {
        context.ExecutedPasses.Add(Name);

        foreach (var (schemaId, schema) in context.RegisteredSchemas
            .OrderBy(kv => kv.Key, StringComparer.Ordinal))
        {
            var dependencies = new HashSet<string>(StringComparer.Ordinal);
            var visited = new HashSet<JsonSchema>(ReferenceEqualityComparer.Instance);

            if (context.EffectiveProperties.TryGetValue(schemaId, out var properties))
            {
                foreach (var property in properties.Values)
                    CollectNamedDependencies(context, schemaId, property, dependencies, visited);
            }

            if (context.AllOfMembers.TryGetValue(schemaId, out var allOfMembers))
                CollectCompositionMembers(context, schemaId, allOfMembers, dependencies, visited);
            if (context.OneOfVariants.TryGetValue(schemaId, out var oneOfVariants))
                CollectCompositionMembers(context, schemaId, oneOfVariants, dependencies, visited);
            if (context.AnyOfVariants.TryGetValue(schemaId, out var anyOfVariants))
                CollectCompositionMembers(context, schemaId, anyOfVariants, dependencies, visited);

            var actual = schema.ActualSchema;
            if (actual.Item != null)
                CollectNamedDependencies(context, schemaId, actual.Item, dependencies, visited);
            if (actual.AdditionalPropertiesSchema != null)
                CollectNamedDependencies(context, schemaId, actual.AdditionalPropertiesSchema, dependencies, visited);

            context.DependencySchemaIds[schemaId] = dependencies;
        }
    }

    private static void CollectCompositionMembers(
        NormalizationContext context,
        string sourceSchemaId,
        IEnumerable<CompositionMember> members,
        HashSet<string> dependencies,
        HashSet<JsonSchema> visited)
    {
        foreach (var member in members)
        {
            if (member.ReferencedSchemaId != null)
            {
                if (!string.Equals(member.ReferencedSchemaId, sourceSchemaId, StringComparison.Ordinal))
                    dependencies.Add(member.ReferencedSchemaId);
                continue;
            }

            CollectNamedDependencies(context, sourceSchemaId, member.RawSchema, dependencies, visited);
        }
    }

    private static void CollectNamedDependencies(
        NormalizationContext context,
        string sourceSchemaId,
        JsonSchema schema,
        HashSet<string> dependencies,
        HashSet<JsonSchema> visited)
    {
        var actual = schema.ActualSchema;
        if (!visited.Add(actual))
            return;

        if (context.Catalog.TryGetId(schema, out var componentId)
            && !string.Equals(componentId, sourceSchemaId, StringComparison.Ordinal))
        {
            dependencies.Add(componentId);
            return;
        }

        foreach (var property in actual.Properties?.Values ?? Array.Empty<JsonSchemaProperty>())
            CollectNamedDependencies(context, sourceSchemaId, property, dependencies, visited);
        if (actual.Item != null)
            CollectNamedDependencies(context, sourceSchemaId, actual.Item, dependencies, visited);
        if (actual.AdditionalPropertiesSchema != null)
            CollectNamedDependencies(context, sourceSchemaId, actual.AdditionalPropertiesSchema, dependencies, visited);
        foreach (var member in actual.AllOf)
            CollectNamedDependencies(context, sourceSchemaId, member, dependencies, visited);
        foreach (var variant in actual.OneOf)
            CollectNamedDependencies(context, sourceSchemaId, variant, dependencies, visited);
        foreach (var variant in actual.AnyOf)
            CollectNamedDependencies(context, sourceSchemaId, variant, dependencies, visited);
    }
}

/// <summary>
/// Pass 4: Propagates required field information.
/// Merges required fields from the schema's own RequiredProperties with any
/// additional required constraints from inline allOf members (recursively).
///
/// Iterates over ALL registered schemas (component + inline).
/// Runs after AllOfPropertyCollectionPass because it needs effective property maps.
/// </summary>
public sealed class RequiredFieldPropagationPass : INormalizationPass
{
    public string Name => "RequiredFieldPropagation";

    public void Execute(NormalizationContext context)
    {
        context.ExecutedPasses.Add(Name);

        // Process ALL registered schemas (component + inline)
        foreach (var (schemaId, schema) in context.RegisteredSchemas
            .OrderBy(kv => kv.Key, StringComparer.Ordinal))
        {
            var actual = schema.ActualSchema;
            var required = new HashSet<string>(StringComparer.Ordinal);

            // Own required fields
            if (actual.RequiredProperties != null)
            {
                foreach (var prop in actual.RequiredProperties)
                    required.Add(prop);
            }

            // Required fields from inline allOf members (recursive)
            CollectInlineRequired(actual, required, new HashSet<JsonSchema>(ReferenceEqualityComparer.Instance));

            context.RequiredProperties[schemaId] = required;
        }
    }

    private static void CollectInlineRequired(
        JsonSchema schema, HashSet<string> required, HashSet<JsonSchema> visited)
    {
        if (!visited.Add(schema)) return;

        foreach (var sub in schema.AllOf)
        {
            if (sub.HasReference) continue;
            var subActual = sub.ActualSchema;
            if (subActual.RequiredProperties != null)
            {
                foreach (var prop in subActual.RequiredProperties)
                    required.Add(prop);
            }
            // Recurse into nested inline allOf
            CollectInlineRequired(subActual, required, visited);
        }
    }
}
