/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

#nullable enable

using System;
using System.Collections.Generic;

namespace ApiGenerator.Configuration.Overrides.Plugins;

/// <summary>
/// Model generation overrides for the <c>ingest</c> namespace.
///
/// The ingest spec uses a flat wrapper-key discriminated union for processors:
///
/// <code>
/// ProcessorContainer:
///   type: object
///   properties:
///     append: { $ref: AppendProcessor }
///     convert: { $ref: ConvertProcessor }
///     ...
///   minProperties: 1
///   maxProperties: 1
/// </code>
///
/// The <c>UnionClassifier</c> detects this as <see cref="ApiGenerator.Domain.Code.HighLevel.Models.UnionEncoding.FlatWrapperKey"/>.
/// <c>NamespaceModel.Build</c> builds a <c>WrapperKeyUnionModel</c> for it. <c>WrapperKeyUnion.cshtml</c>
/// then generates the base interface, formatter, per-variant types, and fluent descriptor list builder.
///
/// The <see cref="UnionPolicies"/> dictionary provides a <see cref="UnionRenderingPolicy"/> that
/// instructs the template to emit processor-compatible output: ProcessorBase inheritance,
/// generic &lt;T&gt; descriptors with Field expression overloads, behavioral base class mappings, etc.
/// </summary>
public sealed class IngestModelOverrides : ModelOverridesBase
{
    public override string Namespace => "ingest";
    public override string OutputFolder => "Ingest/Generated";

    /// <summary>
    /// Ingest operations (put_pipeline, get_pipeline, delete_pipeline, simulate) already have
    /// hand-written request/response types. We generate only the shared model types
    /// (ProcessorContainer hierarchy and supporting schemas).
    /// </summary>
    public override bool IncludeOperationSchemasInReachability => false;
    public override bool GenerateBodyOps => false;
    public override bool GenerateNonBodyOps => false;
    public override bool SuppressLowLevelApiImport => true;

    /// <summary>
    /// ProcessorContainer is the central union type (FlatWrapperKey pattern). Mapping it here
    /// lets array properties (<c>processors: IList&lt;IProcessor&gt;</c>) resolve to the correct
    /// interface. The variant body schemas are mapped as external types since the variant types
    /// are emitted by the union template, not as independent ObjectModels.
    /// </summary>
    public override IDictionary<string, string> MappedTypes { get; } =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            // The union container maps to its generated base interface.
            ["ingest._common___ProcessorContainer"] = "IProcessor",
            // ProcessorBase is a hand-written behavioral class (if/tag/on_failure/ignore_failure/description).
            // It remains hand-written — the union variants inherit from it at the C# level, but this
            // schema itself does not get emitted as a standalone model.
            ["ingest._common___ProcessorBase"] = "ProcessorBase",
            // Script is a cross-namespace type already in OpenSearch.Client.
            ["_common___Script"] = "IScript",
            // Pipeline is already a hand-written type (ingest pipeline definition).
            ["ingest._common___Pipeline"] = "Pipeline",
        };

    /// <summary>
    /// Rename schemas whose default PascalCase name collides with existing client types or is ambiguous.
    /// </summary>
    public override IDictionary<string, string> RenamedTypes { get; } =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            // ConvertType enum (the "type" field enum for ConvertProcessor) — avoid colliding with System.Convert.
            ["ingest._common___ConvertType"] = "ConvertProcessorType",
            // JsonProcessorConflictStrategy enum
            ["ingest._common___JsonProcessorConflictStrategy"] = "JsonProcessorConflictStrategy",
            // ShapeType enum
            ["ingest._common___ShapeType"] = "IngestShapeType",
            // UserAgentProperty enum
            ["ingest._common___UserAgentProperty"] = "IngestUserAgentProperty",
        };

    /// <summary>
    /// The union rendering policy for ProcessorContainer.
    /// This drives WrapperKeyUnion.cshtml to emit processor-compatible output:
    /// - ProcessorBase/ProcessorDescriptorBase inheritance
    /// - Generic &lt;T&gt; descriptors with Expression&lt;Func&lt;T, TValue&gt;&gt; for Field properties
    /// - Retained variants for behavioral processors (text_embedding, etc.)
    /// - Interface/method naming to match existing API surface
    /// </summary>
    public override IDictionary<string, UnionRenderingPolicy> UnionPolicies { get; } =
        new Dictionary<string, UnionRenderingPolicy>(StringComparer.Ordinal)
        {
            ["ingest._common___ProcessorContainer"] = new UnionRenderingPolicy
            {
                // ── Union-level ──
                BaseInterfaceName = "IProcessor",
                FormatterName = "ProcessorFormatter",
                ListDescriptorName = "ProcessorsDescriptor",

                // The base interface (IProcessor) + base class (ProcessorBase) are hand-written
                // and already exist. Don't generate them.
                SuppressBaseInterfaceGeneration = true,

                // ── Variant defaults ──
                VariantBaseClass = "ProcessorBase",
                DescriptorBasePattern = "ProcessorDescriptorBase<{0}, {1}>",
                GenericDescriptors = true,

                // Properties that are typed as Field and get Expression<Func<T, TValue>> overloads
                FieldProperties = new HashSet<string>(StringComparer.Ordinal)
                {
                    "field", "target_field", "source_field",
                },

                // ── Per-variant overrides ──
                VariantOverrides = new Dictionary<string, VariantPolicy>(StringComparer.Ordinal)
                {
                    // Script processor: retained (hand-written, has custom Source/Lang/Params/Id properties
                    // that don't correspond to the spec's Script schema which is a cross-namespace reference type)
                    ["script"] = new VariantPolicy { Retained = true, NonGenericDescriptor = true },

                    // Fail processor: non-generic (just emits a message)
                    ["fail"] = new VariantPolicy { NonGenericDescriptor = true },

                    // Drop processor: non-generic (no properties at all)
                    ["drop"] = new VariantPolicy { NonGenericDescriptor = true },

                    // Pipeline processor: non-generic (just references pipeline name)
                    ["pipeline"] = new VariantPolicy
                    {
                        NonGenericDescriptor = true,
                        PropertyAliases = new Dictionary<string, string>(StringComparer.Ordinal)
                        {
                            ["ProcessorName"] = "name",
                        },
                    },

                    // Remove processor accepts one or many fields through the existing Fields abstraction.
                    ["remove"] = new VariantPolicy
                    {
                        PropertyTypeOverrides = new Dictionary<string, string>(StringComparer.Ordinal)
                        {
                            ["field"] = "Fields",
                        },
                        FieldsSelectorOverloads = new HashSet<string>(StringComparer.Ordinal) { "field" },
                    },

                    // Append processor: params overload for Value
                    ["append"] = new VariantPolicy
                    {
                        ParamsOverloads = new HashSet<string>(StringComparer.Ordinal) { "value" },
                    },

                    // Set processor: params overload for Value
                    ["set"] = new VariantPolicy
                    {
                        ParamsOverloads = new HashSet<string>(StringComparer.Ordinal) { "value" },
                    },

                    // Date processor: TimeZone alias + Formats scalar overload
                    ["date"] = new VariantPolicy
                    {
                        PropertyAliases = new Dictionary<string, string>(StringComparer.Ordinal)
                        {
                            ["TimeZone"] = "timezone",
                        },
                        ScalarStringOverloads = new HashSet<string>(StringComparer.Ordinal) { "formats" },
                    },

                    // DateIndexName processor: TimeZone alias + DateFormats scalar overload
                    ["date_index_name"] = new VariantPolicy
                    {
                        PropertyAliases = new Dictionary<string, string>(StringComparer.Ordinal)
                        {
                            ["TimeZone"] = "timezone",
                        },
                        ScalarStringOverloads = new HashSet<string>(StringComparer.Ordinal) { "date_formats" },
                    },

                    // Grok processor: Patterns scalar overload + PatternDefinitions Func overload
                    ["grok"] = new VariantPolicy
                    {
                        ScalarStringOverloads = new HashSet<string>(StringComparer.Ordinal) { "patterns" },
                        DictionaryLambdaOverloads = new HashSet<string>(StringComparer.Ordinal) { "pattern_definitions" },
                    },

                    // Foreach processor: Processor lambda overload
                    ["foreach"] = new VariantPolicy
                    {
                        ProcessorLambdaOverloads = new HashSet<string>(StringComparer.Ordinal) { "processor" },
                    },

                    // KeyValue processor: IncludeKeys/ExcludeKeys scalar overloads
                    ["kv"] = new VariantPolicy
                    {
                        ScalarStringOverloads = new HashSet<string>(StringComparer.Ordinal) { "include_keys", "exclude_keys" },
                    },

                    // Attachment processor: IndexedCharacters alias + Properties scalar overload
                    ["attachment"] = new VariantPolicy
                    {
                        PropertyAliases = new Dictionary<string, string>(StringComparer.Ordinal)
                        {
                            ["IndexedCharacters"] = "indexed_chars",
                        },
                        ScalarStringOverloads = new HashSet<string>(StringComparer.Ordinal) { "properties" },
                    },

                    // GeoIp processor: Properties scalar overload
                    ["geoip"] = new VariantPolicy
                    {
                        ScalarStringOverloads = new HashSet<string>(StringComparer.Ordinal) { "properties" },
                    },

                    // TextEmbedding: retained (complex behavioral InferenceProcessorBase hierarchy)
                    ["text_embedding"] = new VariantPolicy
                    {
                        Retained = true,
                        BaseClass = "InferenceProcessorBase",
                        AdditionalInterface = "IInferenceProcessor",
                        DescriptorBasePattern = "InferenceProcessorDescriptorBase<{T}, {0}, {1}>",
                        ExcludedProperties = new HashSet<string>(StringComparer.Ordinal)
                        {
                            "model_id", "field_map"
                        },
                    },
                },

                // ── Variant naming ──
                VariantNameOverrides = new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["script"] = "ScriptProcessor",
                    ["kv"] = "KeyValueProcessor",
                    ["community_id"] = "NetworkCommunityIdProcessor",
                    ["network_direction"] = "NetworkDirectionProcessor",
                    ["uri_parts"] = "UriPartsProcessor",
                    ["urldecode"] = "UrlDecodeProcessor",
                    ["date_index_name"] = "DateIndexNameProcessor",
                    ["dot_expander"] = "DotExpanderProcessor",
                    ["user_agent"] = "UserAgentProcessor",
                },

                FluentMethodNameOverrides = new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["kv"] = "Kv",
                    ["community_id"] = "NetworkCommunityId",
                    ["network_direction"] = "NetworkDirection",
                    ["uri_parts"] = "UriParts",
                    ["urldecode"] = "UrlDecode",
                    ["date_index_name"] = "DateIndexName",
                    ["dot_expander"] = "DotExpander",
                    ["geoip"] = "GeoIp",
                    ["user_agent"] = "UserAgent",
                    ["text_embedding"] = "TextEmbedding",
                },

                InterfaceNameOverrides = new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["script"] = "ScriptProcessor",
                    ["kv"] = "KeyValueProcessor",
                    ["community_id"] = "NetworkCommunityIdProcessor",
                    ["network_direction"] = "NetworkDirectionProcessor",
                    ["uri_parts"] = "UriPartsProcessor",
                    ["urldecode"] = "UrlDecodeProcessor",
                    ["date_index_name"] = "DateIndexNameProcessor",
                    ["dot_expander"] = "DotExpanderProcessor",
                    ["user_agent"] = "UserAgentProcessor",
                },
            },
        };
}
