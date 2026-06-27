using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OpenSearch.Client;
using OpenSearch.Net;
using Xunit;

namespace OpenSearch.Net.Stj.Tests;

// Step 3 high-level seam (issue #388): proves ConnectionSettings can substitute
// the internal high-level request/response serializer via the new
// CreateDefaultRequestResponseSerializer() override -- the seam needed to make
// System.Text.Json the high-level default. (The default remains Utf8Json; this
// only proves the override is honored end-to-end through the settings.)
public class HighLevelSeamTests
{
    public sealed class Doc
    {
        public string Name { get; set; } = "";
        public int Count { get; set; }
    }

    [Fact]
    public void Overridden_high_level_serializer_is_used_for_requests()
    {
        RecordingSerializer.Reset();

        var settings = new StjConnectionSettings(
            new SingleNodeConnectionPool(new Uri("http://localhost:9200")),
            new InMemoryConnection());

        var json = ((IConnectionSettingsValues)settings)
            .RequestResponseSerializer
            .SerializeToString(new Doc { Name = "x", Count = 1 });

        Assert.True(RecordingSerializer.WriteCalls > 0, "overridden STJ serializer should handle request serialization");
        Assert.Equal("{\"Name\":\"x\",\"Count\":1}", json); // System.Text.Json default output
    }

    [Fact]
    public void Default_settings_do_not_use_the_override()
    {
        RecordingSerializer.Reset();

        var settings = new ConnectionSettings(
            new SingleNodeConnectionPool(new Uri("http://localhost:9200")),
            new InMemoryConnection());

        _ = ((IConnectionSettingsValues)settings).RequestResponseSerializer.SerializeToString(new Doc { Name = "y", Count = 2 });

        Assert.Equal(0, RecordingSerializer.WriteCalls); // stock settings keep the Utf8Json default
    }

    // Custom settings that swap the internal high-level serializer for a
    // System.Text.Json one (wrapped to record usage).
    private sealed class StjConnectionSettings : ConnectionSettings
    {
        public StjConnectionSettings(IConnectionPool pool, IConnection connection) : base(pool, connection) { }

        protected override IOpenSearchSerializer CreateDefaultRequestResponseSerializer() =>
            new RecordingSerializer(new SystemTextJsonSerializer());
    }

    // Delegating IOpenSearchSerializer that records that serialization ran.
    private sealed class RecordingSerializer : IOpenSearchSerializer
    {
        public static int WriteCalls;
        public static void Reset() => WriteCalls = 0;

        private readonly IOpenSearchSerializer _inner;
        public RecordingSerializer(IOpenSearchSerializer inner) => _inner = inner;

        public object Deserialize(Type type, Stream stream) => _inner.Deserialize(type, stream);
        public T Deserialize<T>(Stream stream) => _inner.Deserialize<T>(stream);
        public Task<object> DeserializeAsync(Type type, Stream stream, CancellationToken ct = default) => _inner.DeserializeAsync(type, stream, ct);
        public Task<T> DeserializeAsync<T>(Stream stream, CancellationToken ct = default) => _inner.DeserializeAsync<T>(stream, ct);

        public void Serialize<T>(T data, Stream stream, SerializationFormatting formatting = SerializationFormatting.None)
        {
            Interlocked.Increment(ref WriteCalls);
            _inner.Serialize(data, stream, formatting);
        }

        public Task SerializeAsync<T>(T data, Stream stream, SerializationFormatting formatting = SerializationFormatting.None, CancellationToken ct = default)
        {
            Interlocked.Increment(ref WriteCalls);
            return _inner.SerializeAsync(data, stream, formatting, ct);
        }
    }
}
