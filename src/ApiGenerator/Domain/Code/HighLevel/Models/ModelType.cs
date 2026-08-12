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
    IReadOnlyList<ModelProperty> Properties,
    string? VersionAdded = null) : ModelType(SchemaId, CsharpName);

/// <summary>
/// A response class with an explicit base class.
/// Rendered by <c>ResponseType.cshtml</c>.
/// </summary>
public sealed record ResponseModel(
    string SchemaId,
    string CsharpName,
    IReadOnlyList<ModelProperty> Properties,
    string BaseClass,
    string? VersionAdded = null) : ModelType(SchemaId, CsharpName);

/// <summary>
/// A single variant in a wrapper-key discriminated union.
/// The wire format is <c>{"key": { ...body... }}</c>.
/// </summary>
public sealed record WrapperKeyVariant(
    string Key,
    string CsharpName,
    string? VersionAdded,
    IReadOnlyList<ModelProperty> BodyProperties)
{
    /// <summary>PascalCase method name for the fluent descriptor builder.</summary>
    public string FluentMethodName =>
        NamingConventions.ToPascal(Key.Replace("-", "_"));
}

/// <summary>
/// A wrapper-key discriminated union (e.g. <c>RequestProcessor</c>, <c>ResponseProcessor</c>).
/// Each variant is <c>{"discriminator_key": { ...body... }}</c>.
/// Rendered by <c>WrapperKeyUnion.cshtml</c> which emits:
/// - A base interface with <c>string Name { get; }</c> and shared base properties (tag, description, ignore_failure).
/// - One concrete class + descriptor per variant.
/// - An <c>IJsonFormatter</c> using <c>AutomataDictionary</c> dispatch.
/// - A <c>*sDescriptor</c> fluent list builder.
/// </summary>
public sealed record WrapperKeyUnionModel(
    string SchemaId,
    string CsharpName,
    IReadOnlyList<WrapperKeyVariant> Variants,
    IReadOnlyList<ModelProperty> BaseProperties) : ModelType(SchemaId, CsharpName)
{
    public string FormatterName => CsharpName + "Formatter";
    public string DescriptorBuilderName => CsharpName + "sDescriptor";
}
