/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

#nullable enable

using System;
#nullable enable

using System.Text.Json;
#nullable enable

using System.Text.Json.Serialization;

namespace OpenSearch.Client.Mapping.SystemTextJsonConverters
{
	/// <summary>
	/// Converter for <see cref="GeoOrientation"/>.
	/// Maps between the enum and string values: "left"/"cw"/"clockwise" = ClockWise,
	/// "right"/"ccw"/"counterclockwise" = CounterClockWise.
	/// </summary>
	internal sealed class GeoOrientationConverter : JsonConverter<GeoOrientation>
	{
		public override GeoOrientation Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null)
				return GeoOrientation.CounterClockWise;

			var value = reader.GetString();
			if (string.IsNullOrEmpty(value))
				return GeoOrientation.CounterClockWise;

			switch (value.ToUpperInvariant())
			{
				case "LEFT":
				case "CW":
				case "CLOCKWISE":
					return GeoOrientation.ClockWise;
				case "RIGHT":
				case "CCW":
				case "COUNTERCLOCKWISE":
				default:
					return GeoOrientation.CounterClockWise;
			}
		}

		public override void Write(Utf8JsonWriter writer, GeoOrientation value, JsonSerializerOptions options)
		{
			switch (value)
			{
				case GeoOrientation.ClockWise:
					writer.WriteStringValue("cw");
					break;
				case GeoOrientation.CounterClockWise:
				default:
					writer.WriteStringValue("ccw");
					break;
			}
		}
	}

	/// <summary>
	/// Converter for nullable <see cref="GeoOrientation"/>.
	/// </summary>
	internal sealed class NullableGeoOrientationConverter : JsonConverter<GeoOrientation?>
	{
		private static readonly GeoOrientationConverter InnerConverter = new GeoOrientationConverter();

		public override GeoOrientation? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null)
				return null;

			var value = reader.GetString();
			if (string.IsNullOrEmpty(value))
				return null;

			switch (value.ToUpperInvariant())
			{
				case "LEFT":
				case "CW":
				case "CLOCKWISE":
					return GeoOrientation.ClockWise;
				case "RIGHT":
				case "CCW":
				case "COUNTERCLOCKWISE":
					return GeoOrientation.CounterClockWise;
				default:
					return null;
			}
		}

		public override void Write(Utf8JsonWriter writer, GeoOrientation? value, JsonSerializerOptions options)
		{
			if (!value.HasValue)
			{
				writer.WriteNullValue();
				return;
			}

			InnerConverter.Write(writer, value.Value, options);
		}
	}
}
