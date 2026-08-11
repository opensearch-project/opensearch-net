/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApiGenerator.Configuration;
using ApiGenerator.Configuration.Overrides;
using ApiGenerator.Configuration.Overrides.Plugins;
using ApiGenerator.Domain;
using ApiGenerator.Domain.Code.HighLevel.Models;
using NSwag;
using ShellProgressBar;

namespace ApiGenerator.Generator.Razor;

public sealed class ModelsGenerator : RazorGeneratorBase
{
    public override string Title => "OpenSearch.Client models";

    public static readonly IModelOverrides[] EnabledPlugins =
    {
        new MlModelOverrides(),
    };

    public override async Task Generate(RestApiSpec spec, ProgressBar progressBar, CancellationToken token)
    {
        if (spec.Document is null) return;

        foreach (var plugin in EnabledPlugins)
            await GeneratePlugin(spec.Document, spec.ExplicitlyOpenSchemaIds, plugin, progressBar, token);
    }

    private async Task GeneratePlugin(OpenApiDocument doc, HashSet<string> openSchemaIds, IModelOverrides plugin, ProgressBar progressBar, CancellationToken token)
    {
        var resolver = BuildResolver(doc, plugin);
        var ns = NamespaceModel.Build(doc, plugin.Namespace, plugin, resolver, openSchemaIds);

        // Emit shared models/enums
        foreach (var t in ns.TypesToEmit)
        {
            token.ThrowIfCancellationRequested();
            await DoRazor(t, ViewLocations.HighLevel("Model.cshtml"),
                GeneratorLocations.HighLevel(plugin.OutputFolder, t.CsharpName + ".g.cs"), token);
            progressBar.Tick($"Generated {plugin.Namespace} model: {t.CsharpName}");
        }

        if (!plugin.GenerateBodyOps && !plugin.GenerateNonBodyOps) return;

        var emittedEnums = new HashSet<string>(
            ns.TypesToEmit.OfType<EnumModel>().Select(t => t.CsharpName),
            StringComparer.Ordinal);

        // Body operations (request + response + referenced enums)
        if (plugin.GenerateBodyOps)
        {
            foreach (var grp in BodyOpGroups(doc, plugin))
            {
                token.ThrowIfCancellationRequested();
                var baseName = ResolveBaseName(grp, plugin);
                var op = OperationModel.Build(doc, grp, baseName + "Request", baseName + "Response", plugin, resolver);

                await DoRazor(op.Request, ViewLocations.HighLevel("RequestBodyPartial.cshtml"),
                    GeneratorLocations.HighLevel(plugin.OutputFolder, op.Request.CsharpName + ".g.cs"), token);
                progressBar.Tick($"Generated {plugin.Namespace} request: {op.Request.CsharpName}");

                await DoRazor(op.Response, ViewLocations.HighLevel("ResponseType.cshtml"),
                    GeneratorLocations.HighLevel(plugin.OutputFolder, op.Response.CsharpName + ".g.cs"), token);
                progressBar.Tick($"Generated {plugin.Namespace} response: {op.Response.CsharpName}");

                foreach (var e in op.ReferencedEnums)
                {
                    token.ThrowIfCancellationRequested();
                    if (!emittedEnums.Add(e.CsharpName)) continue;
                    await DoRazor(e, ViewLocations.HighLevel("Model.cshtml"),
                        GeneratorLocations.HighLevel(plugin.OutputFolder, e.CsharpName + ".g.cs"), token);
                    progressBar.Tick($"Generated {plugin.Namespace} enum: {e.CsharpName}");
                }
            }
        }

        // Non-body operations (response-only)
        if (plugin.GenerateNonBodyOps)
        {
            foreach (var grp in NonBodyOpGroups(doc, plugin))
            {
                token.ThrowIfCancellationRequested();
                var baseName = ResolveBaseName(grp, plugin);
                var resp = OperationModel.BuildResponseOnly(doc, grp, baseName + "Response", resolver);

                await DoRazor(resp, ViewLocations.HighLevel("ResponseType.cshtml"),
                    GeneratorLocations.HighLevel(plugin.OutputFolder, resp.CsharpName + ".g.cs"), token);
                progressBar.Tick($"Generated {plugin.Namespace} response: {resp.CsharpName}");

                foreach (var e in CollectNewEnums(doc, resp, plugin, emittedEnums))
                {
                    token.ThrowIfCancellationRequested();
                    if (!emittedEnums.Add(e.CsharpName)) continue;
                    await DoRazor(e, ViewLocations.HighLevel("Model.cshtml"),
                        GeneratorLocations.HighLevel(plugin.OutputFolder, e.CsharpName + ".g.cs"), token);
                    progressBar.Tick($"Generated {plugin.Namespace} enum: {e.CsharpName}");
                }
            }
        }
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Model building (public for test access)
    // ──────────────────────────────────────────────────────────────────────────

    public static ModelTypeResolver BuildResolver(OpenApiDocument doc, IModelOverrides plugin) =>
        new(plugin,
            ModelTypeResolver.BuildEnumSchemaIds(doc),
            plugin.UseObjectSchemaIds ? ModelTypeResolver.BuildObjectSchemaIds(doc) : null);

    public static (NamespaceModel Namespace, IReadOnlyList<OperationModel> Ops) BuildMlModels(
        OpenApiDocument doc)
    {
        var plugin = new MlModelOverrides();
        var resolver = BuildResolver(doc, plugin);
        var ns = NamespaceModel.Build(doc, plugin.Namespace, plugin, resolver);

        var ops = new List<OperationModel>();
        foreach (var grp in BodyOpGroups(doc, plugin))
        {
            var baseName = ResolveBaseName(grp, plugin);
            ops.Add(OperationModel.Build(doc, grp, baseName + "Request", baseName + "Response", plugin, resolver));
        }
        return (ns, ops);
    }

    public static (IReadOnlyList<ResponseModel> Responses, IReadOnlyList<EnumModel> Enums)
        BuildMlNonBodyOpResponses(OpenApiDocument doc, HashSet<string> alreadyEmittedEnums)
    {
        var plugin = new MlModelOverrides();
        var resolver = BuildResolver(doc, plugin);

        var responses = new List<ResponseModel>();
        var enums = new List<EnumModel>();

        foreach (var grp in NonBodyOpGroups(doc, plugin))
        {
            var baseName = ResolveBaseName(grp, plugin);
            var resp = OperationModel.BuildResponseOnly(doc, grp, baseName + "Response", resolver);
            responses.Add(resp);
            enums.AddRange(CollectNewEnums(doc, resp, plugin, alreadyEmittedEnums));
        }

        return (responses.OrderBy(r => r.CsharpName, StringComparer.Ordinal).ToList(),
                enums.OrderBy(e => e.CsharpName, StringComparer.Ordinal).ToList());
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Rendering helpers (public for test access)
    // ──────────────────────────────────────────────────────────────────────────

    public static Task<string> RenderType(ModelType type, CancellationToken token) =>
        RenderAsync(ViewLocations.HighLevel("Model.cshtml"), type);

    public static Task<string> RenderRequestBody(ModelType type, CancellationToken token) =>
        RenderAsync(ViewLocations.HighLevel("RequestBodyPartial.cshtml"), type);

    public static Task<string> RenderResponse(ModelType type, CancellationToken token) =>
        RenderAsync(ViewLocations.HighLevel("ResponseType.cshtml"), type);

    public static async Task<IReadOnlyList<string>> WriteToTempDir(
        NamespaceModel model, CancellationToken token)
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "ModelsGeneratorTest_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);

        var paths = new List<string>();
        foreach (var t in model.TypesToEmit)
        {
            token.ThrowIfCancellationRequested();
            var rendered = await RenderAsync(ViewLocations.HighLevel("Model.cshtml"), t);
            var outputPath = Path.Combine(tempDir, t.CsharpName + ".g.cs");
            await File.WriteAllTextAsync(outputPath, rendered, token);
            paths.Add(outputPath);
        }
        return paths;
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Utilities
    // ──────────────────────────────────────────────────────────────────────────

    internal static IReadOnlyList<string> BodyOpGroups(OpenApiDocument doc, IModelOverrides plugin) =>
        doc.Paths.Values.SelectMany(p => p.Values)
            .Where(op => op.ExtensionData != null
                && op.ExtensionData.TryGetValue("x-operation-group", out var g)
                && g?.ToString()?.StartsWith(plugin.Namespace + ".", StringComparison.Ordinal) == true)
            .Where(op => op.ActualRequestBody?.Content?.ContainsKey("application/json") == true)
            .Select(op => op.ExtensionData!["x-operation-group"]!.ToString()!)
            .Where(g => !plugin.ExcludedOps.Contains(g))
            .Distinct()
            .OrderBy(x => x, StringComparer.Ordinal)
            .ToList();

    internal static IReadOnlyList<string> NonBodyOpGroups(OpenApiDocument doc, IModelOverrides plugin) =>
        doc.Paths.Values.SelectMany(p => p.Values)
            .Where(op => op.ExtensionData != null
                && op.ExtensionData.TryGetValue("x-operation-group", out var g)
                && g?.ToString()?.StartsWith(plugin.Namespace + ".", StringComparison.Ordinal) == true)
            .Where(op => op.ActualRequestBody?.Content?.ContainsKey("application/json") != true)
            .Select(op => op.ExtensionData!["x-operation-group"]!.ToString()!)
            .Where(g => !plugin.ExcludedOps.Contains(g))
            .Distinct()
            .OrderBy(x => x, StringComparer.Ordinal)
            .ToList();

    private static IEnumerable<EnumModel> CollectNewEnums(
        OpenApiDocument doc, ResponseModel resp, IModelOverrides plugin, HashSet<string> alreadyEmitted)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var prop in resp.Properties)
        {
            var csharpType = prop.CsharpType.TrimEnd('?');
            if (alreadyEmitted.Contains(csharpType) || !seen.Add(csharpType)) continue;

            foreach (var (id, schema) in doc.Components.Schemas)
            {
                if (!schema.ActualSchema.IsEnum()) continue;
                var name = ModelTypeResolver.RefToTypeName(id);
                if (name != csharpType) continue;
                if (plugin.MappedCsharpType(id) != null) continue;

                var members = schema.ActualSchema.GetEnumValues()
                    .Select(v => new EnumMember(v.Value, EnumToPascal(v.Alias ?? v.Value)))
                    .ToList();
                if (members.Count > 0)
                    yield return new EnumModel(id, name, members);
                break;
            }
        }
    }

    private static string ResolveBaseName(string operationGroup, IModelOverrides plugin) =>
        plugin.OpNameOverrides.TryGetValue(operationGroup, out var name)
            ? name
            : Pascal(operationGroup.Substring(plugin.Namespace.Length + 1));

    private static string Pascal(string snake) => NamingConventions.OperationToPascal(snake);

    private static string EnumToPascal(string name) => NamingConventions.ToPascal(name);
}
