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

namespace OpenSearch.Client.Indices.SystemTextJsonConverters;

/// <summary>
/// Polymorphic converter for <see cref="ISimilarity"/>.
/// Reads the "type" discriminator to dispatch to the correct concrete similarity type:
/// BM25, DFR, DFI, IB, LMDirichlet, LMJelinekMercer, scripted.
/// </summary>
internal sealed class SimilarityConverter : JsonConverter<ISimilarity>
{
	public override ISimilarity? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType == JsonTokenType.Null)
			return null;

		if (reader.TokenType != JsonTokenType.StartObject)
			throw new JsonException($"Expected StartObject for ISimilarity but got {reader.TokenType}");

		using var doc = JsonDocument.ParseValue(ref reader);
		var root = doc.RootElement;

		var typeString = root.TryGetProperty("type", out var typeProp)
			? typeProp.GetString()
			: null;

		var rawJson = root.GetRawText();
		return DeserializeByType(rawJson, typeString, options);
	}

	private static ISimilarity? DeserializeByType(string rawJson, string? typeString, JsonSerializerOptions options)
	{
		switch (typeString)
		{
			case "BM25":
				return JsonSerializer.Deserialize<BM25Similarity>(rawJson, options);
			case "DFR":
				return JsonSerializer.Deserialize<DFRSimilarity>(rawJson, options);
			case "DFI":
				return JsonSerializer.Deserialize<DFISimilarity>(rawJson, options);
			case "IB":
				return JsonSerializer.Deserialize<IBSimilarity>(rawJson, options);
			case "LMDirichlet":
				return JsonSerializer.Deserialize<LMDirichletSimilarity>(rawJson, options);
			case "LMJelinekMercer":
				return JsonSerializer.Deserialize<LMJelinekMercerSimilarity>(rawJson, options);
			case "scripted":
				return JsonSerializer.Deserialize<ScriptedSimilarity>(rawJson, options);
			default:
				// Unknown or custom similarity - deserialize as CustomSimilarity
				var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(rawJson, options);
				return dict != null ? new CustomSimilarity(dict) : null;
		}
	}

	public override void Write(Utf8JsonWriter writer, ISimilarity value, JsonSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNullValue();
			return;
		}

		switch (value.Type)
		{
			case "BM25":
				JsonSerializer.Serialize(writer, (BM25Similarity)value, options);
				break;
			case "DFR":
				JsonSerializer.Serialize(writer, (DFRSimilarity)value, options);
				break;
			case "DFI":
				JsonSerializer.Serialize(writer, (DFISimilarity)value, options);
				break;
			case "IB":
				JsonSerializer.Serialize(writer, (IBSimilarity)value, options);
				break;
			case "LMDirichlet":
				JsonSerializer.Serialize(writer, (LMDirichletSimilarity)value, options);
				break;
			case "LMJelinekMercer":
				JsonSerializer.Serialize(writer, (LMJelinekMercerSimilarity)value, options);
				break;
			case "scripted":
				JsonSerializer.Serialize(writer, (ScriptedSimilarity)value, options);
				break;
			default:
				// Custom or unknown similarity
				JsonSerializer.Serialize(writer, value, value.GetType(), options);
				break;
		}
	}
}
