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

namespace OpenSearch.Client.Search.SystemTextJsonConverters;

/// <summary>
/// System.Text.Json converter for <see cref="IMultiSearchRequest"/>.
/// NDJSON format: alternating header lines and body lines.
/// Header: {"index": "...", "search_type": "..."}
/// Body: full search request body
/// Write-only (responses handled by <see cref="MultiSearchResponseConverter"/>).
/// </summary>
internal sealed class MultiSearchRequestConverter : JsonConverter<IMultiSearchRequest>
{
	private readonly IConnectionSettingsValues _settings;

	public MultiSearchRequestConverter(IConnectionSettingsValues settings)
	{
		_settings = settings;
	}

	public override IMultiSearchRequest? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		// Multi-search requests are write-only in NDJSON format
		throw new NotSupportedException(
			"Deserialization of IMultiSearchRequest is not supported. " +
			"Multi-search requests use NDJSON format which cannot be read as standard JSON.");
	}

	public override void Write(Utf8JsonWriter writer, IMultiSearchRequest value, JsonSerializerOptions options)
	{
		if (value?.Operations == null)
		{
			writer.WriteNullValue();
			return;
		}

		// NDJSON cannot be written via Utf8JsonWriter directly since it's not valid JSON.
		// This converter writes a JSON array representation for System.Text.Json compatibility.
		// The actual NDJSON serialization is handled by the transport layer.
		writer.WriteStartArray();

		foreach (var operation in value.Operations)
		{
			var searchRequest = operation.Value;

			// Write header object
			writer.WriteStartObject();

			if (searchRequest.Index != null)
			{
				var indexStr = _settings != null
					? ((IUrlParameter)searchRequest.Index).GetString(_settings)
					: searchRequest.Index.ToString();

				if (!string.IsNullOrEmpty(indexStr))
					writer.WriteString("index", indexStr);
			}

			var requestParameters = searchRequest.RequestParameters;
			if (requestParameters != null)
			{
				var searchType = requestParameters.GetResolvedQueryStringValue("search_type", _settings);
				if (!string.IsNullOrEmpty(searchType) && searchType != "query_then_fetch")
					writer.WriteString("search_type", searchType);

				var preference = requestParameters.GetResolvedQueryStringValue("preference", _settings);
				if (!string.IsNullOrEmpty(preference))
					writer.WriteString("preference", preference);

				var routing = requestParameters.GetResolvedQueryStringValue("routing", _settings);
				if (!string.IsNullOrEmpty(routing))
					writer.WriteString("routing", routing);

				var ignoreUnavailable = requestParameters.GetResolvedQueryStringValue("ignore_unavailable", _settings);
				if (!string.IsNullOrEmpty(ignoreUnavailable))
					writer.WriteString("ignore_unavailable", ignoreUnavailable);
			}

			writer.WriteEndObject();

			// Write body (the search request itself)
			JsonSerializer.Serialize(writer, searchRequest, searchRequest.GetType(), options);
		}

		writer.WriteEndArray();
	}
}
