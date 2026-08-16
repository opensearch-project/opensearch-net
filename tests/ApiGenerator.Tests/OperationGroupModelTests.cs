/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Linq;
using System.Threading.Tasks;
using ApiGenerator.Domain.Code.HighLevel.Models;
using NSwag;
using Xunit;

namespace ApiGenerator.Tests;

public class OperationGroupModelTests
{
    [Fact]
    public async Task SingleVariantCapturesAllMetadata()
    {
        var doc = await OpenApiDocument.FromJsonAsync(SingleVariantSpec);
        var model = OperationGroupModel.Build(doc, "test.create");

        Assert.Single(model.Variants);
        Assert.Equal("test.create", model.OperationGroup);
        Assert.Equal("1.0", model.VersionAdded);
        Assert.False(model.HasErrors);
        Assert.Empty(model.Diagnostics);

        // Single path and method
        Assert.Single(model.Paths);
        Assert.Equal("/_test", model.Paths[0]);
        Assert.Single(model.HttpMethods);
        Assert.Equal("post", model.HttpMethods[0]);
    }

    [Fact]
    public async Task MultiplePathsAggregatesPathParameters()
    {
        var doc = await OpenApiDocument.FromJsonAsync(MultiplePathsSpec);
        var model = OperationGroupModel.Build(doc, "test.action");

        Assert.Equal(3, model.Variants.Count);
        Assert.Equal(3, model.Paths.Count);

        // Path parameters: 'id' in all variants, 'name' in 2 of 3
        Assert.Equal(2, model.PathParameters.Count);

        var idParam = model.PathParameters.First(p => p.Name == "id");
        Assert.True(idParam.IsRequired); // Present and required in all 3

        var nameParam = model.PathParameters.First(p => p.Name == "name");
        Assert.False(nameParam.IsRequired); // Only in 2 of 3 variants
    }

    [Fact]
    public async Task PathParameterRequirednessComputedByIntersection()
    {
        var doc = await OpenApiDocument.FromJsonAsync(PathParamRequirednessSpec);
        var model = OperationGroupModel.Build(doc, "test.action");

        // 'index' is in all variants, required in all -> required
        // 'doc_id' is in only 1 variant -> not required
        var indexParam = model.PathParameters.First(p => p.Name == "index");
        Assert.True(indexParam.IsRequired);

        var docIdParam = model.PathParameters.First(p => p.Name == "doc_id");
        Assert.False(docIdParam.IsRequired);
    }

    [Fact]
    public async Task QueryParametersAggregatedAsUnion()
    {
        var doc = await OpenApiDocument.FromJsonAsync(QueryParamUnionSpec);
        var model = OperationGroupModel.Build(doc, "test.search");

        // Query params from both variants
        Assert.Equal(3, model.QueryParameters.Count);
        Assert.Contains(model.QueryParameters, p => p.Name == "q");
        Assert.Contains(model.QueryParameters, p => p.Name == "size");
        Assert.Contains(model.QueryParameters, p => p.Name == "timeout");

        // 'q' is required in both -> required
        var qParam = model.QueryParameters.First(p => p.Name == "q");
        Assert.True(qParam.IsRequired);

        // 'size' is required only in one variant -> not required
        var sizeParam = model.QueryParameters.First(p => p.Name == "size");
        Assert.False(sizeParam.IsRequired);

        // 'timeout' is optional in both -> not required
        var timeoutParam = model.QueryParameters.First(p => p.Name == "timeout");
        Assert.False(timeoutParam.IsRequired);
    }

    [Fact]
    public async Task Collects200And201Responses()
    {
        var doc = await OpenApiDocument.FromJsonAsync(MultipleSuccessResponsesSpec);
        var model = OperationGroupModel.Build(doc, "test.create");

        Assert.Equal(2, model.SuccessResponses.Count);
        Assert.True(model.SuccessResponses.ContainsKey("200"));
        Assert.True(model.SuccessResponses.ContainsKey("201"));

        // Primary is 200. The distinct 201 schema is retained and diagnosed because
        // OperationModel can render only one response type.
        var primary = model.PrimarySuccessResponse;
        Assert.NotNull(primary);
        Assert.Equal(model.SuccessResponses["200"], primary);
        Assert.Contains(model.Diagnostics, d => d.Code == "INCOMPATIBLE_RESPONSE");
    }

    [Fact]
    public async Task Non200PrimaryWhenNo200Exists()
    {
        var doc = await OpenApiDocument.FromJsonAsync(Only201ResponseSpec);
        var model = OperationGroupModel.Build(doc, "test.create");

        Assert.Single(model.SuccessResponses);
        Assert.True(model.SuccessResponses.ContainsKey("201"));

        var primary = model.PrimarySuccessResponse;
        Assert.NotNull(primary);
        Assert.Equal(model.SuccessResponses["201"], primary);
    }

    [Fact]
    public async Task CollectsBodyless204Response()
    {
        var doc = await OpenApiDocument.FromJsonAsync(Only204ResponseSpec);
        var model = OperationGroupModel.Build(doc, "test.delete");

        var response = Assert.Single(model.SuccessResponses).Value;
        Assert.Equal("204", response.StatusCode);
        Assert.Null(response.Schema);
        Assert.Null(model.PrimarySuccessResponse);
        Assert.False(model.HasErrors);
    }

    [Fact]
    public async Task EquivalentResponseSchemasNoDiagnostic()
    {
        var doc = await OpenApiDocument.FromJsonAsync(EquivalentResponsesSpec);
        var model = OperationGroupModel.Build(doc, "test.action");

        Assert.Equal(2, model.Variants.Count);
        Assert.False(model.HasErrors);
        Assert.DoesNotContain(model.Diagnostics, d => d.Severity == DiagnosticSeverity.Error);
    }

    [Fact]
    public async Task IncompatibleResponseSchemasDiagnosesError()
    {
        var doc = await OpenApiDocument.FromJsonAsync(IncompatibleResponsesSpec);
        var model = OperationGroupModel.Build(doc, "test.action");

        Assert.True(model.HasErrors);
        var error = Assert.Single(model.Diagnostics, d => d.Severity == DiagnosticSeverity.Error);
        Assert.Equal("INCOMPATIBLE_RESPONSE", error.Code);
        Assert.Contains("Incompatible 200 response", error.Message);
    }

    [Fact]
    public async Task IncompatibleRequestBodyDiagnosesError()
    {
        var doc = await OpenApiDocument.FromJsonAsync(IncompatibleRequestBodySpec);
        var model = OperationGroupModel.Build(doc, "test.action");

        Assert.True(model.HasErrors);
        var error = Assert.Single(model.Diagnostics, d => d.Code == "INCOMPATIBLE_REQUEST_BODY");
        Assert.Contains("Incompatible request body", error.Message);
    }

    [Fact]
    public async Task VariantWithNoBodyIsAllowed()
    {
        var doc = await OpenApiDocument.FromJsonAsync(MixedBodySpec);
        var model = OperationGroupModel.Build(doc, "test.action");

        Assert.Equal(2, model.Variants.Count);
        Assert.NotNull(model.RequestBodySchema); // Uses the one from POST
        Assert.False(model.HasErrors);
    }

    [Fact]
    public void NonExistentOperationGroupThrows()
    {
        var doc = new OpenApiDocument();
        Assert.Throws<InvalidOperationException>(() =>
            OperationGroupModel.Build(doc, "nonexistent.operation"));
    }

    [Fact]
    public async Task MultipleMethods_GET_POST_PUT()
    {
        var doc = await OpenApiDocument.FromJsonAsync(MultipleMethodsSpec);
        var model = OperationGroupModel.Build(doc, "test.action");

        Assert.Equal(3, model.Variants.Count);
        Assert.Equal(3, model.HttpMethods.Count);
        Assert.Contains("get", model.HttpMethods);
        Assert.Contains("post", model.HttpMethods);
        Assert.Contains("put", model.HttpMethods);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // Test Specifications
    // ────────────────────────────────────────────────────────────────────────────

    private const string SingleVariantSpec = """
        {
          "openapi": "3.0.1",
          "info": { "title": "Test", "version": "1.0.0" },
          "paths": {
            "/_test": {
              "post": {
                "operationId": "test.create.0",
                "x-operation-group": "test.create",
                "x-version-added": "1.0",
                "parameters": [
                  { "name": "timeout", "in": "query", "schema": { "type": "string" } }
                ],
                "requestBody": {
                  "content": {
                    "application/json": {
                      "schema": { "$ref": "#/components/schemas/test___CreateRequest" }
                    }
                  }
                },
                "responses": {
                  "200": {
                    "content": {
                      "application/json": {
                        "schema": { "$ref": "#/components/schemas/test___CreateResponse" }
                      }
                    }
                  }
                }
              }
            }
          },
          "components": {
            "schemas": {
              "test___CreateRequest": { "type": "object", "properties": { "name": { "type": "string" } } },
              "test___CreateResponse": { "type": "object", "properties": { "id": { "type": "string" } } }
            }
          }
        }
        """;

    private const string MultiplePathsSpec = """
        {
          "openapi": "3.0.1",
          "info": { "title": "Test", "version": "1.0.0" },
          "paths": {
            "/_test/{id}": {
              "get": {
                "operationId": "test.action.0",
                "x-operation-group": "test.action",
                "parameters": [
                  { "name": "id", "in": "path", "required": true, "schema": { "type": "string" } }
                ],
                "responses": { "200": { "content": { "application/json": { "schema": { "type": "object" } } } } }
              }
            },
            "/_test/{id}/{name}": {
              "get": {
                "operationId": "test.action.1",
                "x-operation-group": "test.action",
                "parameters": [
                  { "name": "id", "in": "path", "required": true, "schema": { "type": "string" } },
                  { "name": "name", "in": "path", "required": true, "schema": { "type": "string" } }
                ],
                "responses": { "200": { "content": { "application/json": { "schema": { "type": "object" } } } } }
              }
            },
            "/_test_alias/{id}/{name}": {
              "get": {
                "operationId": "test.action.2",
                "x-operation-group": "test.action",
                "parameters": [
                  { "name": "id", "in": "path", "required": true, "schema": { "type": "string" } },
                  { "name": "name", "in": "path", "required": true, "schema": { "type": "string" } }
                ],
                "responses": { "200": { "content": { "application/json": { "schema": { "type": "object" } } } } }
              }
            }
          },
          "components": { "schemas": {} }
        }
        """;

    private const string PathParamRequirednessSpec = """
        {
          "openapi": "3.0.1",
          "info": { "title": "Test", "version": "1.0.0" },
          "paths": {
            "/{index}": {
              "get": {
                "operationId": "test.action.0",
                "x-operation-group": "test.action",
                "parameters": [
                  { "name": "index", "in": "path", "required": true, "schema": { "type": "string" } }
                ],
                "responses": { "200": { "content": { "application/json": { "schema": { "type": "object" } } } } }
              }
            },
            "/{index}/{doc_id}": {
              "get": {
                "operationId": "test.action.1",
                "x-operation-group": "test.action",
                "parameters": [
                  { "name": "index", "in": "path", "required": true, "schema": { "type": "string" } },
                  { "name": "doc_id", "in": "path", "required": true, "schema": { "type": "string" } }
                ],
                "responses": { "200": { "content": { "application/json": { "schema": { "type": "object" } } } } }
              }
            }
          },
          "components": { "schemas": {} }
        }
        """;

    private const string QueryParamUnionSpec = """
        {
          "openapi": "3.0.1",
          "info": { "title": "Test", "version": "1.0.0" },
          "paths": {
            "/_search": {
              "get": {
                "operationId": "test.search.0",
                "x-operation-group": "test.search",
                "parameters": [
                  { "name": "q", "in": "query", "required": true, "schema": { "type": "string" } },
                  { "name": "size", "in": "query", "required": true, "schema": { "type": "integer" } }
                ],
                "responses": { "200": { "content": { "application/json": { "schema": { "type": "object" } } } } }
              },
              "post": {
                "operationId": "test.search.1",
                "x-operation-group": "test.search",
                "parameters": [
                  { "name": "q", "in": "query", "required": true, "schema": { "type": "string" } },
                  { "name": "size", "in": "query", "required": false, "schema": { "type": "integer" } },
                  { "name": "timeout", "in": "query", "required": false, "schema": { "type": "string" } }
                ],
                "responses": { "200": { "content": { "application/json": { "schema": { "type": "object" } } } } }
              }
            }
          },
          "components": { "schemas": {} }
        }
        """;

    private const string MultipleSuccessResponsesSpec = """
        {
          "openapi": "3.0.1",
          "info": { "title": "Test", "version": "1.0.0" },
          "paths": {
            "/_test": {
              "post": {
                "operationId": "test.create.0",
                "x-operation-group": "test.create",
                "requestBody": { "content": { "application/json": { "schema": { "type": "object" } } } },
                "responses": {
                  "200": { "content": { "application/json": { "schema": { "$ref": "#/components/schemas/test___Response" } } } },
                  "201": { "content": { "application/json": { "schema": { "$ref": "#/components/schemas/test___CreatedResponse" } } } }
                }
              }
            }
          },
          "components": {
            "schemas": {
              "test___Response": { "type": "object", "properties": { "id": { "type": "string" } } },
              "test___CreatedResponse": { "type": "object", "properties": { "id": { "type": "string" }, "created": { "type": "boolean" } } }
            }
          }
        }
        """;

    private const string Only201ResponseSpec = """
        {
          "openapi": "3.0.1",
          "info": { "title": "Test", "version": "1.0.0" },
          "paths": {
            "/_test": {
              "post": {
                "operationId": "test.create.0",
                "x-operation-group": "test.create",
                "requestBody": { "content": { "application/json": { "schema": { "type": "object" } } } },
                "responses": {
                  "201": { "content": { "application/json": { "schema": { "$ref": "#/components/schemas/test___Response" } } } }
                }
              }
            }
          },
          "components": {
            "schemas": {
              "test___Response": { "type": "object", "properties": { "id": { "type": "string" } } }
            }
          }
        }
        """;

    private const string Only204ResponseSpec = """
        {
          "openapi": "3.0.1",
          "info": { "title": "Test", "version": "1.0.0" },
          "paths": {
            "/_test": {
              "delete": {
                "operationId": "test.delete.0",
                "x-operation-group": "test.delete",
                "responses": {
                  "204": { "description": "Deleted" }
                }
              }
            }
          },
          "components": { "schemas": {} }
        }
        """;

    private const string EquivalentResponsesSpec = """
        {
          "openapi": "3.0.1",
          "info": { "title": "Test", "version": "1.0.0" },
          "paths": {
            "/_test": {
              "get": {
                "operationId": "test.action.0",
                "x-operation-group": "test.action",
                "responses": {
                  "200": { "content": { "application/json": { "schema": { "$ref": "#/components/schemas/test___Response" } } } }
                }
              },
              "post": {
                "operationId": "test.action.1",
                "x-operation-group": "test.action",
                "requestBody": { "content": { "application/json": { "schema": { "type": "object" } } } },
                "responses": {
                  "200": { "content": { "application/json": { "schema": { "$ref": "#/components/schemas/test___Response" } } } }
                }
              }
            }
          },
          "components": {
            "schemas": {
              "test___Response": { "type": "object", "properties": { "id": { "type": "string" } } }
            }
          }
        }
        """;

    private const string IncompatibleResponsesSpec = """
        {
          "openapi": "3.0.1",
          "info": { "title": "Test", "version": "1.0.0" },
          "paths": {
            "/_test": {
              "get": {
                "operationId": "test.action.0",
                "x-operation-group": "test.action",
                "responses": {
                  "200": { "content": { "application/json": { "schema": { "$ref": "#/components/schemas/test___ResponseA" } } } }
                }
              },
              "post": {
                "operationId": "test.action.1",
                "x-operation-group": "test.action",
                "requestBody": { "content": { "application/json": { "schema": { "type": "object" } } } },
                "responses": {
                  "200": { "content": { "application/json": { "schema": { "$ref": "#/components/schemas/test___ResponseB" } } } }
                }
              }
            }
          },
          "components": {
            "schemas": {
              "test___ResponseA": { "type": "object", "properties": { "id": { "type": "string" } } },
              "test___ResponseB": { "type": "object", "properties": { "result": { "type": "string" } } }
            }
          }
        }
        """;

    private const string IncompatibleRequestBodySpec = """
        {
          "openapi": "3.0.1",
          "info": { "title": "Test", "version": "1.0.0" },
          "paths": {
            "/_test": {
              "post": {
                "operationId": "test.action.0",
                "x-operation-group": "test.action",
                "requestBody": { "content": { "application/json": { "schema": { "$ref": "#/components/schemas/test___BodyA" } } } },
                "responses": { "200": { "content": { "application/json": { "schema": { "type": "object" } } } } }
              },
              "put": {
                "operationId": "test.action.1",
                "x-operation-group": "test.action",
                "requestBody": { "content": { "application/json": { "schema": { "$ref": "#/components/schemas/test___BodyB" } } } },
                "responses": { "200": { "content": { "application/json": { "schema": { "type": "object" } } } } }
              }
            }
          },
          "components": {
            "schemas": {
              "test___BodyA": { "type": "object", "properties": { "name": { "type": "string" } } },
              "test___BodyB": { "type": "object", "properties": { "title": { "type": "string" } } }
            }
          }
        }
        """;

    private const string MixedBodySpec = """
        {
          "openapi": "3.0.1",
          "info": { "title": "Test", "version": "1.0.0" },
          "paths": {
            "/_test": {
              "get": {
                "operationId": "test.action.0",
                "x-operation-group": "test.action",
                "responses": { "200": { "content": { "application/json": { "schema": { "type": "object" } } } } }
              },
              "post": {
                "operationId": "test.action.1",
                "x-operation-group": "test.action",
                "requestBody": { "content": { "application/json": { "schema": { "$ref": "#/components/schemas/test___Body" } } } },
                "responses": { "200": { "content": { "application/json": { "schema": { "type": "object" } } } } }
              }
            }
          },
          "components": {
            "schemas": {
              "test___Body": { "type": "object", "properties": { "name": { "type": "string" } } }
            }
          }
        }
        """;

    private const string MultipleMethodsSpec = """
        {
          "openapi": "3.0.1",
          "info": { "title": "Test", "version": "1.0.0" },
          "paths": {
            "/_test": {
              "get": {
                "operationId": "test.action.0",
                "x-operation-group": "test.action",
                "responses": { "200": { "content": { "application/json": { "schema": { "type": "object" } } } } }
              },
              "post": {
                "operationId": "test.action.1",
                "x-operation-group": "test.action",
                "requestBody": { "content": { "application/json": { "schema": { "type": "object" } } } },
                "responses": { "200": { "content": { "application/json": { "schema": { "type": "object" } } } } }
              },
              "put": {
                "operationId": "test.action.2",
                "x-operation-group": "test.action",
                "requestBody": { "content": { "application/json": { "schema": { "type": "object" } } } },
                "responses": { "200": { "content": { "application/json": { "schema": { "type": "object" } } } } }
              }
            }
          },
          "components": { "schemas": {} }
        }
        """;
}
