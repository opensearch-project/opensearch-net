using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using OpenSearch.Net;
using Xunit;

namespace OpenSearch.Net.Stj.Tests;

// Step 1 foundation (issue #388): proves the real SystemTextJsonSerializer in
// OpenSearch.Net works through the production IOpenSearchSerializer seam --
// sync/async, stream round-trip, formatting hint, and custom-options injection
// (the mechanism that will thread connection settings into converters).
public class SystemTextJsonSerializerTests
{
    public sealed class Doc
    {
        public string Name { get; set; } = "";
        public int Count { get; set; }
    }

    private static string SerializeToString(IOpenSearchSerializer serializer, object value,
        SerializationFormatting formatting = SerializationFormatting.None)
    {
        using var ms = new MemoryStream();
        serializer.Serialize(value, ms, formatting);
        return Encoding.UTF8.GetString(ms.ToArray());
    }

    private static MemoryStream ToStream(string json) => new(Encoding.UTF8.GetBytes(json));

    [Fact]
    public void Serializes_poco_through_interface()
    {
        IOpenSearchSerializer serializer = new SystemTextJsonSerializer();

        var json = SerializeToString(serializer, new Doc { Name = "alice", Count = 3 });

        Assert.Equal("{\"Name\":\"alice\",\"Count\":3}", json);
    }

    [Fact]
    public void Roundtrips_poco_through_stream()
    {
        IOpenSearchSerializer serializer = new SystemTextJsonSerializer();
        using var ms = ToStream("{\"Name\":\"bob\",\"Count\":7}");

        var doc = serializer.Deserialize<Doc>(ms);

        Assert.Equal("bob", doc.Name);
        Assert.Equal(7, doc.Count);
    }

    [Fact]
    public void Honors_indented_formatting_hint()
    {
        IOpenSearchSerializer serializer = new SystemTextJsonSerializer();

        var compact = SerializeToString(serializer, new Doc { Name = "x", Count = 1 });
        var indented = SerializeToString(serializer, new Doc { Name = "x", Count = 1 }, SerializationFormatting.Indented);

        Assert.DoesNotContain("\n", compact);
        Assert.Contains("\n", indented); // pretty-printed
    }

    [Fact]
    public async Task Async_roundtrip_works()
    {
        IOpenSearchSerializer serializer = new SystemTextJsonSerializer();

        using var outStream = new MemoryStream();
        await serializer.SerializeAsync(new Doc { Name = "carol", Count = 9 }, outStream);
        outStream.Position = 0;
        var doc = await serializer.DeserializeAsync<Doc>(outStream);

        Assert.Equal("carol", doc.Name);
        Assert.Equal(9, doc.Count);
    }

    [Fact]
    public void NonGeneric_deserialize_works()
    {
        IOpenSearchSerializer serializer = new SystemTextJsonSerializer();
        using var ms = ToStream("{\"Name\":\"dan\",\"Count\":2}");

        var obj = serializer.Deserialize(typeof(Doc), ms);

        var doc = Assert.IsType<Doc>(obj);
        Assert.Equal("dan", doc.Name);
        Assert.Equal(2, doc.Count);
    }

    [Fact]
    public void Respects_custom_options_for_state_injection()
    {
        // The options instance is how connection settings / naming policies will be
        // threaded into converters in the full migration.
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        IOpenSearchSerializer serializer = new SystemTextJsonSerializer(options);

        var json = SerializeToString(serializer, new Doc { Name = "eve", Count = 5 });

        Assert.Equal("{\"name\":\"eve\",\"count\":5}", json);
    }
}
