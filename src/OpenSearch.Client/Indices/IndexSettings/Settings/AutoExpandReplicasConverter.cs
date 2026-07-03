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
	/// A <see cref="System.Text.Json"/> converter for <see cref="AutoExpandReplicas"/>, replacing the
	/// vendored Utf8Json <c>AutoExpandReplicasFormatter</c> as part of #388. A disabled value is written
	/// as the boolean <c>false</c>; an enabled value is written as its string form (e.g. <c>"0-5"</c>).
	/// On read, a boolean <c>false</c> maps to <see cref="AutoExpandReplicas.Disabled"/> and a string is
	/// parsed via <see cref="AutoExpandReplicas.Create(string)"/>.
	/// </summary>
	internal sealed class AutoExpandReplicasConverter : JsonConverter<AutoExpandReplicas>
	{
		public override void Write(Utf8JsonWriter writer, AutoExpandReplicas value, JsonSerializerOptions options)
		{
			if (value == null || !value.Enabled)
			{
				writer.WriteBooleanValue(false);
				return;
			}

			writer.WriteStringValue(value.ToString());
		}

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
	}
}
