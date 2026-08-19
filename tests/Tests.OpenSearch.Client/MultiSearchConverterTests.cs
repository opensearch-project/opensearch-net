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
	/// Validates the <see cref="MultiSearchConverter"/> reproduces the legacy Utf8Json <c>MultiSearchFormatter</c>
	/// newline-delimited JSON (ndjson): a header line then a body line per operation, each followed by a raw
	/// <c>'\n'</c> (so the payload ends with a trailing newline). The header carries only the resolved
	/// index/search_type/preference/routing/ignore_unavailable values.
	/// </summary>
	public class MultiSearchConverterTests
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
			options.Converters.Add(new MultiSearchConverter(settings));
			return options;
		}

		private static IMultiSearchRequest BuildRequest()
		{
			var request = new MultiSearchRequest
			{
				Operations = new Dictionary<string, ISearchRequest>
				{
					{ "one", new SearchRequest<Doc>("index-a") { From = 0, Size = 5 } },
					{ "two", new SearchRequest<Doc>("index-b") { From = 10 } }
				}
			};
			return request;
		}

		[U] public void Emits_HeaderThenBody_PerOperation_WithTrailingNewlines()
		{
			var json = JsonSerializer.Serialize<IMultiSearchRequest>(BuildRequest(), Options());

			json.Should().EndWith("\n");

			var lines = json.Split('\n');
			// 2 operations x (header + body) = 4 lines, + trailing empty segment from the final '\n' = 5.
			lines.Should().HaveCount(5);
			lines[4].Should().BeEmpty();

			// Dictionary enumeration order is not guaranteed, so assert on content rather than line index.
			// Each operation contributes a header line carrying only the resolved index and a body line with the
			// serialized search request.
			json.Should().Contain("{\"index\":\"index-a\"}").And.Contain("{\"index\":\"index-b\"}");
			json.Should().Contain("\"size\":5").And.Contain("\"from\":10");
			// Header lines carry the compact header shape, not search body members like from/size.
			lines[0].Should().StartWith("{\"index\":\"index-");
		}

		[U] public void NullOperations_WritesNothing()
		{
			var json = JsonSerializer.Serialize<IMultiSearchRequest>(new MultiSearchRequest(), Options());
			json.Should().BeEmpty();
		}
	}
}
