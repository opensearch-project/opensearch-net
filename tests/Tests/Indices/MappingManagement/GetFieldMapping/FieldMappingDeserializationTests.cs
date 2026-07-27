/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.IO;
using System.Text;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.Net;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Core.Client;

namespace Tests.Indices.MappingManagement.GetFieldMapping;

public class FieldMappingDeserializationTests
{
	// Mirrors the shape the integration test was crashing on: a get-field-mapping response whose "mapping"
	// dictionary holds a polymorphic IFieldMapping (an ip property). Deserialization previously threw
	// "Deserialization of interface or abstract types is not supported. Type 'OpenSearch.Client.IFieldMapping'".
	private const string Json = @"{
		""project"": {
			""mappings"": {
				""leadDeveloper.ipAddress"": {
					""full_name"": ""leadDeveloper.ipAddress"",
					""mapping"": {
						""ipAddress"": { ""type"": ""ip"" }
					}
				},
				""name"": {
					""full_name"": ""name"",
					""mapping"": {
						""name"": { ""type"": ""keyword"", ""store"": true }
					}
				}
			}
		}
	}";

	[U] public void DeserializesPolymorphicFieldMapping()
	{
		var response = TestClient.DisabledStreaming.RequestResponseSerializer.Deserialize<GetFieldMappingResponse>(
			new MemoryStream(Encoding.UTF8.GetBytes(Json)));

		response.Should().NotBeNull();
		var projectMappings = response.Indices[(IndexName)"project"];
		projectMappings.Should().NotBeNull();

		var nameMapping = projectMappings.Mappings[(Field)"name"];
		nameMapping.FullName.Should().Be("name");
		var name = nameMapping.Mapping[(Field)"name"] as IKeywordProperty;
		name.Should().NotBeNull("the name field is a keyword mapping");
		name.Store.Should().BeTrue();

		var ipMapping = projectMappings.Mappings[(Field)"leadDeveloper.ipAddress"];
		var ip = ipMapping.Mapping[(Field)"ipAddress"] as IIpProperty;
		ip.Should().NotBeNull("the ipAddress field is an ip mapping");
	}

	// ClusterHealthResponse.Indices is [JsonFormatter(ResolvableReadOnlyDictionaryFormatter<IndexName,IndexHealthStats>)];
	// the integration test asserts response.Indices contains the "devs" index. Previously the member-level formatter had
	// no STJ bridge so the dictionary came back empty.
	private const string ClusterHealthJson = @"{
		""cluster_name"": ""opensearch"",
		""status"": ""green"",
		""number_of_nodes"": 1,
		""indices"": {
			""devs"": { ""status"": ""green"", ""number_of_shards"": 1, ""number_of_replicas"": 0, ""active_primary_shards"": 1, ""active_shards"": 1 }
		}
	}";

	[U] public void DeserializesClusterHealthIndices()
	{
		var response = TestClient.DisabledStreaming.RequestResponseSerializer.Deserialize<ClusterHealthResponse>(
			new MemoryStream(Encoding.UTF8.GetBytes(ClusterHealthJson)));

		response.Indices.Should().NotBeEmpty().And.ContainKey((IndexName)"devs");
		response.Indices[(IndexName)"devs"].ActivePrimaryShards.Should().Be(1);
	}

	// TermVectorsResponse.TermVectors is [JsonFormatter(ResolvableReadOnlyDictionaryFormatter<Field,TermVector>)]; the
	// integration test looks up the mapping by a Field expression (p => p.FirstName). Previously the key never resolved
	// through the inferrer so the lookup threw KeyNotFoundException.
	private const string TermVectorsJson = @"{
		""_index"": ""project"",
		""_id"": ""1"",
		""_version"": 1,
		""found"": true,
		""took"": 1,
		""term_vectors"": {
			""firstName"": {
				""field_statistics"": { ""sum_doc_freq"": 1, ""doc_count"": 1, ""sum_ttf"": 1 },
				""terms"": { ""alice"": { ""term_freq"": 1 } }
			}
		}
	}";

	[U] public void DeserializesTermVectorsKeyedByField()
	{
		var response = TestClient.DisabledStreaming.RequestResponseSerializer.Deserialize<TermVectorsResponse>(
			new MemoryStream(Encoding.UTF8.GetBytes(TermVectorsJson)));

		response.Found.Should().BeTrue();
		response.TermVectors.Should().NotBeEmpty().And.ContainKey((Field)"firstName");
		response.TermVectors[(Field)"firstName"].Terms.Should().ContainKey("alice");
	}
}
