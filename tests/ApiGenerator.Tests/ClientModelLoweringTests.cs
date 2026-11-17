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
    public async Task TheVendoredModelLowersTheVendoredSpecification()
    {
        var root = LocateGeneratorFolder();
        var (document, _) = await Generator.ApiGenerator.LoadOpenApiDocument(
            Path.Combine(root, "opensearch-openapi.yaml"), default, Path.Combine(root, "opensearch-client-model.json"));
        Assert.True(document.Components.Schemas.ContainsKey("_common.query_dsl___TermsLookup"));
    }

    private static string LocateGeneratorFolder()
    {
        for (var dir = new DirectoryInfo(System.AppContext.BaseDirectory); dir != null; dir = dir.Parent)
        {
            var candidate = Path.Combine(dir.FullName, "src", "ApiGenerator");
            if (File.Exists(Path.Combine(candidate, "opensearch-client-model.json")))
                return candidate;
        }
        throw new FileNotFoundException("Could not locate src/ApiGenerator/opensearch-client-model.json above " + System.AppContext.BaseDirectory);
    }
}
