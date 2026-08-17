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

namespace ApiGenerator.Domain.Code.HighLevel.Models;

/// <summary>
/// Directed dependency graph for schema references. Tracks edges from model schema IDs
/// to their referenced schema IDs, enabling reachability analysis from root models.
///
/// Supports three node classifications:
/// - External leaves: schemas mapped to existing C# types (terminate traversal, not emitted)
/// - Owned nodes: schemas owned by another model (propagate deps, but not independently emitted)
/// - Op-owned nodes: schemas owned by operation rendering (Response-suffixed, emitted by op loop)
/// </summary>
public sealed class ReferenceGraph
{
    // Adjacency list: source schema ID -> set of target schema IDs it references
    private readonly Dictionary<string, HashSet<string>> _edges =
        new(StringComparer.Ordinal);

    // All registered schema IDs — maintained alongside _edges to provide a live IReadOnlySet view.
    private readonly HashSet<string> _allNodes =
        new(StringComparer.Ordinal);

    // Schema IDs that are external leaves (mapped to existing C# types)
    private readonly HashSet<string> _externalLeaves =
        new(StringComparer.Ordinal);

    // Schema IDs that are generation roots (operations, explicit public models)
    private readonly HashSet<string> _roots =
        new(StringComparer.Ordinal);

    // Schema IDs owned by another generated model (for example, union variant bodies).
    // Owned nodes propagate dependencies but are not independently emitted.
    private readonly HashSet<string> _ownedNodes =
        new(StringComparer.Ordinal);
    private readonly Dictionary<string, string> _ownersByNode =
        new(StringComparer.Ordinal);

    // Schema IDs owned by operation rendering (Response-suffixed schemas)
    // These are emitted by the operation loop, not the namespace scan
    private readonly HashSet<string> _opOwnedNodes =
        new(StringComparer.Ordinal);

    /// <summary>All registered schema IDs (nodes in the graph).</summary>
    public IReadOnlySet<string> AllNodes => _allNodes;

    /// <summary>All root schema IDs (entry points for reachability).</summary>
    public IReadOnlySet<string> Roots => _roots;

    /// <summary>All external leaf schema IDs (mapped to existing types).</summary>
    public IReadOnlySet<string> ExternalLeaves => _externalLeaves;

    /// <summary>All owned node schema IDs (union variant bodies, etc.).</summary>
    public IReadOnlySet<string> OwnedNodes => _ownedNodes;

    /// <summary>All operation-owned node schema IDs (Response-suffixed).</summary>
    public IReadOnlySet<string> OpOwnedNodes => _opOwnedNodes;

    /// <summary>
    /// Registers a schema as a node in the graph. Must be called before adding edges from it.
    /// </summary>
    public void RegisterNode(string schemaId)
    {
        ArgumentException.ThrowIfNullOrEmpty(schemaId);
        _allNodes.Add(schemaId);
        _edges.TryAdd(schemaId, new HashSet<string>(StringComparer.Ordinal));
    }

    /// <summary>
    /// Marks a schema ID as a generation root (operation request/response, explicit public model).
    /// Roots are the starting points for reachability analysis.
    /// </summary>
    public void MarkAsRoot(string schemaId)
    {
        ArgumentException.ThrowIfNullOrEmpty(schemaId);
        _roots.Add(schemaId);
        RegisterNode(schemaId);
    }

    /// <summary>
    /// Marks a schema ID as an external leaf (mapped to an existing C# type).
    /// External leaves terminate dependency chains and are not emitted.
    /// </summary>
    public void MarkAsExternalLeaf(string schemaId)
    {
        ArgumentException.ThrowIfNullOrEmpty(schemaId);
        _externalLeaves.Add(schemaId);
        RegisterNode(schemaId);
    }

    /// <summary>
    /// Marks a generated model as an explicit public root. This overrides an ordinary mapped-type
    /// leaf for models whose mapping exists only to project references to their generated interface.
    /// </summary>
    public void MarkAsExplicitRoot(string schemaId)
    {
        ArgumentException.ThrowIfNullOrEmpty(schemaId);
        _externalLeaves.Remove(schemaId);
        MarkAsRoot(schemaId);
    }

    /// <summary>
    /// Records that <paramref name="ownedSchemaId"/> is rendered as part of
    /// <paramref name="ownerSchemaId"/>. The ownership edge participates in traversal, while the
    /// owned node is excluded from independent emission.
    /// </summary>
    public void MarkAsOwned(string ownerSchemaId, string ownedSchemaId)
    {
        ArgumentException.ThrowIfNullOrEmpty(ownerSchemaId);
        ArgumentException.ThrowIfNullOrEmpty(ownedSchemaId);
        _ownedNodes.Add(ownedSchemaId);
        _ownersByNode[ownedSchemaId] = ownerSchemaId;
        AddEdge(ownerSchemaId, ownedSchemaId);
    }

    public string? GetOwner(string schemaId) =>
        _ownersByNode.TryGetValue(schemaId, out var owner) ? owner : null;

    /// <summary>
    /// Marks a schema ID as operation-owned (Response-suffixed schema emitted by op loop).
    /// Op-owned nodes are not emitted by the namespace scan.
    /// </summary>
    public void MarkAsOpOwned(string schemaId)
    {
        ArgumentException.ThrowIfNullOrEmpty(schemaId);
        _opOwnedNodes.Add(schemaId);
        RegisterNode(schemaId);
    }

    /// <summary>
    /// Adds a directed edge from source to target, indicating source references target.
    /// Automatically registers both nodes.
    /// </summary>
    public void AddEdge(string sourceSchemaId, string targetSchemaId)
    {
        ArgumentException.ThrowIfNullOrEmpty(sourceSchemaId);
        ArgumentException.ThrowIfNullOrEmpty(targetSchemaId);

        RegisterNode(sourceSchemaId);
        RegisterNode(targetSchemaId);

        _edges[sourceSchemaId].Add(targetSchemaId);
    }

    /// <summary>
    /// Gets all schema IDs directly referenced by the given source schema.
    /// </summary>
    public IReadOnlySet<string> GetDependencies(string sourceSchemaId)
    {
        ArgumentException.ThrowIfNullOrEmpty(sourceSchemaId);
        return _edges.TryGetValue(sourceSchemaId, out var deps)
            ? deps
            : new HashSet<string>(StringComparer.Ordinal);
    }

    /// <summary>
    /// Computes all schema IDs reachable from the registered roots via BFS traversal.
    /// External leaves are included in the result but do not propagate further.
    /// Owned nodes DO propagate their dependencies (they're part of the type graph).
    /// Returns schema IDs in deterministic (sorted) order.
    /// </summary>
    public IReadOnlyList<string> ComputeReachable()
    {
        var visited = new HashSet<string>(StringComparer.Ordinal);
        var queue = new Queue<string>();

        // Seed with all roots
        foreach (var root in _roots.OrderBy(r => r, StringComparer.Ordinal))
        {
            if (visited.Add(root))
                queue.Enqueue(root);
        }

        // BFS traversal
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            // External leaves don't propagate dependencies
            if (_externalLeaves.Contains(current))
                continue;

            // Owned nodes DO propagate dependencies (their deps are part of the owner's type graph)
            // But they are not independently emitted (handled in ComputeEmittable)

            if (!_edges.TryGetValue(current, out var deps))
                continue;

            foreach (var dep in deps.OrderBy(d => d, StringComparer.Ordinal))
            {
                if (visited.Add(dep))
                    queue.Enqueue(dep);
            }
        }

        return visited.OrderBy(x => x, StringComparer.Ordinal).ToList();
    }

    /// <summary>
    /// Computes the set of schema IDs that are reachable from roots and should be emitted.
    /// Excludes:
    /// - External leaves (mapped to existing types)
    /// - Owned nodes (emitted as part of their owner)
    /// - Op-owned nodes (emitted by the operation loop)
    /// Returns schema IDs in deterministic (sorted) order.
    /// </summary>
    public IReadOnlyList<string> ComputeEmittable()
    {
        var reachable = ComputeReachable();
        return reachable
            .Where(id => !_externalLeaves.Contains(id))
            .Where(id => !_ownedNodes.Contains(id))
            .Where(id => !_opOwnedNodes.Contains(id))
            .OrderBy(x => x, StringComparer.Ordinal)
            .ToList();
    }

    /// <summary>
    /// Detects cycles in the graph starting from the given schema ID.
    /// Returns the cycle path if found, or null if no cycle.
    /// Used for diagnostic purposes.
    /// </summary>
    public IReadOnlyList<string>? DetectCycle(string startSchemaId)
    {
        ArgumentException.ThrowIfNullOrEmpty(startSchemaId);

        var visited = new HashSet<string>(StringComparer.Ordinal);
        var path = new List<string>();

        return DfsCycleDetect(startSchemaId, visited, path) ? path : null;
    }

    private bool DfsCycleDetect(string current, HashSet<string> visited, List<string> path)
    {
        if (path.Contains(current))
        {
            path.Add(current);
            return true;
        }

        if (!visited.Add(current))
            return false;

        path.Add(current);

        if (_edges.TryGetValue(current, out var deps))
        {
            foreach (var dep in deps.OrderBy(d => d, StringComparer.Ordinal))
            {
                // External leaves don't propagate
                if (_externalLeaves.Contains(dep))
                    continue;

                if (DfsCycleDetect(dep, visited, path))
                    return true;
            }
        }

        path.RemoveAt(path.Count - 1);
        return false;
    }

    /// <summary>
    /// Returns diagnostic information about unreachable schemas (schemas registered but not
    /// reachable from any root). Useful for detecting orphaned definitions.
    /// </summary>
    public IReadOnlyList<string> GetUnreachableSchemas()
    {
        var reachable = new HashSet<string>(ComputeReachable(), StringComparer.Ordinal);
        return _edges.Keys
            .Where(id => !reachable.Contains(id))
            .OrderBy(x => x, StringComparer.Ordinal)
            .ToList();
    }
}
