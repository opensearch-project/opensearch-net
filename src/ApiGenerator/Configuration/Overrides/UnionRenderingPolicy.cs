/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

#nullable enable

using System;
using System.Collections.Generic;

namespace ApiGenerator.Configuration.Overrides;

/// <summary>
/// A generic, namespace-agnostic policy that describes how a wrapper-key union should be rendered.
/// This is pure C# rendering policy — no structural OpenAPI information, no namespace conditionals.
/// When null/absent, the WrapperKeyUnion.cshtml template uses the simple default rendering
/// (flat DescriptorBase, no generics, no Field expressions).
/// </summary>
public sealed class UnionRenderingPolicy
{
    // ── Union-level naming ────────────────────────────────────────────────────

    /// <summary>
    /// Override the generated base interface name. Default: <c>I{CsharpName}</c>.
    /// Example: <c>"IProcessor"</c> for the ingest processor union.
    /// </summary>
    public string? BaseInterfaceName { get; init; }

    /// <summary>
    /// Override the formatter class name. Default: <c>{CsharpName}Formatter</c>.
    /// Example: <c>"ProcessorFormatter"</c>.
    /// </summary>
    public string? FormatterName { get; init; }

    /// <summary>
    /// Override the fluent list descriptor class name. Default: <c>{CsharpName}sDescriptor</c>.
    /// Example: <c>"ProcessorsDescriptor"</c>.
    /// </summary>
    public string? ListDescriptorName { get; init; }

    // ── Variant base class ───────────────────────────────────────────────────

    /// <summary>
    /// The base class that variant concrete classes inherit (instead of nothing).
    /// Example: <c>"ProcessorBase"</c>. Null means no base class (struct-like variants).
    /// </summary>
    public string? VariantBaseClass { get; init; }

    /// <summary>
    /// The base interface that the union's base interface extends (instead of standalone).
    /// When set, the generated base interface inherits from this, and shared properties
    /// are NOT re-emitted (they're assumed to live on this parent interface).
    /// Example: <c>"IProcessor"</c> (when the base interface IS the parent interface).
    /// Normally null — set only when the union IS the base interface.
    /// </summary>
    public string? BaseInterfaceInherits { get; init; }

    /// <summary>
    /// When true, the base interface and its shared properties are NOT generated
    /// (they already exist hand-written). Only variant types, formatter, and list
    /// descriptor are generated. Default: false.
    /// </summary>
    public bool SuppressBaseInterfaceGeneration { get; init; }

    /// <summary>
    /// When true, shared base properties from ProcessorBase (if/tag/ignore_failure/on_failure/description)
    /// are included in serialization. This is typically true when variants inherit ProcessorBase
    /// and those fields are serialized at the variant level inside the wrapper key object.
    /// Default: false (base properties are structural only, not serialized per-variant).
    /// </summary>
    public bool SerializeBaseProperties { get; init; }

    // ── Variant descriptor generics ──────────────────────────────────────────

    /// <summary>
    /// The descriptor base class pattern. Uses <c>{0}</c> for descriptor type, <c>{1}</c> for interface type.
    /// Default (when null): <c>"DescriptorBase&lt;{0}, {1}&gt;"</c>.
    /// Example: <c>"ProcessorDescriptorBase&lt;{0}, {1}&gt;"</c> which already has If/Tag/OnFailure methods.
    /// </summary>
    public string? DescriptorBasePattern { get; init; }

    /// <summary>
    /// When true, variant descriptors are generic with <c>&lt;T&gt; where T : class</c>.
    /// The descriptor class name becomes <c>{Name}ProcessorDescriptor&lt;T&gt;</c>.
    /// Default: false.
    /// </summary>
    public bool GenericDescriptors { get; init; }

    /// <summary>
    /// Wire names of properties that should be typed as <c>Field</c> and get an
    /// <c>Expression&lt;Func&lt;T, TValue&gt;&gt;</c> overload on the descriptor.
    /// Only meaningful when <see cref="GenericDescriptors"/> is true.
    /// Example: <c>["field", "target_field"]</c>.
    /// </summary>
    public ISet<string> FieldProperties { get; init; } = new HashSet<string>(StringComparer.Ordinal);

    /// <summary>
    /// Wire names of properties that belong to the union's shared base class and should be
    /// excluded from all variant body property lists. This replaces per-namespace hardcoding
    /// in the code generator.
    /// Example: <c>["tag", "description", "ignore_failure"]</c> for ingest processors.
    /// </summary>
    public ISet<string> ExcludedBaseProperties { get; init; } = new HashSet<string>(StringComparer.Ordinal);

    // ── Per-variant behavioral overrides ─────────────────────────────────────

    /// <summary>
    /// Per-variant overrides keyed by wire key (e.g. "text_embedding", "script").
    /// Allows mapping specific variants to different base classes, marking them as
    /// retained (hand-written, not generated), etc.
    /// </summary>
    public IDictionary<string, VariantPolicy> VariantOverrides { get; init; } =
        new Dictionary<string, VariantPolicy>(StringComparer.Ordinal);

    /// <summary>
    /// Existing C# variants absent from the current specification but retained for public API
    /// compatibility. They participate in formatter and list-descriptor dispatch only; their
    /// model and descriptor implementations remain hand-written.
    /// </summary>
    public IReadOnlyList<RetainedVariantPolicy> AdditionalRetainedVariants { get; init; } =
        Array.Empty<RetainedVariantPolicy>();

    // ── Naming overrides ─────────────────────────────────────────────────────

    /// <summary>
    /// Override the C# class name for a variant. Key: wire key, Value: C# name.
    /// Example: <c>{ "kv": "KeyValueProcessor" }</c>.
    /// Default: PascalCase of schema ID or wire key + no suffix.
    /// </summary>
    public IDictionary<string, string> VariantNameOverrides { get; init; } =
        new Dictionary<string, string>(StringComparer.Ordinal);

    /// <summary>
    /// Override the fluent method name on the list descriptor.
    /// Key: wire key, Value: method name.
    /// Example: <c>{ "kv": "Kv", "community_id": "NetworkCommunityId" }</c>.
    /// Default: PascalCase of wire key.
    /// </summary>
    public IDictionary<string, string> FluentMethodNameOverrides { get; init; } =
        new Dictionary<string, string>(StringComparer.Ordinal);

    /// <summary>
    /// Override the variant interface name. Key: wire key, Value: interface name (without "I" prefix).
    /// Example: <c>{ "kv": "KeyValueProcessor" }</c> produces <c>IKeyValueProcessor</c>.
    /// Default: same as variant C# name.
    /// </summary>
    public IDictionary<string, string> InterfaceNameOverrides { get; init; } =
        new Dictionary<string, string>(StringComparer.Ordinal);
}

/// <summary>
/// Target-language compatibility policy for an existing variant that is not modeled by
/// the current OpenAPI specification.
/// </summary>
public sealed class RetainedVariantPolicy
{
    public required string Key { get; init; }
    public required string CsharpName { get; init; }
    public string? InterfaceName { get; init; }
    public string? FluentMethodName { get; init; }
    public bool GenericDescriptor { get; init; } = true;
}

/// <summary>
/// Per-variant rendering policy for behavioral overrides.
/// </summary>
public sealed class VariantPolicy
{
    /// <summary>
    /// Override the base class for this specific variant's concrete class.
    /// Example: <c>"InferenceProcessorBase"</c> for text_embedding.
    /// Null means use the union-level <see cref="UnionRenderingPolicy.VariantBaseClass"/>.
    /// </summary>
    public string? BaseClass { get; init; }

    /// <summary>
    /// Override the descriptor base class for this specific variant.
    /// Uses <c>{T}</c> for type param, <c>{0}</c> for descriptor type, <c>{1}</c> for interface type.
    /// Example: <c>"InferenceProcessorDescriptorBase&lt;{T}, {0}, {1}&gt;"</c>.
    /// </summary>
    public string? DescriptorBasePattern { get; init; }

    /// <summary>
    /// When true, this variant is NOT generated — it remains hand-written.
    /// The formatter and list descriptor still include dispatch for it.
    /// This is for complex behavioral types that cannot be fully spec-generated.
    /// </summary>
    public bool Retained { get; init; }

    /// <summary>
    /// Additional interface that this variant's interface inherits from.
    /// Example: <c>"IInferenceProcessor"</c> for text_embedding.
    /// </summary>
    public string? AdditionalInterface { get; init; }

    /// <summary>
    /// Properties to EXCLUDE from generation for this variant (they come from the behavioral base).
    /// Example: <c>["model_id", "field_map"]</c> for inference processors.
    /// </summary>
    public ISet<string> ExcludedProperties { get; init; } = new HashSet<string>(StringComparer.Ordinal);

    /// <summary>
    /// When true and <see cref="UnionRenderingPolicy.GenericDescriptors"/> is false
    /// for the union, this specific variant still gets a non-generic descriptor
    /// (overrides the union default). Useful for variants like 'script' that don't
    /// operate on document fields.
    /// </summary>
    public bool NonGenericDescriptor { get; init; }

    /// <summary>
    /// Property aliases for backwards compatibility. Key: alias C# method/property name, Value: canonical property wire name.
    /// Emits an additional fluent method and (on interface+class) an aliased property that delegates to the canonical one.
    /// Example: <c>{ "IndexedCharacters": "indexed_chars", "TimeZone": "timezone" }</c>.
    /// </summary>
    public IDictionary<string, string> PropertyAliases { get; init; } =
        new Dictionary<string, string>(StringComparer.Ordinal);

    /// <summary>
    /// Wire names of IList&lt;T&gt; properties that should get a <c>params T[]</c> convenience overload on the descriptor.
    /// Example: <c>["value", "formats", "patterns"]</c>.
    /// </summary>
    public ISet<string> ParamsOverloads { get; init; } = new HashSet<string>(StringComparer.Ordinal);

    /// <summary>
    /// Wire names of IList&lt;string&gt; properties that should get a single-string convenience overload on the descriptor.
    /// Example: <c>["formats", "patterns", "include_keys", "exclude_keys"]</c>.
    /// </summary>
    public ISet<string> ScalarStringOverloads { get; init; } = new HashSet<string>(StringComparer.Ordinal);

    /// <summary>
    /// Wire names of IProcessor properties that should get a Func&lt;ProcessorsDescriptor, IPromise&lt;IList&lt;IProcessor&gt;&gt;&gt;
    /// lambda convenience overload on the descriptor.
    /// Example: <c>["processor"]</c> for ForeachProcessor.
    /// </summary>
    public ISet<string> ProcessorLambdaOverloads { get; init; } = new HashSet<string>(StringComparer.Ordinal);

    /// <summary>
    /// Wire names of IDictionary&lt;string,string&gt; properties that should get a
    /// Func&lt;FluentDictionary&lt;string,string&gt;, FluentDictionary&lt;string,string&gt;&gt; convenience overload.
    /// Example: <c>["pattern_definitions"]</c> for GrokProcessor.
    /// </summary>
    public ISet<string> DictionaryLambdaOverloads { get; init; } = new HashSet<string>(StringComparer.Ordinal);

    /// <summary>
    /// C# property type overrides keyed by OpenAPI wire property name.
    /// These are rendering policy only; the OpenAPI schema remains the structural source of truth.
    /// </summary>
    public IDictionary<string, string> PropertyTypeOverrides { get; init; } =
        new Dictionary<string, string>(StringComparer.Ordinal);

    /// <summary>
    /// Wire names of <c>Fields</c> properties that should get a
    /// <c>Func&lt;FieldsDescriptor&lt;T&gt;, IPromise&lt;Fields&gt;&gt;</c> convenience overload.
    /// </summary>
    public ISet<string> FieldsSelectorOverloads { get; init; } =
        new HashSet<string>(StringComparer.Ordinal);
}
