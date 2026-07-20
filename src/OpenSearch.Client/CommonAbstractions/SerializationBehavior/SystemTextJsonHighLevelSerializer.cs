/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using OpenSearch.Net;

namespace OpenSearch.Client
{
	/// <summary>
	/// PROTOTYPE (spike) — a System.Text.Json based replacement for <see cref="DefaultHighLevelSerializer"/>,
	/// which today delegates to the Utf8Json engine via <c>OpenSearchClientFormatterResolver</c>.
	///
	/// Goal of the spike: prove that a STJ-based high-level serializer can be driven by the runtime
	/// <see cref="IConnectionSettingsValues"/> configuration (field-name inference, property mappings) and reuse
	/// the contract model we already migrated (InterfaceDataContractResolver), while remaining a drop-in
	/// <see cref="IOpenSearchSerializer"/>.
	///
	/// This is NOT feature-complete: it establishes the wiring and the extension point (a settings-aware
	/// TypeInfoResolver) that the full migration will build on.
	/// </summary>
	internal class SystemTextJsonHighLevelSerializer : IOpenSearchSerializer
	{
		private readonly JsonSerializerOptions _options;

		public SystemTextJsonHighLevelSerializer(IConnectionSettingsValues settings)
		{
			_options = new JsonSerializerOptions
			{
				DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
				PropertyNameCaseInsensitive = true,
				// The legacy Utf8Json engine did not HTML-escape output; System.Text.Json's default encoder escapes
				// '+', '&', '<', '>' etc. as \uXXXX. Use the relaxed encoder so payloads match the legacy bytes
				// (e.g. date-math "now+1d/d" stays literal instead of "now+1d/d").
				Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
				// The settings-aware resolver reproduces the runtime-config-driven behaviour of the old
				// InnerResolver.GetMapping: field-name inference and per-member property mappings.
				TypeInfoResolver = new HighLevelContractResolver(settings)
			};

			// [ReadAs] delegation: interfaces/abstract types deserialize as the concrete type named by the attribute.
			_options.Converters.Add(new ReadAsConverterFactory());
			// Field-name queries ({ "field": { <body> } }): a factory constructs the settings-aware
			// FieldNameQueryConverter<T,TInterface> per query interface, reusing the legacy [JsonFormatter] mapping.
			_options.Converters.Add(new FieldNameQueryConverterFactory(settings));
			// Low-level converters already migrated (OpenSearch.Net) that the high-level client also relies on.
			_options.Converters.Add(new OpenSearch.Net.Serialization.Converters.StringEnumConverterFactory());
			_options.Converters.Add(new OpenSearch.Net.Serialization.Converters.NullableStringIntConverter());
			_options.Converters.Add(new OpenSearch.Net.Serialization.Converters.DynamicDictionaryConverter());
			// double/float: reproduce the legacy Utf8Json trailing ".0" for integral floating-point values (STJ
			// writes "10" where the old engine wrote "10.0"), which otherwise breaks exact-JSON comparisons.
			_options.Converters.Add(new OpenSearch.Net.Serialization.Converters.DoubleConverter());
			_options.Converters.Add(new OpenSearch.Net.Serialization.Converters.NullableDoubleConverter());
			_options.Converters.Add(new OpenSearch.Net.Serialization.Converters.SingleConverter());
			_options.Converters.Add(new OpenSearch.Net.Serialization.Converters.NullableSingleConverter());
			// ISO 8601 date/time: reproduces the legacy engine's default DateTime/DateTimeOffset parsing (basic-format
			// numeric offsets like +1000/+10 and >7 fractional digits), which the built-in Utf8JsonReader rejects
			// (GitHub issue #4876). Type-level default, so registered globally.
			_options.Converters.Add(new OpenSearch.Net.Serialization.Converters.Iso8601DateTimeConverter());
			_options.Converters.Add(new OpenSearch.Net.Serialization.Converters.NullableIso8601DateTimeConverter());
			_options.Converters.Add(new OpenSearch.Net.Serialization.Converters.Iso8601DateTimeOffsetConverter());
			_options.Converters.Add(new OpenSearch.Net.Serialization.Converters.NullableIso8601DateTimeOffsetConverter());
			// TimeSpan/TimeSpan? default to ticks (a JSON number) in the high-level engine (legacy InnerResolver
			// registers TimeSpanTicksFormatter as the type-level default). Members marked [StringTimeSpan] override
			// this with the string form via the member-formatter mapping.
			_options.Converters.Add(new TimeSpanTicksConverter());
			_options.Converters.Add(new NullableTimeSpanTicksConverter());

			// Migrated high-level converters that are the *type-level* default for their target type — i.e. the
			// legacy engine attached them via a type-level [JsonFormatter] on the type definition, so registering
			// them globally reproduces the same behaviour. Only such converters are registered here.
			//
			// Deliberately NOT registered globally (they would hijack every value of a shared type or need
			// per-member/generic parameterisation, and are instead applied member-by-member in a later phase):
			//   - epoch date/time, IntString, NullableStringBoolean, TimeSpanTicks (target string/bool?/DateTime?/TimeSpan)
			//   - SortOrder<T>, Union<,>, UnionList<>, VerbatimDictionaryKeys<>, FieldNameQuery<,> (open generics)
			//   - IndicesConverter (a member-level alternative to the IndicesMultiSyntax type default)

			// Settings-aware converters (Inferrer-driven); constructed with the runtime settings.
			_options.Converters.Add(new IdConverter(settings));
			_options.Converters.Add(new IndexNameConverter(settings));
			_options.Converters.Add(new IndicesMultiSyntaxConverter(settings)); // type-level default for Indices
			_options.Converters.Add(new RoutingConverter(settings));
			_options.Converters.Add(new PropertyNameConverter(settings));
			_options.Converters.Add(new RelationNameConverter(settings));
			_options.Converters.Add(new JoinFieldConverter(settings));
			_options.Converters.Add(new ChildrenConverter(settings));

			// Stateless converters whose target type is specific to the option they model.
			_options.Converters.Add(new FuzzinessConverter());
			_options.Converters.Add(new DistanceConverter());
			_options.Converters.Add(new MinimumShouldMatchConverter());
			_options.Converters.Add(new TimeConverter());
			_options.Converters.Add(new SlicesConverter());
			_options.Converters.Add(new TrackTotalHitsConverter());
			_options.Converters.Add(new SimilarityConverter());
			_options.Converters.Add(new TermsIncludeConverter());
			_options.Converters.Add(new TermsExcludeConverter());
			_options.Converters.Add(new AnalyzerConverter());
			_options.Converters.Add(new CharFilterConverter());
			_options.Converters.Add(new NormalizerConverter());
			_options.Converters.Add(new TokenFilterConverter());
			_options.Converters.Add(new TokenizerConverter());
			_options.Converters.Add(new ScriptConverter());
			_options.Converters.Add(new FuzzyQueryConverter());
			_options.Converters.Add(new RangeQueryConverter());
			_options.Converters.Add(new MultiTermQueryRewriteConverter());
			_options.Converters.Add(new GeoCoordinateConverter());
			_options.Converters.Add(new GeoLocationConverter());
			_options.Converters.Add(new AggregateDictionaryConverter());

			// Batch 9: further stateless type-level defaults (leaf/union/enum types) migrated in parallel.
			// DateMath is abstract; register the concrete converters as well so a property typed as the concrete
			// type binds its own converter, while DateMath-typed properties bind the base converter.
			_options.Converters.Add(new DateMathExpressionConverter());
			_options.Converters.Add(new DateMathTimeConverter());
			_options.Converters.Add(new DateMathConverter());
			_options.Converters.Add(new TotalHitsConverter());
			_options.Converters.Add(new TaskIdConverter());
			_options.Converters.Add(new AutoExpandReplicasConverter());
			_options.Converters.Add(new ReindexRoutingConverter());
			_options.Converters.Add(new GeoOrientationConverter());
			_options.Converters.Add(new NullableGeoOrientationConverter());
			_options.Converters.Add(new ShapeOrientationConverter());
			_options.Converters.Add(new NullableShapeOrientationConverter());
			_options.Converters.Add(new IncludeExcludeConverter());
			_options.Converters.Add(new CartesianPointConverter());
			_options.Converters.Add(new StopWordsConverter());
			_options.Converters.Add(new SimpleQueryStringFlagsConverter());
			_options.Converters.Add(new LikeConverter());

			// Batch 10: settings-aware infer types + polymorphic/union type-level defaults.
			_options.Converters.Add(new FieldConverter(settings));
			_options.Converters.Add(new FieldsConverter(settings));
			_options.Converters.Add(new AliasActionConverter());
			_options.Converters.Add(new ClusterRerouteCommandConverter());
			_options.Converters.Add(new SourceFilterConverter());
			_options.Converters.Add(new ContextConverter());
			_options.Converters.Add(new BucketsPathConverter());
			_options.Converters.Add(new DynamicMappingConverter());
			_options.Converters.Add(new ScoreFunctionConverter(settings));

			// Batch 11: polymorphic heavyweights.
			_options.Converters.Add(new QueryContainerConverter());
			_options.Converters.Add(new QueryContainerInterfaceConverter());
			_options.Converters.Add(new QueryContainerCollectionConverter());
			_options.Converters.Add(new PropertyConverter());
			_options.Converters.Add(new PropertiesConverter(settings));
			_options.Converters.Add(new AggregateConverter(settings));
			_options.Converters.Add(new AggregationContainerConverter());
			_options.Converters.Add(new ProcessorConverter());
			_options.Converters.Add(new GeoShapeConverter());
			_options.Converters.Add(new DynamicIndexSettingsConverter());
			_options.Converters.Add(new IndexSettingsConverter());
			_options.Converters.Add(new SortConverter(settings));

			// Batch 12: tail cleanup — leaf/misc, Snapshot repos, ndjson request bodies, and the stateless bulk
			// response item. NOT registered here: MultiGetResponse / MultiSearchResponse converters, which require
			// per-request state and are installed per-request via CustomResponseBuilder (CreateStateful), matching
			// the legacy design — a global registration cannot supply the originating request's document types.
			_options.Converters.Add(new SuggestContextConverter());
			_options.Converters.Add(new AttachmentConverter());
			_options.Converters.Add(new FieldValuesConverter(settings));
			_options.Converters.Add(new CatFielddataRecordConverter());
			_options.Converters.Add(new KeyedProcessorStatsConverter());
			_options.Converters.Add(new LazyDocumentConverter(settings));
			_options.Converters.Add(new LazyDocumentInterfaceConverter(settings));
			_options.Converters.Add(new CreateRepositoryConverter());
			_options.Converters.Add(new SourceOnlyRepositoryConverter());
			_options.Converters.Add(new GetRepositoryResponseConverter());
			_options.Converters.Add(new BulkResponseItemConverter());
			_options.Converters.Add(new BulkRequestConverter(settings));
			_options.Converters.Add(new MultiGetRequestConverter(settings));
			_options.Converters.Add(new MultiSearchConverter(settings));

			// Batch 13: open-generic factories (proxy requests + response dictionaries + suggest dictionary) and the
			// per-field-analyzer dictionary. Proxy index/create requests go through the global serializer (verified via
			// the PostData.Serializable wire path), so global factory registration is correct here.
			_options.Converters.Add(new IndexRequestConverterFactory());
			_options.Converters.Add(new CreateRequestConverterFactory());
			_options.Converters.Add(new SuggestDictionaryConverterFactory());
			_options.Converters.Add(new DictionaryResponseConverterFactory(settings));
			_options.Converters.Add(new PerFieldAnalyzerConverter(settings));
			// dynamic_templates serialize as a JSON ARRAY of single-key objects [{name: tmpl}], not an object.
			// Registered before the generic IIsADictionary factory below so these win for IDynamicTemplateContainer.
			_options.Converters.Add(new DynamicTemplatesInterfaceConverter());
			_options.Converters.Add(new DynamicTemplatesConverter());
			// IIsADictionary interfaces (IAliases, IRelations, INormalizers, ...): a factory builds the
			// VerbatimDictionaryKeysConverter<TDictionary,TInterface,TKey,TValue> per interface from the legacy
			// [JsonFormatter] mapping. Without it, STJ's default dictionary handling cannot instantiate the abstract
			// interface and throws NotSupportedException.
			_options.Converters.Add(new VerbatimDictionaryKeysConverterFactory(settings));
			// Union<TFirst,TSecond>: type-level default in the legacy engine. Registered after the specific converters
			// (e.g. DistanceFeature handles its own Union<GeoCoordinate,DateMath>), so those win; every other closed
			// Union type falls through to this factory. Without it STJ cannot (de)serialize the concrete Union type.
			_options.Converters.Add(new UnionConverterFactory());

			// Batch 14: final unmigrated type-level formatters — Geo/term/span/specialized query wrappers,
			// aggregations, and the last requests / nested-dictionary responses.
			_options.Converters.Add(new GeoBoundingBoxQueryConverter(settings));
			_options.Converters.Add(new GeoDistanceQueryConverter(settings));
			_options.Converters.Add(new GeoPolygonQueryConverter(settings));
			_options.Converters.Add(new GeoShapeQueryConverter(settings));
			_options.Converters.Add(new ShapeQueryConverter(settings));
			_options.Converters.Add(new TermsQueryConverter(settings));
			_options.Converters.Add(new SpanGapQueryConverter(settings));
			_options.Converters.Add(new DistanceFeatureQueryConverter());
			_options.Converters.Add(new RankFeatureQueryConverter());
			_options.Converters.Add(new FilterAggregationConverter());
			_options.Converters.Add(new MovingAverageAggregationConverter());
			_options.Converters.Add(new PercentilesAggregationConverter(settings));
			_options.Converters.Add(new PercentileRanksAggregationConverter(settings));
			_options.Converters.Add(new CompositeAggregationSourceConverter());
			_options.Converters.Add(new CompositeKeyConverter());
			_options.Converters.Add(new MultiSearchTemplateConverter(settings));
			_options.Converters.Add(new UpdateIndexSettingsConverter());
			_options.Converters.Add(new FieldCapabilitiesFieldsConverter(settings));
			_options.Converters.Add(new IndicesStatsDictionaryConverter(settings));
			// ISortOrder (TermsOrder / HistogramOrder) serialize as a single-property object { "<key>": "<order>" }.
			// The converter exists but is an open generic; register the two closed types the legacy engine annotated
			// with a type-level [JsonFormatter(typeof(SortOrderFormatter<T>))].
			_options.Converters.Add(new SortOrderConverter<TermsOrder>());
			_options.Converters.Add(new SortOrderConverter<HistogramOrder>());
		}

		// An empty/absent response stream must deserialize to default (the legacy Utf8Json engine returned null for
		// empty input). System.Text.Json throws JsonException on empty input, so guard it — otherwise every empty or
		// missing response body throws "input does not contain any JSON tokens".
		private static bool IsEmpty(Stream stream) =>
			stream == null || stream == Stream.Null || (stream.CanSeek && stream.Length == 0);

		public T Deserialize<T>(Stream stream) =>
			IsEmpty(stream) ? default : JsonSerializer.Deserialize<T>(stream, _options);

		public object Deserialize(Type type, Stream stream) =>
			IsEmpty(stream) ? null : JsonSerializer.Deserialize(stream, type, _options);

		public async Task<T> DeserializeAsync<T>(Stream stream, CancellationToken cancellationToken = default) =>
			IsEmpty(stream) ? default : await JsonSerializer.DeserializeAsync<T>(stream, _options, cancellationToken).ConfigureAwait(false);

		public async Task<object> DeserializeAsync(Type type, Stream stream, CancellationToken cancellationToken = default) =>
			IsEmpty(stream) ? null : await JsonSerializer.DeserializeAsync(stream, type, _options, cancellationToken).ConfigureAwait(false);

		// Deserialize using a clone of the configured options with one additional per-request converter prepended so it
		// wins for its target type. Used by response builders (MultiGet/MultiSearch) that need a converter carrying the
		// originating request's document types — the STJ analogue of the legacy CreateStateful path.
		internal T DeserializeWithConverter<T>(System.Text.Json.Serialization.JsonConverter converter, Stream stream)
		{
			if (IsEmpty(stream))
				return default;

			var options = new JsonSerializerOptions(_options);
			options.Converters.Insert(0, converter);
			return JsonSerializer.Deserialize<T>(stream, options);
		}

		internal async Task<T> DeserializeWithConverterAsync<T>(
			System.Text.Json.Serialization.JsonConverter converter, Stream stream, CancellationToken cancellationToken = default)
		{
			if (IsEmpty(stream))
				return default;

			var options = new JsonSerializerOptions(_options);
			options.Converters.Insert(0, converter);
			return await JsonSerializer.DeserializeAsync<T>(stream, options, cancellationToken).ConfigureAwait(false);
		}

		public void Serialize<T>(T data, Stream stream, SerializationFormatting formatting = SerializationFormatting.None) =>
			JsonSerializer.Serialize(stream, data, _options);

		public Task SerializeAsync<T>(T data, Stream stream, SerializationFormatting formatting = SerializationFormatting.None,
			CancellationToken cancellationToken = default) =>
			JsonSerializer.SerializeAsync(stream, data, _options, cancellationToken);
	}
}
