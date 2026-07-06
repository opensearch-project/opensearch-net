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
	/// System.Text.Json replacement for the legacy Utf8Json <c>GeoCoordinateFormatter</c>. A
	/// <see cref="GeoCoordinate"/> is serialized as a <c>[lon, lat]</c> or <c>[lon, lat, z]</c> array.
	/// </summary>
	internal class GeoCoordinateConverter : JsonConverter<GeoCoordinate>
	{
		public override GeoCoordinate Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.StartArray)
			{
				reader.Skip();
				return null;
			}

			var doubles = JsonSerializer.Deserialize<double[]>(ref reader, options);
			switch (doubles.Length)
			{
				case 2:
					return new GeoCoordinate(doubles[1], doubles[0]);
				case 3:
					return new GeoCoordinate(doubles[1], doubles[0], doubles[2]);
				default:
					return null;
			}
		}

		public override void Write(Utf8JsonWriter writer, GeoCoordinate value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			writer.WriteStartArray();
			writer.WriteNumberValue(value.Longitude);
			writer.WriteNumberValue(value.Latitude);
			if (value.Z.HasValue)
				writer.WriteNumberValue(value.Z.Value);
			writer.WriteEndArray();
		}
	}
}
