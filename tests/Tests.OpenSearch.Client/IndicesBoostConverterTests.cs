/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.Collections.Generic;
using System.Text.Json;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;

namespace Tests.OpenSearch.Client.Serialization
{
	/// <summary>
	/// Validates the settings-aware converter mechanism via <see cref="IndicesBoostConverter"/>: the converter is
	/// constructed with <see cref="IConnectionSettingsValues"/> and uses the runtime Inferrer to resolve
	/// <c>IndexName</c> keys — the capability plain converters previously could not access.
	/// </summary>
	public class IndicesBoostConverterTests
	{
		private static JsonSerializerOptions Options()
		{
			var settings = new ConnectionSettings();
			var options = new JsonSerializerOptions();
			options.Converters.Add(new IndicesBoostConverter(settings));
			return options;
		}

		[U] public void Serialize_WritesArrayOfSingleKeyObjects()
		{
			IDictionary<IndexName, double> boosts = new Dictionary<IndexName, double>
			{
				{ "index-a", 2.0 },
				{ "index-b", 0.5 }
			};

			var json = JsonSerializer.Serialize(boosts, Options());

			json.Should().Be(@"[{""index-a"":2.0},{""index-b"":0.5}]");
		}

		[U] public void Serialize_Null_WritesNull()
		{
			var json = JsonSerializer.Serialize<IDictionary<IndexName, double>>(null, Options());
			json.Should().Be("null");
		}

		[U] public void Deserialize_ArrayForm()
		{
			var result = JsonSerializer.Deserialize<IDictionary<IndexName, double>>(
				@"[{""index-a"":2},{""index-b"":0.5}]", Options());

			result.Should().HaveCount(2);
			result[(IndexName)"index-a"].Should().Be(2.0);
			result[(IndexName)"index-b"].Should().Be(0.5);
		}

		[U] public void RoundTrip()
		{
			IDictionary<IndexName, double> boosts = new Dictionary<IndexName, double> { { "idx", 1.5 } };
			var json = JsonSerializer.Serialize(boosts, Options());
			var back = JsonSerializer.Deserialize<IDictionary<IndexName, double>>(json, Options());
			back[(IndexName)"idx"].Should().Be(1.5);
		}
	}
}
