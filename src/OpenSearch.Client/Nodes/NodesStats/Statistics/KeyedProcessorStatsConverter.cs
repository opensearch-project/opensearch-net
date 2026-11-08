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
	/// System.Text.Json replacement for the legacy Utf8Json <c>KeyedProcessorStatsFormatter</c>.
	///
	/// A <see cref="KeyedProcessorStats"/> is a single-entry object <c>{ "&lt;type&gt;": { ...ProcessStats... } }</c>:
	/// the (only) property name is the processor <see cref="KeyedProcessorStats.Type"/> and its value is the
	/// <see cref="KeyedProcessorStats.Statistics"/> (a <see cref="ProcessStats"/>). A non-object reads as null; on
	/// write, a null value or a value with a null <c>Type</c> writes JSON <c>null</c>.
	/// </summary>
	internal class KeyedProcessorStatsConverter : JsonConverter<KeyedProcessorStats>
	{
		public override KeyedProcessorStats Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.StartObject)
			{
				reader.Skip();
				return null;
			}

			var stats = new KeyedProcessorStats();
			while (reader.Read())
			{
				if (reader.TokenType == JsonTokenType.EndObject)
					break;

				stats.Type = reader.GetString();
				reader.Read();
				stats.Statistics = JsonSerializer.Deserialize<ProcessStats>(ref reader, options);
			}

			return stats;
		}

		public override void Write(Utf8JsonWriter writer, KeyedProcessorStats value, JsonSerializerOptions options)
		{
			if (value?.Type == null)
			{
				writer.WriteNullValue();
				return;
			}

			writer.WriteStartObject();
			writer.WritePropertyName(value.Type);
			JsonSerializer.Serialize(writer, value.Statistics, options);
			writer.WriteEndObject();
		}
	}
}
