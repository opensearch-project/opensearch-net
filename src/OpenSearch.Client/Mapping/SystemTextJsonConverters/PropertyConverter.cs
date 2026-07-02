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

namespace OpenSearch.Client.Mapping.SystemTextJsonConverters
{
	/// <summary>
	/// Polymorphic converter for <see cref="IProperty"/>.
	/// Reads the "type" discriminator to dispatch to the correct concrete property type.
	/// Writes by serializing the concrete runtime type including the "type" field.
	/// </summary>
	internal sealed class PropertyConverter : JsonConverter<IProperty>
	{
		public override IProperty Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null)
				return null;

			if (reader.TokenType != JsonTokenType.StartObject)
				throw new JsonException($"Expected StartObject for IProperty but got {reader.TokenType}");

			// Buffer the object to peek at "type" then deserialize to the correct concrete type
			using var doc = JsonDocument.ParseValue(ref reader);
			var root = doc.RootElement;

			var typeString = root.TryGetProperty("type", out var typeProp)
				? typeProp.GetString()
				: null;

			// If no explicit "type" but has "properties", treat as object
			if (typeString == null && root.TryGetProperty("properties", out _))
				typeString = "object";

			var rawJson = root.GetRawText();
			return DeserializeByType(rawJson, typeString, options);
		}

		private static IProperty DeserializeByType(string rawJson, string typeString, JsonSerializerOptions options)
		{
			switch (typeString)
			{
				case "text":
					return JsonSerializer.Deserialize<TextProperty>(rawJson, options);
				case "keyword":
					return JsonSerializer.Deserialize<KeywordProperty>(rawJson, options);
				case "search_as_you_type":
					return JsonSerializer.Deserialize<SearchAsYouTypeProperty>(rawJson, options);
				case "float":
				case "double":
				case "byte":
				case "short":
				case "integer":
				case "long":
				case "scaled_float":
				case "half_float":
					return DeserializeNumber(rawJson, typeString, options);
				case "date":
					return JsonSerializer.Deserialize<DateProperty>(rawJson, options);
				case "date_nanos":
					return JsonSerializer.Deserialize<DateNanosProperty>(rawJson, options);
				case "boolean":
					return JsonSerializer.Deserialize<BooleanProperty>(rawJson, options);
				case "binary":
					return JsonSerializer.Deserialize<BinaryProperty>(rawJson, options);
				case "object":
					return JsonSerializer.Deserialize<ObjectProperty>(rawJson, options);
				case "nested":
					return JsonSerializer.Deserialize<NestedProperty>(rawJson, options);
				case "ip":
					return JsonSerializer.Deserialize<IpProperty>(rawJson, options);
				case "geo_point":
					return JsonSerializer.Deserialize<GeoPointProperty>(rawJson, options);
				case "geo_shape":
					return JsonSerializer.Deserialize<GeoShapeProperty>(rawJson, options);
				case "completion":
					return JsonSerializer.Deserialize<CompletionProperty>(rawJson, options);
				case "token_count":
					return JsonSerializer.Deserialize<TokenCountProperty>(rawJson, options);
				case "murmur3":
					return JsonSerializer.Deserialize<Murmur3HashProperty>(rawJson, options);
				case "percolator":
					return JsonSerializer.Deserialize<PercolatorProperty>(rawJson, options);
				case "date_range":
					return JsonSerializer.Deserialize<DateRangeProperty>(rawJson, options);
				case "double_range":
					return JsonSerializer.Deserialize<DoubleRangeProperty>(rawJson, options);
				case "float_range":
					return JsonSerializer.Deserialize<FloatRangeProperty>(rawJson, options);
				case "integer_range":
					return JsonSerializer.Deserialize<IntegerRangeProperty>(rawJson, options);
				case "long_range":
					return JsonSerializer.Deserialize<LongRangeProperty>(rawJson, options);
				case "ip_range":
					return JsonSerializer.Deserialize<IpRangeProperty>(rawJson, options);
				case "join":
					return JsonSerializer.Deserialize<JoinProperty>(rawJson, options);
				case "alias":
					return JsonSerializer.Deserialize<FieldAliasProperty>(rawJson, options);
				case "rank_feature":
					return JsonSerializer.Deserialize<RankFeatureProperty>(rawJson, options);
				case "rank_features":
					return JsonSerializer.Deserialize<RankFeaturesProperty>(rawJson, options);
				case "knn_vector":
					return JsonSerializer.Deserialize<KnnVectorProperty>(rawJson, options);
				case null:
					// No type field: default to ObjectProperty
					return JsonSerializer.Deserialize<ObjectProperty>(rawJson, options);
				default:
					// Unknown type: deserialize as ObjectProperty, preserve the type string
					var prop = JsonSerializer.Deserialize<ObjectProperty>(rawJson, options);
					if (prop != null)
						((IProperty)prop).Type = typeString;
					return prop;
			}
		}

		private static IProperty DeserializeNumber(string rawJson, string typeString, JsonSerializerOptions options)
		{
			var property = JsonSerializer.Deserialize<NumberProperty>(rawJson, options);
			if (property != null)
				((IProperty)property).Type = typeString;
			return property;
		}

		public override void Write(Utf8JsonWriter writer, IProperty value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			// Serialize as the concrete runtime type
			switch (value)
			{
				case TextProperty v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				case KeywordProperty v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				case SearchAsYouTypeProperty v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				case NumberProperty v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				case DateProperty v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				case DateNanosProperty v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				case BooleanProperty v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				case BinaryProperty v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				case NestedProperty v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				case ObjectProperty v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				case IpProperty v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				case GeoPointProperty v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				case GeoShapeProperty v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				case CompletionProperty v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				case TokenCountProperty v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				case Murmur3HashProperty v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				case PercolatorProperty v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				case DateRangeProperty v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				case DoubleRangeProperty v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				case FloatRangeProperty v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				case IntegerRangeProperty v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				case LongRangeProperty v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				case IpRangeProperty v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				case JoinProperty v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				case FieldAliasProperty v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				case RankFeatureProperty v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				case RankFeaturesProperty v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				case KnnVectorProperty v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				default:
					JsonSerializer.Serialize(writer, value, value.GetType(), options);
					break;
			}
		}
	}
}
