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
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenSearch.Client
{
	/// <summary>
	/// System.Text.Json replacement for the legacy Utf8Json <c>GeoOrientationFormatter</c>.
	/// A <see cref="GeoOrientation"/> serializes to <c>"cw"</c> (<see cref="GeoOrientation.ClockWise"/>)
	/// or <c>"ccw"</c> (<see cref="GeoOrientation.CounterClockWise"/>). When reading, a JSON null or any
	/// unrecognized string yields <see cref="GeoOrientation.CounterClockWise"/> (the OGC standard default);
	/// <c>"left"</c>/<c>"cw"</c>/<c>"clockwise"</c> (case-insensitive) yield <see cref="GeoOrientation.ClockWise"/>.
	/// </summary>
	internal class GeoOrientationConverter : JsonConverter<GeoOrientation>
	{
		// The legacy formatter treated a JSON null as the default (CounterClockWise), so we must be invoked on null.
		public override bool HandleNull => true;

		public override GeoOrientation Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null)
				// Default, complies with the OGC standard
				return GeoOrientation.CounterClockWise;

			var enumString = reader.GetString();
			switch (enumString?.ToUpperInvariant())
			{
				case "LEFT":
				case "CW":
				case "CLOCKWISE":
					return GeoOrientation.ClockWise;
			}

			// Default, complies with the OGC standard
			return GeoOrientation.CounterClockWise;
		}

		public override void Write(Utf8JsonWriter writer, GeoOrientation value, JsonSerializerOptions options)
		{
			switch (value)
			{
				case GeoOrientation.ClockWise:
					writer.WriteStringValue("cw");
					break;
				case GeoOrientation.CounterClockWise:
					writer.WriteStringValue("ccw");
					break;
			}
		}
	}

	/// <summary>
	/// System.Text.Json replacement for the legacy Utf8Json <c>NullableGeoOrientationFormatter</c>.
	/// Behaves like <see cref="GeoOrientationConverter"/> but a JSON null reads/writes as <c>null</c>, and any
	/// unrecognized string yields <c>null</c> instead of the default.
	/// </summary>
	internal class NullableGeoOrientationConverter : JsonConverter<GeoOrientation?>
	{
		public override GeoOrientation? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null)
				return null;

			var enumString = reader.GetString();
			switch (enumString?.ToUpperInvariant())
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

			switch (value.Value)
			{
				case GeoOrientation.ClockWise:
					writer.WriteStringValue("cw");
					break;
				case GeoOrientation.CounterClockWise:
					writer.WriteStringValue("ccw");
					break;
			}
		}
	}
}
