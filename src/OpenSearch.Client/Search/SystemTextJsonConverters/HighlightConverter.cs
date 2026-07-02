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

namespace OpenSearch.Client.Search.SystemTextJsonConverters;

/// <summary>
/// System.Text.Json converter for <see cref="IHighlight"/>.
/// Format: {"fields": {"field1": {options}, "field2": {options}}, "pre_tags": [...], "post_tags": [...], ...}
/// </summary>
internal sealed class HighlightConverter : JsonConverter<IHighlight>
{
	public override IHighlight? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType == JsonTokenType.Null)
			return null;

		if (reader.TokenType != JsonTokenType.StartObject)
			throw new JsonException($"Expected StartObject for IHighlight but got {reader.TokenType}");

		using var doc = JsonDocument.ParseValue(ref reader);
		var root = doc.RootElement;

		var highlight = new Highlight();

		foreach (var property in root.EnumerateObject())
		{
			switch (property.Name)
			{
				case "fields":
					if (property.Value.ValueKind == JsonValueKind.Object)
					{
						var fields = new Dictionary<Field, IHighlightField>();
						foreach (var fieldProp in property.Value.EnumerateObject())
						{
							var field = new Field(fieldProp.Name);
							var highlightField = JsonSerializer.Deserialize<HighlightField>(fieldProp.Value.GetRawText(), options);
							if (highlightField != null)
								fields[field] = highlightField;
						}
						highlight.Fields = fields;
					}
					break;
				case "pre_tags":
					highlight.PreTags = JsonSerializer.Deserialize<IEnumerable<string>>(property.Value.GetRawText(), options);
					break;
				case "post_tags":
					highlight.PostTags = JsonSerializer.Deserialize<IEnumerable<string>>(property.Value.GetRawText(), options);
					break;
				case "fragment_size":
					highlight.FragmentSize = property.Value.GetInt32();
					break;
				case "number_of_fragments":
					highlight.NumberOfFragments = property.Value.GetInt32();
					break;
				case "fragment_offset":
					highlight.FragmentOffset = property.Value.GetInt32();
					break;
				case "boundary_chars":
					highlight.BoundaryChars = property.Value.GetString();
					break;
				case "boundary_max_scan":
					highlight.BoundaryMaxScan = property.Value.GetInt32();
					break;
				case "boundary_scanner":
					var scannerStr = property.Value.GetString();
					if (!string.IsNullOrEmpty(scannerStr))
					{
						highlight.BoundaryScanner = scannerStr switch
						{
							"chars" => Client.BoundaryScanner.Chars,
							"sentence" => Client.BoundaryScanner.Sentence,
							"word" => Client.BoundaryScanner.Word,
							_ => null
						};
					}
					break;
				case "boundary_scanner_locale":
					highlight.BoundaryScannerLocale = property.Value.GetString();
					break;
				case "encoder":
					var encoderStr = property.Value.GetString();
					if (!string.IsNullOrEmpty(encoderStr))
					{
						highlight.Encoder = encoderStr switch
						{
							"default" => HighlighterEncoder.Default,
							"html" => HighlighterEncoder.Html,
							_ => null
						};
					}
					break;
				case "order":
					var orderStr = property.Value.GetString();
					if (!string.IsNullOrEmpty(orderStr))
					{
						highlight.Order = orderStr switch
						{
							"score" => HighlighterOrder.Score,
							_ => null
						};
					}
					break;
				case "tags_schema":
					var schemaStr = property.Value.GetString();
					if (!string.IsNullOrEmpty(schemaStr))
					{
						highlight.TagsSchema = schemaStr switch
						{
							"styled" => HighlighterTagsSchema.Styled,
							_ => null
						};
					}
					break;
				case "require_field_match":
					highlight.RequireFieldMatch = property.Value.GetBoolean();
					break;
				case "max_analyzer_offset":
					highlight.MaxAnalyzerOffset = property.Value.GetInt32();
					break;
			}
		}

		return highlight;
	}

	public override void Write(Utf8JsonWriter writer, IHighlight value, JsonSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNullValue();
			return;
		}

		writer.WriteStartObject();

		// Write fields
		if (value.Fields != null)
		{
			writer.WritePropertyName("fields");
			writer.WriteStartObject();
			foreach (var kvp in value.Fields)
			{
				var fieldName = kvp.Key?.ToString();
				if (fieldName != null)
				{
					writer.WritePropertyName(fieldName);
					JsonSerializer.Serialize(writer, kvp.Value, options);
				}
			}
			writer.WriteEndObject();
		}

		// Write highlight-level options
		if (value.PreTags != null)
		{
			writer.WritePropertyName("pre_tags");
			JsonSerializer.Serialize(writer, value.PreTags, options);
		}

		if (value.PostTags != null)
		{
			writer.WritePropertyName("post_tags");
			JsonSerializer.Serialize(writer, value.PostTags, options);
		}

		if (value.FragmentSize.HasValue)
			writer.WriteNumber("fragment_size", value.FragmentSize.Value);

		if (value.NumberOfFragments.HasValue)
			writer.WriteNumber("number_of_fragments", value.NumberOfFragments.Value);

		if (value.FragmentOffset.HasValue)
			writer.WriteNumber("fragment_offset", value.FragmentOffset.Value);

		if (value.BoundaryChars != null)
			writer.WriteString("boundary_chars", value.BoundaryChars);

		if (value.BoundaryMaxScan.HasValue)
			writer.WriteNumber("boundary_max_scan", value.BoundaryMaxScan.Value);

		if (value.BoundaryScanner.HasValue)
		{
			var scannerStr = value.BoundaryScanner.Value switch
			{
				Client.BoundaryScanner.Chars => "chars",
				Client.BoundaryScanner.Sentence => "sentence",
				Client.BoundaryScanner.Word => "word",
				_ => null
			};
			if (scannerStr != null)
				writer.WriteString("boundary_scanner", scannerStr);
		}

		if (value.BoundaryScannerLocale != null)
			writer.WriteString("boundary_scanner_locale", value.BoundaryScannerLocale);

		if (value.Encoder.HasValue)
		{
			var encoderStr = value.Encoder.Value switch
			{
				HighlighterEncoder.Default => "default",
				HighlighterEncoder.Html => "html",
				_ => null
			};
			if (encoderStr != null)
				writer.WriteString("encoder", encoderStr);
		}

		if (value.Order.HasValue)
		{
			var orderStr = value.Order.Value switch
			{
				HighlighterOrder.Score => "score",
				_ => null
			};
			if (orderStr != null)
				writer.WriteString("order", orderStr);
		}

		if (value.TagsSchema.HasValue)
		{
			var schemaStr = value.TagsSchema.Value switch
			{
				HighlighterTagsSchema.Styled => "styled",
				_ => null
			};
			if (schemaStr != null)
				writer.WriteString("tags_schema", schemaStr);
		}

		if (value.RequireFieldMatch.HasValue)
			writer.WriteBoolean("require_field_match", value.RequireFieldMatch.Value);

		if (value.MaxAnalyzerOffset.HasValue)
			writer.WriteNumber("max_analyzer_offset", value.MaxAnalyzerOffset.Value);

		writer.WriteEndObject();
	}
}
