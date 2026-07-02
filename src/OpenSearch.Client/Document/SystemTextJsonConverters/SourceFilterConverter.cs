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

namespace OpenSearch.Client.Document.SystemTextJsonConverters;

/// <summary>
/// Converts <see cref="ISourceFilter"/> which can be represented as:
/// - boolean (true/false) to enable/disable source
/// - string (single field name)
/// - array of strings (field names to include)
/// - object {"includes": [...], "excludes": [...]}
/// </summary>
internal sealed class SourceFilterConverter : JsonConverter<ISourceFilter>
{
	private readonly IConnectionSettingsValues _settings;

	public SourceFilterConverter(IConnectionSettingsValues settings) => _settings = settings;

	public override ISourceFilter? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType == JsonTokenType.Null)
			return null;

		switch (reader.TokenType)
		{
			case JsonTokenType.True:
			case JsonTokenType.False:
			{
				var enabled = reader.GetBoolean();
				// false means exclude all, true means include all
				return enabled ? SourceFilter.IncludeAll : SourceFilter.ExcludeAll;
			}

			case JsonTokenType.String:
			{
				var fieldName = reader.GetString();
				return new SourceFilter { Includes = fieldName != null ? new[] { fieldName } : null };
			}

			case JsonTokenType.StartArray:
			{
				var fields = new List<string>();
				while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
				{
					if (reader.TokenType == JsonTokenType.String)
						fields.Add(reader.GetString()!);
				}
				return new SourceFilter { Includes = fields.ToArray() };
			}

			case JsonTokenType.StartObject:
			{
				var filter = new SourceFilter();
				while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
				{
					if (reader.TokenType != JsonTokenType.PropertyName)
						continue;

					var propertyName = reader.GetString();
					reader.Read();

					switch (propertyName)
					{
						case "includes" or "include":
							filter.Includes = ReadFieldsArray(ref reader);
							break;
						case "excludes" or "exclude":
							filter.Excludes = ReadFieldsArray(ref reader);
							break;
						default:
							reader.Skip();
							break;
					}
				}
				return filter;
			}

			default:
				throw new JsonException($"Unexpected token type '{reader.TokenType}' when reading ISourceFilter.");
		}
	}

	public override void Write(Utf8JsonWriter writer, ISourceFilter value, JsonSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNullValue();
			return;
		}

		// If both includes and excludes are null/empty, write as disabled (false)
		var hasIncludes = value.Includes != null;
		var hasExcludes = value.Excludes != null;

		if (!hasIncludes && !hasExcludes)
		{
			writer.WriteBooleanValue(false);
			return;
		}

		writer.WriteStartObject();

		if (hasIncludes)
		{
			writer.WritePropertyName("includes");
			WriteFields(writer, value.Includes);
		}

		if (hasExcludes)
		{
			writer.WritePropertyName("excludes");
			WriteFields(writer, value.Excludes);
		}

		writer.WriteEndObject();
	}

	private static Fields? ReadFieldsArray(ref Utf8JsonReader reader)
	{
		if (reader.TokenType == JsonTokenType.Null)
			return null;

		if (reader.TokenType == JsonTokenType.String)
		{
			var single = reader.GetString();
			return single != null ? new[] { single } : null;
		}

		if (reader.TokenType == JsonTokenType.StartArray)
		{
			var fields = new List<string>();
			while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
			{
				if (reader.TokenType == JsonTokenType.String)
					fields.Add(reader.GetString()!);
			}
			return fields.Count > 0 ? fields.ToArray() : null;
		}

		reader.Skip();
		return null;
	}

	private void WriteFields(Utf8JsonWriter writer, Fields? fields)
	{
		if (fields == null)
		{
			writer.WriteNullValue();
			return;
		}

		writer.WriteStartArray();
		foreach (var field in fields)
		{
			var fieldName = _settings.Inferrer.Field(field);
			writer.WriteStringValue(fieldName);
		}
		writer.WriteEndArray();
	}
}
