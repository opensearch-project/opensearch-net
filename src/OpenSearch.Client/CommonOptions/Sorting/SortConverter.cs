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
using OpenSearch.Net; // SortOrder enum

namespace OpenSearch.Client
{
	/// <summary>
	/// System.Text.Json replacement for the legacy Utf8Json <c>SortFormatter</c>.
	///
	/// An <see cref="ISort"/> is polymorphic in its wire shape:
	/// <list type="bullet">
	/// <item><description>a bare field string (<c>"field"</c>) is a <see cref="FieldSort"/> on that field;</description></item>
	/// <item><description>a single-key object <c>{ "&lt;field&gt;": { order, mode, missing, … } }</c> is a
	/// <see cref="FieldSort"/> (or <c>{ "&lt;field&gt;": "asc" }</c> short-form for just the order);</description></item>
	/// <item><description><c>{ "_script": { … } }</c> is a <see cref="ScriptSort"/>;</description></item>
	/// <item><description><c>{ "_geo_distance": { "&lt;field&gt;": [points], … } }</c> is a
	/// <see cref="GeoDistanceSort"/> whose field is the single array-valued property.</description></item>
	/// </list>
	/// The field-name property key is resolved through the runtime <c>Inferrer</c> on write, hence a
	/// <see cref="SettingsAwareConverter{T}"/>. On read the value is buffered into a <see cref="JsonDocument"/>
	/// (System.Text.Json's <see cref="Utf8JsonReader"/> is forward-only) and dispatched on the sort-key shape.
	/// </summary>
	internal class SortConverter : SettingsAwareConverter<ISort>
	{
		public SortConverter(IConnectionSettingsValues settings) : base(settings) { }

		public override ISort Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			switch (reader.TokenType)
			{
				case JsonTokenType.Null:
					return null;
				case JsonTokenType.String:
					return new FieldSort { Field = reader.GetString() };
				case JsonTokenType.StartObject:
					using (var doc = JsonDocument.ParseValue(ref reader))
						return ReadObject(doc.RootElement, options);
				default:
					throw new JsonException($"Cannot deserialize {nameof(ISort)} from {reader.TokenType}");
			}
		}

		private static ISort ReadObject(JsonElement root, JsonSerializerOptions options)
		{
			ISort sort = null;

			// Sort objects are single-key; iterate to mirror the legacy formatter (last property wins).
			foreach (var property in root.EnumerateObject())
			{
				var name = property.Name;
				var body = property.Value;

				switch (name)
				{
					case "_geo_distance":
						sort = ReadGeoDistance(body, options);
						break;
					case "_script":
						sort = body.Deserialize<ScriptSort>(options);
						break;
					default:
						sort = ReadFieldSort(name, body, options);
						break;
				}
			}

			return sort;
		}

		private static ISort ReadGeoDistance(JsonElement body, JsonSerializerOptions options)
		{
			string field = null;
			IEnumerable<GeoLocation> points = null;

			// The single array-valued property carries the field name and the geo points.
			foreach (var property in body.EnumerateObject())
			{
				if (property.Value.ValueKind == JsonValueKind.Array)
				{
					field = property.Name;
					points = property.Value.Deserialize<IEnumerable<GeoLocation>>(options);
					break;
				}
			}

			// The remaining properties (distance_type, unit, order, mode, …) bind directly to GeoDistanceSort.
			var geoDistanceSort = body.Deserialize<GeoDistanceSort>(options) ?? new GeoDistanceSort();
			geoDistanceSort.Field = field;
			geoDistanceSort.Points = points;
			return geoDistanceSort;
		}

		private static ISort ReadFieldSort(string field, JsonElement body, JsonSerializerOptions options)
		{
			if (body.ValueKind == JsonValueKind.String)
			{
				var sortOrder = body.Deserialize<SortOrder>(options);
				return new FieldSort { Field = field, Order = sortOrder };
			}

			var sortField = body.Deserialize<FieldSort>(options) ?? new FieldSort();
			sortField.Field = field;
			return sortField;
		}

		public override void Write(Utf8JsonWriter writer, ISort value, JsonSerializerOptions options)
		{
			if (value?.SortKey == null)
			{
				writer.WriteNullValue();
				return;
			}

			writer.WriteStartObject();
			switch (value.SortKey.Name ?? string.Empty)
			{
				case "_script":
					writer.WritePropertyName("_script");
					JsonSerializer.Serialize(writer, (IScriptSort)value, options);
					break;
				case "_geo_distance":
					var geo = (IGeoDistanceSort)value;
					writer.WritePropertyName(geo.SortKey.Name);
					writer.WriteStartObject();

					// Write the geo body members (distance_type, unit, order, mode, …) — but NOT the field/points,
					// which are re-attached as the field-keyed array below (mirrors the legacy formatter).
					using (var doc = SerializeToDocument(geo, typeof(IGeoDistanceSort), options))
					{
						foreach (var property in doc.RootElement.EnumerateObject())
							property.WriteTo(writer);
					}

					writer.WritePropertyName(Settings.Inferrer.Field(geo.Field));
					JsonSerializer.Serialize(writer, geo.Points, options);
					writer.WriteEndObject();
					break;
				default:
					writer.WritePropertyName(Settings.Inferrer.Field(value.SortKey));
					JsonSerializer.Serialize(writer, (IFieldSort)value, options);
					break;
			}
			writer.WriteEndObject();
		}

		private static JsonDocument SerializeToDocument(object value, Type type, JsonSerializerOptions options)
		{
			var bytes = JsonSerializer.SerializeToUtf8Bytes(value, type, options);
			return JsonDocument.Parse(new ReadOnlyMemory<byte>(bytes));
		}
	}
}
