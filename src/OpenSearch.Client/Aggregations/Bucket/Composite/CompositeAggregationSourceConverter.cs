/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using OpenSearch.Net; // SortOrder enum

namespace OpenSearch.Client
{
	/// <summary>
	/// System.Text.Json replacement for the legacy Utf8Json <c>CompositeAggregationSourceFormatter</c>.
	///
	/// An <see cref="ICompositeAggregationSource"/> is written as a doubly-nested object keyed first by the source
	/// <see cref="ICompositeAggregationSource.Name"/> and then by its <see cref="ICompositeAggregationSource.SourceType"/>
	/// (<c>terms</c> / <c>date_histogram</c> / <c>histogram</c> / <c>geotile_grid</c>), whose body is the source's own
	/// members: <c>{ "&lt;name&gt;": { "&lt;source_type&gt;": { &lt;body&gt; } } }</c>. The body is produced by
	/// serializing the value as its concrete source INTERFACE, reproducing the legacy formatter which delegated to the
	/// exclude-null camelCase dynamic resolver (all wire members carry explicit snake_case <c>[DataMember]</c> names, so
	/// the contract resolver yields the identical shape). Field-name inference on <c>field</c> is handled by the
	/// registered <c>FieldConverter</c>, exactly as the legacy engine delegated to the Field formatter — so this
	/// converter itself is not settings-aware.
	///
	/// <para>On read the two wrapper objects are unwound: the outer property name is the source name, the inner
	/// property name is the source type used to dispatch to the concrete type, and the inner value is the body. Because
	/// the concrete source types expose only a <c>(string name)</c> constructor (no parameterless constructor for
	/// System.Text.Json to invoke), each concrete instance is constructed explicitly and its body members populated
	/// from the buffered DOM.</para>
	///
	/// An unknown source type throws (mirroring the legacy formatter's exception); a non-object root yields
	/// <c>null</c>.
	/// </summary>
	internal class CompositeAggregationSourceConverter : JsonConverter<ICompositeAggregationSource>
	{
		public override ICompositeAggregationSource Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.StartObject)
			{
				reader.Skip();
				return null;
			}

			using var doc = JsonDocument.ParseValue(ref reader);
			var root = doc.RootElement;

			string name = null;
			var outer = default(JsonElement);
			foreach (var p in root.EnumerateObject())
			{
				name = p.Name;
				outer = p.Value;
				break;
			}

			if (name == null || outer.ValueKind != JsonValueKind.Object)
				return null;

			string sourceType = null;
			var body = default(JsonElement);
			foreach (var p in outer.EnumerateObject())
			{
				sourceType = p.Name;
				body = p.Value;
				break;
			}

			ICompositeAggregationSource source;
			switch (sourceType)
			{
				case "terms":
					source = ReadTerms(name, body, options);
					break;
				case "date_histogram":
					source = ReadDateHistogram(name, body, options);
					break;
				case "histogram":
					source = ReadHistogram(name, body, options);
					break;
				case "geotile_grid":
					source = ReadGeoTileGrid(name, body, options);
					break;
				default:
					throw new JsonException($"Unknown {nameof(ICompositeAggregationSource)}: {sourceType}");
			}

			source.Name = name;
			return source;
		}

		private static TermsCompositeAggregationSource ReadTerms(string name, JsonElement body, JsonSerializerOptions options)
		{
			var source = new TermsCompositeAggregationSource(name);
			ReadCommon(source, body, options);
			if (body.TryGetProperty("script", out var script))
				source.Script = JsonSerializer.Deserialize<IScript>(script.GetRawText(), options);
			return source;
		}

		private static HistogramCompositeAggregationSource ReadHistogram(string name, JsonElement body, JsonSerializerOptions options)
		{
			var source = new HistogramCompositeAggregationSource(name);
			ReadCommon(source, body, options);
			if (body.TryGetProperty("interval", out var interval) && interval.ValueKind == JsonValueKind.Number)
				source.Interval = interval.GetDouble();
			if (body.TryGetProperty("script", out var script))
				source.Script = JsonSerializer.Deserialize<IScript>(script.GetRawText(), options);
			return source;
		}

		private static DateHistogramCompositeAggregationSource ReadDateHistogram(string name, JsonElement body, JsonSerializerOptions options)
		{
			var source = new DateHistogramCompositeAggregationSource(name);
			ReadCommon(source, body, options);
			if (body.TryGetProperty("format", out var format) && format.ValueKind == JsonValueKind.String)
				source.Format = format.GetString();
			if (body.TryGetProperty("calendar_interval", out var calendar))
				source.CalendarInterval = JsonSerializer.Deserialize<Union<DateInterval?, DateMathTime>>(calendar.GetRawText(), options);
			if (body.TryGetProperty("fixed_interval", out var fixedInterval))
				source.FixedInterval = JsonSerializer.Deserialize<Time>(fixedInterval.GetRawText(), options);
			if (body.TryGetProperty("time_zone", out var tz) && tz.ValueKind == JsonValueKind.String)
				source.TimeZone = tz.GetString();
			return source;
		}

		private static GeoTileGridCompositeAggregationSource ReadGeoTileGrid(string name, JsonElement body, JsonSerializerOptions options)
		{
			var source = new GeoTileGridCompositeAggregationSource(name);
			ReadCommon(source, body, options);
			if (body.TryGetProperty("precision", out var precision) && precision.ValueKind == JsonValueKind.Number)
				source.Precision = (GeoTilePrecision)precision.GetInt32();
			return source;
		}

		// field / missing_bucket / order are the members shared by every source (ICompositeAggregationSource).
		private static void ReadCommon(ICompositeAggregationSource source, JsonElement body, JsonSerializerOptions options)
		{
			if (body.TryGetProperty("field", out var field) && field.ValueKind == JsonValueKind.String)
				source.Field = field.GetString();
			if (body.TryGetProperty("missing_bucket", out var missing) &&
				(missing.ValueKind == JsonValueKind.True || missing.ValueKind == JsonValueKind.False))
				source.MissingBucket = missing.GetBoolean();
			if (body.TryGetProperty("order", out var order))
				source.Order = JsonSerializer.Deserialize<SortOrder?>(order.GetRawText(), options);
		}

		public override void Write(Utf8JsonWriter writer, ICompositeAggregationSource value, JsonSerializerOptions options)
		{
			writer.WriteStartObject();
			writer.WritePropertyName(value.Name);
			writer.WriteStartObject();
			writer.WritePropertyName(value.SourceType);

			// Serialize by the concrete source INTERFACE so the [DataMember]-annotated members (incl. the Inferrer-
			// resolved field via the registered FieldConverter) are emitted, matching the legacy exclude-null dynamic
			// resolver. Dispatching on a different type than ICompositeAggregationSource avoids re-entering this
			// converter.
			switch (value)
			{
				case ITermsCompositeAggregationSource terms:
					JsonSerializer.Serialize(writer, terms, typeof(ITermsCompositeAggregationSource), options);
					break;
				case IDateHistogramCompositeAggregationSource dateHistogram:
					JsonSerializer.Serialize(writer, dateHistogram, typeof(IDateHistogramCompositeAggregationSource), options);
					break;
				case IHistogramCompositeAggregationSource histogram:
					JsonSerializer.Serialize(writer, histogram, typeof(IHistogramCompositeAggregationSource), options);
					break;
				case IGeoTileGridCompositeAggregationSource geoTileGrid:
					JsonSerializer.Serialize(writer, geoTileGrid, typeof(IGeoTileGridCompositeAggregationSource), options);
					break;
				default:
					JsonSerializer.Serialize(writer, value, value.GetType(), options);
					break;
			}

			writer.WriteEndObject();
			writer.WriteEndObject();
		}
	}
}
