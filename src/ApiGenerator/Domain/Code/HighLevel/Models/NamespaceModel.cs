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
using ApiGenerator.Configuration.Overrides;
using ApiGenerator.Generator;
using NSwag;

namespace ApiGenerator.Domain.Code.HighLevel.Models;

public sealed class NamespaceModel
{
    public string Namespace { get; init; } = "";
    public IReadOnlyList<ModelType> TypesToEmit { get; init; } = new List<ModelType>();
    public IReadOnlyList<ModelType> AllTypes { get; init; } = new List<ModelType>();

    /// <summary>
    /// The reference graph used for reachability analysis. Exposed for diagnostics and testing.
    /// </summary>
    public ReferenceGraph Graph { get; init; } = null!;

    /// <summary>
    /// Builds the namespace model using reference graph-based reachability filtering.
    /// This is the single production implementation that:
    /// 1. Builds all model types from namespace schemas
    /// 2. Constructs a reference graph with operation roots
    /// 3. Models union variant body ownership explicitly in the graph
    /// 4. Models operation-owned response schemas via output-role semantics
    /// 5. Filters emission to reachable models only, excluding owned nodes
    ///
    /// The convenience overload creates an isolated normalization result for tests.
    /// Production uses the core overload below with one document-scoped result shared
    /// across all plugins.
    /// </summary>
    public static NamespaceModel Build(
        OpenApiDocument doc, string @namespace, IModelOverrides plugin, ModelTypeResolver resolver,
        HashSet<string>? explicitlyOpenSchemaIds = null) =>
        Build(
            doc,
            @namespace,
            plugin,
            resolver,
            new SchemaNormalizer(resolver.Schemas).Normalize(doc),
            explicitlyOpenSchemaIds);

    /// <summary>
    /// Core build path. Production supplies the single document-scoped normalization result.
    /// </summary>
    public static NamespaceModel Build(
        OpenApiDocument doc, string @namespace, IModelOverrides plugin, ModelTypeResolver resolver,
        NormalizationResult normalization,
        HashSet<string>? explicitlyOpenSchemaIds = null)
    {
        ArgumentNullException.ThrowIfNull(normalization);

        var classifier = new UnionClassifier(resolver.Schemas);
        var allModels = new Dictionary<string, ModelType>(StringComparer.Ordinal);

        var unionOwnerships = new List<(string Owner, string Owned)>();

        // First pass: build all model types from namespace schemas (declaration-order independent)
        foreach (var (id, schema) in doc.Components.Schemas
                     .Where(kv => kv.Key.StartsWith(@namespace + ".", StringComparison.Ordinal))
                     .OrderBy(kv => kv.Key, StringComparer.Ordinal))
        {
            var s = schema.ActualSchema;

            // String enums become C# [StringEnum] enums.
            if (s.IsEnum())
            {
                var members = s.GetEnumValues()
                    .Select(v => new EnumMember(v.Value, ToPascal(v.Alias ?? v.Value)))
                    .ToList();
                if (members.Count == 0) continue;
                allModels.TryAdd(id, new EnumModel(id, resolver.CsharpTypeName(id), members));
                continue;
            }

            // Wrapper-key oneOf: detect and build union model
            if (TryBuildWrapperKeyUnion(id, s, plugin, resolver, classifier, unionOwnerships, out var unionModel, normalization))
            {
                allModels.TryAdd(id, unionModel!);
                continue;
            }

            // Collect properties: always from normalized schema facts (Phase 6).
            var propSource = normalization.TryGet(id, out var normalized)
                ? normalized.EffectiveProperties
                : (IReadOnlyDictionary<string, NJsonSchema.JsonSchema>)new Dictionary<string, NJsonSchema.JsonSchema>(StringComparer.Ordinal);
            var isOpen = explicitlyOpenSchemaIds?.Contains(id) ?? false;
            // Skip schemas with no properties unless they're explicitly open (pure dictionary wrappers).
            if (propSource.Count == 0 && !isOpen) continue;

            var requiredNames = normalization.TryGet(id, out var normalizedReq)
                ? normalizedReq.RequiredProperties
                : (IReadOnlySet<string>)new HashSet<string>(StringComparer.Ordinal);

            var props = propSource
                .OrderBy(p => p.Key, StringComparer.Ordinal)
                .Select(p =>
                {
                    var typeRef = resolver.ResolveTypeRef(p.Value);
                    var csharpType = typeRef.ToCsharp();

                    // Global schema-scoped property type override
                    var overrideType = plugin.ResolvePropertyTypeOverride("", p.Key, id);
                    if (overrideType != null) csharpType = overrideType;

                    return new ModelProperty(
                        WireName: p.Key,
                        CsharpName: ToPascal(p.Key),
                        CsharpType: csharpType,
                        Type: typeRef,
                        IsRequired: requiredNames.Contains(p.Key),
                        Description: p.Value.ActualSchema.Description,
                        VersionAdded: null);
                })
                .ToList();
            allModels.TryAdd(id, new ObjectModel(id, resolver.CsharpTypeName(id), props, isOpen));
        }

        // Build reference graph for reachability analysis
        var graphBuilder = new ReferenceGraphBuilder(doc, plugin, resolver.Schemas, normalization);
        var graph = graphBuilder.Build();

        foreach (var (owner, owned) in unionOwnerships)
            graph.MarkAsOwned(owner, owned);

        // Wrapper-key unions are part of the current public plugin output. Their mapped-type
        // entries project references to generated interfaces, rather than declaring external types.
        // Suppressed unions are excluded — they exist for structural classification only.
        foreach (var union in allModels.Values.OfType<WrapperKeyUnionModel>())
        {
            if (!plugin.SuppressedUnionSchemaIds.Contains(union.SchemaId))
                graph.MarkAsExplicitRoot(union.SchemaId);
        }

        foreach (var explicitRoot in plugin.ExplicitlyPublicSchemaIds
            .Concat(explicitlyOpenSchemaIds ?? Enumerable.Empty<string>())
            .Distinct(StringComparer.Ordinal))
            graph.MarkAsExplicitRoot(explicitRoot);

        var emittable = new HashSet<string>(graph.ComputeEmittable(), StringComparer.Ordinal);
        var emit = allModels.Values
            .Where(type => emittable.Contains(type.SchemaId))
            .Where(type => type is not WrapperKeyUnionModel || !plugin.SuppressedUnionSchemaIds.Contains(type.SchemaId))
            .OrderBy(type => type.SchemaId, StringComparer.Ordinal)
            .ToList();

        return new NamespaceModel
        {
            Namespace = @namespace,
            TypesToEmit = emit,
            AllTypes = allModels.Values.OrderBy(t => t.SchemaId, StringComparer.Ordinal).ToList(),
            Graph = graph
        };
    }

    /// <summary>
    /// Tries to interpret a schema as a wrapper-key discriminated union using the <see cref="UnionClassifier"/>.
    /// Returns true and populates <paramref name="model"/> if the schema is a WrapperKeyOneOf or FlatWrapperKey union.
    /// Both encodings produce the same <see cref="WrapperKeyUnionModel"/> output shape (base interface +
    /// per-variant types + AutomataDictionary formatter + list descriptor).
    /// </summary>
    private static bool TryBuildWrapperKeyUnion(
        string schemaId,
        NJsonSchema.JsonSchema s,
        IModelOverrides registry,
        ModelTypeResolver resolver,
        UnionClassifier classifier,
        List<(string Owner, string Owned)> ownerships,
        out WrapperKeyUnionModel? model,
        NormalizationResult normalization)
    {
        model = null;

        // Use the classifier to detect union patterns
        var unionModel = classifier.TryClassify(schemaId, s);
        if (unionModel == null ||
            (unionModel.Encoding != UnionEncoding.WrapperKeyOneOf && unionModel.Encoding != UnionEncoding.FlatWrapperKey))
            return false;

        // Build WrapperKeyVariants from the classified UnionModel
        var variants = new List<WrapperKeyVariant>();

        foreach (var variant in unionModel.Variants)
        {
            var bodyRefId = variant.BodySchemaId;
            var bodySchema = variant.BodySchema;

            if (bodyRefId != null)
                ownerships.Add((schemaId, bodyRefId));

            // The variant C# name: PascalCase of the body schema ID or the key.
            var variantCsharpName = bodyRefId != null
                ? resolver.CsharpTypeName(bodyRefId)
                : ToPascal(variant.Key);

            // Skip if mapped to an existing type.
            if (bodyRefId != null && registry.MappedCsharpType(bodyRefId) != null)
            {
                // For WrapperKeyOneOf unions, a mapped variant means the entire union
                // interface is hand-written — reject the union entirely.
                // For FlatWrapperKey unions, mapped variants are simply excluded from
                // generation (e.g., script processor maps to existing IScript type).
                if (unionModel.Encoding == UnionEncoding.WrapperKeyOneOf)
                    return false;

                // FlatWrapperKey: check if policy marks this variant as Retained.
                // Retained variants with mapped body schemas still need formatter/descriptor dispatch.
                var retainedByPolicy = false;
                if (registry.UnionPolicies.TryGetValue(schemaId, out var policyForSkipCheck)
                    && policyForSkipCheck.VariantOverrides.TryGetValue(variant.Key, out var vpForSkipCheck))
                {
                    retainedByPolicy = vpForSkipCheck.Retained;
                }

                if (!retainedByPolicy)
                {
                    // Remove ownership and skip.
                    ownerships.RemoveAll(o => o.Owner == schemaId && o.Owned == bodyRefId);
                    continue;
                }
                // Retained mapped variant: add it with empty body properties
                // (its types are hand-written, we just need it for formatter/descriptor dispatch).
                ownerships.RemoveAll(o => o.Owner == schemaId && o.Owned == bodyRefId);
                variants.Add(new WrapperKeyVariant(variant.Key, variantCsharpName, variant.VersionAdded, Array.Empty<ModelProperty>()));
                continue;
            }

            registry.UnionPolicies.TryGetValue(schemaId, out var variantPolicy);
            var bodyProps = bodySchema != null
                ? BuildVariantProperties(bodySchema, resolver, normalization,
                    variantPolicy?.ExcludedBaseProperties)
                : Array.Empty<ModelProperty>();

            variants.Add(new WrapperKeyVariant(variant.Key, variantCsharpName, variant.VersionAdded, bodyProps));
        }

        if (variants.Count == 0) return false;

        // Build base properties from the shared properties
        var basePropsList = unionModel.SharedProperties
            .Select(p =>
            {
                var typeRef = resolver.ResolveTypeRef(p.Schema);
                return new ModelProperty(
                    WireName: p.WireName,
                    CsharpName: ToPascal(p.WireName),
                    CsharpType: typeRef.ToCsharp(),
                    Type: typeRef,
                    IsRequired: false,
                    Description: null,
                    VersionAdded: null);
            })
            .ToList();

        // Apply rendering policy if one is configured for this union schema
        Configuration.Overrides.UnionRenderingPolicy? policy = null;
        registry.UnionPolicies.TryGetValue(schemaId, out policy);

        // Apply variant-level policy overrides
        if (policy != null)
        {
            variants = variants.Select(v =>
            {
                var isGeneric = policy.GenericDescriptors;
                var baseClass = policy.VariantBaseClass;
                var descBase = policy.DescriptorBasePattern;
                IReadOnlySet<string> fieldProps = policy.FieldProperties.Count > 0
                    ? new HashSet<string>(policy.FieldProperties, StringComparer.Ordinal)
                    : new HashSet<string>(StringComparer.Ordinal);
                string? fluentOverride = null;
                string? ifaceOverride = null;
                string? additionalIface = null;
                bool retained = false;

                policy.VariantNameOverrides.TryGetValue(v.Key, out var nameOverride);
                policy.FluentMethodNameOverrides.TryGetValue(v.Key, out fluentOverride);
                policy.InterfaceNameOverrides.TryGetValue(v.Key, out ifaceOverride);

                if (policy.VariantOverrides.TryGetValue(v.Key, out var vp))
                {
                    if (vp.BaseClass != null) baseClass = vp.BaseClass;
                    if (vp.DescriptorBasePattern != null) descBase = vp.DescriptorBasePattern;
                    retained = vp.Retained;
                    additionalIface = vp.AdditionalInterface;
                    if (vp.NonGenericDescriptor) isGeneric = false;
                    // Exclude properties that come from the behavioral base
                    if (vp.ExcludedProperties.Count > 0)
                    {
                        var filtered = v.BodyProperties
                            .Where(p => !vp.ExcludedProperties.Contains(p.WireName))
                            .ToList();
                        v = v with { BodyProperties = filtered };
                    }
                }

                return v with
                {
                    CsharpName = nameOverride ?? v.CsharpName,
                    FluentMethodNameOverride = fluentOverride,
                    InterfaceNameOverride = ifaceOverride,
                    BaseClass = baseClass,
                    DescriptorBasePattern = descBase,
                    IsGenericDescriptor = isGeneric,
                    IsRetained = retained,
                    AdditionalInterface = additionalIface,
                    FieldProperties = fieldProps,
                    PropertyAliases = vp?.PropertyAliases ?? new Dictionary<string, string>(StringComparer.Ordinal),
                    ParamsOverloads = vp?.ParamsOverloads ?? new HashSet<string>(StringComparer.Ordinal),
                    ScalarStringOverloads = vp?.ScalarStringOverloads ?? new HashSet<string>(StringComparer.Ordinal),
                    ProcessorLambdaOverloads = vp?.ProcessorLambdaOverloads ?? new HashSet<string>(StringComparer.Ordinal),
                    DictionaryLambdaOverloads = vp?.DictionaryLambdaOverloads ?? new HashSet<string>(StringComparer.Ordinal),
                    PropertyTypeOverrides = vp?.PropertyTypeOverrides ?? new Dictionary<string, string>(StringComparer.Ordinal),
                    FieldsSelectorOverloads = vp?.FieldsSelectorOverloads ?? new HashSet<string>(StringComparer.Ordinal),
                };
            }).ToList();

            foreach (var retained in policy.AdditionalRetainedVariants)
            {
                if (variants.Any(v => string.Equals(v.Key, retained.Key, StringComparison.Ordinal)))
                    continue;

                variants.Add(new WrapperKeyVariant(
                    retained.Key,
                    retained.CsharpName,
                    VersionAdded: null,
                    BodyProperties: Array.Empty<ModelProperty>())
                {
                    InterfaceNameOverride = retained.InterfaceName,
                    FluentMethodNameOverride = retained.FluentMethodName,
                    IsGenericDescriptor = retained.GenericDescriptor,
                    IsRetained = true,
                });
            }
        }

        model = new WrapperKeyUnionModel(
            schemaId,
            resolver.CsharpTypeName(schemaId),
            variants,
            basePropsList)
        {
            RenderingPolicy = policy
        };
        return true;
    }

    private static IReadOnlyList<ModelProperty> BuildVariantProperties(
        NJsonSchema.JsonSchema body, ModelTypeResolver resolver,
        NormalizationResult normalization,
        ISet<string>? excludedBaseProperties = null)
    {
        // Properties owned by the union base class are excluded from individual variant bodies.
        // The exclusion set is supplied by the plugin via UnionRenderingPolicy.ExcludedBaseProperties.
        var baseProps = excludedBaseProperties ?? new HashSet<string>(StringComparer.Ordinal);

        IReadOnlyDictionary<string, NJsonSchema.JsonSchema> propSource;
        IReadOnlySet<string> required;

        // Phase 6: Use normalization instance-map lookup. ALL schemas (including inline
        // variant bodies) were discovered and normalized during SchemaNormalizer.Normalize().
        // No raw AllOf fallback allowed.
        if (normalization.TryGetForSchema(body, out var normalizedBody))
        {
            propSource = normalizedBody.EffectiveProperties;
            required = normalizedBody.RequiredProperties;
        }
        else
        {
            // Schema was not discovered during normalization. This indicates a gap in
            // the discovery traversal. Fall back to direct properties ONLY (no AllOf
            // iteration). In practice this should not happen if discovery is complete.
            Console.Error.WriteLine(
                $"[NamespaceModel] BuildVariantProperties: schema instance not found in normalization result. " +
                $"Falling back to direct properties only. This indicates an incomplete schema discovery.");
            var directProps = new Dictionary<string, NJsonSchema.JsonSchema>(StringComparer.Ordinal);
            if (body.Properties != null)
                foreach (var kv in body.Properties)
                    directProps[kv.Key] = kv.Value;
            propSource = directProps;
            required = new HashSet<string>(
                body.RequiredProperties ?? Enumerable.Empty<string>(),
                StringComparer.Ordinal);
        }

        return propSource
            .Where(p => !baseProps.Contains(p.Key))
            .OrderBy(p => p.Key, StringComparer.Ordinal)
            .Select(p =>
            {
                var typeRef = resolver.ResolveTypeRef(p.Value);
                return new ModelProperty(
                    WireName: p.Key,
                    CsharpName: ToPascal(p.Key),
                    CsharpType: typeRef.ToCsharp(),
                    Type: typeRef,
                    IsRequired: required.Contains(p.Key),
                    Description: p.Value.ActualSchema.Description,
                    VersionAdded: null);
            })
            .ToList();
    }

    private static string ToPascal(string name) => NamingConventions.ToPascal(name);
}
