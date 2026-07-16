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
	/// System.Text.Json replacement for the legacy Utf8Json <c>AutoExpandReplicasFormatter</c>. Reads a JSON
	/// <c>false</c> as <see cref="AutoExpandReplicas.Disabled"/> and a JSON string (e.g. <c>"0-5"</c>, <c>"0-all"</c>,
	/// <c>"false"</c>) via <see cref="AutoExpandReplicas.Create(string)"/>; any other token throws. Writes
	/// <c>false</c> when the value is <c>null</c> or not enabled, otherwise the <see cref="AutoExpandReplicas.ToString"/>
	/// string.
	/// </summary>
	internal class AutoExpandReplicasConverter : JsonConverter<AutoExpandReplicas>
	{
		// A null value must serialize as `false` (disabled), matching the legacy formatter. STJ skips the converter
		// for a null reference type unless HandleNull is true, so opt in to keep that behaviour.
		public override bool HandleNull => true;

		public override AutoExpandReplicas Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			switch (reader.TokenType)
			{
				case JsonTokenType.False:
					return AutoExpandReplicas.Disabled;
				case JsonTokenType.String:
					return AutoExpandReplicas.Create(reader.GetString());
				default:
					throw new JsonException($"Cannot deserialize {typeof(AutoExpandReplicas)} from {reader.TokenType}");
			}
		}

		public override void Write(Utf8JsonWriter writer, AutoExpandReplicas value, JsonSerializerOptions options)
		{
			if (value == null || !value.Enabled)
			{
				writer.WriteBooleanValue(false);
				return;
			}

			writer.WriteStringValue(value.ToString());
		}
	}
}
