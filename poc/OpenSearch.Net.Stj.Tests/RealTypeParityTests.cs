using System;
using System.Text.Json;
using OpenSearch.Client;
using OpenSearch.Net;
using OpenSearch.Net.Stj.Tests.Model;
using Xunit;

namespace OpenSearch.Net.Stj.Tests;

// Step 3 (issue #388): proves a GENERATED System.Text.Json converter reproduces
// the REAL OpenSearch.Client (Utf8Json) wire format byte-for-byte for a family
// of leaf queries. Ground truth comes from the real client; the candidate comes
// from the converter emitted by StjConverterGen.
public class RealTypeParityTests
{
    private static readonly JsonSerializerOptions Options =
        new() { Converters = { new GeneratedRealLeafQueryConverter() } };

    private static string GroundTruth(QueryBase q)
    {
        var settings = new ConnectionSettings(
            new SingleNodeConnectionPool(new Uri("http://localhost:9200")), new InMemoryConnection());
        var client = new OpenSearchClient(settings);
        return client.RequestResponseSerializer.SerializeToString(new QueryContainer(q));
    }

    private static string Generated(RealLeafQuery q) => JsonSerializer.Serialize(q, Options);

    [Fact]
    public void MatchAll_parity() =>
        Assert.Equal(GroundTruth(new MatchAllQuery()), Generated(new MatchAllLeaf()));

    [Fact]
    public void Exists_parity() =>
        Assert.Equal(GroundTruth(new ExistsQuery { Field = "user" }),
                     Generated(new ExistsLeaf { Field = "user" }));

    [Fact]
    public void Term_parity() =>
        Assert.Equal(GroundTruth(new TermQuery { Field = "user", Value = "bob" }),
                     Generated(new TermLeaf { Field = "user", Value = "bob" }));

    [Fact]
    public void Prefix_parity() =>
        Assert.Equal(GroundTruth(new PrefixQuery { Field = "user", Value = "bo" }),
                     Generated(new PrefixLeaf { Field = "user", Value = "bo" }));

    [Fact]
    public void Wildcard_parity() =>
        Assert.Equal(GroundTruth(new WildcardQuery { Field = "user", Value = "bo*" }),
                     Generated(new WildcardLeaf { Field = "user", Value = "bo*" }));

    [Fact]
    public void Regexp_parity() =>
        Assert.Equal(GroundTruth(new RegexpQuery { Field = "user", Value = "bo.*" }),
                     Generated(new RegexpLeaf { Field = "user", Value = "bo.*" }));

    [Fact]
    public void Match_parity() =>
        Assert.Equal(GroundTruth(new MatchQuery { Field = "user", Query = "bob" }),
                     Generated(new MatchLeaf { Field = "user", Value = "bob" }));
}
