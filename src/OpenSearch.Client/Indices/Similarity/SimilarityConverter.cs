/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenSearch.Client
{
	/// <summary>
	/// A <see cref="System.Text.Json"/> converter for <see cref="ISimilarity"/>, replacing the vendored
	/// Utf8Json <c>SimilarityFormatter</c> as part of #388. The concrete similarity is selected from the
	/// <c>type</c> discriminator property; unrecognized types fall back to a <see cref="CustomSimilarity"/>
	/// backed by a <c>Dictionary&lt;string, object&gt;</c>.
	/// </summary>
	internal sealed class SimilarityConverter : JsonConverter<ISimilarity>
	{
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
					JsonSerializer.Serialize(writer, value as IBM25Similarity, options);
					break;
				case "LMDirichlet":
					JsonSerializer.Serialize(writer, value as ILMDirichletSimilarity, options);
					break;
				case "DFR":
					JsonSerializer.Serialize(writer, value as IDFRSimilarity, options);
					break;
				case "DFI":
					JsonSerializer.Serialize(writer, value as IDFISimilarity, options);
					break;
				case "IB":
					JsonSerializer.Serialize(writer, value as IIBSimilarity, options);
					break;
				case "LMJelinekMercer":
					JsonSerializer.Serialize(writer, value as ILMJelinekMercerSimilarity, options);
					break;
				case "scripted":
					JsonSerializer.Serialize(writer, value as IScriptedSimilarity, options);
					break;
				default:
					JsonSerializer.Serialize(writer, value as ICustomSimilarity, options);
					break;
			}
		}

		public override ISimilarity Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null) return null;

			using var document = JsonDocument.ParseValue(ref reader);
			var root = document.RootElement;
			if (root.ValueKind != JsonValueKind.Object) return null;

			string type = null;
			if (root.TryGetProperty("type", out var typeElement) && typeElement.ValueKind == JsonValueKind.String)
				type = typeElement.GetString();

			switch (type)
			{
				case "BM25":
					return root.Deserialize<BM25Similarity>(options);
				case "LMDirichlet":
					return root.Deserialize<LMDirichletSimilarity>(options);
				case "DFR":
					return root.Deserialize<DFRSimilarity>(options);
				case "DFI":
					return root.Deserialize<DFISimilarity>(options);
				case "IB":
					return root.Deserialize<IBSimilarity>(options);
				case "LMJelinekMercer":
					return root.Deserialize<LMJelinekMercerSimilarity>(options);
				case "scripted":
					return root.Deserialize<ScriptedSimilarity>(options);
				default:
					var dict = root.Deserialize<Dictionary<string, object>>(options);
					return new CustomSimilarity(dict);
			}
		}
	}
}
