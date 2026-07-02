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

using System.Reflection;
#nullable enable

using System.Text.Json;
#nullable enable

using System.Text.Json.Serialization;
#nullable enable

using OpenSearch.Net;

namespace OpenSearch.Client.Search.SystemTextJsonConverters;

/// <summary>
/// System.Text.Json converter for <see cref="MultiSearchResponse"/>.
/// Deserializes: {"took": N, "responses": [{response1}, {response2}, ...]}
/// Each response is a full search response typed according to the original request.
/// </summary>
internal sealed class MultiSearchResponseConverter : JsonConverter<MultiSearchResponse>
{
	private static readonly MethodInfo CreateSearchResponseMethodInfo =
		typeof(MultiSearchResponseConverter).GetMethod(nameof(CreateSearchResponse), BindingFlags.Static | BindingFlags.NonPublic)!;

	private readonly IRequest? _request;

	public MultiSearchResponseConverter() : this(null) { }

	public MultiSearchResponseConverter(IRequest? request) => _request = request;

	public override MultiSearchResponse? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType == JsonTokenType.Null)
			return null;

		if (reader.TokenType != JsonTokenType.StartObject)
			throw new JsonException($"Expected StartObject for MultiSearchResponse but got {reader.TokenType}");

		var response = new MultiSearchResponse();
		var rawResponses = new List<string>();
		long took = 0;

		while (reader.Read())
		{
			if (reader.TokenType == JsonTokenType.EndObject)
				break;

			if (reader.TokenType != JsonTokenType.PropertyName)
				continue;

			var propertyName = reader.GetString();
			reader.Read();

			switch (propertyName)
			{
				case "took":
					took = reader.GetInt64();
					break;
				case "responses":
					if (reader.TokenType == JsonTokenType.StartArray)
					{
						while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
						{
							using var doc = JsonDocument.ParseValue(ref reader);
							rawResponses.Add(doc.RootElement.GetRawText());
						}
					}
					break;
				default:
					reader.Skip();
					break;
			}
		}

		response.Took = took;

		if (rawResponses.Count == 0 || _request == null)
			return response;

		// Match responses to the original request operations
		IEnumerable<KeyValuePair<string, ITypedSearchRequest>>? operations = _request switch
		{
			IMultiSearchRequest multiSearch => multiSearch.Operations?
				.Select(kvp => new KeyValuePair<string, ITypedSearchRequest>(kvp.Key, kvp.Value)),
			_ => null
		};

		if (operations == null)
			return response;

		var operationsList = operations.ToList();
		var responsesDict = response.Responses;

		for (var i = 0; i < rawResponses.Count && i < operationsList.Count; i++)
		{
			var rawJson = rawResponses[i];
			var operation = operationsList[i];
			var clrType = operation.Value?.ClrType ?? typeof(object);

			try
			{
				var method = CreateSearchResponseMethodInfo.MakeGenericMethod(clrType);
				method.Invoke(null, new object[] { rawJson, operation.Key, options, responsesDict });
			}
			catch
			{
				// If deserialization fails for a specific type, try as object
				var searchResponse = JsonSerializer.Deserialize<SearchResponse<object>>(rawJson, options);
				if (searchResponse != null)
					responsesDict[operation.Key] = searchResponse;
			}
		}

		return response;
	}

	public override void Write(Utf8JsonWriter writer, MultiSearchResponse value, JsonSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNullValue();
			return;
		}

		writer.WriteStartObject();
		writer.WriteNumber("took", value.Took);

		writer.WritePropertyName("responses");
		writer.WriteStartArray();
		foreach (var resp in value.Responses.Values)
		{
			JsonSerializer.Serialize(writer, resp, resp.GetType(), options);
		}
		writer.WriteEndArray();

		writer.WriteEndObject();
	}

	private static void CreateSearchResponse<T>(
		string rawJson,
		string key,
		JsonSerializerOptions options,
		IDictionary<string, IResponse> collection) where T : class
	{
		var response = JsonSerializer.Deserialize<SearchResponse<T>>(rawJson, options);
		if (response != null)
			collection[key] = response;
	}
}
