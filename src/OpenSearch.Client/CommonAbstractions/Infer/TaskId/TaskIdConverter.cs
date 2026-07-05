/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenSearch.Client
{
	/// <summary>
	/// A <see cref="System.Text.Json"/> converter for <see cref="TaskId"/>, replacing the vendored
	/// Utf8Json <c>TaskIdFormatter</c> as part of #388. Serializes the fully-qualified
	/// <c>[node_id]:[task_id]</c> as a JSON string; deserializes a JSON string into a
	/// <see cref="TaskId"/> (any other token yields null). Stateless.
	/// </summary>
	internal sealed class TaskIdConverter : JsonConverter<TaskId>
	{
		public override void Write(Utf8JsonWriter writer, TaskId value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			writer.WriteStringValue(value.ToString());
		}

		public override TaskId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.String)
				return new TaskId(reader.GetString());

			using (JsonDocument.ParseValue(ref reader)) { }
			return null;
		}

		/// <inheritdoc />
		public override TaskId ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
			new(reader.GetString());

		/// <inheritdoc />
		public override void WriteAsPropertyName(Utf8JsonWriter writer, TaskId value, JsonSerializerOptions options) =>
			writer.WritePropertyName(value.ToString());
	}
}
