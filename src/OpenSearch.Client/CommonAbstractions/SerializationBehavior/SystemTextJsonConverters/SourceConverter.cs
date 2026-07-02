/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

#nullable enable

using System;
#nullable enable

using System.Buffers;
#nullable enable

using System.IO;
#nullable enable

using System.Text.Json;
#nullable enable

using System.Text.Json.Serialization;

namespace OpenSearch.Client.SystemTextJsonConverters;

/// <summary>
/// A converter factory that delegates serialization of user document types (T in generic APIs)
/// to the configured SourceSerializer. This replaces the SourceFormatter&lt;T&gt; from Utf8Json.
/// </summary>
internal sealed class SourceConverterFactory : JsonConverterFactory
{
	private readonly IConnectionSettingsValues _settings;

	public SourceConverterFactory(IConnectionSettingsValues settings) => _settings = settings;

	public override bool CanConvert(Type typeToConvert) =>
		// This factory should only be used for types that are explicitly registered as source types.
		// The resolution logic should be controlled by the serializer infrastructure.
		!IsOpenSearchClientType(typeToConvert);

	public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
	{
		var converterType = typeof(SourceConverter<>).MakeGenericType(typeToConvert);
		return (JsonConverter?)Activator.CreateInstance(converterType, _settings);
	}

	/// <summary>
	/// Determines if a type belongs to the OpenSearch.Client namespace and should NOT be
	/// handled by the source serializer.
	/// </summary>
	private static bool IsOpenSearchClientType(Type type)
	{
		var ns = type.Namespace;
		return ns != null && (ns.StartsWith("OpenSearch.Client", StringComparison.Ordinal)
			|| ns.StartsWith("OpenSearch.Net", StringComparison.Ordinal));
	}
}

internal sealed class SourceConverter<T> : JsonConverter<T>
{
	private readonly IConnectionSettingsValues _settings;

	public SourceConverter(IConnectionSettingsValues settings) => _settings = settings;

	public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		// Capture the raw JSON for the current value
		using var document = JsonDocument.ParseValue(ref reader);
		var buffer = new ArrayBufferWriter<byte>();
		using (var jsonWriter = new Utf8JsonWriter(buffer))
		{
			document.RootElement.WriteTo(jsonWriter);
		}

		var bytes = buffer.WrittenMemory;
		using var stream = new MemoryStream(bytes.ToArray());

		var sourceSerializer = _settings.SourceSerializer;
		return sourceSerializer.Deserialize<T>(stream);
	}

	public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNullValue();
			return;
		}

		var sourceSerializer = _settings.SourceSerializer;
		using var stream = new MemoryStream();
		sourceSerializer.Serialize(value, stream);
		stream.Position = 0;

		// Write the raw JSON produced by the source serializer
		using var document = JsonDocument.Parse(stream);
		document.RootElement.WriteTo(writer);
	}
}
