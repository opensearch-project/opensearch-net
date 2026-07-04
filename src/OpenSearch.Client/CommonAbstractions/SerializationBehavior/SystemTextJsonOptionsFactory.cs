/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
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

				var openMap = DataContractResolver.PropertyConverterOverridesOpenGeneric;
				openMap[typeof(SingleOrEnumerableFormatter<>)] = typeof(SingleOrEnumerableConverter<>);
				openMap[typeof(SerializeAsSingleFormatter<>)] = typeof(SerializeAsSingleConverter<>);

				// Verbatim dictionary-key formatters (keys written as inferred, no camel-casing).
				openMap[typeof(VerbatimDictionaryKeysFormatter<,,,>)] = typeof(VerbatimDictionaryKeysConverter<,,,>);
				openMap[typeof(VerbatimDictionaryInterfaceKeysFormatter<,>)] = typeof(VerbatimDictionaryInterfaceKeysConverter<,>);
				openMap[typeof(VerbatimInterfaceReadOnlyDictionaryKeysFormatter<,>)] = typeof(VerbatimInterfaceReadOnlyDictionaryKeysConverter<,>);
				openMap[typeof(VerbatimDictionaryKeysFormatter<,>)] = typeof(VerbatimDictionaryKeysConverter<,>);
				openMap[typeof(VerbatimDictionaryKeysPreservingNullFormatter<,>)] = typeof(VerbatimDictionaryKeysPreservingNullConverter<,>);
				openMap[typeof(VerbatimDictionaryKeysBaseFormatter<,,>)] = typeof(VerbatimDictionaryKeysBaseConverter<,,>);
				openMap[typeof(SuggestDictionaryFormatter<>)] = typeof(SuggestDictionaryConverter<>);

				// Document bodies (_source, update doc/upsert, term-vector/percolate documents) route
				// through the source serializer, mirroring the vendored SourceFormatter<> (#388).
				openMap[typeof(SourceFormatter<>)] = typeof(SourceConverter<>);
				openMap[typeof(CollapsedSourceFormatter<>)] = typeof(SourceConverter<>);
				openMap[typeof(SourceWriteFormatter<>)] = typeof(SourceConverter<>);

				_perPropertyRegistered = true;
			}
		}

		/// <summary>
		/// Builds the options for the request/response serializer: honors the client's <c>[DataMember]</c>
		/// wire names and does not apply document field-name inference.
		/// </summary>
		public static JsonSerializerOptions Create(IConnectionSettingsValues settings) =>
			Build(settings, DataContractResolver.Instance);

		/// <summary>
		/// Builds the options for the <em>source</em> serializer (documents): identical to
		/// <see cref="Create"/> but with a resolver that applies the client's document field-name
		/// inference — camel-casing plus configured property mappings and mapping attributes — mirroring
		/// the vendored <c>OpenSearchClientFormatterResolver</c> (#388).
		/// </summary>
		public static JsonSerializerOptions CreateForSource(IConnectionSettingsValues settings) =>
			Build(settings, new DataContractResolver(BuildSourceNameOverride(settings)));

		/// <summary>
		/// Reproduces the name/ignore precedence of the vendored source resolver's <c>GetMapping</c>:
		/// a configured/attribute-based property mapping wins, then a <c>[PropertyName]</c>/<c>[DataMember]</c>
		/// serializer mapping, otherwise the default field-name inferrer (camel-casing). Settings are read
		/// lazily so mappings configured after construction are honored.
		/// </summary>
		private static Func<MemberInfo, (string Name, bool Ignore)?> BuildSourceNameOverride(IConnectionSettingsValues settings) =>
			member =>
			{
				if (!settings.PropertyMappings.TryGetValue(member, out var propertyMapping))
					propertyMapping = OpenSearchPropertyAttributeBase.From(member);

				var serializerMapping = settings.PropertyMappingProvider?.CreatePropertyMapping(member);

				if ((propertyMapping?.Ignore ?? false) || (serializerMapping?.Ignore ?? false))
					return (null, true);

				var name = propertyMapping?.Name ?? serializerMapping?.Name ?? settings.DefaultFieldNameInferrer(member.Name);
				return (name, false);
			};

		private static JsonSerializerOptions Build(IConnectionSettingsValues settings, IJsonTypeInfoResolver resolver)
		{
			EnsurePerPropertyConvertersRegistered();

			var options = new JsonSerializerOptions
			{
				TypeInfoResolver = resolver,
				DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
				Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
			};

			// Carries the source serializer to the stateless per-property source converters (#388).
			options.Converters.Add(new SourceSerializerProviderConverter(settings));

			// Document (proxy) requests — index/create — serialize as their document body via the source
			// serializer. Registered early so it wins over the generic [ReadAs] factory.
			options.Converters.Add(new ProxyRequestConverterFactory(settings));

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
			options.Converters.Add(new FieldNameQueryConverter<NeuralQuery, INeuralQuery>(settings));
			options.Converters.Add(new MultiTermQueryRewriteConverter());
			options.Converters.Add(new FuzzyQueryConverter(settings));
			options.Converters.Add(new LikeConverter());
			options.Converters.Add(new FieldNameQueryConverter<IntervalsQuery, IIntervalsQuery>(settings));

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
			options.Converters.Add(new GeoShapeConverter());
			options.Converters.Add(new GeoDistanceQueryConverter(settings));
			options.Converters.Add(new GeoPolygonQueryConverter(settings));
			options.Converters.Add(new GeoBoundingBoxQueryConverter(settings));
			options.Converters.Add(new GeoShapeQueryConverter(settings));

			// Specialized
			options.Converters.Add(new RankFeatureQueryConverter(settings));
			options.Converters.Add(new ShapeQueryConverter(settings));
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

			// Other response-side value readers.
			options.Converters.Add(new TotalHitsConverter());
			options.Converters.Add(new TrackTotalHitsConverter());
			options.Converters.Add(new KeyedProcessorStatsConverter());
			options.Converters.Add(new CatFielddataRecordConverter());
			options.Converters.Add(new LazyDocumentConverter(settings));
			options.Converters.Add(new LazyDocumentInterfaceConverter(settings));
			options.Converters.Add(new FieldValuesConverter(settings));

			options.Converters.Add(new BulkResponseItemConverter());
			options.Converters.Add(new GetRepositoryResponseConverter());

			// Top-level dictionary/dynamic response readers (recognized by their [JsonFormatter] type).
			options.Converters.Add(new ResponseFormatterConverterFactory(settings));

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
			options.Converters.Add(new DynamicTemplatesConverter());
			options.Converters.Add(new SimilarityConverter());
			options.Converters.Add(new AutoExpandReplicasConverter());
			options.Converters.Add(new IndexSettingsConverter());
			options.Converters.Add(new DynamicIndexSettingsConverter());
			options.Converters.Add(new AttachmentConverter());
			options.Converters.Add(new AliasActionConverter());
			options.Converters.Add(new IndicesBoostConverter(settings));

			// Fuzziness (queries + full-text options).
			options.Converters.Add(new FuzzinessConverter());

			// Ingest pipeline processors (polymorphic dispatch).
			options.Converters.Add(new ProcessorConverter());

			// Other polymorphic / value converters.
			options.Converters.Add(new ChildrenConverter());
			options.Converters.Add(new DynamicMappingConverter());
			options.Converters.Add(new ClusterRerouteCommandConverter());

			// Generic [ReadAs] mapping for any remaining interface used as a nested property
			// (e.g. ISpanQuery). Registered last so dedicated converters take precedence.
			options.Converters.Add(new ReadAsConverterFactory());

			return options;
		}
	}
}
