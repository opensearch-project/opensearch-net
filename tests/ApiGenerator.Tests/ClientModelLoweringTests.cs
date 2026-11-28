/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ApiGenerator.Generator;
using Newtonsoft.Json.Linq;
using Xunit;

namespace ApiGenerator.Tests;

public class ClientModelLoweringTests
{
    private static JObject Json(string text) => JObject.Parse(text.Replace('\'', '"'));

    private static JObject Lowered()
    {
        var model = Json(
            "{'format':'opensearch-client-model','version':'0.2.0','types':{"
            + "'a___Id':{'type_kind':'alias','type':{'scalar':'string'}},"
            + "'a___Lookup':{'type_kind':'object','fields':{"
            + "'index':{'type':{'scalar':'string'},'required':true},'id':{'type':{'ref':'a___Id'},'required':false}},"
            + "'constraints':[{'union_kind':'presence','one_of':[['id'],['index']]}]},"
            + "'a___Unit':{'type_kind':'union','union_kind':'scalar','exclusive':true,'members':[{'enum':['d']},{'enum':['h']}]},"
            + "'a___Doc':{'type_kind':'object','fields':{'note':{'type':{'union':[{'scalar':'string'},{'scalar':'null'}],"
            + "'union_kind':'scalar','exclusive':true},'required':false}},'constraints':[]}"
            + "},'responses':{},'request_bodies':{}}");
        var spec = Json(
            "{'components':{'schemas':{"
            + "'a___Id':{'type':'string','pattern':'^[a-z]+$','description':'An id.'},"
            + "'a___Unit':{'oneOf':[{'type':'string','const':'d','title':'days'},{'type':'string','const':'h','title':'hours'}]}"
            + "}}}");
        ClientModelLowering.Lower(spec, model);
        return (JObject)spec["components"]!["schemas"]!;
    }

    [Fact]
    public void PresenceConstraintsAreNotWritten()
    {
        var lookup = Lowered()["a___Lookup"]!;
        Assert.Null(lookup["allOf"]);
        Assert.Equal(new[] { "index" }, lookup["required"]!.Select(r => (string)r!));
    }

    [Fact]
    public void ValidationKeywordsAndAnnotationsAreKept()
    {
        var id = Lowered()["a___Id"]!;
        Assert.Equal("^[a-z]+$", (string)id["pattern"]!);
        Assert.Equal("An id.", (string)id["description"]!);
    }

    [Fact]
    public void EnumBranchesKeepTheirConstAndTitle()
    {
        var first = Lowered()["a___Unit"]!["oneOf"]![0]!;
        Assert.Equal("d", (string)first["const"]!);
        Assert.Equal("days", (string)first["title"]!);
    }

    [Fact]
    public void NullableScalarsAreTypeArrays()
    {
        var note = Lowered()["a___Doc"]!["properties"]!["note"]!;
        Assert.Equal(new[] { "string", "null" }, note["type"]!.Select(t => (string)t!));
    }

    [Fact]
    public void TheModelIsReadFromTheXModelExtensions()
    {
        var spec = Json(
            "{'components':{'x-model':{'format':'opensearch-client-model','version':'0.2.0'},"
            + "'schemas':{'a___Id':{'type':'string','x-model':{'type_kind':'alias','type':{'scalar':'string'}}},'a___Plain':{'type':'string'}},"
            + "'responses':{'r':{'content':{'application/json':{'schema':{'type':'object'}}},"
            + "'x-model':{'media_type':'application/json','type':{'ref':'a___Id'}}}},"
            + "'requestBodies':{}}}");
        var model = ClientModelLowering.ReadModel(spec);
        Assert.Equal("0.2.0", (string)model["version"]!);
        Assert.Equal(new[] { "a___Id" }, ((JObject)model["types"]!).Properties().Select(p => p.Name));
        Assert.Equal("alias", (string)model["types"]!["a___Id"]!["type_kind"]!);
        Assert.Equal("a___Id", (string)model["responses"]!["r"]!["type"]!["ref"]!);
        Assert.Empty((JObject)model["request_bodies"]!);

        ClientModelLowering.Lower(spec, model);
        var components = spec["components"]!;
        Assert.Null(components["x-model"]);
        Assert.Null(components["schemas"]!["a___Id"]!["x-model"]);
        Assert.Null(components["responses"]!["r"]!["x-model"]);
        Assert.Equal("#/components/schemas/a___Id", (string)components["responses"]!["r"]!["content"]!["application/json"]!["schema"]!["$ref"]!);
    }

    [Fact]
    public void ASpecificationWithoutAModelIsRejected()
    {
        var error = Assert.Throws<InvalidDataException>(() => ClientModelLowering.ReadModel(Json("{'components':{'schemas':{}}}")));
        Assert.Contains("components.x-model", error.Message);
        Assert.Throws<InvalidDataException>(() => ClientModelLowering.ReadModel(
            Json("{'components':{'x-model':{'format':'opensearch-client-model','version':'9.9.9'}}}")));
    }

    [Fact]
    public void TheXModelExtensionIsNotCopiedOntoLoweredSchemas()
    {
        var lowered = new JObject { ["type"] = "string" };
        ClientModelLowering.MergeAnnotations(lowered, Json("{'type':'string','x-model':{'type_kind':'alias'},'x-version-added':'2.0'}"));
        Assert.Null(lowered["x-model"]);
        Assert.Equal("2.0", (string)lowered["x-version-added"]!);
    }

    [Fact]
    public async Task TheVendoredSpecificationLowersFromItsOwnModel()
    {
        var path = Path.Combine(LocateGeneratorFolder(), "opensearch-openapi.yaml");
        var (document, _) = await Generator.ApiGenerator.LoadOpenApiDocument(path, default, useClientModel: true);
        Assert.True(document.Components.Schemas.ContainsKey("_common.query_dsl___TermsLookup"));
        Assert.False(document.Components.Schemas["_common.query_dsl___TermsLookup"].ExtensionData?.ContainsKey("x-model") == true);
    }

    [Fact]
    public async Task TheVendoredModelKeepsTheFieldOrder()
    {
        var path = Path.Combine(LocateGeneratorFolder(), "opensearch-openapi.yaml");
        var (json, _) = Generator.ApiGenerator.PreprocessRawOpenApiSpec(await File.ReadAllTextAsync(path));
        var model = ClientModelLowering.ReadModel(JObject.Parse(json));
        var fields = (JObject)model["types"]!["_common.query_dsl___TermsLookup"]!["fields"]!;
        Assert.Equal(new[] { "index", "id", "path", "routing", "query", "store" }, fields.Properties().Select(p => p.Name));
    }

    private static string LocateGeneratorFolder()
    {
        for (var dir = new DirectoryInfo(System.AppContext.BaseDirectory); dir != null; dir = dir.Parent)
        {
            var candidate = Path.Combine(dir.FullName, "src", "ApiGenerator");
            if (File.Exists(Path.Combine(candidate, "opensearch-openapi.yaml")))
                return candidate;
        }
        throw new FileNotFoundException("Could not locate src/ApiGenerator/opensearch-openapi.yaml above " + System.AppContext.BaseDirectory);
    }
}
