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
	/// System.Text.Json replacement for the legacy Utf8Json <c>TaskIdFormatter</c>. A <see cref="TaskId"/> is
	/// serialized as its <see cref="TaskId.ToString"/> (the fully-qualified <c>[node_id]:[task_id]</c>) string. On
	/// read, a JSON string is parsed into a <see cref="TaskId"/>; any other token is skipped and yields <c>null</c>.
	/// The legacy formatter also implemented <c>IObjectPropertyNameFormatter&lt;TaskId&gt;</c> (<see cref="TaskId"/> is
	/// used as a dictionary key), so the same string behavior is preserved for property names via
	/// <see cref="ReadAsPropertyName"/>/<see cref="WriteAsPropertyName"/>.
	/// </summary>
	internal class TaskIdConverter : JsonConverter<TaskId>
	{
		public override TaskId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.String)
				return new TaskId(reader.GetString());

			reader.Skip();
			return null;
		}

		public override void Write(Utf8JsonWriter writer, TaskId value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			writer.WriteStringValue(value.ToString());
		}

		public override TaskId ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.PropertyName)
				return new TaskId(reader.GetString());

			reader.Skip();
			return null;
		}

		public override void WriteAsPropertyName(Utf8JsonWriter writer, TaskId value, JsonSerializerOptions options) =>
			writer.WritePropertyName(value.ToString());
	}
}
