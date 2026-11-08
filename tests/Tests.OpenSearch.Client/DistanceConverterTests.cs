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
	/// Behavioural tests for <see cref="DistanceConverter"/>. A <see cref="Distance"/> serializes to
	/// its string representation and reads back from a JSON string; non-string tokens are skipped and
	/// yield <c>null</c>.
	/// </summary>
	public class DistanceConverterTests
	{
		private static JsonSerializerOptions Options()
		{
			var options = new JsonSerializerOptions();
			options.Converters.Add(new DistanceConverter());
			return options;
		}

		[U] public void Read_String()
		{
			var distance = JsonSerializer.Deserialize<Distance>(@"""10km""", Options());
			distance.Should().NotBeNull();
			distance.Precision.Should().Be(10);
			distance.Unit.Should().Be(DistanceUnit.Kilometers);
		}

		[U] public void Read_String_DefaultUnitMeters()
		{
			var distance = JsonSerializer.Deserialize<Distance>(@"""42""", Options());
			distance.Should().NotBeNull();
			distance.Precision.Should().Be(42);
			distance.Unit.Should().Be(DistanceUnit.Meters);
		}

		[U] public void Read_Null()
		{
			var distance = JsonSerializer.Deserialize<Distance>("null", Options());
			distance.Should().BeNull();
		}

		[U] public void Read_NonStringToken_IsSkipped_ReturnsNull()
		{
			// A number token is not a string; the converter skips it and returns null.
			var distance = JsonSerializer.Deserialize<Distance>("123", Options());
			distance.Should().BeNull();
		}

		[U] public void Read_ObjectToken_IsSkipped_ReturnsNull()
		{
			var distance = JsonSerializer.Deserialize<Distance>(@"{""precision"":1,""unit"":""m""}", Options());
			distance.Should().BeNull();
		}

		[U] public void Write_Value()
		{
			var json = JsonSerializer.Serialize(new Distance(5, DistanceUnit.Miles), Options());
			json.Should().Be(@"""5mi""");
		}

		[U] public void Write_Null()
		{
			var json = JsonSerializer.Serialize<Distance>(null, Options());
			json.Should().Be("null");
		}

		[U] public void RoundTrip()
		{
			var json = JsonSerializer.Serialize(new Distance(2.5, DistanceUnit.Kilometers), Options());
			var distance = JsonSerializer.Deserialize<Distance>(json, Options());
			distance.Should().NotBeNull();
			distance.Precision.Should().Be(2.5);
			distance.Unit.Should().Be(DistanceUnit.Kilometers);
		}
	}
}
