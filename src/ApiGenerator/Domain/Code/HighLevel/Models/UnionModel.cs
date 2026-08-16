/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

#nullable enable

using System;
using System.Collections.Generic;

namespace ApiGenerator.Domain.Code.HighLevel.Models;

/// <summary>
/// Semantic intermediate representation for a discriminated union schema.
/// Records the wire encoding independently from plugin names and rendering decisions.
/// </summary>
public sealed class UnionModel
{
    /// <summary>Schema ID (component path) of this union.</summary>
    public string SchemaId { get; }

    /// <summary>Wire encoding classification.</summary>
    public UnionEncoding Encoding { get; }

    /// <summary>Variants of the union.</summary>
    public IReadOnlyList<UnionVariant> Variants { get; }

    /// <summary>
    /// For <see cref="UnionEncoding.InternalDiscriminator"/>: the property name that holds the discriminator value.
    /// Null for other encodings.
    /// </summary>
    public string? DiscriminatorProperty { get; }

    /// <summary>
    /// Shared properties present in all variants (for WrapperKeyOneOf).
    /// Empty for other encodings.
    /// </summary>
    public IReadOnlyList<UnionSharedProperty> SharedProperties { get; }

    /// <summary>
    /// Diagnostic message when encoding is <see cref="UnionEncoding.Unknown"/>.
    /// </summary>
    public string? DiagnosticMessage { get; }

    public UnionModel(
        string schemaId,
        UnionEncoding encoding,
        IReadOnlyList<UnionVariant> variants,
        string? discriminatorProperty = null,
        IReadOnlyList<UnionSharedProperty>? sharedProperties = null,
        string? diagnosticMessage = null)
    {
        SchemaId = schemaId ?? throw new ArgumentNullException(nameof(schemaId));
        Encoding = encoding;
        Variants = variants ?? throw new ArgumentNullException(nameof(variants));
        DiscriminatorProperty = discriminatorProperty;
        SharedProperties = sharedProperties ?? Array.Empty<UnionSharedProperty>();
        DiagnosticMessage = diagnosticMessage;
    }

    /// <summary>
    /// Creates a failed classification result with a diagnostic message.
    /// </summary>
    public static UnionModel Failed(string schemaId, string diagnosticMessage) =>
        new(schemaId, UnionEncoding.Unknown, Array.Empty<UnionVariant>(), diagnosticMessage: diagnosticMessage);
}

/// <summary>
/// A single variant within a union.
/// </summary>
public sealed class UnionVariant
{
    /// <summary>
    /// Wire key for wrapper-key encodings (e.g., "filter_query", "append").
    /// For InternalDiscriminator: the discriminator value (e.g., "linear", "simple").
    /// For TypedKeys: the type suffix (e.g., "avg", "sum").
    /// </summary>
    public string Key { get; }

    /// <summary>
    /// Schema ID of the variant body (if referencing a named schema), or null for inline schemas.
    /// </summary>
    public string? BodySchemaId { get; }

    /// <summary>
    /// The NJsonSchema instance for the variant body (for property resolution).
    /// </summary>
    public NJsonSchema.JsonSchema? BodySchema { get; }

    /// <summary>Version when this variant was added (from x-version-added).</summary>
    public string? VersionAdded { get; }

    public UnionVariant(string key, string? bodySchemaId, NJsonSchema.JsonSchema? bodySchema, string? versionAdded = null)
    {
        Key = key ?? throw new ArgumentNullException(nameof(key));
        BodySchemaId = bodySchemaId;
        BodySchema = bodySchema;
        VersionAdded = versionAdded;
    }
}

/// <summary>
/// A property shared across all variants in a wrapper-key union (e.g., tag, description, ignore_failure).
/// </summary>
public sealed class UnionSharedProperty
{
    /// <summary>Wire name of the property.</summary>
    public string WireName { get; }

    /// <summary>The schema for this property.</summary>
    public NJsonSchema.JsonSchema Schema { get; }

    public UnionSharedProperty(string wireName, NJsonSchema.JsonSchema schema)
    {
        WireName = wireName ?? throw new ArgumentNullException(nameof(wireName));
        Schema = schema ?? throw new ArgumentNullException(nameof(schema));
    }
}
