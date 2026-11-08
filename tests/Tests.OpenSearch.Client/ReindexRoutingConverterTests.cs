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
	/// Behavioural tests for <see cref="ReindexRoutingConverter"/>. Reads <c>"keep"</c>/<c>"discard"</c> into the
	/// shared singletons and any other string into a new prefixed (<c>=</c>) <see cref="ReindexRouting"/>. Writes
	/// null as JSON null, otherwise the routing string.
	/// </summary>
	public class ReindexRoutingConverterTests
	{
		private static JsonSerializerOptions Options()
		{
			var options = new JsonSerializerOptions();
			options.Converters.Add(new ReindexRoutingConverter());
			return options;
		}

		[U] public void Read_Keep()
		{
			var routing = JsonSerializer.Deserialize<ReindexRouting>(@"""keep""", Options());
			routing.Should().BeSameAs(ReindexRouting.Keep);
			routing.ToString().Should().Be("keep");
		}

		[U] public void Read_Discard()
		{
			var routing = JsonSerializer.Deserialize<ReindexRouting>(@"""discard""", Options());
			routing.Should().BeSameAs(ReindexRouting.Discard);
			routing.ToString().Should().Be("discard");
		}

		[U] public void Read_CustomValue_IsPrefixed()
		{
			var routing = JsonSerializer.Deserialize<ReindexRouting>(@"""shard-1""", Options());
			routing.Should().NotBeNull();
			routing.ToString().Should().Be("=shard-1");
		}

		[U] public void Read_AlreadyPrefixedValue_NotDoublePrefixed()
		{
			var routing = JsonSerializer.Deserialize<ReindexRouting>(@"""=shard-1""", Options());
			routing.ToString().Should().Be("=shard-1");
		}

		[U] public void Write_Keep()
		{
			var json = JsonSerializer.Serialize(ReindexRouting.Keep, Options());
			json.Should().Be(@"""keep""");
		}

		[U] public void Write_Discard()
		{
			var json = JsonSerializer.Serialize(ReindexRouting.Discard, Options());
			json.Should().Be(@"""discard""");
		}

		[U] public void Write_CustomValue()
		{
			var json = JsonSerializer.Serialize(new ReindexRouting("shard-1"), Options());
			json.Should().Be(@"""=shard-1""");
		}

		[U] public void Write_Null()
		{
			var json = JsonSerializer.Serialize<ReindexRouting>(null, Options());
			json.Should().Be("null");
		}

		[U] public void RoundTrip_Keep()
		{
			var options = Options();
			var json = JsonSerializer.Serialize(ReindexRouting.Keep, options);
			var routing = JsonSerializer.Deserialize<ReindexRouting>(json, options);
			routing.Should().BeSameAs(ReindexRouting.Keep);
		}

		[U] public void RoundTrip_CustomValue()
		{
			var options = Options();
			var json = JsonSerializer.Serialize(new ReindexRouting("shard-1"), options);
			var routing = JsonSerializer.Deserialize<ReindexRouting>(json, options);
			// Serialized as "=shard-1"; read back trims the leading '=' then re-prefixes → "=shard-1".
			routing.ToString().Should().Be("=shard-1");
		}
	}
}
