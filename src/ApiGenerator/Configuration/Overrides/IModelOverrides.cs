/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

#nullable enable

using System.Collections.Generic;

namespace ApiGenerator.Configuration.Overrides;

/// <summary>
/// Per-plugin overrides for high-level model (request/response/shared-type) code generation.
/// Each plugin namespace provides an implementation that declares which schemas to skip,
/// map to existing types, rename, or treat as union bases, plus the generation scope
/// (namespace prefix, output folder, whether to generate body-ops/non-body-ops).
/// </summary>
public interface IModelOverrides
{
    // ── Generation scope ──────────────────────────────────────────────────────

    /// <summary>OpenAPI namespace prefix (e.g. <c>"ml"</c>, <c>"ingest"</c>).</summary>
    string Namespace { get; }

    /// <summary>Output subfolder under <c>_Generated/</c> (e.g. <c>"Ml"</c>, <c>"Ingest"</c>).</summary>
    string OutputFolder { get; }

    /// <summary>Whether operation request/response schemas seed shared-model reachability.</summary>
    bool IncludeOperationSchemasInReachability { get; }

    /// <summary>Whether to generate request/response types for body operations.</summary>
    bool GenerateBodyOps { get; }

    /// <summary>Whether to generate response types for non-body operations (GET/DELETE).</summary>
    bool GenerateNonBodyOps { get; }

    /// <summary>
    /// When <c>true</c>, the Requests and Descriptors Razor generators will NOT emit
    /// <c>using OpenSearch.Net.Specification.{Namespace}Api;</c>.  Set this for plugin
    /// namespaces that generate all their <c>RequestParameters</c> via
    /// <c>ModelsGenerator</c> and do not need the low-level namespace import — in
    /// particular when the low-level namespace uses generic parameter class names
    /// (<c>DeleteRequestParameters</c>, <c>GetRequestParameters</c>) that collide with
    /// identically-named classes in the top-level <c>OpenSearch.Net</c> namespace.
    /// </summary>
    bool SuppressLowLevelApiImport { get; }

    /// <summary>Operation groups to exclude from generation entirely (streaming ops, etc.).</summary>
    ISet<string> ExcludedOps { get; }

    /// <summary>
    /// Operation groups whose generated C# name should differ from the default Pascal(snake_name).
    /// Key: operation group (e.g. <c>"ml.get_task"</c>), Value: C# base name (e.g. <c>"GetMlTask"</c>).
    /// The base name is used to form <c>{BaseName}Request</c> / <c>{BaseName}Response</c>.
    /// </summary>
    IDictionary<string, string> OpNameOverrides { get; }

    // ── Schema overrides ──────────────────────────────────────────────────────

    /// <summary>
    /// Schema IDs retained as public generated models even when no current operation references
    /// them. This is output-scope compatibility policy, not OpenAPI structural information.
    /// </summary>
    ISet<string> ExplicitlyPublicSchemaIds { get; }

    /// <summary>
    /// Schema IDs mapped to existing C# types (not emitted; references resolve to the mapped type).
    /// Key: schema ID (e.g. <c>_common___Id</c>), Value: C# type name (e.g. <c>Id</c>).
    /// </summary>
    IDictionary<string, string> MappedTypes { get; }

    /// <summary>
    /// Schema IDs that ARE emitted as full models but under a renamed C# identifier to avoid
    /// collisions with BCL or framework types.
    /// Key: schema ID, Value: safe C# name (e.g. <c>MlTask</c>).
    /// </summary>
    IDictionary<string, string> RenamedTypes { get; }

    // ── Union rendering policy ──────────────────────────────────────────────

    /// <summary>
    /// Optional rendering policy for wrapper-key unions in this namespace.
    /// When non-null, WrapperKeyUnion.cshtml uses this policy to generate richer
    /// output (base class inheritance, generic descriptors, Field expression overloads,
    /// behavioral base classes). Keyed by union schema ID.
    /// </summary>
    IDictionary<string, UnionRenderingPolicy> UnionPolicies { get; }

    string? MappedCsharpType(string schemaId);
    string? RenamedCsharpName(string schemaId);
}
