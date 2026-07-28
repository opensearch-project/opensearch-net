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
