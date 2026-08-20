/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.Text.Json;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.Net.Serialization.Converters;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;

namespace Tests.OpenSearch.Client.Serialization
{
	/// <summary>
	/// Behavioural tests for <see cref="TotalHitsConverter"/>. A <see cref="TotalHits"/> serializes as a bare
	/// integral number when <see cref="TotalHits.Relation"/> is null, or as a
	/// <c>{ "value": &lt;long&gt;, "relation": "eq"|"gte" }</c> object otherwise. Reads accept both shapes; any other
	/// token yields null. The <c>value</c> is a <see cref="long"/> and must preserve precision beyond 2^53.
	/// </summary>
	public class TotalHitsConverterTests
	{
		private static JsonSerializerOptions Options()
		{
			var options = new JsonSerializerOptions();
			options.Converters.Add(new TotalHitsConverter());
			// TotalHitsRelation is a [StringEnum] serialized as eq/gte.
			options.Converters.Add(new StringEnumConverterFactory());
			return options;
		}

		[U] public void Read_Object_WithRelation()
		{
			var hits = JsonSerializer.Deserialize<TotalHits>(@"{""value"":42,""relation"":""eq""}", Options());
			hits.Should().NotBeNull();
			hits.Value.Should().Be(42);
			hits.Relation.Should().Be(TotalHitsRelation.EqualTo);
		}

		[U] public void Read_Object_GreaterThanOrEqualTo()
		{
			var hits = JsonSerializer.Deserialize<TotalHits>(@"{""value"":1000,""relation"":""gte""}", Options());
			hits.Should().NotBeNull();
			hits.Value.Should().Be(1000);
			hits.Relation.Should().Be(TotalHitsRelation.GreaterThanOrEqualTo);
		}

		[U] public void Read_Object_PropertyOrderIndependent()
		{
			var hits = JsonSerializer.Deserialize<TotalHits>(@"{""relation"":""gte"",""value"":7}", Options());
			hits.Value.Should().Be(7);
			hits.Relation.Should().Be(TotalHitsRelation.GreaterThanOrEqualTo);
		}

		[U] public void Read_Object_UnknownPropertyIsSkipped()
		{
			var hits = JsonSerializer.Deserialize<TotalHits>(@"{""value"":5,""extra"":{""nested"":true},""relation"":""eq""}", Options());
			hits.Value.Should().Be(5);
			hits.Relation.Should().Be(TotalHitsRelation.EqualTo);
		}

		[U] public void Read_Object_MissingValue_DefaultsToMinusOne()
		{
			// Legacy initialises value to -1 before reading object members.
			var hits = JsonSerializer.Deserialize<TotalHits>(@"{""relation"":""eq""}", Options());
			hits.Value.Should().Be(-1);
			hits.Relation.Should().Be(TotalHitsRelation.EqualTo);
		}

		[U] public void Read_BareNumber_SetsValueOnly()
		{
			var hits = JsonSerializer.Deserialize<TotalHits>("123", Options());
			hits.Should().NotBeNull();
			hits.Value.Should().Be(123);
			hits.Relation.Should().BeNull();
		}

		[U] public void Read_BareNumber_PreservesLongPrecision()
		{
			// 9007199254740993 == 2^53 + 1, not exactly representable as a double.
			const long big = 9007199254740993L;
			var hits = JsonSerializer.Deserialize<TotalHits>("9007199254740993", Options());
			hits.Value.Should().Be(big);
		}

		[U] public void Read_NonNumberNonObject_ReturnsNull()
		{
			var hits = JsonSerializer.Deserialize<TotalHits>(@"""nope""", Options());
			hits.Should().BeNull();
		}

		[U] public void Read_NullToken_ReturnsNull()
		{
			var hits = JsonSerializer.Deserialize<TotalHits>("null", Options());
			hits.Should().BeNull();
		}

		[U] public void Write_WithRelation_AsObject()
		{
			var hits = new TotalHits { Value = 42, Relation = TotalHitsRelation.EqualTo };
			var json = JsonSerializer.Serialize(hits, Options());
			json.Should().Be(@"{""value"":42,""relation"":""eq""}");
		}

		[U] public void Write_WithoutRelation_AsBareNumber()
		{
			var hits = new TotalHits { Value = 99 };
			var json = JsonSerializer.Serialize(hits, Options());
			json.Should().Be("99");
		}

		[U] public void Write_LargeLong_PreservesPrecision()
		{
			var hits = new TotalHits { Value = 9007199254740993L };
			var json = JsonSerializer.Serialize(hits, Options());
			json.Should().Be("9007199254740993");
		}

		[U] public void Write_Null()
		{
			var json = JsonSerializer.Serialize<TotalHits>(null, Options());
			json.Should().Be("null");
		}

		[U] public void RoundTrip_Object()
		{
			var options = Options();
			var json = JsonSerializer.Serialize(new TotalHits { Value = 250, Relation = TotalHitsRelation.GreaterThanOrEqualTo }, options);
			var hits = JsonSerializer.Deserialize<TotalHits>(json, options);
			hits.Value.Should().Be(250);
			hits.Relation.Should().Be(TotalHitsRelation.GreaterThanOrEqualTo);
		}
	}
}
