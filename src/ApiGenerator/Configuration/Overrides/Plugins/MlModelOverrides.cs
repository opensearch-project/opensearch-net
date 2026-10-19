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
/// model generation overrides for the <c>ml</c> namespace.
/// Enforces "one schema, one owner": op-owned response schemas are skipped from the namespace
/// scan; cross-namespace <c>_common___*</c> refs and already-hand-written types map to existing
/// OSC types; BCL-colliding names are renamed.
/// </summary>
public sealed class MlModelOverrides : ModelOverridesBase
{
    public override string Namespace => "ml";
    public override string OutputFolder => "ML";
    public override bool GenerateBodyOps => true;
    public override bool GenerateNonBodyOps => true;

    // Streaming endpoints require chunked/SSE transport support not yet available in the client.
    public override ISet<string> ExcludedOps { get; } = new HashSet<string>(StringComparer.Ordinal)
    {
        "ml.predict_model_stream", "ml.execute_agent_stream",
    };

    public override IDictionary<string, string> OpNameOverrides { get; } = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        // "GetTask" collides with Tasks/GetTask in flat OpenSearch.Client namespace.
        // High-level types renamed via CodeConfiguration.HighLevelOnlyApiNameOverrides;
        // POCO generator renamed via OpNameOverrides. Low-level keeps "GetTask".
        ["ml.get_task"] = "GetMLTask",
    };

    public override ISet<string> ExplicitlyPublicSchemaIds { get; } = new HashSet<string>(StringComparer.Ordinal)
    {
        "ml._common___MemoryType",
        "ml._common___MlStatName",
        "ml._common___SearchModelGroupsResponse",
        "ml._common___SearchResponse",
        "ml._common___ToolName",
    };

    public override IDictionary<string, string> RenamedTypes { get; } = BuildRenamedTypes();

    // ML node-id arrays. The spec types these items as NodeIds (a request-filter union
    // oneOf[NodeId, array<NodeId>]), so an array of them would generate a list of unions.
    // The server returns a flat list of node-id strings, so force IList<string>.
    // Schema-scoped keys cover the namespace models; the get_task response is generated via the
    // operation path and reroutes to the Task schema, so it needs the operation-scoped key.
    // opensearch-api-specification#1284 fixes the spec items to the scalar NodeId (string);
    // once merged these overrides become redundant and can be removed.
    public override IDictionary<string, string> PropertyTypeOverrides { get; } = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["ml._common___Source.worker_node"] = "IList<string>",
        ["ml._common___Source.planning_worker_nodes"] = "IList<string>",
        ["ml._common___Task.worker_node"] = "IList<string>",
        ["ml._common___ModelProfile.worker_nodes"] = "IList<string>",
        ["ml._common___ModelProfile.target_worker_nodes"] = "IList<string>",
        ["ml.get_task.worker_node"] = "IList<string>",
    };

    private static Dictionary<string, string> BuildRenamedTypes() => new(StringComparer.Ordinal)
    {
        // All generated types are in the flat OpenSearch.Client namespace.
        ["ml._common___Task"] = "MLTask",             // shadows System.Threading.Tasks.Task
        ["ml._common___Action"] = "MLAction",         // shadows System.Action delegate
        ["ml._common___Node"] = "MLNode",             // collides with OpenSearch.Net.Node
        ["ml._common___Aggregation"] = "MLAggregation", // collides with OSC's IAggregation hierarchy
        ["ml._common___Result"] = "MLResult",         // collides with OSC's Document/Result enum
        ["ml._common___TaskState"] = "MLTaskState",   // collides with OSC's TaskState class
        ["ml._common___IndexSettings"] = "MLIndexSettings", // collides with OSC's IndexSettings class
        ["ml._common___Metadata"] = "MLMetadata",    // collides with Tests.Domain.Metadata
    };
}
