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
	/// A <see cref="System.Text.Json"/> converter for <see cref="IGeoDistanceQuery"/>, replacing the
	/// vendored Utf8Json formatter as part of #388. Like the terms query it flattens <c>_name</c>,
	/// <c>boost</c>, <c>validation_method</c>, <c>distance</c> and <c>distance_type</c> alongside the
	/// field key whose value is the <see cref="GeoLocation"/>. Constructed with the connection
	/// settings for field-name inference.
	/// </summary>
	internal sealed class GeoDistanceQueryConverter : JsonConverter<IGeoDistanceQuery>
	{
		private readonly IConnectionSettingsValues _settings;

		public GeoDistanceQueryConverter(IConnectionSettingsValues settings) =>
			_settings = settings ?? throw new ArgumentNullException(nameof(settings));

		public override void Write(Utf8JsonWriter writer, IGeoDistanceQuery value, JsonSerializerOptions options)
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
			if (value.Distance != null)
			{
				writer.WritePropertyName("distance");
				JsonSerializer.Serialize(writer, value.Distance, options);
			}
			if (value.DistanceType.HasValue)
			{
				writer.WritePropertyName("distance_type");
				JsonSerializer.Serialize(writer, value.DistanceType.Value, options);
			}

			var field = value.Field == null ? null : _settings.Inferrer.Field(value.Field);
			if (!string.IsNullOrEmpty(field))
			{
				writer.WritePropertyName(field);
				JsonSerializer.Serialize(writer, value.Location, options);
			}

			writer.WriteEndObject();
		}

		public override IGeoDistanceQuery Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null) return null;

			using var document = JsonDocument.ParseValue(ref reader);
			var root = document.RootElement;
			if (root.ValueKind != JsonValueKind.Object) return null;

			var query = new GeoDistanceQuery();
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
					case "distance":
						query.Distance = member.Value.Deserialize<Distance>(options);
						break;
					case "distance_type":
						query.DistanceType = member.Value.Deserialize<GeoDistanceType>(options);
						break;
					default:
						query.Field = member.Name;
						query.Location = member.Value.Deserialize<GeoLocation>(options);
						break;
				}
			}

			return query;
		}
	}
}
