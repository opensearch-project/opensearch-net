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
	/// STJ converter for <see cref="ShapeOrientation"/> (#388), replacing the vendored
	/// <c>ShapeOrientationFormatter</c>. Writes <c>"clockwise"</c>/<c>"counterclockwise"</c>.
	/// </summary>
	internal sealed class ShapeOrientationConverter : JsonConverter<ShapeOrientation>
	{
		public override void Write(Utf8JsonWriter writer, ShapeOrientation value, JsonSerializerOptions options) =>
			writer.WriteStringValue(value == ShapeOrientation.ClockWise ? "clockwise" : "counterclockwise");

		public override ShapeOrientation Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null) return ShapeOrientation.CounterClockWise;
			var s = reader.GetString()?.ToUpperInvariant();
			return s == "CLOCKWISE" || s == "LEFT" || s == "CW" ? ShapeOrientation.ClockWise : ShapeOrientation.CounterClockWise;
		}
	}

	internal sealed class NullableShapeOrientationConverter : JsonConverter<ShapeOrientation?>
	{
		public override void Write(Utf8JsonWriter writer, ShapeOrientation? value, JsonSerializerOptions options)
		{
			if (value == null) { writer.WriteNullValue(); return; }
			writer.WriteStringValue(value.Value == ShapeOrientation.ClockWise ? "clockwise" : "counterclockwise");
		}

		public override ShapeOrientation? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null) return null;
			switch (reader.GetString()?.ToUpperInvariant())
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
	}
}
