/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.Text.Json;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;

namespace Tests.OpenSearch.Client.Serialization
{
	/// <summary>
	/// Behavioural tests for <see cref="UnionConverter{TFirst,TSecond}"/> and <see cref="TrackTotalHitsConverter"/>.
	/// TrackTotalHits is a union of bool and long; the converter must round-trip both variants and, on read,
	/// fall back from one type to the other (the "try-read" behaviour, reimplemented over a buffered DOM).
	/// </summary>
	public class TrackTotalHitsConverterTests
	{
		private static JsonSerializerOptions Options()
		{
			var options = new JsonSerializerOptions();
			options.Converters.Add(new TrackTotalHitsConverter());
			return options;
		}

		[U] public void Read_Bool()
		{
			var tth = JsonSerializer.Deserialize<TrackTotalHits>("true", Options());
			tth.Should().NotBeNull();
			tth.Tag.Should().Be(0);
			tth.Item1.Should().BeTrue();
		}

		[U] public void Read_Long()
		{
			var tth = JsonSerializer.Deserialize<TrackTotalHits>("10000", Options());
			tth.Should().NotBeNull();
			tth.Tag.Should().Be(1);
			tth.Item2.Should().Be(10000);
		}

		[U] public void Write_Bool()
		{
			JsonSerializer.Serialize<TrackTotalHits>(true, Options()).Should().Be("true");
		}

		[U] public void Write_Long()
		{
			JsonSerializer.Serialize<TrackTotalHits>(5000L, Options()).Should().Be("5000");
		}

		[U] public void RoundTrip_Bool()
		{
			var json = JsonSerializer.Serialize<TrackTotalHits>(false, Options());
			var tth = JsonSerializer.Deserialize<TrackTotalHits>(json, Options());
			tth.Tag.Should().Be(0);
			tth.Item1.Should().BeFalse();
		}

		[U] public void RoundTrip_Long()
		{
			var json = JsonSerializer.Serialize<TrackTotalHits>(12345L, Options());
			var tth = JsonSerializer.Deserialize<TrackTotalHits>(json, Options());
			tth.Tag.Should().Be(1);
			tth.Item2.Should().Be(12345);
		}
	}
}
