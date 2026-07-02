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

using System.Text.Json;
#nullable enable

using System.Text.Json.Serialization;
#nullable enable

using OpenSearch.Net;

namespace OpenSearch.Client.CommonOptions.SystemTextJsonConverters;

internal sealed class SortConverter : JsonConverter<ISort>
{
	private readonly IConnectionSettingsValues _settings;

	public SortConverter(IConnectionSettingsValues settings) => _settings = settings;

	public override ISort? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType == JsonTokenType.Null)
			return null;

		if (reader.TokenType == JsonTokenType.String)
		{
			var field = reader.GetString();
			return new FieldSort { Field = field };
		}

		if (reader.TokenType != JsonTokenType.StartObject)
			throw new JsonException($"Unexpected token type '{reader.TokenType}' when reading ISort.");

		reader.Read();
		if (reader.TokenType != JsonTokenType.PropertyName)
			throw new JsonException("Expected property name in sort object.");

		var propertyName = reader.GetString()!;
		reader.Read();

		ISort? sort;

		switch (propertyName)
		{
			case "_script":
				sort = JsonSerializer.Deserialize<ScriptSort>(ref reader, options);
				break;
			case "_geo_distance":
				sort = JsonSerializer.Deserialize<GeoDistanceSort>(ref reader, options);
				break;
			default:
				sort = ReadFieldSort(ref reader, propertyName, options);
				break;
		}

		reader.Read(); // end object
		return sort;
	}

	private static ISort ReadFieldSort(ref Utf8JsonReader reader, string fieldName, JsonSerializerOptions options)
	{
		if (reader.TokenType == JsonTokenType.String)
		{
			var orderStr = reader.GetString();
			SortOrder? order = orderStr?.ToLowerInvariant() switch
			{
				"asc" => SortOrder.Ascending,
				"desc" => SortOrder.Descending,
				_ => null
			};
			return new FieldSort { Field = fieldName, Order = order };
		}

		var fieldSort = JsonSerializer.Deserialize<FieldSort>(ref reader, options) ?? new FieldSort();
		fieldSort.Field = fieldName;
		return fieldSort;
	}

	public override void Write(Utf8JsonWriter writer, ISort value, JsonSerializerOptions options)
	{
		if (value?.SortKey == null)
		{
			writer.WriteNullValue();
			return;
		}

		writer.WriteStartObject();

		var fieldName = value.SortKey.Name ?? _settings.Inferrer.Field(value.SortKey);

		switch (value)
		{
			case IScriptSort scriptSort:
				writer.WritePropertyName("_script");
				JsonSerializer.Serialize(writer, scriptSort, options);
				break;
			case IGeoDistanceSort geoDistanceSort:
				writer.WritePropertyName("_geo_distance");
				JsonSerializer.Serialize(writer, geoDistanceSort, options);
				break;
			default:
				writer.WritePropertyName(fieldName);
				if (value is IFieldSort fieldSort)
					JsonSerializer.Serialize(writer, fieldSort, options);
				else
					JsonSerializer.Serialize(writer, value, value.GetType(), options);
				break;
		}

		writer.WriteEndObject();
	}
}
