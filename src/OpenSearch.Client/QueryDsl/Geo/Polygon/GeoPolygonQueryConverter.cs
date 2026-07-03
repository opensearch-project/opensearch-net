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
	/// A <see cref="System.Text.Json"/> converter for <see cref="IGeoPolygonQuery"/>, replacing the
	/// vendored Utf8Json formatter as part of #388. Flattens <c>_name</c>/<c>boost</c>/
	/// <c>validation_method</c> alongside the field key whose body is <c>{ "points": [ … ] }</c>.
	/// Constructed with the connection settings for field-name inference.
	/// </summary>
	internal sealed class GeoPolygonQueryConverter : JsonConverter<IGeoPolygonQuery>
	{
		private readonly IConnectionSettingsValues _settings;

		public GeoPolygonQueryConverter(IConnectionSettingsValues settings) =>
			_settings = settings ?? throw new ArgumentNullException(nameof(settings));

		public override void Write(Utf8JsonWriter writer, IGeoPolygonQuery value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			writer.WriteStartObject();

			if (!string.IsNullOrEmpty(value.Name))
				writer.WriteString("_name", value.Name);
			if (value.Boost.HasValue)
			{
				writer.WritePropertyName("boost");
				JsonSerializer.Serialize(writer, value.Boost.Value, options);
			}
			if (value.ValidationMethod.HasValue)
			{
				writer.WritePropertyName("validation_method");
				JsonSerializer.Serialize(writer, value.ValidationMethod.Value, options);
			}

			var field = value.Field == null ? null : _settings.Inferrer.Field(value.Field);
			if (!string.IsNullOrEmpty(field))
			{
				writer.WritePropertyName(field);
				writer.WriteStartObject();
				writer.WritePropertyName("points");
				JsonSerializer.Serialize(writer, value.Points, options);
				writer.WriteEndObject();
			}

			writer.WriteEndObject();
		}

		public override IGeoPolygonQuery Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null) return null;

			using var document = JsonDocument.ParseValue(ref reader);
			var root = document.RootElement;
			if (root.ValueKind != JsonValueKind.Object) return null;

			var query = new GeoPolygonQuery();
			foreach (var member in root.EnumerateObject())
			{
				switch (member.Name)
				{
					case "_name":
						query.Name = member.Value.GetString();
						break;
					case "boost":
						query.Boost = member.Value.GetDouble();
						break;
					case "validation_method":
						query.ValidationMethod = member.Value.Deserialize<GeoValidationMethod>(options);
						break;
					default:
						query.Field = member.Name;
						if (member.Value.ValueKind == JsonValueKind.Object
							&& member.Value.TryGetProperty("points", out var points))
							query.Points = points.Deserialize<List<GeoLocation>>(options);
						break;
				}
			}

			return query;
		}
	}
}
