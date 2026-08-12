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
	// ──────────────────────────────────────────────────────────────────────────
	// Base
	// ──────────────────────────────────────────────────────────────────────────

	/// <summary>Base interface for all search pipeline request processors.</summary>
	[JsonFormatter(typeof(RequestProcessorFormatter))]
	public interface IRequestProcessor
	{
		/// <summary>The processor name key used for serialization.</summary>
		string Name { get; }

		/// <summary>A tag for the processor, used for error messages and debugging.</summary>
		[DataMember(Name = "tag")]
		string Tag { get; set; }

		/// <summary>A description of the processor.</summary>
		[DataMember(Name = "description")]
		string Description { get; set; }

		/// <summary>Whether to ignore failures.</summary>
		[DataMember(Name = "ignore_failure")]
		bool? IgnoreFailure { get; set; }
	}

	/// <inheritdoc cref="IRequestProcessor"/>
	public abstract class RequestProcessorBase : IRequestProcessor
	{
		/// <inheritdoc />
		public string Tag { get; set; }
		/// <inheritdoc />
		public string Description { get; set; }
		/// <inheritdoc />
		public bool? IgnoreFailure { get; set; }

		protected abstract string Name { get; }
		string IRequestProcessor.Name => Name;
	}

	/// <inheritdoc cref="IRequestProcessor"/>
	public abstract class RequestProcessorDescriptorBase<TDescriptor, TInterface>
		: DescriptorBase<TDescriptor, TInterface>, IRequestProcessor
		where TDescriptor : RequestProcessorDescriptorBase<TDescriptor, TInterface>, TInterface
		where TInterface : class, IRequestProcessor
	{
		string IRequestProcessor.Name => null;
		string IRequestProcessor.Tag { get; set; }
		string IRequestProcessor.Description { get; set; }
		bool? IRequestProcessor.IgnoreFailure { get; set; }

		/// <inheritdoc cref="IRequestProcessor.Tag"/>
		public TDescriptor Tag(string tag) => Assign(tag, (a, v) => a.Tag = v);

		/// <inheritdoc cref="IRequestProcessor.Description"/>
		public TDescriptor Description(string description) => Assign(description, (a, v) => a.Description = v);

		/// <inheritdoc cref="IRequestProcessor.IgnoreFailure"/>
		public TDescriptor IgnoreFailure(bool? ignoreFailure = true) =>
			Assign(ignoreFailure, (a, v) => a.IgnoreFailure = v);
	}

	// ──────────────────────────────────────────────────────────────────────────
	// neural_query_enricher
	// ──────────────────────────────────────────────────────────────────────────

	/// <summary>
	/// The <c>neural_query_enricher</c> request processor sets default model IDs for neural queries.
	/// </summary>
	[InterfaceDataContract]
	public interface INeuralQueryEnricherRequestProcessor : IRequestProcessor
	{
		/// <summary>The default model ID to use for neural queries that do not specify a model ID.</summary>
		[DataMember(Name = "default_model_id")]
		string DefaultModelId { get; set; }

		/// <summary>
		/// A mapping from field name to model ID. Overrides <see cref="DefaultModelId"/> for
		/// specific fields. Serializes as <c>"neural_field_default_id"</c>.
		/// </summary>
		[DataMember(Name = "neural_field_default_id")]
		IDictionary<string, string> NeuralFieldDefaultId { get; set; }
	}

	/// <inheritdoc cref="INeuralQueryEnricherRequestProcessor"/>
	public class NeuralQueryEnricherRequestProcessor
		: RequestProcessorBase, INeuralQueryEnricherRequestProcessor
	{
		protected override string Name => "neural_query_enricher";
		/// <inheritdoc />
		public string DefaultModelId { get; set; }
		/// <inheritdoc />
		public IDictionary<string, string> NeuralFieldDefaultId { get; set; }
	}

	/// <inheritdoc cref="INeuralQueryEnricherRequestProcessor"/>
	public class NeuralQueryEnricherRequestProcessorDescriptor
		: RequestProcessorDescriptorBase<NeuralQueryEnricherRequestProcessorDescriptor,
			INeuralQueryEnricherRequestProcessor>,
		  INeuralQueryEnricherRequestProcessor
	{
		string INeuralQueryEnricherRequestProcessor.DefaultModelId { get; set; }
		IDictionary<string, string> INeuralQueryEnricherRequestProcessor.NeuralFieldDefaultId { get; set; }

		/// <inheritdoc cref="INeuralQueryEnricherRequestProcessor.DefaultModelId"/>
		public NeuralQueryEnricherRequestProcessorDescriptor DefaultModelId(string modelId) =>
			Assign(modelId, (a, v) => a.DefaultModelId = v);

		/// <inheritdoc cref="INeuralQueryEnricherRequestProcessor.NeuralFieldDefaultId"/>
		public NeuralQueryEnricherRequestProcessorDescriptor NeuralFieldDefaultId(
			IDictionary<string, string> fieldModelMap) =>
			Assign(fieldModelMap, (a, v) => a.NeuralFieldDefaultId = v);
	}

	// ──────────────────────────────────────────────────────────────────────────
	// filter_query
	// ──────────────────────────────────────────────────────────────────────────

	/// <summary>The <c>filter_query</c> request processor filters search results using a query.</summary>
	[InterfaceDataContract]
	public interface IFilterQueryRequestProcessor : IRequestProcessor
	{
		/// <summary>The query to use to filter results.</summary>
		[DataMember(Name = "query")]
		QueryContainer Query { get; set; }
	}

	/// <inheritdoc cref="IFilterQueryRequestProcessor"/>
	public class FilterQueryRequestProcessor : RequestProcessorBase, IFilterQueryRequestProcessor
	{
		protected override string Name => "filter_query";
		/// <inheritdoc />
		public QueryContainer Query { get; set; }
	}

	/// <inheritdoc cref="IFilterQueryRequestProcessor"/>
	public class FilterQueryRequestProcessorDescriptor
		: RequestProcessorDescriptorBase<FilterQueryRequestProcessorDescriptor,
			IFilterQueryRequestProcessor>,
		  IFilterQueryRequestProcessor
	{
		QueryContainer IFilterQueryRequestProcessor.Query { get; set; }

		/// <inheritdoc cref="IFilterQueryRequestProcessor.Query"/>
		public FilterQueryRequestProcessorDescriptor Query(QueryContainer query) =>
			Assign(query, (a, v) => a.Query = v);

		/// <inheritdoc cref="IFilterQueryRequestProcessor.Query"/>
		public FilterQueryRequestProcessorDescriptor Query<T>(
			Func<QueryContainerDescriptor<T>, QueryContainer> selector) where T : class =>
			Assign(selector?.Invoke(new QueryContainerDescriptor<T>()), (a, v) => a.Query = v);
	}

	// ──────────────────────────────────────────────────────────────────────────
	// script
	// ──────────────────────────────────────────────────────────────────────────

	/// <summary>The <c>script</c> request processor runs a script against the search request.</summary>
	[InterfaceDataContract]
	public interface ISearchScriptRequestProcessor : IRequestProcessor
	{
		/// <summary>The script source.</summary>
		[DataMember(Name = "source")]
		string Source { get; set; }

		/// <summary>The script language. Defaults to Painless.</summary>
		[DataMember(Name = "lang")]
		string Lang { get; set; }
	}

	/// <inheritdoc cref="ISearchScriptRequestProcessor"/>
	public class SearchScriptRequestProcessor : RequestProcessorBase, ISearchScriptRequestProcessor
	{
		protected override string Name => "script";
		/// <inheritdoc />
		public string Source { get; set; }
		/// <inheritdoc />
		public string Lang { get; set; }
	}

	/// <inheritdoc cref="ISearchScriptRequestProcessor"/>
	public class SearchScriptRequestProcessorDescriptor
		: RequestProcessorDescriptorBase<SearchScriptRequestProcessorDescriptor,
			ISearchScriptRequestProcessor>,
		  ISearchScriptRequestProcessor
	{
		string ISearchScriptRequestProcessor.Source { get; set; }
		string ISearchScriptRequestProcessor.Lang { get; set; }

		/// <inheritdoc cref="ISearchScriptRequestProcessor.Source"/>
		public SearchScriptRequestProcessorDescriptor Source(string source) =>
			Assign(source, (a, v) => a.Source = v);

		/// <inheritdoc cref="ISearchScriptRequestProcessor.Lang"/>
		public SearchScriptRequestProcessorDescriptor Lang(string lang) =>
			Assign(lang, (a, v) => a.Lang = v);
	}

	// ──────────────────────────────────────────────────────────────────────────
	// oversample
	// ──────────────────────────────────────────────────────────────────────────

	/// <summary>The <c>oversample</c> request processor increases the requested document count.</summary>
	[InterfaceDataContract]
	public interface IOversampleRequestProcessor : IRequestProcessor
	{
		/// <summary>
		/// The factor by which to multiply the requested document count.
		/// For example, a factor of <c>1.5</c> returns 150% of the requested number of documents.
		/// </summary>
		[DataMember(Name = "sample_factor")]
		float SampleFactor { get; set; }

		/// <summary>
		/// A prefix for the context variable that holds the original requested size so that
		/// downstream response processors can restore it.
		/// </summary>
		[DataMember(Name = "content_prefix")]
		string ContentPrefix { get; set; }
	}

	/// <inheritdoc cref="IOversampleRequestProcessor"/>
	public class OversampleRequestProcessor : RequestProcessorBase, IOversampleRequestProcessor
	{
		protected override string Name => "oversample";
		/// <inheritdoc />
		public float SampleFactor { get; set; }
		/// <inheritdoc />
		public string ContentPrefix { get; set; }
	}

	/// <inheritdoc cref="IOversampleRequestProcessor"/>
	public class OversampleRequestProcessorDescriptor
		: RequestProcessorDescriptorBase<OversampleRequestProcessorDescriptor,
			IOversampleRequestProcessor>,
		  IOversampleRequestProcessor
	{
		float IOversampleRequestProcessor.SampleFactor { get; set; }
		string IOversampleRequestProcessor.ContentPrefix { get; set; }

		/// <inheritdoc cref="IOversampleRequestProcessor.SampleFactor"/>
		public OversampleRequestProcessorDescriptor SampleFactor(float? factor) =>
			Assign(factor, (a, v) => a.SampleFactor = v ?? 0);

		/// <inheritdoc cref="IOversampleRequestProcessor.ContentPrefix"/>
		public OversampleRequestProcessorDescriptor ContentPrefix(string prefix) =>
			Assign(prefix, (a, v) => a.ContentPrefix = v);
	}

	// ──────────────────────────────────────────────────────────────────────────
	// agentic_query_translator (3.2+)
	// ──────────────────────────────────────────────────────────────────────────

	/// <summary>
	/// The <c>agentic_query_translator</c> request processor uses an agent to translate
	/// a natural-language query into a structured OpenSearch query.
	/// </summary>
	/// <remarks>Supported by OpenSearch servers of version 3.2 or greater.</remarks>
	[InterfaceDataContract]
	public interface IAgenticQueryTranslatorRequestProcessor : IRequestProcessor
	{
		/// <summary>The ID of the agent to use for query translation.</summary>
		[DataMember(Name = "agent_id")]
		string AgentId { get; set; }
	}

	/// <inheritdoc cref="IAgenticQueryTranslatorRequestProcessor"/>
	public class AgenticQueryTranslatorRequestProcessor
		: RequestProcessorBase, IAgenticQueryTranslatorRequestProcessor
	{
		protected override string Name => "agentic_query_translator";
		/// <inheritdoc />
		public string AgentId { get; set; }
	}

	/// <inheritdoc cref="IAgenticQueryTranslatorRequestProcessor"/>
	public class AgenticQueryTranslatorRequestProcessorDescriptor
		: RequestProcessorDescriptorBase<AgenticQueryTranslatorRequestProcessorDescriptor,
			IAgenticQueryTranslatorRequestProcessor>,
		  IAgenticQueryTranslatorRequestProcessor
	{
		string IAgenticQueryTranslatorRequestProcessor.AgentId { get; set; }

		/// <inheritdoc cref="IAgenticQueryTranslatorRequestProcessor.AgentId"/>
		public AgenticQueryTranslatorRequestProcessorDescriptor AgentId(string agentId) =>
			Assign(agentId, (a, v) => a.AgentId = v);
	}
}
