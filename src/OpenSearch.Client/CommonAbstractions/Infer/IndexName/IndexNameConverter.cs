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
	/// A <see cref="System.Text.Json"/> converter for <see cref="IndexName"/>, replacing the vendored
	/// Utf8Json <c>IndexNameFormatter</c> as part of #388. Serializes the inferred index name as a JSON
	/// string; deserializes a JSON string into an <see cref="IndexName"/> (any other token yields null).
	/// Constructed with the connection settings for index-name inference.
	/// </summary>
	internal sealed class IndexNameConverter : JsonConverter<IndexName>
	{
		private readonly IConnectionSettingsValues _settings;

		public IndexNameConverter(IConnectionSettingsValues settings) =>
			_settings = settings ?? throw new ArgumentNullException(nameof(settings));

		public override void Write(Utf8JsonWriter writer, IndexName value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			var indexName = _settings.Inferrer.IndexName(value);
			writer.WriteStringValue(indexName);
		}

		public override IndexName Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.String)
			{
				IndexName indexName = reader.GetString();
				return indexName;
			}

			using (JsonDocument.ParseValue(ref reader)) { }
			return null;
		}
	}
}
