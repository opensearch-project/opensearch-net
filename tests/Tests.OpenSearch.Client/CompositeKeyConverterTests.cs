/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Collections.Generic;
using System.Text.Json;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;

namespace Tests.OpenSearch.Client.Serialization
{
	/// <summary>
	/// Behavioural tests for <see cref="CompositeKeyConverter"/>, the System.Text.Json replacement for the legacy
	/// Utf8Json <c>CompositeKeyFormatter</c>. A <see cref="CompositeKey"/> is a string-keyed dictionary of mixed scalar
	/// values; the load-bearing behaviours are the long-vs-double number precision (integral numbers box as
	/// <see cref="long"/>, fractional ones as <see cref="double"/>) that the typed accessors depend on, and the
	/// preservation of null values.
	/// </summary>
	public class CompositeKeyConverterTests
	{
		private static JsonSerializerOptions Options()
		{
			var options = new JsonSerializerOptions();
			options.Converters.Add(new CompositeKeyConverter());
			return options;
		}

		private static CompositeKey Deserialize(string json) =>
			JsonSerializer.Deserialize<CompositeKey>(json, Options());

		private static string Serialize(CompositeKey value) =>
			JsonSerializer.Serialize(value, Options());

		[U] public void Read_IntegralNumber_IsLong()
		{
			var key = Deserialize(@"{""branch_count"":5}");
			key.Should().NotBeNull();
			key.TryGetValue("branch_count", out long l).Should().BeTrue();
			l.Should().Be(5L);
			// The boxed value is a long, not a double.
			((object)key["branch_count"]).Should().BeOfType<long>();
		}

		[U] public void Read_FloatingPointNumber_IsDouble()
		{
			var key = Deserialize(@"{""score"":1.5}");
			key.Should().NotBeNull();
			((object)key["score"]).Should().BeOfType<double>();
			key.TryGetValue("score", out double d).Should().BeTrue();
			d.Should().Be(1.5);
		}

		[U] public void Read_LargeLong_KeepsPrecisionAsLong()
		{
			// A value beyond exact double precision must stay a long (the legacy IsLong-first check).
			var key = Deserialize(@"{""ts"":9007199254740993}");
			((object)key["ts"]).Should().BeOfType<long>();
			key.TryGetValue("ts", out long l).Should().BeTrue();
			l.Should().Be(9007199254740993L);
		}

		[U] public void Read_String_Value()
		{
			var key = Deserialize(@"{""branches"":""main""}");
			key.TryGetValue("branches", out string s).Should().BeTrue();
			s.Should().Be("main");
		}

		[U] public void Read_Bool_Value()
		{
			var key = Deserialize(@"{""flag"":true}");
			((object)key["flag"]).Should().Be(true);
		}

		[U] public void Read_NullValue_IsPreserved()
		{
			var key = Deserialize(@"{""branches"":null}");
			key.Should().NotBeNull();
			key.ContainsKey("branches").Should().BeTrue();
			key["branches"].Should().BeNull();
		}

		[U] public void Read_MixedValueTypes()
		{
			var key = Deserialize(@"{""branches"":""main"",""branch_count"":3,""score"":2.25,""missing"":null}");
			key.Count.Should().Be(4);
			((object)key["branches"]).Should().BeOfType<string>();
			((object)key["branch_count"]).Should().BeOfType<long>();
			((object)key["score"]).Should().BeOfType<double>();
			key["missing"].Should().BeNull();
		}

		[U] public void Read_JsonNull_ReturnsNull() =>
			Deserialize("null").Should().BeNull();

		[U] public void Read_NonObject_ReturnsNull() =>
			Deserialize("42").Should().BeNull();

		[U] public void Write_Null_WritesJsonNull() =>
			Serialize(null).Should().Be("null");

		[U] public void Write_LongNotAsDouble()
		{
			var key = new CompositeKey(new Dictionary<string, object> { { "branch_count", 5L } });
			Serialize(key).Should().Be(@"{""branch_count"":5}");
		}

		[U] public void Write_Double()
		{
			var key = new CompositeKey(new Dictionary<string, object> { { "score", 1.5 } });
			Serialize(key).Should().Be(@"{""score"":1.5}");
		}

		[U] public void Write_NullValue_IsPreserved()
		{
			var key = new CompositeKey(new Dictionary<string, object> { { "branches", null } });
			Serialize(key).Should().Be(@"{""branches"":null}");
		}

		[U] public void RoundTrip_MixedValues_PreservesTypes()
		{
			var options = Options();
			var json = @"{""branches"":""main"",""branch_count"":3,""score"":2.25,""missing"":null}";
			var key = JsonSerializer.Deserialize<CompositeKey>(json, options);
			var again = JsonSerializer.Serialize(key, options);
			again.Should().Be(json);
		}
	}
}
