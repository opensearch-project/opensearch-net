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
	[MapsApi("search_pipeline.put.json")]
	public partial interface IPutSearchPipelineRequest : ISearchPipeline { }

	public partial class PutSearchPipelineRequest
	{
		public string Description { get; set; }
		public int? Version { get; set; }
		public IEnumerable<IRequestProcessor> RequestProcessors { get; set; }
		public IEnumerable<IResponseProcessor> ResponseProcessors { get; set; }
		public IEnumerable<IPhaseResultsProcessor> PhaseResultsProcessors { get; set; }
	}

	public partial class PutSearchPipelineDescriptor
	{
		string ISearchPipeline.Description { get; set; }
		int? ISearchPipeline.Version { get; set; }
		IEnumerable<IRequestProcessor> ISearchPipeline.RequestProcessors { get; set; }
		IEnumerable<IResponseProcessor> ISearchPipeline.ResponseProcessors { get; set; }
		IEnumerable<IPhaseResultsProcessor> ISearchPipeline.PhaseResultsProcessors { get; set; }

		/// <inheritdoc cref="ISearchPipeline.Description"/>
		public PutSearchPipelineDescriptor Description(string description) =>
			Assign(description, (a, v) => a.Description = v);

		/// <inheritdoc cref="ISearchPipeline.Version"/>
		public PutSearchPipelineDescriptor Version(int? version) =>
			Assign(version, (a, v) => a.Version = v);

		/// <inheritdoc cref="ISearchPipeline.RequestProcessors"/>
		public PutSearchPipelineDescriptor RequestProcessors(
			IEnumerable<IRequestProcessor> processors) =>
			Assign(processors.ToListOrNullIfEmpty(), (a, v) => a.RequestProcessors = v);

		/// <inheritdoc cref="ISearchPipeline.RequestProcessors"/>
		public PutSearchPipelineDescriptor RequestProcessors(
			Func<RequestProcessorsDescriptor, IPromise<IList<IRequestProcessor>>> selector) =>
			Assign(selector, (a, v) =>
				a.RequestProcessors = v?.Invoke(new RequestProcessorsDescriptor())?.Value);

		/// <inheritdoc cref="ISearchPipeline.ResponseProcessors"/>
		public PutSearchPipelineDescriptor ResponseProcessors(
			IEnumerable<IResponseProcessor> processors) =>
			Assign(processors.ToListOrNullIfEmpty(), (a, v) => a.ResponseProcessors = v);

		/// <inheritdoc cref="ISearchPipeline.ResponseProcessors"/>
		public PutSearchPipelineDescriptor ResponseProcessors(
			Func<ResponseProcessorsDescriptor, IPromise<IList<IResponseProcessor>>> selector) =>
			Assign(selector, (a, v) =>
				a.ResponseProcessors = v?.Invoke(new ResponseProcessorsDescriptor())?.Value);

		/// <inheritdoc cref="ISearchPipeline.PhaseResultsProcessors"/>
		public PutSearchPipelineDescriptor PhaseResultsProcessors(
			IEnumerable<IPhaseResultsProcessor> processors) =>
			Assign(processors.ToListOrNullIfEmpty(), (a, v) => a.PhaseResultsProcessors = v);

		/// <inheritdoc cref="ISearchPipeline.PhaseResultsProcessors"/>
		public PutSearchPipelineDescriptor PhaseResultsProcessors(
			Func<PhaseResultsProcessorsDescriptor, IPromise<IList<IPhaseResultsProcessor>>> selector) =>
			Assign(selector, (a, v) =>
				a.PhaseResultsProcessors = v?.Invoke(new PhaseResultsProcessorsDescriptor())?.Value);
	}
}
