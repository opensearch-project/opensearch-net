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
	/// A <see cref="System.Text.Json"/> converter that (de)serializes the polymorphic
	/// <see cref="ITokenizer"/> hierarchy by dispatching on the <c>type</c> discriminator, replacing
	/// the vendored Utf8Json <c>TokenizerFormatter</c> as part of the migration tracked by #388.
	/// <para>
	/// On write the concrete runtime type is serialized (its property names are resolved from the
	/// <c>[DataMember]</c> attributes by <see cref="OpenSearch.Net.DataContractResolver"/>). On read
	/// the <c>type</c> property selects the concrete type to deserialize into.
	/// </para>
	/// <para>
	/// The dispatch table mirrors <c>TokenizerFormatter</c> and additionally registers
	/// <c>keyword</c>, <c>letter</c> and <c>lowercase</c> — concrete types that exist in the client
	/// but were absent from the hand-written read dispatch. This closes that drift, matching the
	/// table produced by the converter generator (see <c>ConverterSpikeGenerator</c>).
	/// </para>
	/// </summary>
	internal sealed class TokenizerInterfaceConverter : JsonConverter<ITokenizer>
	{
		private static readonly IReadOnlyDictionary<string, Type> TypeByDiscriminator = new Dictionary<string, Type>(StringComparer.Ordinal)
		{
			{ "char_group", typeof(CharGroupTokenizer) },
			{ "edgengram", typeof(EdgeNGramTokenizer) },
			{ "edge_ngram", typeof(EdgeNGramTokenizer) },
			{ "ngram", typeof(NGramTokenizer) },
			{ "path_hierarchy", typeof(PathHierarchyTokenizer) },
			{ "pattern", typeof(PatternTokenizer) },
			{ "standard", typeof(StandardTokenizer) },
			{ "uax_url_email", typeof(UaxEmailUrlTokenizer) },
			{ "whitespace", typeof(WhitespaceTokenizer) },
			{ "keyword", typeof(KeywordTokenizer) },
			{ "letter", typeof(LetterTokenizer) },
			{ "lowercase", typeof(LowercaseTokenizer) },
			{ "kuromoji_tokenizer", typeof(KuromojiTokenizer) },
			{ "icu_tokenizer", typeof(IcuTokenizer) },
			{ "nori_tokenizer", typeof(NoriTokenizer) },
		};

		public override ITokenizer Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null) return null;

			using var document = JsonDocument.ParseValue(ref reader);
			var root = document.RootElement;

			if (root.ValueKind != JsonValueKind.Object
				|| !root.TryGetProperty("type", out var typeProperty)
				|| typeProperty.ValueKind != JsonValueKind.String)
				return null;

			var discriminator = typeProperty.GetString();
			if (discriminator == null || !TypeByDiscriminator.TryGetValue(discriminator, out var concreteType))
				return null;

			return (ITokenizer)root.Deserialize(concreteType, options);
		}

		public override void Write(Utf8JsonWriter writer, ITokenizer value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			// Serialize the concrete runtime type; the type is not ITokenizer so this does not recurse.
			JsonSerializer.Serialize(writer, value, value.GetType(), options);
		}
	}
}
