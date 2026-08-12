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
using NJsonSchema.References;
using NSwag;

namespace ApiGenerator.Domain.Code.HighLevel.Models;

public sealed class NamespaceModel
{
    public string Namespace { get; init; } = "";
    public IReadOnlyList<ModelType> TypesToEmit { get; init; } = new List<ModelType>();
    public IReadOnlyList<ModelType> AllTypes { get; init; } = new List<ModelType>();

    public static NamespaceModel Build(
        OpenApiDocument doc, string @namespace, IModelOverrides registry, ModelTypeResolver resolver,
        HashSet<string>? explicitlyOpenSchemaIds = null)
    {
        var all = new List<ModelType>();
        // Track body schema IDs consumed by wrapper-key unions to avoid duplicate ObjectModel emit.
        var unionBodySchemaIds = new HashSet<string>(StringComparer.Ordinal);

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
                // Use resolver.CsharpTypeName so that registry renames (e.g. Task→MlTask) apply.
                all.Add(new EnumModel(id, resolver.CsharpTypeName(id), members));
                continue;
            }

            // Wrapper-key oneOf: each variant has exactly one required property whose value is
            // the body schema $ref.  Wire format: { "discriminator_key": { ...body... } }.
            // Detected by: s.OneOf.Count > 0 AND every variant has exactly one required property.
            if (TryBuildWrapperKeyUnion(id, s, doc, registry, resolver, unionBodySchemaIds, out var unionModel))
            {
                all.Add(unionModel!);
                continue;
            }

            // Skip schemas that were already consumed as variant bodies of a wrapper-key union.
            if (unionBodySchemaIds.Contains(id)) continue;

            // Collect properties: from direct properties, and from inline allOf object schemas.
            var propSource = CollectProperties(s);
            var isOpen = explicitlyOpenSchemaIds?.Contains(id) ?? false;
            // Skip schemas with no properties unless they're explicitly open (pure dictionary wrappers).
            if (propSource.Count == 0 && !isOpen) continue; // arrays/bare-refs — handled later

            var requiredNames = new HashSet<string>(
                s.RequiredProperties ?? Enumerable.Empty<string>(),
                StringComparer.Ordinal);

            var props = propSource
                .OrderBy(p => p.Key, StringComparer.Ordinal)
                .Select(p =>
                {
                    var typeRef = resolver.ResolveTypeRef(p.Value);
                    return new ModelProperty(
                        WireName: p.Key,
                        CsharpName: ToPascal(p.Key),
                        CsharpType: typeRef.ToCsharp(),
                        Type: typeRef,
                        IsRequired: requiredNames.Contains(p.Key),
                        Description: p.Value.ActualSchema.Description,
                        VersionAdded: null);
                })
                .ToList();
            // Use resolver.CsharpTypeName so that registry renames (e.g. Task→MlTask) apply.
            all.Add(new ObjectModel(id, resolver.CsharpTypeName(id), props, isOpen));
        }

        // Second pass: remove any ObjectModels whose schema IDs were added to unionBodySchemaIds
        // during a union scan that happened AFTER the object was already added (ordering issue).
        var finalEmit = all
            .Where(t => !(t is ObjectModel && unionBodySchemaIds.Contains(t.SchemaId)))
            .ToList();

        var opOwnedSchemas = CollectOpOwnedResponseSchemas(doc, @namespace, registry);

        var emit = finalEmit
            .Where(t => !opOwnedSchemas.Contains(t.SchemaId)
                // WrapperKeyUnionModel is always emitted — its MappedCsharpType entry
                // exists only to help the type resolver name array-item properties; it
                // does not mean the union itself should be suppressed.
                && (t is WrapperKeyUnionModel || registry.MappedCsharpType(t.SchemaId) == null))
            .ToList();

        return new NamespaceModel { Namespace = @namespace, TypesToEmit = emit, AllTypes = finalEmit };
    }

    /// <summary>
    /// Tries to interpret a schema as a wrapper-key discriminated union:
    /// <code>
    /// oneOf:
    ///   - title: foo
    ///     properties:
    ///       foo: { $ref: '#/...FooBody' }
    ///     required: [foo]
    ///   - title: bar
    ///     ...
    /// </code>
    /// Returns true and populates <paramref name="model"/> if the schema fits this pattern.
    /// </summary>
    private static bool TryBuildWrapperKeyUnion(
        string schemaId,
        NJsonSchema.JsonSchema s,
        OpenApiDocument doc,
        IModelOverrides registry,
        ModelTypeResolver resolver,
        HashSet<string> consumedBodySchemaIds,
        out WrapperKeyUnionModel? model)
    {
        model = null;
        if (s.OneOf.Count == 0) return false;

        // Every variant must have exactly one required property (the discriminator key).
        // That property must be an object schema (the body), either inline or via $ref.
        var variants = new List<WrapperKeyVariant>();

        // Collect shared base properties: the set of property names that appear in ALL variants.
        // Typically these are tag, description, ignore_failure.
        Dictionary<string, NJsonSchema.JsonSchema>? sharedProps = null;

        foreach (var variant in s.OneOf)
        {
            var v = variant.ActualSchema;
            var required = v.RequiredProperties ?? Array.Empty<string>();
            var props = v.Properties ?? new Dictionary<string, NJsonSchema.JsonSchemaProperty>();

            if (required.Count != 1) return false; // not wrapper-key pattern

            var key = required.First();
            if (!props.TryGetValue(key, out var bodyProp)) return false;

            var bodySchema = bodyProp.ActualSchema;
            var bodyRefId = bodyProp.Reference?.Id ?? bodySchema.Reference?.Id;

            // If bodyRefId is null (NSwag inlined the $ref), try to recover it by
            // scanning the document's component schemas for the same schema instance.
            if (bodyRefId == null)
            {
                foreach (var (sid, sschema) in doc.Components.Schemas)
                    if (ReferenceEquals(sschema.ActualSchema, bodySchema)
                        || ReferenceEquals(sschema, bodyProp))
                    {
                        bodyRefId = sid;
                        break;
                    }
            }

            // Look up the named body schema (either the inline schema or the $ref target).
            NJsonSchema.JsonSchema? namedBody = null;
            if (bodyRefId != null && doc.Components.Schemas.TryGetValue(bodyRefId, out var refSchema))
                namedBody = refSchema.ActualSchema;
            else if (bodySchema.Properties?.Count > 0)
                namedBody = bodySchema;

            if (namedBody == null) return false;

            // Record the body schema ID so the regular emit loop skips it.
            if (bodyRefId != null)
                consumedBodySchemaIds.Add(bodyRefId);

            // The variant C# name: PascalCase of the body schema ID or the key.
            var variantCsharpName = bodyRefId != null
                ? resolver.CsharpTypeName(bodyRefId)
                : ToPascal(key);

            // Skip if mapped to an existing type.
            if (bodyRefId != null && registry.MappedCsharpType(bodyRefId) != null)
                return false;

            var bodyProps = BuildVariantProperties(namedBody, resolver);

            // Collect non-key properties of the variant envelope as shared base candidates.
            var envelopeProps = props
                .Where(p => p.Key != key)
                .ToDictionary(p => p.Key, p => (NJsonSchema.JsonSchema)p.Value, StringComparer.Ordinal);

            if (sharedProps == null)
                sharedProps = new Dictionary<string, NJsonSchema.JsonSchema>(envelopeProps, StringComparer.Ordinal);
            else
                // Intersect: keep only keys present in every variant.
                foreach (var k in sharedProps.Keys.ToList())
                    if (!envelopeProps.ContainsKey(k)) sharedProps.Remove(k);

            var versionAdded = variant.ExtensionData?.TryGetValue("x-version-added", out var va) == true
                ? va?.ToString()
                : null;

            variants.Add(new WrapperKeyVariant(key, variantCsharpName, versionAdded, bodyProps));
        }

        if (variants.Count == 0) return false;

        // Build base properties from the shared envelope properties.
        var basePropsList = (sharedProps ?? new Dictionary<string, NJsonSchema.JsonSchema>())
            .OrderBy(p => p.Key, StringComparer.Ordinal)
            .Select(p =>
            {
                var typeRef = resolver.ResolveTypeRef(
                    p.Value is NJsonSchema.JsonSchemaProperty jsp ? jsp : p.Value);
                return new ModelProperty(
                    WireName: p.Key,
                    CsharpName: ToPascal(p.Key),
                    CsharpType: typeRef.ToCsharp(),
                    Type: typeRef,
                    IsRequired: false,
                    Description: null,
                    VersionAdded: null);
            })
            .ToList();

        model = new WrapperKeyUnionModel(
            schemaId,
            resolver.CsharpTypeName(schemaId),
            variants,
            basePropsList);
        return true;
    }

    private static IReadOnlyList<ModelProperty> BuildVariantProperties(
        NJsonSchema.JsonSchema body, ModelTypeResolver resolver)
    {
        // Skip standard processor base properties — they live on the generated base interface.
        var baseProps = new HashSet<string>(
            new[] { "tag", "description", "ignore_failure" },
            StringComparer.Ordinal);

        var propSource = CollectProperties(body);
        var required = new HashSet<string>(
            body.RequiredProperties ?? Enumerable.Empty<string>(),
            StringComparer.Ordinal);

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

    /// <summary>
    /// Collects schema IDs that are directly referenced as response schemas from operations in
    /// the given namespace. These are emitted by the operation loop and must be skipped from the
    /// namespace scan to avoid duplicates.
    /// </summary>
    private static HashSet<string> CollectOpOwnedResponseSchemas(
        OpenApiDocument doc, string @namespace, IModelOverrides registry)
    {
        var prefix = @namespace + ".";
        var owned = new HashSet<string>(StringComparer.Ordinal);

        foreach (var pathItem in doc.Paths.Values)
        foreach (var op in pathItem.Values)
        {
            if (op.ExtensionData == null
                || !op.ExtensionData.TryGetValue("x-operation-group", out var g)
                || g?.ToString()?.StartsWith(prefix, StringComparison.Ordinal) != true)
                continue;

            var grp = g!.ToString()!;
            if (registry.ExcludedOps.Contains(grp)) continue;

            if (!op.ActualResponses.TryGetValue("200", out var resp)) continue;
            if (resp.Content == null || !resp.Content.TryGetValue("application/json", out var mt)) continue;

            var refPath = (mt.Schema as IJsonReference)?.ReferencePath;
            if (refPath == null) continue;

            // Extract schema ID from "#/components/schemas/ml._common___GetAgentResponse"
            // Only skip schemas conventionally named *Response — shared types like Memory/Message
            // can be both a response schema and a property type in other schemas.
            var schemaId = refPath.Split('/').Last();
            if (schemaId.StartsWith(prefix, StringComparison.Ordinal)
                && schemaId.EndsWith("Response", StringComparison.Ordinal))
                owned.Add(schemaId);
        }

        return owned;
    }

    /// <summary>
    /// Gather the own-properties of a schema, including those nested inside inline allOf members.
    /// This covers the common pattern: { allOf: [ {$ref: Base}, { type: object, properties: {...} } ] }.
    /// </summary>
    private static Dictionary<string, NJsonSchema.JsonSchema> CollectProperties(NJsonSchema.JsonSchema s)
    {
        var result = new Dictionary<string, NJsonSchema.JsonSchema>(StringComparer.Ordinal);

        // Direct properties.
        if (s.Properties != null)
            foreach (var kv in s.Properties)
                result[kv.Key] = kv.Value;

        // Inline allOf members that are plain objects (not refs) with properties.
        foreach (var sub in s.AllOf)
        {
            var actual = sub.ActualSchema;
            if (sub.HasReference) continue; // skip $ref entries — those are base classes
            if (actual.Properties == null) continue;
            foreach (var kv in actual.Properties)
                result.TryAdd(kv.Key, kv.Value);
        }

        return result;
    }

    private static string ToPascal(string name) => NamingConventions.ToPascal(name);
}
