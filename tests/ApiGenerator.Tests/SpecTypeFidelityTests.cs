/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ApiGenerator.Configuration.Overrides;
using ApiGenerator.Domain.Code.HighLevel.Models;
using ApiGenerator.Generator;
using ApiGenerator.Generator.Razor;
using NJsonSchema;
using NSwag;
using Xunit;

namespace ApiGenerator.Tests;

/// <summary>
/// Checks every property the model generator emits against the real OpenAPI spec: a property the spec
/// types as a number (or a number-or-string "stringified" union) must be a C# numeric type, a boolean
/// must be <c>bool</c>, and an object must not be a C# primitive. Arrays are compared element-wise. Client-side fixtures are built from the generated C# types, so they cannot catch a
/// mapping that is self-consistent but wrong for the wire (e.g. a numeric field generated as <c>string</c>).
/// </summary>
public class SpecTypeFidelityTests
{
    private static readonly HashSet<string> NumericTypes = new(StringComparer.Ordinal)
    {
        "byte", "short", "int", "long", "uint", "ulong", "float", "double", "decimal",
    };

    private static readonly Lazy<Task<(OpenApiDocument Doc, HashSet<string> OpenSchemaIds)>> Spec =
        new(() => Generator.ApiGenerator.LoadOpenApiDocument(LocateSpecFile()));

    public static IEnumerable<object[]> Plugins =>
        ModelsGenerator.EnabledPlugins.Select(p => new object[] { p.Namespace });

    [Theory]
    [MemberData(nameof(Plugins))]
    public async Task GeneratedPropertyTypesMatchSpecPrimitiveKinds(string pluginNamespace)
    {
        var plugin = ModelsGenerator.EnabledPlugins.Single(p => p.Namespace == pluginNamespace);
        var (doc, openSchemaIds) = await Spec.Value;
        var catalog = new SchemaCatalog(doc);
        var resolver = new ModelTypeResolver(plugin, catalog);
        var normalization = new SchemaNormalizer(catalog).Normalize(doc, openSchemaIds);

        var mismatches = new List<string>();

        // Shared models: compare each generated property with the schema's effective property.
        var ns = NamespaceModel.Build(doc, plugin.Namespace, plugin, resolver, normalization);
        foreach (var model in ns.TypesToEmit.OfType<ObjectModel>())
        {
            if (!catalog.TryGetSchema(model.SchemaId, out var schema)) continue;
            var specProps = normalization.TryGetForSchema(schema, out var normalized)
                ? normalized.EffectiveProperties
                : SpecProperties(schema);
            Check(model.CsharpName, model.Properties, specProps, mismatches);
        }

        // Operation request bodies and responses: compare with the operation's request/response schema,
        // including allOf members and flattened oneOf/anyOf variants.
        if (plugin.GenerateBodyOps)
        {
            foreach (var grp in ModelsGenerator.BodyOpGroups(doc, plugin))
            {
                var op = OperationModel.Build(doc, grp, grp + ".Request", grp + ".Response", plugin, resolver, normalization);
                var group = OperationGroupModel.Build(doc, grp, catalog);
                Check(op.Request.CsharpName, op.Request.Properties, SpecProperties(group.RequestBodySchema), mismatches);
                Check(op.Response.CsharpName, op.Response.Properties, SpecProperties(group.PrimarySuccessResponse?.Schema), mismatches);
            }
        }
        if (plugin.GenerateNonBodyOps)
        {
            foreach (var grp in ModelsGenerator.NonBodyOpGroups(doc, plugin))
            {
                var resp = OperationModel.BuildResponseOnly(doc, grp, grp + ".Response", resolver);
                var group = OperationGroupModel.Build(doc, grp, catalog);
                Check(resp.CsharpName, resp.Properties, SpecProperties(group.PrimarySuccessResponse?.Schema), mismatches);
            }
        }

        Assert.True(mismatches.Count == 0,
            $"{mismatches.Count} generated properties do not match the spec's primitive type:\n" + string.Join("\n", mismatches));
    }

    private static void Check(
        string owner, IEnumerable<ModelProperty> generated, IReadOnlyDictionary<string, JsonSchema> specProps,
        List<string> mismatches)
    {
        foreach (var prop in generated)
        {
            if (!specProps.TryGetValue(prop.WireName, out var specSchema)) continue;

            var (spec, actual) = UnwrapArrays(specSchema, prop.CsharpType);
            var expected = Kind(spec);
            actual = actual.TrimEnd('?');
            var ok = expected switch
            {
                SpecKind.Number => NumericTypes.Contains(actual),
                SpecKind.Boolean => actual == "bool",
                SpecKind.Object => actual != "string" && actual != "bool" && !NumericTypes.Contains(actual),
                _ => true,
            };
            if (!ok)
                mismatches.Add($"  {owner}.{prop.CsharpName} (\"{prop.WireName}\"): spec is {expected}, generated as {prop.CsharpType}");
        }
    }

    private enum SpecKind { Other, Number, Boolean, Object }

    private static readonly string[] ListTypes = { "IList<", "IReadOnlyCollection<", "IEnumerable<", "List<" };

    /// <summary>Strips matching array levels from the spec schema and the C# collection type.</summary>
    private static (JsonSchema Spec, string Csharp) UnwrapArrays(JsonSchema spec, string csharp)
    {
        while (true)
        {
            var s = spec.ActualSchema;
            var list = ListTypes.FirstOrDefault(l => csharp.StartsWith(l, StringComparison.Ordinal));
            if (!s.Type.HasFlag(JsonObjectType.Array) || s.Item == null || list == null) return (spec, csharp);
            spec = s.Item;
            csharp = csharp.Substring(list.Length, csharp.Length - list.Length - 1);
        }
    }

    /// <summary>
    /// The primitive kind of a spec schema: a plain number/integer/boolean, or a oneOf/anyOf whose members are
    /// all of that kind or a plain string (the spec's "stringified" encodings, e.g. <c>StringifiedDouble</c>).
    /// </summary>
    private static SpecKind Kind(JsonSchema schema)
    {
        var s = schema.ActualSchema;
        if (s.IsEnum()) return SpecKind.Other;
        if (IsOnly(s, JsonObjectType.Number) || IsOnly(s, JsonObjectType.Integer)) return SpecKind.Number;
        if (IsOnly(s, JsonObjectType.Boolean)) return SpecKind.Boolean;
        if ((s.Properties?.Count ?? 0) > 0) return SpecKind.Object;

        var members = s.OneOf.Concat(s.AnyOf).ToList();
        if (members.Count == 0) return SpecKind.Other;

        var kinds = members.Select(m => IsPlainString(m) ? (SpecKind?)null : Kind(m)).ToList();
        // A union of objects (e.g. BulkByScrollFailure's anyOf of two failure shapes) is still an object.
        if (kinds.All(k => k == SpecKind.Object)) return SpecKind.Object;
        foreach (var kind in new[] { SpecKind.Number, SpecKind.Boolean })
            if (kinds.Contains(kind) && kinds.All(k => k == null || k == kind))
                return kind;
        return SpecKind.Other;
    }

    private static bool IsOnly(JsonSchema s, JsonObjectType type) =>
        s.Type.HasFlag(type)
        && !s.Type.HasFlag(JsonObjectType.String)
        && !s.Type.HasFlag(JsonObjectType.Object)
        && !s.Type.HasFlag(JsonObjectType.Array);

    /// <summary>
    /// An unformatted string: the textual encoding of a number/boolean in a stringified union. A formatted
    /// string (e.g. <c>date-time</c> in an ISO-date-or-epoch-millis union) is a different value, not a quoted number.
    /// </summary>
    private static bool IsPlainString(JsonSchema schema)
    {
        var s = schema.ActualSchema;
        return s.Type.HasFlag(JsonObjectType.String) && string.IsNullOrEmpty(s.Format)
            && !s.IsEnum() && s.OneOf.Count == 0 && s.AnyOf.Count == 0;
    }

    /// <summary>Properties of a schema, including its allOf members and its oneOf/anyOf variants.</summary>
    private static IReadOnlyDictionary<string, JsonSchema> SpecProperties(JsonSchema? schema)
    {
        var result = new Dictionary<string, JsonSchema>(StringComparer.Ordinal);
        if (schema == null) return result;

        void Collect(JsonSchema s, int depth)
        {
            var actual = s.ActualSchema;
            foreach (var (name, prop) in actual.Properties ?? new Dictionary<string, JsonSchemaProperty>())
                result.TryAdd(name, prop);
            if (depth > 3) return;
            foreach (var member in actual.AllOf.Concat(actual.OneOf).Concat(actual.AnyOf))
                Collect(member, depth + 1);
        }

        Collect(schema, 0);
        return result;
    }

    private static string LocateSpecFile()
    {
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir != null; dir = dir.Parent)
        {
            var candidate = Path.Combine(dir.FullName, "src", "ApiGenerator", "opensearch-openapi.yaml");
            if (File.Exists(candidate)) return candidate;
        }
        throw new FileNotFoundException("Could not locate src/ApiGenerator/opensearch-openapi.yaml above " + AppContext.BaseDirectory);
    }
}
