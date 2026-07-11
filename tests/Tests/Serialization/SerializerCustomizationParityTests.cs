/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.IO;
using System.Linq;
using System.Text;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.Net;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Core.Client;

namespace Tests.Serialization
{
	/// <summary>
	/// Pins parity for OpenSearch-specific serialization customizations that were previously exercised
	/// only transitively by the usage-test suite (see the #388 migration audit): string-encoded
	/// primitives the server may send as JSON strings, verbatim (non-camel-cased) dictionary keys, and
	/// the JSON.NET-compatible <see cref="Uri"/> wire form. These convert "covered-but-untested" bridge
	/// converters into directly-asserted behavior so a regression is caught without a live cluster.
	/// </summary>
	public class SerializerCustomizationParityTests
	{
		private static readonly IOpenSearchClient Client = TestClient.DefaultInMemoryClient;

		private static T Deserialize<T>(string json)
		{
			using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
			return Client.RequestResponseSerializer.Deserialize<T>(stream);
		}

		private static string Serialize(object value) => Client.RequestResponseSerializer.SerializeToString(value);

		/// <summary>
		/// The server may send extended-stats std-deviation bounds as JSON strings (e.g. "Infinity").
		/// The <c>NullableStringDoubleFormatter</c> bridge must parse the string members while leaving
		/// plain numeric members untouched.
		/// </summary>
		[U]
		public void ReadsStringEncodedDoubles()
		{
			var bounds = Deserialize<StandardDeviationBounds>(
				@"{""lower"":1.0,""upper"":2.0,""lower_sampling"":""3.5"",""upper_sampling"":""4.5""}");

			bounds.Lower.Should().Be(1.0);
			bounds.Upper.Should().Be(2.0);
			bounds.LowerSampling.Should().Be(3.5);
			bounds.UpperSampling.Should().Be(4.5);
		}

		/// <summary>
		/// The server may send numeric/boolean analyzer settings as JSON strings. The
		/// <c>NullableStringIntFormatter</c> / <c>NullableStringBooleanFormatter</c> bridges must parse
		/// them into the typed members.
		/// </summary>
		[U]
		public void ReadsStringEncodedIntAndBool()
		{
			var analyzer = Deserialize<FingerprintAnalyzer>(
				@"{""type"":""fingerprint"",""max_output_size"":""255"",""preserve_original"":""true"",""separator"":""-""}");

			analyzer.MaxOutputSize.Should().Be(255);
			analyzer.PreserveOriginal.Should().BeTrue();
			analyzer.Separator.Should().Be("-");
		}

		/// <summary>
		/// Dictionary families keyed by user-supplied names (analyzers, etc.) must serialize keys
		/// verbatim — the request/response camel-casing of un-attributed members must NOT be applied to
		/// the keys.
		/// </summary>
		[U]
		public void PreservesVerbatimDictionaryKeys()
		{
			var analyzers = new Analyzers();
			analyzers.Add("MyAnalyzer_UPPER", new StopAnalyzer());

			var json = Serialize(analyzers);

			json.Should().Contain("\"MyAnalyzer_UPPER\"");
			json.Should().NotContain("myAnalyzer_UPPER");
		}

		/// <summary>
		/// <see cref="Uri"/> members must use the JSON.NET-compatible wire form: the (non-normalized)
		/// <see cref="Uri.OriginalString"/> is written and round-trips, including relative URIs. This
		/// pins STJ's built-in Uri behavior against a future divergence.
		/// </summary>
		[U]
		public void UriUsesOriginalStringAndRoundTrips()
		{
			var doc = new UriDoc
			{
				Absolute = new Uri("http://host:9200/path?q=1"),
				NonNormalized = new Uri("http://host:9200/a/../b"),
				Relative = new Uri("relative/path", UriKind.Relative)
			};

			var json = Serialize(doc);
			// OriginalString is preserved (not normalized to ".../b").
			json.Should().Contain("http://host:9200/a/../b");

			var back = Deserialize<UriDoc>(json);
			back.Absolute.OriginalString.Should().Be("http://host:9200/path?q=1");
			back.NonNormalized.OriginalString.Should().Be("http://host:9200/a/../b");
			back.Relative.OriginalString.Should().Be("relative/path");
			back.Relative.IsAbsoluteUri.Should().BeFalse();
		}

		/// <summary>
		/// <c>SingleOrEnumerableFormatter</c> members accept either a single scalar or an array on the
		/// wire (the server may send either); both must deserialize to a collection. On write the value
		/// is always emitted as an array.
		/// </summary>
		[U]
		public void ReadsSingleOrEnumerableAsScalarOrArray()
		{
			var analyzer = Deserialize<CustomAnalyzer>(
				@"{""type"":""custom"",""char_filter"":""html_strip"",""filter"":[""lowercase"",""stop""],""tokenizer"":""standard""}");

			// scalar -> single-element collection
			analyzer.CharFilter.Should().Equal("html_strip");
			// array -> collection
			analyzer.Filter.Should().Equal("lowercase", "stop");

			// write is always an array, even for a single element
			var json = Serialize(new CustomAnalyzer { Tokenizer = "standard", CharFilter = new[] { "html_strip" } });
			json.Should().Contain("\"char_filter\":[\"html_strip\"]");
		}

		/// <summary>
		/// Epoch-millisecond date members (e.g. node-usage <c>since</c>/<c>timestamp</c>) are read from a
		/// numeric epoch-millis value into <see cref="DateTimeOffset"/> via the epoch date bridge. These
		/// members also have non-public setters, exercising the resolver's non-public-setter wiring.
		/// </summary>
		[U]
		public void ReadsEpochMillisecondDateTimeOffset()
		{
			// 1609459200000 ms == 2021-01-01T00:00:00Z
			var usage = Deserialize<NodeUsageInformation>(
				@"{""since"":1609459200000,""timestamp"":1609459200000,""rest_actions"":{},""aggregations"":{}}");

			usage.Since.ToUniversalTime().Should().Be(new DateTimeOffset(2021, 1, 1, 0, 0, 0, TimeSpan.Zero));
			usage.Timestamp.ToUniversalTime().Should().Be(new DateTimeOffset(2021, 1, 1, 0, 0, 0, TimeSpan.Zero));
		}

		/// <summary>
		/// A document (proxy) request such as <see cref="IndexRequest{TDocument}"/> serializes as its
		/// document body through the source serializer — the request envelope (index/id) is not emitted.
		/// </summary>
		[U]
		public void IndexRequestSerializesDocumentBody()
		{
			var request = new IndexRequest<ProxyDoc>(new ProxyDoc { Name = "a", Age = 3 }, "idx", "1");

			var json = Serialize(request);

			json.Should().Contain("\"name\":\"a\"").And.Contain("\"age\":3");
			// it is the document body, not the request envelope
			json.Should().NotContain("idx");
		}

		/// <summary>
		/// An interface carrying <c>[ReadAs(typeof(Concrete))]</c> deserializes into its concrete type
		/// via the ReadAs converter factory.
		/// </summary>
		[U]
		public void ReadAsDeserializesInterfaceToConcrete()
		{
			var field = Deserialize<IFieldNamesField>(@"{""enabled"":false}");

			field.Should().BeOfType<FieldNamesField>();
			field.Enabled.Should().Be(false);
		}

		private class ProxyDoc
		{
			public string Name { get; set; }
			public int Age { get; set; }
		}

		private class UriDoc
		{
			public Uri Absolute { get; set; }
			public Uri NonNormalized { get; set; }
			public Uri Relative { get; set; }
		}
	}
}
