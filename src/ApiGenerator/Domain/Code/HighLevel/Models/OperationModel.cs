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

    /// <summary>
    /// The underlying <see cref="OperationGroupModel"/> with aggregated parameters,
    /// all 2xx responses, and diagnostics. Exposed for advanced consumers.
    /// </summary>
    public OperationGroupModel? GroupModel { get; init; }

    public static OperationModel Build(
        OpenApiDocument doc,
        string operationGroup,
        string requestCsharpName,
        string responseCsharpName,
        IModelOverrides registry,
        ModelTypeResolver resolver,
        NormalizationResult? normalization = null)
    {
        // Build the aggregated operation group model using the shared catalog
        var groupModel = OperationGroupModel.Build(doc, operationGroup, resolver.Schemas);

        var requestSchema = groupModel.RequestBodySchema
            ?? throw new InvalidOperationException($"Operation '{operationGroup}' has no JSON request body.");

        // URL path parameters are already emitted by the high-level request generator.
        // Skip any request-body property whose wire name matches a path parameter to avoid
        // generating a duplicate member on the merged partial interface (e.g. model_id declared
        // both as Id from the URL and as string from the request body).
        var pathParamNames = new HashSet<string>(
            groupModel.PathParameters.Select(p => p.Name),
            StringComparer.Ordinal);
        var requestProps = BuildProperties(requestSchema, resolver, skipWireNames: pathParamNames,
            plugin: registry, operationGroup: operationGroup);
        var versionAdded = groupModel.VersionAdded;
        var request = new RequestModel(operationGroup + "___RequestBody", requestCsharpName, requestProps, versionAdded);

        var responseSchema = groupModel.PrimarySuccessResponse?.Schema;

        // oneOf response: flatten all variants' properties into a single response class.
        // A bodyless success response (for example, 204) produces an empty response model.
        var responseProps = responseSchema == null
            ? new List<ModelProperty>()
            : responseSchema.OneOf?.Count > 0
                ? FlattenOneOfProperties(responseSchema, resolver, normalization)
                : BuildProperties(responseSchema, resolver, skipWireNames: null, isResponse: true);
        var response = new ResponseModel(operationGroup + "___Response", responseCsharpName, responseProps, "ResponseBase", versionAdded);

        var enumRoots = responseSchema == null
            ? new[] { requestSchema }
            : new[] { requestSchema, responseSchema };
        var enums = CollectReferencedEnums(enumRoots, registry, resolver);

        return new OperationModel
        {
            Request = request,
            Response = response,
            ReferencedEnums = enums,
            GroupModel = groupModel
        };
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
    /// Now uses <see cref="OperationGroupModel"/> to collect all 2xx responses.
    /// </summary>
    public static ResponseModel BuildResponseOnly(
        OpenApiDocument doc,
        string operationGroup,
        string responseCsharpName,
        ModelTypeResolver resolver)
    {
        var groupModel = OperationGroupModel.Build(doc, operationGroup, resolver.Schemas);

        var (responseSchema, isWriteResponseBase) = ResolveSuccessResponseFromGroup(groupModel);
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
            responseProps, baseClass, groupModel.VersionAdded);
    }

    /// <summary>
    /// Resolves the primary success response from an OperationGroupModel.
    /// Detects WriteResponseBase from the $ref path.
    /// </summary>
    private static (JsonSchema? Schema, bool IsWriteResponseBase) ResolveSuccessResponseFromGroup(
        OperationGroupModel groupModel)
    {
        var response = groupModel.PrimarySuccessResponse;
        if (response?.Schema == null) return (null, false);

        var isWriteBase = response.ReferencePath?
            .EndsWith("/_common___WriteResponseBase", StringComparison.Ordinal) == true;
        return (response.Schema, isWriteBase);
    }

    private static IReadOnlyList<ModelProperty> BuildProperties(
        JsonSchema schema, ModelTypeResolver resolver,
        HashSet<string>? skipWireNames = null,
        bool isResponse = false,
        IModelOverrides? plugin = null,
        string? operationGroup = null,
        string? bodySchemaId = null)
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

                // Property-level type override: per-plugin (operation-scoped) + global (schema-scoped)
                if (plugin != null && operationGroup != null)
                {
                    var overrideType = plugin.ResolvePropertyTypeOverride(operationGroup, wireName, bodySchemaId);
                    if (overrideType != null)
                        csharpType = overrideType;
                }

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
        IEnumerable<JsonSchema> roots, IModelOverrides registry, ModelTypeResolver resolver)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var result = new List<EnumModel>();

        foreach (var root in roots)
        foreach (var prop in root.Properties ?? new Dictionary<string, JsonSchemaProperty>())
        {
            var s = prop.Value.ActualSchema;
            if (!s.IsEnum()) continue;
            if (!resolver.TryGetSchemaId(prop.Value, out var id)) continue;
            if (registry.MappedCsharpType(id) != null) continue;
            if (!seen.Add(id)) continue;

            var members = s.GetEnumValues()
                .Select(v => new EnumMember(v.Value, ToPascal(v.Alias ?? v.Value)))
                .ToList();
            if (members.Count == 0) continue;
            result.Add(new EnumModel(id, resolver.CsharpTypeName(id), members));
        }

        return result.OrderBy(t => t.CsharpName, StringComparer.Ordinal).ToList();
    }

    /// <summary>
    /// Flatten all oneOf variants' properties into a single property list.
    /// Each variant's properties are merged; duplicates (by wire name) are kept from the first variant.
    /// All properties are nullable since only one variant's fields will be populated at runtime.
    /// </summary>
    private static IReadOnlyList<ModelProperty> FlattenOneOfProperties(
        JsonSchema schema, ModelTypeResolver resolver, NormalizationResult? normalization)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var props = new List<ModelProperty>();

        foreach (var variant in schema.OneOf)
        {
            var actual = variant.ActualSchema;
            // Use normalized effective properties when available so allOf-composed variant
            // properties are included (plain .Properties only covers top-level declarations).
            IReadOnlyDictionary<string, JsonSchema> variantProps;
            if (normalization != null && normalization.TryGetForSchema(actual, out var normalized))
                variantProps = normalized.EffectiveProperties;
            else
                variantProps = actual.Properties != null
                    ? actual.Properties.ToDictionary(p => p.Key, p => (JsonSchema)p.Value, StringComparer.Ordinal)
                    : new Dictionary<string, JsonSchema>(StringComparer.Ordinal);

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
