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

namespace ApiGenerator.Domain.Code.HighLevel.Models;

/// <summary>
/// Classifies union schemas by their structural encoding pattern.
/// Classification is purely spec-driven — no plugin names, ML/search-pipeline checks,
/// or C# rendering decisions are involved.
/// </summary>
public sealed class UnionClassifier
{
    private readonly SchemaCatalog _catalog;

    public UnionClassifier(SchemaCatalog catalog)
    {
        _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
    }

    /// <summary>
    /// Attempts to classify a schema as a union. Returns null if the schema is not a union pattern.
    /// </summary>
    public UnionModel? TryClassify(string schemaId, JsonSchema schema)
    {
        ArgumentNullException.ThrowIfNull(schema);
        ArgumentException.ThrowIfNullOrEmpty(schemaId);

        var actual = schema.ActualSchema;

        // Priority 1: InternalDiscriminator — has discriminator.propertyName with oneOf
        if (TryClassifyInternalDiscriminator(schemaId, actual, out var internalModel))
            return internalModel;

        // Priority 2: WrapperKeyOneOf — oneOf where each variant has exactly one required property
        if (TryClassifyWrapperKeyOneOf(schemaId, actual, out var wrapperKeyModel))
            return wrapperKeyModel;

        // Priority 3: FlatWrapperKey — object with minProperties=1, maxProperties=1
        if (TryClassifyFlatWrapperKey(schemaId, actual, out var flatModel))
            return flatModel;

        // Priority 4: TypedKeys — additionalProperties with typed key pattern
        if (TryClassifyTypedKeys(schemaId, actual, out var typedKeysModel))
            return typedKeysModel;

        return null;
    }

    /// <summary>
    /// Classifies a schema known to be a union. Returns Unknown encoding with diagnostic if classification fails.
    /// </summary>
    public UnionModel Classify(string schemaId, JsonSchema schema)
    {
        return TryClassify(schemaId, schema)
            ?? UnionModel.Failed(schemaId, "Schema does not match any known union pattern");
    }

    /// <summary>
    /// Checks if a schema matches any union pattern without fully classifying it.
    /// </summary>
    public bool IsUnionCandidate(JsonSchema schema)
    {
        var actual = schema.ActualSchema;

        // Has discriminator keyword
        if (HasDiscriminator(actual))
            return true;

        // Has oneOf with variants
        if (actual.OneOf.Count > 0)
            return true;

        // Has minProperties=1, maxProperties=1 with object properties
        if (IsFlatWrapperKeyCandidate(actual))
            return true;

        return HasTypedKeysExtension(actual);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // InternalDiscriminator: discriminator.propertyName with oneOf
    // ────────────────────────────────────────────────────────────────────────────

    private bool TryClassifyInternalDiscriminator(string schemaId, JsonSchema schema, out UnionModel? model)
    {
        model = null;

        if (!HasDiscriminator(schema) || schema.OneOf.Count == 0)
            return false;

        var discriminatorProperty = GetDiscriminatorPropertyName(schema);
        if (string.IsNullOrEmpty(discriminatorProperty))
        {
            model = UnionModel.Failed(schemaId, "Discriminator present but propertyName not specified");
            return true;
        }

        var variants = new List<UnionVariant>();

        foreach (var variant in schema.OneOf)
        {
            var variantActual = variant.ActualSchema;

            _catalog.TryGetId(variant, out var bodySchemaId);

            // Prefer the standard OpenAPI discriminator mapping, then fall back to titles,
            // const/enum discriminator values, and finally the canonical schema ID.
            var discriminatorValue = GetMappedDiscriminatorValue(schema, variant)
                ?? GetDiscriminatorValue(variant, variantActual, discriminatorProperty, bodySchemaId);
            if (string.IsNullOrEmpty(discriminatorValue))
            {
                model = UnionModel.Failed(schemaId, "Could not determine discriminator value for variant");
                return true;
            }

            var versionAdded = ExtractVersionAdded(variant);

            variants.Add(new UnionVariant(discriminatorValue, bodySchemaId, variantActual, versionAdded));
        }

        model = new UnionModel(schemaId, UnionEncoding.InternalDiscriminator, variants, discriminatorProperty);
        return true;
    }

    private static bool HasDiscriminator(JsonSchema schema) =>
        schema.DiscriminatorObject != null || !string.IsNullOrEmpty(schema.Discriminator);

    private static string? GetDiscriminatorPropertyName(JsonSchema schema)
    {
        // NJsonSchema exposes discriminator info via DiscriminatorObject (OAS3) or Discriminator (OAS2)
        if (schema.DiscriminatorObject != null)
            return schema.DiscriminatorObject.PropertyName;

        return !string.IsNullOrEmpty(schema.Discriminator) ? schema.Discriminator : null;
    }

    private string? GetMappedDiscriminatorValue(JsonSchema schema, JsonSchema variant)
    {
        var mapping = schema.DiscriminatorObject?.Mapping;
        if (mapping == null) return null;

        foreach (var (key, mappedSchema) in mapping)
        {
            if (ReferenceEquals(mappedSchema.ActualSchema, variant.ActualSchema))
                return key;

            var hasMappedId = _catalog.TryGetId(mappedSchema, out var mappedId);
            var hasVariantId = _catalog.TryGetId(variant, out var variantId);
            if (hasMappedId && hasVariantId
                && string.Equals(mappedId, variantId, StringComparison.Ordinal))
                return key;
        }

        return null;
    }

    private static string? GetDiscriminatorValue(
        JsonSchema variantRef,
        JsonSchema variantActual,
        string discriminatorProperty,
        string? canonicalSchemaId)
    {
        // Strategy 1: Use the variant's title
        if (!string.IsNullOrEmpty(variantRef.Title))
            return variantRef.Title;

        if (!string.IsNullOrEmpty(variantActual.Title))
            return variantActual.Title;

        // Strategy 2: Look for a const value on the discriminator property
        if (variantActual.Properties?.TryGetValue(discriminatorProperty, out var discProp) == true)
        {
            var constValue = discProp.ActualSchema.Enumeration?.FirstOrDefault()?.ToString();
            if (!string.IsNullOrEmpty(constValue))
                return constValue;
        }

        // Strategy 3: Use the canonical component ID suffix (last part after ___).
        var refId = canonicalSchemaId ?? variantRef.Reference?.Id ?? variantActual.Reference?.Id;
        if (!string.IsNullOrEmpty(refId))
        {
            var lastPart = refId.Split("___").Last();
            // Remove common suffixes like "Aggregation", "Processor"
            if (lastPart.EndsWith("Aggregation", StringComparison.Ordinal))
                return lastPart[..^11].ToLowerInvariant();
            if (lastPart.EndsWith("Processor", StringComparison.Ordinal))
                return lastPart[..^9].ToLowerInvariant();
            return lastPart.ToLowerInvariant();
        }

        return null;
    }

    // ────────────────────────────────────────────────────────────────────────────
    // WrapperKeyOneOf: oneOf where each variant has exactly one required property
    // ────────────────────────────────────────────────────────────────────────────

    private bool TryClassifyWrapperKeyOneOf(string schemaId, JsonSchema schema, out UnionModel? model)
    {
        model = null;

        if (schema.OneOf.Count == 0)
            return false;

        // Skip if this is an InternalDiscriminator pattern
        if (HasDiscriminator(schema))
            return false;

        var variants = new List<UnionVariant>();
        Dictionary<string, JsonSchema>? sharedProps = null;

        foreach (var variant in schema.OneOf)
        {
            var variantActual = variant.ActualSchema;
            var required = variantActual.RequiredProperties ?? Array.Empty<string>();
            var properties = variantActual.Properties ?? new Dictionary<string, JsonSchemaProperty>();

            // Must have exactly one required property (the wrapper key)
            if (required.Count != 1)
                return false;

            var key = required.First();
            if (!properties.TryGetValue(key, out var bodyProp))
                return false;

            var bodyActual = bodyProp.ActualSchema;

            // The body must be an object schema (has properties or is a $ref to one)
            if (!IsObjectLike(bodyProp, bodyActual))
                return false;

            // Get the body schema ID
            _catalog.TryGetId(bodyProp, out var bodySchemaId);

            var versionAdded = ExtractVersionAdded(variant);

            variants.Add(new UnionVariant(key, bodySchemaId, bodyActual, versionAdded));

            // Collect non-key properties for shared properties calculation
            var envelopeProps = properties
                .Where(p => p.Key != key)
                .ToDictionary(p => p.Key, p => (JsonSchema)p.Value, StringComparer.Ordinal);

            if (sharedProps == null)
            {
                sharedProps = new Dictionary<string, JsonSchema>(envelopeProps, StringComparer.Ordinal);
            }
            else
            {
                // Intersect: keep only keys present in every variant
                foreach (var k in sharedProps.Keys.ToList())
                {
                    if (!envelopeProps.ContainsKey(k))
                        sharedProps.Remove(k);
                }
            }
        }

        if (variants.Count == 0)
            return false;

        var shared = (sharedProps ?? new Dictionary<string, JsonSchema>())
            .OrderBy(p => p.Key, StringComparer.Ordinal)
            .Select(p => new UnionSharedProperty(p.Key, p.Value))
            .ToList();

        model = new UnionModel(schemaId, UnionEncoding.WrapperKeyOneOf, variants, sharedProperties: shared);
        return true;
    }

    private static bool IsObjectLike(JsonSchema wrapper, JsonSchema actual)
    {
        // Has $ref to a named schema
        if (wrapper.Reference?.Id != null || actual.Reference?.Id != null)
            return true;

        // Has properties
        if ((actual.Properties?.Count ?? 0) > 0)
            return true;

        // Is a oneOf (nested union)
        if (actual.OneOf.Count > 0)
            return true;

        // Has allOf composition
        if (actual.AllOf.Count > 0)
            return true;

        return false;
    }

    // ────────────────────────────────────────────────────────────────────────────
    // FlatWrapperKey: object with minProperties=1, maxProperties=1
    // ────────────────────────────────────────────────────────────────────────────

    private bool TryClassifyFlatWrapperKey(string schemaId, JsonSchema schema, out UnionModel? model)
    {
        model = null;

        if (!IsFlatWrapperKeyCandidate(schema))
            return false;

        var properties = schema.Properties;
        if (properties == null || properties.Count == 0)
            return false;

        var variants = new List<UnionVariant>();

        foreach (var (key, prop) in properties.OrderBy(p => p.Key, StringComparer.Ordinal))
        {
            var bodyActual = prop.ActualSchema;

            // Get the body schema ID
            _catalog.TryGetId(prop, out var bodySchemaId);

            var versionAdded = ExtractVersionAdded(prop);

            variants.Add(new UnionVariant(key, bodySchemaId, bodyActual, versionAdded));
        }

        model = new UnionModel(schemaId, UnionEncoding.FlatWrapperKey, variants);
        return true;
    }

    private static bool IsFlatWrapperKeyCandidate(JsonSchema schema)
    {
        // Must be an object with minProperties=1 and maxProperties=1
        if (!schema.Type.HasFlag(JsonObjectType.Object))
            return false;

        // Check for minProperties and maxProperties
        // NJsonSchema exposes these as nullable ints
        var minProps = schema.MinProperties;
        var maxProps = schema.MaxProperties;

        return minProps == 1 && maxProps == 1;
    }

    // ────────────────────────────────────────────────────────────────────────────
    // TypedKeys: dictionary with typed key pattern (name#type)
    // ────────────────────────────────────────────────────────────────────────────

    private bool TryClassifyTypedKeys(string schemaId, JsonSchema schema, out UnionModel? model)
    {
        model = null;
        if (!HasTypedKeysExtension(schema)) return false;

        var variants = new List<UnionVariant>();
        var valueSchema = schema.AdditionalPropertiesSchema?.ActualSchema;
        if (valueSchema != null)
        {
            foreach (var variant in valueSchema.OneOf)
            {
                _catalog.TryGetId(variant, out var bodySchemaId);
                var key = variant.Title
                    ?? variant.ActualSchema.Title
                    ?? bodySchemaId?.Split("___").Last();
                if (string.IsNullOrEmpty(key)) continue;

                variants.Add(new UnionVariant(
                    key,
                    bodySchemaId,
                    variant.ActualSchema,
                    ExtractVersionAdded(variant)));
            }
        }

        model = new UnionModel(schemaId, UnionEncoding.TypedKeys, variants);
        return true;
    }

    private static bool HasTypedKeysExtension(JsonSchema schema) =>
        schema.ExtensionData?.TryGetValue("x-typed-keys", out var value) == true
        && value is bool enabled
        && enabled;

    // ────────────────────────────────────────────────────────────────────────────
    // Helpers
    // ────────────────────────────────────────────────────────────────────────────

    private static string? ExtractVersionAdded(JsonSchema schema)
    {
        if (schema.ExtensionData?.TryGetValue("x-version-added", out var va) == true)
            return va?.ToString();
        return null;
    }
}
