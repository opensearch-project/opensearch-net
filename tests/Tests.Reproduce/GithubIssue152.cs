/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using FluentAssertions;
using OpenSearch.Net.Utf8Json;
using OpenSearch.Net.Utf8Json.Formatters;
using OpenSearch.Net.Utf8Json.Resolvers;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;

namespace Tests.Reproduce
{
	public class GithubIssue152
	{
		[U]
		public void ISO8601DateTimeFormatter_AlwaysEmitsFractionalSeconds()
		{
			var formatter = ISO8601DateTimeFormatter.Default;
			var resolver = StandardResolver.Default;

			// Zero milliseconds — previously serialized without fractional part,
			// causing OpenSearch `date_time` format parse failures.
			var zeroMs = new DateTime(2022, 11, 18, 11, 6, 55, 0, DateTimeKind.Utc);
			var writer = new JsonWriter();
			formatter.Serialize(ref writer, zeroMs, resolver);
			var result = writer.ToString();

			result.Should().Contain(".", because: "fractional seconds must always be present for OpenSearch date_time compatibility");
			result.Should().Be("\"2022-11-18T11:06:55.0000000Z\"");

			// Non-zero milliseconds — should still work as before.
			var nonZeroMs = new DateTime(2022, 11, 18, 11, 6, 55, 123, DateTimeKind.Utc);
			writer = new JsonWriter();
			formatter.Serialize(ref writer, nonZeroMs, resolver);
			writer.ToString().Should().Be("\"2022-11-18T11:06:55.1230000Z\"");
		}

		[U]
		public void ISO8601DateTimeFormatter_RoundTrips_ZeroMilliseconds()
		{
			var formatter = ISO8601DateTimeFormatter.Default;
			var resolver = StandardResolver.Default;

			var original = new DateTime(2022, 11, 18, 11, 6, 55, 0, DateTimeKind.Utc);
			var writer = new JsonWriter();
			formatter.Serialize(ref writer, original, resolver);
			var json = writer.ToString();

			var reader = new JsonReader(System.Text.Encoding.UTF8.GetBytes(json));
			var deserialized = formatter.Deserialize(ref reader, resolver);

			deserialized.Should().Be(original);
		}
	}
}
