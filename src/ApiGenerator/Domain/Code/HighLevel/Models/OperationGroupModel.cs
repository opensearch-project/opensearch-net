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
using ApiGenerator.Generator;
using NJsonSchema;
using NJsonSchema.References;
using NSwag;

namespace ApiGenerator.Domain.Code.HighLevel.Models;

/// <summary>
/// Diagnostic message from operation group analysis.
/// </summary>
public sealed record OperationGroupDiagnostic(
    string OperationGroup,
    DiagnosticSeverity Severity,
    string Code,
    string Message);

public enum DiagnosticSeverity { Warning, Error }

/// <summary>
/// Represents a single HTTP verb/path variant within an operation group.
/// </summary>
public sealed record OperationVariant(
    string Path,
    string HttpMethod,
    string OperationId,
    IReadOnlyList<OperationParameter> Parameters,
    JsonSchema? RequestBodySchema,
    IReadOnlyDictionary<string, OperationSuccessResponse> SuccessResponses);

/// <summary>
/// Aggregated parameter from an operation group (union of all variants).
/// </summary>
public sealed record OperationParameter(
    string Name,
    ParameterLocation Location,
    bool IsRequired,
    JsonSchema Schema,
    string? Description);

public enum ParameterLocation { Path, Query, Header }

/// <summary>
/// Successful response metadata. <see cref="Schema"/> is null for successful responses
/// without a JSON body, such as HTTP 204.
/// </summary>
public sealed record OperationSuccessResponse(
    string StatusCode,
    JsonSchema? Schema,
    string? ReferencePath);

/// <summary>
/// Semantic model for an operation group: all operations sharing the same <c>x-operation-group</c>.
/// Aggregates parameters across variants and collects all successful 2xx responses.
/// </summary>
public sealed class OperationGroupModel
{
    public string OperationGroup { get; }
    public IReadOnlyList<OperationVariant> Variants { get; }
    public IReadOnlyList<OperationParameter> PathParameters { get; }
    public IReadOnlyList<OperationParameter> QueryParameters { get; }
    public IReadOnlyList<OperationParameter> HeaderParameters { get; }
    public JsonSchema? RequestBodySchema { get; }
    public IReadOnlyDictionary<string, OperationSuccessResponse> SuccessResponses { get; }
    public IReadOnlyList<OperationGroupDiagnostic> Diagnostics { get; }
    public string? VersionAdded { get; }

    private OperationGroupModel(
        string operationGroup,
        IReadOnlyList<OperationVariant> variants,
        IReadOnlyList<OperationParameter> pathParameters,
        IReadOnlyList<OperationParameter> queryParameters,
        IReadOnlyList<OperationParameter> headerParameters,
        JsonSchema? requestBodySchema,
        IReadOnlyDictionary<string, OperationSuccessResponse> successResponses,
        IReadOnlyList<OperationGroupDiagnostic> diagnostics,
        string? versionAdded)
    {
        OperationGroup = operationGroup;
        Variants = variants;
        PathParameters = pathParameters;
        QueryParameters = queryParameters;
        HeaderParameters = headerParameters;
        RequestBodySchema = requestBodySchema;
        SuccessResponses = successResponses;
        Diagnostics = diagnostics;
        VersionAdded = versionAdded;
    }

    /// <summary>
    /// Test-only convenience overload that creates an isolated SchemaCatalog.
    /// Production code must pass a shared catalog.
    /// </summary>
    internal static OperationGroupModel Build(OpenApiDocument doc, string operationGroup) =>
        Build(doc, operationGroup, new SchemaCatalog(doc));

    /// <summary>
    /// Builds an <see cref="OperationGroupModel"/> containing every OpenAPI operation
    /// with the specified <c>x-operation-group</c> value. Aggregates parameters,
    /// computes required path parameters by intersection, collects all 2xx responses,
    /// and diagnoses incompatible response schemas.
    /// </summary>
    public static OperationGroupModel Build(OpenApiDocument doc, string operationGroup, SchemaCatalog schemas)
    {
        ArgumentNullException.ThrowIfNull(doc);
        ArgumentException.ThrowIfNullOrWhiteSpace(operationGroup);
        ArgumentNullException.ThrowIfNull(schemas);
        var diagnostics = new List<OperationGroupDiagnostic>();
        var variants = new List<OperationVariant>();
        string? versionAdded = null;

        // Collect all operations with this x-operation-group
        foreach (var (path, pathItem) in doc.Paths)
        {
            foreach (var (method, op) in pathItem)
            {
                if (op.ExtensionData == null
                    || !op.ExtensionData.TryGetValue("x-operation-group", out var g)
                    || !string.Equals(g?.ToString(), operationGroup, StringComparison.Ordinal))
                    continue;

                // Extract version-added from the first variant (they should all match per spec rules)
                versionAdded ??= ExtractVersionAdded(op);

                var variantParams = ExtractParameters(op);
                var requestBody = ExtractRequestBodySchema(op);
                var successResponses = ExtractSuccessResponses(op);

                variants.Add(new OperationVariant(
                    path,
                    method,
                    op.OperationId,
                    variantParams,
                    requestBody,
                    successResponses));
            }
        }

        if (variants.Count == 0)
            throw new InvalidOperationException($"Operation group '{operationGroup}' not found in spec.");

        // Aggregate parameters across all variants
        var (pathParams, queryParams, headerParams) = AggregateParameters(variants);

        // Collect all successful responses, including responses without a JSON body.
        var (successResponseSchemas, responseDiags) = CollectSuccessResponses(variants, operationGroup, schemas);
        diagnostics.AddRange(responseDiags);

        // Pick the canonical request body schema (first variant with a body; diagnose conflicts)
        var (requestBodySchema, bodyDiags) = ResolveRequestBodySchema(variants, operationGroup, schemas);
        diagnostics.AddRange(bodyDiags);

        return new OperationGroupModel(
            operationGroup,
            variants,
            pathParams,
            queryParams,
            headerParams,
            requestBodySchema,
            successResponseSchemas,
            diagnostics,
            versionAdded);
    }

    /// <summary>
    /// Returns true if any diagnostic has Error severity.
    /// </summary>
    public bool HasErrors => Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error);

    /// <summary>
    /// Returns the preferred successful response that has a JSON body: status 200 when
    /// available, otherwise the lowest 2xx status with a body. Returns null when every
    /// successful response is bodyless.
    /// </summary>
    public OperationSuccessResponse? PrimarySuccessResponse =>
        SuccessResponses.Values
            .Where(r => r.Schema != null)
            .OrderBy(r => r.StatusCode == "200" ? 0 : 1)
            .ThenBy(r => r.StatusCode, StringComparer.Ordinal)
            .FirstOrDefault();

    /// <summary>
    /// Returns all supported HTTP methods for this operation group.
    /// </summary>
    public IReadOnlyList<string> HttpMethods =>
        Variants.Select(v => v.HttpMethod).Distinct().OrderBy(m => m).ToList();

    /// <summary>
    /// Returns all supported URL paths for this operation group.
    /// </summary>
    public IReadOnlyList<string> Paths =>
        Variants.Select(v => v.Path).Distinct().OrderBy(p => p, StringComparer.Ordinal).ToList();

    // ────────────────────────────────────────────────────────────────────────────
    // Private helpers
    // ────────────────────────────────────────────────────────────────────────────

    private static string? ExtractVersionAdded(OpenApiOperation op) =>
        op.ExtensionData != null
        && op.ExtensionData.TryGetValue("x-version-added", out var v)
        && v is string s
            ? s
            : null;

    private static IReadOnlyList<OperationParameter> ExtractParameters(OpenApiOperation op)
    {
        if (op.Parameters == null) return Array.Empty<OperationParameter>();

        return op.ActualParameters
            .Select(p => new OperationParameter(
                p.Name,
                MapParameterKind(p.Kind),
                p.IsRequired,
                p.Schema ?? p.ActualSchema,
                p.Description))
            .ToList();
    }

    private static ParameterLocation MapParameterKind(OpenApiParameterKind kind) => kind switch
    {
        OpenApiParameterKind.Path => ParameterLocation.Path,
        OpenApiParameterKind.Query => ParameterLocation.Query,
        OpenApiParameterKind.Header => ParameterLocation.Header,
        _ => ParameterLocation.Query // Default fallback for cookie, body, etc.
    };

    private static JsonSchema? ExtractRequestBodySchema(OpenApiOperation op)
    {
        var content = op.ActualRequestBody?.Content;
        if (content == null) return null;
        if (!content.TryGetValue("application/json", out var mt)) return null;
        return mt.Schema?.ActualSchema;
    }

    private static IReadOnlyDictionary<string, OperationSuccessResponse> ExtractSuccessResponses(
        OpenApiOperation op)
    {
        var result = new Dictionary<string, OperationSuccessResponse>(StringComparer.Ordinal);
        if (op.ActualResponses == null) return result;

        foreach (var (status, response) in op.ActualResponses)
        {
            if (!status.StartsWith("2", StringComparison.Ordinal)) continue;

            JsonSchema? schema = null;
            string? referencePath = null;
            if (response.Content != null
                && response.Content.TryGetValue("application/json", out var mediaType)
                && mediaType.Schema != null)
            {
                referencePath = (mediaType.Schema as IJsonReference)?.ReferencePath
                    ?? mediaType.Schema.Reference?.Id;
                schema = mediaType.Schema.ActualSchema;
            }

            result[status] = new OperationSuccessResponse(status, schema, referencePath);
        }

        return result;
    }

    private static (IReadOnlyList<OperationParameter> Path, IReadOnlyList<OperationParameter> Query, IReadOnlyList<OperationParameter> Header)
        AggregateParameters(IReadOnlyList<OperationVariant> variants)
    {
        // Union of all parameters by name and location
        var allPath = new Dictionary<string, List<OperationParameter>>(StringComparer.Ordinal);
        var allQuery = new Dictionary<string, List<OperationParameter>>(StringComparer.Ordinal);
        var allHeader = new Dictionary<string, List<OperationParameter>>(StringComparer.Ordinal);

        // Track which variants have each path parameter (for required calculation)
        var pathParamVariantCount = new Dictionary<string, int>(StringComparer.Ordinal);
        var pathParamRequiredCount = new Dictionary<string, int>(StringComparer.Ordinal);

        foreach (var variant in variants)
        {
            foreach (var p in variant.Parameters)
            {
                var dict = p.Location switch
                {
                    ParameterLocation.Path => allPath,
                    ParameterLocation.Query => allQuery,
                    ParameterLocation.Header => allHeader,
                    _ => allQuery
                };

                if (!dict.TryGetValue(p.Name, out var list))
                {
                    list = new List<OperationParameter>();
                    dict[p.Name] = list;
                }
                list.Add(p);

                // Track path parameter presence and required-ness across variants
                if (p.Location == ParameterLocation.Path)
                {
                    pathParamVariantCount.TryGetValue(p.Name, out var count);
                    pathParamVariantCount[p.Name] = count + 1;

                    if (p.IsRequired)
                    {
                        pathParamRequiredCount.TryGetValue(p.Name, out var reqCount);
                        pathParamRequiredCount[p.Name] = reqCount + 1;
                    }
                }
            }
        }

        // Build aggregated path parameters
        // Required = present and required in ALL variants that have this parameter
        var pathParams = allPath
            .Select(kv =>
            {
                var first = kv.Value.First();
                var variantCount = pathParamVariantCount.GetValueOrDefault(kv.Key, 0);
                var requiredCount = pathParamRequiredCount.GetValueOrDefault(kv.Key, 0);
                // Required only if present in all variants AND required in all those variants
                var isRequired = variantCount == variants.Count && requiredCount == variantCount;
                return new OperationParameter(
                    kv.Key, ParameterLocation.Path, isRequired,
                    first.Schema, first.Description);
            })
            .OrderBy(p => p.Name, StringComparer.Ordinal)
            .ToList();

        // Query parameters: union with OR semantics (if any variant has it, include it; required only if ALL have it required)
        var queryParams = AggregateUnionParameters(allQuery, variants.Count)
            .OrderBy(p => p.Name, StringComparer.Ordinal)
            .ToList();

        var headerParams = AggregateUnionParameters(allHeader, variants.Count)
            .OrderBy(p => p.Name, StringComparer.Ordinal)
            .ToList();

        return (pathParams, queryParams, headerParams);
    }

    private static IEnumerable<OperationParameter> AggregateUnionParameters(
        Dictionary<string, List<OperationParameter>> paramsByName,
        int variantCount)
    {
        foreach (var (name, instances) in paramsByName)
        {
            var first = instances.First();
            // Required only if ALL variants have this parameter AND all mark it required
            var isRequired = instances.Count == variantCount && instances.All(p => p.IsRequired);
            yield return new OperationParameter(
                name, first.Location, isRequired,
                first.Schema, first.Description);
        }
    }

    private static (IReadOnlyDictionary<string, OperationSuccessResponse> Responses, IReadOnlyList<OperationGroupDiagnostic> Diagnostics)
        CollectSuccessResponses(
            IReadOnlyList<OperationVariant> variants,
            string operationGroup,
            SchemaCatalog schemas)
    {
        var diagnostics = new List<OperationGroupDiagnostic>();
        var responses = variants
            .SelectMany(variant => variant.SuccessResponses.Values
                .Select(response => (variant.OperationId, Response: response)))
            .OrderBy(item => item.Response.StatusCode, StringComparer.Ordinal)
            .ThenBy(item => item.OperationId, StringComparer.Ordinal)
            .ToList();

        var result = new Dictionary<string, OperationSuccessResponse>(StringComparer.Ordinal);
        foreach (var item in responses)
        {
            if (!result.TryGetValue(item.Response.StatusCode, out var existing))
            {
                result[item.Response.StatusCode] = item.Response;
                continue;
            }

            if ((existing.Schema == null) != (item.Response.Schema == null))
            {
                diagnostics.Add(new OperationGroupDiagnostic(
                    operationGroup,
                    DiagnosticSeverity.Error,
                    "INCOMPATIBLE_RESPONSE",
                    $"Incompatible {item.Response.StatusCode} response in '{item.OperationId}': "
                    + "variants disagree on whether the response has a JSON body."));
            }
            else if (existing.Schema != null
                && item.Response.Schema != null
                && !AreSchemasEquivalent(existing.Schema, item.Response.Schema, schemas))
            {
                diagnostics.Add(new OperationGroupDiagnostic(
                    operationGroup,
                    DiagnosticSeverity.Error,
                    "INCOMPATIBLE_RESPONSE",
                    $"Incompatible {item.Response.StatusCode} response schema in '{item.OperationId}'."));
            }
        }

        // OperationModel renders one response type, so every body-bearing successful response
        // must project to the same schema even when the status codes differ.
        var bodyResponses = responses.Where(item => item.Response.Schema != null).ToList();
        if (bodyResponses.Count > 1)
        {
            var canonical = bodyResponses
                .OrderBy(item => item.Response.StatusCode == "200" ? 0 : 1)
                .ThenBy(item => item.Response.StatusCode, StringComparer.Ordinal)
                .First();

            foreach (var item in bodyResponses)
            {
                if (ReferenceEquals(item.Response, canonical.Response)) continue;
                if (AreSchemasEquivalent(canonical.Response.Schema!, item.Response.Schema!, schemas)) continue;

                var alreadyReported = diagnostics.Any(d =>
                    d.Code == "INCOMPATIBLE_RESPONSE"
                    && d.Message.Contains(item.OperationId, StringComparison.Ordinal));
                if (alreadyReported) continue;

                diagnostics.Add(new OperationGroupDiagnostic(
                    operationGroup,
                    DiagnosticSeverity.Error,
                    "INCOMPATIBLE_RESPONSE",
                    $"Incompatible successful response schema for status {item.Response.StatusCode} "
                    + $"in '{item.OperationId}' compared with status {canonical.Response.StatusCode} "
                    + $"in '{canonical.OperationId}'."));
            }
        }

        return (result, diagnostics);
    }

    private static (JsonSchema? Schema, IReadOnlyList<OperationGroupDiagnostic> Diagnostics)
        ResolveRequestBodySchema(
            IReadOnlyList<OperationVariant> variants,
            string operationGroup,
            SchemaCatalog schemas)
    {
        var diagnostics = new List<OperationGroupDiagnostic>();
        JsonSchema? canonical = null;
        string? canonicalOpId = null;

        foreach (var variant in variants)
        {
            if (variant.RequestBodySchema == null) continue;

            if (canonical == null)
            {
                canonical = variant.RequestBodySchema;
                canonicalOpId = variant.OperationId;
            }
            else if (!AreSchemasEquivalent(canonical, variant.RequestBodySchema, schemas))
            {
                diagnostics.Add(new OperationGroupDiagnostic(
                    operationGroup,
                    DiagnosticSeverity.Error,
                    "INCOMPATIBLE_REQUEST_BODY",
                    $"Incompatible request body schema in '{variant.OperationId}' vs canonical from '{canonicalOpId}'"));
            }
        }

        return (canonical, diagnostics);
    }

    private static bool AreSchemasEquivalent(
        JsonSchema left,
        JsonSchema right,
        SchemaCatalog schemas,
        int depth = 0)
    {
        if (ReferenceEquals(left, right)) return true;

        var hasLeftId = schemas.TryGetId(left, out var leftId);
        var hasRightId = schemas.TryGetId(right, out var rightId);
        if (hasLeftId || hasRightId)
            return hasLeftId && hasRightId && string.Equals(leftId, rightId, StringComparison.Ordinal);

        if (depth >= 32
            || left.Type != right.Type
            || !string.Equals(left.Format, right.Format, StringComparison.Ordinal)
            || left.IsNullableRaw != right.IsNullableRaw
            || left.AllowAdditionalProperties != right.AllowAdditionalProperties)
            return false;

        var leftRequired = (left.RequiredProperties ?? Array.Empty<string>())
            .OrderBy(name => name, StringComparer.Ordinal);
        var rightRequired = (right.RequiredProperties ?? Array.Empty<string>())
            .OrderBy(name => name, StringComparer.Ordinal);
        if (!leftRequired.SequenceEqual(rightRequired, StringComparer.Ordinal)) return false;

        var leftEnum = left.Enumeration.Select(value => value?.ToString()).ToList();
        var rightEnum = right.Enumeration.Select(value => value?.ToString()).ToList();
        if (!leftEnum.SequenceEqual(rightEnum, StringComparer.Ordinal)) return false;

        var leftProperties = left.Properties ?? new Dictionary<string, JsonSchemaProperty>();
        var rightProperties = right.Properties ?? new Dictionary<string, JsonSchemaProperty>();
        if (leftProperties.Count != rightProperties.Count) return false;
        foreach (var (name, leftProperty) in leftProperties)
        {
            if (!rightProperties.TryGetValue(name, out var rightProperty)
                || !AreSchemasEquivalent(
                    leftProperty.ActualSchema,
                    rightProperty.ActualSchema,
                    schemas,
                    depth + 1))
                return false;
        }

        if ((left.Item == null) != (right.Item == null)) return false;
        if (left.Item != null
            && !AreSchemasEquivalent(left.Item.ActualSchema, right.Item!.ActualSchema, schemas, depth + 1))
            return false;

        return EquivalentComposition(left.OneOf, right.OneOf, schemas, depth)
            && EquivalentComposition(left.AllOf, right.AllOf, schemas, depth)
            && EquivalentComposition(left.AnyOf, right.AnyOf, schemas, depth);
    }

    private static bool EquivalentComposition(
        ICollection<JsonSchema> left,
        ICollection<JsonSchema> right,
        SchemaCatalog schemas,
        int depth)
    {
        if (left.Count != right.Count) return false;
        return left.Zip(right).All(pair =>
            AreSchemasEquivalent(pair.First.ActualSchema, pair.Second.ActualSchema, schemas, depth + 1));
    }

}
