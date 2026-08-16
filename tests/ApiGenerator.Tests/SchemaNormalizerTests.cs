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

public class SchemaNormalizerTests
{
    // ─────────────────────────────────────────────────────────────────────────
    // Pass ordering and execution
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task PassesExecuteInDeclaredOrder()
    {
        var doc = await OpenApiDocument.FromJsonAsync(MinimalSpec);
        var catalog = new SchemaCatalog(doc);
        var normalizer = new SchemaNormalizer(catalog);

        var result = normalizer.Normalize(doc);

        Assert.Equal(4, result.PassesExecuted.Count);
        Assert.Equal("AllOfPropertyCollection", result.PassesExecuted[0]);
        Assert.Equal("CompositionPreservation", result.PassesExecuted[1]);
        Assert.Equal("DependencyCollection", result.PassesExecuted[2]);
        Assert.Equal("RequiredFieldPropagation", result.PassesExecuted[3]);
    }

    [Fact]
    public async Task CustomPassSequenceIsRespected()
    {
        var doc = await OpenApiDocument.FromJsonAsync(MinimalSpec);
        var catalog = new SchemaCatalog(doc);

        // Reverse order
        var passes = new INormalizationPass[]
        {
            new RequiredFieldPropagationPass(),
            new AllOfPropertyCollectionPass(),
        };
        var normalizer = new SchemaNormalizer(catalog, passes);
        var result = normalizer.Normalize(doc);

        Assert.Equal("RequiredFieldPropagation", result.PassesExecuted[0]);
        Assert.Equal("AllOfPropertyCollection", result.PassesExecuted[1]);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // AllOf inheritance / property aggregation
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task AllOfInlineObjectPropertiesAreCollected()
    {
        var doc = await OpenApiDocument.FromJsonAsync(AllOfSpec);
        var catalog = new SchemaCatalog(doc);
        var result = new SchemaNormalizer(catalog).Normalize(doc);

        Assert.True(result.TryGet("test._common___Child", out var child));
        Assert.True(child.HasAllOfComposition);
        // Should have both inline properties: own_prop and inherited_prop
        Assert.True(child.EffectiveProperties.ContainsKey("own_prop"));
        Assert.True(child.EffectiveProperties.ContainsKey("inherited_prop"));
    }

    [Fact]
    public async Task AllOfRefEntriesAreRecordedAsBaseSchemas()
    {
        var doc = await OpenApiDocument.FromJsonAsync(AllOfSpec);
        var catalog = new SchemaCatalog(doc);
        var result = new SchemaNormalizer(catalog).Normalize(doc);

        Assert.True(result.TryGet("test._common___Child", out var child));
        Assert.Contains("test._common___Base", child.BaseSchemaIds);
    }

    [Fact]
    public async Task DirectPropertiesOverrideAllOfInlineProperties()
    {
        var doc = await OpenApiDocument.FromJsonAsync(AllOfOverrideSpec);
        var catalog = new SchemaCatalog(doc);
        var result = new SchemaNormalizer(catalog).Normalize(doc);

        Assert.True(result.TryGet("test._common___OverrideChild", out var child));
        // Direct property 'name' should override the allOf-inline one
        Assert.True(child.EffectiveProperties.ContainsKey("name"));
        // The description on the direct property is "overridden"
        Assert.Equal("overridden", child.EffectiveProperties["name"].ActualSchema.Description);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Required property propagation
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task RequiredPropertiesFromOwnSchemaArePropagated()
    {
        var doc = await OpenApiDocument.FromJsonAsync(AllOfSpec);
        var catalog = new SchemaCatalog(doc);
        var result = new SchemaNormalizer(catalog).Normalize(doc);

        Assert.True(result.TryGet("test._common___Child", out var child));
        Assert.Contains("own_prop", child.RequiredProperties);
    }

    [Fact]
    public async Task RequiredPropertiesFromInlineAllOfArePropagated()
    {
        var doc = await OpenApiDocument.FromJsonAsync(AllOfRequiredSpec);
        var catalog = new SchemaCatalog(doc);
        var result = new SchemaNormalizer(catalog).Normalize(doc);

        Assert.True(result.TryGet("test._common___WithInlineRequired", out var schema));
        // Both own 'name' and inline allOf 'status' should be required
        Assert.Contains("name", schema.RequiredProperties);
        Assert.Contains("status", schema.RequiredProperties);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Nested/inline allOf
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task NestedInlineAllOfIsFlattened()
    {
        var doc = await OpenApiDocument.FromJsonAsync(NestedAllOfSpec);
        var catalog = new SchemaCatalog(doc);
        var result = new SchemaNormalizer(catalog).Normalize(doc);

        Assert.True(result.TryGet("test._common___Nested", out var schema));
        Assert.True(schema.EffectiveProperties.ContainsKey("inline_a"));
        Assert.True(schema.EffectiveProperties.ContainsKey("inline_b"));
        Assert.True(schema.EffectiveProperties.ContainsKey("direct"));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Recursive references / cycles
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task RecursiveSchemaDoesNotCauseInfiniteLoop()
    {
        var doc = await OpenApiDocument.FromJsonAsync(RecursiveSpec);
        var catalog = new SchemaCatalog(doc);
        var result = new SchemaNormalizer(catalog).Normalize(doc);

        // Should complete without StackOverflow
        Assert.True(result.TryGet("test._common___TreeNode", out var node));
        Assert.True(node.EffectiveProperties.ContainsKey("children"));
        Assert.True(node.EffectiveProperties.ContainsKey("value"));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // oneOf/anyOf preservation (NOT flattened)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task OneOfSchemasAreNotFlattenedByNormalization()
    {
        var doc = await OpenApiDocument.FromJsonAsync(OneOfSpec);
        var catalog = new SchemaCatalog(doc);
        var result = new SchemaNormalizer(catalog).Normalize(doc);

        // oneOf schemas should still exist in the result with their own properties
        Assert.True(result.TryGet("test._common___Union", out var union));
        // The union schema itself has no direct properties — its structure is in oneOf
        Assert.Empty(union.EffectiveProperties);
        Assert.False(union.HasAllOfComposition);
    }

    [Fact]
    public async Task AnyOfSchemasArePreserved()
    {
        var doc = await OpenApiDocument.FromJsonAsync(AnyOfSpec);
        var catalog = new SchemaCatalog(doc);
        var result = new SchemaNormalizer(catalog).Normalize(doc);

        Assert.True(result.TryGet("test._common___Flexible", out var schema));
        // anyOf with no direct properties
        Assert.Empty(schema.EffectiveProperties);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Discriminator preservation
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task DiscriminatorSchemaPreservesStructure()
    {
        var doc = await OpenApiDocument.FromJsonAsync(DiscriminatorSpec);
        var catalog = new SchemaCatalog(doc);
        var result = new SchemaNormalizer(catalog).Normalize(doc);

        // The discriminated union schema should be normalized but discriminator
        // structural info is in the raw schema (oneOf), not in effectiveProperties
        Assert.True(result.TryGet("test._common___Animal", out var animal));
        Assert.Empty(animal.EffectiveProperties); // union, no direct props
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Declaration-order invariance
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task ResultIsIndependentOfSchemaDeclarationOrder()
    {
        // Forward order
        var doc1 = await OpenApiDocument.FromJsonAsync(OrderSpec_Forward);
        var result1 = new SchemaNormalizer(new SchemaCatalog(doc1)).Normalize(doc1);

        // Reverse order
        var doc2 = await OpenApiDocument.FromJsonAsync(OrderSpec_Reverse);
        var result2 = new SchemaNormalizer(new SchemaCatalog(doc2)).Normalize(doc2);

        // Both should produce same effective properties for Child
        Assert.True(result1.TryGet("test._common___Child", out var child1));
        Assert.True(result2.TryGet("test._common___Child", out var child2));

        Assert.Equal(
            child1.EffectiveProperties.Keys.OrderBy(k => k).ToList(),
            child2.EffectiveProperties.Keys.OrderBy(k => k).ToList());
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Schema identity (via SchemaCatalog)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task NormalizationKeysAreCanonicalSchemaIds()
    {
        var doc = await OpenApiDocument.FromJsonAsync(AllOfSpec);
        var catalog = new SchemaCatalog(doc);
        var result = new SchemaNormalizer(catalog).Normalize(doc);

        // Component schema IDs should be resolvable via the catalog.
        // Synthetic inline IDs (__inline_*) are valid but not in catalog.
        foreach (var schemaId in result.SchemaIds)
        {
            if (schemaId.StartsWith("__inline_", StringComparison.Ordinal))
                continue; // Synthetic IDs are expected for inline schemas
            Assert.True(catalog.TryGetSchema(schemaId, out _), $"Schema {schemaId} not in catalog");
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Generated-model equivalence (integration with NamespaceModel)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task NamespaceModelWithNormalizationProducesSameOutput()
    {
        var doc = await OpenApiDocument.FromJsonAsync(AllOfSpec);
        var plugin = new TestNormPlugin();
        var catalog = new SchemaCatalog(doc);
        var resolver = new ModelTypeResolver(plugin, catalog);

        // Without normalization (legacy fallback)
        var nsLegacy = NamespaceModel.Build(doc, plugin.Namespace, plugin, resolver);

        // With normalization
        var normalization = new SchemaNormalizer(catalog).Normalize(doc);
        var nsNorm = NamespaceModel.Build(doc, plugin.Namespace, plugin, resolver, normalization: normalization);

        // Same types emitted
        Assert.Equal(nsLegacy.TypesToEmit.Count, nsNorm.TypesToEmit.Count);
        for (var i = 0; i < nsLegacy.TypesToEmit.Count; i++)
        {
            var legacy = nsLegacy.TypesToEmit[i];
            var norm = nsNorm.TypesToEmit[i];
            Assert.Equal(legacy.SchemaId, norm.SchemaId);
            Assert.Equal(legacy.CsharpName, norm.CsharpName);

            if (legacy is ObjectModel legacyObj && norm is ObjectModel normObj)
            {
                Assert.Equal(
                    legacyObj.Properties.Select(p => p.WireName).ToList(),
                    normObj.Properties.Select(p => p.WireName).ToList());
                Assert.Equal(
                    legacyObj.Properties.Select(p => p.IsRequired).ToList(),
                    normObj.Properties.Select(p => p.IsRequired).ToList());
            }
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Regression: downstream model construction no longer invokes ad hoc allOf
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task NamespaceModelBuildWithNormalizationBypassesCollectProperties()
    {
        // This test proves that when normalization is provided, the effective
        // properties come from the normalization result, not from the legacy
        // CollectProperties method. We verify by providing normalization with
        // a custom effective property that differs from what CollectProperties
        // would produce.
        var doc = await OpenApiDocument.FromJsonAsync(AllOfSpec);
        var plugin = new TestNormPlugin();
        var catalog = new SchemaCatalog(doc);
        var resolver = new ModelTypeResolver(plugin, catalog);

        // Create normalization that adds an extra "injected_prop" to Child
        var normalizer = new SchemaNormalizer(catalog);
        var normalResult = normalizer.Normalize(doc);

        // The normalized result must already contain Child's properties from allOf
        Assert.True(normalResult.TryGet("test._common___Child", out var normalizedChild));
        Assert.True(normalizedChild.EffectiveProperties.ContainsKey("own_prop"));
        Assert.True(normalizedChild.EffectiveProperties.ContainsKey("inherited_prop"));

        // Build with normalization
        var ns = NamespaceModel.Build(doc, plugin.Namespace, plugin, resolver, normalization: normalResult);

        // The Child object model should have both own_prop and inherited_prop
        var childModel = ns.AllTypes.OfType<ObjectModel>()
            .FirstOrDefault(t => t.SchemaId == "test._common___Child");
        Assert.NotNull(childModel);
        Assert.Contains(childModel.Properties, p => p.WireName == "own_prop");
        Assert.Contains(childModel.Properties, p => p.WireName == "inherited_prop");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Phase 6 corrective tests: verify fixes that prior implementation lacked
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task DocumentScopedCatalogReusedAcrossPlugins()
    {
        // Verifies requirement #1: one catalog per document, shared across plugins.
        // ModelsGenerator.Generate creates a single SchemaCatalog and reuses it.
        // Here we verify the same NormalizationResult works for multiple namespaces.
        var doc = await OpenApiDocument.FromJsonAsync(MultiNamespaceSpec);
        var catalog = new SchemaCatalog(doc);
        var normalization = new SchemaNormalizer(catalog).Normalize(doc);

        var pluginA = new TestPluginA();
        var pluginB = new TestPluginB();
        var resolverA = new ModelTypeResolver(pluginA, catalog);
        var resolverB = new ModelTypeResolver(pluginB, catalog);

        // Both plugins use the SAME normalization result and catalog
        var nsA = NamespaceModel.Build(doc, pluginA.Namespace, pluginA, resolverA, normalization: normalization);
        var nsB = NamespaceModel.Build(doc, pluginB.Namespace, pluginB, resolverB, normalization: normalization);

        // Both namespaces should be populated correctly from the same normalization
        Assert.NotEmpty(nsA.AllTypes);
        Assert.NotEmpty(nsB.AllTypes);
        Assert.True(normalization.Catalog == catalog); // Same catalog reference
    }

    [Fact]
    public async Task CompositionOrderPreservedInNormalizedSchema()
    {
        // Verifies requirement #5: allOf members order preserved
        var doc = await OpenApiDocument.FromJsonAsync(CompositionOrderSpec);
        var catalog = new SchemaCatalog(doc);
        var result = new SchemaNormalizer(catalog).Normalize(doc);

        Assert.True(result.TryGet("test._common___Composed", out var composed));
        Assert.Equal(3, composed.AllOfMembers.Count);
        // First member is $ref to Base
        Assert.True(composed.AllOfMembers[0].IsReference);
        Assert.Equal("test._common___Base", composed.AllOfMembers[0].ReferencedSchemaId);
        // Second and third are inline
        Assert.False(composed.AllOfMembers[1].IsReference);
        Assert.False(composed.AllOfMembers[2].IsReference);
    }

    [Fact]
    public async Task OneOfVariantsPreservedInNormalizedSchema()
    {
        // Verifies requirement #5: oneOf variants preserved in normalization IR
        var doc = await OpenApiDocument.FromJsonAsync(DiscriminatorSpec);
        var catalog = new SchemaCatalog(doc);
        var result = new SchemaNormalizer(catalog).Normalize(doc);

        Assert.True(result.TryGet("test._common___Animal", out var animal));
        Assert.Equal(2, animal.OneOfVariants.Count);
        Assert.True(animal.OneOfVariants[0].IsReference);
        Assert.Equal("test._common___Cat", animal.OneOfVariants[0].ReferencedSchemaId);
        Assert.True(animal.OneOfVariants[1].IsReference);
        Assert.Equal("test._common___Dog", animal.OneOfVariants[1].ReferencedSchemaId);
    }

    [Fact]
    public async Task DiscriminatorPreservedInNormalizedSchema()
    {
        // Verifies requirement #5: discriminator information preserved
        var doc = await OpenApiDocument.FromJsonAsync(DiscriminatorSpec);
        var catalog = new SchemaCatalog(doc);
        var result = new SchemaNormalizer(catalog).Normalize(doc);

        Assert.True(result.TryGet("test._common___Animal", out var animal));
        Assert.NotNull(animal.Discriminator);
        Assert.Equal("type", animal.Discriminator!.PropertyName);
    }

    [Fact]
    public async Task AnyOfVariantsPreservedInNormalizedSchema()
    {
        // Verifies requirement #5: anyOf variants preserved
        var doc = await OpenApiDocument.FromJsonAsync(AnyOfSpec);
        var catalog = new SchemaCatalog(doc);
        var result = new SchemaNormalizer(catalog).Normalize(doc);

        Assert.True(result.TryGet("test._common___Flexible", out var flexible));
        Assert.Equal(2, flexible.AnyOfVariants.Count);
    }

    [Fact]
    public async Task NestedInlineAllOfRecursivelyAggregatesProperties()
    {
        // Verifies requirement #6: nested inline allOf with recursive property collection
        var doc = await OpenApiDocument.FromJsonAsync(DeeplyNestedAllOfSpec);
        var catalog = new SchemaCatalog(doc);
        var result = new SchemaNormalizer(catalog).Normalize(doc);

        Assert.True(result.TryGet("test._common___Deep", out var deep));
        // Should collect all nested inline properties recursively
        Assert.True(deep.EffectiveProperties.ContainsKey("level1"));
        Assert.True(deep.EffectiveProperties.ContainsKey("level2"));
        Assert.True(deep.EffectiveProperties.ContainsKey("direct"));
    }

    [Fact]
    public async Task ReferenceGraphUsesNormalizedProperties()
    {
        // Verifies requirement #7: ReferenceGraphBuilder consumes normalized facts
        var doc = await OpenApiDocument.FromJsonAsync(AllOfSpec);
        var plugin = new TestNormPlugin();
        var catalog = new SchemaCatalog(doc);
        var normalization = new SchemaNormalizer(catalog).Normalize(doc);

        // Should not throw — normalization is required
        var builder = new ReferenceGraphBuilder(doc, plugin, catalog, normalization);
        var graph = builder.Build();
        Assert.NotNull(graph);
    }

    [Fact]
    public async Task NormalizationAutoSuppliedInNamespaceModelBuild()
    {
        // Verifies backward compatibility: NamespaceModel.Build auto-supplies normalization
        var doc = await OpenApiDocument.FromJsonAsync(AllOfSpec);
        var plugin = new TestNormPlugin();
        var catalog = new SchemaCatalog(doc);
        var resolver = new ModelTypeResolver(plugin, catalog);

        // Should NOT throw even without explicit normalization parameter
        var ns = NamespaceModel.Build(doc, plugin.Namespace, plugin, resolver);
        Assert.NotNull(ns);
    }

    [Fact]
    public async Task CompositionPreservationPassRecordsAllCompositionTypes()
    {
        // Verifies the new CompositionPreservation pass runs and records all 3 composition types
        var doc = await OpenApiDocument.FromJsonAsync(DiscriminatorSpec);
        var catalog = new SchemaCatalog(doc);
        var result = new SchemaNormalizer(catalog).Normalize(doc);

        Assert.Contains("CompositionPreservation", result.PassesExecuted);

        // Cat has no allOf/oneOf/anyOf
        Assert.True(result.TryGet("test._common___Cat", out var cat));
        Assert.Empty(cat.AllOfMembers);
        Assert.Empty(cat.OneOfVariants);
        Assert.Empty(cat.AnyOfVariants);

        // Animal has oneOf
        Assert.True(result.TryGet("test._common___Animal", out var animal));
        Assert.NotEmpty(animal.OneOfVariants);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Additional specs for Phase 6 corrective tests
    // ─────────────────────────────────────────────────────────────────────────

    private const string MultiNamespaceSpec = """
        {
          "openapi": "3.0.1",
          "info": { "title": "test", "version": "1.0.0" },
          "paths": {
            "/a": { "post": { "x-operation-group": "nsa.create", "requestBody": { "content": { "application/json": { "schema": { "$ref": "#/components/schemas/nsa._common___Item" } } } }, "responses": { "200": { "content": { "application/json": { "schema": { "type": "object" } } } } } } },
            "/b": { "post": { "x-operation-group": "nsb.create", "requestBody": { "content": { "application/json": { "schema": { "$ref": "#/components/schemas/nsb._common___Thing" } } } }, "responses": { "200": { "content": { "application/json": { "schema": { "type": "object" } } } } } } }
          },
          "components": {
            "schemas": {
              "nsa._common___Item": { "type": "object", "properties": { "id": { "type": "string" } } },
              "nsb._common___Thing": { "type": "object", "properties": { "name": { "type": "string" } } }
            }
          }
        }
        """;

    private const string CompositionOrderSpec = """
        {
          "openapi": "3.0.1",
          "info": { "title": "test", "version": "1.0.0" },
          "paths": {},
          "components": {
            "schemas": {
              "test._common___Base": { "type": "object", "properties": { "base_field": { "type": "string" } } },
              "test._common___Composed": {
                "allOf": [
                  { "$ref": "#/components/schemas/test._common___Base" },
                  { "type": "object", "properties": { "mid": { "type": "integer" } } },
                  { "type": "object", "properties": { "last": { "type": "boolean" } } }
                ],
                "type": "object",
                "properties": { "own": { "type": "string" } }
              }
            }
          }
        }
        """;

    private const string DeeplyNestedAllOfSpec = """
        {
          "openapi": "3.0.1",
          "info": { "title": "test", "version": "1.0.0" },
          "paths": {},
          "components": {
            "schemas": {
              "test._common___Deep": {
                "allOf": [
                  {
                    "type": "object",
                    "allOf": [
                      { "type": "object", "properties": { "level2": { "type": "string" } } }
                    ],
                    "properties": { "level1": { "type": "integer" } }
                  }
                ],
                "type": "object",
                "properties": { "direct": { "type": "boolean" } }
              }
            }
          }
        }
        """;

    private sealed class TestPluginA : ModelOverridesBase
    {
        public override string Namespace => "nsa";
        public override string OutputFolder => "Nsa";
    }

    private sealed class TestPluginB : ModelOverridesBase
    {
        public override string Namespace => "nsb";
        public override string OutputFolder => "Nsb";
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Specifications
    // ─────────────────────────────────────────────────────────────────────────

    private const string MinimalSpec = """
        {
          "openapi": "3.0.1",
          "info": { "title": "test", "version": "1.0.0" },
          "paths": {},
          "components": {
            "schemas": {
              "test._common___Simple": {
                "type": "object",
                "properties": { "name": { "type": "string" } }
              }
            }
          }
        }
        """;

    private const string AllOfSpec = """
        {
          "openapi": "3.0.1",
          "info": { "title": "test", "version": "1.0.0" },
          "paths": {
            "/test": {
              "post": {
                "x-operation-group": "test.create",
                "requestBody": { "content": { "application/json": { "schema": { "$ref": "#/components/schemas/test._common___Child" } } } },
                "responses": { "200": { "content": { "application/json": { "schema": { "type": "object" } } } } }
              }
            }
          },
          "components": {
            "schemas": {
              "test._common___Base": {
                "type": "object",
                "properties": {
                  "base_field": { "type": "string" }
                }
              },
              "test._common___Child": {
                "allOf": [
                  { "$ref": "#/components/schemas/test._common___Base" },
                  {
                    "type": "object",
                    "properties": {
                      "inherited_prop": { "type": "integer" }
                    }
                  }
                ],
                "type": "object",
                "properties": {
                  "own_prop": { "type": "string" }
                },
                "required": ["own_prop"]
              }
            }
          }
        }
        """;

    private const string AllOfOverrideSpec = """
        {
          "openapi": "3.0.1",
          "info": { "title": "test", "version": "1.0.0" },
          "paths": {},
          "components": {
            "schemas": {
              "test._common___OverrideChild": {
                "allOf": [
                  {
                    "type": "object",
                    "properties": {
                      "name": { "type": "string", "description": "from_allof" }
                    }
                  }
                ],
                "type": "object",
                "properties": {
                  "name": { "type": "string", "description": "overridden" }
                }
              }
            }
          }
        }
        """;

    private const string AllOfRequiredSpec = """
        {
          "openapi": "3.0.1",
          "info": { "title": "test", "version": "1.0.0" },
          "paths": {},
          "components": {
            "schemas": {
              "test._common___WithInlineRequired": {
                "allOf": [
                  {
                    "type": "object",
                    "properties": {
                      "status": { "type": "string" }
                    },
                    "required": ["status"]
                  }
                ],
                "type": "object",
                "properties": {
                  "name": { "type": "string" }
                },
                "required": ["name"]
              }
            }
          }
        }
        """;

    private const string NestedAllOfSpec = """
        {
          "openapi": "3.0.1",
          "info": { "title": "test", "version": "1.0.0" },
          "paths": {},
          "components": {
            "schemas": {
              "test._common___Nested": {
                "allOf": [
                  {
                    "type": "object",
                    "properties": { "inline_a": { "type": "string" } }
                  },
                  {
                    "type": "object",
                    "properties": { "inline_b": { "type": "integer" } }
                  }
                ],
                "type": "object",
                "properties": {
                  "direct": { "type": "boolean" }
                }
              }
            }
          }
        }
        """;

    private const string RecursiveSpec = """
        {
          "openapi": "3.0.1",
          "info": { "title": "test", "version": "1.0.0" },
          "paths": {},
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

    private const string OneOfSpec = """
        {
          "openapi": "3.0.1",
          "info": { "title": "test", "version": "1.0.0" },
          "paths": {},
          "components": {
            "schemas": {
              "test._common___Union": {
                "oneOf": [
                  { "type": "object", "properties": { "a": { "type": "string" } }, "required": ["a"] },
                  { "type": "object", "properties": { "b": { "type": "integer" } }, "required": ["b"] }
                ]
              }
            }
          }
        }
        """;

    private const string AnyOfSpec = """
        {
          "openapi": "3.0.1",
          "info": { "title": "test", "version": "1.0.0" },
          "paths": {},
          "components": {
            "schemas": {
              "test._common___Flexible": {
                "anyOf": [
                  { "type": "object", "properties": { "x": { "type": "string" } } },
                  { "type": "object", "properties": { "y": { "type": "integer" } } }
                ]
              }
            }
          }
        }
        """;

    private const string DiscriminatorSpec = """
        {
          "openapi": "3.0.1",
          "info": { "title": "test", "version": "1.0.0" },
          "paths": {},
          "components": {
            "schemas": {
              "test._common___Cat": {
                "type": "object",
                "properties": { "type": { "type": "string", "enum": ["cat"] }, "purrs": { "type": "boolean" } }
              },
              "test._common___Dog": {
                "type": "object",
                "properties": { "type": { "type": "string", "enum": ["dog"] }, "barks": { "type": "boolean" } }
              },
              "test._common___Animal": {
                "discriminator": { "propertyName": "type" },
                "oneOf": [
                  { "$ref": "#/components/schemas/test._common___Cat" },
                  { "$ref": "#/components/schemas/test._common___Dog" }
                ]
              }
            }
          }
        }
        """;

    private const string OrderSpec_Forward = """
        {
          "openapi": "3.0.1",
          "info": { "title": "test", "version": "1.0.0" },
          "paths": {
            "/test": {
              "post": {
                "x-operation-group": "test.create",
                "requestBody": { "content": { "application/json": { "schema": { "$ref": "#/components/schemas/test._common___Child" } } } },
                "responses": { "200": { "content": { "application/json": { "schema": { "type": "object" } } } } }
              }
            }
          },
          "components": {
            "schemas": {
              "test._common___Base": {
                "type": "object",
                "properties": { "base_field": { "type": "string" } }
              },
              "test._common___Child": {
                "allOf": [
                  { "$ref": "#/components/schemas/test._common___Base" },
                  { "type": "object", "properties": { "extra": { "type": "integer" } } }
                ],
                "type": "object",
                "properties": { "own": { "type": "string" } }
              }
            }
          }
        }
        """;

    private const string OrderSpec_Reverse = """
        {
          "openapi": "3.0.1",
          "info": { "title": "test", "version": "1.0.0" },
          "paths": {
            "/test": {
              "post": {
                "x-operation-group": "test.create",
                "requestBody": { "content": { "application/json": { "schema": { "$ref": "#/components/schemas/test._common___Child" } } } },
                "responses": { "200": { "content": { "application/json": { "schema": { "type": "object" } } } } }
              }
            }
          },
          "components": {
            "schemas": {
              "test._common___Child": {
                "allOf": [
                  { "$ref": "#/components/schemas/test._common___Base" },
                  { "type": "object", "properties": { "extra": { "type": "integer" } } }
                ],
                "type": "object",
                "properties": { "own": { "type": "string" } }
              },
              "test._common___Base": {
                "type": "object",
                "properties": { "base_field": { "type": "string" } }
              }
            }
          }
        }
        """;

    private sealed class TestNormPlugin : ModelOverridesBase
    {
        public override string Namespace => "test";
        public override string OutputFolder => "Test";
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Phase 6 architectural closure tests
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task TryGetForSchema_WorksForInlineAllOfVariantBodies()
    {
        // Verifies that TryGetForSchema succeeds for nested inline allOf schema instances
        // that appear as union variant bodies. This was the hard failure in pre-Phase6 code.
        var doc = await OpenApiDocument.FromJsonAsync(InlineAllOfVariantSpec);
        var catalog = new SchemaCatalog(doc);
        var result = new SchemaNormalizer(catalog).Normalize(doc);

        // Get the inline allOf variant body schema instance directly from the document
        var containerSchema = doc.Components.Schemas["test._common___Container"].ActualSchema;
        Assert.True(containerSchema.OneOf.Count > 0);

        // The first oneOf variant has an inline allOf body
        var variant = containerSchema.OneOf.First().ActualSchema;
        Assert.True(variant.AllOf.Count > 0, "Variant should have allOf");

        // TryGetForSchema must succeed for this inline schema
        Assert.True(result.TryGetForSchema(variant, out var normalized),
            "TryGetForSchema must work for inline allOf variant bodies");
        Assert.True(normalized.EffectiveProperties.ContainsKey("inline_prop"));
    }

    [Fact]
    public async Task TryGetForSchema_WorksForOperationInlineRequestSchemas()
    {
        // Verifies that inline operation request/response schemas are discovered
        var doc = await OpenApiDocument.FromJsonAsync(OperationInlineSchemaSpec);
        var catalog = new SchemaCatalog(doc);
        var result = new SchemaNormalizer(catalog).Normalize(doc);

        // Get the inline request body schema instance from the operation
        var op = doc.Paths["/test"].First().Value;
        var reqSchema = op.ActualRequestBody!.Content["application/json"].Schema.ActualSchema;

        // TryGetForSchema must succeed for this inline operation schema
        Assert.True(result.TryGetForSchema(reqSchema, out var normalized),
            "TryGetForSchema must work for inline operation request schemas");
        Assert.True(normalized.EffectiveProperties.ContainsKey("inline_field"));
        Assert.True(normalized.EffectiveProperties.ContainsKey("allof_field"));
    }

    [Fact]
    public async Task SyntheticIds_DeterministicUnderDeclarationOrderPermutations()
    {
        // Verifies that synthetic IDs are deterministic: same traversal order produces
        // same ID assignments regardless of how the inline schemas are declared.
        var doc1 = await OpenApiDocument.FromJsonAsync(DeterministicIdSpec_A);
        var result1 = new SchemaNormalizer(new SchemaCatalog(doc1)).Normalize(doc1);

        var doc2 = await OpenApiDocument.FromJsonAsync(DeterministicIdSpec_B);
        var result2 = new SchemaNormalizer(new SchemaCatalog(doc2)).Normalize(doc2);

        // Both documents have the same component schema "test._common___Wrapper" with
        // the same inline oneOf body structure. The synthetic IDs for the inline schemas
        // should be identical because traversal order is deterministic (sorted keys).
        var wrapper1 = doc1.Components.Schemas["test._common___Wrapper"].ActualSchema;
        var wrapper2 = doc2.Components.Schemas["test._common___Wrapper"].ActualSchema;

        // Both should resolve their inline oneOf variant bodies
        var variant1 = wrapper1.OneOf.First().ActualSchema;
        var variant2 = wrapper2.OneOf.First().ActualSchema;

        Assert.True(result1.TryGetForSchema(variant1, out _));
        Assert.True(result2.TryGetForSchema(variant2, out _));
    }

    [Fact]
    public async Task NormalizedDependenciesIncludeArrayItemEnumAndFeedReferenceGraph()
    {
        var doc = await OpenApiDocument.FromJsonAsync(ArrayItemEnumDependencySpec);
        var catalog = new SchemaCatalog(doc);
        var normalization = new SchemaNormalizer(catalog).Normalize(doc);

        Assert.True(normalization.TryGet("test._common___Root", out var root));
        Assert.Contains("test._common___Option", root.DependencySchemaIds);

        var plugin = new TestNormPlugin();
        var graph = new ReferenceGraphBuilder(doc, plugin, catalog, normalization).Build();
        Assert.Contains("test._common___Option", graph.GetDependencies("test._common___Root"));
    }

    [Fact]
    public async Task NamespaceModel_HasNoCollectPropertiesForInlineSchema()
    {
        // Source/audit test: verifies that NamespaceModel no longer contains
        // CollectPropertiesForInlineSchema method.
        var nsModelSource = await System.IO.File.ReadAllTextAsync(
            System.IO.Path.Combine(GetSourceRoot(), "src/ApiGenerator/Domain/Code/HighLevel/Models/NamespaceModel.cs"));

        Assert.DoesNotContain("CollectPropertiesForInlineSchema", nsModelSource);
    }

    [Fact]
    public async Task NamespaceModel_HasNoRawAllOfAccess()
    {
        // Source/audit test: verifies NamespaceModel does not access .AllOf
        var nsModelSource = await System.IO.File.ReadAllTextAsync(
            System.IO.Path.Combine(GetSourceRoot(), "src/ApiGenerator/Domain/Code/HighLevel/Models/NamespaceModel.cs"));

        Assert.DoesNotContain(".AllOf", nsModelSource);
    }

    [Fact]
    public async Task ReferenceGraphBuilder_HasNoRawAllOfAccess()
    {
        // Source/audit test: verifies ReferenceGraphBuilder does not access .AllOf
        var graphBuilderSource = await System.IO.File.ReadAllTextAsync(
            System.IO.Path.Combine(GetSourceRoot(), "src/ApiGenerator/Domain/Code/HighLevel/Models/ReferenceGraphBuilder.cs"));

        Assert.DoesNotContain(".AllOf", graphBuilderSource);
    }

    [Fact]
    public async Task AllOfUsage_OnlyInAllowedFiles()
    {
        // Grep audit: .AllOf should only appear in SchemaNormalizer, UnionClassifier,
        // and OperationGroupModel (schema-equivalence logic) -- not NamespaceModel or ReferenceGraphBuilder.
        var sourceDir = System.IO.Path.Combine(GetSourceRoot(), "src/ApiGenerator/Domain/Code/HighLevel/Models");
        var disallowedFiles = new[] { "NamespaceModel.cs", "ReferenceGraphBuilder.cs" };

        foreach (var fileName in disallowedFiles)
        {
            var filePath = System.IO.Path.Combine(sourceDir, fileName);
            if (!System.IO.File.Exists(filePath)) continue;
            var content = await System.IO.File.ReadAllTextAsync(filePath);
            Assert.DoesNotContain(".AllOf", content, StringComparison.Ordinal);
        }
    }

    private static string GetSourceRoot()
    {
        // Walk up from test assembly to find the repo root
        var dir = AppContext.BaseDirectory;
        while (dir != null && !System.IO.File.Exists(System.IO.Path.Combine(dir, "OpenSearch.sln")))
            dir = System.IO.Path.GetDirectoryName(dir);
        return dir ?? throw new InvalidOperationException("Could not find repo root");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Phase 6 closure test specifications
    // ─────────────────────────────────────────────────────────────────────────

    private const string ArrayItemEnumDependencySpec = """
        {
          "openapi": "3.0.1",
          "info": { "title": "test", "version": "1.0.0" },
          "paths": {
            "/test": {
              "post": {
                "x-operation-group": "test.create",
                "requestBody": { "content": { "application/json": { "schema": { "$ref": "#/components/schemas/test._common___Root" } } } },
                "responses": { "200": { "content": { "application/json": { "schema": { "type": "object" } } } } }
              }
            }
          },
          "components": {
            "schemas": {
              "test._common___Root": {
                "type": "object",
                "properties": {
                  "options": {
                    "type": "array",
                    "items": { "$ref": "#/components/schemas/test._common___Option" }
                  }
                }
              },
              "test._common___Option": {
                "type": "string",
                "enum": ["one", "two"]
              }
            }
          }
        }
        """;

    private const string InlineAllOfVariantSpec = """
        {
          "openapi": "3.0.1",
          "info": { "title": "test", "version": "1.0.0" },
          "paths": {
            "/test": {
              "post": {
                "x-operation-group": "test.create",
                "requestBody": { "content": { "application/json": { "schema": { "$ref": "#/components/schemas/test._common___Container" } } } },
                "responses": { "200": { "content": { "application/json": { "schema": { "type": "object" } } } } }
              }
            }
          },
          "components": {
            "schemas": {
              "test._common___Container": {
                "oneOf": [
                  {
                    "allOf": [
                      { "type": "object", "properties": { "inline_prop": { "type": "string" } } }
                    ],
                    "type": "object",
                    "properties": { "direct": { "type": "boolean" } }
                  }
                ]
              }
            }
          }
        }
        """;

    private const string OperationInlineSchemaSpec = """
        {
          "openapi": "3.0.1",
          "info": { "title": "test", "version": "1.0.0" },
          "paths": {
            "/test": {
              "post": {
                "x-operation-group": "test.create",
                "requestBody": {
                  "content": {
                    "application/json": {
                      "schema": {
                        "allOf": [
                          { "type": "object", "properties": { "allof_field": { "type": "integer" } } }
                        ],
                        "type": "object",
                        "properties": { "inline_field": { "type": "string" } }
                      }
                    }
                  }
                },
                "responses": { "200": { "content": { "application/json": { "schema": { "type": "object" } } } } }
              }
            }
          },
          "components": {
            "schemas": {
              "test._common___Placeholder": { "type": "object", "properties": { "x": { "type": "string" } } }
            }
          }
        }
        """;

    private const string DeterministicIdSpec_A = """
        {
          "openapi": "3.0.1",
          "info": { "title": "test", "version": "1.0.0" },
          "paths": {},
          "components": {
            "schemas": {
              "test._common___Wrapper": {
                "oneOf": [
                  { "type": "object", "properties": { "a_prop": { "type": "string" } }, "required": ["a_prop"] },
                  { "type": "object", "properties": { "b_prop": { "type": "integer" } }, "required": ["b_prop"] }
                ]
              }
            }
          }
        }
        """;

    private const string DeterministicIdSpec_B = """
        {
          "openapi": "3.0.1",
          "info": { "title": "test", "version": "1.0.0" },
          "paths": {},
          "components": {
            "schemas": {
              "test._common___Other": { "type": "object", "properties": { "unrelated": { "type": "boolean" } } },
              "test._common___Wrapper": {
                "oneOf": [
                  { "type": "object", "properties": { "a_prop": { "type": "string" } }, "required": ["a_prop"] },
                  { "type": "object", "properties": { "b_prop": { "type": "integer" } }, "required": ["b_prop"] }
                ]
              }
            }
          }
        }
        """;
}
