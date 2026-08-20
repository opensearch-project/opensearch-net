/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

#nullable enable

using System;
using System.Collections.Generic;

namespace ApiGenerator.Configuration.Overrides;

public abstract class ModelOverridesBase : IModelOverrides
{
    /// <summary>
    /// Cross-namespace shared types already present in OpenSearch.Client. These are common to
    /// all plugin namespaces — any plugin that references a <c>_common___*</c> schema gets the
    /// mapping automatically without having to redeclare it.
    /// </summary>
    private static readonly Dictionary<string, string> GlobalMappedTypes = new(StringComparer.Ordinal)
    {
        // Shared OSC types
        ["_common___Id"] = "Id",
        ["_common___IndexName"] = "IndexName",
        ["_common___Indices"] = "Indices",
        ["_common___Name"] = "Name",
        ["_common___Fields"] = "Fields",
        ["_common___NodeIds"] = "NodeIds",
        ["_common___ShardStatistics"] = "ShardStatistics",
        ["_common___Retries"] = "Retries",
        ["indices._common___IndexSettings"] = "IIndexSettings",
        // Primitive fallbacks: no dedicated OSC type exists for these.
        ["_common___VersionNumber"] = "long?",
        ["_common___SequenceNumber"] = "long?",
        ["_common___VersionString"] = "string",
        ["_common___StringifiedDouble"] = "string",
        ["_common___BulkByScrollFailure"] = "string",
        // Duration types map to the OSC Time class.
        ["_common___Duration"] = "Time",
        ["_common___DurationLarge"] = "Time",
        // Hand-written base class already in OpenSearch.Client.
        ["_common___WriteResponseBase"] = "WriteResponseBase",
        // Hand-written Result enum already exists in OpenSearch.Client.Document/Result.cs.
        ["_common___Result"] = "Result",
    };

    /// <summary>
    /// Global property-level type overrides keyed by <c>"{schemaId}.{propertyName}"</c>.
    /// Applies to any generated property that comes from the specified schema, regardless
    /// of which operation or namespace uses it. Use for shared schema properties that always
    /// map to a specific hand-written wrapper type (e.g. TypeMapping.properties → IProperties).
    /// </summary>
    private static readonly Dictionary<string, string> GlobalPropertyTypeOverrides = new(StringComparer.Ordinal)
    {
        // TypeMapping wrapper types
        ["_common.mapping___TypeMapping.properties"] = "IProperties",
        ["_common.mapping___TypeMapping.dynamic_templates"] = "IDynamicTemplateContainer",
        ["_common.mapping___TypeMapping.dynamic"] = "Union<bool, DynamicMapping>",
    };

    // ── Generation scope defaults ──────────────────────────────────────────────
    public abstract string Namespace { get; }
    public abstract string OutputFolder { get; }
    public virtual bool IncludeOperationSchemasInReachability => true;
    public virtual bool GenerateBodyOps => false;
    public virtual bool GenerateNonBodyOps => false;
    public virtual bool SuppressLowLevelApiImport => false;
    public virtual ISet<string> ExcludedOps { get; } = new HashSet<string>(StringComparer.Ordinal);
    public virtual IDictionary<string, string> OpNameOverrides { get; } = new Dictionary<string, string>(StringComparer.Ordinal);

    // ── Schema overrides ─────────────────────────────────────────────────────
    public virtual ISet<string> ExplicitlyPublicSchemaIds { get; } = new HashSet<string>(StringComparer.Ordinal);
    public virtual IDictionary<string, string> MappedTypes { get; } = new Dictionary<string, string>(StringComparer.Ordinal);
    public virtual IDictionary<string, string> RenamedTypes { get; } = new Dictionary<string, string>(StringComparer.Ordinal);

    public virtual IDictionary<string, UnionRenderingPolicy> UnionPolicies { get; } =
        new Dictionary<string, UnionRenderingPolicy>(StringComparer.Ordinal);

    public virtual ISet<string> SuppressedUnionSchemaIds { get; } = new HashSet<string>(StringComparer.Ordinal);

    public virtual IDictionary<string, string> PropertyTypeOverrides { get; } =
        new Dictionary<string, string>(StringComparer.Ordinal);

    public string? MappedCsharpType(string schemaId)
    {
        if (MappedTypes.TryGetValue(schemaId, out var t)) return t;
        if (GlobalMappedTypes.TryGetValue(schemaId, out t)) return t;
        return null;
    }

    /// <summary>
    /// Resolves a property-level type override. Checks (in order):
    /// 1. Per-plugin PropertyTypeOverrides with operation-scoped key ("{operationGroup}.{wireName}")
    /// 2. GlobalPropertyTypeOverrides with schema-scoped key ("{schemaId}.{wireName}")
    /// Returns null if no override exists.
    /// </summary>
    public string? ResolvePropertyTypeOverride(string operationGroup, string wireName, string? schemaId)
    {
        // Per-plugin operation-scoped override (most specific)
        var opKey = $"{operationGroup}.{wireName}";
        if (PropertyTypeOverrides.TryGetValue(opKey, out var t)) return t;

        // Global schema-scoped override
        if (schemaId != null)
        {
            var schemaKey = $"{schemaId}.{wireName}";
            if (GlobalPropertyTypeOverrides.TryGetValue(schemaKey, out t)) return t;
        }

        return null;
    }

    public string? RenamedCsharpName(string schemaId) =>
        RenamedTypes.TryGetValue(schemaId, out var t) ? t : null;
}
