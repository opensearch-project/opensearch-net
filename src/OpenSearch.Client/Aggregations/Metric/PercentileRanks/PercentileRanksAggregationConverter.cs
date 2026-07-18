/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/
/*
* Modifications Copyright OpenSearch Contributors. See
* GitHub history for details.
*
*  Licensed to Elasticsearch B.V. under one or more contributor
*  license agreements. See the NOTICE file distributed with
*  this work for additional information regarding copyright
*  ownership. Elasticsearch B.V. licenses this file to you under
*  the Apache License, Version 2.0 (the "License"); you may
*  not use this file except in compliance with the License.
*  You may obtain a copy of the License at
*
* 	http://www.apache.org/licenses/LICENSE-2.0
*
*  Unless required by applicable law or agreed to in writing,
*  software distributed under the License is distributed on an
*  "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
*  KIND, either express or implied.  See the License for the
*  specific language governing permissions and limitations
*  under the License.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenSearch.Client
{
	/// <summary>
	/// System.Text.Json replacement for the legacy Utf8Json <c>PercentileRanksAggregationFormatter</c>.
	///
	/// Structurally identical to <see cref="PercentilesAggregationConverter"/> except the values list is the
	/// <c>values</c> property (mapped to <see cref="IPercentileRanksAggregation.Values"/>) and — matching the legacy
	/// formatter — the values are only written when the collection is non-empty. Read maps each known property by name
	/// (the polymorphic <see cref="IPercentilesMethod"/> selected by <c>hdr</c>/<c>tdigest</c>) and ignores unknown
	/// ones. Write emits, in the legacy order, <c>meta</c>, <c>field</c> (resolved through the runtime
	/// <c>Inferrer</c>), <c>script</c>, the method object, <c>missing</c>, <c>values</c>, <c>keyed</c>, <c>format</c>.
	///
	/// The legacy formatter resolved the field name via <c>formatterResolver.GetConnectionSettings().Inferrer</c>, so
	/// this converter is settings-aware.
	/// </summary>
	internal class PercentileRanksAggregationConverter : SettingsAwareConverter<IPercentileRanksAggregation>
	{
		public PercentileRanksAggregationConverter(IConnectionSettingsValues settings) : base(settings) { }

		public override IPercentileRanksAggregation Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.StartObject)
			{
				reader.Skip();
				return null;
			}

			using var doc = JsonDocument.ParseValue(ref reader);
			var root = doc.RootElement;

			var percentiles = new PercentileRanksAggregation();

			foreach (var property in root.EnumerateObject())
			{
				switch (property.Name)
				{
					case "hdr":
						percentiles.Method = JsonSerializer.Deserialize<HDRHistogramMethod>(property.Value.GetRawText(), options);
						break;
					case "tdigest":
						percentiles.Method = JsonSerializer.Deserialize<TDigestMethod>(property.Value.GetRawText(), options);
						break;
					case "field":
						percentiles.Field = property.Value.ValueKind == JsonValueKind.String ? property.Value.GetString() : null;
						break;
					case "script":
						percentiles.Script = JsonSerializer.Deserialize<IScript>(property.Value.GetRawText(), options);
						break;
					case "missing":
						if (property.Value.ValueKind == JsonValueKind.Number)
							percentiles.Missing = property.Value.GetDouble();
						break;
					case "meta":
						percentiles.Meta = JsonSerializer.Deserialize<Dictionary<string, object>>(property.Value.GetRawText(), options);
						break;
					case "values":
						percentiles.Values = JsonSerializer.Deserialize<List<double>>(property.Value.GetRawText(), options);
						break;
					case "keyed":
						if (property.Value.ValueKind == JsonValueKind.True || property.Value.ValueKind == JsonValueKind.False)
							percentiles.Keyed = property.Value.GetBoolean();
						break;
					case "format":
						percentiles.Format = property.Value.ValueKind == JsonValueKind.String ? property.Value.GetString() : null;
						break;
				}
			}

			return percentiles;
		}

		public override void Write(Utf8JsonWriter writer, IPercentileRanksAggregation value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			writer.WriteStartObject();

			if (value.Meta != null && value.Meta.Any())
			{
				writer.WritePropertyName("meta");
				JsonSerializer.Serialize(writer, value.Meta, options);
			}

			if (value.Field != null)
			{
				writer.WritePropertyName("field");
				writer.WriteStringValue(Settings.Inferrer.Field(value.Field));
			}

			if (value.Script != null)
			{
				writer.WritePropertyName("script");
				JsonSerializer.Serialize(writer, value.Script, options);
			}

			PercentilesMethodConverterHelper.WriteMethod(writer, value.Method);

			if (value.Missing.HasValue)
			{
				writer.WritePropertyName("missing");
				writer.WriteNumberValue(value.Missing.Value);
			}

			if (value.Values != null && value.Values.Any())
			{
				writer.WritePropertyName("values");
				JsonSerializer.Serialize(writer, value.Values, options);
			}

			if (value.Keyed.HasValue)
			{
				writer.WritePropertyName("keyed");
				writer.WriteBooleanValue(value.Keyed.Value);
			}

			if (!string.IsNullOrEmpty(value.Format))
			{
				writer.WritePropertyName("format");
				writer.WriteStringValue(value.Format);
			}

			writer.WriteEndObject();
		}
	}
}
