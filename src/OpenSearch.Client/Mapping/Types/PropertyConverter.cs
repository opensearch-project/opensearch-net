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
	/// System.Text.Json replacement for the legacy Utf8Json <c>PropertyFormatter</c>.
	///
	/// An <see cref="IProperty"/> is polymorphic: the concrete mapping type is selected by the value of the
	/// <c>type</c> discriminator field (<c>text</c>, <c>keyword</c>, <c>date</c>, <c>long</c>, <c>geo_point</c>, …).
	/// When no <c>type</c> field is present the mapping is treated as an object mapping (inferred by the presence of a
	/// <c>properties</c> field, otherwise still defaulting to an object mapping), exactly as the legacy formatter did.
	///
	/// System.Text.Json's <see cref="Utf8JsonReader"/> is forward-only and cannot be rewound — the Utf8Json version
	/// peeked at a byte segment and re-read it — so we buffer the value into a <see cref="JsonDocument"/>, scan the DOM
	/// for the discriminator (preserving the legacy scan order and fallback), then deserialize the whole element as the
	/// resolved concrete type so no members are dropped. On write we dispatch on the runtime type.
	///
	/// This converter does not resolve field names itself (that happens inside the concrete property contracts and the
	/// member-level converters), so — like the legacy formatter, which never called
	/// <c>formatterResolver.GetConnectionSettings()</c> — it is a plain <see cref="JsonConverter{T}"/> and needs no
	/// settings.
	/// </summary>
	internal class PropertyConverter : JsonConverter<IProperty>
	{
		public override IProperty Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null)
				return null;

			using var doc = JsonDocument.ParseValue(ref reader);
			var root = doc.RootElement;

			if (root.ValueKind != JsonValueKind.Object)
				return null;

			string typeString = null;
			var type = FieldType.None;

			// Mirror the legacy forward scan: an explicit "type" wins and stops the scan; a "properties" field seen
			// before any "type" tentatively marks the mapping as an object but the scan continues in case a later
			// "type" field appears.
			foreach (var property in root.EnumerateObject())
			{
				if (property.Name == "type")
				{
					typeString = property.Value.ValueKind == JsonValueKind.String ? property.Value.GetString() : null;
					type = typeString.ToEnum<FieldType>().GetValueOrDefault(type);
					// explicit type has been set
					break;
				}

				if (property.Name == "properties")
				{
					if (type == FieldType.None)
						type = FieldType.Object;
					// keep scanning for a possible explicit "type"
				}
			}

			switch (type)
			{
				case FieldType.Text: return root.Deserialize<TextProperty>(options);
				case FieldType.Keyword: return root.Deserialize<KeywordProperty>(options);
				case FieldType.SearchAsYouType: return root.Deserialize<SearchAsYouTypeProperty>(options);
				case FieldType.Float:
				case FieldType.Double:
				case FieldType.Byte:
				case FieldType.Short:
				case FieldType.Integer:
				case FieldType.Long:
				case FieldType.ScaledFloat:
				case FieldType.HalfFloat:
					var numberProperty = root.Deserialize<NumberProperty>(options);
					((IProperty)numberProperty).Type = typeString;
					return numberProperty;
				case FieldType.Date: return root.Deserialize<DateProperty>(options);
				case FieldType.DateNanos: return root.Deserialize<DateNanosProperty>(options);
				case FieldType.Boolean: return root.Deserialize<BooleanProperty>(options);
				case FieldType.Binary: return root.Deserialize<BinaryProperty>(options);
				case FieldType.Object: return root.Deserialize<ObjectProperty>(options);
				case FieldType.Nested: return root.Deserialize<NestedProperty>(options);
				case FieldType.Ip: return root.Deserialize<IpProperty>(options);
				case FieldType.GeoPoint: return root.Deserialize<GeoPointProperty>(options);
				case FieldType.GeoShape: return root.Deserialize<GeoShapeProperty>(options);
				case FieldType.Completion: return root.Deserialize<CompletionProperty>(options);
				case FieldType.TokenCount: return root.Deserialize<TokenCountProperty>(options);
				case FieldType.Murmur3Hash: return root.Deserialize<Murmur3HashProperty>(options);
				case FieldType.Percolator: return root.Deserialize<PercolatorProperty>(options);
				case FieldType.DateRange: return root.Deserialize<DateRangeProperty>(options);
				case FieldType.DoubleRange: return root.Deserialize<DoubleRangeProperty>(options);
				case FieldType.FloatRange: return root.Deserialize<FloatRangeProperty>(options);
				case FieldType.IntegerRange: return root.Deserialize<IntegerRangeProperty>(options);
				case FieldType.LongRange: return root.Deserialize<LongRangeProperty>(options);
				case FieldType.IpRange: return root.Deserialize<IpRangeProperty>(options);
				case FieldType.Join: return root.Deserialize<JoinProperty>(options);
				case FieldType.Alias: return root.Deserialize<FieldAliasProperty>(options);
				case FieldType.RankFeature: return root.Deserialize<RankFeatureProperty>(options);
				case FieldType.RankFeatures: return root.Deserialize<RankFeaturesProperty>(options);
				case FieldType.KnnVector: return root.Deserialize<KnnVectorProperty>(options);
				case FieldType.Wildcard: return root.Deserialize<WildcardProperty>(options);
				case FieldType.None:
					// no "type" field in the property mapping, or FieldType enum could not be parsed from typeString
					return root.Deserialize<ObjectProperty>(options);
				default:
					throw new ArgumentOutOfRangeException(nameof(type), type, "mapping property converter does not know this value");
			}
		}

		public override void Write(Utf8JsonWriter writer, IProperty value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			switch (value)
			{
				case ITextProperty textProperty:
					JsonSerializer.Serialize(writer, textProperty, options);
					break;
				case IKeywordProperty keywordProperty:
					JsonSerializer.Serialize(writer, keywordProperty, options);
					break;
				case INumberProperty numberProperty:
					JsonSerializer.Serialize(writer, numberProperty, options);
					break;
				case IDateProperty dateProperty:
					JsonSerializer.Serialize(writer, dateProperty, options);
					break;
				case IBooleanProperty booleanProperty:
					JsonSerializer.Serialize(writer, booleanProperty, options);
					break;
				case INestedProperty nestedProperty:
					JsonSerializer.Serialize(writer, nestedProperty, options);
					break;
				case IObjectProperty objectProperty:
					JsonSerializer.Serialize(writer, objectProperty, options);
					break;
				case ISearchAsYouTypeProperty searchAsYouTypeProperty:
					JsonSerializer.Serialize(writer, searchAsYouTypeProperty, options);
					break;
				case IDateNanosProperty dateNanosProperty:
					JsonSerializer.Serialize(writer, dateNanosProperty, options);
					break;
				case IBinaryProperty binaryProperty:
					JsonSerializer.Serialize(writer, binaryProperty, options);
					break;
				case IIpProperty ipProperty:
					JsonSerializer.Serialize(writer, ipProperty, options);
					break;
				case IGeoPointProperty geoPointProperty:
					JsonSerializer.Serialize(writer, geoPointProperty, options);
					break;
				case IGeoShapeProperty geoShapeProperty:
					JsonSerializer.Serialize(writer, geoShapeProperty, options);
					break;
				case ICompletionProperty completionProperty:
					JsonSerializer.Serialize(writer, completionProperty, options);
					break;
				case ITokenCountProperty tokenCountProperty:
					JsonSerializer.Serialize(writer, tokenCountProperty, options);
					break;
				case IMurmur3HashProperty murmur3HashProperty:
					JsonSerializer.Serialize(writer, murmur3HashProperty, options);
					break;
				case IPercolatorProperty percolatorProperty:
					JsonSerializer.Serialize(writer, percolatorProperty, options);
					break;
				case IDateRangeProperty dateRangeProperty:
					JsonSerializer.Serialize(writer, dateRangeProperty, options);
					break;
				case IDoubleRangeProperty doubleRangeProperty:
					JsonSerializer.Serialize(writer, doubleRangeProperty, options);
					break;
				case IFloatRangeProperty floatRangeProperty:
					JsonSerializer.Serialize(writer, floatRangeProperty, options);
					break;
				case IIntegerRangeProperty integerRangeProperty:
					JsonSerializer.Serialize(writer, integerRangeProperty, options);
					break;
				case ILongRangeProperty longRangeProperty:
					JsonSerializer.Serialize(writer, longRangeProperty, options);
					break;
				case IIpRangeProperty ipRangeProperty:
					JsonSerializer.Serialize(writer, ipRangeProperty, options);
					break;
				case IJoinProperty joinProperty:
					JsonSerializer.Serialize(writer, joinProperty, options);
					break;
				case IFieldAliasProperty fieldAliasProperty:
					JsonSerializer.Serialize(writer, fieldAliasProperty, options);
					break;
				case IRankFeatureProperty rankFeatureProperty:
					JsonSerializer.Serialize(writer, rankFeatureProperty, options);
					break;
				case IRankFeaturesProperty rankFeaturesProperty:
					JsonSerializer.Serialize(writer, rankFeaturesProperty, options);
					break;
				case IKnnVectorProperty knnVectorProperty:
					JsonSerializer.Serialize(writer, knnVectorProperty, options);
					break;
				case IWildcardProperty wildcardProperty:
					JsonSerializer.Serialize(writer, wildcardProperty, options);
					break;
				case IGenericProperty genericProperty:
					JsonSerializer.Serialize(writer, genericProperty, options);
					break;
				default:
					JsonSerializer.Serialize(writer, value, value.GetType(), options);
					break;
			}
		}
	}
}
