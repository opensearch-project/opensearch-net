/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

#nullable enable

using System;
#nullable enable

using System.Text.Json;
#nullable enable

using System.Text.Json.Serialization;

namespace OpenSearch.Client.Document.SystemTextJsonConverters;

/// <summary>
/// Converts <see cref="IIndexRequest{TDocument}"/> by serializing the document body directly.
/// The index request body IS the document itself - no wrapper object.
/// Delegates to the SourceSerializer (or default serializer) for the actual document serialization.
/// </summary>
internal sealed class IndexRequestConverter<TDocument> : JsonConverter<IIndexRequest<TDocument>>
	where TDocument : class
{
	private readonly IConnectionSettingsValues _settings;

	public IndexRequestConverter(IConnectionSettingsValues settings) => _settings = settings;

	public override IIndexRequest<TDocument>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		// Index requests are typically only serialized (written), not deserialized
		// The response is a different type (IndexResponse)
		if (reader.TokenType == JsonTokenType.Null)
			return null;

		var document = JsonSerializer.Deserialize<TDocument>(ref reader, options);
		return new IndexRequest<TDocument>(document);
	}

	public override void Write(Utf8JsonWriter writer, IIndexRequest<TDocument> value, JsonSerializerOptions options)
	{
		if (value?.Document == null)
		{
			writer.WriteNullValue();
			return;
		}

		// The body of an index request is just the document itself.
		// Use the source serializer options if available, otherwise fall back to default.
		JsonSerializer.Serialize(writer, value.Document, options);
	}
}

/// <summary>
/// Non-generic factory for creating <see cref="IndexRequestConverter{TDocument}"/> instances.
/// </summary>
internal sealed class IndexRequestConverterFactory : JsonConverterFactory
{
	private readonly IConnectionSettingsValues _settings;

	public IndexRequestConverterFactory(IConnectionSettingsValues settings) => _settings = settings;

	public override bool CanConvert(Type typeToConvert)
	{
		if (!typeToConvert.IsGenericType)
			return false;

		return typeToConvert.GetGenericTypeDefinition() == typeof(IIndexRequest<>);
	}

	public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
	{
		var documentType = typeToConvert.GetGenericArguments()[0];
		var converterType = typeof(IndexRequestConverter<>).MakeGenericType(documentType);
		return (JsonConverter)Activator.CreateInstance(converterType, _settings)!;
	}
}
