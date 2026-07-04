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
	/// STJ converter for the completion-suggester <see cref="Context"/> union (#388), replacing the
	/// vendored <c>ContextFormatter</c>. A category context is written as a bare string; a geo context as
	/// its <see cref="GeoLocation"/> object.
	/// </summary>
	internal sealed class ContextConverter : JsonConverter<Context>
	{
		public override void Write(Utf8JsonWriter writer, Context value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			switch (value.Tag)
			{
				case 0:
					JsonSerializer.Serialize(writer, value.Item1, options);
					break;
				case 1:
					JsonSerializer.Serialize(writer, value.Item2, options);
					break;
				default:
					writer.WriteNullValue();
					break;
			}
		}

		public override Context Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null) return null;

			if (reader.TokenType == JsonTokenType.String)
				return new Context(reader.GetString());

			var geo = JsonSerializer.Deserialize<GeoLocation>(ref reader, options);
			return geo == null ? null : new Context(geo);
		}
	}
}
