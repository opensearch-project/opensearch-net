using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using OpenSearch.Client;
using OpenSearch.Net;
using Xunit;
using Xunit.Abstractions;

namespace OpenSearch.Net.Stj.Tests;

// Step 3 parity (issue #388): the migration's correctness criterion is that the
// new System.Text.Json converters reproduce the EXACT wire JSON the current
// Utf8Json-based client emits (the ~3,200 existing tests assert wire strings).
//
// These tests capture ground-truth JSON from the REAL high-level OpenSearch.Client
// (Utf8Json) and assert a self-contained System.Text.Json converter reproduces it
// byte-for-byte.
public class WireFormatParityTests
{
    private readonly ITestOutputHelper _output;
    public WireFormatParityTests(ITestOutputHelper output) => _output = output;

    private static string GroundTruth(object highLevelObject)
    {
        var settings = new ConnectionSettings(
            new SingleNodeConnectionPool(new Uri("http://localhost:9200")),
            new InMemoryConnection());
        var client = new OpenSearchClient(settings);
        return client.RequestResponseSerializer.SerializeToString(highLevelObject);
    }

    private static string Stj(StjQuery query)
    {
        var options = new JsonSerializerOptions { Converters = { new StjQueryConverter() } };
        return JsonSerializer.Serialize(query, options);
    }

    [Fact]
    public void MatchAll_wire_format_parity()
    {
        var ground = GroundTruth(new QueryContainer(new MatchAllQuery()));
        _output.WriteLine($"ground truth: {ground}");

        var stj = Stj(new StjMatchAll());

        Assert.Equal(ground, stj);
    }

    [Fact]
    public void Term_wire_format_parity()
    {
        var ground = GroundTruth(new QueryContainer(new TermQuery { Field = "user", Value = "bob" }));
        _output.WriteLine($"ground truth: {ground}");

        var stj = Stj(new StjTerm { Field = "user", Value = "bob" });

        Assert.Equal(ground, stj);
    }

    // ---- self-contained STJ candidate (mirrors what the generator would emit) ----
    public abstract class StjQuery { public abstract string Variant { get; } }
    public sealed class StjMatchAll : StjQuery { public override string Variant => "match_all"; }
    public sealed class StjTerm : StjQuery
    {
        public override string Variant => "term";
        public string Field { get; set; } = "";
        public string Value { get; set; } = "";
    }

    public sealed class StjQueryConverter : JsonConverter<StjQuery>
    {
        public override void Write(Utf8JsonWriter writer, StjQuery value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WritePropertyName(value.Variant);
            switch (value)
            {
                case StjMatchAll:
                    writer.WriteStartObject();
                    writer.WriteEndObject();
                    break;
                case StjTerm term:
                    writer.WriteStartObject();
                    writer.WritePropertyName(term.Field);
                    writer.WriteStartObject();
                    writer.WriteString("value", term.Value);
                    writer.WriteEndObject();
                    writer.WriteEndObject();
                    break;
                default:
                    throw new JsonException();
            }
            writer.WriteEndObject();
        }

        public override StjQuery Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            throw new NotSupportedException();
    }
}
