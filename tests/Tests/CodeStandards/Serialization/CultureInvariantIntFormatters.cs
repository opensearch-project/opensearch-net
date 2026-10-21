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
using System.Threading;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using OpenSearch.Net;
using FluentAssertions;
using OpenSearch.Client;

namespace Tests.CodeStandards.Serialization
{
	/// <summary>
	/// Regression tests for https://github.com/opensearch-project/opensearch-net/issues/1076.
	/// The Utf8Json <c>IntStringFormatter</c> and <c>NullableStringIntFormatter</c> parsed integer
	/// wire values with the culture-sensitive <c>int.TryParse(string, out int)</c> overload. Under a
	/// culture whose negative sign differs from the ASCII '-' (such as fa-IR), parsing a negative
	/// numeric wire value failed. Both must parse with <see cref="CultureInfo.InvariantCulture"/>.
	/// </summary>
	public class CultureInvariantIntFormatters
	{
		// fa-IR uses U+2212 as its negative sign, so int.TryParse under the current culture rejects
		// the ASCII "-7" that OpenSearch sends on the wire.
		private const string NonInvariantCulture = "fa-IR";

		private static readonly IOpenSearchSerializer Serializer;

		static CultureInvariantIntFormatters()
		{
			var pool = new SingleNodeConnectionPool(new Uri("http://localhost:9200"));
			var settings = new ConnectionSettings(pool, new InMemoryConnection());
			Serializer = new OpenSearchClient(settings).RequestResponseSerializer;
		}

		private static T WithCulture<T>(string culture, Func<T> action)
		{
			var original = Thread.CurrentThread.CurrentCulture;
			try
			{
				Thread.CurrentThread.CurrentCulture = new CultureInfo(culture, false);
				return action();
			}
			finally
			{
				Thread.CurrentThread.CurrentCulture = original;
			}
		}

		// Exercises NullableStringIntFormatter.Deserialize via ErrorCause.Shard.
		[U]
		public void DeserializeNegativeNumericString_UnderNonInvariantCulture()
		{
			const string json = @"{""type"":""x"",""reason"":""y"",""shard"":""-7""}";

			var errorCause = WithCulture(NonInvariantCulture,
				() => Serializer.Deserialize<ErrorCause>(new MemoryStream(Encoding.UTF8.GetBytes(json))));

			errorCause.Shard.Should().Be(-7);
		}

		// Exercises IntStringFormatter.Serialize via SnapshotShardFailure.ShardId.
		[U]
		public void SerializeNegativeIntString_UnderNonInvariantCulture()
		{
			var failure = new SnapshotShardFailure { ShardId = "-7" };

			var serialized = WithCulture(NonInvariantCulture, () => Serializer.SerializeToString(failure));

			serialized.Should().Contain("\"shard_id\":-7");
		}
	}
}
