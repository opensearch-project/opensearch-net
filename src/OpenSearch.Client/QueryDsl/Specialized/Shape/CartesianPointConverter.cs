/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/
/*
* Modifications Copyright OpenSearch Contributors. See
* GitHub history for details.
*
*  Licensed to Elasticsearch B.V. under one or more contributor
*  license agreements. See the NOTICE file distributed with
*  this work for additional information regarding copyright
*  ownership. Elasticsearch B.V. licenses this file to you under
*  the Apache License, Version 2.0 (the "License"); you may
*  not use this file except in compliance with the License.
*  You may obtain a copy of the License at
*
* 	http://www.apache.org/licenses/LICENSE-2.0
*
*  Unless required by applicable law or agreed to in writing,
*  software distributed under the License is distributed on an
*  "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
*  KIND, either express or implied.  See the License for the
*  specific language governing permissions and limitations
*  under the License.
*/

using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenSearch.Client
{
	/// <summary>
	/// System.Text.Json replacement for the legacy Utf8Json <c>CartesianPointFormatter</c>.
	/// A <see cref="CartesianPoint"/> is serialized in one of four shapes, selected by its
	/// <see cref="CartesianPoint.Format"/>: an object (<c>{"x":X,"y":Y}</c>), an array (<c>[X,Y]</c>), Well-Known
	/// Text (<c>"POINT (X Y)"</c>), or a comma-separated string (<c>"X,Y"</c>). On read, an object or array is
	/// parsed positionally (a third coordinate <c>z</c> is accepted and ignored); a string is parsed either as
	/// coordinates (when it contains a comma) or as WKT.
	/// </summary>
	internal class CartesianPointConverter : JsonConverter<CartesianPoint>
	{
		public override CartesianPoint Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			switch (reader.TokenType)
			{
				case JsonTokenType.StartObject:
				{
					var point = new CartesianPoint { Format = ShapeFormat.Object };
					while (reader.Read())
					{
						if (reader.TokenType == JsonTokenType.EndObject)
							break;

						var property = reader.GetString();
						reader.Read();
						switch (property)
						{
							case "x":
								point.X = reader.GetSingle();
								break;
							case "y":
								point.Y = reader.GetSingle();
								break;
							case "z":
								reader.GetSingle();
								break;
							default:
								throw new JsonException($"Unknown property {property} when parsing {nameof(CartesianPoint)}");
						}
					}

					return point;
				}
				case JsonTokenType.StartArray:
				{
					var count = 0;
					var point = new CartesianPoint { Format = ShapeFormat.Array };
					while (reader.Read())
					{
						if (reader.TokenType == JsonTokenType.EndArray)
							break;

						count++;
						switch (count)
						{
							case 1:
								point.X = reader.GetSingle();
								break;
							case 2:
								point.Y = reader.GetSingle();
								break;
							case 3:
								reader.GetSingle();
								break;
							default:
								throw new JsonException($"Expected 2 or 3 coordinates but found {count}");
						}
					}

					return point;
				}
				case JsonTokenType.String:
				{
					var value = reader.GetString();
					return value.IndexOf(",", StringComparison.InvariantCultureIgnoreCase) > -1
						? CartesianPoint.FromCoordinates(value)
						: CartesianPoint.FromWellKnownText(value);
				}
				default:
					throw new JsonException($"Unexpected token type {reader.TokenType} when parsing {nameof(CartesianPoint)}");
			}
		}

		public override void Write(Utf8JsonWriter writer, CartesianPoint value, JsonSerializerOptions options)
		{
			if (value is null)
			{
				writer.WriteNullValue();
				return;
			}

			switch (value.Format)
			{
				case ShapeFormat.Object:
					writer.WriteStartObject();
					writer.WriteNumber("x", value.X);
					writer.WriteNumber("y", value.Y);
					writer.WriteEndObject();
					break;
				case ShapeFormat.Array:
					writer.WriteStartArray();
					writer.WriteNumberValue(value.X);
					writer.WriteNumberValue(value.Y);
					writer.WriteEndArray();
					break;
				case ShapeFormat.WellKnownText:
					writer.WriteStringValue(
						$"{GeoShapeType.Point} ({value.X.ToString(CultureInfo.InvariantCulture)} {value.Y.ToString(CultureInfo.InvariantCulture)})");
					break;
				case ShapeFormat.String:
					writer.WriteStringValue(
						$"{value.X.ToString(CultureInfo.InvariantCulture)},{value.Y.ToString(CultureInfo.InvariantCulture)}");
					break;
			}
		}
	}
}
