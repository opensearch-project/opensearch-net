/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Globalization;
using System.IO;
using System.Text;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;

namespace Tests.OpenSearch.Client.Serialization
{
	/// <summary>
	/// Numbers the server sends as JSON strings (e.g. <c>"5.5"</c>) must parse with the invariant culture, not the
	/// current one. Under a culture whose decimal separator is a comma (de-DE), a culture-sensitive parse reads
	/// <c>"5.5"</c> as <c>55</c> without failing.
	/// </summary>
	public class CultureInvariantNumberParsingTests
	{
		private static readonly CultureInfo CommaDecimalCulture = new CultureInfo("de-DE");

		[U] public void StringifiedDouble_ParsesInvariantly_Utf8Json() => AssertLimit(useSystemTextJson: false);

		[U] public void StringifiedDouble_ParsesInvariantly_SystemTextJson() => AssertLimit(useSystemTextJson: true);

		[U] public void CartesianPointCoordinates_ParseInvariantly() =>
			UnderCommaDecimalCulture(() =>
			{
				var point = CartesianPoint.FromCoordinates("1.5,2.5");
				point.X.Should().Be(1.5f);
				point.Y.Should().Be(2.5f);
			});

		private static void AssertLimit(bool useSystemTextJson) =>
			UnderCommaDecimalCulture(() =>
			{
				var client = new OpenSearchClient(new ConnectionSettings().UseSystemTextJson(useSystemTextJson));
				var limiter = client.RequestResponseSerializer.Deserialize<RateLimiter>(
					new MemoryStream(Encoding.UTF8.GetBytes(@"{""limit"":""5.5""}")));
				limiter.Limit.Should().Be(5.5);
			});

		private static void UnderCommaDecimalCulture(Action assertion)
		{
			var original = CultureInfo.CurrentCulture;
			try
			{
				CultureInfo.CurrentCulture = CommaDecimalCulture;
				assertion();
			}
			finally
			{
				CultureInfo.CurrentCulture = original;
			}
		}
	}
}
