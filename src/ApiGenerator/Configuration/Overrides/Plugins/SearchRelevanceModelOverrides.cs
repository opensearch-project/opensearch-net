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
/// Model generation overrides for the <c>search_relevance</c> namespace (Search Relevance
/// Workbench: query sets, judgments, search configurations, and experiments).
///
/// Two request bodies (<c>put_experiments</c>, <c>put_judgments</c>) are a bare
/// <c>anyOf</c>/<c>oneOf</c> of sibling schemas with no wrapper key or discriminator — none of
/// <c>UnionClassifier</c>'s four patterns match, so they are handled by
/// <c>OperationModel</c>'s composition-flattening path (same treatment already used for oneOf
/// responses elsewhere), not as a generated union type.
///
/// The five list endpoints (<c>get_query_sets</c>, <c>get_experiments</c>, <c>get_judgments</c>,
/// <c>get_scheduled_experiments</c>, <c>get_search_configurations</c>) all return the shared
/// generic <c>_core.search___SearchResult</c> envelope (took/timed_out/_shards/hits/aggregations)
/// rather than a namespace-specific response schema.
/// </summary>
public sealed class SearchRelevanceModelOverrides : ModelOverridesBase
{
    public override string Namespace => "search_relevance";
    public override string OutputFolder => "SearchRelevance/Generated";
    public override bool GenerateBodyOps => true;
    public override bool GenerateNonBodyOps => true;

    public override IDictionary<string, string> OpNameOverrides { get; } = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        // search_relevance.get_stats would default to "GetStats", colliding with ml.get_stats
        // (both plugins are registered together in ModelsGenerator.EnabledPlugins, so their
        // default request/response/descriptor type names share the same OpenSearch.Client
        // namespace). "GetNodeStats" has no such collision and keeps its default name.
        // Renames the POCO generator's output; kept in sync with the classic-pipeline rename in
        // CodeConfiguration.HighLevelOnlyApiNameOverrides (same pattern as ml.get_task).
        ["search_relevance.get_stats"] = "GetSearchRelevanceStats",
    };

    public override IDictionary<string, string> RenamedTypes { get; } = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        // search_relevance._common___SearchRequest is the shared body schema for the four
        // *_search operations (experiments_search, judgments_search, query_sets_search,
        // search_configurations_search). Its default name "SearchRequest" collides with the
        // hand-written core Search API's SearchRequest/ISearchRequest/SearchRequestDescriptor in
        // the flat OpenSearch.Client namespace.
        ["search_relevance._common___SearchRequest"] = "SearchRelevanceSearchRequest",
    };
}
