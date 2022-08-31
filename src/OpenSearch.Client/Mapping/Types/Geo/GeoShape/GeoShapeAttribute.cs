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

namespace OpenSearch.Client
{
	/// <inheritdoc cref="IGeoShapeProperty" />
	public class GeoShapeAttribute : OpenSearchDocValuesPropertyAttributeBase, IGeoShapeProperty
	{
		public GeoShapeAttribute() : base(FieldType.GeoShape) { }

		bool? IGeoShapeProperty.IgnoreMalformed { get; set; }
		bool? IGeoShapeProperty.IgnoreZValue { get; set; }
		GeoOrientation? IGeoShapeProperty.Orientation { get; set; }
		private IGeoShapeProperty Self => this;
		GeoStrategy? IGeoShapeProperty.Strategy { get; set; }
		bool? IGeoShapeProperty.Coerce { get; set; }

		/// <inheritdoc cref="IGeoShapeProperty.IgnoreMalformed" />
		public bool IgnoreMalformed
		{
			get => Self.IgnoreMalformed.GetValueOrDefault(false);
			set => Self.IgnoreMalformed = value;
		}

		/// <inheritdoc cref="IGeoShapeProperty.IgnoreZValue" />
		public bool IgnoreZValue
		{
			get => Self.IgnoreZValue.GetValueOrDefault(true);
			set => Self.IgnoreZValue = value;
		}

		/// <inheritdoc cref="IGeoShapeProperty.Orientation" />
		public GeoOrientation Orientation
		{
			get => Self.Orientation.GetValueOrDefault(GeoOrientation.CounterClockWise);
			set => Self.Orientation = value;
		}

		/// <inheritdoc cref="IGeoShapeProperty.Strategy" />
		public GeoStrategy Strategy
		{
			get => Self.Strategy.GetValueOrDefault(GeoStrategy.Recursive);
			set => Self.Strategy = value;
		}

		/// <inheritdoc cref="IGeoShapeProperty.Coerce" />
		public bool Coerce
		{
			get => Self.Coerce.GetValueOrDefault(true);
			set => Self.Coerce = value;
		}

	}
}
