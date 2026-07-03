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
	/// A <see cref="System.Text.Json"/> converter for <see cref="RelationName"/> (join relation /
	/// type names), replacing the vendored Utf8Json <c>RelationNameFormatter</c> as part of #388.
	/// Written as the inferred relation-name string; constructed with the connection settings
	/// (decision D1). Also usable as an object key.
	/// </summary>
	internal sealed class RelationNameConverter : JsonConverter<RelationName>
	{
		private readonly IConnectionSettingsValues _settings;

		public RelationNameConverter(IConnectionSettingsValues settings) =>
			_settings = settings ?? throw new ArgumentNullException(nameof(settings));

		public override RelationName Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
			reader.TokenType == JsonTokenType.String ? reader.GetString() : null;

		public override void Write(Utf8JsonWriter writer, RelationName value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			writer.WriteStringValue(_settings.Inferrer.RelationName(value));
		}

		/// <inheritdoc />
		public override RelationName ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
			reader.GetString();

		/// <inheritdoc />
		public override void WriteAsPropertyName(Utf8JsonWriter writer, RelationName value, JsonSerializerOptions options) =>
			writer.WritePropertyName(_settings.Inferrer.RelationName(value));
	}
}
