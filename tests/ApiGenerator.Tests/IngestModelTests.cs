/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiGenerator.Configuration.Overrides;
using ApiGenerator.Configuration.Overrides.Plugins;
using ApiGenerator.Domain.Code.HighLevel.Models;
using ApiGenerator.Generator;
using ApiGenerator.Generator.Razor;
using NSwag;
using Xunit;

namespace ApiGenerator.Tests;

/// <summary>
/// Tests for Phase 5: Ingest processor migration via FlatWrapperKey classification.
/// Validates that ProcessorContainer (minProperties=1, maxProperties=1) is classified
/// as FlatWrapperKey and that NamespaceModel correctly builds a WrapperKeyUnionModel for it.
/// </summary>
public class IngestModelTests
{
    // ────────────────────────────────────────────────────────────────────────────
    // FlatWrapperKey Classification in NamespaceModel
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task FlatWrapperKey_BuildsWrapperKeyUnionModel()
    {
        var doc = await OpenApiDocument.FromJsonAsync(IngestMinimalSpec);
        var plugin = new IngestModelOverrides();
        var resolver = ModelsGenerator.BuildResolverForTest(doc, plugin);
        var ns = NamespaceModel.Build(doc, plugin.Namespace, plugin, resolver);

        var unions = ns.AllTypes.OfType<WrapperKeyUnionModel>().ToList();
        Assert.Single(unions);
        Assert.Equal("ingest._common___ProcessorContainer", unions[0].SchemaId);
    }

    [Fact]
    public async Task FlatWrapperKey_UnionHasCorrectVariants()
    {
        var doc = await OpenApiDocument.FromJsonAsync(IngestMinimalSpec);
        var plugin = new IngestModelOverrides();
        var resolver = ModelsGenerator.BuildResolverForTest(doc, plugin);
        var ns = NamespaceModel.Build(doc, plugin.Namespace, plugin, resolver);

        var union = ns.AllTypes.OfType<WrapperKeyUnionModel>().Single();
        Assert.Equal(3, union.Variants.Count);
        Assert.Contains(union.Variants, v => v.Key == "append");
        Assert.Contains(union.Variants, v => v.Key == "convert");
        Assert.Contains(union.Variants, v => v.Key == "rename");
    }

    [Fact]
    public async Task FlatWrapperKey_VariantPropertiesExtracted()
    {
        var doc = await OpenApiDocument.FromJsonAsync(IngestMinimalSpec);
        var plugin = new IngestModelOverrides();
        var resolver = ModelsGenerator.BuildResolverForTest(doc, plugin);
        var ns = NamespaceModel.Build(doc, plugin.Namespace, plugin, resolver);

        var union = ns.AllTypes.OfType<WrapperKeyUnionModel>().Single();
        var appendVariant = union.Variants.First(v => v.Key == "append");

        // AppendProcessor has field, value, allow_duplicates
        Assert.Contains(appendVariant.BodyProperties, p => p.WireName == "field");
        Assert.Contains(appendVariant.BodyProperties, p => p.WireName == "value");
        Assert.Contains(appendVariant.BodyProperties, p => p.WireName == "allow_duplicates");
    }

    [Fact]
    public async Task FlatWrapperKey_BasePropertiesFromProcessorBaseExcluded()
    {
        var doc = await OpenApiDocument.FromJsonAsync(IngestWithBaseSpec);
        var plugin = new IngestModelOverrides();
        var resolver = ModelsGenerator.BuildResolverForTest(doc, plugin);
        var ns = NamespaceModel.Build(doc, plugin.Namespace, plugin, resolver);

        var union = ns.AllTypes.OfType<WrapperKeyUnionModel>().Single();
        var appendVariant = union.Variants.First(v => v.Key == "append");

        // Base properties (tag, description, ignore_failure) should be excluded
        // from variant properties — they live on the generated base interface
        Assert.DoesNotContain(appendVariant.BodyProperties, p => p.WireName == "tag");
        Assert.DoesNotContain(appendVariant.BodyProperties, p => p.WireName == "description");
        Assert.DoesNotContain(appendVariant.BodyProperties, p => p.WireName == "ignore_failure");
        // But variant-specific properties should be present
        Assert.Contains(appendVariant.BodyProperties, p => p.WireName == "field");
    }

    [Fact]
    public async Task FlatWrapperKey_UnionIsEmitted()
    {
        var doc = await OpenApiDocument.FromJsonAsync(IngestMinimalSpec);
        var plugin = new IngestModelOverrides();
        var resolver = ModelsGenerator.BuildResolverForTest(doc, plugin);
        var ns = NamespaceModel.Build(doc, plugin.Namespace, plugin, resolver);

        Assert.Contains(ns.TypesToEmit, t => t.SchemaId == "ingest._common___ProcessorContainer");
    }

    [Fact]
    public async Task FlatWrapperKey_VariantBodiesAreOwned()
    {
        var doc = await OpenApiDocument.FromJsonAsync(IngestMinimalSpec);
        var plugin = new IngestModelOverrides();
        var resolver = ModelsGenerator.BuildResolverForTest(doc, plugin);
        var ns = NamespaceModel.Build(doc, plugin.Namespace, plugin, resolver);

        // Variant body schemas should be owned by the union (not independently emitted)
        Assert.Contains("ingest._common___AppendProcessor", ns.Graph.OwnedNodes);
        Assert.Contains("ingest._common___ConvertProcessor", ns.Graph.OwnedNodes);
        Assert.Contains("ingest._common___RenameProcessor", ns.Graph.OwnedNodes);
    }

    [Fact]
    public async Task FlatWrapperKey_EnumsReferencedByVariantsAreEmitted()
    {
        var doc = await OpenApiDocument.FromJsonAsync(IngestWithEnumSpec);
        var plugin = new IngestModelOverrides();
        var resolver = ModelsGenerator.BuildResolverForTest(doc, plugin);
        var ns = NamespaceModel.Build(doc, plugin.Namespace, plugin, resolver);

        // ConvertType enum should be emitted as it's referenced by the convert processor variant
        var enums = ns.TypesToEmit.OfType<EnumModel>().ToList();
        Assert.Contains(enums, e => e.CsharpName == "ConvertProcessorType");
    }

    [Fact]
    public async Task FlatWrapperKey_MappedVariantsSkippedButUnionStillBuilt()
    {
        // When a variant's body schema is mapped to an existing type in a FlatWrapperKey union,
        // that variant is SKIPPED (not included) but the union is still emitted with the
        // remaining non-mapped variants.
        var doc = await OpenApiDocument.FromJsonAsync(IngestWithMappedVariantSpec);
        var plugin = new TestIngestPluginWithScriptMapping();
        var resolver = ModelsGenerator.BuildResolverForTest(doc, plugin);
        var ns = NamespaceModel.Build(doc, plugin.Namespace, plugin, resolver);

        var unions = ns.AllTypes.OfType<WrapperKeyUnionModel>().ToList();
        Assert.Single(unions);
        // Only the non-mapped variant should be present
        var union = unions[0];
        Assert.Single(union.Variants);
        Assert.Equal("append", union.Variants[0].Key);
        // The mapped 'script' variant should NOT be in the union
        Assert.DoesNotContain(union.Variants, v => v.Key == "script");
    }

    // ────────────────────────────────────────────────────────────────────────────
    // Compatibility: Existing search-pipeline WrapperKeyOneOf unchanged
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task WrapperKeyOneOf_StillDetectedCorrectly()
    {
        var doc = await OpenApiDocument.FromJsonAsync(SearchPipelineSpec);
        var plugin = new SearchPipelineModelOverrides();
        var resolver = ModelsGenerator.BuildResolverForTest(doc, plugin);
        var ns = NamespaceModel.Build(doc, plugin.Namespace, plugin, resolver);

        var unions = ns.AllTypes.OfType<WrapperKeyUnionModel>().ToList();
        Assert.Single(unions);
        Assert.Equal("search_pipeline._common___RequestProcessor", unions[0].SchemaId);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // Phase 5: UnionRenderingPolicy-driven generation
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Policy_AppliesVariantBaseClass()
    {
        var doc = await OpenApiDocument.FromJsonAsync(IngestMinimalSpec);
        var plugin = new TestPolicyPlugin();
        var resolver = ModelsGenerator.BuildResolverForTest(doc, plugin);
        var ns = NamespaceModel.Build(doc, plugin.Namespace, plugin, resolver);

        var union = ns.AllTypes.OfType<WrapperKeyUnionModel>().Single();
        // All non-overridden variants should get the policy base class
        foreach (var v in union.Variants)
            Assert.Equal("ProcessorBase", v.BaseClass);
    }

    [Fact]
    public async Task Policy_AppliesGenericDescriptors()
    {
        var doc = await OpenApiDocument.FromJsonAsync(IngestMinimalSpec);
        var plugin = new TestPolicyPlugin();
        var resolver = ModelsGenerator.BuildResolverForTest(doc, plugin);
        var ns = NamespaceModel.Build(doc, plugin.Namespace, plugin, resolver);

        var union = ns.AllTypes.OfType<WrapperKeyUnionModel>().Single();
        foreach (var v in union.Variants)
            Assert.True(v.IsGenericDescriptor);
    }

    [Fact]
    public async Task Policy_AppliesFieldProperties()
    {
        var doc = await OpenApiDocument.FromJsonAsync(IngestMinimalSpec);
        var plugin = new TestPolicyPlugin();
        var resolver = ModelsGenerator.BuildResolverForTest(doc, plugin);
        var ns = NamespaceModel.Build(doc, plugin.Namespace, plugin, resolver);

        var union = ns.AllTypes.OfType<WrapperKeyUnionModel>().Single();
        var appendVariant = union.Variants.First(v => v.Key == "append");
        Assert.Contains("field", appendVariant.FieldProperties);
    }

    [Fact]
    public async Task Policy_NonGenericOverrideWorks()
    {
        var doc = await OpenApiDocument.FromJsonAsync(IngestWithNonGenericSpec);
        var plugin = new TestPolicyWithNonGenericPlugin();
        var resolver = ModelsGenerator.BuildResolverForTest(doc, plugin);
        var ns = NamespaceModel.Build(doc, plugin.Namespace, plugin, resolver);

        var union = ns.AllTypes.OfType<WrapperKeyUnionModel>().Single();
        var scriptVariant = union.Variants.First(v => v.Key == "script");
        var appendVariant = union.Variants.First(v => v.Key == "append");

        // script should be non-generic (overridden)
        Assert.False(scriptVariant.IsGenericDescriptor);
        // append should still be generic (default)
        Assert.True(appendVariant.IsGenericDescriptor);
    }

    [Fact]
    public async Task Policy_RetainedVariantsExcludedFromGeneration()
    {
        var doc = await OpenApiDocument.FromJsonAsync(IngestWithRetainedSpec);
        var plugin = new TestPolicyWithRetainedPlugin();
        var resolver = ModelsGenerator.BuildResolverForTest(doc, plugin);
        var ns = NamespaceModel.Build(doc, plugin.Namespace, plugin, resolver);

        var union = ns.AllTypes.OfType<WrapperKeyUnionModel>().Single();
        // AllVariants includes the retained variant
        Assert.Contains(union.AllVariants, v => v.Key == "text_embedding");
        // GeneratedVariants excludes it
        Assert.DoesNotContain(union.GeneratedVariants, v => v.Key == "text_embedding");
        // Non-retained variant is in both
        Assert.Contains(union.GeneratedVariants, v => v.Key == "append");
    }

    [Fact]
    public async Task Policy_VariantNameOverridesApplied()
    {
        var doc = await OpenApiDocument.FromJsonAsync(IngestMinimalSpec);
        var plugin = new TestPolicyWithNamingPlugin();
        var resolver = ModelsGenerator.BuildResolverForTest(doc, plugin);
        var ns = NamespaceModel.Build(doc, plugin.Namespace, plugin, resolver);

        var union = ns.AllTypes.OfType<WrapperKeyUnionModel>().Single();
        // The "rename" variant should be named "RenameProcessor" via override
        var renameVar = union.Variants.First(v => v.Key == "rename");
        Assert.Equal("RenameProcessor", renameVar.CsharpName);
        Assert.Equal("MyRename", renameVar.FluentMethodName);
    }

    [Fact]
    public async Task Policy_SuppressBaseInterfaceWorks()
    {
        var doc = await OpenApiDocument.FromJsonAsync(IngestMinimalSpec);
        var plugin = new TestPolicyPlugin();
        var resolver = ModelsGenerator.BuildResolverForTest(doc, plugin);
        var ns = NamespaceModel.Build(doc, plugin.Namespace, plugin, resolver);

        var union = ns.AllTypes.OfType<WrapperKeyUnionModel>().Single();
        Assert.True(union.SuppressBaseInterface);
        Assert.Equal("IProcessor", union.EffectiveInterfaceName);
        Assert.Equal("ProcessorFormatter", union.FormatterName);
        Assert.Equal("ProcessorsDescriptor", union.DescriptorBuilderName);
    }

    [Fact]
    public async Task Policy_SyntheticNewProcessor_AutoGenerates()
    {
        // Add a "synthetic_new" processor — proves that a new ProcessorContainer property
        // automatically produces model + formatter branch + fluent descriptor method
        // WITHOUT any generator code changes.
        var doc = await OpenApiDocument.FromJsonAsync(IngestWithSyntheticNewSpec);
        var plugin = new TestPolicyPlugin();
        var resolver = ModelsGenerator.BuildResolverForTest(doc, plugin);
        var ns = NamespaceModel.Build(doc, plugin.Namespace, plugin, resolver);

        var union = ns.AllTypes.OfType<WrapperKeyUnionModel>().Single();
        // New variant is automatically included
        Assert.Contains(union.Variants, v => v.Key == "synthetic_new");
        var newVariant = union.Variants.First(v => v.Key == "synthetic_new");
        // Has the correct properties from the spec
        Assert.Contains(newVariant.BodyProperties, p => p.WireName == "source");
        Assert.Contains(newVariant.BodyProperties, p => p.WireName == "target");
        // Gets the union-level defaults (generic, ProcessorBase, field properties)
        Assert.True(newVariant.IsGenericDescriptor);
        Assert.Equal("ProcessorBase", newVariant.BaseClass);
    }

    [Fact]
    public async Task Policy_DescriptorBasePatternApplied()
    {
        var doc = await OpenApiDocument.FromJsonAsync(IngestMinimalSpec);
        var plugin = new TestPolicyPlugin();
        var resolver = ModelsGenerator.BuildResolverForTest(doc, plugin);
        var ns = NamespaceModel.Build(doc, plugin.Namespace, plugin, resolver);

        var union = ns.AllTypes.OfType<WrapperKeyUnionModel>().Single();
        foreach (var v in union.Variants)
            Assert.Equal("ProcessorDescriptorBase<{0}, {1}>", v.DescriptorBasePattern);
    }

    [Fact]
    public async Task Policy_ExcludedPropertiesWork()
    {
        var doc = await OpenApiDocument.FromJsonAsync(IngestWithRetainedSpec);
        var plugin = new TestPolicyWithRetainedPlugin();
        var resolver = ModelsGenerator.BuildResolverForTest(doc, plugin);
        var ns = NamespaceModel.Build(doc, plugin.Namespace, plugin, resolver);

        var union = ns.AllTypes.OfType<WrapperKeyUnionModel>().Single();
        var retained = union.AllVariants.First(v => v.Key == "text_embedding");
        // model_id should be excluded
        Assert.DoesNotContain(retained.BodyProperties, p => p.WireName == "model_id");
    }

    // ────────────────────────────────────────────────────────────────────────────
    // Phase 5 Test helpers: policy plugins
    // ────────────────────────────────────────────────────────────────────────────

    private sealed class TestPolicyPlugin : ModelOverridesBase
    {
        public override string Namespace => "ingest";
        public override string OutputFolder => "Ingest/Generated";
        public override IDictionary<string, string> MappedTypes { get; } =
            new Dictionary<string, string>(System.StringComparer.Ordinal)
            {
                ["ingest._common___ProcessorContainer"] = "IProcessor",
            };
        public override IDictionary<string, UnionRenderingPolicy> UnionPolicies { get; } =
            new Dictionary<string, UnionRenderingPolicy>(System.StringComparer.Ordinal)
            {
                ["ingest._common___ProcessorContainer"] = new UnionRenderingPolicy
                {
                    BaseInterfaceName = "IProcessor",
                    FormatterName = "ProcessorFormatter",
                    ListDescriptorName = "ProcessorsDescriptor",
                    SuppressBaseInterfaceGeneration = true,
                    VariantBaseClass = "ProcessorBase",
                    DescriptorBasePattern = "ProcessorDescriptorBase<{0}, {1}>",
                    GenericDescriptors = true,
                    FieldProperties = new HashSet<string>(System.StringComparer.Ordinal) { "field", "target_field" },
                },
            };
    }

    private sealed class TestPolicyWithNonGenericPlugin : ModelOverridesBase
    {
        public override string Namespace => "ingest";
        public override string OutputFolder => "Ingest/Generated";
        public override IDictionary<string, string> MappedTypes { get; } =
            new Dictionary<string, string>(System.StringComparer.Ordinal)
            {
                ["ingest._common___ProcessorContainer"] = "IProcessor",
            };
        public override IDictionary<string, UnionRenderingPolicy> UnionPolicies { get; } =
            new Dictionary<string, UnionRenderingPolicy>(System.StringComparer.Ordinal)
            {
                ["ingest._common___ProcessorContainer"] = new UnionRenderingPolicy
                {
                    GenericDescriptors = true,
                    VariantOverrides = new Dictionary<string, VariantPolicy>(System.StringComparer.Ordinal)
                    {
                        ["script"] = new VariantPolicy { NonGenericDescriptor = true },
                    },
                },
            };
    }

    private sealed class TestPolicyWithRetainedPlugin : ModelOverridesBase
    {
        public override string Namespace => "ingest";
        public override string OutputFolder => "Ingest/Generated";
        public override IDictionary<string, string> MappedTypes { get; } =
            new Dictionary<string, string>(System.StringComparer.Ordinal)
            {
                ["ingest._common___ProcessorContainer"] = "IProcessor",
            };
        public override IDictionary<string, UnionRenderingPolicy> UnionPolicies { get; } =
            new Dictionary<string, UnionRenderingPolicy>(System.StringComparer.Ordinal)
            {
                ["ingest._common___ProcessorContainer"] = new UnionRenderingPolicy
                {
                    GenericDescriptors = true,
                    VariantBaseClass = "ProcessorBase",
                    VariantOverrides = new Dictionary<string, VariantPolicy>(System.StringComparer.Ordinal)
                    {
                        ["text_embedding"] = new VariantPolicy
                        {
                            Retained = true,
                            ExcludedProperties = new HashSet<string>(System.StringComparer.Ordinal)
                            {
                                "model_id",
                            },
                        },
                    },
                },
            };
    }

    private sealed class TestPolicyWithNamingPlugin : ModelOverridesBase
    {
        public override string Namespace => "ingest";
        public override string OutputFolder => "Ingest/Generated";
        public override IDictionary<string, string> MappedTypes { get; } =
            new Dictionary<string, string>(System.StringComparer.Ordinal)
            {
                ["ingest._common___ProcessorContainer"] = "IProcessor",
            };
        public override IDictionary<string, UnionRenderingPolicy> UnionPolicies { get; } =
            new Dictionary<string, UnionRenderingPolicy>(System.StringComparer.Ordinal)
            {
                ["ingest._common___ProcessorContainer"] = new UnionRenderingPolicy
                {
                    VariantNameOverrides = new Dictionary<string, string>(System.StringComparer.Ordinal)
                    {
                        ["rename"] = "RenameProcessor",
                    },
                    FluentMethodNameOverrides = new Dictionary<string, string>(System.StringComparer.Ordinal)
                    {
                        ["rename"] = "MyRename",
                    },
                },
            };
    }

    // ────────────────────────────────────────────────────────────────────────────
    // Phase 5 test specs
    // ────────────────────────────────────────────────────────────────────────────

    private const string IngestWithNonGenericSpec = """
    {
      "openapi": "3.0.3",
      "info": { "title": "Test", "version": "1.0" },
      "paths": {
        "/_ingest/pipeline/{id}": {
          "put": {
            "operationId": "ingest.put_pipeline",
            "x-operation-group": "ingest.put_pipeline",
            "parameters": [{ "name": "id", "in": "path", "required": true, "schema": { "type": "string" } }],
            "requestBody": { "content": { "application/json": { "schema": { "type": "object", "properties": { "processors": { "type": "array", "items": { "$ref": "#/components/schemas/ingest._common___ProcessorContainer" } } } } } } },
            "responses": { "200": { "description": "OK", "content": { "application/json": { "schema": { "type": "object" } } } } }
          }
        }
      },
      "components": {
        "schemas": {
          "ingest._common___ProcessorContainer": {
            "type": "object",
            "properties": {
              "append": { "$ref": "#/components/schemas/ingest._common___AppendProcessor" },
              "script": { "$ref": "#/components/schemas/ingest._common___ScriptProcessor" }
            },
            "minProperties": 1, "maxProperties": 1
          },
          "ingest._common___AppendProcessor": { "type": "object", "properties": { "field": { "type": "string" } } },
          "ingest._common___ScriptProcessor": { "type": "object", "properties": { "source": { "type": "string" } } }
        }
      }
    }
    """;

    private const string IngestWithRetainedSpec = """
    {
      "openapi": "3.0.3",
      "info": { "title": "Test", "version": "1.0" },
      "paths": {
        "/_ingest/pipeline/{id}": {
          "put": {
            "operationId": "ingest.put_pipeline",
            "x-operation-group": "ingest.put_pipeline",
            "parameters": [{ "name": "id", "in": "path", "required": true, "schema": { "type": "string" } }],
            "requestBody": { "content": { "application/json": { "schema": { "type": "object", "properties": { "processors": { "type": "array", "items": { "$ref": "#/components/schemas/ingest._common___ProcessorContainer" } } } } } } },
            "responses": { "200": { "description": "OK", "content": { "application/json": { "schema": { "type": "object" } } } } }
          }
        }
      },
      "components": {
        "schemas": {
          "ingest._common___ProcessorContainer": {
            "type": "object",
            "properties": {
              "append": { "$ref": "#/components/schemas/ingest._common___AppendProcessor" },
              "text_embedding": { "$ref": "#/components/schemas/ingest._common___TextEmbeddingProcessor" }
            },
            "minProperties": 1, "maxProperties": 1
          },
          "ingest._common___AppendProcessor": { "type": "object", "properties": { "field": { "type": "string" } } },
          "ingest._common___TextEmbeddingProcessor": { "type": "object", "properties": { "model_id": { "type": "string" }, "some_field": { "type": "string" } } }
        }
      }
    }
    """;

    private const string IngestWithSyntheticNewSpec = """
    {
      "openapi": "3.0.3",
      "info": { "title": "Test", "version": "1.0" },
      "paths": {
        "/_ingest/pipeline/{id}": {
          "put": {
            "operationId": "ingest.put_pipeline",
            "x-operation-group": "ingest.put_pipeline",
            "parameters": [{ "name": "id", "in": "path", "required": true, "schema": { "type": "string" } }],
            "requestBody": { "content": { "application/json": { "schema": { "type": "object", "properties": { "processors": { "type": "array", "items": { "$ref": "#/components/schemas/ingest._common___ProcessorContainer" } } } } } } },
            "responses": { "200": { "description": "OK", "content": { "application/json": { "schema": { "type": "object" } } } } }
          }
        }
      },
      "components": {
        "schemas": {
          "ingest._common___ProcessorContainer": {
            "type": "object",
            "properties": {
              "append": { "$ref": "#/components/schemas/ingest._common___AppendProcessor" },
              "synthetic_new": { "$ref": "#/components/schemas/ingest._common___SyntheticNewProcessor" }
            },
            "minProperties": 1, "maxProperties": 1
          },
          "ingest._common___AppendProcessor": { "type": "object", "properties": { "field": { "type": "string" } } },
          "ingest._common___SyntheticNewProcessor": { "type": "object", "properties": { "source": { "type": "string" }, "target": { "type": "string" } } }
        }
      }
    }
    """;

    // ────────────────────────────────────────────────────────────────────────────
    // Edge cases
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task FlatWrapperKey_EmptyPropertiesNotClassified()
    {
        // An object with minProperties=1, maxProperties=1 but no properties should not be a union
        var doc = await OpenApiDocument.FromJsonAsync(EmptyFlatWrapperSpec);
        var catalog = new SchemaCatalog(doc);
        var classifier = new UnionClassifier(catalog);

        var model = classifier.TryClassify("test___Empty", doc.Components.Schemas["test___Empty"]);
        Assert.Null(model);
    }

    [Fact]
    public async Task FlatWrapperKey_WithoutMinMaxNotClassified()
    {
        // A regular object (no min/maxProperties) should not be classified as FlatWrapperKey
        var doc = await OpenApiDocument.FromJsonAsync(RegularObjectSpec);
        var catalog = new SchemaCatalog(doc);
        var classifier = new UnionClassifier(catalog);

        var model = classifier.TryClassify("test___Regular", doc.Components.Schemas["test___Regular"]);
        Assert.Null(model);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // Test helpers
    // ────────────────────────────────────────────────────────────────────────────

    private sealed class TestIngestPluginWithScriptMapping : ModelOverridesBase
    {
        public override string Namespace => "ingest";
        public override string OutputFolder => "Ingest/Generated";

        public override IDictionary<string, string> MappedTypes { get; } =
            new Dictionary<string, string>(System.StringComparer.Ordinal)
            {
                ["ingest._common___ProcessorContainer"] = "IProcessor",
                // Map the script variant body to an existing type — this triggers rejection
                ["ingest._common___ScriptProcessor"] = "IScript",
            };
    }

    // ────────────────────────────────────────────────────────────────────────────
    // Inline specs (JSON format matching NSwag expectations)
    // ────────────────────────────────────────────────────────────────────────────

    private const string IngestMinimalSpec = """
    {
      "openapi": "3.0.3",
      "info": { "title": "Test", "version": "1.0" },
      "paths": {
        "/_ingest/pipeline/{id}": {
          "put": {
            "operationId": "ingest.put_pipeline",
            "x-operation-group": "ingest.put_pipeline",
            "parameters": [{ "name": "id", "in": "path", "required": true, "schema": { "type": "string" } }],
            "requestBody": {
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "processors": {
                        "type": "array",
                        "items": { "$ref": "#/components/schemas/ingest._common___ProcessorContainer" }
                      }
                    }
                  }
                }
              }
            },
            "responses": { "200": { "description": "OK", "content": { "application/json": { "schema": { "type": "object" } } } } }
          }
        }
      },
      "components": {
        "schemas": {
          "ingest._common___ProcessorContainer": {
            "type": "object",
            "properties": {
              "append": { "$ref": "#/components/schemas/ingest._common___AppendProcessor" },
              "convert": { "$ref": "#/components/schemas/ingest._common___ConvertProcessor" },
              "rename": { "$ref": "#/components/schemas/ingest._common___RenameProcessor" }
            },
            "minProperties": 1,
            "maxProperties": 1
          },
          "ingest._common___AppendProcessor": {
            "type": "object",
            "properties": {
              "field": { "type": "string" },
              "value": { "type": "string" },
              "allow_duplicates": { "type": "boolean" }
            },
            "required": ["field", "value"]
          },
          "ingest._common___ConvertProcessor": {
            "type": "object",
            "properties": {
              "field": { "type": "string" },
              "type": { "type": "string" }
            },
            "required": ["field", "type"]
          },
          "ingest._common___RenameProcessor": {
            "type": "object",
            "properties": {
              "field": { "type": "string" },
              "target_field": { "type": "string" }
            },
            "required": ["field", "target_field"]
          }
        }
      }
    }
    """;

    private const string IngestWithBaseSpec = """
    {
      "openapi": "3.0.3",
      "info": { "title": "Test", "version": "1.0" },
      "paths": {
        "/_ingest/pipeline/{id}": {
          "put": {
            "operationId": "ingest.put_pipeline",
            "x-operation-group": "ingest.put_pipeline",
            "parameters": [{ "name": "id", "in": "path", "required": true, "schema": { "type": "string" } }],
            "requestBody": {
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "processors": {
                        "type": "array",
                        "items": { "$ref": "#/components/schemas/ingest._common___ProcessorContainer" }
                      }
                    }
                  }
                }
              }
            },
            "responses": { "200": { "description": "OK", "content": { "application/json": { "schema": { "type": "object" } } } } }
          }
        }
      },
      "components": {
        "schemas": {
          "ingest._common___ProcessorContainer": {
            "type": "object",
            "properties": {
              "append": { "$ref": "#/components/schemas/ingest._common___AppendProcessor" }
            },
            "minProperties": 1,
            "maxProperties": 1
          },
          "ingest._common___ProcessorBase": {
            "type": "object",
            "properties": {
              "tag": { "type": "string" },
              "description": { "type": "string" },
              "ignore_failure": { "type": "boolean" }
            }
          },
          "ingest._common___AppendProcessor": {
            "allOf": [
              { "$ref": "#/components/schemas/ingest._common___ProcessorBase" },
              {
                "type": "object",
                "properties": {
                  "field": { "type": "string" },
                  "value": { "type": "string" }
                },
                "required": ["field", "value"]
              }
            ]
          }
        }
      }
    }
    """;

    private const string IngestWithEnumSpec = """
    {
      "openapi": "3.0.3",
      "info": { "title": "Test", "version": "1.0" },
      "paths": {
        "/_ingest/pipeline/{id}": {
          "put": {
            "operationId": "ingest.put_pipeline",
            "x-operation-group": "ingest.put_pipeline",
            "parameters": [{ "name": "id", "in": "path", "required": true, "schema": { "type": "string" } }],
            "requestBody": {
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "processors": {
                        "type": "array",
                        "items": { "$ref": "#/components/schemas/ingest._common___ProcessorContainer" }
                      }
                    }
                  }
                }
              }
            },
            "responses": { "200": { "description": "OK", "content": { "application/json": { "schema": { "type": "object" } } } } }
          }
        }
      },
      "components": {
        "schemas": {
          "ingest._common___ProcessorContainer": {
            "type": "object",
            "properties": {
              "convert": { "$ref": "#/components/schemas/ingest._common___ConvertProcessor" }
            },
            "minProperties": 1,
            "maxProperties": 1
          },
          "ingest._common___ConvertProcessor": {
            "type": "object",
            "properties": {
              "field": { "type": "string" },
              "type": { "$ref": "#/components/schemas/ingest._common___ConvertType" }
            },
            "required": ["field", "type"]
          },
          "ingest._common___ConvertType": {
            "type": "string",
            "enum": ["integer", "long", "float", "double", "string", "boolean", "auto"]
          }
        }
      }
    }
    """;

    private const string IngestWithMappedVariantSpec = """
    {
      "openapi": "3.0.3",
      "info": { "title": "Test", "version": "1.0" },
      "paths": {
        "/_ingest/pipeline/{id}": {
          "put": {
            "operationId": "ingest.put_pipeline",
            "x-operation-group": "ingest.put_pipeline",
            "parameters": [{ "name": "id", "in": "path", "required": true, "schema": { "type": "string" } }],
            "requestBody": {
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "processors": {
                        "type": "array",
                        "items": { "$ref": "#/components/schemas/ingest._common___ProcessorContainer" }
                      }
                    }
                  }
                }
              }
            },
            "responses": { "200": { "description": "OK", "content": { "application/json": { "schema": { "type": "object" } } } } }
          }
        }
      },
      "components": {
        "schemas": {
          "ingest._common___ProcessorContainer": {
            "type": "object",
            "properties": {
              "append": { "$ref": "#/components/schemas/ingest._common___AppendProcessor" },
              "script": { "$ref": "#/components/schemas/ingest._common___ScriptProcessor" }
            },
            "minProperties": 1,
            "maxProperties": 1
          },
          "ingest._common___AppendProcessor": {
            "type": "object",
            "properties": { "field": { "type": "string" } }
          },
          "ingest._common___ScriptProcessor": {
            "type": "object",
            "properties": { "source": { "type": "string" } }
          }
        }
      }
    }
    """;

    private const string SearchPipelineSpec = """
    {
      "openapi": "3.0.3",
      "info": { "title": "Test", "version": "1.0" },
      "paths": {
        "/_search/pipeline/{id}": {
          "put": {
            "operationId": "search_pipeline.put",
            "x-operation-group": "search_pipeline.put",
            "parameters": [{ "name": "id", "in": "path", "required": true, "schema": { "type": "string" } }],
            "requestBody": {
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "request_processors": {
                        "type": "array",
                        "items": { "$ref": "#/components/schemas/search_pipeline._common___RequestProcessor" }
                      }
                    }
                  }
                }
              }
            },
            "responses": { "200": { "description": "OK", "content": { "application/json": { "schema": { "type": "object" } } } } }
          }
        }
      },
      "components": {
        "schemas": {
          "search_pipeline._common___RequestProcessor": {
            "oneOf": [
              {
                "type": "object",
                "title": "filter_query",
                "properties": {
                  "filter_query": { "$ref": "#/components/schemas/search_pipeline._common___FilterProcessor" }
                },
                "required": ["filter_query"]
              },
              {
                "type": "object",
                "title": "neural",
                "properties": {
                  "neural": { "$ref": "#/components/schemas/search_pipeline._common___NeuralProcessor" }
                },
                "required": ["neural"]
              }
            ]
          },
          "search_pipeline._common___FilterProcessor": {
            "type": "object",
            "properties": { "query": { "type": "object" } }
          },
          "search_pipeline._common___NeuralProcessor": {
            "type": "object",
            "properties": { "model_id": { "type": "string" } }
          }
        }
      }
    }
    """;

    private const string EmptyFlatWrapperSpec = """
    {
      "openapi": "3.0.3",
      "info": { "title": "Test", "version": "1.0" },
      "paths": {},
      "components": {
        "schemas": {
          "test___Empty": {
            "type": "object",
            "minProperties": 1,
            "maxProperties": 1
          }
        }
      }
    }
    """;

    private const string RegularObjectSpec = """
    {
      "openapi": "3.0.3",
      "info": { "title": "Test", "version": "1.0" },
      "paths": {},
      "components": {
        "schemas": {
          "test___Regular": {
            "type": "object",
            "properties": {
              "name": { "type": "string" },
              "value": { "type": "integer" }
            }
          }
        }
      }
    }
    """;
}
