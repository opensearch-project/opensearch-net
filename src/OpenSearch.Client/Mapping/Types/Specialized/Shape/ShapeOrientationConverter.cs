/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenSearch.Client
{
	/// <summary>
	/// System.Text.Json replacement for the legacy Utf8Json <c>ShapeOrientationFormatter</c>.
	/// A <see cref="ShapeOrientation"/> serializes to <c>"clockwise"</c> (<see cref="ShapeOrientation.ClockWise"/>)
	/// or <c>"counterclockwise"</c> (<see cref="ShapeOrientation.CounterClockWise"/>). When reading, a JSON null or
	/// any unrecognized string yields <see cref="ShapeOrientation.CounterClockWise"/> (the default);
	/// <c>"clockwise"</c>/<c>"left"</c>/<c>"cw"</c> (case-insensitive) yield <see cref="ShapeOrientation.ClockWise"/>.
	/// </summary>
	internal class ShapeOrientationConverter : JsonConverter<ShapeOrientation>
	{
		// The legacy formatter treated a JSON null as the default (CounterClockWise), so we must be invoked on null.
		public override bool HandleNull => true;

		public override ShapeOrientation Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null)
				// Default
				return ShapeOrientation.CounterClockWise;

			var enumString = reader.GetString();
			switch (enumString?.ToUpperInvariant())
			{
				case "CLOCKWISE":
				case "LEFT":
				case "CW":
					return ShapeOrientation.ClockWise;
			}

			// Default
			return ShapeOrientation.CounterClockWise;
		}

		public override void Write(Utf8JsonWriter writer, ShapeOrientation value, JsonSerializerOptions options)
		{
			switch (value)
			{
				case ShapeOrientation.CounterClockWise:
					writer.WriteStringValue("counterclockwise");
					break;
				case ShapeOrientation.ClockWise:
					writer.WriteStringValue("clockwise");
					break;
			}
		}
	}

	/// <summary>
	/// System.Text.Json replacement for the legacy Utf8Json <c>NullableShapeOrientationFormatter</c>.
	/// Behaves like <see cref="ShapeOrientationConverter"/> but a JSON null reads/writes as <c>null</c>, and any
	/// unrecognized string yields <c>null</c> instead of the default.
	/// </summary>
	internal class NullableShapeOrientationConverter : JsonConverter<ShapeOrientation?>
	{
		public override ShapeOrientation? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null)
				return null;

			var enumString = reader.GetString();
			switch (enumString?.ToUpperInvariant())
			{
				case "COUNTERCLOCKWISE":
				case "RIGHT":
				case "CCW":
					return ShapeOrientation.CounterClockWise;
				case "CLOCKWISE":
				case "LEFT":
				case "CW":
					return ShapeOrientation.ClockWise;
				default:
					return null;
			}
		}

		public override void Write(Utf8JsonWriter writer, ShapeOrientation? value, JsonSerializerOptions options)
		{
			if (!value.HasValue)
			{
				writer.WriteNullValue();
				return;
			}

			switch (value.Value)
			{
				case ShapeOrientation.CounterClockWise:
					writer.WriteStringValue("counterclockwise");
					break;
				case ShapeOrientation.ClockWise:
					writer.WriteStringValue("clockwise");
					break;
			}
		}
	}
}
