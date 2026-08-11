/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

#nullable enable

using System.Collections.Generic;

namespace ApiGenerator.Domain.Code.HighLevel.Models;

public sealed record ModelProperty(
    string WireName,
    string CsharpName,
    string CsharpType,
    TypeRef Type,
    bool IsRequired,
    string? Description,
    string? VersionAdded,
    string? JsonFormatterType = null);

/// <summary>A single member of a generated enum: its wire value and PascalCase C# name.</summary>
public sealed record EnumMember(string WireValue, string CsharpName);

/// <summary>
/// Base type for all generated model types. Each subclass maps to a specific Razor template.
/// </summary>
public abstract record ModelType(string SchemaId, string CsharpName)
{
    public string InterfaceName => "I" + CsharpName;
}

/// <summary>
/// A shared object type (interface + class + descriptor).
/// Rendered by <c>Model.cshtml</c>.
/// </summary>
public sealed record ObjectModel(
    string SchemaId,
    string CsharpName,
    IReadOnlyList<ModelProperty> Properties,
    bool AllowAdditionalProperties = false) : ModelType(SchemaId, CsharpName);

/// <summary>
/// A string enum type.
/// Rendered by <c>Model.cshtml</c> (enum branch).
/// </summary>
public sealed record EnumModel(
    string SchemaId,
    string CsharpName,
    IReadOnlyList<EnumMember> Members) : ModelType(SchemaId, CsharpName);

/// <summary>
/// A request body partial (merged into the base-half generated Request class).
/// Rendered by <c>RequestBodyPartial.cshtml</c>.
/// </summary>
public sealed record RequestModel(
    string SchemaId,
    string CsharpName,
    IReadOnlyList<ModelProperty> Properties) : ModelType(SchemaId, CsharpName);

/// <summary>
/// A response class with an explicit base class.
/// Rendered by <c>ResponseType.cshtml</c>.
/// </summary>
public sealed record ResponseModel(
    string SchemaId,
    string CsharpName,
    IReadOnlyList<ModelProperty> Properties,
    string BaseClass) : ModelType(SchemaId, CsharpName);
