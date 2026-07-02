/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

#nullable enable

using System;
#nullable enable

using System.Collections.Generic;
#nullable enable

using System.Linq;
#nullable enable

using System.Text.Json;
#nullable enable

using System.Text.Json.Serialization;
#nullable enable

using OpenSearch.Net;

namespace OpenSearch.Client.Document.SystemTextJsonConverters;

/// <summary>
/// Converts <see cref="MultiGetResponse"/> from the server response format.
/// Reads: {"docs": [{"_index": "...", "_id": "...", "found": true, "_source": {...}}, ...]}
/// Handles found/not-found cases and generic _source deserialization using the request's type information.
/// </summary>
internal sealed class MultiGetResponseConverter : JsonConverter<MultiGetResponse>
{
	private readonly IConnectionSettingsValues _settings;
	private readonly IMultiGetRequest? _request;

	public MultiGetResponseConverter(IConnectionSettingsValues settings, IMultiGetRequest? request = null)
	{
		_settings = settings;
		_request = request;
	}

	public override MultiGetResponse? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType == JsonTokenType.Null)
			return null;

		var response = new MultiGetResponse();

		if (reader.TokenType != JsonTokenType.StartObject)
			throw new JsonException("Expected StartObject for MultiGetResponse.");

		var rawDocs = new List<JsonElement>();

		while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
		{
			if (reader.TokenType != JsonTokenType.PropertyName)
				continue;

			var propertyName = reader.GetString();
			reader.Read();

			if (propertyName == "docs")
			{
				if (reader.TokenType == JsonTokenType.StartArray)
				{
					while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
					{
						using var doc = JsonDocument.ParseValue(ref reader);
						rawDocs.Add(doc.RootElement.Clone());
					}
				}
			}
			else
			{
				reader.Skip();
			}
		}

		if (rawDocs.Count == 0)
			return response;

		// Pair each raw doc with its request descriptor to determine the target CLR type
		var documents = _request?.Documents?.ToList();
		for (var i = 0; i < rawDocs.Count; i++)
		{
			var rawDoc = rawDocs[i];
			var clrType = documents != null && i < documents.Count ? documents[i].ClrType : typeof(object);

			var hit = DeserializeHit(rawDoc, clrType, options);
			if (hit != null)
				response.InternalHits.Add(hit);
		}

		return response;
	}

	private IMultiGetHit<object>? DeserializeHit(JsonElement element, Type clrType, JsonSerializerOptions options)
	{
		var hit = new MultiGetHit<object>();

		if (element.TryGetProperty("_index", out var indexProp))
			hit.Index = indexProp.GetString();

		if (element.TryGetProperty("_type", out var typeProp))
			hit.Type = typeProp.GetString();

		if (element.TryGetProperty("_id", out var idProp))
			hit.Id = idProp.GetString();

		if (element.TryGetProperty("_version", out var versionProp))
			hit.Version = versionProp.GetInt64();

		if (element.TryGetProperty("_seq_no", out var seqNoProp))
			hit.SequenceNumber = seqNoProp.GetInt64();

		if (element.TryGetProperty("_primary_term", out var primaryTermProp))
			hit.PrimaryTerm = primaryTermProp.GetInt64();

		if (element.TryGetProperty("_routing", out var routingProp))
			hit.Routing = routingProp.GetString();

		if (element.TryGetProperty("found", out var foundProp))
			hit.Found = foundProp.GetBoolean();

		if (element.TryGetProperty("error", out var errorProp))
			hit.Error = JsonSerializer.Deserialize<Error>(errorProp.GetRawText(), options);

		if (element.TryGetProperty("_source", out var sourceProp) && hit.Found)
		{
			var sourceJson = sourceProp.GetRawText();
			hit.Source = JsonSerializer.Deserialize(sourceJson, clrType, options);
		}

		if (element.TryGetProperty("fields", out var fieldsProp))
			hit.Fields = JsonSerializer.Deserialize<FieldValues>(fieldsProp.GetRawText(), options);

		return hit;
	}

	public override void Write(Utf8JsonWriter writer, MultiGetResponse value, JsonSerializerOptions options) =>
		throw new NotSupportedException("Serialization of MultiGetResponse is not typically required.");
}
