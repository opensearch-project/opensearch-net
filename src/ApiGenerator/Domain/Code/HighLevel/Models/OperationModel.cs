/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using ApiGenerator.Configuration.Overrides;
using ApiGenerator.Generator;
using NJsonSchema;
using NSwag;

namespace ApiGenerator.Domain.Code.HighLevel.Models;

/// <summary>
/// Resolves the request-body and success-response types for a single OpenAPI operation
/// (identified by its <c>x-operation-group</c>), plus any string enums those types reference.
/// The request/response are rendered as partials/response-classes, not the full model triple.
/// </summary>
public sealed class OperationModel
{
    public RequestModel Request { get; init; } = null!;
    public ResponseModel Response { get; init; } = null!;
    public IReadOnlyList<EnumModel> ReferencedEnums { get; init; } = new List<EnumModel>();

    public static OperationModel Build(
        OpenApiDocument doc,
        string operationGroup,
        string requestCsharpName,
        string responseCsharpName,
        IModelOverrides registry,
        ModelTypeResolver resolver)
    {
        var operation = doc.Paths.Values
            .SelectMany(pathItem => pathItem.Values)
            .FirstOrDefault(op =>
                op.ExtensionData != null
                && op.ExtensionData.TryGetValue("x-operation-group", out var g)
                && string.Equals(g?.ToString(), operationGroup, StringComparison.Ordinal))
            ?? throw new InvalidOperationException($"Operation group '{operationGroup}' not found in spec.");

        var requestSchema = JsonContent(operation.ActualRequestBody?.Content)
            ?? throw new InvalidOperationException($"Operation '{operationGroup}' has no JSON request body.");

        // URL path parameters are already emitted by the high-level request generator.
        // Skip any request-body property whose wire name matches a path parameter to avoid
        // generating a duplicate member on the merged partial interface (e.g. model_id declared
        // both as Id from the URL and as string from the request body).
        var pathParamNames = CollectPathParameterNames(operation);
        var requestProps = BuildProperties(requestSchema, resolver, skipWireNames: pathParamNames);
        var request = new RequestModel(operationGroup + "___RequestBody", requestCsharpName, requestProps);

        var responseSchema = ResolveSuccessResponseSchema(operation)
            ?? throw new InvalidOperationException($"Operation '{operationGroup}' has no JSON 200 response.");

        // oneOf response: flatten all variants' properties into a single response class.
        var responseProps = responseSchema.OneOf?.Count > 0
            ? FlattenOneOfProperties(responseSchema, resolver)
            : BuildProperties(responseSchema, resolver, skipWireNames: null, isResponse: true);
        var response = new ResponseModel(operationGroup + "___Response", responseCsharpName, responseProps, "ResponseBase");

        var enums = CollectReferencedEnums(new[] { requestSchema, responseSchema }, registry, doc);

        return new OperationModel { Request = request, Response = response, ReferencedEnums = enums };
    }

    // Properties defined on the hand-written WriteResponseBase; subclasses must not redeclare them.
    private static readonly HashSet<string> WriteResponseBasePropertyNames =
        new HashSet<string>(StringComparer.Ordinal)
        {
            "_id", "_index", "_primary_term", "result", "_seq_no", "_shards", "_type", "_version",
            "forced_refresh",
        };

    /// <summary>
    /// Wire names reserved by <c>ResponseBase</c> via <c>[DataMember]</c>:
    ///   <c>status</c>  → <c>int? StatusCode</c>
    ///   <c>error</c>   → <c>Error Error</c>
    /// A generated response property whose wire name matches one of these would collide with
    /// the base member's DataMember binding.  The fix: rename the C# property (e.g. "Status"
    /// → "OperationStatus") while keeping the original wire name in [DataMember(Name=...)],
    /// exactly as <c>ClusterHealthResponse</c> does for "status" vs <c>ResponseBase.StatusCode</c>.
    /// </summary>
    private static readonly Dictionary<string, string> ResponseBaseReservedWireNames =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            // wire name → C# suffix to append when renaming (e.g. "Status" → "OperationStatus")
            { "status", "Operation" },
            { "error",  "Operation" },
        };

    /// <summary>
    /// Builds only the response for a no-body operation.
    /// Automatically detects <c>WriteResponseBase</c> from the spec schema ref.
    /// </summary>
    public static ResponseModel BuildResponseOnly(
        OpenApiDocument doc,
        string operationGroup,
        string responseCsharpName,
        ModelTypeResolver resolver)
    {
        var operation = doc.Paths.Values
            .SelectMany(pathItem => pathItem.Values)
            .FirstOrDefault(op =>
                op.ExtensionData != null
                && op.ExtensionData.TryGetValue("x-operation-group", out var g)
                && string.Equals(g?.ToString(), operationGroup, StringComparison.Ordinal))
            ?? throw new InvalidOperationException($"Operation group '{operationGroup}' not found in spec.");

        var (responseSchema, isWriteResponseBase) = ResolveSuccessResponse(operation);
        var baseClass = isWriteResponseBase ? "WriteResponseBase" : "ResponseBase";

        IReadOnlyList<ModelProperty> responseProps;
        if (responseSchema == null)
        {
            responseProps = new List<ModelProperty>();
        }
        else if (isWriteResponseBase)
        {
            responseProps = BuildProperties(responseSchema, resolver,
                skipWireNames: WriteResponseBasePropertyNames, isResponse: true);
        }
        else
        {
            responseProps = BuildProperties(responseSchema, resolver, skipWireNames: null, isResponse: true);
        }

        return new ResponseModel(operationGroup + "___Response", responseCsharpName,
            responseProps, baseClass);
    }

    /// <summary>
    /// Returns the wire names of all path parameters for the given operation.
    /// These are already generated by the high-level request generator and must not
    /// be re-declared in the request-body partial to avoid duplicate-member errors.
    /// </summary>
    private static HashSet<string> CollectPathParameterNames(OpenApiOperation operation)
    {
        var names = new HashSet<string>(StringComparer.Ordinal);
        if (operation.Parameters == null) return names;
        foreach (var p in operation.ActualParameters)
        {
            if (p.Kind == OpenApiParameterKind.Path)
                names.Add(p.Name);
        }
        return names;
    }

    private static JsonSchema? JsonContent(IDictionary<string, OpenApiMediaType>? content) =>
        content != null && content.TryGetValue("application/json", out var mt)
            ? mt.Schema?.ActualSchema
            : null;

    private static (JsonSchema? Schema, bool IsWriteResponseBase) ResolveSuccessResponse(OpenApiOperation operation)
    {
        var responses = operation.ActualResponses;
        if (responses == null) return (null, false);
        if (!responses.TryGetValue("200", out var resp)) return (null, false);

        if (resp.Content == null || !resp.Content.TryGetValue("application/json", out var mt) || mt.Schema == null)
            return (null, false);

        var refPath = (mt.Schema as NJsonSchema.References.IJsonReference)?.ReferencePath;
        var isWriteBase = refPath?.EndsWith("/_common___WriteResponseBase", StringComparison.Ordinal) == true;
        return (mt.Schema.ActualSchema, isWriteBase);
    }

    private static JsonSchema? ResolveSuccessResponseSchema(OpenApiOperation operation) =>
        ResolveSuccessResponse(operation).Schema;

    private static IReadOnlyList<ModelProperty> BuildProperties(
        JsonSchema schema, ModelTypeResolver resolver,
        HashSet<string>? skipWireNames = null,
        bool isResponse = false)
    {
        var requiredNames = new HashSet<string>(
            schema.RequiredProperties ?? Enumerable.Empty<string>(),
            StringComparer.Ordinal);

        return (schema.Properties ?? new Dictionary<string, JsonSchemaProperty>())
            .Where(p => !p.Key.Contains('.'))
            .Where(p => skipWireNames == null || !skipWireNames.Contains(p.Key))
            .OrderBy(p => p.Key, StringComparer.Ordinal)
            .Select(p =>
            {
                var wireName = p.Key;
                var csharpName = ToPascal(wireName);
                var typeRef = resolver.ResolveTypeRef(p.Value);
                var csharpType = typeRef.ToCsharp();
                var isRequired = requiredNames.Contains(wireName);

                string? jsonFormatterType = null;
                if (isResponse && ResponseBaseReservedWireNames.TryGetValue(wireName, out var prefix))
                {
                    csharpName = prefix + csharpName;
                    if (wireName == "status" && csharpType == "string")
                        jsonFormatterType = "IntStringFormatter";
                }

                return new ModelProperty(
                    WireName: wireName,
                    CsharpName: csharpName,
                    CsharpType: csharpType,
                    Type: typeRef,
                    IsRequired: isRequired,
                    Description: p.Value.ActualSchema.Description,
                    VersionAdded: null,
                    JsonFormatterType: jsonFormatterType);
            })
            .ToList();
    }

    private static IReadOnlyList<EnumModel> CollectReferencedEnums(
        IEnumerable<JsonSchema> roots, IModelOverrides registry, OpenApiDocument doc)
    {
        var enumIds = ModelTypeResolver.BuildEnumSchemaIds(doc);
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var result = new List<EnumModel>();

        foreach (var root in roots)
        foreach (var prop in root.Properties ?? new Dictionary<string, JsonSchemaProperty>())
        {
            var s = prop.Value.ActualSchema;
            if (!s.IsEnum()) continue;
            var id = prop.Value.Reference?.Id ?? s.Reference?.Id;
            if (id == null) enumIds.TryGetValue(s, out id);
            if (id == null) continue;
            if (registry.MappedCsharpType(id) != null) continue;
            if (!seen.Add(id)) continue;

            var members = s.GetEnumValues()
                .Select(v => new EnumMember(v.Value, ToPascal(v.Alias ?? v.Value)))
                .ToList();
            if (members.Count == 0) continue;
            result.Add(new EnumModel(id, ModelTypeResolver.RefToTypeName(id), members));
        }

        return result.OrderBy(t => t.CsharpName, StringComparer.Ordinal).ToList();
    }

    /// <summary>
    /// Flatten all oneOf variants' properties into a single property list.
    /// Each variant's properties are merged; duplicates (by wire name) are kept from the first variant.
    /// All properties are nullable since only one variant's fields will be populated at runtime.
    /// </summary>
    private static IReadOnlyList<ModelProperty> FlattenOneOfProperties(
        JsonSchema schema, ModelTypeResolver resolver)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var props = new List<ModelProperty>();

        foreach (var variant in schema.OneOf)
        {
            var actual = variant.ActualSchema;
            var variantProps = actual.Properties ?? new Dictionary<string, JsonSchemaProperty>();
            foreach (var p in variantProps.OrderBy(p => p.Key, StringComparer.Ordinal))
            {
                if (!seen.Add(p.Key)) continue;
                var typeRef = resolver.ResolveTypeRef(p.Value);
                props.Add(new ModelProperty(
                    WireName: p.Key,
                    CsharpName: ToPascal(p.Key),
                    CsharpType: typeRef.ToCsharp(),
                    Type: typeRef,
                    IsRequired: false,
                    Description: p.Value.ActualSchema.Description,
                    VersionAdded: null));
            }
        }

        return props;
    }

    private static string ToPascal(string name) => NamingConventions.ToPascal(name);
}
