/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

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
	/// Validates the <see cref="MultiSearchTemplateConverter"/> reproduces the legacy Utf8Json
	/// <c>MultiSearchTemplateFormatter</c> newline-delimited JSON (ndjson): a header line then a search-template body
	/// line per operation, each followed by a raw <c>'\n'</c> (so the payload ends with a trailing newline). The
	/// header carries only the resolved index/search_type/preference/routing/ignore_unavailable values. This is the
	/// template variant of <see cref="MultiSearchConverterTests"/>.
	/// </summary>
	public class MultiSearchTemplateConverterTests
	{
		private class Doc { }

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
			options.Converters.Add(new IndicesMultiSyntaxConverter(settings));
			options.Converters.Add(new RoutingConverter(settings));
			options.Converters.Add(new MultiSearchTemplateConverter(settings));
			return options;
		}

		private static IMultiSearchTemplateRequest BuildRequest() =>
			new MultiSearchTemplateRequest
			{
				Operations = new Dictionary<string, ISearchTemplateRequest>
				{
					{ "inline", new SearchTemplateRequest<Doc>("index-a") { Source = "{\"query\":{\"match_all\":{}}}" } },
					{ "byid", new SearchTemplateRequest<Doc>("index-b") { Id = "template-id" } }
				}
			};

		[U] public void Emits_HeaderThenBody_PerOperation_WithTrailingNewlines()
		{
			var json = JsonSerializer.Serialize(BuildRequest(), Options());

			json.Should().EndWith("\n");

			var lines = json.Split('\n');
			// 2 operations x (header + body) = 4 lines, + trailing empty segment from the final '\n' = 5.
			lines.Should().HaveCount(5);
			lines[4].Should().BeEmpty();

			// Dictionary enumeration order is not guaranteed, so assert on content rather than line index. Each
			// operation contributes a header line carrying only the resolved index and a body line with the template.
			json.Should().Contain("{\"index\":\"index-a\"}").And.Contain("{\"index\":\"index-b\"}");
			json.Should().Contain("\"source\":\"{\\\"query\\\":{\\\"match_all\\\":{}}}\"");
			json.Should().Contain("\"id\":\"template-id\"");
			// Header lines carry the compact header shape, not body members like source/id.
			lines[0].Should().StartWith("{\"index\":\"index-");
		}

		[U] public void OperationMatchingRequestIndex_OmitsIndexFromHeader()
		{
			// When the operation's index equals the request-level index the legacy formatter emitted a null index on
			// the header (dropped under WhenWritingNull), so the header is an empty object.
			var request = new MultiSearchTemplateRequest("index-a")
			{
				Operations = new Dictionary<string, ISearchTemplateRequest>
				{
					{ "same", new SearchTemplateRequest<Doc>("index-a") { Id = "t" } }
				}
			};
			var json = JsonSerializer.Serialize<IMultiSearchTemplateRequest>(request, Options());

			var lines = json.Split('\n');
			lines[0].Should().Be("{}");
		}

		[U] public void NullOperations_WritesNothing()
		{
			var json = JsonSerializer.Serialize<IMultiSearchTemplateRequest>(new MultiSearchTemplateRequest(), Options());
			json.Should().BeEmpty();
		}
	}
}
