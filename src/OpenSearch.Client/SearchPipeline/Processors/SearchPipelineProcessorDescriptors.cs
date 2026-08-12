/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Collections.Generic;

namespace OpenSearch.Client
{
	/// <summary>Fluent builder for a list of search pipeline request processors.</summary>
	public class RequestProcessorsDescriptor
		: DescriptorPromiseBase<RequestProcessorsDescriptor, IList<IRequestProcessor>>
	{
		public RequestProcessorsDescriptor() : base(new List<IRequestProcessor>()) { }

		/// <inheritdoc cref="INeuralQueryEnricherRequestProcessor"/>
		public RequestProcessorsDescriptor NeuralQueryEnricher(
			Func<NeuralQueryEnricherRequestProcessorDescriptor, INeuralQueryEnricherRequestProcessor> selector) =>
			Assign(selector, (a, v) =>
				a.AddIfNotNull(v?.Invoke(new NeuralQueryEnricherRequestProcessorDescriptor())));

		/// <inheritdoc cref="IFilterQueryRequestProcessor"/>
		public RequestProcessorsDescriptor FilterQuery(
			Func<FilterQueryRequestProcessorDescriptor, IFilterQueryRequestProcessor> selector) =>
			Assign(selector, (a, v) =>
				a.AddIfNotNull(v?.Invoke(new FilterQueryRequestProcessorDescriptor())));

		/// <inheritdoc cref="ISearchScriptRequestProcessor"/>
		public RequestProcessorsDescriptor Script(
			Func<SearchScriptRequestProcessorDescriptor, ISearchScriptRequestProcessor> selector) =>
			Assign(selector, (a, v) =>
				a.AddIfNotNull(v?.Invoke(new SearchScriptRequestProcessorDescriptor())));

		/// <inheritdoc cref="IOversampleRequestProcessor"/>
		public RequestProcessorsDescriptor Oversample(
			Func<OversampleRequestProcessorDescriptor, IOversampleRequestProcessor> selector) =>
			Assign(selector, (a, v) =>
				a.AddIfNotNull(v?.Invoke(new OversampleRequestProcessorDescriptor())));

		/// <inheritdoc cref="IAgenticQueryTranslatorRequestProcessor"/>
		public RequestProcessorsDescriptor AgenticQueryTranslator(
			Func<AgenticQueryTranslatorRequestProcessorDescriptor,
				IAgenticQueryTranslatorRequestProcessor> selector) =>
			Assign(selector, (a, v) =>
				a.AddIfNotNull(v?.Invoke(new AgenticQueryTranslatorRequestProcessorDescriptor())));
	}

	/// <summary>Fluent builder for a list of search pipeline response processors.</summary>
	public class ResponseProcessorsDescriptor
		: DescriptorPromiseBase<ResponseProcessorsDescriptor, IList<IResponseProcessor>>
	{
		public ResponseProcessorsDescriptor() : base(new List<IResponseProcessor>()) { }

		/// <inheritdoc cref="IRetrievalAugmentedGenerationResponseProcessor"/>
		public ResponseProcessorsDescriptor RetrievalAugmentedGeneration(
			Func<RetrievalAugmentedGenerationResponseProcessorDescriptor,
				IRetrievalAugmentedGenerationResponseProcessor> selector) =>
			Assign(selector, (a, v) =>
				a.AddIfNotNull(v?.Invoke(new RetrievalAugmentedGenerationResponseProcessorDescriptor())));

		/// <inheritdoc cref="IRerankResponseProcessor"/>
		public ResponseProcessorsDescriptor Rerank(
			Func<RerankResponseProcessorDescriptor, IRerankResponseProcessor> selector) =>
			Assign(selector, (a, v) =>
				a.AddIfNotNull(v?.Invoke(new RerankResponseProcessorDescriptor())));

		/// <inheritdoc cref="IRenameFieldResponseProcessor"/>
		public ResponseProcessorsDescriptor RenameField(
			Func<RenameFieldResponseProcessorDescriptor, IRenameFieldResponseProcessor> selector) =>
			Assign(selector, (a, v) =>
				a.AddIfNotNull(v?.Invoke(new RenameFieldResponseProcessorDescriptor())));

		/// <inheritdoc cref="ITruncateHitsResponseProcessor"/>
		public ResponseProcessorsDescriptor TruncateHits(
			Func<TruncateHitsResponseProcessorDescriptor, ITruncateHitsResponseProcessor> selector) =>
			Assign(selector, (a, v) =>
				a.AddIfNotNull(v?.Invoke(new TruncateHitsResponseProcessorDescriptor())));

		/// <inheritdoc cref="ISortResponseProcessor"/>
		public ResponseProcessorsDescriptor Sort(
			Func<SortResponseProcessorDescriptor, ISortResponseProcessor> selector) =>
			Assign(selector, (a, v) =>
				a.AddIfNotNull(v?.Invoke(new SortResponseProcessorDescriptor())));

		/// <inheritdoc cref="ISplitResponseProcessor"/>
		public ResponseProcessorsDescriptor Split(
			Func<SplitResponseProcessorDescriptor, ISplitResponseProcessor> selector) =>
			Assign(selector, (a, v) =>
				a.AddIfNotNull(v?.Invoke(new SplitResponseProcessorDescriptor())));

		/// <inheritdoc cref="ICollapseResponseProcessor"/>
		public ResponseProcessorsDescriptor Collapse(
			Func<CollapseResponseProcessorDescriptor, ICollapseResponseProcessor> selector) =>
			Assign(selector, (a, v) =>
				a.AddIfNotNull(v?.Invoke(new CollapseResponseProcessorDescriptor())));

		/// <inheritdoc cref="IPersonalizeSearchRankingResponseProcessor"/>
		public ResponseProcessorsDescriptor PersonalizeSearchRanking(
			Func<PersonalizeSearchRankingResponseProcessorDescriptor,
				IPersonalizeSearchRankingResponseProcessor> selector) =>
			Assign(selector, (a, v) =>
				a.AddIfNotNull(v?.Invoke(new PersonalizeSearchRankingResponseProcessorDescriptor())));

		/// <inheritdoc cref="IAgenticContextResponseProcessor"/>
		public ResponseProcessorsDescriptor AgenticContext(
			Func<AgenticContextResponseProcessorDescriptor, IAgenticContextResponseProcessor> selector) =>
			Assign(selector, (a, v) =>
				a.AddIfNotNull(v?.Invoke(new AgenticContextResponseProcessorDescriptor())));
	}

	/// <summary>Fluent builder for a list of search pipeline phase-results processors.</summary>
	public class PhaseResultsProcessorsDescriptor
		: DescriptorPromiseBase<PhaseResultsProcessorsDescriptor, IList<IPhaseResultsProcessor>>
	{
		public PhaseResultsProcessorsDescriptor() : base(new List<IPhaseResultsProcessor>()) { }

		/// <inheritdoc cref="INormalizationPhaseResultsProcessor"/>
		public PhaseResultsProcessorsDescriptor Normalization(
			Func<NormalizationPhaseResultsProcessorDescriptor, INormalizationPhaseResultsProcessor> selector) =>
			Assign(selector, (a, v) =>
				a.AddIfNotNull(v?.Invoke(new NormalizationPhaseResultsProcessorDescriptor())));

		/// <inheritdoc cref="IScoreRankerPhaseResultsProcessor"/>
		public PhaseResultsProcessorsDescriptor ScoreRanker(
			Func<ScoreRankerPhaseResultsProcessorDescriptor, IScoreRankerPhaseResultsProcessor> selector) =>
			Assign(selector, (a, v) =>
				a.AddIfNotNull(v?.Invoke(new ScoreRankerPhaseResultsProcessorDescriptor())));
	}
}
