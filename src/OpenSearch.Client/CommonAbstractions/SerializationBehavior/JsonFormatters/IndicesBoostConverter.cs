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
	/// A <see cref="System.Text.Json"/> converter for <c>IDictionary&lt;IndexName, double&gt;</c>, replacing the
	/// vendored Utf8Json <c>IndicesBoostFormatter</c> as part of #388. Writes an array of single-property objects
	/// (<c>[ { "&lt;index&gt;": &lt;boost&gt; }, … ]</c>) with index names resolved via the connection settings
	/// inferrer, and reads back either that array form or a plain object (<c>{ "&lt;index&gt;": &lt;boost&gt; }</c>).
	/// Constructed with the connection settings for index-name inference.
	/// </summary>
	internal sealed class IndicesBoostConverter : JsonConverter<IDictionary<IndexName, double>>
	{
		private readonly IConnectionSettingsValues _settings;

		public IndicesBoostConverter(IConnectionSettingsValues settings) =>
			_settings = settings ?? throw new ArgumentNullException(nameof(settings));

		public override void Write(Utf8JsonWriter writer, IDictionary<IndexName, double> value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			writer.WriteStartArray();
			foreach (var entry in value)
			{
				writer.WriteStartObject();
				var indexName = _settings.Inferrer.IndexName(entry.Key);
				writer.WritePropertyName(indexName);
				// Route through the global double converter so integral boosts keep their trailing ".0".
				JsonSerializer.Serialize(writer, entry.Value, options);
				writer.WriteEndObject();
			}
			writer.WriteEndArray();
		}

		public override IDictionary<IndexName, double> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null) return null;

			using var document = JsonDocument.ParseValue(ref reader);
			var root = document.RootElement;

			switch (root.ValueKind)
			{
				case JsonValueKind.Object:
				{
					var dictionary = new Dictionary<IndexName, double>();
					foreach (var member in root.EnumerateObject())
						dictionary.Add(member.Name, member.Value.GetDouble());
					return dictionary;
				}
				case JsonValueKind.Array:
				{
					var dictionary = new Dictionary<IndexName, double>();
					foreach (var element in root.EnumerateArray())
					{
						if (element.ValueKind != JsonValueKind.Object) continue;
						foreach (var member in element.EnumerateObject())
							dictionary.Add(member.Name, member.Value.GetDouble());
					}
					return dictionary;
				}
				default:
					return null;
			}
		}
	}
}
