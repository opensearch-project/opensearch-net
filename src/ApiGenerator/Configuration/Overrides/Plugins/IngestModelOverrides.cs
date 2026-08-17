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
/// The ingest spec uses a flat wrapper-key discriminated union for processors
/// (ProcessorContainer). The handwritten implementation (ProcessorFormatter,
/// ProcessorsDescriptor, and per-processor types in src/OpenSearch.Client/Ingest/)
/// is authoritative and must not be replaced by generated code.
///
/// This override keeps structural classification of ProcessorContainer for generic pipeline
/// validation, but suppresses it as a generation root and output so the handwritten implementation
/// remains the only emitted Ingest processor API.
/// </summary>
public sealed class IngestModelOverrides : ModelOverridesBase
{
    public override string Namespace => "ingest";
    public override string OutputFolder => "Ingest/Generated";

    /// <summary>
    /// Ingest operations (put_pipeline, get_pipeline, delete_pipeline, simulate) and their shared
    /// processor types are handwritten. This plugin performs structural classification only and
    /// emits no replacement Ingest processor code.
    /// </summary>
    public override bool IncludeOperationSchemasInReachability => false;
    public override bool GenerateBodyOps => false;
    public override bool GenerateNonBodyOps => false;
    public override bool SuppressLowLevelApiImport => true;

    /// <summary>
    /// ProcessorContainer is structurally classified as a FlatWrapperKey union, but it is excluded
    /// from generation roots and output. Its dependent models therefore are not emitted either.
    /// The handwritten ProcessorFormatter, ProcessorsDescriptor, and per-processor types remain
    /// authoritative.
    /// </summary>
    public override ISet<string> SuppressedUnionSchemaIds { get; } = new HashSet<string>(StringComparer.Ordinal)
    {
        "ingest._common___ProcessorContainer",
    };

    /// <summary>
    /// ProcessorContainer is the central union type (FlatWrapperKey pattern). Mapping it here
    /// lets array properties (<c>processors: IList&lt;IProcessor&gt;</c>) resolve to the correct
    /// interface. The variant body schemas are mapped as external types since the variant types
    /// are handwritten.
    /// </summary>
    public override IDictionary<string, string> MappedTypes { get; } =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            // The union container maps to its handwritten base interface.
            ["ingest._common___ProcessorContainer"] = "IProcessor",
            // ProcessorBase is a hand-written behavioral class (if/tag/on_failure/ignore_failure/description).
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
    /// The union rendering policy is retained so the generic pipeline can classify and inspect the
    /// structure consistently. The union is not a generation root and is suppressed from output via
    /// <see cref="SuppressedUnionSchemaIds"/>.
    /// </summary>
    public override IDictionary<string, UnionRenderingPolicy> UnionPolicies { get; } =
        new Dictionary<string, UnionRenderingPolicy>(StringComparer.Ordinal)
        {
            ["ingest._common___ProcessorContainer"] = new UnionRenderingPolicy
            {
                // Minimal policy — only needed for structural classification.
                // No code is generated from this policy because the union is suppressed.
                BaseInterfaceName = "IProcessor",
                FormatterName = "ProcessorFormatter",
                ListDescriptorName = "ProcessorsDescriptor",
                SuppressBaseInterfaceGeneration = true,
                VariantBaseClass = "ProcessorBase",
                DescriptorBasePattern = "ProcessorDescriptorBase<{0}, {1}>",
                GenericDescriptors = true,
                FieldProperties = new HashSet<string>(StringComparer.Ordinal)
                {
                    "field", "target_field", "source_field",
                },

                // All spec variants are effectively retained (handwritten).
                VariantOverrides = new Dictionary<string, VariantPolicy>(StringComparer.Ordinal)
                {
                    ["script"] = new VariantPolicy { Retained = true },
                    ["text_embedding"] = new VariantPolicy { Retained = true },
                },

                // Spec-absent processors that exist in the handwritten codebase.
                AdditionalRetainedVariants = new RetainedVariantPolicy[]
                {
                    new()
                    {
                        Key = "uri_parts",
                        CsharpName = "UriPartsProcessor",
                        FluentMethodName = "UriParts",
                    },
                    new()
                    {
                        Key = "fingerprint",
                        CsharpName = "FingerprintProcessor",
                        FluentMethodName = "Fingerprint",
                    },
                    new()
                    {
                        Key = "community_id",
                        CsharpName = "NetworkCommunityIdProcessor",
                        FluentMethodName = "NetworkCommunityId",
                    },
                    new()
                    {
                        Key = "network_direction",
                        CsharpName = "NetworkDirectionProcessor",
                        FluentMethodName = "NetworkDirection",
                    },
                },

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

                FluentMethodNameOverrides = new Dictionary<string, string>(StringComparer.Ordinal),
                InterfaceNameOverrides = new Dictionary<string, string>(StringComparer.Ordinal),
            },
        };
}
