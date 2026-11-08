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
	/// Behavioural tests for <see cref="ShapeOrientationConverter"/> and <see cref="NullableShapeOrientationConverter"/>.
	/// A <see cref="ShapeOrientation"/> serializes to <c>"clockwise"</c>/<c>"counterclockwise"</c>. On read,
	/// <c>clockwise</c>/<c>left</c>/<c>cw</c> (case-insensitive) map to <see cref="ShapeOrientation.ClockWise"/>;
	/// everything else (including null, for the non-nullable converter) defaults to
	/// <see cref="ShapeOrientation.CounterClockWise"/>. The nullable converter returns <c>null</c> for null and for
	/// unrecognized strings.
	/// </summary>
	public class ShapeOrientationConverterTests
	{
		private static JsonSerializerOptions Options()
		{
			var options = new JsonSerializerOptions();
			options.Converters.Add(new ShapeOrientationConverter());
			return options;
		}

		private static JsonSerializerOptions NullableOptions()
		{
			var options = new JsonSerializerOptions();
			options.Converters.Add(new NullableShapeOrientationConverter());
			return options;
		}

		// ---- non-nullable write ----

		[U] public void Write_ClockWise() =>
			JsonSerializer.Serialize(ShapeOrientation.ClockWise, Options()).Should().Be(@"""clockwise""");

		[U] public void Write_CounterClockWise() =>
			JsonSerializer.Serialize(ShapeOrientation.CounterClockWise, Options()).Should().Be(@"""counterclockwise""");

		// ---- non-nullable read ----

		[U] public void Read_Clockwise_IsClockWise() =>
			JsonSerializer.Deserialize<ShapeOrientation>(@"""clockwise""", Options()).Should().Be(ShapeOrientation.ClockWise);

		[U] public void Read_Left_IsClockWise() =>
			JsonSerializer.Deserialize<ShapeOrientation>(@"""LEFT""", Options()).Should().Be(ShapeOrientation.ClockWise);

		[U] public void Read_Cw_IsClockWise() =>
			JsonSerializer.Deserialize<ShapeOrientation>(@"""cw""", Options()).Should().Be(ShapeOrientation.ClockWise);

		[U] public void Read_Right_DefaultsToCounterClockWise() =>
			JsonSerializer.Deserialize<ShapeOrientation>(@"""right""", Options()).Should().Be(ShapeOrientation.CounterClockWise);

		[U] public void Read_Unknown_DefaultsToCounterClockWise() =>
			JsonSerializer.Deserialize<ShapeOrientation>(@"""nonsense""", Options()).Should().Be(ShapeOrientation.CounterClockWise);

		[U] public void Read_Null_DefaultsToCounterClockWise() =>
			JsonSerializer.Deserialize<ShapeOrientation>("null", Options()).Should().Be(ShapeOrientation.CounterClockWise);

		[U] public void RoundTrip_NonNullable()
		{
			var options = Options();
			var json = JsonSerializer.Serialize(ShapeOrientation.ClockWise, options);
			JsonSerializer.Deserialize<ShapeOrientation>(json, options).Should().Be(ShapeOrientation.ClockWise);
		}

		// ---- nullable write ----

		[U] public void WriteNullable_ClockWise() =>
			JsonSerializer.Serialize<ShapeOrientation?>(ShapeOrientation.ClockWise, NullableOptions()).Should().Be(@"""clockwise""");

		[U] public void WriteNullable_CounterClockWise() =>
			JsonSerializer.Serialize<ShapeOrientation?>(ShapeOrientation.CounterClockWise, NullableOptions()).Should().Be(@"""counterclockwise""");

		[U] public void WriteNullable_Null() =>
			JsonSerializer.Serialize<ShapeOrientation?>(null, NullableOptions()).Should().Be("null");

		// ---- nullable read ----

		[U] public void ReadNullable_Clockwise() =>
			JsonSerializer.Deserialize<ShapeOrientation?>(@"""clockwise""", NullableOptions()).Should().Be(ShapeOrientation.ClockWise);

		[U] public void ReadNullable_CounterClockwise() =>
			JsonSerializer.Deserialize<ShapeOrientation?>(@"""counterclockwise""", NullableOptions()).Should().Be(ShapeOrientation.CounterClockWise);

		[U] public void ReadNullable_Right_IsCounterClockWise() =>
			JsonSerializer.Deserialize<ShapeOrientation?>(@"""RIGHT""", NullableOptions()).Should().Be(ShapeOrientation.CounterClockWise);

		[U] public void ReadNullable_Ccw_IsCounterClockWise() =>
			JsonSerializer.Deserialize<ShapeOrientation?>(@"""ccw""", NullableOptions()).Should().Be(ShapeOrientation.CounterClockWise);

		[U] public void ReadNullable_Null_IsNull() =>
			JsonSerializer.Deserialize<ShapeOrientation?>("null", NullableOptions()).Should().BeNull();

		[U] public void ReadNullable_Unknown_IsNull() =>
			JsonSerializer.Deserialize<ShapeOrientation?>(@"""nonsense""", NullableOptions()).Should().BeNull();

		[U] public void RoundTrip_Nullable()
		{
			var options = NullableOptions();
			var json = JsonSerializer.Serialize<ShapeOrientation?>(ShapeOrientation.ClockWise, options);
			JsonSerializer.Deserialize<ShapeOrientation?>(json, options).Should().Be(ShapeOrientation.ClockWise);
		}
	}
}
