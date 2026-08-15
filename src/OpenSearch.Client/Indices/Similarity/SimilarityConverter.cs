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
	/// System.Text.Json replacement for the legacy Utf8Json <c>SimilarityFormatter</c>.
	///
	/// <see cref="ISimilarity"/> is polymorphic: the concrete type is selected by the value of the
	/// <c>type</c> field. System.Text.Json's <see cref="Utf8JsonReader"/> is forward-only and cannot be
	/// rewound, so — unlike the Utf8Json version which peeked at a byte segment and re-read it — we buffer
	/// the value into a <see cref="JsonDocument"/>, read the <c>type</c> discriminator from the DOM, then
	/// deserialize the whole element as the matching concrete type. Any unrecognized (or missing) type is
	/// treated as a user-defined <see cref="CustomSimilarity"/>, matching the legacy behaviour.
	/// Serialization writes by runtime type.
	/// </summary>
	internal class SimilarityConverter : JsonConverter<ISimilarity>
	{
		public override ISimilarity Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null)
				return null;

			using var doc = JsonDocument.ParseValue(ref reader);
			var root = doc.RootElement;

			var similarityType = root.TryGetProperty("type", out var t) ? t.GetString() : null;

			switch (similarityType)
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
					// Any unrecognized or missing type is a user-defined similarity, matching the legacy formatter.
					var dict = root.Deserialize<Dictionary<string, object>>(options);
					return new CustomSimilarity(dict);
			}
		}

		public override void Write(Utf8JsonWriter writer, ISimilarity value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			JsonSerializer.Serialize(writer, value, value.GetType(), options);
		}
	}
}
