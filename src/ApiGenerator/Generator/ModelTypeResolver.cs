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
    private readonly SchemaCatalog _schemas;

    public ModelTypeResolver(IModelOverrides registry, SchemaCatalog schemas)
    {
        _registry = registry;
        _schemas = schemas;
    }

    public SchemaCatalog Schemas => _schemas;

    public static string RefToTypeName(string refId) =>
        refId.Contains("___") ? refId.Split("___").Last() : refId;

    /// <summary>
    /// Returns the C# name for the given schema id, taking registry renames into account.
    /// If the registry has a <see cref="IModelOverrides.RenamedCsharpName"/> for this id, that
    /// wins; otherwise falls back to <see cref="RefToTypeName"/>.
    /// </summary>
    public string CsharpTypeName(string schemaId) =>
        _registry.RenamedCsharpName(schemaId) ?? RefToTypeName(schemaId);

    public bool TryGetSchemaId(JsonSchema schema, out string schemaId) =>
        _schemas.TryGetId(schema, out schemaId);

    public bool TryGetSchema(string schemaId, out JsonSchema schema) =>
        _schemas.TryGetSchema(schemaId, out schema);

    public TypeRef ResolveTypeRef(JsonSchema schema)
    {
        var s = schema.ActualSchema;

        if (s.IsEnum())
        {
            _schemas.TryGetId(schema, out var enumRefId);
            if (enumRefId != null)
            {
                var mappedEnum = _registry.MappedCsharpType(enumRefId);
                if (mappedEnum != null) return new MappedType(mappedEnum, false);
                return new EnumType(CsharpTypeName(enumRefId), true);
            }
        }

        if (s.Type.HasFlag(JsonObjectType.String))
        {
            // Check if this string-typed schema is a $ref to a mapped rich type (e.g. IndexName, Id, Name).
            // NSwag inlines string-alias $refs, but SchemaCatalog or Reference.Id may recover the ID.
            if ((TryGetReferenceId(schema, out var strRefId) || _schemas.TryGetId(schema, out strRefId))
                && strRefId != null)
            {
                var mappedStr = _registry.MappedCsharpType(strRefId);
                if (mappedStr != null) return new MappedType(mappedStr, false);
            }
            return new StringType();
        }
        if (s.Type.HasFlag(JsonObjectType.Boolean)) return new PrimitiveType("bool", true);
        if (s.Type.HasFlag(JsonObjectType.Integer)) return new PrimitiveType(s.Format == "int64" ? "long" : "int", true);
        if (s.Type.HasFlag(JsonObjectType.Number)) return new PrimitiveType(s.Format == "double" ? "double" : "float", true);

        if (s.Type.HasFlag(JsonObjectType.Array) && s.Item != null)
        {
            var item = s.Item.ActualSchema;
            if (TryGetObjectSchemaId(s.Item, out var refId))
            {
                var mapped = _registry.MappedCsharpType(refId);
                if (mapped != null) return new ListType(new MappedType(mapped, false), false);
                return new ListType(new ObjectRefType(CsharpTypeName(refId), false), false);
            }
            return new ListType(ResolveTypeRef(item), false);
        }

        if (s.Type.HasFlag(JsonObjectType.Object) && (s.Properties?.Count ?? 0) > 0)
        {
            _schemas.TryGetId(schema, out var objRefId);
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
            if (TryGetObjectSchemaId(s.AdditionalPropertiesSchema, out var refId))
                return new DictionaryType(new ObjectRefType(CsharpTypeName(refId), false), false);
            return new DictionaryType(ResolveTypeRef(ap), false);
        }

        if (s.Type.HasFlag(JsonObjectType.Object) && s.AllowAdditionalProperties)
            return new DictionaryType(new FallbackType(), false);

        if (TryGetReferenceId(schema, out var refSchemaId) || _schemas.TryGetId(schema, out refSchemaId))
        {
            if (refSchemaId != null)
            {
                var mapped = _registry.MappedCsharpType(refSchemaId);
                if (mapped != null) return new MappedType(mapped, false);
                // Only emit ObjectRefType for schemas with actual properties (true object types).
                // Schemas that are just unions/aliases (oneOf, $ref to primitives) without properties
                // should fall through to FallbackType to avoid generating non-existent interfaces.
                if (s.Properties?.Count > 0)
                    return new ObjectRefType(CsharpTypeName(refSchemaId), false);
            }
        }

        return new FallbackType();
    }

    private bool TryGetObjectSchemaId(JsonSchema schema, out string schemaId)
    {
        if (TryGetReferenceId(schema, out schemaId)) return true;

        var actual = schema.ActualSchema;
        if (actual.Type.HasFlag(JsonObjectType.Object) && (actual.Properties?.Count ?? 0) > 0
            || actual.OneOf.Count > 0)
            return _schemas.TryGetId(actual, out schemaId);

        schemaId = null!;
        return false;
    }

    private static bool TryGetReferenceId(JsonSchema schema, out string schemaId)
    {
        schemaId = schema.Reference?.Id ?? schema.ActualSchema.Reference?.Id!;
        return schemaId != null;
    }

    public string ResolveCsharpType(JsonSchema schema)
    {
        var s = schema.ActualSchema;

        // A $ref to a string enum resolves to a generated C# enum (nullable value type),
        // not a string. Detect this before the plain-string branch below. The id may come
        // from an intact Reference, or (once NSwag has inlined the ref) from SchemaCatalog.
        if (s.IsEnum())
        {
            _schemas.TryGetId(schema, out var enumRefId);
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
            if (TryGetObjectSchemaId(s.Item, out var refId))
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
            _schemas.TryGetId(schema, out var objRefId);
            if (objRefId != null)
            {
                var mappedObj = _registry.MappedCsharpType(objRefId);
                return mappedObj ?? $"I{CsharpTypeName(objRefId)}";
            }
        }

        if (s.Type.HasFlag(JsonObjectType.Object) && s.AdditionalPropertiesSchema != null)
        {
            var ap = s.AdditionalPropertiesSchema.ActualSchema;
            if (TryGetObjectSchemaId(s.AdditionalPropertiesSchema, out var refId))
                return $"IDictionary<string, I{CsharpTypeName(refId)}>";
            return $"IDictionary<string, {ResolveCsharpType(ap)}>";
        }

        if (s.Type.HasFlag(JsonObjectType.Object) && s.AllowAdditionalProperties)
            return "IDictionary<string, object>";

        if (TryGetReferenceId(schema, out var refSchemaId))
        {
            var mapped = _registry.MappedCsharpType(refSchemaId);
            return mapped ?? $"I{CsharpTypeName(refSchemaId)}";
        }

        return "object";
    }
}
