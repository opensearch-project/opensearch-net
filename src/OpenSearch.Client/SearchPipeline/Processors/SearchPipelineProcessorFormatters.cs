/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using OpenSearch.Net.Utf8Json;
using OpenSearch.Net.Utf8Json.Internal;
using OpenSearch.Net.Utf8Json.Resolvers;

namespace OpenSearch.Client
{
	internal class RequestProcessorFormatter : IJsonFormatter<IRequestProcessor>
	{
		private static readonly AutomataDictionary Processors = new AutomataDictionary
		{
			{ "neural_query_enricher",    0 },
			{ "filter_query",             1 },
			{ "script",                   2 },
			{ "oversample",               3 },
			{ "agentic_query_translator", 4 },
		};

		public IRequestProcessor Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
		{
			if (reader.GetCurrentJsonToken() != JsonToken.BeginObject)
			{
				reader.ReadNextBlock();
				return null;
			}

			// read opening {
			reader.ReadNext();

			IRequestProcessor processor = null;

			var processorName = reader.ReadPropertyNameSegmentRaw();
			if (Processors.TryGetValue(processorName, out var value))
			{
				processor = value switch
				{
					0 => Deserialize<NeuralQueryEnricherRequestProcessor>(ref reader, formatterResolver),
					1 => Deserialize<FilterQueryRequestProcessor>(ref reader, formatterResolver),
					2 => Deserialize<SearchScriptRequestProcessor>(ref reader, formatterResolver),
					3 => Deserialize<OversampleRequestProcessor>(ref reader, formatterResolver),
					4 => Deserialize<AgenticQueryTranslatorRequestProcessor>(ref reader, formatterResolver),
					_ => null
				};
			}
			else
				reader.ReadNextBlock();

			reader.ReadIsEndObjectWithVerify();
			return processor;
		}

		public void Serialize(ref JsonWriter writer, IRequestProcessor value, IJsonFormatterResolver formatterResolver)
		{
			if (value?.Name == null) { writer.WriteNull(); return; }

			writer.WriteBeginObject();
			writer.WritePropertyName(value.Name);

			switch (value.Name)
			{
				case "neural_query_enricher":
					Serialize<INeuralQueryEnricherRequestProcessor>(ref writer, value, formatterResolver); break;
				case "filter_query":
					Serialize<IFilterQueryRequestProcessor>(ref writer, value, formatterResolver); break;
				case "script":
					Serialize<ISearchScriptRequestProcessor>(ref writer, value, formatterResolver); break;
				case "oversample":
					Serialize<IOversampleRequestProcessor>(ref writer, value, formatterResolver); break;
				case "agentic_query_translator":
					Serialize<IAgenticQueryTranslatorRequestProcessor>(ref writer, value, formatterResolver); break;
				default:
					DynamicObjectResolver.ExcludeNullCamelCase.GetFormatter<IRequestProcessor>()
						.Serialize(ref writer, value, formatterResolver); break;
			}

			writer.WriteEndObject();
		}

		private static T Deserialize<T>(ref JsonReader reader, IJsonFormatterResolver resolver)
			where T : IRequestProcessor =>
			resolver.GetFormatter<T>().Deserialize(ref reader, resolver);

		private static void Serialize<T>(ref JsonWriter writer, IRequestProcessor value,
			IJsonFormatterResolver resolver) where T : class, IRequestProcessor =>
			resolver.GetFormatter<T>().Serialize(ref writer, value as T, resolver);
	}

	internal class ResponseProcessorFormatter : IJsonFormatter<IResponseProcessor>
	{
		private static readonly AutomataDictionary Processors = new AutomataDictionary
		{
			{ "retrieval_augmented_generation", 0 },
			{ "rerank",                         1 },
			{ "rename_field",                   2 },
			{ "truncate_hits",                  3 },
			{ "sort",                           4 },
			{ "split",                          5 },
			{ "collapse",                       6 },
			{ "personalize_search_ranking",     7 },
			{ "agentic_context",                8 },
		};

		public IResponseProcessor Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
		{
			if (reader.GetCurrentJsonToken() != JsonToken.BeginObject)
			{
				reader.ReadNextBlock();
				return null;
			}

			reader.ReadNext();
			IResponseProcessor processor = null;

			var processorName = reader.ReadPropertyNameSegmentRaw();
			if (Processors.TryGetValue(processorName, out var value))
			{
				processor = value switch
				{
					0 => Deserialize<RetrievalAugmentedGenerationResponseProcessor>(ref reader, formatterResolver),
					1 => Deserialize<RerankResponseProcessor>(ref reader, formatterResolver),
					2 => Deserialize<RenameFieldResponseProcessor>(ref reader, formatterResolver),
					3 => Deserialize<TruncateHitsResponseProcessor>(ref reader, formatterResolver),
					4 => Deserialize<SortResponseProcessor>(ref reader, formatterResolver),
					5 => Deserialize<SplitResponseProcessor>(ref reader, formatterResolver),
					6 => Deserialize<CollapseResponseProcessor>(ref reader, formatterResolver),
					7 => Deserialize<PersonalizeSearchRankingResponseProcessor>(ref reader, formatterResolver),
					8 => Deserialize<AgenticContextResponseProcessor>(ref reader, formatterResolver),
					_ => null
				};
			}
			else
				reader.ReadNextBlock();

			reader.ReadIsEndObjectWithVerify();
			return processor;
		}

		public void Serialize(ref JsonWriter writer, IResponseProcessor value, IJsonFormatterResolver formatterResolver)
		{
			if (value?.Name == null) { writer.WriteNull(); return; }

			writer.WriteBeginObject();
			writer.WritePropertyName(value.Name);

			switch (value.Name)
			{
				case "retrieval_augmented_generation":
					Serialize<IRetrievalAugmentedGenerationResponseProcessor>(ref writer, value, formatterResolver); break;
				case "rerank":
					Serialize<IRerankResponseProcessor>(ref writer, value, formatterResolver); break;
				case "rename_field":
					Serialize<IRenameFieldResponseProcessor>(ref writer, value, formatterResolver); break;
				case "truncate_hits":
					Serialize<ITruncateHitsResponseProcessor>(ref writer, value, formatterResolver); break;
				case "sort":
					Serialize<ISortResponseProcessor>(ref writer, value, formatterResolver); break;
				case "split":
					Serialize<ISplitResponseProcessor>(ref writer, value, formatterResolver); break;
				case "collapse":
					Serialize<ICollapseResponseProcessor>(ref writer, value, formatterResolver); break;
				case "personalize_search_ranking":
					Serialize<IPersonalizeSearchRankingResponseProcessor>(ref writer, value, formatterResolver); break;
				case "agentic_context":
					Serialize<IAgenticContextResponseProcessor>(ref writer, value, formatterResolver); break;
				default:
					DynamicObjectResolver.ExcludeNullCamelCase.GetFormatter<IResponseProcessor>()
						.Serialize(ref writer, value, formatterResolver); break;
			}

			writer.WriteEndObject();
		}

		private static T Deserialize<T>(ref JsonReader reader, IJsonFormatterResolver resolver)
			where T : IResponseProcessor =>
			resolver.GetFormatter<T>().Deserialize(ref reader, resolver);

		private static void Serialize<T>(ref JsonWriter writer, IResponseProcessor value,
			IJsonFormatterResolver resolver) where T : class, IResponseProcessor =>
			resolver.GetFormatter<T>().Serialize(ref writer, value as T, resolver);
	}

	internal class PhaseResultsProcessorFormatter : IJsonFormatter<IPhaseResultsProcessor>
	{
		private static readonly AutomataDictionary Processors = new AutomataDictionary
		{
			{ "normalization-processor", 0 },
			{ "score-ranker-processor",  1 },
		};

		public IPhaseResultsProcessor Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
		{
			if (reader.GetCurrentJsonToken() != JsonToken.BeginObject)
			{
				reader.ReadNextBlock();
				return null;
			}

			reader.ReadNext();
			IPhaseResultsProcessor processor = null;

			var processorName = reader.ReadPropertyNameSegmentRaw();
			if (Processors.TryGetValue(processorName, out var value))
			{
				processor = value switch
				{
					0 => Deserialize<NormalizationPhaseResultsProcessor>(ref reader, formatterResolver),
					1 => Deserialize<ScoreRankerPhaseResultsProcessor>(ref reader, formatterResolver),
					_ => null
				};
			}
			else
				reader.ReadNextBlock();

			reader.ReadIsEndObjectWithVerify();
			return processor;
		}

		public void Serialize(ref JsonWriter writer, IPhaseResultsProcessor value,
			IJsonFormatterResolver formatterResolver)
		{
			if (value?.Name == null) { writer.WriteNull(); return; }

			writer.WriteBeginObject();
			writer.WritePropertyName(value.Name);

			switch (value.Name)
			{
				case "normalization-processor":
					Serialize<INormalizationPhaseResultsProcessor>(ref writer, value, formatterResolver); break;
				case "score-ranker-processor":
					Serialize<IScoreRankerPhaseResultsProcessor>(ref writer, value, formatterResolver); break;
				default:
					DynamicObjectResolver.ExcludeNullCamelCase.GetFormatter<IPhaseResultsProcessor>()
						.Serialize(ref writer, value, formatterResolver); break;
			}

			writer.WriteEndObject();
		}

		private static T Deserialize<T>(ref JsonReader reader, IJsonFormatterResolver resolver)
			where T : IPhaseResultsProcessor =>
			resolver.GetFormatter<T>().Deserialize(ref reader, resolver);

		private static void Serialize<T>(ref JsonWriter writer, IPhaseResultsProcessor value,
			IJsonFormatterResolver resolver) where T : class, IPhaseResultsProcessor =>
			resolver.GetFormatter<T>().Serialize(ref writer, value as T, resolver);
	}
}
