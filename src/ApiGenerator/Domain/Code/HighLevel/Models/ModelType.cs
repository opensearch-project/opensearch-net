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
        FluentMethodNameOverride ?? NamingConventions.ToPascal(Key.Replace("-", "_"));

    // ── Policy-driven overrides (populated by NamespaceModel when policy exists) ──

    /// <summary>Override for the fluent method name on the list descriptor.</summary>
    public string? FluentMethodNameOverride { get; init; }

    /// <summary>Override for the variant interface name (without "I" prefix).</summary>
    public string? InterfaceNameOverride { get; init; }

    /// <summary>Effective interface name: override or default.</summary>
    public string InterfaceName => InterfaceNameOverride ?? CsharpName;

    /// <summary>Base class for this variant's concrete class (e.g. "ProcessorBase").</summary>
    public string? BaseClass { get; init; }

    /// <summary>Descriptor base class pattern for this variant.</summary>
    public string? DescriptorBasePattern { get; init; }

    /// <summary>Whether this variant is generic with &lt;T&gt; where T : class.</summary>
    public bool IsGenericDescriptor { get; init; }

    /// <summary>Whether this variant is retained (hand-written, not generated).</summary>
    public bool IsRetained { get; init; }

    /// <summary>Additional interface this variant's interface inherits from.</summary>
    public string? AdditionalInterface { get; init; }

    /// <summary>Properties that are typed as Field and get Expression overloads.</summary>
    public IReadOnlySet<string> FieldProperties { get; init; } =
        new HashSet<string>(StringComparer.Ordinal);

    /// <summary>Property aliases for backwards compatibility. Key: alias name, Value: canonical wire name.</summary>
    public IDictionary<string, string> PropertyAliases { get; init; } =
        new Dictionary<string, string>(StringComparer.Ordinal);

    /// <summary>Wire names of properties that get params array overloads.</summary>
    public ISet<string> ParamsOverloads { get; init; } =
        new HashSet<string>(StringComparer.Ordinal);

    /// <summary>Wire names of IList&lt;string&gt; properties that get scalar string overloads.</summary>
    public ISet<string> ScalarStringOverloads { get; init; } =
        new HashSet<string>(StringComparer.Ordinal);

    /// <summary>Wire names of IProcessor properties that get Func&lt;ProcessorsDescriptor,...&gt; overloads.</summary>
    public ISet<string> ProcessorLambdaOverloads { get; init; } =
        new HashSet<string>(StringComparer.Ordinal);

    /// <summary>Wire names of IDictionary properties that get Func&lt;FluentDictionary,...&gt; overloads.</summary>
    public ISet<string> DictionaryLambdaOverloads { get; init; } =
        new HashSet<string>(StringComparer.Ordinal);

    /// <summary>C# property type overrides keyed by OpenAPI wire property name.</summary>
    public IDictionary<string, string> PropertyTypeOverrides { get; init; } =
        new Dictionary<string, string>(StringComparer.Ordinal);

    /// <summary>Wire names of Fields properties that get FieldsDescriptor selector overloads.</summary>
    public ISet<string> FieldsSelectorOverloads { get; init; } =
        new HashSet<string>(StringComparer.Ordinal);
}

/// <summary>
/// A wrapper-key discriminated union (e.g. <c>RequestProcessor</c>, <c>ProcessorContainer</c>).
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
    public string FormatterName => RenderingPolicy?.FormatterName ?? CsharpName + "Formatter";
    public string DescriptorBuilderName => RenderingPolicy?.ListDescriptorName ?? CsharpName + "sDescriptor";

    /// <summary>Resolved rendering policy (null for simple/default rendering).</summary>
    public Configuration.Overrides.UnionRenderingPolicy? RenderingPolicy { get; init; }

    /// <summary>Effective base interface name.</summary>
    public string EffectiveInterfaceName => RenderingPolicy?.BaseInterfaceName ?? "I" + CsharpName;

    /// <summary>Whether the base interface generation is suppressed (hand-written).</summary>
    public bool SuppressBaseInterface => RenderingPolicy?.SuppressBaseInterfaceGeneration ?? false;

    /// <summary>All variants INCLUDING retained ones (for formatter/descriptor dispatch).</summary>
    public IReadOnlyList<WrapperKeyVariant> AllVariants => Variants;

    /// <summary>Only the variants that should be generated (not retained).</summary>
    public IReadOnlyList<WrapperKeyVariant> GeneratedVariants =>
        Variants.Where(v => !v.IsRetained).ToList();
}
