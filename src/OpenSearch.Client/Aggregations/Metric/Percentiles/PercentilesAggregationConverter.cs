/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenSearch.Client
{
	/// <summary>
	/// System.Text.Json replacement for the legacy Utf8Json <c>PercentilesAggregationFormatter</c>.
	///
	/// An <see cref="IPercentilesAggregation"/> carries a polymorphic <see cref="IPercentilesMethod"/> (a
	/// <c>tdigest</c> or <c>hdr</c> object) alongside the metric-aggregation fields (<c>field</c>, <c>script</c>,
	/// <c>missing</c>, <c>meta</c>, <c>format</c>) plus <c>percents</c> and <c>keyed</c>. Read maps each known property
	/// by name (the method being selected by whether <c>hdr</c> or <c>tdigest</c> is present) and ignores unknown ones,
	/// mirroring the legacy automata dispatch. Write emits, in the legacy order, <c>meta</c>, <c>field</c> (resolved
	/// through the runtime <c>Inferrer</c>), <c>script</c>, the method object, <c>missing</c>, <c>percents</c>,
	/// <c>keyed</c>, <c>format</c>.
	///
	/// The legacy formatter resolved the field name via <c>formatterResolver.GetConnectionSettings().Inferrer</c>, so
	/// this converter is settings-aware.
	/// </summary>
	internal class PercentilesAggregationConverter : SettingsAwareConverter<IPercentilesAggregation>
	{
		public PercentilesAggregationConverter(IConnectionSettingsValues settings) : base(settings) { }

		public override IPercentilesAggregation Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.StartObject)
			{
				reader.Skip();
				return null;
			}

			using var doc = JsonDocument.ParseValue(ref reader);
			var root = doc.RootElement;

			var percentiles = new PercentilesAggregation();

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
					case "percents":
						percentiles.Percents = JsonSerializer.Deserialize<List<double>>(property.Value.GetRawText(), options);
						break;
					case "meta":
						percentiles.Meta = JsonSerializer.Deserialize<Dictionary<string, object>>(property.Value.GetRawText(), options);
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

		public override void Write(Utf8JsonWriter writer, IPercentilesAggregation value, JsonSerializerOptions options)
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
				OpenSearch.Net.Serialization.Converters.RealNumberFormat.WriteDouble(writer, value.Missing.Value);
			}

			if (value.Percents != null)
			{
				writer.WritePropertyName("percents");
				JsonSerializer.Serialize(writer, value.Percents, options);
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

	/// <summary>
	/// Shared writer for the polymorphic <see cref="IPercentilesMethod"/> object used by both
	/// <see cref="PercentilesAggregationConverter"/> and <see cref="PercentileRanksAggregationConverter"/>. Emits
	/// <c>tdigest</c>/<c>hdr</c> with only the set inner value, exactly as the legacy formatters did (and nothing when
	/// the method is null).
	/// </summary>
	internal static class PercentilesMethodConverterHelper
	{
		public static void WriteMethod(Utf8JsonWriter writer, IPercentilesMethod method)
		{
			switch (method)
			{
				case ITDigestMethod tdigest:
					writer.WritePropertyName("tdigest");
					writer.WriteStartObject();
					if (tdigest.Compression.HasValue)
					{
						writer.WritePropertyName("compression");
						OpenSearch.Net.Serialization.Converters.RealNumberFormat.WriteDouble(writer, tdigest.Compression.Value);
					}
					writer.WriteEndObject();
					break;
				case IHDRHistogramMethod hdr:
					writer.WritePropertyName("hdr");
					writer.WriteStartObject();
					if (hdr.NumberOfSignificantValueDigits.HasValue)
					{
						writer.WritePropertyName("number_of_significant_value_digits");
						writer.WriteNumberValue(hdr.NumberOfSignificantValueDigits.Value);
					}
					writer.WriteEndObject();
					break;
			}
		}
	}
}
