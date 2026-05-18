/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Text;
using FluentAssertions;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Core.Client;

namespace Tests.Reproduce
{
	public class GithubIssue152
	{
		[U]
		public void DateRangeQuery_SerializesZeroMilliseconds_WithFractionalSeconds()
		{
			// DateTime with zero milliseconds previously serialized without fractional seconds
			// (e.g. "2022-11-18T11:06:55Z"), which OpenSearch's `date_time` format rejects.
			// It must always include fractional seconds (e.g. "2022-11-18T11:06:55.0000000Z").
			var zeroMs = new DateTime(2022, 11, 18, 11, 6, 55, 0, DateTimeKind.Utc);

			var response = TestClient.DefaultInMemoryClient.Search<object>(s => s
				.AllIndices()
				.Query(q => q
					.DateRange(d => d
						.Field("timestamp")
						.GreaterThanOrEquals(zeroMs)
					)
				)
			);

			var body = Encoding.UTF8.GetString(response.ApiCall.RequestBodyInBytes);
			body.Should().Contain(".", because: "fractional seconds must always be present for OpenSearch date_time compatibility");
			body.Should().Contain("2022-11-18T11:06:55.0000000Z");
		}

		[U]
		public void DateRangeQuery_SerializesNonZeroMilliseconds_WithFractionalSeconds()
		{
			var nonZeroMs = new DateTime(2022, 11, 18, 11, 6, 55, 123, DateTimeKind.Utc);

			var response = TestClient.DefaultInMemoryClient.Search<object>(s => s
				.AllIndices()
				.Query(q => q
					.DateRange(d => d
						.Field("timestamp")
						.GreaterThanOrEquals(nonZeroMs)
					)
				)
			);

			var body = Encoding.UTF8.GetString(response.ApiCall.RequestBodyInBytes);
			body.Should().Contain("2022-11-18T11:06:55.1230000Z");
		}
	}
}
