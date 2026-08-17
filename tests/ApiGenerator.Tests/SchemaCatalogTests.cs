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

public class SchemaCatalogTests
{
    [Fact]
    public async Task ResolvesComponentWrappersResolvedSchemasAndInlinedReferences()
    {
        var document = await ReadDocument();
        var catalog = new SchemaCatalog(document);

        foreach (var schemaId in new[] { StatusId, BodyId, UnionId, ContainerId })
        {
            var component = document.Components.Schemas[schemaId];
            Assert.True(catalog.TryGetId(component, out var wrapperId));
            Assert.Equal(schemaId, wrapperId);
            Assert.True(catalog.TryGetId(component.ActualSchema, out var actualId));
            Assert.Equal(schemaId, actualId);
        }

        var properties = document.Components.Schemas[ContainerId].ActualSchema.Properties;
        Assert.Equal(StatusId, GetId(catalog, properties["status"]));
        Assert.Equal(BodyId, GetId(catalog, properties["body"]));
        Assert.Equal(UnionId, GetId(catalog, properties["items"].ActualSchema.Item!));
    }

    [Fact]
    public async Task ResolverProjectsCanonicalReferencesToNamedCsharpTypes()
    {
        var document = await ReadDocument();
        var resolver = new ModelTypeResolver(new TestModelOverrides(), new SchemaCatalog(document));
        var properties = document.Components.Schemas[ContainerId].ActualSchema.Properties;

        Assert.Equal("Status?", resolver.ResolveTypeRef(properties["status"]).ToCsharp());
        Assert.Equal("IBody", resolver.ResolveTypeRef(properties["body"]).ToCsharp());
        Assert.Equal("IList<IUnion>", resolver.ResolveTypeRef(properties["items"]).ToCsharp());
    }

    [Fact]
    public async Task NamespaceModelUsesCatalogForWrapperKeyUnionBodies()
    {
        var document = await ReadDocument();
        var overrides = new TestModelOverrides();
        var resolver = new ModelTypeResolver(overrides, new SchemaCatalog(document));

        var model = NamespaceModel.Build(document, overrides.Namespace, overrides, resolver);
        var union = Assert.Single(model.TypesToEmit.OfType<WrapperKeyUnionModel>());
        var variant = Assert.Single(union.Variants);

        Assert.Equal("Union", union.CsharpName);
        Assert.Equal("body", variant.Key);
        Assert.Equal("Body", variant.CsharpName);
        Assert.DoesNotContain(model.TypesToEmit, type => type.SchemaId == BodyId);
    }

    [Fact]
    public async Task UnknownSchemaHasNoCanonicalId()
    {
        var catalog = new SchemaCatalog(await ReadDocument());

        Assert.False(catalog.TryGetId(new NJsonSchema.JsonSchema(), out _));
        Assert.False(catalog.TryGetSchema("missing", out _));
    }

    [Fact]
    public void BothAliasesResolveToSameIdWhenNswagSharesOneSchemaInstance()
    {
        var shared = new NJsonSchema.JsonSchema { Type = NJsonSchema.JsonObjectType.Object };
        var document = new OpenApiDocument();
        document.Components.Schemas["test._common___FirstAlias"] = shared;
        document.Components.Schemas["test._common___LastAlias"] = shared;

        var catalog = new SchemaCatalog(document);

        // NSwag may iterate component schemas in any order; the catalog picks one key as
        // canonical. Assert only that both aliases resolve to the same canonical ID.
        var id = GetId(catalog, shared);
        Assert.True(
            id == "test._common___FirstAlias" || id == "test._common___LastAlias",
            $"Expected canonical ID to be one of the two registered aliases, got: {id}");
        // Both the wrapper and its ActualSchema must resolve to the same canonical ID.
        Assert.True(catalog.TryGetId(shared.ActualSchema, out var actualId));
        Assert.Equal(id, actualId);
    }

    private static string GetId(SchemaCatalog catalog, NJsonSchema.JsonSchema schema)
    {
        Assert.True(catalog.TryGetId(schema, out var id));
        return id;
    }

    private static Task<OpenApiDocument> ReadDocument() => OpenApiDocument.FromJsonAsync(Specification);

    private const string StatusId = "test._common___Status";
    private const string BodyId = "test._common___Body";
    private const string UnionId = "test._common___Union";
    private const string ContainerId = "test._common___Container";

    private const string Specification = """
        {
          "openapi": "3.0.1",
          "info": { "title": "Schema catalog tests", "version": "1.0.0" },
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
              "test._common___Status": {
                "type": "string",
                "enum": ["ready", "failed"]
              },
              "test._common___Body": {
                "type": "object",
                "properties": {
                  "name": { "type": "string" }
                }
              },
              "test._common___Union": {
                "oneOf": [
                  {
                    "title": "body",
                    "type": "object",
                    "properties": {
                      "body": { "$ref": "#/components/schemas/test._common___Body" }
                    },
                    "required": ["body"]
                  }
                ]
              },
              "test._common___Container": {
                "type": "object",
                "properties": {
                  "status": { "$ref": "#/components/schemas/test._common___Status" },
                  "body": { "$ref": "#/components/schemas/test._common___Body" },
                  "items": {
                    "type": "array",
                    "items": { "$ref": "#/components/schemas/test._common___Union" }
                  }
                }
              }
            }
          }
        }
        """;

    private sealed class TestModelOverrides : ModelOverridesBase
    {
        public override string Namespace => "test";
        public override string OutputFolder => "Test";
        public override IDictionary<string, string> MappedTypes { get; } =
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                [UnionId] = "IUnion",
            };
    }
}

