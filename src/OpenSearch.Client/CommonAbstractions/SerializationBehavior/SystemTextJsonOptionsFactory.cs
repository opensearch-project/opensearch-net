/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using OpenSearch.Net;

namespace OpenSearch.Client
{
	/// <summary>
	/// Builds the <see cref="JsonSerializerOptions"/> for the high-level client's
	/// <c>System.Text.Json</c> serializer (#388), threading the connection settings into the
	/// converters that need field-name inference (decision D1). Central registration point for every
	/// migrated converter; as more namespaces are migrated their converters are added here.
	/// </summary>
	internal static class SystemTextJsonOptionsFactory
	{
		private static readonly object PerPropertyLock = new object();
		private static bool _perPropertyRegistered;

		/// <summary>
		/// Registers the per-property converter overrides (primitives the server may send as strings)
		/// with the shared <see cref="DataContractResolver"/> once (#388). Keyed by the vendored
		/// Utf8Json formatter type each member's <c>[JsonFormatter]</c> references.
		/// </summary>
		private static void EnsurePerPropertyConvertersRegistered()
		{
			if (_perPropertyRegistered) return;
			lock (PerPropertyLock)
			{
				if (_perPropertyRegistered) return;

				var map = DataContractResolver.PropertyConverterOverrides;
				var nullableInt = new NullableStringIntConverter();
				map[typeof(NullableStringBooleanFormatter)] = new NullableStringBooleanConverter();
				map[typeof(NullableStringIntFormatter)] = nullableInt;
				map[typeof(OpenSearch.Net.NullableStringIntFormatter)] = nullableInt;
				map[typeof(NullableStringLongFormatter)] = new NullableStringLongConverter();
				map[typeof(NullableStringDoubleFormatter)] = new NullableStringDoubleConverter();
				map[typeof(StringLongFormatter)] = new StringLongConverter();
				map[typeof(StringIntFormatter)] = new StringIntConverter();
				map[typeof(IntStringFormatter)] = new IntStringConverter();

				_perPropertyRegistered = true;
			}
		}

		public static JsonSerializerOptions Create(IConnectionSettingsValues settings)
		{
			EnsurePerPropertyConvertersRegistered();

			var options = new JsonSerializerOptions
			{
				TypeInfoResolver = DataContractResolver.Instance,
				DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
				Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
			};

			// Stateless infrastructure converters (OpenSearch.Net).
			options.Converters.Add(ObjectConverter.Instance);
			options.Converters.Add(DoubleFormatConverter.Instance);
			options.Converters.Add(SingleFormatConverter.Instance);
			options.Converters.Add(StringEnumConverterFactory.Instance);

			// Settings-bearing converters (field-name / id inference — decision D1).
			options.Converters.Add(new FieldConverter(settings));
			options.Converters.Add(new FieldsConverter(settings));
			options.Converters.Add(new PropertyNameConverter(settings));
			options.Converters.Add(new IdConverter(settings));
			options.Converters.Add(new RelationNameConverter(settings));

			// Value-type and polymorphic converters (OpenSearch.Client).
			options.Converters.Add(new StopWordsConverter());
			options.Converters.Add(new MinimumShouldMatchConverter());
			options.Converters.Add(new ScriptInterfaceConverter());
			options.Converters.Add(new TokenizerInterfaceConverter());
			options.Converters.Add(new CharFilterInterfaceConverter());
			options.Converters.Add(new TokenFilterInterfaceConverter());
			options.Converters.Add(new AnalyzerInterfaceConverter());
			options.Converters.Add(new NormalizerInterfaceConverter());
			options.Converters.Add(new QueryContainerConverter());

			// Field-name-keyed queries ({ "<field>": { … } }); settings-bearing (decision D1).
			options.Converters.Add(new FieldNameQueryConverter<TermQuery, ITermQuery>(settings));
			options.Converters.Add(new FieldNameQueryConverter<PrefixQuery, IPrefixQuery>(settings));
			options.Converters.Add(new FieldNameQueryConverter<WildcardQuery, IWildcardQuery>(settings));
			options.Converters.Add(new FieldNameQueryConverter<RegexpQuery, IRegexpQuery>(settings));
			options.Converters.Add(new FieldNameQueryConverter<MatchQuery, IMatchQuery>(settings));
			options.Converters.Add(new FieldNameQueryConverter<MatchPhraseQuery, IMatchPhraseQuery>(settings));
			options.Converters.Add(new FieldNameQueryConverter<MatchPhrasePrefixQuery, IMatchPhrasePrefixQuery>(settings));
			options.Converters.Add(new FieldNameQueryConverter<MatchBoolPrefixQuery, IMatchBoolPrefixQuery>(settings));
			options.Converters.Add(new TermsQueryConverter(settings));
			options.Converters.Add(new FieldNameQueryConverter<TermsSetQuery, ITermsSetQuery>(settings));
			options.Converters.Add(new FieldNameQueryConverter<SpanTermQuery, ISpanTermQuery>(settings));
			options.Converters.Add(new FieldNameQueryConverter<KnnQuery, IKnnQuery>(settings));

			// Range family: IRangeQuery sniffs the bound types and dispatches to the concrete range,
			// each of which is a field-name-keyed query.
			options.Converters.Add(new DateMathConverter());
			options.Converters.Add(new RangeQueryInterfaceConverter());
			options.Converters.Add(new FieldNameQueryConverter<NumericRangeQuery, INumericRangeQuery>(settings));
			options.Converters.Add(new FieldNameQueryConverter<LongRangeQuery, ILongRangeQuery>(settings));
			options.Converters.Add(new FieldNameQueryConverter<TermRangeQuery, ITermRangeQuery>(settings));
			options.Converters.Add(new FieldNameQueryConverter<DateRangeQuery, IDateRangeQuery>(settings));

			// Geo
			options.Converters.Add(new DistanceConverter());
			options.Converters.Add(new GeoLocationConverter());
			options.Converters.Add(new GeoCoordinateConverter());
			options.Converters.Add(new GeoDistanceQueryConverter(settings));
			options.Converters.Add(new GeoPolygonQueryConverter(settings));
			options.Converters.Add(new GeoBoundingBoxQueryConverter(settings));

			// Specialized
			options.Converters.Add(new RankFeatureQueryConverter(settings));
			options.Converters.Add(new DistanceFeatureQueryConverter());
			options.Converters.Add(new UnionConverter<GeoCoordinate, DateMath>());
			options.Converters.Add(new UnionConverter<Distance, Time>());

			// Aggregations (request side)
			options.Converters.Add(new AggregationDictionaryConverter());
			options.Converters.Add(new SortOrderConverter<TermsOrder>());
			options.Converters.Add(new SortOrderConverter<HistogramOrder>());
			options.Converters.Add(new BucketsPathConverter());
			options.Converters.Add(new TimeConverter());
			options.Converters.Add(new TermsIncludeConverter());
			options.Converters.Add(new TermsExcludeConverter());
			options.Converters.Add(new IncludeExcludeConverter());
			options.Converters.Add(new DateMathTimeConverter());
			options.Converters.Add(new UnionConverter<DateInterval?, DateMathTime>());
			options.Converters.Add(new FilterAggregationConverter());
			options.Converters.Add(new PercentilesAggregationConverter());
			options.Converters.Add(new PercentileRanksAggregationConverter());
			options.Converters.Add(new CompositeAggregationSourceConverter());

			// Aggregations (response side, read-only)
			options.Converters.Add(new AggregateConverter());
			options.Converters.Add(new AggregateResponseDictionaryConverter());

			// Infer / identity value types.
			options.Converters.Add(new IndexNameConverter(settings));
			options.Converters.Add(new IndicesConverter(settings));
			options.Converters.Add(new RoutingConverter(settings));
			options.Converters.Add(new TaskIdConverter());

			// Common request/document options.
			options.Converters.Add(new SortConverter(settings));
			options.Converters.Add(new SourceFilterConverter());
			options.Converters.Add(new SlicesConverter());
			options.Converters.Add(new ReindexRoutingConverter());

			// Mappings
			options.Converters.Add(new PropertyInterfaceConverter());
			options.Converters.Add(new PropertiesConverter(settings));
			options.Converters.Add(new JoinFieldConverter(settings));
			options.Converters.Add(new SimilarityConverter());
			options.Converters.Add(new AutoExpandReplicasConverter());
			options.Converters.Add(new AttachmentConverter());
			options.Converters.Add(new AliasActionConverter());
			options.Converters.Add(new IndicesBoostConverter(settings));

			// Fuzziness (queries + full-text options).
			options.Converters.Add(new FuzzinessConverter());

			// Generic [ReadAs] mapping for any remaining interface used as a nested property
			// (e.g. ISpanQuery). Registered last so dedicated converters take precedence.
			options.Converters.Add(new ReadAsConverterFactory());

			return options;
		}
	}
}
