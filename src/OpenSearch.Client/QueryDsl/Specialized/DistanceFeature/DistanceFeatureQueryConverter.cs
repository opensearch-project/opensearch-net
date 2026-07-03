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
	/// A <see cref="System.Text.Json"/> converter for <see cref="IDistanceFeatureQuery"/>, replacing
	/// the vendored Utf8Json <c>DistanceFeatureQueryFormatter</c> as part of #388. Unlike the other
	/// field-name queries the field is written as a <c>field</c> value (not a key), followed by the
	/// <c>origin</c> (<see cref="GeoCoordinate"/> or date-math) and <c>pivot</c>
	/// (<see cref="Distance"/> or <see cref="Time"/>) unions.
	/// </summary>
	internal sealed class DistanceFeatureQueryConverter : JsonConverter<IDistanceFeatureQuery>
	{
		public override void Write(Utf8JsonWriter writer, IDistanceFeatureQuery value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			writer.WriteStartObject();

			if (!string.IsNullOrEmpty(value.Name))
				writer.WriteString("_name", value.Name);
			if (value.Boost.HasValue)
			{
				writer.WritePropertyName("boost");
				JsonSerializer.Serialize(writer, value.Boost.Value, options);
			}

			writer.WritePropertyName("field");
			JsonSerializer.Serialize(writer, value.Field, options);

			writer.WritePropertyName("origin");
			JsonSerializer.Serialize(writer, value.Origin, options);

			writer.WritePropertyName("pivot");
			JsonSerializer.Serialize(writer, value.Pivot, options);

			writer.WriteEndObject();
		}

		public override IDistanceFeatureQuery Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null) return null;

			using var document = JsonDocument.ParseValue(ref reader);
			var root = document.RootElement;
			if (root.ValueKind != JsonValueKind.Object) return null;

			var query = new DistanceFeatureQuery();
			foreach (var member in root.EnumerateObject())
			{
				switch (member.Name)
				{
					case "_name":
						query.Name = member.Value.GetString();
						break;
					case "boost":
						query.Boost = member.Value.GetDouble();
						break;
					case "field":
						query.Field = member.Value.Deserialize<Field>(options);
						break;
					case "origin":
						query.Origin = member.Value.Deserialize<Union<GeoCoordinate, DateMath>>(options);
						break;
					case "pivot":
						query.Pivot = member.Value.Deserialize<Union<Distance, Time>>(options);
						break;
				}
			}

			return query;
		}
	}
}
