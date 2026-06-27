using System;
using System.IO;
using System.Text;
using OpenSearch.Net;
using Xunit;

namespace OpenSearch.Net.Stj.Tests;

// Step 3 (partial, issue #388): proves SystemTextJsonSerializer plugs into the
// REAL low-level client/transport via ConnectionConfiguration and is used on the
// request path -- end-to-end, no Utf8Json involved for these bodies.
public class LowLevelWiringTests
{
    public sealed class Doc
    {
        public string Name { get; set; } = "";
        public int Count { get; set; }
    }

    private static ConnectionConfiguration StjSettings() =>
        new(new SingleNodeConnectionPool(new Uri("http://localhost:9200")),
            new InMemoryConnection(),
            new SystemTextJsonSerializer());

    [Fact]
    public void LowLevelClient_serializer_is_system_text_json()
    {
        var client = new OpenSearchLowLevelClient(StjSettings());

        using var ms = new MemoryStream();
        client.Serializer.Serialize(new Doc { Name = "x", Count = 1 }, ms);

        // Output matches System.Text.Json defaults (PascalCase, compact),
        // proving the configured STJ serializer is the one the client uses.
        Assert.Equal("{\"Name\":\"x\",\"Count\":1}", Encoding.UTF8.GetString(ms.ToArray()));
    }

    [Fact]
    public void Request_body_is_serialized_by_configured_serializer()
    {
        IConnectionConfigurationValues settings = StjSettings();
        var data = PostData.Serializable(new Doc { Name = "y", Count = 2 });

        using var ms = new MemoryStream();
        data.Write(ms, settings); // transport uses settings.RequestResponseSerializer

        Assert.Equal("{\"Name\":\"y\",\"Count\":2}", Encoding.UTF8.GetString(ms.ToArray()));
    }

    [Fact]
    public void Client_serializer_roundtrips_response_shape()
    {
        var client = new OpenSearchLowLevelClient(StjSettings());
        using var input = new MemoryStream(Encoding.UTF8.GetBytes("{\"Name\":\"z\",\"Count\":9}"));

        var doc = client.Serializer.Deserialize<Doc>(input);

        Assert.Equal("z", doc.Name);
        Assert.Equal(9, doc.Count);
    }
}
