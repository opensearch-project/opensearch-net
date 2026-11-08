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
	/// Behavioural tests for <see cref="KeyedProcessorStatsConverter"/>, the System.Text.Json replacement for the
	/// legacy Utf8Json <c>KeyedProcessorStatsFormatter</c>. A <see cref="KeyedProcessorStats"/> is a single-entry
	/// object <c>{ "&lt;type&gt;": { ...ProcessStats... } }</c> where the property name is the processor type and the
	/// value deserializes to a <see cref="ProcessStats"/>.
	/// </summary>
	public class KeyedProcessorStatsConverterTests
	{
		private static JsonSerializerOptions Options()
		{
			var settings = new ConnectionSettings();
			var options = new JsonSerializerOptions
			{
				TypeInfoResolver = new HighLevelContractResolver(settings)
			};
			options.Converters.Add(new KeyedProcessorStatsConverter());
			return options;
		}

		private static KeyedProcessorStats Deserialize(string json) =>
			JsonSerializer.Deserialize<KeyedProcessorStats>(json, Options());

		[U] public void Read_TypeAndStatistics()
		{
			var stats = Deserialize(@"{""set"":{""open_file_descriptors"":123,""timestamp"":456}}");
			stats.Should().NotBeNull();
			stats.Type.Should().Be("set");
			stats.Statistics.Should().NotBeNull();
			stats.Statistics.OpenFileDescriptors.Should().Be(123);
			stats.Statistics.Timestamp.Should().Be(456);
		}

		[U] public void Read_NonObject_ReturnsNull() => Deserialize(@"""x""").Should().BeNull();

		[U] public void Write_WithType_WritesKeyedObject()
		{
			var stats = new KeyedProcessorStats { Type = "set", Statistics = new ProcessStats() };
			var json = JsonSerializer.Serialize(stats, Options());
			json.Should().StartWith(@"{""set"":");
		}

		[U] public void Write_NullType_WritesNull()
		{
			var stats = new KeyedProcessorStats();
			JsonSerializer.Serialize(stats, Options()).Should().Be("null");
		}

		[U] public void Write_NullValue_WritesNull() =>
			JsonSerializer.Serialize<KeyedProcessorStats>(null, Options()).Should().Be("null");

		[U] public void Roundtrip()
		{
			var json = @"{""grok"":{""open_file_descriptors"":7,""timestamp"":9}}";
			var back = Deserialize(json);
			back.Type.Should().Be("grok");
			back.Statistics.OpenFileDescriptors.Should().Be(7);
			back.Statistics.Timestamp.Should().Be(9);
		}
	}
}
