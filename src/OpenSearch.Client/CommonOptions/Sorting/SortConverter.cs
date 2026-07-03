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
	/// A <see cref="System.Text.Json"/> converter for <see cref="ISort"/>, replacing the vendored
	/// Utf8Json <c>SortFormatter</c> as part of #388. Handles the four sort shapes: a bare string
	/// (a <see cref="FieldSort"/>), <c>_geo_distance</c> (a <see cref="GeoDistanceSort"/>),
	/// <c>_script</c> (a <see cref="ScriptSort"/>), and <c>field:order</c> / <c>field:{ … }</c>
	/// (a <see cref="FieldSort"/>). Constructed with the connection settings for field-name inference
	/// on write.
	/// </summary>
	internal sealed class SortConverter : JsonConverter<ISort>
	{
		private readonly IConnectionSettingsValues _settings;

		public SortConverter(IConnectionSettingsValues settings) =>
			_settings = settings ?? throw new ArgumentNullException(nameof(settings));

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

					// The Utf8Json formatter merged the serialized IGeoDistanceSort object with an
					// extra "<field>": [points] member into a single object. Re-serialize the geo
					// distance sort (Field/Points are not [DataMember] so they are excluded by the
					// data-contract resolver) and splice its members in, then append field + points.
					var geoJson = JsonSerializer.Serialize((IGeoDistanceSort)geo, options);
					using (var geoDocument = JsonDocument.Parse(geoJson))
					{
						foreach (var member in geoDocument.RootElement.EnumerateObject())
							member.WriteTo(writer);
					}

					writer.WritePropertyName(_settings.Inferrer.Field(geo.Field));
					JsonSerializer.Serialize(writer, geo.Points, options);
					writer.WriteEndObject();
					break;
				default:
					writer.WritePropertyName(_settings.Inferrer.Field(value.SortKey));
					JsonSerializer.Serialize(writer, (IFieldSort)value, options);
					break;
			}
			writer.WriteEndObject();
		}

		public override ISort Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			switch (reader.TokenType)
			{
				case JsonTokenType.Null:
					return null;
				case JsonTokenType.String:
					return new FieldSort { Field = reader.GetString() };
				case JsonTokenType.StartObject:
				{
					using var document = JsonDocument.ParseValue(ref reader);
					var root = document.RootElement;

					ISort sort = null;
					foreach (var member in root.EnumerateObject())
					{
						switch (member.Name)
						{
							case "_geo_distance":
								sort = ReadGeoDistanceSort(member.Value, options);
								break;
							case "_script":
								sort = member.Value.Deserialize<ScriptSort>(options);
								break;
							default:
								sort = ReadFieldSort(member.Name, member.Value, options);
								break;
						}
					}

					return sort;
				}
				default:
					throw new JsonException($"Cannot deserialize {nameof(ISort)} from token {reader.TokenType}.");
			}
		}

		private static ISort ReadGeoDistanceSort(JsonElement element, JsonSerializerOptions options)
		{
			var geoDistanceSort = element.Deserialize<GeoDistanceSort>(options) ?? new GeoDistanceSort();

			string field = null;
			IEnumerable<GeoLocation> points = null;
			if (element.ValueKind == JsonValueKind.Object)
			{
				foreach (var member in element.EnumerateObject())
				{
					if (member.Value.ValueKind != JsonValueKind.Array) continue;

					field = member.Name;
					points = member.Value.Deserialize<List<GeoLocation>>(options);
					break;
				}
			}

			geoDistanceSort.Field = field;
			geoDistanceSort.Points = points;
			return geoDistanceSort;
		}

		private static ISort ReadFieldSort(string field, JsonElement element, JsonSerializerOptions options)
		{
			if (element.ValueKind == JsonValueKind.String)
			{
				var order = element.Deserialize<SortOrder>(options);
				return new FieldSort { Field = field, Order = order };
			}

			var sortField = element.Deserialize<FieldSort>(options) ?? new FieldSort();
			sortField.Field = field;
			return sortField;
		}
	}
}
