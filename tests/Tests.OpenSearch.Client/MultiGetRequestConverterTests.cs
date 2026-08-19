/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Collections.Generic;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;

namespace Tests.OpenSearch.Client.Serialization
{
	/// <summary>
	/// Validates the <see cref="MultiGetRequestConverter"/> reproduces the legacy Utf8Json
	/// <c>MultiGetRequestFormatter</c>. Unlike bulk/multi-search this request is a single JSON object — either
	/// <c>{ "ids": [ … ] }</c> (all operations flattenable to a bare id) or <c>{ "docs": [ … ] }</c>. Also verifies
	/// the empty-object short-circuit and the legacy <c>Deserialize</c> NotSupported behaviour.
	/// </summary>
	public class MultiGetRequestConverterTests
	{
		private class Doc
		{
			public string Name { get; set; }
		}

		private static JsonSerializerOptions Options()
		{
			var settings = new ConnectionSettings();
			var options = new JsonSerializerOptions
			{
				DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
				PropertyNameCaseInsensitive = true,
				Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
				TypeInfoResolver = new HighLevelContractResolver(settings)
			};
			options.Converters.Add(new IdConverter(settings));
			options.Converters.Add(new IndexNameConverter(settings));
			options.Converters.Add(new MultiGetRequestConverter(settings));
			return options;
		}

		[U] public void FlattensToIds_WhenEveryOperationIsBareId()
		{
			var request = new MultiGetRequest
			{
				// No index/routing/source/stored_fields set: each operation CanBeFlattened => "ids".
				Documents = new List<IMultiGetOperation>
				{
					new MultiGetOperation<Doc>(new Id(1L)) { Index = null },
					new MultiGetOperation<Doc>(new Id(2L)) { Index = null }
				}
			};

			var json = JsonSerializer.Serialize<IMultiGetRequest>(request, Options());
			json.Should().Be("{\"ids\":[1,2]}");
		}

		[U] public void WritesDocs_WhenOperationsCarryMetadata()
		{
			var request = new MultiGetRequest
			{
				Documents = new List<IMultiGetOperation>
				{
					new MultiGetOperation<Doc>("a") { Index = "idx", Routing = "r1" }
				}
			};

			var json = JsonSerializer.Serialize<IMultiGetRequest>(request, Options());
			json.Should().StartWith("{\"docs\":[").And.EndWith("]}");
			json.Should().Contain("\"_id\":\"a\"").And.Contain("\"_index\":\"idx\"").And.Contain("\"routing\":\"r1\"");
		}

		[U] public void EmptyDocuments_WritesEmptyObject()
		{
			var json = JsonSerializer.Serialize<IMultiGetRequest>(new MultiGetRequest(), Options());
			json.Should().Be("{}");
		}

		[U] public void Deserialize_Throws_NotSupported()
		{
			Action act = () => JsonSerializer.Deserialize<IMultiGetRequest>("{}", Options());
			act.Should().Throw<NotSupportedException>();
		}
	}
}
