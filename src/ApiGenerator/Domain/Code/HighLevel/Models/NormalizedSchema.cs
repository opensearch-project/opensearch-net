/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

#nullable enable

using System;
using System.Collections.Generic;
using NJsonSchema;

namespace ApiGenerator.Domain.Code.HighLevel.Models;

/// <summary>
/// Immutable normalized representation of a schema's effective properties and composition.
/// Produced by <see cref="SchemaNormalizer"/> and keyed by canonical schema ID from
/// <see cref="SchemaCatalog"/> (for component schemas) or by synthetic identity (for inline schemas).
///
/// Downstream model construction (NamespaceModel, ReferenceGraphBuilder) consumes these
/// facts rather than re-interpreting raw allOf/composition from NSwag schemas.
/// </summary>
public sealed class NormalizedSchema
{
    /// <summary>The canonical schema ID from SchemaCatalog (or synthetic ID for inline schemas).</summary>
    public string SchemaId { get; }

    /// <summary>
    /// The effective properties of this schema after allOf flattening.
    /// Keyed by wire property name. Value is the NSwag property schema (for downstream
    /// type resolution). Includes properties from inline allOf members and direct properties.
    /// Direct properties override allOf-inherited ones.
    /// </summary>
    public IReadOnlyDictionary<string, JsonSchema> EffectiveProperties { get; }

    /// <summary>
    /// The set of effective required property names after merging base and own requirements.
    /// Includes required fields from both the schema itself and inline allOf members.
    /// </summary>
    public IReadOnlySet<string> RequiredProperties { get; }

    /// <summary>
    /// Ordered allOf composition members preserved for downstream semantic consumption.
    /// Each entry records whether it's a $ref (with resolved schema ID) or inline, and
    /// retains the raw schema for structural queries. Order matches declaration order.
    /// </summary>
    public IReadOnlyList<CompositionMember> AllOfMembers { get; }

    /// <summary>
    /// oneOf variant schema references preserved for downstream union classification.
    /// Each entry is a schema ID (for $ref variants) or null (for inline variants),
    /// with the raw schema retained. Order matches declaration order.
    /// </summary>
    public IReadOnlyList<CompositionMember> OneOfVariants { get; }

    /// <summary>
    /// anyOf variant schema references preserved similarly to oneOf.
    /// </summary>
    public IReadOnlyList<CompositionMember> AnyOfVariants { get; }

    /// <summary>
    /// Discriminator information if present on the schema. Null if no discriminator.
    /// </summary>
    public DiscriminatorInfo? Discriminator { get; }

    /// <summary>
    /// Schema IDs of inline allOf member properties that reference named schemas.
    /// Used by ReferenceGraphBuilder for dependency edges from inline compositions.
    /// </summary>
    public IReadOnlyList<InlinePropertyRef> InlinePropertyRefs { get; }

    /// <summary>
    /// Named component schema IDs reachable through normalized properties, arrays,
    /// dictionaries, and composition members.
    /// </summary>
    public IReadOnlySet<string> DependencySchemaIds { get; }

    /// <summary>
    /// Whether this schema uses allOf composition (has at least one allOf member).
    /// </summary>
    public bool HasAllOfComposition => AllOfMembers.Count > 0;

    /// <summary>
    /// Schema IDs of allOf $ref entries (base schemas). Derived from AllOfMembers for convenience.
    /// </summary>
    public IReadOnlyList<string> BaseSchemaIds { get; }

    public NormalizedSchema(
        string schemaId,
        IReadOnlyDictionary<string, JsonSchema> effectiveProperties,
        IReadOnlySet<string> requiredProperties,
        IReadOnlyList<CompositionMember> allOfMembers,
        IReadOnlyList<CompositionMember> oneOfVariants,
        IReadOnlyList<CompositionMember> anyOfVariants,
        DiscriminatorInfo? discriminator,
        IReadOnlyList<InlinePropertyRef> inlinePropertyRefs,
        IReadOnlySet<string> dependencySchemaIds)
    {
        SchemaId = schemaId ?? throw new ArgumentNullException(nameof(schemaId));
        EffectiveProperties = effectiveProperties ?? throw new ArgumentNullException(nameof(effectiveProperties));
        RequiredProperties = requiredProperties ?? throw new ArgumentNullException(nameof(requiredProperties));
        AllOfMembers = allOfMembers ?? throw new ArgumentNullException(nameof(allOfMembers));
        OneOfVariants = oneOfVariants ?? throw new ArgumentNullException(nameof(oneOfVariants));
        AnyOfVariants = anyOfVariants ?? throw new ArgumentNullException(nameof(anyOfVariants));
        Discriminator = discriminator;
        InlinePropertyRefs = inlinePropertyRefs ?? throw new ArgumentNullException(nameof(inlinePropertyRefs));
        DependencySchemaIds = dependencySchemaIds ?? throw new ArgumentNullException(nameof(dependencySchemaIds));

        // Derive BaseSchemaIds from AllOfMembers for backward compatibility
        var baseIds = new List<string>();
        foreach (var member in allOfMembers)
        {
            if (member.IsReference && member.ReferencedSchemaId != null)
                baseIds.Add(member.ReferencedSchemaId);
        }
        BaseSchemaIds = baseIds;
    }
}

/// <summary>
/// Records a composition member (allOf, oneOf, or anyOf entry).
/// </summary>
public sealed class CompositionMember
{
    /// <summary>Whether this member is a $ref (true) or inline schema (false).</summary>
    public bool IsReference { get; }

    /// <summary>The referenced schema ID if this is a $ref member. Null for inline members.</summary>
    public string? ReferencedSchemaId { get; }

    /// <summary>The raw NSwag schema for structural queries. Always non-null.</summary>
    public JsonSchema RawSchema { get; }

    public CompositionMember(bool isReference, string? referencedSchemaId, JsonSchema rawSchema)
    {
        IsReference = isReference;
        ReferencedSchemaId = referencedSchemaId;
        RawSchema = rawSchema ?? throw new ArgumentNullException(nameof(rawSchema));
    }
}

/// <summary>
/// Discriminator information from a schema.
/// </summary>
public sealed class DiscriminatorInfo
{
    /// <summary>The discriminator property name.</summary>
    public string PropertyName { get; }

    /// <summary>Discriminator mapping (value -> schema ref), if present.</summary>
    public IReadOnlyDictionary<string, string> Mapping { get; }

    public DiscriminatorInfo(string propertyName, IReadOnlyDictionary<string, string> mapping)
    {
        PropertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
        Mapping = mapping ?? throw new ArgumentNullException(nameof(mapping));
    }
}

/// <summary>
/// Records a named property schema reference found inside an inline allOf member.
/// </summary>
public sealed class InlinePropertyRef
{
    public string PropertyName { get; }
    public string ReferencedSchemaId { get; }

    public InlinePropertyRef(string propertyName, string referencedSchemaId)
    {
        PropertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
        ReferencedSchemaId = referencedSchemaId ?? throw new ArgumentNullException(nameof(referencedSchemaId));
    }
}
