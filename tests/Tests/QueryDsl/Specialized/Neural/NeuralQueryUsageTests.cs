/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using OpenSearch.Client;
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Domain;
using Tests.Framework.EndpointTests.TestState;

namespace Tests.QueryDsl.Specialized.Neural;

public class NeuralQueryUsageTests : QueryDslUsageTestsBase
{
    public NeuralQueryUsageTests(ReadOnlyCluster i, EndpointUsage usage) : base(i, usage) { }

    protected override ConditionlessWhen ConditionlessWhen => new ConditionlessWhen<INeuralQuery>(a => a.Neural)
    {
        q =>
        {
            q.Field = null;
            q.QueryText = "wild west";
            q.K = 5;
            q.ModelId = "aFcV879";
        },
        q =>
        {
            q.Field = "passage_embedding";
            q.QueryText = null;
            q.K = 5;
            q.ModelId = "aFcV879";
        },
        q =>
        {
            q.Field = "passage_embedding";
            q.QueryText = "";
            q.K = 5;
            q.ModelId = "aFcV879";
        },
        q =>
        {
            q.Field = "passage_embedding";
            q.QueryText = "wild west";
            q.K = null;
            q.ModelId = "aFcV879";
        },
        q =>
        {
            q.Field = "passage_embedding";
            q.QueryText = "wild west";
            q.K = 0;
            q.ModelId = "aFcV879";
        },
        q =>
        {
            q.Field = "passage_embedding";
            q.QueryText = "wild west";
            q.K = 5;
            q.ModelId = null;
        },
        q =>
        {
            q.Field = "passage_embedding";
            q.QueryText = "wild west";
            q.K = 5;
            q.ModelId = "";
        }
    };

    protected override QueryContainer QueryInitializer => new NeuralQuery
    {
        Boost = 1.1,
        Field = Infer.Field<Project>(f => f.Vector),
        QueryText = "wild west",
        K = 5,
        ModelId = "aFcV879"
    };

    protected override object QueryJson =>
        new
        {
            neural = new
            {
                vector = new
                {
                    boost = 1.1,
                    query_text = "wild west",
                    k = 5,
                    model_id = "aFcV879"
                }
            }
        };

    protected override QueryContainer QueryFluent(QueryContainerDescriptor<Project> q) => q
        .Neural(n => n
            .Boost(1.1)
            .Field(f => f.Vector)
            .QueryText("wild west")
            .K(5)
            .ModelId("aFcV879")
        );
}
