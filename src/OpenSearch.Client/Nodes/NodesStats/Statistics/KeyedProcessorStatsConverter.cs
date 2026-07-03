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
	/// A <see cref="System.Text.Json"/> converter for <see cref="KeyedProcessorStats"/>, replacing the
	/// vendored Utf8Json <c>KeyedProcessorStatsFormatter</c> as part of #388. Shaped as a single-property
	/// object <c>{ "&lt;type&gt;": { …ProcessStats… } }</c>.
	/// </summary>
	internal sealed class KeyedProcessorStatsConverter : JsonConverter<KeyedProcessorStats>
	{
		public override KeyedProcessorStats Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.StartObject) return null;

			using var document = JsonDocument.ParseValue(ref reader);
			var stats = new KeyedProcessorStats();
			foreach (var member in document.RootElement.EnumerateObject())
			{
				stats.Type = member.Name;
				stats.Statistics = member.Value.Deserialize<ProcessStats>(options);
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
