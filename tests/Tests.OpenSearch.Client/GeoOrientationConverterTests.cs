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
	/// Behavioural tests for <see cref="GeoOrientationConverter"/> and <see cref="NullableGeoOrientationConverter"/>.
	/// A <see cref="GeoOrientation"/> serializes to <c>"cw"</c>/<c>"ccw"</c>. On read, <c>left</c>/<c>cw</c>/<c>clockwise</c>
	/// (case-insensitive) map to <see cref="GeoOrientation.ClockWise"/>; everything else (including null, for the
	/// non-nullable converter) defaults to <see cref="GeoOrientation.CounterClockWise"/>. The nullable converter
	/// returns <c>null</c> for null and for unrecognized strings.
	/// </summary>
	public class GeoOrientationConverterTests
	{
		private static JsonSerializerOptions Options()
		{
			var options = new JsonSerializerOptions();
			options.Converters.Add(new GeoOrientationConverter());
			return options;
		}

		private static JsonSerializerOptions NullableOptions()
		{
			var options = new JsonSerializerOptions();
			options.Converters.Add(new NullableGeoOrientationConverter());
			return options;
		}

		// ---- non-nullable write ----

		[U] public void Write_ClockWise() =>
			JsonSerializer.Serialize(GeoOrientation.ClockWise, Options()).Should().Be(@"""cw""");

		[U] public void Write_CounterClockWise() =>
			JsonSerializer.Serialize(GeoOrientation.CounterClockWise, Options()).Should().Be(@"""ccw""");

		// ---- non-nullable read ----

		[U] public void Read_Left_IsClockWise() =>
			JsonSerializer.Deserialize<GeoOrientation>(@"""left""", Options()).Should().Be(GeoOrientation.ClockWise);

		[U] public void Read_Cw_IsClockWise() =>
			JsonSerializer.Deserialize<GeoOrientation>(@"""CW""", Options()).Should().Be(GeoOrientation.ClockWise);

		[U] public void Read_Clockwise_IsClockWise() =>
			JsonSerializer.Deserialize<GeoOrientation>(@"""ClockWise""", Options()).Should().Be(GeoOrientation.ClockWise);

		[U] public void Read_Right_DefaultsToCounterClockWise() =>
			JsonSerializer.Deserialize<GeoOrientation>(@"""right""", Options()).Should().Be(GeoOrientation.CounterClockWise);

		[U] public void Read_Unknown_DefaultsToCounterClockWise() =>
			JsonSerializer.Deserialize<GeoOrientation>(@"""nonsense""", Options()).Should().Be(GeoOrientation.CounterClockWise);

		[U] public void Read_Null_DefaultsToCounterClockWise() =>
			JsonSerializer.Deserialize<GeoOrientation>("null", Options()).Should().Be(GeoOrientation.CounterClockWise);

		[U] public void RoundTrip_NonNullable()
		{
			var options = Options();
			var json = JsonSerializer.Serialize(GeoOrientation.ClockWise, options);
			JsonSerializer.Deserialize<GeoOrientation>(json, options).Should().Be(GeoOrientation.ClockWise);
		}

		// ---- nullable write ----

		[U] public void WriteNullable_ClockWise() =>
			JsonSerializer.Serialize<GeoOrientation?>(GeoOrientation.ClockWise, NullableOptions()).Should().Be(@"""cw""");

		[U] public void WriteNullable_CounterClockWise() =>
			JsonSerializer.Serialize<GeoOrientation?>(GeoOrientation.CounterClockWise, NullableOptions()).Should().Be(@"""ccw""");

		[U] public void WriteNullable_Null() =>
			JsonSerializer.Serialize<GeoOrientation?>(null, NullableOptions()).Should().Be("null");

		// ---- nullable read ----

		[U] public void ReadNullable_Clockwise() =>
			JsonSerializer.Deserialize<GeoOrientation?>(@"""clockwise""", NullableOptions()).Should().Be(GeoOrientation.ClockWise);

		[U] public void ReadNullable_CounterClockwise() =>
			JsonSerializer.Deserialize<GeoOrientation?>(@"""ccw""", NullableOptions()).Should().Be(GeoOrientation.CounterClockWise);

		[U] public void ReadNullable_Right_IsCounterClockWise() =>
			JsonSerializer.Deserialize<GeoOrientation?>(@"""right""", NullableOptions()).Should().Be(GeoOrientation.CounterClockWise);

		[U] public void ReadNullable_Null_IsNull() =>
			JsonSerializer.Deserialize<GeoOrientation?>("null", NullableOptions()).Should().BeNull();

		[U] public void ReadNullable_Unknown_IsNull() =>
			JsonSerializer.Deserialize<GeoOrientation?>(@"""nonsense""", NullableOptions()).Should().BeNull();

		[U] public void RoundTrip_Nullable()
		{
			var options = NullableOptions();
			var json = JsonSerializer.Serialize<GeoOrientation?>(GeoOrientation.CounterClockWise, options);
			JsonSerializer.Deserialize<GeoOrientation?>(json, options).Should().Be(GeoOrientation.CounterClockWise);
		}
	}
}
