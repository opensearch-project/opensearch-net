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
}
