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
	/// A <see cref="System.Text.Json"/> converter for the polymorphic <see cref="IProperty"/> mapping
	/// hierarchy, replacing the vendored Utf8Json <c>PropertyFormatter</c> as part of #388. Dispatch is
	/// on the <c>type</c> string; the numeric field types all map to <see cref="NumberProperty"/> (with
	/// its <c>Type</c> restored from the wire value), and a missing/unknown type (or a bare
	/// <c>properties</c> object) falls back to <see cref="ObjectProperty"/>.
	/// </summary>
	internal sealed class PropertyInterfaceConverter : JsonConverter<IProperty>
	{
		private static readonly HashSet<string> NumberTypes = new(StringComparer.Ordinal)
		{
			"float", "double", "byte", "short", "integer", "long", "scaled_float", "half_float",
		};

		private static readonly IReadOnlyDictionary<string, Type> TypeByDiscriminator = new Dictionary<string, Type>(StringComparer.Ordinal)
		{
			{ "text", typeof(TextProperty) },
			{ "keyword", typeof(KeywordProperty) },
			{ "search_as_you_type", typeof(SearchAsYouTypeProperty) },
			{ "float", typeof(NumberProperty) },
			{ "double", typeof(NumberProperty) },
			{ "byte", typeof(NumberProperty) },
			{ "short", typeof(NumberProperty) },
			{ "integer", typeof(NumberProperty) },
			{ "long", typeof(NumberProperty) },
			{ "scaled_float", typeof(NumberProperty) },
			{ "half_float", typeof(NumberProperty) },
			{ "date", typeof(DateProperty) },
			{ "date_nanos", typeof(DateNanosProperty) },
			{ "boolean", typeof(BooleanProperty) },
			{ "binary", typeof(BinaryProperty) },
			{ "object", typeof(ObjectProperty) },
			{ "nested", typeof(NestedProperty) },
			{ "ip", typeof(IpProperty) },
			{ "geo_point", typeof(GeoPointProperty) },
			{ "geo_shape", typeof(GeoShapeProperty) },
			{ "completion", typeof(CompletionProperty) },
			{ "token_count", typeof(TokenCountProperty) },
			{ "murmur3", typeof(Murmur3HashProperty) },
			{ "percolator", typeof(PercolatorProperty) },
			{ "date_range", typeof(DateRangeProperty) },
			{ "double_range", typeof(DoubleRangeProperty) },
			{ "float_range", typeof(FloatRangeProperty) },
			{ "integer_range", typeof(IntegerRangeProperty) },
			{ "long_range", typeof(LongRangeProperty) },
			{ "ip_range", typeof(IpRangeProperty) },
			{ "join", typeof(JoinProperty) },
			{ "alias", typeof(FieldAliasProperty) },
			{ "rank_feature", typeof(RankFeatureProperty) },
			{ "rank_features", typeof(RankFeaturesProperty) },
			{ "knn_vector", typeof(KnnVectorProperty) },
		};

		public override IProperty Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null) return null;

			using var document = JsonDocument.ParseValue(ref reader);
			var root = document.RootElement;
			if (root.ValueKind != JsonValueKind.Object) return null;

			string typeString = null;
			if (root.TryGetProperty("type", out var typeProperty) && typeProperty.ValueKind == JsonValueKind.String)
				typeString = typeProperty.GetString();

			if (typeString != null && TypeByDiscriminator.TryGetValue(typeString, out var concreteType))
			{
				var property = (IProperty)root.Deserialize(concreteType, options);
				if (property != null && NumberTypes.Contains(typeString))
					property.Type = typeString; // preserve which numeric type it was
				return property;
			}

			// No (or unrecognized) type: an object mapping, possibly with nested "properties".
			return (IProperty)root.Deserialize(typeof(ObjectProperty), options);
		}

		public override void Write(Utf8JsonWriter writer, IProperty value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			JsonSerializer.Serialize(writer, value, value.GetType(), options);
		}
	}
}
