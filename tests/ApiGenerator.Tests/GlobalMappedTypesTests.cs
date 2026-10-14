/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.Linq;
using System.Threading.Tasks;
using ApiGenerator.Configuration.Overrides;
using ApiGenerator.Domain.Code.HighLevel.Models;
using ApiGenerator.Generator.Razor;
using NSwag;
using Xunit;

namespace ApiGenerator.Tests;

/// <summary>
/// Global schema mappings must resolve to C# types that can round-trip through the JSON serializer.
/// </summary>
public class GlobalMappedTypesTests
{
    [Theory]
    [InlineData("name", "Name")]
    [InlineData("worker_nodes", "NodeIds")]
    [InlineData("failures", "IList<BulkIndexByScrollFailure>")]
    [InlineData("limit", "double?")]
    public async Task MappedSchemasResolveToSerializableTypes(string property, string expectedType)
    {
        var prop = await ResolveProperty(property);

        Assert.Equal(expectedType, prop.CsharpType);
    }

    [Fact]
    public async Task StringifiedDoubleCarriesStringTolerantFormatter()
    {
        var prop = await ResolveProperty("limit");

        Assert.Equal("NullableStringDoubleFormatter", prop.EffectiveJsonFormatterType);
    }

    [Fact]
    public async Task MappedFormatterIsDroppedWhenPropertyTypeIsOverridden()
    {
        var prop = await ResolveProperty("limit") with { CsharpType = "string" };

        Assert.Null(prop.EffectiveJsonFormatterType);
    }

    [Theory]
    [InlineData("name")]
    [InlineData("worker_nodes")]
    [InlineData("failures")]
    public async Task OtherMappedSchemasCarryNoFormatter(string property)
    {
        var prop = await ResolveProperty(property);

        Assert.Null(prop.EffectiveJsonFormatterType);
    }

    private static async Task<ModelProperty> ResolveProperty(string wireName)
    {
        var doc = await OpenApiDocument.FromJsonAsync(Spec);
        var plugin = new TestPlugin();
        var resolver = ModelsGenerator.BuildResolverForTest(doc, plugin);
        var resp = OperationModel.BuildResponseOnly(doc, "test.get", "GetResponse", resolver);
        return resp.Properties.Single(p => p.WireName == wireName);
    }

    private sealed class TestPlugin : ModelOverridesBase
    {
        public override string Namespace => "test";
        public override string OutputFolder => "Test";
    }

    private const string Spec = """
        {
          "openapi": "3.0.1",
          "info": { "title": "Test", "version": "1.0.0" },
          "paths": {
            "/_test": {
              "get": {
                "operationId": "test.get.0",
                "x-operation-group": "test.get",
                "responses": {
                  "200": {
                    "content": {
                      "application/json": {
                        "schema": {
                          "type": "object",
                          "properties": {
                            "name": { "$ref": "#/components/schemas/_common___Name" },
                            "worker_nodes": { "type": "array", "items": { "$ref": "#/components/schemas/_common___NodeIds" } },
                            "failures": { "type": "array", "items": { "$ref": "#/components/schemas/_common___BulkByScrollFailure" } },
                            "limit": { "$ref": "#/components/schemas/_common___StringifiedDouble" }
                          }
                        }
                      }
                    }
                  }
                }
              }
            }
          },
          "components": {
            "schemas": {
              "_common___Name": { "type": "string" },
              "_common___NodeId": { "type": "string" },
              "_common___NodeIds": {
                "oneOf": [
                  { "$ref": "#/components/schemas/_common___NodeId" },
                  { "type": "array", "items": { "$ref": "#/components/schemas/_common___NodeId" } }
                ]
              },
              "_common___BulkItemResponseFailure": { "type": "object", "properties": { "index": { "type": "string" } } },
              "_common___ScrollableHitSourceSearchFailure": { "type": "object", "properties": { "shard": { "type": "integer" } } },
              "_common___BulkByScrollFailure": {
                "anyOf": [
                  { "$ref": "#/components/schemas/_common___BulkItemResponseFailure" },
                  { "$ref": "#/components/schemas/_common___ScrollableHitSourceSearchFailure" }
                ]
              },
              "_common___StringifiedDouble": {
                "oneOf": [ { "type": "number", "format": "double" }, { "type": "string" } ]
              }
            }
          }
        }
        """;
}
