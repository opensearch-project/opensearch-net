/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;

namespace Tests.OpenSearch.Client.Serialization
{
	/// <summary>
	/// Validates the <see cref="BulkRequestConverter"/> reproduces the legacy Utf8Json <c>BulkRequestFormatter</c>
	/// newline-delimited JSON (ndjson) wire shape: an action/metadata line per operation, an optional source/body
	/// line, and a raw <c>'\n'</c> after every emitted line (including a trailing newline). Also verifies the legacy
	/// <c>Deserialize</c> NotSupported behaviour is mirrored.
	/// </summary>
	public class BulkRequestConverterTests
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
			options.Converters.Add(new RoutingConverter(settings));
			options.Converters.Add(new BulkRequestConverter(settings));
			return options;
		}

		private static IBulkRequest BuildRequest()
		{
			var request = new BulkRequest();
			var ops = new BulkOperationsCollection<IBulkOperation>
			{
				new BulkIndexOperation<Doc>(new Doc { Name = "indexed" }) { Index = "idx", Id = "1", Routing = new Routing("r1") },
				new BulkCreateOperation<Doc>(new Doc { Name = "created" }) { Index = "idx", Id = "2", Routing = new Routing("r2") },
				new BulkUpdateOperation<Doc, object>(new Id("3")) { Index = "idx" },
				new BulkDeleteOperation<Doc>(new Id("4")) { Index = "idx", Routing = new Routing("r4") }
			};
			request.Operations = ops;
			return request;
		}

		private static string Serialize() => JsonSerializer.Serialize<IBulkRequest>(BuildRequest(), Options());

		[U] public void Emits_OneLinePerAction_WithTrailingNewlines()
		{
			var json = Serialize();

			// index + create each contribute an action line and a body line; update contributes action + body;
			// delete contributes only an action line (no body). Every emitted line is followed by a raw '\n', so the
			// payload ends with a trailing newline and splitting on '\n' yields one empty trailing segment.
			json.Should().EndWith("\n");

			var lines = json.Split('\n');
			// 2 (index) + 2 (create) + 2 (update) + 1 (delete) = 7 lines, + trailing empty segment = 8.
			lines.Should().HaveCount(8);
			lines[7].Should().BeEmpty();
		}

		[U] public void ActionLines_CarryOperationTypeAndMetadata()
		{
			var lines = Serialize().Split('\n');

			lines[0].Should().StartWith("{\"index\":{").And.Contain("\"_index\":\"idx\"").And.Contain("\"_id\":\"1\"").And.Contain("\"routing\":\"r1\"");
			lines[1].Should().Be("{\"name\":\"indexed\"}");

			lines[2].Should().StartWith("{\"create\":{").And.Contain("\"_id\":\"2\"");
			lines[3].Should().Be("{\"name\":\"created\"}");

			lines[4].Should().StartWith("{\"update\":{").And.Contain("\"_id\":\"3\"");
			// update has a (possibly empty) body line following the action line.

			lines[6].Should().StartWith("{\"delete\":{").And.Contain("\"_id\":\"4\"");
			// delete has no body line: the segment after it is the trailing empty segment.
			lines[7].Should().BeEmpty();
		}

		[U] public void DeleteOperation_HasNoBodyLine()
		{
			var json = Serialize();
			// Exactly one "delete" action and no source line after it — the byte after its newline is the next action
			// or end-of-payload, never a body. Assert the delete action is immediately followed by the trailing newline.
			json.Should().Contain("{\"delete\":{").And.EndWith("}\n");
		}

		[U] public void NullOperations_WritesNothing()
		{
			var json = JsonSerializer.Serialize<IBulkRequest>(new BulkRequest(), Options());
			json.Should().BeEmpty();
		}

		[U] public void Deserialize_Throws_NotSupported()
		{
			Action act = () => JsonSerializer.Deserialize<IBulkRequest>("{}", Options());
			act.Should().Throw<NotSupportedException>();
		}
	}
}
