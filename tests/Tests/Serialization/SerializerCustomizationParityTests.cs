/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.IO;
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

		private class UriDoc
		{
			public Uri Absolute { get; set; }
			public Uri NonNormalized { get; set; }
			public Uri Relative { get; set; }
		}
	}
}
