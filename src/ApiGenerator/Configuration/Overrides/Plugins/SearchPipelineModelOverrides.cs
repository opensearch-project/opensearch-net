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
/// Model generation overrides for the <c>search_pipeline</c> namespace.
///
/// The search pipeline spec uses wrapper-key discriminated unions for processor types:
///
/// <code>
/// RequestProcessor:
///   oneOf:
///     - title: neural_query_enricher
///       properties:
///         neural_query_enricher: { $ref: NeuralQueryEnricherRequestProcessor }
///       required: [neural_query_enricher]
///     - ...
/// </code>
///
/// <c>NamespaceModel.Build</c> detects this pattern and emits <c>WrapperKeyUnionModel</c> for
/// each such union. <c>WrapperKeyUnion.cshtml</c> then generates the base interface, formatter,
/// per-variant types, and fluent descriptor list builder.
/// </summary>
public sealed class SearchPipelineModelOverrides : ModelOverridesBase
{
    public override string Namespace => "search_pipeline";
    public override string OutputFolder => "SearchPipeline/Generated";

    /// <summary>
    /// Generates body operations (e.g. PutSearchPipelineRequest) and non-body operations
    /// (e.g. GetSearchPipelineResponse, DeleteSearchPipelineResponse) in addition to the
    /// shared processor-union model types. The Requests/Descriptors Razor templates also
    /// generate client entry points for these operations (driven by [MapsApi] on hand-written
    /// request files), but the body-partial and response-POCO files are generated here.
    /// </summary>
    public override bool GenerateBodyOps => true;
    public override bool GenerateNonBodyOps => true;
    public override bool SuppressLowLevelApiImport => true;

    public override IDictionary<string, string> OpNameOverrides { get; } =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["search_pipeline.put"]    = "PutSearchPipeline",
            ["search_pipeline.get"]    = "GetSearchPipeline",
            ["search_pipeline.delete"] = "DeleteSearchPipeline",
        };

    /// <summary>
    /// The three processor union types (<c>RequestProcessor</c>, <c>ResponseProcessor</c>,
    /// <c>PhaseResultsProcessor</c>) are wrapper-key discriminated unions generated as
    /// <c>WrapperKeyUnionModel</c>. They are not plain object schemas, so the type resolver
    /// cannot discover their C# interface names automatically. Mapping them here lets array
    /// properties (e.g. <c>request_processors: IList&lt;IRequestProcessor&gt;</c>) resolve
    /// to the correct interface instead of falling back to <c>object</c>.
    /// </summary>
    public override IDictionary<string, string> MappedTypes { get; } =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["search_pipeline._common___RequestProcessor"]      = "IRequestProcessor",
            ["search_pipeline._common___ResponseProcessor"]     = "IResponseProcessor",
            ["search_pipeline._common___PhaseResultsProcessor"] = "IPhaseResultsProcessor",
        };

    public override IDictionary<string, string> RenamedTypes { get; } =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            // Rename the spec's internal schema name to the clean public-facing name.
            ["search_pipeline._common___SearchPipelineStructure"] = "SearchPipeline",
            // search_pipeline._common___Script collides with the existing Script class used elsewhere.
            ["search_pipeline._common___SearchScriptRequestProcessor"] = "SearchScriptRequestProcessor",
            // search_pipeline._common___SortResponseProcessor could collide with SortProcessor (ingest).
            ["search_pipeline._common___SortResponseProcessor"] = "SortResponseProcessor",
            // search_pipeline._common___SplitResponseProcessor could collide with SplitProcessor (ingest).
            ["search_pipeline._common___SplitResponseProcessor"] = "SplitResponseProcessor",
            // search_pipeline._common___RerankContext collides with nothing, but prefix for clarity.
            ["search_pipeline._common___RerankContext"] = "SearchPipelineRerankContext",
            // search_pipeline._common___MLOpenSearchReranker for clarity.
            ["search_pipeline._common___MLOpenSearchReranker"] = "SearchPipelineMLOpenSearchReranker",
        };
}
