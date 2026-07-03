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
	/// A <see cref="System.Text.Json"/> converter for <see cref="IShapeQuery"/>, replacing the vendored
	/// Utf8Json <c>CompositeFormatter&lt;…, ShapeQueryFormatter, ShapeQueryFieldNameFormatter&gt;</c> as
	/// part of #388. Field-name-keyed like <see cref="GeoShapeQueryConverter"/>, but with a
	/// <see cref="ShapeRelation"/> spatial relation.
	/// </summary>
	internal sealed class ShapeQueryConverter : JsonConverter<IShapeQuery>
	{
		private readonly IConnectionSettingsValues _settings;

		public ShapeQueryConverter(IConnectionSettingsValues settings) =>
			_settings = settings ?? throw new ArgumentNullException(nameof(settings));

		public override void Write(Utf8JsonWriter writer, IShapeQuery value, JsonSerializerOptions options)
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
			if (value.IgnoreUnmapped.HasValue)
				writer.WriteBoolean("ignore_unmapped", value.IgnoreUnmapped.Value);

			var field = value.Field == null ? null : _settings.Inferrer.Field(value.Field);
			if (!string.IsNullOrEmpty(field))
			{
				writer.WritePropertyName(field);
				writer.WriteStartObject();

				if (value.Shape != null)
				{
					writer.WritePropertyName("shape");
					JsonSerializer.Serialize(writer, value.Shape, options);
				}
				else if (value.IndexedShape != null)
				{
					writer.WritePropertyName("indexed_shape");
					JsonSerializer.Serialize(writer, value.IndexedShape, options);
				}

				if (value.Relation.HasValue)
				{
					writer.WritePropertyName("relation");
					JsonSerializer.Serialize(writer, value.Relation.Value, options);
				}

				writer.WriteEndObject();
			}

			writer.WriteEndObject();
		}

		public override IShapeQuery Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null) return null;

			using var document = JsonDocument.ParseValue(ref reader);
			var root = document.RootElement;
			if (root.ValueKind != JsonValueKind.Object) return null;

			var query = new ShapeQuery();
			var hasBody = false;
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
					case "ignore_unmapped":
						query.IgnoreUnmapped = member.Value.GetBoolean();
						break;
					default:
						query.Field = member.Name;
						hasBody = true;
						foreach (var shapeMember in member.Value.EnumerateObject())
						{
							switch (shapeMember.Name)
							{
								case "shape":
									query.Shape = shapeMember.Value.Deserialize<IGeoShape>(options);
									break;
								case "indexed_shape":
									query.IndexedShape = shapeMember.Value.Deserialize<FieldLookup>(options);
									break;
								case "relation":
									query.Relation = shapeMember.Value.Deserialize<ShapeRelation>(options);
									break;
							}
						}
						break;
				}
			}

			return hasBody ? query : null;
		}
	}
}
