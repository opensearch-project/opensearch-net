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
using NJsonSchema;
using NSwag;

namespace ApiGenerator.Domain.Code.HighLevel.Models;

/// <summary>
/// Builds a <see cref="ReferenceGraph"/> from an OpenAPI document and plugin overrides.
/// Extracts dependency edges from normalized effective properties, normalized composition
/// members, array items, dictionary values, union variants, and allOf base schemas.
///
/// Normalization is required — the builder consumes pre-computed normalized facts for all
/// dependency analysis. No raw allOf iteration is performed downstream.
/// </summary>
public sealed class ReferenceGraphBuilder
{
    private readonly OpenApiDocument _document;
    private readonly IModelOverrides _plugin;
    private readonly SchemaCatalog _catalog;
    private readonly NormalizationResult _normalization;
    private readonly ReferenceGraph _graph = new();
    private readonly HashSet<string> _visited = new(StringComparer.Ordinal);

    public ReferenceGraphBuilder(OpenApiDocument document, IModelOverrides plugin, SchemaCatalog catalog,
        NormalizationResult normalization)
    {
        _document = document ?? throw new ArgumentNullException(nameof(document));
        _plugin = plugin ?? throw new ArgumentNullException(nameof(plugin));
        _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
        _normalization = normalization ?? throw new ArgumentNullException(nameof(normalization));
    }

    /// <summary>
    /// Builds the complete reference graph for the plugin's namespace.
    /// </summary>
    public ReferenceGraph Build()
    {
        // Mark mapped types as external leaves
        foreach (var (schemaId, _) in _plugin.MappedTypes)
            _graph.MarkAsExternalLeaf(schemaId);

        // Collect operation roots (request/response schemas)
        CollectOperationRoots();

        // Register all schemas in the namespace and extract their dependencies
        RegisterNamespaceSchemas();

        return _graph;
    }

    /// <summary>
    /// Builds a reference graph with specific additional roots (for testing).
    /// </summary>
    public ReferenceGraph BuildWithAdditionalRoots(IEnumerable<string> additionalRoots)
    {
        var graph = Build();
        foreach (var root in additionalRoots)
            graph.MarkAsRoot(root);
        return graph;
    }

    private void CollectOperationRoots()
    {
        if (!_plugin.IncludeOperationSchemasInReachability)
            return;

        var prefix = _plugin.Namespace + ".";

        foreach (var pathItem in _document.Paths.Values)
        foreach (var op in pathItem.Values)
        {
            if (op.ExtensionData == null) continue;
            if (!op.ExtensionData.TryGetValue("x-operation-group", out var g)) continue;
            var grp = g?.ToString();
            if (grp == null || !grp.StartsWith(prefix, StringComparison.Ordinal)) continue;
            if (_plugin.ExcludedOps.Contains(grp)) continue;

            // Request body schema is a root
            if (op.ActualRequestBody?.Content?.TryGetValue("application/json", out var reqMt) == true
                && reqMt.Schema != null)
            {
                var reqSchemaId = GetSchemaId(reqMt.Schema);
                if (reqSchemaId != null)
                {
                    _graph.MarkAsRoot(reqSchemaId);
                    TraverseSchema(reqMt.Schema, reqSchemaId);
                }
                else
                {
                    // Inline request schema - traverse its dependencies
                    TraverseInlineSchema(reqMt.Schema);
                }
            }

            // All 2xx response schemas are roots
            foreach (var (statusCode, response) in op.ActualResponses)
            {
                if (!statusCode.StartsWith("2", StringComparison.Ordinal)) continue;
                if (response.Content == null) continue;
                if (!response.Content.TryGetValue("application/json", out var respMt)) continue;
                if (respMt.Schema == null) continue;

                var respSchemaId = GetSchemaId(respMt.Schema);
                if (respSchemaId != null)
                {
                    _graph.MarkAsRoot(respSchemaId);
                    if (respSchemaId.StartsWith(prefix, StringComparison.Ordinal)
                        && respSchemaId.EndsWith("Response", StringComparison.Ordinal))
                        _graph.MarkAsOpOwned(respSchemaId);
                    TraverseSchema(respMt.Schema, respSchemaId);
                }
                else
                {
                    TraverseInlineSchema(respMt.Schema);
                }
            }
        }
    }

    private void RegisterNamespaceSchemas()
    {
        var prefix = _plugin.Namespace + ".";

        foreach (var (schemaId, schema) in _document.Components.Schemas
            .Where(kv => kv.Key.StartsWith(prefix, StringComparison.Ordinal))
            .OrderBy(kv => kv.Key, StringComparer.Ordinal))
        {
            if (_visited.Contains(schemaId)) continue;
            _graph.RegisterNode(schemaId);
            TraverseSchema(schema, schemaId);
        }
    }

    private void TraverseSchema(JsonSchema schema, string sourceSchemaId)
    {
        if (!_visited.Add(sourceSchemaId))
            return;

        if (!_normalization.TryGet(sourceSchemaId, out var normalized))
        {
            throw new InvalidOperationException(
                $"Schema '{sourceSchemaId}' was not discovered during normalization.");
        }

        AddNormalizedDependencies(sourceSchemaId, normalized, markAsRoots: false);
    }

    private void TraverseInlineSchema(JsonSchema schema)
    {
        if (!_normalization.TryGetForSchema(schema, out var normalized))
        {
            throw new InvalidOperationException(
                "Inline operation schema was not discovered during normalization.");
        }

        AddNormalizedDependencies(normalized.SchemaId, normalized, markAsRoots: true);
    }

    private void AddDependency(string sourceSchemaId, JsonSchema targetSchema)
    {
        var targetId = GetSchemaId(targetSchema);
        if (targetId == null)
        {
            AddInlineDependencies(sourceSchemaId, targetSchema.ActualSchema);
            return;
        }

        _graph.AddEdge(sourceSchemaId, targetId);

        if (!_visited.Contains(targetId))
            TraverseSchema(targetSchema, targetId);
    }

    private void AddInlineDependencies(string sourceSchemaId, JsonSchema schema)
    {
        if (!_normalization.TryGetForSchema(schema, out var normalized))
        {
            throw new InvalidOperationException(
                $"Inline dependency of '{sourceSchemaId}' was not discovered during normalization.");
        }

        AddNormalizedDependencies(sourceSchemaId, normalized, markAsRoots: false);
    }

    private void AddNormalizedDependencies(
        string sourceSchemaId,
        NormalizedSchema normalized,
        bool markAsRoots)
    {
        foreach (var dependencySchemaId in normalized.DependencySchemaIds)
        {
            if (markAsRoots)
                _graph.MarkAsRoot(dependencySchemaId);
            else
                _graph.AddEdge(sourceSchemaId, dependencySchemaId);

            if (!_visited.Contains(dependencySchemaId)
                && _catalog.TryGetSchema(dependencySchemaId, out var dependencySchema))
            {
                TraverseSchema(dependencySchema, dependencySchemaId);
            }
        }
    }

    private string? GetSchemaId(JsonSchema schema)
    {
        // Try direct reference first
        if (!string.IsNullOrEmpty(schema.Reference?.Id))
            return schema.Reference.Id;

        // Try catalog lookup (handles NSwag inlining)
        if (_catalog.TryGetId(schema, out var catalogId))
            return catalogId;

        return null;
    }
}
