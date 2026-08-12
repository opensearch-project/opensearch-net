/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using OpenSearch.Net.Utf8Json;

namespace OpenSearch.Client
{
	/// <summary>A search pipeline definition.</summary>
	[InterfaceDataContract]
	[ReadAs(typeof(SearchPipeline))]
	public interface ISearchPipeline
	{
		/// <summary>A description of the search pipeline.</summary>
		[DataMember(Name = "description")]
		string Description { get; set; }

		/// <summary>A version number used to manage the pipeline.</summary>
		[DataMember(Name = "version")]
		int? Version { get; set; }

		/// <summary>Processors that run before a search request.</summary>
		[DataMember(Name = "request_processors")]
		IEnumerable<IRequestProcessor> RequestProcessors { get; set; }

		/// <summary>Processors that run after a search response.</summary>
		[DataMember(Name = "response_processors")]
		IEnumerable<IResponseProcessor> ResponseProcessors { get; set; }

		/// <summary>Processors that run between query and fetch phases.</summary>
		[DataMember(Name = "phase_results_processors")]
		IEnumerable<IPhaseResultsProcessor> PhaseResultsProcessors { get; set; }
	}

	/// <inheritdoc cref="ISearchPipeline"/>
	public class SearchPipeline : ISearchPipeline
	{
		/// <inheritdoc />
		public string Description { get; set; }
		/// <inheritdoc />
		public int? Version { get; set; }
		/// <inheritdoc />
		public IEnumerable<IRequestProcessor> RequestProcessors { get; set; }
		/// <inheritdoc />
		public IEnumerable<IResponseProcessor> ResponseProcessors { get; set; }
		/// <inheritdoc />
		public IEnumerable<IPhaseResultsProcessor> PhaseResultsProcessors { get; set; }
	}

	/// <inheritdoc cref="ISearchPipeline"/>
	public class SearchPipelineDescriptor : DescriptorBase<SearchPipelineDescriptor, ISearchPipeline>, ISearchPipeline
	{
		string ISearchPipeline.Description { get; set; }
		int? ISearchPipeline.Version { get; set; }
		IEnumerable<IRequestProcessor> ISearchPipeline.RequestProcessors { get; set; }
		IEnumerable<IResponseProcessor> ISearchPipeline.ResponseProcessors { get; set; }
		IEnumerable<IPhaseResultsProcessor> ISearchPipeline.PhaseResultsProcessors { get; set; }

		/// <inheritdoc cref="ISearchPipeline.Description"/>
		public SearchPipelineDescriptor Description(string description) =>
			Assign(description, (a, v) => a.Description = v);

		/// <inheritdoc cref="ISearchPipeline.Version"/>
		public SearchPipelineDescriptor Version(int? version) =>
			Assign(version, (a, v) => a.Version = v);

		/// <inheritdoc cref="ISearchPipeline.RequestProcessors"/>
		public SearchPipelineDescriptor RequestProcessors(IEnumerable<IRequestProcessor> processors) =>
			Assign(processors.ToListOrNullIfEmpty(), (a, v) => a.RequestProcessors = v);

		/// <inheritdoc cref="ISearchPipeline.RequestProcessors"/>
		public SearchPipelineDescriptor RequestProcessors(
			Func<RequestProcessorsDescriptor, IPromise<IList<IRequestProcessor>>> selector) =>
			Assign(selector, (a, v) => a.RequestProcessors = v?.Invoke(new RequestProcessorsDescriptor())?.Value);

		/// <inheritdoc cref="ISearchPipeline.ResponseProcessors"/>
		public SearchPipelineDescriptor ResponseProcessors(IEnumerable<IResponseProcessor> processors) =>
			Assign(processors.ToListOrNullIfEmpty(), (a, v) => a.ResponseProcessors = v);

		/// <inheritdoc cref="ISearchPipeline.ResponseProcessors"/>
		public SearchPipelineDescriptor ResponseProcessors(
			Func<ResponseProcessorsDescriptor, IPromise<IList<IResponseProcessor>>> selector) =>
			Assign(selector, (a, v) => a.ResponseProcessors = v?.Invoke(new ResponseProcessorsDescriptor())?.Value);

		/// <inheritdoc cref="ISearchPipeline.PhaseResultsProcessors"/>
		public SearchPipelineDescriptor PhaseResultsProcessors(IEnumerable<IPhaseResultsProcessor> processors) =>
			Assign(processors.ToListOrNullIfEmpty(), (a, v) => a.PhaseResultsProcessors = v);

		/// <inheritdoc cref="ISearchPipeline.PhaseResultsProcessors"/>
		public SearchPipelineDescriptor PhaseResultsProcessors(
			Func<PhaseResultsProcessorsDescriptor, IPromise<IList<IPhaseResultsProcessor>>> selector) =>
			Assign(selector, (a, v) => a.PhaseResultsProcessors = v?.Invoke(new PhaseResultsProcessorsDescriptor())?.Value);
	}
}
