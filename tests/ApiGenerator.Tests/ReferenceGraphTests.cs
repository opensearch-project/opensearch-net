/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiGenerator.Configuration.Overrides;
using ApiGenerator.Domain.Code.HighLevel.Models;
using ApiGenerator.Generator;
using NSwag;
using Xunit;

namespace ApiGenerator.Tests;

/// <summary>
/// Tests for Phase 4: Reference Graph and Reachability.
/// Validates:
/// - Declaration-order invariance
/// - No duplicate emitted models
/// - Mapped types act as external leaves
/// - Recursive references terminate
/// - Every emitted model is reachable from a root
/// </summary>
public class ReferenceGraphTests
{
    // ────────────────────────────────────────────────────────────────────────────
    // ReferenceGraph Unit Tests
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public void EmptyGraph_ReturnsEmptyReachable()
    {
        var graph = new ReferenceGraph();
        Assert.Empty(graph.ComputeReachable());
        Assert.Empty(graph.ComputeEmittable());
    }

    [Fact]
    public void SingleRoot_IsReachable()
    {
        var graph = new ReferenceGraph();
        graph.MarkAsRoot("test.Model");

        var reachable = graph.ComputeReachable();
        Assert.Single(reachable);
        Assert.Equal("test.Model", reachable[0]);
    }

    [Fact]
    public void RootWithDependency_BothReachable()
    {
        var graph = new ReferenceGraph();
        graph.MarkAsRoot("test.Root");
        graph.AddEdge("test.Root", "test.Dependency");

        var reachable = graph.ComputeReachable();
        Assert.Equal(2, reachable.Count);
        Assert.Contains("test.Root", reachable);
        Assert.Contains("test.Dependency", reachable);
    }

    [Fact]
    public void TransitiveDependencies_AllReachable()
    {
        var graph = new ReferenceGraph();
        graph.MarkAsRoot("test.A");
        graph.AddEdge("test.A", "test.B");
        graph.AddEdge("test.B", "test.C");
        graph.AddEdge("test.C", "test.D");

        var reachable = graph.ComputeReachable();
        Assert.Equal(4, reachable.Count);
        Assert.Equal(new[] { "test.A", "test.B", "test.C", "test.D" }, reachable);
    }

    [Fact]
    public void ExternalLeaf_IncludedButDoesNotPropagate()
    {
        var graph = new ReferenceGraph();
        graph.MarkAsRoot("test.Root");
        graph.AddEdge("test.Root", "test.External");
        graph.MarkAsExternalLeaf("test.External");
        graph.AddEdge("test.External", "test.NotReachable");

        var reachable = graph.ComputeReachable();
        Assert.Equal(2, reachable.Count);
        Assert.Contains("test.Root", reachable);
        Assert.Contains("test.External", reachable);
        Assert.DoesNotContain("test.NotReachable", reachable);
    }

    [Fact]
    public void ExternalLeaf_ExcludedFromEmittable()
    {
        var graph = new ReferenceGraph();
        graph.MarkAsRoot("test.Root");
        graph.AddEdge("test.Root", "test.External");
        graph.MarkAsExternalLeaf("test.External");

        var emittable = graph.ComputeEmittable();
        Assert.Single(emittable);
        Assert.Equal("test.Root", emittable[0]);
    }

    [Fact]
    public void UnreachableNode_NotInReachable()
    {
        var graph = new ReferenceGraph();
        graph.MarkAsRoot("test.Root");
        graph.RegisterNode("test.Orphan");

        var reachable = graph.ComputeReachable();
        Assert.Single(reachable);
        Assert.Equal("test.Root", reachable[0]);
    }

    [Fact]
    public void UnreachableSchemas_Detected()
    {
        var graph = new ReferenceGraph();
        graph.MarkAsRoot("test.Root");
        graph.RegisterNode("test.Orphan1");
        graph.RegisterNode("test.Orphan2");

        var unreachable = graph.GetUnreachableSchemas();
        Assert.Equal(2, unreachable.Count);
        Assert.Contains("test.Orphan1", unreachable);
        Assert.Contains("test.Orphan2", unreachable);
    }

    [Fact]
    public void CyclicReference_Terminates()
    {
        var graph = new ReferenceGraph();
        graph.MarkAsRoot("test.A");
        graph.AddEdge("test.A", "test.B");
        graph.AddEdge("test.B", "test.C");
        graph.AddEdge("test.C", "test.A"); // cycle back to A

        var reachable = graph.ComputeReachable();
        Assert.Equal(3, reachable.Count);
        Assert.Contains("test.A", reachable);
        Assert.Contains("test.B", reachable);
        Assert.Contains("test.C", reachable);
    }

    [Fact]
    public void SelfReference_Terminates()
    {
        var graph = new ReferenceGraph();
        graph.MarkAsRoot("test.SelfRef");
        graph.AddEdge("test.SelfRef", "test.SelfRef");

        var reachable = graph.ComputeReachable();
        Assert.Single(reachable);
        Assert.Equal("test.SelfRef", reachable[0]);
    }

    [Fact]
    public void DeterministicOrder_Maintained()
    {
        var graph = new ReferenceGraph();
        // Add in non-sorted order
        graph.MarkAsRoot("test.Z");
        graph.MarkAsRoot("test.A");
        graph.MarkAsRoot("test.M");

        var reachable = graph.ComputeReachable();
        Assert.Equal(new[] { "test.A", "test.M", "test.Z" }, reachable);
    }

    [Fact]
    public void MultipleRoots_AllReachable()
    {
        var graph = new ReferenceGraph();
        graph.MarkAsRoot("test.Root1");
        graph.MarkAsRoot("test.Root2");
        graph.AddEdge("test.Root1", "test.Shared");
        graph.AddEdge("test.Root2", "test.Shared");

        var reachable = graph.ComputeReachable();
        Assert.Equal(3, reachable.Count);
        Assert.Contains("test.Root1", reachable);
        Assert.Contains("test.Root2", reachable);
        Assert.Contains("test.Shared", reachable);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // Declaration Order Invariance Tests
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task DeclarationOrderInvariance_SchemasInDifferentOrder_SameOutput()
    {
        // Schema A references B, declared A before B
        var specAFirst = """
        {
          "openapi": "3.0.1",
          "info": { "title": "Order Test", "version": "1.0.0" },
          "paths": {
            "/test": {
              "post": {
                "x-operation-group": "test.create",
                "requestBody": {
                  "content": { "application/json": { "schema": { "$ref": "#/components/schemas/test._common___ModelA" } } }
                },
                "responses": { "200": { "content": { "application/json": { "schema": { "type": "object" } } } } }
              }
            }
          },
          "components": {
            "schemas": {
              "test._common___ModelA": {
                "type": "object",
                "properties": { "b": { "$ref": "#/components/schemas/test._common___ModelB" } }
              },
              "test._common___ModelB": {
                "type": "object",
                "properties": { "value": { "type": "string" } }
              }
            }
          }
        }
        """;

        // Same schemas but B declared before A
        var specBFirst = """
        {
          "openapi": "3.0.1",
          "info": { "title": "Order Test", "version": "1.0.0" },
          "paths": {
            "/test": {
              "post": {
                "x-operation-group": "test.create",
                "requestBody": {
                  "content": { "application/json": { "schema": { "$ref": "#/components/schemas/test._common___ModelA" } } }
                },
                "responses": { "200": { "content": { "application/json": { "schema": { "type": "object" } } } } }
              }
            }
          },
          "components": {
            "schemas": {
              "test._common___ModelB": {
                "type": "object",
                "properties": { "value": { "type": "string" } }
              },
              "test._common___ModelA": {
                "type": "object",
                "properties": { "b": { "$ref": "#/components/schemas/test._common___ModelB" } }
              }
            }
          }
        }
        """;

        var docAFirst = await OpenApiDocument.FromJsonAsync(specAFirst);
        var docBFirst = await OpenApiDocument.FromJsonAsync(specBFirst);

        var plugin = new TestModelOverrides { NamespaceValue = "test" };
        var resolverA = new ModelTypeResolver(plugin, new SchemaCatalog(docAFirst));
        var resolverB = new ModelTypeResolver(plugin, new SchemaCatalog(docBFirst));

        var modelA = NamespaceModel.Build(docAFirst, "test", plugin, resolverA);
        var modelB = NamespaceModel.Build(docBFirst, "test", plugin, resolverB);

        // Both should emit the same types in the same order
        Assert.Equal(
            modelA.TypesToEmit.Select(t => t.SchemaId).ToList(),
            modelB.TypesToEmit.Select(t => t.SchemaId).ToList());
    }

    // ────────────────────────────────────────────────────────────────────────────
    // No Duplicate Model Tests
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task NoDuplicates_UnionBodyNotEmittedSeparately()
    {
        var spec = """
        {
          "openapi": "3.0.1",
          "info": { "title": "Union Test", "version": "1.0.0" },
          "paths": {
            "/test": {
              "post": {
                "x-operation-group": "test.create",
                "requestBody": {
                  "content": { "application/json": { "schema": { "$ref": "#/components/schemas/test._common___ProcessorUnion" } } }
                },
                "responses": { "200": { "content": { "application/json": { "schema": { "type": "object" } } } } }
              }
            }
          },
          "components": {
            "schemas": {
              "test._common___ProcessorUnion": {
                "oneOf": [
                  {
                    "type": "object",
                    "properties": { "filter": { "$ref": "#/components/schemas/test._common___FilterProcessor" } },
                    "required": ["filter"]
                  }
                ]
              },
              "test._common___FilterProcessor": {
                "type": "object",
                "properties": { "query": { "type": "string" } }
              }
            }
          }
        }
        """;

        var doc = await OpenApiDocument.FromJsonAsync(spec);
        var plugin = new TestModelOverrides { NamespaceValue = "test" };
        var resolver = new ModelTypeResolver(plugin, new SchemaCatalog(doc));

        var model = NamespaceModel.Build(doc, "test", plugin, resolver);

        // FilterProcessor should be consumed by the union, not emitted separately
        var schemaIds = model.TypesToEmit.Select(t => t.SchemaId).ToList();
        var distinctIds = schemaIds.Distinct().ToList();
        Assert.Equal(distinctIds.Count, schemaIds.Count); // No duplicates

        // Union should be emitted, body should not
        Assert.Contains("test._common___ProcessorUnion", schemaIds);
        Assert.DoesNotContain("test._common___FilterProcessor", schemaIds);

        // Body should be marked as owned in the graph
        Assert.Contains("test._common___FilterProcessor", model.Graph!.OwnedNodes);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // Mapped Types as External Leaves Tests
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task MappedType_ActsAsExternalLeaf()
    {
        var spec = """
        {
          "openapi": "3.0.1",
          "info": { "title": "Mapped Type Test", "version": "1.0.0" },
          "paths": {
            "/test": {
              "post": {
                "x-operation-group": "test.create",
                "requestBody": {
                  "content": { "application/json": { "schema": { "$ref": "#/components/schemas/test._common___Container" } } }
                },
                "responses": { "200": { "content": { "application/json": { "schema": { "type": "object" } } } } }
              }
            }
          },
          "components": {
            "schemas": {
              "test._common___Container": {
                "type": "object",
                "properties": {
                  "id": { "$ref": "#/components/schemas/test._common___Id" }
                }
              },
              "test._common___Id": {
                "type": "string",
                "description": "Mapped to existing Id type"
              }
            }
          }
        }
        """;

        var doc = await OpenApiDocument.FromJsonAsync(spec);
        var plugin = new TestModelOverrides
        {
            NamespaceValue = "test",
            MappedTypesValue = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["test._common___Id"] = "Id"
            }
        };
        var resolver = new ModelTypeResolver(plugin, new SchemaCatalog(doc));

        var model = NamespaceModel.Build(doc, "test", plugin, resolver);

        // Container should be emitted, but Id should not (it's mapped)
        Assert.Single(model.TypesToEmit);
        Assert.Equal("test._common___Container", model.TypesToEmit[0].SchemaId);

        // Id should be marked as external leaf in the graph
        Assert.Contains("test._common___Id", model.Graph!.ExternalLeaves);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // Recursive Reference Termination Tests
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task RecursiveReference_Terminates()
    {
        var spec = """
        {
          "openapi": "3.0.1",
          "info": { "title": "Recursive Test", "version": "1.0.0" },
          "paths": {
            "/test": {
              "post": {
                "x-operation-group": "test.create",
                "requestBody": {
                  "content": { "application/json": { "schema": { "$ref": "#/components/schemas/test._common___TreeNode" } } }
                },
                "responses": { "200": { "content": { "application/json": { "schema": { "type": "object" } } } } }
              }
            }
          },
          "components": {
            "schemas": {
              "test._common___TreeNode": {
                "type": "object",
                "properties": {
                  "value": { "type": "string" },
                  "children": {
                    "type": "array",
                    "items": { "$ref": "#/components/schemas/test._common___TreeNode" }
                  }
                }
              }
            }
          }
        }
        """;

        var doc = await OpenApiDocument.FromJsonAsync(spec);
        var plugin = new TestModelOverrides { NamespaceValue = "test" };
        var resolver = new ModelTypeResolver(plugin, new SchemaCatalog(doc));

        // Should not hang or throw
        var model = NamespaceModel.Build(doc, "test", plugin, resolver);

        Assert.Single(model.TypesToEmit);
        Assert.Equal("test._common___TreeNode", model.TypesToEmit[0].SchemaId);
    }

    [Fact]
    public async Task MutuallyRecursive_Terminates()
    {
        var spec = """
        {
          "openapi": "3.0.1",
          "info": { "title": "Mutual Recursion Test", "version": "1.0.0" },
          "paths": {
            "/test": {
              "post": {
                "x-operation-group": "test.create",
                "requestBody": {
                  "content": { "application/json": { "schema": { "$ref": "#/components/schemas/test._common___Parent" } } }
                },
                "responses": { "200": { "content": { "application/json": { "schema": { "type": "object" } } } } }
              }
            }
          },
          "components": {
            "schemas": {
              "test._common___Parent": {
                "type": "object",
                "properties": {
                  "child": { "$ref": "#/components/schemas/test._common___Child" }
                }
              },
              "test._common___Child": {
                "type": "object",
                "properties": {
                  "parent": { "$ref": "#/components/schemas/test._common___Parent" }
                }
              }
            }
          }
        }
        """;

        var doc = await OpenApiDocument.FromJsonAsync(spec);
        var plugin = new TestModelOverrides { NamespaceValue = "test" };
        var resolver = new ModelTypeResolver(plugin, new SchemaCatalog(doc));

        var model = NamespaceModel.Build(doc, "test", plugin, resolver);

        Assert.Equal(2, model.TypesToEmit.Count);
        Assert.Contains(model.TypesToEmit, t => t.SchemaId == "test._common___Parent");
        Assert.Contains(model.TypesToEmit, t => t.SchemaId == "test._common___Child");
    }

    // ────────────────────────────────────────────────────────────────────────────
    // Reachability from Root Tests
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task OnlyReachableModels_Emitted()
    {
        var spec = """
        {
          "openapi": "3.0.1",
          "info": { "title": "Reachability Test", "version": "1.0.0" },
          "paths": {
            "/test": {
              "post": {
                "x-operation-group": "test.create",
                "requestBody": {
                  "content": { "application/json": { "schema": { "$ref": "#/components/schemas/test._common___Used" } } }
                },
                "responses": { "200": { "content": { "application/json": { "schema": { "type": "object" } } } } }
              }
            }
          },
          "components": {
            "schemas": {
              "test._common___Used": {
                "type": "object",
                "properties": { "name": { "type": "string" } }
              },
              "test._common___Unused": {
                "type": "object",
                "properties": { "data": { "type": "string" } }
              }
            }
          }
        }
        """;

        var doc = await OpenApiDocument.FromJsonAsync(spec);
        var plugin = new TestModelOverrides { NamespaceValue = "test" };
        var resolver = new ModelTypeResolver(plugin, new SchemaCatalog(doc));

        var model = NamespaceModel.Build(doc, "test", plugin, resolver);

        // Only Used should be emitted (it's a root via operation)
        // Unused has no path to it and should not be emitted
        Assert.Single(model.TypesToEmit);
        Assert.Equal("test._common___Used", model.TypesToEmit[0].SchemaId);
    }

    [Fact]
    public async Task TransitivelyReachable_AllEmitted()
    {
        var spec = """
        {
          "openapi": "3.0.1",
          "info": { "title": "Transitive Test", "version": "1.0.0" },
          "paths": {
            "/test": {
              "post": {
                "x-operation-group": "test.create",
                "requestBody": {
                  "content": { "application/json": { "schema": { "$ref": "#/components/schemas/test._common___Root" } } }
                },
                "responses": { "200": { "content": { "application/json": { "schema": { "type": "object" } } } } }
              }
            }
          },
          "components": {
            "schemas": {
              "test._common___Root": {
                "type": "object",
                "properties": { "level1": { "$ref": "#/components/schemas/test._common___Level1" } }
              },
              "test._common___Level1": {
                "type": "object",
                "properties": { "level2": { "$ref": "#/components/schemas/test._common___Level2" } }
              },
              "test._common___Level2": {
                "type": "object",
                "properties": { "value": { "type": "string" } }
              },
              "test._common___Orphan": {
                "type": "object",
                "properties": { "data": { "type": "string" } }
              }
            }
          }
        }
        """;

        var doc = await OpenApiDocument.FromJsonAsync(spec);
        var plugin = new TestModelOverrides { NamespaceValue = "test" };
        var resolver = new ModelTypeResolver(plugin, new SchemaCatalog(doc));

        var model = NamespaceModel.Build(doc, "test", plugin, resolver);

        // Root, Level1, Level2 should all be emitted; Orphan should not
        Assert.Equal(3, model.TypesToEmit.Count);
        Assert.Contains(model.TypesToEmit, t => t.SchemaId == "test._common___Root");
        Assert.Contains(model.TypesToEmit, t => t.SchemaId == "test._common___Level1");
        Assert.Contains(model.TypesToEmit, t => t.SchemaId == "test._common___Level2");
        Assert.DoesNotContain(model.TypesToEmit, t => t.SchemaId == "test._common___Orphan");
    }

    // ────────────────────────────────────────────────────────────────────────────
    // Ownership Semantics Tests
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public void OwnedNode_PropagateDependencies_NotEmitted()
    {
        var graph = new ReferenceGraph();
        graph.MarkAsRoot("test.Union");
        graph.MarkAsOwned("test.Union", "test.OwnedBody");
        graph.AddEdge("test.OwnedBody", "test.BodyDependency");

        Assert.Equal("test.Union", graph.GetOwner("test.OwnedBody"));

        var reachable = graph.ComputeReachable();
        // All three should be reachable (owned node propagates deps)
        Assert.Equal(3, reachable.Count);
        Assert.Contains("test.Union", reachable);
        Assert.Contains("test.OwnedBody", reachable);
        Assert.Contains("test.BodyDependency", reachable);

        var emittable = graph.ComputeEmittable();
        // But owned node is not emittable
        Assert.Equal(2, emittable.Count);
        Assert.Contains("test.Union", emittable);
        Assert.Contains("test.BodyDependency", emittable);
        Assert.DoesNotContain("test.OwnedBody", emittable);
    }

    [Fact]
    public void OpOwnedNode_NotEmittedByNamespaceScan()
    {
        var graph = new ReferenceGraph();
        graph.MarkAsRoot("test.Request");
        graph.MarkAsRoot("test.Response");
        graph.MarkAsOpOwned("test.Response");

        var emittable = graph.ComputeEmittable();
        Assert.Single(emittable);
        Assert.Equal("test.Request", emittable[0]);
    }

    [Fact]
    public async Task OpOwnedResponseSchema_NotDuplicatedByNamespaceScan()
    {
        var spec = """
        {
          "openapi": "3.0.1",
          "info": { "title": "Op-Owned Test", "version": "1.0.0" },
          "paths": {
            "/test": {
              "post": {
                "x-operation-group": "test.create",
                "requestBody": {
                  "content": { "application/json": { "schema": { "$ref": "#/components/schemas/test.CreateRequest" } } }
                },
                "responses": { "200": { "content": { "application/json": { "schema": { "$ref": "#/components/schemas/test.CreateResponse" } } } } }
              }
            }
          },
          "components": {
            "schemas": {
              "test.CreateRequest": {
                "type": "object",
                "properties": { "name": { "type": "string" } }
              },
              "test.CreateResponse": {
                "type": "object",
                "properties": { "id": { "type": "string" } }
              }
            }
          }
        }
        """;

        var doc = await OpenApiDocument.FromJsonAsync(spec);
        var plugin = new TestModelOverrides { NamespaceValue = "test" };
        var resolver = new ModelTypeResolver(plugin, new SchemaCatalog(doc));

        var model = NamespaceModel.Build(doc, "test", plugin, resolver);

        // Response schema should be op-owned
        Assert.Contains("test.CreateResponse", model.Graph!.OpOwnedNodes);

        // Response schema should NOT be emitted by namespace scan (emitted by op loop instead)
        Assert.Single(model.TypesToEmit);
        Assert.Equal("test.CreateRequest", model.TypesToEmit[0].SchemaId);
    }

    [Fact]
    public async Task ExplicitPublicSchema_IsEmittedAsReachableRoot()
    {
        var doc = await OpenApiDocument.FromJsonAsync("""
            {
              "openapi": "3.0.1",
              "info": { "title": "Explicit root", "version": "1.0.0" },
              "paths": {},
              "components": {
                "schemas": {
                  "test.PublicModel": {
                    "type": "object",
                    "properties": { "name": { "type": "string" } }
                  },
                  "test.Orphan": {
                    "type": "object",
                    "properties": { "value": { "type": "string" } }
                  }
                }
              }
            }
            """);
        var plugin = new TestModelOverrides
        {
            ExplicitlyPublicSchemaIdsValue = new HashSet<string>(StringComparer.Ordinal)
            {
                "test.PublicModel",
            },
        };
        var model = NamespaceModel.Build(
            doc,
            "test",
            plugin,
            new ModelTypeResolver(plugin, new SchemaCatalog(doc)));

        var emitted = Assert.Single(model.TypesToEmit);
        Assert.Equal("test.PublicModel", emitted.SchemaId);
        Assert.Contains(emitted.SchemaId, model.Graph!.Roots);
        Assert.Contains(emitted.SchemaId, model.Graph.ComputeReachable());
        Assert.DoesNotContain(model.TypesToEmit, type => type.SchemaId == "test.Orphan");
    }

    // ────────────────────────────────────────────────────────────────────────────
    // Test Helpers
    // ────────────────────────────────────────────────────────────────────────────

    private sealed class TestModelOverrides : ModelOverridesBase
    {
        public string NamespaceValue { get; init; } = "test";
        public Dictionary<string, string> MappedTypesValue { get; init; } = new(StringComparer.Ordinal);
        public ISet<string> ExplicitlyPublicSchemaIdsValue { get; init; } =
            new HashSet<string>(StringComparer.Ordinal);

        public override string Namespace => NamespaceValue;
        public override string OutputFolder => "Test";
        public override ISet<string> ExplicitlyPublicSchemaIds => ExplicitlyPublicSchemaIdsValue;
        public override IDictionary<string, string> MappedTypes => MappedTypesValue;
    }
}
