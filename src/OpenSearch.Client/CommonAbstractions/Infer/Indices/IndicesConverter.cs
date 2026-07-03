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
using OpenSearch.Net;

namespace OpenSearch.Client
{
	/// <summary>
	/// A <see cref="System.Text.Json"/> converter for <see cref="Indices"/>, replacing the vendored
	/// Utf8Json <c>IndicesMultiSyntaxFormatter</c> (the type-level default) as part of #388. Writes the
	/// multi-index syntax as a single string (<c>"_all"</c> for all-indices, otherwise the
	/// comma-joined inferred index names); reads a single string, and leniently also a JSON array of
	/// strings (any other token yields null). Constructed with the connection settings for index-name
	/// inference.
	/// </summary>
	internal sealed class IndicesConverter : JsonConverter<Indices>
	{
		private readonly IConnectionSettingsValues _settings;

		public IndicesConverter(IConnectionSettingsValues settings) =>
			_settings = settings ?? throw new ArgumentNullException(nameof(settings));

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
					writer.WriteStringValue("_all");
					break;
				case 1:
					writer.WriteStringValue(((IUrlParameter)value).GetString(_settings));
					break;
			}
		}

		public override Indices Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			switch (reader.TokenType)
			{
				case JsonTokenType.StartArray:
				{
					var indices = new List<IndexName>();
					while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
					{
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
					using (JsonDocument.ParseValue(ref reader)) { }
					return null;
			}
		}
	}
}
