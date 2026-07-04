/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenSearch.Client
{
	/// <summary>
	/// A <see cref="System.Text.Json"/> converter for <see cref="FieldValues"/> (a search hit's
	/// <c>fields</c>), replacing the vendored Utf8Json <c>FieldValuesFormatter</c> as part of #388. Reads
	/// the object into a map of <see cref="LazyDocument"/> values (deferred deserialization) keyed by
	/// field name, wrapped in a <see cref="FieldValues"/> that carries the settings inferrer.
	/// </summary>
	internal sealed class FieldValuesConverter : JsonConverter<FieldValues>
	{
		private readonly IConnectionSettingsValues _settings;

		public FieldValuesConverter(IConnectionSettingsValues settings) =>
			_settings = settings ?? throw new ArgumentNullException(nameof(settings));

		public override FieldValues Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.StartObject) { reader.Skip(); return null; }

			using var document = JsonDocument.ParseValue(ref reader);
			var fields = new Dictionary<string, LazyDocument>();
			foreach (var member in document.RootElement.EnumerateObject())
				fields[member.Name] = member.Value.Deserialize<LazyDocument>(options);

			return new FieldValues(_settings.Inferrer, fields);
		}

		public override void Write(Utf8JsonWriter writer, FieldValues value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			writer.WriteStartObject();
			foreach (var entry in value)
			{
				writer.WritePropertyName(entry.Key);
				JsonSerializer.Serialize(writer, entry.Value, options);
			}
			writer.WriteEndObject();
		}
	}
}
