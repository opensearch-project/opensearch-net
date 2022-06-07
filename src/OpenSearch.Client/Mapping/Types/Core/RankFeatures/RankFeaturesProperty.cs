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

using System.Diagnostics;
using OpenSearch.Net.Utf8Json;

namespace OpenSearch.Client
{
	/// <summary>
	/// A field that can index numeric feature vectors, so that they can later be used to boost documents in queries with a rank_feature query.
	/// It is analogous to the <see cref="IRankFeatureProperty"/> datatype, but is better suited when the list of features is sparse so that it
	/// wouldn't be reasonable to add one field to the mappings for each of them.
	/// </summary>
	[InterfaceDataContract]
	public interface IRankFeaturesProperty : IProperty
	{
	}

	/// <inheritdoc cref="IRankFeaturesProperty" />
	public class RankFeaturesProperty : PropertyBase, IRankFeaturesProperty
	{
		public RankFeaturesProperty() : base(FieldType.RankFeatures) { }
	}

	/// <inheritdoc cref="IRankFeaturesProperty" />
	[DebuggerDisplay("{DebugDisplay}")]
	public class RankFeaturesPropertyDescriptor<T>
		: PropertyDescriptorBase<RankFeaturesPropertyDescriptor<T>, IRankFeaturesProperty, T>, IRankFeaturesProperty
		where T : class
	{
		public RankFeaturesPropertyDescriptor() : base(FieldType.RankFeatures) { }
	}
}
