/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Collections.Generic;
using System.Text.Json;

namespace OpenSearch.Client
{
	/// <summary>
	/// System.Text.Json replacement for the legacy Utf8Json <c>IndicesFormatter</c>. Serializes an
	/// <see cref="Indices"/> as a JSON array of index names, resolving each <see cref="IndexName"/> through the
	/// runtime <c>Inferrer</c> — hence a <see cref="SettingsAwareConverter{T}"/>.
	/// </summary>
	internal class IndicesConverter : SettingsAwareConverter<Indices>
	{
		public IndicesConverter(IConnectionSettingsValues settings) : base(settings) { }

		public override Indices Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			switch (reader.TokenType)
			{
				case JsonTokenType.StartArray:
				{
					var indices = new List<IndexName>();
					while (reader.Read())
					{
						if (reader.TokenType == JsonTokenType.EndArray)
							break;

						IndexName index = reader.GetString();
						indices.Add(index);
					}
					return new Indices(indices);
				}
				case JsonTokenType.String:
				{
					Indices indices = reader.GetString();
					return indices;
				}
				default:
					reader.Skip();
					return null;
			}
		}

		public override void Write(Utf8JsonWriter writer, Indices value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			switch (value.Tag)
			{
				case 0:
					writer.WriteStartArray();
					writer.WriteStringValue("_all");
					writer.WriteEndArray();
					break;
				case 1:
					writer.WriteStartArray();
					for (var index = 0; index < value.Item2.Indices.Count; index++)
					{
						var indexName = value.Item2.Indices[index];
						writer.WriteStringValue(Settings.Inferrer.IndexName(indexName));
					}
					writer.WriteEndArray();
					break;
			}
		}
	}
}
