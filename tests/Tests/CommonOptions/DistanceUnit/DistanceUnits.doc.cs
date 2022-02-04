/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*
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

using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Osc;
using Tests.Framework;
using static Tests.Core.Serialization.SerializationTestHelper;

namespace Tests.CommonOptions.DistanceUnit
{
	public class DistanceUnits
	{
		/**[[distance-units]]
		 * === Distance units
		 * Whenever distances need to be specified, e.g. for a {ref_current}/query-dsl-geo-distance-query.html[geo distance query],
		 * the distance unit can be specified as a double number representing distance in meters, as a new instance of
		 * a `Distance`, or as a string of the form number and distance unit e.g. "`2.72km`"
		 *
		 * OSC uses a `Distance` type to strongly type distance units and there are several ways to construct one.
		 *
		 * ==== Constructor
		 * The most straight forward way to construct a `Distance` is through its constructor
		 */
		[U]
		public void Constructor()
		{
			var unitComposed = new Distance(25);
			var unitComposedWithUnits = new Distance(25, Osc.DistanceUnit.Meters);

			/**
			* `Distance` serializes to a string composed of a factor and distance unit.
			* The factor is a double so always has at least one decimal place when serialized
			*/
			Expect("25m")
				.WhenSerializing(unitComposed)
				.WhenSerializing(unitComposedWithUnits);
		}

		/**
		* ==== Implicit conversion
		* Alternatively a distance unit `string` can be assigned to a `Distance`, resulting in an implicit conversion to a new `Distance` instance.
		* If no `DistanceUnit` is specified, the default distance unit is meters
		*/
		[U]
		public void ImplicitConversion()
		{
			Distance distanceString = "25";
			Distance distanceStringWithUnits = "25m";

			Expect("25m")
				.WhenSerializing(distanceString)
				.WhenSerializing(distanceStringWithUnits);
		}

		/**
		* ==== Supported units
		* A number of distance units are supported, from millimeters to nautical miles
		*/
		[U]
		public void UsingDifferentUnits()
		{
			/** ===== Metric
			*`mm` (Millimeters)
			*/
			Expect("2mm").WhenSerializing(new Distance(2, Osc.DistanceUnit.Millimeters));

			/**
			*`cm` (Centimeters)
			*/
			Expect("123.456cm").WhenSerializing(new Distance(123.456, Osc.DistanceUnit.Centimeters));

			/**
			*`m` (Meters)
			*/
			Expect("400m").WhenSerializing(new Distance(400, Osc.DistanceUnit.Meters));

			/**
			*`km` (Kilometers)
			*/
			Expect("0.1km").WhenSerializing(new Distance(0.1, Osc.DistanceUnit.Kilometers));

			/** ===== Imperial
			*`in` (Inches)
			*/
			Expect("43.23in").WhenSerializing(new Distance(43.23, Osc.DistanceUnit.Inch));

			/**
			*`ft` (Feet)
			*/
			Expect("3.33ft").WhenSerializing(new Distance(3.33, Osc.DistanceUnit.Feet));

			/**
			*`yd` (Yards)
			*/
			Expect("9yd").WhenSerializing(new Distance(9, Osc.DistanceUnit.Yards));

			/**
			*`mi` (Miles)
			*/
			Expect("0.62mi").WhenSerializing(new Distance(0.62, Osc.DistanceUnit.Miles));

			/**
			*`nmi` or `NM` (Nautical Miles)
			*/
			Expect("45.5nmi").WhenSerializing(new Distance(45.5, Osc.DistanceUnit.NauticalMiles));
		}
	}
}
