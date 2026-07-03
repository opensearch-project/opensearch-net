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
	/// A <see cref="System.Text.Json"/> converter for <see cref="ICompositeAggregationSource"/>,
	/// replacing the vendored Utf8Json <c>CompositeAggregationSourceFormatter</c> as part of #388. Each
	/// source is wrapped as <c>{ "&lt;name&gt;": { "&lt;source_type&gt;": { … } } }</c>; the inner body
	/// is serialized through the concrete runtime type (its <c>[DataMember]</c> shape), which this
	/// converter does not intercept, avoiding recursion.
	/// </summary>
	internal sealed class CompositeAggregationSourceConverter : JsonConverter<ICompositeAggregationSource>
	{
		public override void Write(Utf8JsonWriter writer, ICompositeAggregationSource value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			writer.WriteStartObject();
			writer.WritePropertyName(value.Name);
			writer.WriteStartObject();
			writer.WritePropertyName(value.SourceType);
			JsonSerializer.Serialize(writer, value, value.GetType(), options);
			writer.WriteEndObject();
			writer.WriteEndObject();
		}

		public override ICompositeAggregationSource Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.StartObject) return null;

			reader.Read();
			var name = reader.GetString();
			reader.Read(); // into source object
			reader.Read(); // source type property name
			var sourceType = reader.GetString();
			reader.Read(); // into source body

			ICompositeAggregationSource source = sourceType switch
			{
				"terms" => JsonSerializer.Deserialize<TermsCompositeAggregationSource>(ref reader, options),
				"date_histogram" => JsonSerializer.Deserialize<DateHistogramCompositeAggregationSource>(ref reader, options),
				"histogram" => JsonSerializer.Deserialize<HistogramCompositeAggregationSource>(ref reader, options),
				"geotile_grid" => JsonSerializer.Deserialize<GeoTileGridCompositeAggregationSource>(ref reader, options),
				_ => throw new JsonException($"Unknown {nameof(ICompositeAggregationSource)}: {sourceType}")
			};

			reader.Read(); // end source object
			reader.Read(); // end wrapper object

			if (source != null)
				source.Name = name;
			return source;
		}
	}
}
