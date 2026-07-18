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

namespace OpenSearch.Client
{
	/// <summary>
	/// System.Text.Json replacement for the legacy Utf8Json <c>RelationNameFormatter</c>. Serializes a
	/// <see cref="RelationName"/> as a JSON string, resolving it through the runtime <c>Inferrer</c> — hence a
	/// <see cref="SettingsAwareConverter{T}"/>.
	/// </summary>
	internal class RelationNameConverter : SettingsAwareConverter<RelationName>
	{
		public RelationNameConverter(IConnectionSettingsValues settings) : base(settings) { }

		public override RelationName Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.String)
			{
				RelationName relationName = reader.GetString();
				return relationName;
			}

			reader.Skip();
			return null;
		}

		public override void Write(Utf8JsonWriter writer, RelationName value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			writer.WriteStringValue(Settings.Inferrer.RelationName(value));
		}

		// RelationName is used as a dictionary key (e.g. join-field relations). STJ needs these overrides to
		// (de)serialize it as a property name; without them it throws NotSupportedException.
		public override RelationName ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			RelationName relationName = reader.GetString();
			return relationName;
		}

		public override void WriteAsPropertyName(Utf8JsonWriter writer, RelationName value, JsonSerializerOptions options) =>
			writer.WritePropertyName(value == null ? string.Empty : Settings.Inferrer.RelationName(value));
	}
}
