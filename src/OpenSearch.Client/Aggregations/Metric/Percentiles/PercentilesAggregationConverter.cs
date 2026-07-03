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
	/// A <see cref="System.Text.Json"/> converter for <see cref="IPercentilesAggregation"/>, replacing
	/// the vendored Utf8Json <c>PercentilesAggregationFormatter</c> as part of #388. The
	/// <c>percents</c> array and the <c>tdigest</c>/<c>hdr</c> method are declared on the interface
	/// without <c>[DataMember]</c>, so the default resolver drops them; this converter writes the full
	/// shape explicitly.
	/// </summary>
	internal sealed class PercentilesAggregationConverter : JsonConverter<IPercentilesAggregation>
	{
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
				JsonSerializer.Serialize(writer, value.Field, options);
			}

			if (value.Script != null)
			{
				writer.WritePropertyName("script");
				JsonSerializer.Serialize(writer, value.Script, options);
			}

			PercentilesMethodConverter.Write(writer, value.Method);

			if (value.Missing.HasValue)
				writer.WriteNumber("missing", value.Missing.Value);

			if (value.Percents != null)
			{
				writer.WritePropertyName("percents");
				JsonSerializer.Serialize<IEnumerable<double>>(writer, value.Percents, options);
			}

			if (value.Keyed.HasValue)
				writer.WriteBoolean("keyed", value.Keyed.Value);

			if (!string.IsNullOrEmpty(value.Format))
				writer.WriteString("format", value.Format);

			writer.WriteEndObject();
		}

		public override IPercentilesAggregation Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.StartObject) { reader.Skip(); return null; }

			var agg = new PercentilesAggregation();

			while (reader.Read() && reader.TokenType == JsonTokenType.PropertyName)
			{
				var name = reader.GetString();
				reader.Read();
				switch (name)
				{
					case "hdr":
						agg.Method = JsonSerializer.Deserialize<HDRHistogramMethod>(ref reader, options);
						break;
					case "tdigest":
						agg.Method = JsonSerializer.Deserialize<TDigestMethod>(ref reader, options);
						break;
					case "field":
						agg.Field = JsonSerializer.Deserialize<Field>(ref reader, options);
						break;
					case "script":
						agg.Script = JsonSerializer.Deserialize<IScript>(ref reader, options);
						break;
					case "missing":
						agg.Missing = reader.GetDouble();
						break;
					case "percents":
						agg.Percents = JsonSerializer.Deserialize<IEnumerable<double>>(ref reader, options);
						break;
					case "meta":
						agg.Meta = JsonSerializer.Deserialize<IDictionary<string, object>>(ref reader, options);
						break;
					case "keyed":
						agg.Keyed = reader.GetBoolean();
						break;
					case "format":
						agg.Format = reader.GetString();
						break;
					default:
						reader.Skip();
						break;
				}
			}

			return agg;
		}
	}
}
