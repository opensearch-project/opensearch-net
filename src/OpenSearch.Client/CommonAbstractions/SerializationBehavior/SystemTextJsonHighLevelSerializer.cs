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
		}

		public T Deserialize<T>(Stream stream) => JsonSerializer.Deserialize<T>(stream, _options);

		public object Deserialize(Type type, Stream stream) => JsonSerializer.Deserialize(stream, type, _options);

		public async Task<T> DeserializeAsync<T>(Stream stream, CancellationToken cancellationToken = default) =>
			await JsonSerializer.DeserializeAsync<T>(stream, _options, cancellationToken).ConfigureAwait(false);

		public async Task<object> DeserializeAsync(Type type, Stream stream, CancellationToken cancellationToken = default) =>
			await JsonSerializer.DeserializeAsync(stream, type, _options, cancellationToken).ConfigureAwait(false);

		public void Serialize<T>(T data, Stream stream, SerializationFormatting formatting = SerializationFormatting.None) =>
			JsonSerializer.Serialize(stream, data, _options);

		public Task SerializeAsync<T>(T data, Stream stream, SerializationFormatting formatting = SerializationFormatting.None,
			CancellationToken cancellationToken = default) =>
			JsonSerializer.SerializeAsync(stream, data, _options, cancellationToken);
	}
}
