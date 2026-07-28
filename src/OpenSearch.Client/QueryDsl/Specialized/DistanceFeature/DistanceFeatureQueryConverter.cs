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
	/// System.Text.Json replacement for the legacy Utf8Json <c>DistanceFeatureQueryFormatter</c>.
	///
	/// Unlike the field-name queries that use the field as the object key, a distance-feature query serializes as a flat
	/// object with literal keys: the common <c>_name</c>/<c>boost</c> from <c>QueryBase</c>, a <c>field</c> (delegated to
	/// the registered settings-aware <see cref="FieldConverter"/>), and the <c>origin</c>/<c>pivot</c> unions. The origin
	/// is a <see cref="Union{GeoCoordinate, DateMath}"/> and the pivot a <see cref="Union{Distance, Time}"/>; the open
	/// generic union converter is not registered globally, so — matching the legacy formatter's two
	/// <c>UnionFormatter</c> instances — closed <see cref="UnionConverter{TFirst, TSecond}"/> instances are held here
	/// (the origin union attempts the second type when the first reads as null, mirroring the legacy
	/// <c>new UnionFormatter&lt;GeoCoordinate, DateMath&gt;(true)</c>). Because it reads/writes literal keys and delegates
	/// field resolution to the registered <see cref="FieldConverter"/>, this converter itself needs no settings.
	/// </summary>
	internal class DistanceFeatureQueryConverter : JsonConverter<IDistanceFeatureQuery>
	{
		private static readonly UnionConverter<GeoCoordinate, DateMath> OriginUnionConverter =
			new UnionConverter<GeoCoordinate, DateMath>(true);

		private static readonly UnionConverter<Distance, Time> PivotUnionConverter =
			new UnionConverter<Distance, Time>();

		public override IDistanceFeatureQuery Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null)
				return null;

			if (reader.TokenType != JsonTokenType.StartObject)
			{
				reader.Skip();
				return null;
			}

			var query = new DistanceFeatureQuery();

			while (reader.Read())
			{
				if (reader.TokenType == JsonTokenType.EndObject)
					break;

				if (reader.TokenType != JsonTokenType.PropertyName)
					continue;

				var property = reader.GetString();
				reader.Read(); // advance to value

				switch (property)
				{
					case "field":
						query.Field = JsonSerializer.Deserialize<Field>(ref reader, options);
						break;
					case "origin":
						query.Origin = OriginUnionConverter.Read(ref reader, typeof(Union<GeoCoordinate, DateMath>), options);
						break;
					case "pivot":
						query.Pivot = PivotUnionConverter.Read(ref reader, typeof(Union<Distance, Time>), options);
						break;
					case "boost":
						query.Boost = reader.GetDouble();
						break;
					case "_name":
						query.Name = reader.GetString();
						break;
					default:
						reader.Skip();
						break;
				}
			}

			return query;
		}

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
				writer.WriteNumber("boost", value.Boost.Value);

			writer.WritePropertyName("field");
			JsonSerializer.Serialize(writer, value.Field, options);

			writer.WritePropertyName("origin");
			OriginUnionConverter.Write(writer, value.Origin, options);

			writer.WritePropertyName("pivot");
			PivotUnionConverter.Write(writer, value.Pivot, options);

			writer.WriteEndObject();
		}
	}
}
