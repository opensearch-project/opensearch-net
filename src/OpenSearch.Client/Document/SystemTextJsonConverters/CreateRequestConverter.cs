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
/// Converts <see cref="ICreateRequest{TDocument}"/> by serializing the document body directly.
/// The create request body IS the document itself - no wrapper object.
/// This follows the same pattern as <see cref="IndexRequestConverter{TDocument}"/>.
/// Delegates to the SourceSerializer (or default serializer) for the actual document serialization.
/// </summary>
internal sealed class CreateRequestConverter<TDocument> : JsonConverter<ICreateRequest<TDocument>>
	where TDocument : class
{
	private readonly IConnectionSettingsValues _settings;

	public CreateRequestConverter(IConnectionSettingsValues settings) => _settings = settings;

	public override ICreateRequest<TDocument>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		// Create requests are typically only serialized (written), not deserialized
		// The response is a different type (CreateResponse)
		if (reader.TokenType == JsonTokenType.Null)
			return null;

		var document = JsonSerializer.Deserialize<TDocument>(ref reader, options);
		return new CreateRequest<TDocument>(document);
	}

	public override void Write(Utf8JsonWriter writer, ICreateRequest<TDocument> value, JsonSerializerOptions options)
	{
		if (value?.Document == null)
		{
			writer.WriteNullValue();
			return;
		}

		// The body of a create request is just the document itself.
		// Use the source serializer options if available, otherwise fall back to default.
		JsonSerializer.Serialize(writer, value.Document, options);
	}
}

/// <summary>
/// Non-generic factory for creating <see cref="CreateRequestConverter{TDocument}"/> instances.
/// </summary>
internal sealed class CreateRequestConverterFactory : JsonConverterFactory
{
	private readonly IConnectionSettingsValues _settings;

	public CreateRequestConverterFactory(IConnectionSettingsValues settings) => _settings = settings;

	public override bool CanConvert(Type typeToConvert)
	{
		if (!typeToConvert.IsGenericType)
			return false;

		return typeToConvert.GetGenericTypeDefinition() == typeof(ICreateRequest<>);
	}

	public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
	{
		var documentType = typeToConvert.GetGenericArguments()[0];
		var converterType = typeof(CreateRequestConverter<>).MakeGenericType(documentType);
		return (JsonConverter)Activator.CreateInstance(converterType, _settings)!;
	}
}
