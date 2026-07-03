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
	/// A <see cref="System.Text.Json"/> converter for <see cref="GeoLocation"/>, replacing the
	/// vendored Utf8Json <c>GeoLocationFormatter</c> as part of #388. Serialized as
	/// <c>{ "lat": …, "lon": … }</c>; a converter is required on read because latitude/longitude are
	/// get-only and set through the constructor.
	/// </summary>
	internal sealed class GeoLocationConverter : JsonConverter<GeoLocation>
	{
		public override GeoLocation Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null) return null;

			using var document = JsonDocument.ParseValue(ref reader);
			var root = document.RootElement;

			switch (root.ValueKind)
			{
				case JsonValueKind.Object:
				{
					double lat = 0, lon = 0;
					foreach (var member in root.EnumerateObject())
					{
						if (member.Name == "lat") lat = member.Value.GetDouble();
						else if (member.Name == "lon") lon = member.Value.GetDouble();
					}
					return new GeoLocation(lat, lon);
				}
				case JsonValueKind.Array when root.GetArrayLength() == 2:
					// GeoJSON order is [lon, lat].
					return new GeoLocation(root[1].GetDouble(), root[0].GetDouble());
				default:
					return null;
			}
		}

		public override void Write(Utf8JsonWriter writer, GeoLocation value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			writer.WriteStartObject();
			writer.WritePropertyName("lat");
			JsonSerializer.Serialize(writer, value.Latitude, options);
			writer.WritePropertyName("lon");
			JsonSerializer.Serialize(writer, value.Longitude, options);
			writer.WriteEndObject();
		}
	}
}
