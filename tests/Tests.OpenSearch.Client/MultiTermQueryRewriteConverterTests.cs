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
	/// Behavioural tests for <see cref="MultiTermQueryRewriteConverter"/>. A MultiTermQueryRewrite is serialized as a
	/// single JSON string; on read it is reconstructed via <c>MultiTermQueryRewrite.Create</c>. Covers the null,
	/// non-string, plain-value and sized-value branches.
	/// </summary>
	public class MultiTermQueryRewriteConverterTests
	{
		private static JsonSerializerOptions Options()
		{
			var options = new JsonSerializerOptions();
			options.Converters.Add(new MultiTermQueryRewriteConverter());
			return options;
		}

		[U] public void Read_Null()
		{
			var value = JsonSerializer.Deserialize<MultiTermQueryRewrite>("null", Options());
			value.Should().BeNull();
		}

		[U] public void Read_ConstantScore()
		{
			var value = JsonSerializer.Deserialize<MultiTermQueryRewrite>(@"""constant_score""", Options());
			value.Should().Be(MultiTermQueryRewrite.ConstantScore);
			value.Rewrite.Should().Be(RewriteMultiTerm.ConstantScore);
			value.Size.Should().BeNull();
		}

		[U] public void Read_TopTermsWithSize()
		{
			var value = JsonSerializer.Deserialize<MultiTermQueryRewrite>(@"""top_terms_5""", Options());
			value.Should().Be(MultiTermQueryRewrite.TopTerms(5));
			value.Rewrite.Should().Be(RewriteMultiTerm.TopTermsN);
			value.Size.Should().Be(5);
		}

		[U] public void Read_NonStringThrows()
		{
			System.Action act = () => JsonSerializer.Deserialize<MultiTermQueryRewrite>("123", Options());
			act.Should().Throw<JsonException>();
		}

		[U] public void Write_Null()
		{
			JsonSerializer.Serialize<MultiTermQueryRewrite>(null, Options()).Should().Be("null");
		}

		[U] public void Write_ConstantScore()
		{
			JsonSerializer.Serialize(MultiTermQueryRewrite.ConstantScore, Options()).Should().Be(@"""constant_score""");
		}

		[U] public void Write_TopTermsWithSize()
		{
			JsonSerializer.Serialize(MultiTermQueryRewrite.TopTerms(5), Options()).Should().Be(@"""top_terms_5""");
		}

		[U] public void RoundTrip_TopTermsBoost()
		{
			var json = JsonSerializer.Serialize(MultiTermQueryRewrite.TopTermsBoost(10), Options());
			var back = JsonSerializer.Deserialize<MultiTermQueryRewrite>(json, Options());
			back.Should().Be(MultiTermQueryRewrite.TopTermsBoost(10));
		}
	}
}
