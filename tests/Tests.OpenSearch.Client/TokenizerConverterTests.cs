/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.Text.Json;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;

namespace Tests.OpenSearch.Client.Serialization
{
	/// <summary>
	/// Behavioural tests for <see cref="TokenizerConverter"/>: dispatches an <see cref="ITokenizer"/> to the
	/// concrete type named by the <c>type</c> discriminator field (including the <c>edgengram</c> alias), and
	/// serializes by runtime type.
	/// </summary>
	public class TokenizerConverterTests
	{
		private static JsonSerializerOptions Options()
		{
			var settings = new ConnectionSettings();
			var options = new JsonSerializerOptions
			{
				TypeInfoResolver = new HighLevelContractResolver(settings)
			};
			options.Converters.Add(new TokenizerConverter());
			return options;
		}

		[U] public void Deserialize_Standard()
		{
			var tokenizer = JsonSerializer.Deserialize<ITokenizer>(@"{""type"":""standard""}", Options());

			tokenizer.Should().BeOfType<StandardTokenizer>();
			tokenizer.Type.Should().Be("standard");
		}

		[U] public void Deserialize_Whitespace()
		{
			var tokenizer = JsonSerializer.Deserialize<ITokenizer>(@"{""type"":""whitespace""}", Options());
			tokenizer.Should().BeOfType<WhitespaceTokenizer>();
		}

		[U] public void Deserialize_EdgeNGram_ByCanonicalType()
		{
			var tokenizer = JsonSerializer.Deserialize<ITokenizer>(@"{""type"":""edge_ngram""}", Options());
			tokenizer.Should().BeOfType<EdgeNGramTokenizer>();
		}

		[U] public void Deserialize_EdgeNGram_ByAlias()
		{
			// The legacy formatter mapped both "edgengram" and "edge_ngram" to EdgeNGramTokenizer.
			var tokenizer = JsonSerializer.Deserialize<ITokenizer>(@"{""type"":""edgengram""}", Options());
			tokenizer.Should().BeOfType<EdgeNGramTokenizer>();
		}

		[U] public void Deserialize_NGram_WithNumericGrams()
		{
			// [DataMember(Name="min_gram"/"max_gram")] on INGramTokenizer is now authoritative (the resolver honours
			// explicit DataMember names even on types not marked [InterfaceDataContract]), so the wire names are
			// snake_case regardless of the field-name inferrer.
			var tokenizer = JsonSerializer.Deserialize<ITokenizer>(
				@"{""type"":""ngram"",""min_gram"":1,""max_gram"":2}", Options());

			tokenizer.Should().BeOfType<NGramTokenizer>();
			var ngram = (INGramTokenizer)tokenizer;
			ngram.MinGram.Should().Be(1);
			ngram.MaxGram.Should().Be(2);
		}

		[U] public void Deserialize_UnknownType_ReturnsNull()
		{
			var tokenizer = JsonSerializer.Deserialize<ITokenizer>(@"{""type"":""does_not_exist""}", Options());
			tokenizer.Should().BeNull();
		}

		[U] public void Deserialize_MissingType_ReturnsNull()
		{
			var tokenizer = JsonSerializer.Deserialize<ITokenizer>(@"{""foo"":""bar""}", Options());
			tokenizer.Should().BeNull();
		}

		[U] public void Deserialize_Null_ReturnsNull()
		{
			var tokenizer = JsonSerializer.Deserialize<ITokenizer>("null", Options());
			tokenizer.Should().BeNull();
		}

		[U] public void Serialize_ByRuntimeType()
		{
			ITokenizer tokenizer = new StandardTokenizer();

			var json = JsonSerializer.Serialize(tokenizer, Options());

			json.Should().Contain(@"""type"":""standard""");
		}

		[U] public void Serialize_Null_WritesNull()
		{
			JsonSerializer.Serialize<ITokenizer>(null, Options()).Should().Be("null");
		}

		[U] public void RoundTrip_NGram()
		{
			ITokenizer original = new NGramTokenizer { MinGram = 2, MaxGram = 4 };

			var json = JsonSerializer.Serialize(original, Options());
			var back = JsonSerializer.Deserialize<ITokenizer>(json, Options());

			back.Should().BeOfType<NGramTokenizer>();
			var ngram = (INGramTokenizer)back;
			ngram.MinGram.Should().Be(2);
			ngram.MaxGram.Should().Be(4);
		}
	}
}
