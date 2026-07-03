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
	/// A <see cref="System.Text.Json"/> converter for <see cref="IGeoBoundingBoxQuery"/>, replacing the
	/// vendored Utf8Json <c>GeoBoundingBoxQueryFormatter</c> as part of #388. Flattens <c>_name</c>,
	/// <c>boost</c>, <c>validation_method</c> and <c>type</c> alongside the field key whose value is the
	/// <see cref="IBoundingBox"/>. Constructed with the connection settings for field-name inference.
	/// </summary>
	internal sealed class GeoBoundingBoxQueryConverter : JsonConverter<IGeoBoundingBoxQuery>
	{
		private readonly IConnectionSettingsValues _settings;

		public GeoBoundingBoxQueryConverter(IConnectionSettingsValues settings) =>
			_settings = settings ?? throw new ArgumentNullException(nameof(settings));

		public override void Write(Utf8JsonWriter writer, IGeoBoundingBoxQuery value, JsonSerializerOptions options)
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
			if (value.Type.HasValue)
			{
				writer.WritePropertyName("type");
				JsonSerializer.Serialize(writer, value.Type.Value, options);
			}

			var field = value.Field == null ? null : _settings.Inferrer.Field(value.Field);
			if (!string.IsNullOrEmpty(field))
			{
				writer.WritePropertyName(field);
				JsonSerializer.Serialize(writer, value.BoundingBox, options);
			}

			writer.WriteEndObject();
		}

		public override IGeoBoundingBoxQuery Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null) return null;

			using var document = JsonDocument.ParseValue(ref reader);
			var root = document.RootElement;
			if (root.ValueKind != JsonValueKind.Object) return null;

			var query = new GeoBoundingBoxQuery();
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
					case "type":
						query.Type = member.Value.Deserialize<GeoExecution>(options);
						break;
					default:
						query.Field = member.Name;
						query.BoundingBox = member.Value.Deserialize<IBoundingBox>(options);
						break;
				}
			}

			return query;
		}
	}
}
