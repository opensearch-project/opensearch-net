/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

#nullable enable

using System.Collections.Generic;
using System.Linq;
using ApiGenerator.Configuration.Overrides;
using ApiGenerator.Domain.Code.HighLevel.Models;
using NJsonSchema;

namespace ApiGenerator.Generator;

public sealed class ModelTypeResolver
{
    private readonly IModelOverrides _registry;

    // Maps a resolved enum schema instance back to its component schema id. NSwag inlines
    // $refs during resolution, dropping Reference.Id, so property schemas that point at an
    // enum lose their id — this reverse lookup (by reference equality) recovers it so enum
    // properties are typed as the generated enum instead of falling back to string.
    private readonly IReadOnlyDictionary<JsonSchema, string> _enumSchemaIds;

    // Maps a resolved object schema instance back to its component schema id. Mirrors
    // _enumSchemaIds but for named objects with properties. Enables object-typed properties
    // to resolve to I<TypeName> instead of falling back to IDictionary<string, object>.
    private readonly IReadOnlyDictionary<JsonSchema, string> _objectSchemaIds;

    public ModelTypeResolver(
        IModelOverrides registry,
        IReadOnlyDictionary<JsonSchema, string>? enumSchemaIds = null,
        IReadOnlyDictionary<JsonSchema, string>? objectSchemaIds = null)
    {
        _registry = registry;
        _enumSchemaIds = enumSchemaIds ?? new Dictionary<JsonSchema, string>();
        _objectSchemaIds = objectSchemaIds ?? new Dictionary<JsonSchema, string>();
    }

    public static string RefToTypeName(string refId) =>
        refId.Contains("___") ? refId.Split("___").Last() : refId;

    /// <summary>
    /// Returns the C# name for the given schema id, taking registry renames into account.
    /// If the registry has a <see cref="IModelOverrides.RenamedCsharpName"/> for this id, that
    /// wins; otherwise falls back to <see cref="RefToTypeName"/>.
    /// </summary>
    public string CsharpTypeName(string schemaId) =>
        _registry.RenamedCsharpName(schemaId) ?? RefToTypeName(schemaId);

    /// <summary>
    /// Build a reverse map from each component's resolved enum schema instance to its schema id.
    /// Enables the resolver to name enum-typed properties even after NSwag inlines their $refs.
    /// </summary>
    public static IReadOnlyDictionary<JsonSchema, string> BuildEnumSchemaIds(NSwag.OpenApiDocument doc)
    {
        var map = new Dictionary<JsonSchema, string>();
        foreach (var (id, schema) in doc.Components.Schemas)
        {
            var actual = schema.ActualSchema;
            if (actual.IsEnum()) map[actual] = id;
        }
        return map;
    }

    /// <summary>
    /// Reverse-map from each named object component's resolved schema instance to its id.
    /// Lets the resolver name object-typed properties even after NSwag inlines their $refs.
    /// Excludes enums (handled separately) and free-form maps (no named properties).
    /// </summary>
    public static IReadOnlyDictionary<JsonSchema, string> BuildObjectSchemaIds(NSwag.OpenApiDocument doc)
    {
        var map = new Dictionary<JsonSchema, string>();
        foreach (var (id, schema) in doc.Components.Schemas)
        {
            var actual = schema.ActualSchema;
            if (actual.IsEnum()) continue;
            // Track plain object schemas with named properties.
            if (actual.Type.HasFlag(JsonObjectType.Object) && (actual.Properties?.Count ?? 0) > 0)
            {
                map[actual] = id;
                continue;
            }
            // Also track oneOf union containers (wrapper-key discriminated unions such as
            // RequestProcessor, ResponseProcessor). These have no own properties but are
            // referenced from array item schemas whose $refs get inlined by NSwag.
            if (actual.OneOf.Count > 0)
                map[actual] = id;
        }
        return map;
    }

    public TypeRef ResolveTypeRef(JsonSchema schema)
    {
        var s = schema.ActualSchema;

        if (s.IsEnum())
        {
            var enumRefId = schema.Reference?.Id ?? s.Reference?.Id;
            if (enumRefId == null) _enumSchemaIds.TryGetValue(s, out enumRefId);
            if (enumRefId != null)
            {
                var mappedEnum = _registry.MappedCsharpType(enumRefId);
                if (mappedEnum != null) return new MappedType(mappedEnum, false);
                return new EnumType(CsharpTypeName(enumRefId), true);
            }
        }

        if (s.Type.HasFlag(JsonObjectType.String)) return new StringType();
        if (s.Type.HasFlag(JsonObjectType.Boolean)) return new PrimitiveType("bool", true);
        if (s.Type.HasFlag(JsonObjectType.Integer)) return new PrimitiveType(s.Format == "int64" ? "long" : "int", true);
        if (s.Type.HasFlag(JsonObjectType.Number)) return new PrimitiveType(s.Format == "double" ? "double" : "float", true);

        if (s.Type.HasFlag(JsonObjectType.Array) && s.Item != null)
        {
            var item = s.Item.ActualSchema;
            var refId = s.Item.Reference?.Id ?? item.Reference?.Id;
            if (refId == null) _objectSchemaIds.TryGetValue(item, out refId);
            if (refId != null)
            {
                var mapped = _registry.MappedCsharpType(refId);
                if (mapped != null) return new ListType(new MappedType(mapped, false), false);
                return new ListType(new ObjectRefType(CsharpTypeName(refId), false), false);
            }
            return new ListType(ResolveTypeRef(item), false);
        }

        if (s.Type.HasFlag(JsonObjectType.Object) && (s.Properties?.Count ?? 0) > 0)
        {
            var objRefId = schema.Reference?.Id ?? s.Reference?.Id;
            if (objRefId == null) _objectSchemaIds.TryGetValue(s, out objRefId);
            if (objRefId != null)
            {
                var mappedObj = _registry.MappedCsharpType(objRefId);
                if (mappedObj != null) return new MappedType(mappedObj, false);
                return new ObjectRefType(CsharpTypeName(objRefId), false);
            }
        }

        if (s.Type.HasFlag(JsonObjectType.Object) && s.AdditionalPropertiesSchema != null)
        {
            var ap = s.AdditionalPropertiesSchema.ActualSchema;
            var refId = s.AdditionalPropertiesSchema.Reference?.Id ?? ap.Reference?.Id;
            if (refId != null) return new DictionaryType(new ObjectRefType(CsharpTypeName(refId), false), false);
            return new DictionaryType(ResolveTypeRef(ap), false);
        }

        if (s.Type.HasFlag(JsonObjectType.Object) && s.AllowAdditionalProperties)
            return new DictionaryType(new FallbackType(), false);

        var refSchemaId = schema.Reference?.Id;
        if (refSchemaId != null)
        {
            var mapped = _registry.MappedCsharpType(refSchemaId);
            if (mapped != null) return new MappedType(mapped, false);
            return new ObjectRefType(CsharpTypeName(refSchemaId), false);
        }

        return new FallbackType();
    }

    public string ResolveCsharpType(JsonSchema schema)
    {
        var s = schema.ActualSchema;

        // A $ref to a string enum resolves to a generated C# enum (nullable value type),
        // not a string. Detect this before the plain-string branch below. The id may come
        // from an intact Reference, or (once NSwag has inlined the ref) from the
        // resolved-schema reverse lookup.
        if (s.IsEnum())
        {
            var enumRefId = schema.Reference?.Id ?? s.Reference?.Id;
            if (enumRefId == null) _enumSchemaIds.TryGetValue(s, out enumRefId);
            if (enumRefId != null)
            {
                var mappedEnum = _registry.MappedCsharpType(enumRefId);
                return mappedEnum ?? $"{CsharpTypeName(enumRefId)}?";
            }
        }

        if (s.Type.HasFlag(JsonObjectType.String)) return "string";
        if (s.Type.HasFlag(JsonObjectType.Boolean)) return "bool?";
        if (s.Type.HasFlag(JsonObjectType.Integer)) return s.Format == "int64" ? "long?" : "int?";
        if (s.Type.HasFlag(JsonObjectType.Number)) return s.Format == "double" ? "double?" : "float?";

        if (s.Type.HasFlag(JsonObjectType.Array) && s.Item != null)
        {
            var item = s.Item.ActualSchema;
            var refId = s.Item.Reference?.Id ?? item.Reference?.Id;
            // If refId is null the $ref was inlined by NSwag; try to recover it via
            // the reverse instance map (handles both plain objects and oneOf unions).
            if (refId == null) _objectSchemaIds.TryGetValue(item, out refId);
            if (refId != null)
            {
                var mapped = _registry.MappedCsharpType(refId);
                return mapped != null ? $"IList<{mapped}>" : $"IList<I{CsharpTypeName(refId)}>";
            }
            return $"IList<{ResolveCsharpType(item)}>";
        }

        // A $ref to a named object schema resolves to the generated C# interface type,
        // not a dictionary. The id may come from an intact Reference, or (once NSwag has
        // inlined the ref) from the resolved-schema reverse lookup.
        // This must come BEFORE the AdditionalPropertiesSchema and AllowAdditionalProperties
        // branches so a named type with properties is never degraded to a dictionary even if
        // NJsonSchema reports AllowAdditionalProperties=true on it.
        if (s.Type.HasFlag(JsonObjectType.Object) && (s.Properties?.Count ?? 0) > 0)
        {
            var objRefId = schema.Reference?.Id ?? s.Reference?.Id;
            if (objRefId == null) _objectSchemaIds.TryGetValue(s, out objRefId);
            if (objRefId != null)
            {
                var mappedObj = _registry.MappedCsharpType(objRefId);
                return mappedObj ?? $"I{CsharpTypeName(objRefId)}";
            }
        }

        if (s.Type.HasFlag(JsonObjectType.Object) && s.AdditionalPropertiesSchema != null)
        {
            var ap = s.AdditionalPropertiesSchema.ActualSchema;
            var refId = s.AdditionalPropertiesSchema.Reference?.Id ?? ap.Reference?.Id;
            if (refId != null) return $"IDictionary<string, I{CsharpTypeName(refId)}>";
            return $"IDictionary<string, {ResolveCsharpType(ap)}>";
        }

        if (s.Type.HasFlag(JsonObjectType.Object) && s.AllowAdditionalProperties)
            return "IDictionary<string, object>";

        var refSchemaId = schema.Reference?.Id;
        if (refSchemaId != null)
        {
            var mapped = _registry.MappedCsharpType(refSchemaId);
            return mapped ?? $"I{CsharpTypeName(refSchemaId)}";
        }

        return "object";
    }
}
