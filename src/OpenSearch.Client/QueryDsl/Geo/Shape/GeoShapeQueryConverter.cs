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
	/// System.Text.Json replacement for the legacy Utf8Json
	/// <c>CompositeFormatter&lt;IGeoShapeQuery, GeoShapeQueryFormatter, GeoShapeQueryFieldNameFormatter&gt;</c>.
	///
	/// The composite dispatched read and write to two different sub-formatters; this converter folds both paths in:
	/// <list type="bullet">
	/// <item><description><b>Write</b> (legacy <c>GeoShapeQueryFieldNameFormatter</c>): emits the field-name wrapper
	/// <c>{ "_name"?, "boost"?, "ignore_unmapped"?, "&lt;field&gt;": { "shape"|"indexed_shape", "relation"? } }</c>,
	/// resolving the field through the runtime <c>Inferrer</c> (hence <see cref="SettingsAwareConverter{T}"/>). A null
	/// field, or a field that resolves to empty, writes <c>null</c>. Only one of <c>shape</c> / <c>indexed_shape</c> is
	/// written — <c>shape</c> takes precedence.</description></item>
	/// <item><description><b>Read</b> (legacy <c>GeoShapeQueryFormatter</c>): reads the same shape; the property key
	/// that is not <c>boost</c>/<c>_name</c>/<c>ignore_unmapped</c> is taken verbatim as the field name (no
	/// inference), and its object body carries <c>shape</c>/<c>indexed_shape</c>/<c>relation</c>. If neither
	/// <c>shape</c> nor <c>indexed_shape</c> is present the result is <c>null</c>, matching the legacy formatter.</description></item>
	/// </list>
	/// The <see cref="IGeoShape"/> body, <see cref="IFieldLookup"/>, and <see cref="GeoShapeRelation"/> are delegated to
	/// <see cref="JsonSerializer"/> so the registered converters apply.
	/// </summary>
	internal class GeoShapeQueryConverter : SettingsAwareConverter<IGeoShapeQuery>
	{
		public GeoShapeQueryConverter(IConnectionSettingsValues settings) : base(settings) { }

		public override IGeoShapeQuery Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null)
				return null;

			if (reader.TokenType != JsonTokenType.StartObject)
				return null;

			string field = null;
			double? boost = null;
			string name = null;
			bool? ignoreUnmapped = null;
			GeoShapeRelation? relation = null;
			GeoShapeQuery query = null;

			while (reader.Read())
			{
				if (reader.TokenType == JsonTokenType.EndObject)
					break;

				var propertyName = reader.GetString();
				reader.Read(); // advance to the value

				switch (propertyName)
				{
					case "boost":
						boost = reader.GetDouble();
						break;
					case "_name":
						name = reader.GetString();
						break;
					case "ignore_unmapped":
						ignoreUnmapped = reader.GetBoolean();
						break;
					default:
						field = propertyName;
						if (reader.TokenType == JsonTokenType.StartObject)
						{
							while (reader.Read())
							{
								if (reader.TokenType == JsonTokenType.EndObject)
									break;

								var shapeProperty = reader.GetString();
								reader.Read(); // advance to the value
								switch (shapeProperty)
								{
									case "shape":
										query = new GeoShapeQuery
										{
											Shape = JsonSerializer.Deserialize<IGeoShape>(ref reader, options)
										};
										break;
									case "indexed_shape":
										query = new GeoShapeQuery
										{
											IndexedShape = JsonSerializer.Deserialize<IFieldLookup>(ref reader, options)
										};
										break;
									case "relation":
										relation = JsonSerializer.Deserialize<GeoShapeRelation>(ref reader, options);
										break;
									default:
										reader.Skip();
										break;
								}
							}
						}
						break;
				}
			}

			if (query == null)
				return null;

			query.Boost = boost;
			query.Name = name;
			query.Field = field;
			query.Relation = relation;
			query.IgnoreUnmapped = ignoreUnmapped;
			return query;
		}

		public override void Write(Utf8JsonWriter writer, IGeoShapeQuery value, JsonSerializerOptions options)
		{
			if (value?.Field == null)
			{
				writer.WriteNullValue();
				return;
			}

			var field = Settings.Inferrer.Field(value.Field);
			if (string.IsNullOrEmpty(field))
			{
				writer.WriteNullValue();
				return;
			}

			writer.WriteStartObject();

			if (!string.IsNullOrEmpty(value.Name))
			{
				writer.WritePropertyName("_name");
				writer.WriteStringValue(value.Name);
			}

			if (value.Boost != null)
			{
				writer.WritePropertyName("boost");
				writer.WriteNumberValue(value.Boost.Value);
			}

			if (value.IgnoreUnmapped != null)
			{
				writer.WritePropertyName("ignore_unmapped");
				writer.WriteBooleanValue(value.IgnoreUnmapped.Value);
			}

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
			writer.WriteEndObject();
		}
	}
}
