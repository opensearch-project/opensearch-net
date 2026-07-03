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
	/// A <see cref="System.Text.Json"/> converter for <see cref="GeoCoordinate"/>, replacing the
	/// vendored Utf8Json <c>GeoCoordinateFormatter</c> as part of #388. Serialized as a GeoJSON
	/// coordinate array <c>[lon, lat]</c> (or <c>[lon, lat, z]</c>), distinct from the
	/// <c>{ "lat": …, "lon": … }</c> shape used by the <see cref="GeoLocation"/> base.
	/// </summary>
	internal sealed class GeoCoordinateConverter : JsonConverter<GeoCoordinate>
	{
		public override void Write(Utf8JsonWriter writer, GeoCoordinate value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			writer.WriteStartArray();
			JsonSerializer.Serialize(writer, value.Longitude, options);
			JsonSerializer.Serialize(writer, value.Latitude, options);
			if (value.Z.HasValue)
				JsonSerializer.Serialize(writer, value.Z.Value, options);
			writer.WriteEndArray();
		}

		public override GeoCoordinate Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null) return null;

			using var document = JsonDocument.ParseValue(ref reader);
			var root = document.RootElement;
			if (root.ValueKind != JsonValueKind.Array) return null;

			switch (root.GetArrayLength())
			{
				case 2:
					return new GeoCoordinate(root[1].GetDouble(), root[0].GetDouble());
				case 3:
					return new GeoCoordinate(root[1].GetDouble(), root[0].GetDouble(), root[2].GetDouble());
				default:
					return null;
			}
		}
	}
}
