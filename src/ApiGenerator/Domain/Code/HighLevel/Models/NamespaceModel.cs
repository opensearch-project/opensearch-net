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

        var opOwnedSchemas = CollectOpOwnedResponseSchemas(doc, @namespace, registry);

        var emit = all
            .Where(t => !opOwnedSchemas.Contains(t.SchemaId)
                && registry.MappedCsharpType(t.SchemaId) == null)
            .ToList();

        return new NamespaceModel { Namespace = @namespace, TypesToEmit = emit, AllTypes = all };
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
