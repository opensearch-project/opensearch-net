/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using FluentAssertions;
using OpenSearch.Client;
using static Tests.Core.Serialization.SerializationTestHelper;

namespace Tests.Serialization
{
	/// <summary>
	/// Fast, deterministic unit coverage for the System.Text.Json response-dictionary readers (#388):
	/// the key-resolving <c>ResolvableDictionaryProxy</c> responses (`field_caps`, `indices/stats`),
	/// the polymorphic get-field-mapping response, and the `Field`-keyed term-vectors map. These
	/// deserialization paths otherwise run only under integration ([I]); they regressed there and are
	/// pinned here so a break is caught without a live cluster.
	/// </summary>
	public class ResponseDictionaryReaderTests
	{
		private class Doc { public string Name { get; set; } }

		[U]
		public void FieldCapabilities_ResolvesFieldKeyedTypes()
		{
			const string json = @"{ ""fields"": { ""name"": {
				""text"": { ""type"": ""text"", ""searchable"": true, ""aggregatable"": false } } } }";
			var response = Expect(json).NoRoundTrip().DeserializesTo<FieldCapabilitiesResponse>();

			response.Fields.Should().NotBeNull();
			response.Fields["name"].Should().NotBeNull();
			response.Fields["name"].Text.Should().NotBeNull();
			response.Fields["name"].Text.Searchable.Should().BeTrue();
		}

		[U]
		public void IndicesStats_ResolvesIndexNameKeyedEntries()
		{
			const string json = @"{
				""_shards"": { ""total"": 1, ""successful"": 1, ""failed"": 0 },
				""_all"": { ""primaries"": {}, ""total"": {} },
				""indices"": { ""my-index"": { ""uuid"": ""abc"", ""primaries"": {}, ""total"": {} } } }";
			var response = Expect(json).NoRoundTrip().DeserializesTo<IndicesStatsResponse>();

			response.Indices.Should().NotBeNull();
			response.Indices.Count.Should().Be(1);
			// Lookup by the (inferred) IndexName key must resolve through the ResolvableDictionaryProxy.
			response.Indices["my-index"].Should().NotBeNull();
			response.Indices["my-index"].Uuid.Should().Be("abc");
		}

		[U]
		public void GetFieldMapping_ReadsPolymorphicFieldMapping()
		{
			const string json = @"{ ""my-index"": { ""mappings"": {
				""name"": { ""full_name"": ""name"", ""mapping"": { ""name"": { ""type"": ""text"" } } } } } }";
			var response = Expect(json).NoRoundTrip().DeserializesTo<GetFieldMappingResponse>();

			response.Indices.Should().NotBeNull();
			var mapping = response.GetMapping("my-index", "name");
			mapping.Should().BeOfType<TextProperty>();
		}

		[U]
		public void TermVectors_ResolvesFieldKeyedTermVectors()
		{
			const string json = @"{ ""_index"": ""my-index"", ""_id"": ""1"", ""_version"": 1, ""found"": true,
				""took"": 1, ""term_vectors"": { ""message"": { ""terms"": { ""foo"": { ""term_freq"": 2 } } } } }";
			var response = Expect(json).NoRoundTrip().DeserializesTo<TermVectorsResponse>();

			response.Found.Should().BeTrue();
			response.TermVectors.Should().NotBeNull();
			response.TermVectors.Count.Should().Be(1);
			// Lookup by the (inferred) Field key must resolve through the ResolvableDictionaryProxy.
			response.TermVectors["message"].Should().NotBeNull();
		}
	}
}
