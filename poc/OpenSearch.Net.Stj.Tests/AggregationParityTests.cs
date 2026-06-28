using System;
using System.Collections.Generic;
using System.Text.Json;
using OpenSearch.Client;
using OpenSearch.Net;
using OpenSearch.Net.Stj.Tests.Model;
using Xunit;

namespace OpenSearch.Net.Stj.Tests;

// Step 3 (issue #388): a second polymorphic family. Proves the GENERATED
// aggregation converter reproduces the real OpenSearch.Client wire format for
// single aggregations and for the named "aggs" map (which STJ serializes as a
// dictionary, values via the generated converter).
public class AggregationParityTests
{
    private static readonly JsonSerializerOptions Options =
        new() { Converters = { new GeneratedRealAggregationConverter() } };

    private static string GroundTruth(object aggContainerOrDict)
    {
        var settings = new ConnectionSettings(
            new SingleNodeConnectionPool(new Uri("http://localhost:9200")), new InMemoryConnection());
        var client = new OpenSearchClient(settings);
        return client.RequestResponseSerializer.SerializeToString(aggContainerOrDict);
    }

    [Fact]
    public void Terms_aggregation_parity()
    {
        AggregationContainer ground = new TermsAggregation("by_user") { Field = "user" };

        var generated = JsonSerializer.Serialize<RealAggregation>(new TermsAgg { Field = "user" }, Options);

        Assert.Equal(GroundTruth(ground), generated);
    }

    [Fact]
    public void Max_aggregation_parity()
    {
        AggregationContainer ground = new MaxAggregation("max_age", "age");

        var generated = JsonSerializer.Serialize<RealAggregation>(new MaxAgg { Field = "age" }, Options);

        Assert.Equal(GroundTruth(ground), generated);
    }

    [Fact]
    public void Named_aggregation_map_parity()
    {
        var groundDict = new AggregationDictionary
        {
            { "by_user", new TermsAggregation("by_user") { Field = "user" } },
            { "max_age", new MaxAggregation("max_age", "age") }
        };

        // STJ serializes a Dictionary<string, RealAggregation> as a JSON object,
        // each value via the generated converter -> the "aggs" map shape.
        var generatedMap = new Dictionary<string, RealAggregation>
        {
            ["by_user"] = new TermsAgg { Field = "user" },
            ["max_age"] = new MaxAgg { Field = "age" }
        };
        var generated = JsonSerializer.Serialize(generatedMap, Options);

        Assert.Equal(GroundTruth(groundDict), generated);
    }
}
