/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.Linq;
using System.Threading.Tasks;
using ApiGenerator.Domain.Code.HighLevel.Models;
using NSwag;
using Xunit;

namespace ApiGenerator.Tests;

/// <summary>
/// Tests for <see cref="UnionClassifier"/> structural union detection.
/// Classification is purely spec-driven — no plugin names, ML/search-pipeline checks,
/// or rendering decisions.
/// </summary>
public class UnionClassifierTests
{
    // ────────────────────────────────────────────────────────────────────────────
    // WrapperKeyOneOf Tests (search-pipeline pattern)
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task WrapperKeyOneOf_DetectedWithSingleRequiredProperty()
    {
        var doc = await OpenApiDocument.FromJsonAsync(WrapperKeyOneOfSpec);
        var catalog = new SchemaCatalog(doc);
        var classifier = new UnionClassifier(catalog);

        var model = classifier.Classify("test._common___ProcessorUnion", doc.Components.Schemas["test._common___ProcessorUnion"]);

        Assert.Equal(UnionEncoding.WrapperKeyOneOf, model.Encoding);
        Assert.Equal(2, model.Variants.Count);
        Assert.Equal("filter", model.Variants[0].Key);
        Assert.Equal("neural", model.Variants[1].Key);
    }

    [Fact]
    public async Task WrapperKeyOneOf_ExtractsBodySchemaIds()
    {
        var doc = await OpenApiDocument.FromJsonAsync(WrapperKeyOneOfSpec);
        var catalog = new SchemaCatalog(doc);
        var classifier = new UnionClassifier(catalog);

        var model = classifier.Classify("test._common___ProcessorUnion", doc.Components.Schemas["test._common___ProcessorUnion"]);

        Assert.Equal("test._common___FilterProcessor", model.Variants[0].BodySchemaId);
        Assert.Equal("test._common___NeuralProcessor", model.Variants[1].BodySchemaId);
    }

    [Fact]
    public async Task WrapperKeyOneOf_ExtractsVersionAdded()
    {
        var doc = await OpenApiDocument.FromJsonAsync(WrapperKeyOneOfWithVersionSpec);
        var catalog = new SchemaCatalog(doc);
        var classifier = new UnionClassifier(catalog);

        var model = classifier.Classify("test._common___ProcessorUnion", doc.Components.Schemas["test._common___ProcessorUnion"]);

        Assert.Equal("2.5", model.Variants[0].VersionAdded);
        Assert.Null(model.Variants[1].VersionAdded);
    }

    [Fact]
    public async Task WrapperKeyOneOf_CollectsSharedProperties()
    {
        var doc = await OpenApiDocument.FromJsonAsync(WrapperKeyOneOfWithSharedPropsSpec);
        var catalog = new SchemaCatalog(doc);
        var classifier = new UnionClassifier(catalog);

        var model = classifier.Classify("test._common___ProcessorUnion", doc.Components.Schemas["test._common___ProcessorUnion"]);

        Assert.Equal(UnionEncoding.WrapperKeyOneOf, model.Encoding);
        Assert.Equal(2, model.SharedProperties.Count);
        Assert.Contains(model.SharedProperties, p => p.WireName == "tag");
        Assert.Contains(model.SharedProperties, p => p.WireName == "description");
    }

    [Fact]
    public async Task WrapperKeyOneOf_RejectedWhenMultipleRequiredProperties()
    {
        var doc = await OpenApiDocument.FromJsonAsync(WrapperKeyMultipleRequiredSpec);
        var catalog = new SchemaCatalog(doc);
        var classifier = new UnionClassifier(catalog);

        var model = classifier.TryClassify("test._common___NotAUnion", doc.Components.Schemas["test._common___NotAUnion"]);

        Assert.Null(model);
    }

    [Fact]
    public async Task WrapperKeyOneOf_RejectedWhenNoRequiredProperty()
    {
        var doc = await OpenApiDocument.FromJsonAsync(WrapperKeyNoRequiredSpec);
        var catalog = new SchemaCatalog(doc);
        var classifier = new UnionClassifier(catalog);

        var model = classifier.TryClassify("test._common___NotAUnion", doc.Components.Schemas["test._common___NotAUnion"]);

        Assert.Null(model);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // InternalDiscriminator Tests
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task InternalDiscriminator_DetectedWithDiscriminatorKeyword()
    {
        var doc = await OpenApiDocument.FromJsonAsync(InternalDiscriminatorSpec);
        var catalog = new SchemaCatalog(doc);
        var classifier = new UnionClassifier(catalog);

        var model = classifier.Classify("test._common___AggregationUnion", doc.Components.Schemas["test._common___AggregationUnion"]);

        Assert.Equal(UnionEncoding.InternalDiscriminator, model.Encoding);
        Assert.Equal("model", model.DiscriminatorProperty);
        Assert.Equal(2, model.Variants.Count);
    }

    [Fact]
    public async Task InternalDiscriminator_ExtractsVariantKeysFromTitles()
    {
        var doc = await OpenApiDocument.FromJsonAsync(InternalDiscriminatorSpec);
        var catalog = new SchemaCatalog(doc);
        var classifier = new UnionClassifier(catalog);

        var model = classifier.Classify("test._common___AggregationUnion", doc.Components.Schemas["test._common___AggregationUnion"]);

        Assert.Contains(model.Variants, v => v.Key == "linear");
        Assert.Contains(model.Variants, v => v.Key == "simple");
    }

    [Fact]
    public async Task InternalDiscriminator_UsesExplicitMapping()
    {
        var doc = await OpenApiDocument.FromJsonAsync(InternalDiscriminatorMappingSpec);
        var classifier = new UnionClassifier(new SchemaCatalog(doc));

        var model = classifier.Classify(
            "test._common___MappedUnion",
            doc.Components.Schemas["test._common___MappedUnion"]);

        Assert.Equal(UnionEncoding.InternalDiscriminator, model.Encoding);
        Assert.Equal(new[] { "mapped_a", "mapped_b" }, model.Variants.Select(v => v.Key));
    }

    [Fact]
    public async Task InternalDiscriminator_TakesPrecedenceOverWrapperKey()
    {
        // A schema with both discriminator and wrapper-key-like variants should be classified
        // as InternalDiscriminator, not WrapperKeyOneOf
        var doc = await OpenApiDocument.FromJsonAsync(DiscriminatorWithSingleRequiredSpec);
        var catalog = new SchemaCatalog(doc);
        var classifier = new UnionClassifier(catalog);

        var model = classifier.Classify("test._common___HybridUnion", doc.Components.Schemas["test._common___HybridUnion"]);

        Assert.Equal(UnionEncoding.InternalDiscriminator, model.Encoding);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // FlatWrapperKey Tests (ingest pattern)
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task FlatWrapperKey_DetectedWithMinMaxProperties()
    {
        var doc = await OpenApiDocument.FromJsonAsync(FlatWrapperKeySpec);
        var catalog = new SchemaCatalog(doc);
        var classifier = new UnionClassifier(catalog);

        var model = classifier.Classify("test._common___ProcessorContainer", doc.Components.Schemas["test._common___ProcessorContainer"]);

        Assert.Equal(UnionEncoding.FlatWrapperKey, model.Encoding);
        Assert.Equal(3, model.Variants.Count);
    }

    [Fact]
    public async Task FlatWrapperKey_ExtractsVariantKeys()
    {
        var doc = await OpenApiDocument.FromJsonAsync(FlatWrapperKeySpec);
        var catalog = new SchemaCatalog(doc);
        var classifier = new UnionClassifier(catalog);

        var model = classifier.Classify("test._common___ProcessorContainer", doc.Components.Schemas["test._common___ProcessorContainer"]);

        Assert.Contains(model.Variants, v => v.Key == "append");
        Assert.Contains(model.Variants, v => v.Key == "convert");
        Assert.Contains(model.Variants, v => v.Key == "rename");
    }

    [Fact]
    public async Task FlatWrapperKey_ExtractsBodySchemaIds()
    {
        var doc = await OpenApiDocument.FromJsonAsync(FlatWrapperKeySpec);
        var catalog = new SchemaCatalog(doc);
        var classifier = new UnionClassifier(catalog);

        var model = classifier.Classify("test._common___ProcessorContainer", doc.Components.Schemas["test._common___ProcessorContainer"]);

        var appendVariant = model.Variants.First(v => v.Key == "append");
        Assert.Equal("test._common___AppendProcessor", appendVariant.BodySchemaId);
    }

    [Fact]
    public async Task FlatWrapperKey_RejectedWithoutMinMaxProperties()
    {
        var doc = await OpenApiDocument.FromJsonAsync(PlainObjectSpec);
        var catalog = new SchemaCatalog(doc);
        var classifier = new UnionClassifier(catalog);

        var model = classifier.TryClassify("test._common___PlainObject", doc.Components.Schemas["test._common___PlainObject"]);

        Assert.Null(model);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // TypedKeys Tests
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task TypedKeys_DetectedWithExtension()
    {
        var doc = await OpenApiDocument.FromJsonAsync(TypedKeysSpec);
        var catalog = new SchemaCatalog(doc);
        var classifier = new UnionClassifier(catalog);

        var model = classifier.Classify("test._common___AggregationResults", doc.Components.Schemas["test._common___AggregationResults"]);

        Assert.Equal(UnionEncoding.TypedKeys, model.Encoding);
    }

    [Fact]
    public async Task TypedKeys_ExtractsValueVariants()
    {
        var doc = await OpenApiDocument.FromJsonAsync(TypedKeysSpec);
        var classifier = new UnionClassifier(new SchemaCatalog(doc));

        var model = classifier.Classify(
            "test._common___AggregationResults",
            doc.Components.Schemas["test._common___AggregationResults"]);

        Assert.Equal(new[] { "AvgResult", "SumResult" }, model.Variants.Select(v => v.Key));
        Assert.All(model.Variants, variant => Assert.NotNull(variant.BodySchemaId));
    }

    [Fact]
    public async Task TypedKeys_IsUnionCandidate()
    {
        var doc = await OpenApiDocument.FromJsonAsync(TypedKeysSpec);
        var classifier = new UnionClassifier(new SchemaCatalog(doc));

        Assert.True(classifier.IsUnionCandidate(
            doc.Components.Schemas["test._common___AggregationResults"]));
    }

    [Fact]
    public async Task TypedKeys_NotDetectedWithoutExtension()
    {
        var doc = await OpenApiDocument.FromJsonAsync(AdditionalPropertiesOnlySpec);
        var catalog = new SchemaCatalog(doc);
        var classifier = new UnionClassifier(catalog);

        var model = classifier.TryClassify("test._common___GenericDict", doc.Components.Schemas["test._common___GenericDict"]);

        // Should not match TypedKeys without the extension
        Assert.Null(model);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // Unknown/Rejection Tests
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Unknown_ReturnedForPlainObject()
    {
        var doc = await OpenApiDocument.FromJsonAsync(PlainObjectSpec);
        var catalog = new SchemaCatalog(doc);
        var classifier = new UnionClassifier(catalog);

        var model = classifier.TryClassify("test._common___PlainObject", doc.Components.Schemas["test._common___PlainObject"]);

        Assert.Null(model);
    }

    [Fact]
    public async Task Unknown_ReturnedForEmptyOneOf()
    {
        var doc = await OpenApiDocument.FromJsonAsync(EmptyOneOfSpec);
        var catalog = new SchemaCatalog(doc);
        var classifier = new UnionClassifier(catalog);

        var model = classifier.TryClassify("test._common___EmptyUnion", doc.Components.Schemas["test._common___EmptyUnion"]);

        Assert.Null(model);
    }

    [Fact]
    public async Task IsUnionCandidate_TrueForOneOfSchemas()
    {
        var doc = await OpenApiDocument.FromJsonAsync(WrapperKeyOneOfSpec);
        var catalog = new SchemaCatalog(doc);
        var classifier = new UnionClassifier(catalog);

        Assert.True(classifier.IsUnionCandidate(doc.Components.Schemas["test._common___ProcessorUnion"]));
    }

    [Fact]
    public async Task IsUnionCandidate_TrueForFlatWrapperKey()
    {
        var doc = await OpenApiDocument.FromJsonAsync(FlatWrapperKeySpec);
        var catalog = new SchemaCatalog(doc);
        var classifier = new UnionClassifier(catalog);

        Assert.True(classifier.IsUnionCandidate(doc.Components.Schemas["test._common___ProcessorContainer"]));
    }

    [Fact]
    public async Task IsUnionCandidate_FalseForPlainObject()
    {
        var doc = await OpenApiDocument.FromJsonAsync(PlainObjectSpec);
        var catalog = new SchemaCatalog(doc);
        var classifier = new UnionClassifier(catalog);

        Assert.False(classifier.IsUnionCandidate(doc.Components.Schemas["test._common___PlainObject"]));
    }

    // ────────────────────────────────────────────────────────────────────────────
    // Edge Cases
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task WrapperKeyOneOf_HandlesInlineBodySchema()
    {
        var doc = await OpenApiDocument.FromJsonAsync(WrapperKeyInlineBodySpec);
        var catalog = new SchemaCatalog(doc);
        var classifier = new UnionClassifier(catalog);

        var model = classifier.Classify("test._common___InlineUnion", doc.Components.Schemas["test._common___InlineUnion"]);

        Assert.Equal(UnionEncoding.WrapperKeyOneOf, model.Encoding);
        Assert.Equal(2, model.Variants.Count);
        // Inline schemas don't have a schema ID
        Assert.Null(model.Variants[0].BodySchemaId);
        Assert.NotNull(model.Variants[0].BodySchema);
    }

    [Fact]
    public async Task WrapperKeyOneOf_HandlesMixedRefAndInline()
    {
        var doc = await OpenApiDocument.FromJsonAsync(WrapperKeyMixedSpec);
        var catalog = new SchemaCatalog(doc);
        var classifier = new UnionClassifier(catalog);

        var model = classifier.Classify("test._common___MixedUnion", doc.Components.Schemas["test._common___MixedUnion"]);

        Assert.Equal(UnionEncoding.WrapperKeyOneOf, model.Encoding);
        Assert.Equal(2, model.Variants.Count);
        Assert.NotNull(model.Variants[0].BodySchemaId);  // $ref variant
        Assert.Null(model.Variants[1].BodySchemaId);     // inline variant
    }

    // ────────────────────────────────────────────────────────────────────────────
    // Test Specifications
    // ────────────────────────────────────────────────────────────────────────────

    private const string WrapperKeyOneOfSpec = """
        {
          "openapi": "3.0.1",
          "info": { "title": "Test", "version": "1.0.0" },
          "paths": {},
          "components": {
            "schemas": {
              "test._common___ProcessorUnion": {
                "oneOf": [
                  {
                    "type": "object",
                    "properties": {
                      "filter": { "$ref": "#/components/schemas/test._common___FilterProcessor" }
                    },
                    "required": ["filter"]
                  },
                  {
                    "type": "object",
                    "properties": {
                      "neural": { "$ref": "#/components/schemas/test._common___NeuralProcessor" }
                    },
                    "required": ["neural"]
                  }
                ]
              },
              "test._common___FilterProcessor": {
                "type": "object",
                "properties": { "query": { "type": "string" } }
              },
              "test._common___NeuralProcessor": {
                "type": "object",
                "properties": { "model_id": { "type": "string" } }
              }
            }
          }
        }
        """;

    private const string WrapperKeyOneOfWithVersionSpec = """
        {
          "openapi": "3.0.1",
          "info": { "title": "Test", "version": "1.0.0" },
          "paths": {},
          "components": {
            "schemas": {
              "test._common___ProcessorUnion": {
                "oneOf": [
                  {
                    "x-version-added": "2.5",
                    "type": "object",
                    "properties": {
                      "filter": { "$ref": "#/components/schemas/test._common___FilterProcessor" }
                    },
                    "required": ["filter"]
                  },
                  {
                    "type": "object",
                    "properties": {
                      "neural": { "$ref": "#/components/schemas/test._common___NeuralProcessor" }
                    },
                    "required": ["neural"]
                  }
                ]
              },
              "test._common___FilterProcessor": {
                "type": "object",
                "properties": { "query": { "type": "string" } }
              },
              "test._common___NeuralProcessor": {
                "type": "object",
                "properties": { "model_id": { "type": "string" } }
              }
            }
          }
        }
        """;

    private const string WrapperKeyOneOfWithSharedPropsSpec = """
        {
          "openapi": "3.0.1",
          "info": { "title": "Test", "version": "1.0.0" },
          "paths": {},
          "components": {
            "schemas": {
              "test._common___ProcessorUnion": {
                "oneOf": [
                  {
                    "type": "object",
                    "properties": {
                      "filter": { "$ref": "#/components/schemas/test._common___FilterProcessor" },
                      "tag": { "type": "string" },
                      "description": { "type": "string" }
                    },
                    "required": ["filter"]
                  },
                  {
                    "type": "object",
                    "properties": {
                      "neural": { "$ref": "#/components/schemas/test._common___NeuralProcessor" },
                      "tag": { "type": "string" },
                      "description": { "type": "string" }
                    },
                    "required": ["neural"]
                  }
                ]
              },
              "test._common___FilterProcessor": {
                "type": "object",
                "properties": { "query": { "type": "string" } }
              },
              "test._common___NeuralProcessor": {
                "type": "object",
                "properties": { "model_id": { "type": "string" } }
              }
            }
          }
        }
        """;

    private const string WrapperKeyMultipleRequiredSpec = """
        {
          "openapi": "3.0.1",
          "info": { "title": "Test", "version": "1.0.0" },
          "paths": {},
          "components": {
            "schemas": {
              "test._common___NotAUnion": {
                "oneOf": [
                  {
                    "type": "object",
                    "properties": {
                      "a": { "type": "string" },
                      "b": { "type": "string" }
                    },
                    "required": ["a", "b"]
                  }
                ]
              }
            }
          }
        }
        """;

    private const string WrapperKeyNoRequiredSpec = """
        {
          "openapi": "3.0.1",
          "info": { "title": "Test", "version": "1.0.0" },
          "paths": {},
          "components": {
            "schemas": {
              "test._common___NotAUnion": {
                "oneOf": [
                  {
                    "type": "object",
                    "properties": {
                      "a": { "type": "string" }
                    }
                  }
                ]
              }
            }
          }
        }
        """;

    private const string InternalDiscriminatorSpec = """
        {
          "openapi": "3.0.1",
          "info": { "title": "Test", "version": "1.0.0" },
          "paths": {},
          "components": {
            "schemas": {
              "test._common___AggregationUnion": {
                "discriminator": {
                  "propertyName": "model"
                },
                "oneOf": [
                  {
                    "title": "linear",
                    "type": "object",
                    "properties": {
                      "model": { "type": "string" },
                      "settings": { "type": "object" }
                    }
                  },
                  {
                    "title": "simple",
                    "type": "object",
                    "properties": {
                      "model": { "type": "string" },
                      "window": { "type": "integer" }
                    }
                  }
                ]
              }
            }
          }
        }
        """;

    private const string InternalDiscriminatorMappingSpec = """
        {
          "openapi": "3.0.1",
          "info": { "title": "Test", "version": "1.0.0" },
          "paths": {},
          "components": {
            "schemas": {
              "test._common___MappedUnion": {
                "discriminator": {
                  "propertyName": "kind",
                  "mapping": {
                    "mapped_a": "#/components/schemas/test._common___VariantA",
                    "mapped_b": "#/components/schemas/test._common___VariantB"
                  }
                },
                "oneOf": [
                  { "$ref": "#/components/schemas/test._common___VariantA" },
                  { "$ref": "#/components/schemas/test._common___VariantB" }
                ]
              },
              "test._common___VariantA": {
                "title": "wrong_a",
                "type": "object",
                "properties": { "kind": { "type": "string" } }
              },
              "test._common___VariantB": {
                "title": "wrong_b",
                "type": "object",
                "properties": { "kind": { "type": "string" } }
              }
            }
          }
        }
        """;

    private const string DiscriminatorWithSingleRequiredSpec = """
        {
          "openapi": "3.0.1",
          "info": { "title": "Test", "version": "1.0.0" },
          "paths": {},
          "components": {
            "schemas": {
              "test._common___HybridUnion": {
                "discriminator": {
                  "propertyName": "type"
                },
                "oneOf": [
                  {
                    "title": "foo",
                    "type": "object",
                    "properties": {
                      "foo": { "type": "object" }
                    },
                    "required": ["foo"]
                  }
                ]
              }
            }
          }
        }
        """;

    private const string FlatWrapperKeySpec = """
        {
          "openapi": "3.0.1",
          "info": { "title": "Test", "version": "1.0.0" },
          "paths": {},
          "components": {
            "schemas": {
              "test._common___ProcessorContainer": {
                "type": "object",
                "properties": {
                  "append": { "$ref": "#/components/schemas/test._common___AppendProcessor" },
                  "convert": { "$ref": "#/components/schemas/test._common___ConvertProcessor" },
                  "rename": { "$ref": "#/components/schemas/test._common___RenameProcessor" }
                },
                "minProperties": 1,
                "maxProperties": 1
              },
              "test._common___AppendProcessor": {
                "type": "object",
                "properties": { "field": { "type": "string" } }
              },
              "test._common___ConvertProcessor": {
                "type": "object",
                "properties": { "field": { "type": "string" } }
              },
              "test._common___RenameProcessor": {
                "type": "object",
                "properties": { "field": { "type": "string" } }
              }
            }
          }
        }
        """;

    private const string TypedKeysSpec = """
        {
          "openapi": "3.0.1",
          "info": { "title": "Test", "version": "1.0.0" },
          "paths": {},
          "components": {
            "schemas": {
              "test._common___AggregationResults": {
                "type": "object",
                "x-typed-keys": true,
                "additionalProperties": {
                  "oneOf": [
                    { "$ref": "#/components/schemas/test._common___AvgResult" },
                    { "$ref": "#/components/schemas/test._common___SumResult" }
                  ]
                }
              },
              "test._common___AvgResult": {
                "title": "AvgResult",
                "type": "object",
                "properties": { "value": { "type": "number" } }
              },
              "test._common___SumResult": {
                "title": "SumResult",
                "type": "object",
                "properties": { "value": { "type": "number" } }
              }
            }
          }
        }
        """;

    private const string AdditionalPropertiesOnlySpec = """
        {
          "openapi": "3.0.1",
          "info": { "title": "Test", "version": "1.0.0" },
          "paths": {},
          "components": {
            "schemas": {
              "test._common___GenericDict": {
                "type": "object",
                "additionalProperties": {
                  "type": "object"
                }
              }
            }
          }
        }
        """;

    private const string PlainObjectSpec = """
        {
          "openapi": "3.0.1",
          "info": { "title": "Test", "version": "1.0.0" },
          "paths": {},
          "components": {
            "schemas": {
              "test._common___PlainObject": {
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

    private const string EmptyOneOfSpec = """
        {
          "openapi": "3.0.1",
          "info": { "title": "Test", "version": "1.0.0" },
          "paths": {},
          "components": {
            "schemas": {
              "test._common___EmptyUnion": {
                "oneOf": []
              }
            }
          }
        }
        """;

    private const string WrapperKeyInlineBodySpec = """
        {
          "openapi": "3.0.1",
          "info": { "title": "Test", "version": "1.0.0" },
          "paths": {},
          "components": {
            "schemas": {
              "test._common___InlineUnion": {
                "oneOf": [
                  {
                    "type": "object",
                    "properties": {
                      "foo": {
                        "type": "object",
                        "properties": { "name": { "type": "string" } }
                      }
                    },
                    "required": ["foo"]
                  },
                  {
                    "type": "object",
                    "properties": {
                      "bar": {
                        "type": "object",
                        "properties": { "value": { "type": "integer" } }
                      }
                    },
                    "required": ["bar"]
                  }
                ]
              }
            }
          }
        }
        """;

    private const string WrapperKeyMixedSpec = """
        {
          "openapi": "3.0.1",
          "info": { "title": "Test", "version": "1.0.0" },
          "paths": {},
          "components": {
            "schemas": {
              "test._common___MixedUnion": {
                "oneOf": [
                  {
                    "type": "object",
                    "properties": {
                      "ref_variant": { "$ref": "#/components/schemas/test._common___RefBody" }
                    },
                    "required": ["ref_variant"]
                  },
                  {
                    "type": "object",
                    "properties": {
                      "inline_variant": {
                        "type": "object",
                        "properties": { "value": { "type": "integer" } }
                      }
                    },
                    "required": ["inline_variant"]
                  }
                ]
              },
              "test._common___RefBody": {
                "type": "object",
                "properties": { "name": { "type": "string" } }
              }
            }
          }
        }
        """;
}
