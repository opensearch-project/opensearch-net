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
	/// STJ converter for <see cref="GeoOrientation"/> (#388), replacing the vendored
	/// <c>GeoOrientationFormatter</c>. Writes <c>"cw"</c>/<c>"ccw"</c>; reads LEFT/CW/CLOCKWISE as
	/// clockwise, everything else as the OGC-standard counter-clockwise default.
	/// </summary>
	internal sealed class GeoOrientationConverter : JsonConverter<GeoOrientation>
	{
		public override void Write(Utf8JsonWriter writer, GeoOrientation value, JsonSerializerOptions options) =>
			writer.WriteStringValue(value == GeoOrientation.ClockWise ? "cw" : "ccw");

		public override GeoOrientation Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null) return GeoOrientation.CounterClockWise;
			var s = reader.GetString()?.ToUpperInvariant();
			return s == "LEFT" || s == "CW" || s == "CLOCKWISE" ? GeoOrientation.ClockWise : GeoOrientation.CounterClockWise;
		}
	}

	internal sealed class NullableGeoOrientationConverter : JsonConverter<GeoOrientation?>
	{
		public override void Write(Utf8JsonWriter writer, GeoOrientation? value, JsonSerializerOptions options)
		{
			if (value == null) { writer.WriteNullValue(); return; }
			writer.WriteStringValue(value.Value == GeoOrientation.ClockWise ? "cw" : "ccw");
		}

		public override GeoOrientation? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null) return null;
			switch (reader.GetString()?.ToUpperInvariant())
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
	}
}
