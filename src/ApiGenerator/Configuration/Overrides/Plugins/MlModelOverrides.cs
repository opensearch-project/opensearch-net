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
    public override string OutputFolder => "Ml";
    public override bool GenerateBodyOps => true;
    public override bool GenerateNonBodyOps => true;
    public override bool UseObjectSchemaIds => true;

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
        ["ml.get_task"] = "GetMlTask",
    };

    public override IDictionary<string, string> RenamedTypes { get; } = BuildRenamedTypes();

    private static Dictionary<string, string> BuildRenamedTypes() => new(StringComparer.Ordinal)
    {
        // All generated types are in the flat OpenSearch.Client namespace.
        ["ml._common___Task"] = "MlTask",             // shadows System.Threading.Tasks.Task
        ["ml._common___Action"] = "MlAction",         // shadows System.Action delegate
        ["ml._common___Node"] = "MlNode",             // collides with OpenSearch.Net.Node
        ["ml._common___Aggregation"] = "MlAggregation", // collides with OSC's IAggregation hierarchy
        ["ml._common___Result"] = "MlResult",         // collides with OSC's Document/Result enum
        ["ml._common___TaskState"] = "MlTaskState",   // collides with OSC's TaskState class
        ["ml._common___IndexSettings"] = "MlIndexSettings", // collides with OSC's IndexSettings class
    };
}
