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
	/// Behavioural tests for <see cref="SimilarityConverter"/>: dispatches an <see cref="ISimilarity"/> to the
	/// concrete type named by the <c>type</c> discriminator field, falls back to <see cref="CustomSimilarity"/>
	/// for unrecognized/missing types (matching the legacy formatter), and serializes by runtime type.
	/// </summary>
	public class SimilarityConverterTests
	{
		private static JsonSerializerOptions Options()
		{
			var settings = new ConnectionSettings();
			var options = new JsonSerializerOptions
			{
				TypeInfoResolver = new HighLevelContractResolver(settings)
			};
			options.Converters.Add(new SimilarityConverter());
			return options;
		}

		private static ISimilarity Deserialize(string type) =>
			JsonSerializer.Deserialize<ISimilarity>($@"{{""type"":""{type}""}}", Options());

		[U] public void Deserialize_BM25() => Deserialize("BM25").Should().BeOfType<BM25Similarity>();
		[U] public void Deserialize_LMDirichlet() => Deserialize("LMDirichlet").Should().BeOfType<LMDirichletSimilarity>();
		[U] public void Deserialize_DFR() => Deserialize("DFR").Should().BeOfType<DFRSimilarity>();
		[U] public void Deserialize_DFI() => Deserialize("DFI").Should().BeOfType<DFISimilarity>();
		[U] public void Deserialize_IB() => Deserialize("IB").Should().BeOfType<IBSimilarity>();
		[U] public void Deserialize_LMJelinekMercer() => Deserialize("LMJelinekMercer").Should().BeOfType<LMJelinekMercerSimilarity>();
		[U] public void Deserialize_Scripted() => Deserialize("scripted").Should().BeOfType<ScriptedSimilarity>();

		[U] public void Deserialize_UnknownType_ReturnsCustomSimilarity()
		{
			// The legacy formatter treated any unrecognized type as a user-defined CustomSimilarity.
			Deserialize("my_custom_similarity").Should().BeOfType<CustomSimilarity>();
		}

		[U] public void Deserialize_MissingType_ReturnsCustomSimilarity()
		{
			var similarity = JsonSerializer.Deserialize<ISimilarity>(@"{""foo"":""bar""}", Options());
			similarity.Should().BeOfType<CustomSimilarity>();
		}

		[U] public void Deserialize_Null_ReturnsNull()
		{
			var similarity = JsonSerializer.Deserialize<ISimilarity>("null", Options());
			similarity.Should().BeNull();
		}

		[U] public void Serialize_ByRuntimeType()
		{
			ISimilarity similarity = new BM25Similarity();

			var json = JsonSerializer.Serialize(similarity, Options());

			json.Should().Contain(@"""type"":""BM25""");
		}

		[U] public void Serialize_Null_WritesNull()
		{
			JsonSerializer.Serialize<ISimilarity>(null, Options()).Should().Be("null");
		}
	}
}
