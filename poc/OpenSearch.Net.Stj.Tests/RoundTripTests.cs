using System.Text.Json;
using OpenSearch.Net.Stj.Tests.Model;
using Xunit;

namespace OpenSearch.Net.Stj.Tests;

// Step 3 (issue #388): proves the GENERATED converters also deserialize -- a
// serialize -> deserialize -> serialize round-trip is lossless for the leaf and
// aggregation shapes (identity field naming, so field names round-trip cleanly).
public class RoundTripTests
{
    private static readonly JsonSerializerOptions QueryOptions =
        new() { Converters = { new GeneratedRealLeafQueryConverter() } };

    private static readonly JsonSerializerOptions AggOptions =
        new() { Converters = { new GeneratedRealAggregationConverter() } };

    [Theory]
    [InlineData("{\"match_all\":{}}")]
    [InlineData("{\"exists\":{\"field\":\"user\"}}")]
    [InlineData("{\"term\":{\"user\":{\"value\":\"bob\"}}}")]
    [InlineData("{\"prefix\":{\"user\":{\"value\":\"bo\"}}}")]
    [InlineData("{\"wildcard\":{\"user\":{\"value\":\"bo*\"}}}")]
    [InlineData("{\"regexp\":{\"user\":{\"value\":\"bo.*\"}}}")]
    [InlineData("{\"match\":{\"user\":{\"query\":\"bob\"}}}")]
    public void Query_roundtrips_losslessly(string json)
    {
        var roundTripped = JsonSerializer.Serialize(
            JsonSerializer.Deserialize<RealLeafQuery>(json, QueryOptions), QueryOptions);

        Assert.Equal(json, roundTripped);
    }

    [Theory]
    [InlineData("{\"terms\":{\"field\":\"user\"}}")]
    [InlineData("{\"max\":{\"field\":\"age\"}}")]
    public void Aggregation_roundtrips_losslessly(string json)
    {
        var roundTripped = JsonSerializer.Serialize(
            JsonSerializer.Deserialize<RealAggregation>(json, AggOptions), AggOptions);

        Assert.Equal(json, roundTripped);
    }
}
