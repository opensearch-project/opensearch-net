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
	/// Validates <see cref="RoutingConverter"/>: the settings-aware converter serializes string/long
	/// <see cref="Routing"/> values directly and resolves document-based routing through the runtime
	/// Inferrer.
	/// </summary>
	public class RoutingConverterTests
	{
		private static JsonSerializerOptions Options()
		{
			var settings = new ConnectionSettings();
			var options = new JsonSerializerOptions();
			options.Converters.Add(new RoutingConverter(settings));
			return options;
		}

		[U] public void Serialize_StringValue()
		{
			Routing routing = "route-1";
			var json = JsonSerializer.Serialize(routing, Options());
			json.Should().Be(@"""route-1""");
		}

		[U] public void Serialize_LongValue()
		{
			Routing routing = 42L;
			var json = JsonSerializer.Serialize(routing, Options());
			json.Should().Be("42");
		}

		[U] public void Serialize_Null_WritesNull()
		{
			var json = JsonSerializer.Serialize<Routing>(null, Options());
			json.Should().Be("null");
		}

		[U] public void Deserialize_String()
		{
			var result = JsonSerializer.Deserialize<Routing>(@"""route-1""", Options());
			result.Should().Be((Routing)"route-1");
		}

		[U] public void Deserialize_Long()
		{
			var result = JsonSerializer.Deserialize<Routing>("42", Options());
			result.Should().Be((Routing)42L);
		}

		[U] public void RoundTrip_String()
		{
			Routing routing = "route-x";
			var json = JsonSerializer.Serialize(routing, Options());
			var back = JsonSerializer.Deserialize<Routing>(json, Options());
			back.Should().Be((Routing)"route-x");
		}
	}
}
