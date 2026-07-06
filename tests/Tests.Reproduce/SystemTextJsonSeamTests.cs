/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.Net;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;

namespace Tests.Reproduce
{
	// Regression protection for the high-level serializer seam added for the
	// System.Text.Json migration (#388): ConnectionSettings must honor an
	// overridden CreateDefaultRequestResponseSerializer(), and by default the
	// System.Text.Json serializer backs both the request/response serializer and
	// a distinct source serializer that applies document field-name inference.
	public class SystemTextJsonSeamTests
	{
		private class Doc
		{
			public string Name { get; set; }
			public int Count { get; set; }
		}

		[U]
		public void DefaultSourceSerializerIsDistinctAndAppliesDocumentInference()
		{
			var settings = new ConnectionSettings(new InMemoryConnection())
				.DefaultFieldNameInferrer(p => p.ToUpperInvariant());
			var values = (IConnectionSettingsValues)settings;

			// The default source serializer is a distinct instance from the request/response serializer
			// (previously it was the same instance when no source serializer factory was supplied).
			values.SourceSerializer.Should().NotBeSameAs(values.RequestResponseSerializer);

			var doc = new Doc { Name = "x", Count = 1 };

			// The source serializer applies the client's document field-name inference (here the custom
			// uppercasing DefaultFieldNameInferrer); the request/response serializer camel-cases
			// un-attributed members but does not apply document inference.
			values.SourceSerializer.SerializeToString(doc).Should().Be("{\"NAME\":\"x\",\"COUNT\":1}");
			values.RequestResponseSerializer.SerializeToString(doc).Should().Be("{\"name\":\"x\",\"count\":1}");
		}

		[U]
		public void OverriddenSerializerIsUsedForRequests()
		{
			var settings = new StjSettings(new InMemoryConnection());

			var json = ((IConnectionSettingsValues)settings).RequestResponseSerializer
				.SerializeToString(new Doc { Name = "x", Count = 1 });

			settings.Recorder.WriteCalls.Should().BeGreaterThan(0, "the overridden serializer should be used");
			json.Should().Be("{\"Name\":\"x\",\"Count\":1}");
		}

		private class StjSettings : ConnectionSettings
		{
			// Captures the serializer created by the seam so the test can assert on it
			// without relying on shared static state.
			public RecordingSerializer Recorder { get; private set; }

			public StjSettings(InMemoryConnection connection) : base(connection) { }

			protected override IOpenSearchSerializer CreateDefaultRequestResponseSerializer() =>
				Recorder = new RecordingSerializer(new SystemTextJsonSerializer());
		}

		private class RecordingSerializer : IOpenSearchSerializer
		{
			public int WriteCalls;

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
}
