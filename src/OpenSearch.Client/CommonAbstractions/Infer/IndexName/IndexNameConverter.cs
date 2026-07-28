/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Text.Json;

namespace OpenSearch.Client
{
	/// <summary>
	/// System.Text.Json replacement for the legacy Utf8Json <c>IndexNameFormatter</c>. Serializes an
	/// <see cref="IndexName"/> as a JSON string, resolving it through the runtime <c>Inferrer</c> — hence a
	/// <see cref="SettingsAwareConverter{T}"/>.
	/// </summary>
	internal class IndexNameConverter : SettingsAwareConverter<IndexName>
	{
		public IndexNameConverter(IConnectionSettingsValues settings) : base(settings) { }

		public override IndexName Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.String)
			{
				reader.Skip();
				return null;
			}

			IndexName indexName = reader.GetString();
			return indexName;
		}

		public override void Write(Utf8JsonWriter writer, IndexName value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			var indexName = Settings.Inferrer.IndexName(value);
			writer.WriteStringValue(indexName);
		}

		// IndexName is used as a dictionary key (e.g. indices_boost, get-alias/mapping responses). STJ requires these
		// overrides to (de)serialize the type as a property name; without them it throws NotSupportedException.
		public override IndexName ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			IndexName indexName = reader.GetString();
			return indexName;
		}

		public override void WriteAsPropertyName(Utf8JsonWriter writer, IndexName value, JsonSerializerOptions options) =>
			writer.WritePropertyName(value == null ? string.Empty : Settings.Inferrer.IndexName(value));
	}
}
