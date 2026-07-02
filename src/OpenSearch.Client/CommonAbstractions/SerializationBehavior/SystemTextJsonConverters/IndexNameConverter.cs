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

namespace OpenSearch.Client.SystemTextJsonConverters;

internal sealed class IndexNameConverter : JsonConverter<IndexName>
{
	private readonly IConnectionSettingsValues _settings;

	public IndexNameConverter(IConnectionSettingsValues settings) => _settings = settings;

	public override IndexName? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType == JsonTokenType.Null)
			return null;

		var indexName = reader.GetString();
		return indexName == null ? null : (IndexName)indexName;
	}

	public override void Write(Utf8JsonWriter writer, IndexName value, JsonSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNullValue();
			return;
		}

		writer.WriteStringValue(_settings.Inferrer.IndexName(value));
	}
}
