/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Text.Json;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;

namespace Tests.OpenSearch.Client.Serialization
{
	/// <summary>
	/// Behavioural tests for <see cref="IncludeExcludeConverter"/>. An <see cref="IncludeExclude"/> is serialized as an
	/// array of strings (exact values) or a regex string (pattern); read back from an array, string, or null.
	/// </summary>
	public class IncludeExcludeConverterTests
	{
		private static JsonSerializerOptions Options()
		{
			var options = new JsonSerializerOptions();
			options.Converters.Add(new IncludeExcludeConverter());
			return options;
		}

		// ---- read ----

		[U] public void Read_StringArray()
		{
			var value = JsonSerializer.Deserialize<IncludeExclude>(@"[""foo"",""bar""]", Options());
			value.Should().NotBeNull();
			value.Values.Should().Equal("foo", "bar");
			value.Pattern.Should().BeNull();
		}

		[U] public void Read_Pattern()
		{
			var value = JsonSerializer.Deserialize<IncludeExclude>(@"""foo.*""", Options());
			value.Should().NotBeNull();
			value.Pattern.Should().Be("foo.*");
			value.Values.Should().BeNull();
		}

		[U] public void Read_Null_IsNull() =>
			JsonSerializer.Deserialize<IncludeExclude>("null", Options()).Should().BeNull();

		[U] public void Read_EmptyArray()
		{
			var value = JsonSerializer.Deserialize<IncludeExclude>(@"[]", Options());
			value.Should().NotBeNull();
			value.Values.Should().BeEmpty();
		}

		[U] public void Read_UnexpectedToken_Throws()
		{
			Action act = () => JsonSerializer.Deserialize<IncludeExclude>(@"{""x"":1}", Options());
			act.Should().Throw<JsonException>();
		}

		// ---- write ----

		[U] public void Write_StringArray()
		{
			var json = JsonSerializer.Serialize(new IncludeExclude(new[] { "foo", "bar" }), Options());
			json.Should().Be(@"[""foo"",""bar""]");
		}

		[U] public void Write_Pattern()
		{
			var json = JsonSerializer.Serialize(new IncludeExclude("foo.*"), Options());
			json.Should().Be(@"""foo.*""");
		}

		[U] public void Write_Null() =>
			JsonSerializer.Serialize<IncludeExclude>(null, Options()).Should().Be("null");

		[U] public void Write_NullPattern_WritesNull()
		{
			// Neither Values nor Pattern set: legacy wrote WriteString(null) => JSON null.
			var json = JsonSerializer.Serialize(new IncludeExclude((string)null), Options());
			json.Should().Be("null");
		}

		// ---- round trip ----

		[U] public void RoundTrip_StringArray()
		{
			var options = Options();
			var json = JsonSerializer.Serialize(new IncludeExclude(new[] { "a", "b", "c" }), options);
			var back = JsonSerializer.Deserialize<IncludeExclude>(json, options);
			back.Values.Should().Equal("a", "b", "c");
		}

		[U] public void RoundTrip_Pattern()
		{
			var options = Options();
			var json = JsonSerializer.Serialize(new IncludeExclude("bar.*"), options);
			var back = JsonSerializer.Deserialize<IncludeExclude>(json, options);
			back.Pattern.Should().Be("bar.*");
		}
	}
}
