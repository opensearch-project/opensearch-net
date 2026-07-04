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
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using OpenSearch.Net;

namespace OpenSearch.Client
{
	/// <summary>
	/// STJ converter for the array form of <see cref="Indices"/> (the vendored <c>IndicesFormatter</c>),
	/// applied per-member (e.g. alias-action <c>indices</c>) where the multi-index value must be written
	/// as a JSON array rather than the comma-joined string of the type-level <c>IndicesConverter</c> (#388).
	/// Stateless; the connection settings (for index-name inference) are resolved from the ambient options.
	/// </summary>
	internal sealed class IndicesArrayConverter : JsonConverter<Indices>
	{
		public override void Write(Utf8JsonWriter writer, Indices value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			switch (value.Tag)
			{
				case 0:
					writer.WriteStartArray();
					writer.WriteStringValue("_all");
					writer.WriteEndArray();
					break;
				case 1:
					var settings = SourceSerializerProviderConverter.Find(options)?.Settings;
					writer.WriteStartArray();
					foreach (var index in value.Item2.Indices)
						writer.WriteStringValue(index.GetString(settings));
					writer.WriteEndArray();
					break;
			}
		}

		public override Indices Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			switch (reader.TokenType)
			{
				case JsonTokenType.StartArray:
				{
					var indices = new List<IndexName>();
					while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
						indices.Add(reader.GetString());
					return new Indices(indices);
				}
				case JsonTokenType.String:
				{
					Indices indices = reader.GetString();
					return indices;
				}
				default:
					using (JsonDocument.ParseValue(ref reader)) { }
					return null;
			}
		}
	}
}
