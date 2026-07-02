/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

#nullable enable

using System;
#nullable enable

using System.Text.Json;
#nullable enable

using System.Text.Json.Serialization;
#nullable enable

using OpenSearch.Client;

namespace OpenSearch.Client.QueryDsl.SystemTextJsonConverters
{
	/// <summary>
	/// Converter for <see cref="IGeoShapeQuery"/>.
	/// JSON format (field-name wrapper pattern):
	/// <code>
	/// {"field_name": {"shape": {...}, "relation": "intersects"}}
	/// </code>
	/// or with indexed shape:
	/// <code>
	/// {"field_name": {"indexed_shape": {"index": "...", "id": "...", "path": "..."}}}
	/// </code>
	/// </summary>
	internal sealed class GeoShapeQueryConverter : JsonConverter<IGeoShapeQuery>
	{
		private static readonly GeoShapeConverter ShapeConverter = new GeoShapeConverter();

		public override IGeoShapeQuery Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null)
				return null;

			if (reader.TokenType != JsonTokenType.StartObject)
				throw new JsonException($"Expected StartObject for IGeoShapeQuery but got {reader.TokenType}");

			var query = new GeoShapeQuery();

			while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
			{
				if (reader.TokenType != JsonTokenType.PropertyName)
					throw new JsonException("Expected PropertyName token");

				var propertyName = reader.GetString();
				reader.Read();

				switch (propertyName)
				{
					case "boost":
						query.Boost = reader.GetDouble();
						break;
					case "_name":
						query.Name = reader.GetString();
						break;
					case "ignore_unmapped":
						query.IgnoreUnmapped = reader.GetBoolean();
						break;
					default:
						// This is the field name
						query.Field = new Field(propertyName);
						ReadFieldObject(ref reader, query, options);
						break;
				}
			}

			return query;
		}

		public override void Write(Utf8JsonWriter writer, IGeoShapeQuery value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			writer.WriteStartObject();

			var fieldName = value.Field?.ToString();
			if (string.IsNullOrEmpty(fieldName))
				throw new JsonException("Field name cannot be null for geo_shape query");

			writer.WritePropertyName(fieldName);
			writer.WriteStartObject();

			if (value.Shape != null)
			{
				writer.WritePropertyName("shape");
				ShapeConverter.Write(writer, value.Shape, options);
			}

			if (value.IndexedShape != null)
			{
				writer.WritePropertyName("indexed_shape");
				WriteFieldLookup(writer, value.IndexedShape);
			}

			if (value.Relation.HasValue)
			{
				writer.WritePropertyName("relation");
				writer.WriteStringValue(GetRelationString(value.Relation.Value));
			}

			writer.WriteEndObject(); // end field object

			if (value.IgnoreUnmapped.HasValue)
			{
				writer.WritePropertyName("ignore_unmapped");
				writer.WriteBooleanValue(value.IgnoreUnmapped.Value);
			}

			if (value.Boost.HasValue)
			{
				writer.WritePropertyName("boost");
				writer.WriteNumberValue(value.Boost.Value);
			}

			if (!string.IsNullOrEmpty(value.Name))
			{
				writer.WritePropertyName("_name");
				writer.WriteStringValue(value.Name);
			}

			writer.WriteEndObject();
		}

		private void ReadFieldObject(ref Utf8JsonReader reader, GeoShapeQuery query, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.StartObject)
				throw new JsonException("Expected StartObject for geo_shape field value");

			while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
			{
				if (reader.TokenType != JsonTokenType.PropertyName)
					continue;

				var prop = reader.GetString();
				reader.Read();

				switch (prop)
				{
					case "shape":
						query.Shape = ShapeConverter.Read(ref reader, typeof(IGeoShape), options);
						break;
					case "indexed_shape":
						query.IndexedShape = ReadFieldLookup(ref reader);
						break;
					case "relation":
						query.Relation = ParseRelation(reader.GetString());
						break;
					default:
						reader.Skip();
						break;
				}
			}
		}

		private static IFieldLookup ReadFieldLookup(ref Utf8JsonReader reader)
		{
			if (reader.TokenType != JsonTokenType.StartObject)
				return null;

			var lookup = new FieldLookup();
			while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
			{
				if (reader.TokenType != JsonTokenType.PropertyName)
					continue;

				var prop = reader.GetString();
				reader.Read();

				switch (prop)
				{
					case "index":
						lookup.Index = reader.GetString();
						break;
					case "id":
						lookup.Id = new Id(reader.GetString());
						break;
					case "path":
						lookup.Path = new Field(reader.GetString());
						break;
					case "routing":
						lookup.Routing = reader.GetString();
						break;
					default:
						reader.Skip();
						break;
				}
			}
			return lookup;
		}

		private static void WriteFieldLookup(Utf8JsonWriter writer, IFieldLookup lookup)
		{
			writer.WriteStartObject();
			if (lookup.Index != null)
			{
				writer.WritePropertyName("index");
				writer.WriteStringValue(lookup.Index.ToString());
			}
			if (lookup.Id != null)
			{
				writer.WritePropertyName("id");
				writer.WriteStringValue(lookup.Id.ToString());
			}
			if (lookup.Path != null)
			{
				writer.WritePropertyName("path");
				writer.WriteStringValue(lookup.Path.ToString());
			}
			if (lookup.Routing != null)
			{
				writer.WritePropertyName("routing");
				writer.WriteStringValue(lookup.Routing.ToString());
			}
			writer.WriteEndObject();
		}

		private static string GetRelationString(GeoShapeRelation relation) => relation switch
		{
			GeoShapeRelation.Intersects => "intersects",
			GeoShapeRelation.Disjoint => "disjoint",
			GeoShapeRelation.Within => "within",
			GeoShapeRelation.Contains => "contains",
			_ => relation.ToString().ToLowerInvariant()
		};

		private static GeoShapeRelation? ParseRelation(string value) => value?.ToLowerInvariant() switch
		{
			"intersects" => GeoShapeRelation.Intersects,
			"disjoint" => GeoShapeRelation.Disjoint,
			"within" => GeoShapeRelation.Within,
			"contains" => GeoShapeRelation.Contains,
			_ => null
		};
	}
}
