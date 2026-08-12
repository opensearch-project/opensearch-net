/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.Collections.Generic;
using System.Runtime.Serialization;
using OpenSearch.Net.Utf8Json;

namespace OpenSearch.Client
{
	// ──────────────────────────────────────────────────────────────────────────
	// Base
	// ──────────────────────────────────────────────────────────────────────────

	/// <summary>Base interface for all search pipeline response processors.</summary>
	[JsonFormatter(typeof(ResponseProcessorFormatter))]
	public interface IResponseProcessor
	{
		/// <summary>The processor name key used for serialization.</summary>
		string Name { get; }

		/// <summary>A tag for the processor.</summary>
		[DataMember(Name = "tag")]
		string Tag { get; set; }

		/// <summary>A description of the processor.</summary>
		[DataMember(Name = "description")]
		string Description { get; set; }

		/// <summary>Whether to ignore failures.</summary>
		[DataMember(Name = "ignore_failure")]
		bool? IgnoreFailure { get; set; }
	}

	/// <inheritdoc cref="IResponseProcessor"/>
	public abstract class ResponseProcessorBase : IResponseProcessor
	{
		/// <inheritdoc />
		public string Tag { get; set; }
		/// <inheritdoc />
		public string Description { get; set; }
		/// <inheritdoc />
		public bool? IgnoreFailure { get; set; }

		protected abstract string Name { get; }
		string IResponseProcessor.Name => Name;
	}

	/// <inheritdoc cref="IResponseProcessor"/>
	public abstract class ResponseProcessorDescriptorBase<TDescriptor, TInterface>
		: DescriptorBase<TDescriptor, TInterface>, IResponseProcessor
		where TDescriptor : ResponseProcessorDescriptorBase<TDescriptor, TInterface>, TInterface
		where TInterface : class, IResponseProcessor
	{
		string IResponseProcessor.Name => null;
		string IResponseProcessor.Tag { get; set; }
		string IResponseProcessor.Description { get; set; }
		bool? IResponseProcessor.IgnoreFailure { get; set; }

		/// <inheritdoc cref="IResponseProcessor.Tag"/>
		public TDescriptor Tag(string tag) => Assign(tag, (a, v) => a.Tag = v);

		/// <inheritdoc cref="IResponseProcessor.Description"/>
		public TDescriptor Description(string description) => Assign(description, (a, v) => a.Description = v);

		/// <inheritdoc cref="IResponseProcessor.IgnoreFailure"/>
		public TDescriptor IgnoreFailure(bool? ignoreFailure = true) =>
			Assign(ignoreFailure, (a, v) => a.IgnoreFailure = v);
	}

	// ──────────────────────────────────────────────────────────────────────────
	// retrieval_augmented_generation
	// ──────────────────────────────────────────────────────────────────────────

	/// <summary>The <c>retrieval_augmented_generation</c> response processor calls an LLM with the
	/// search results as context and returns its response.</summary>
	[InterfaceDataContract]
	public interface IRetrievalAugmentedGenerationResponseProcessor : IResponseProcessor
	{
		/// <summary>The ID of the ML model to use for response generation.</summary>
		[DataMember(Name = "model_id")]
		string ModelId { get; set; }

		/// <summary>
		/// The list of document fields whose values are passed to the model as context.
		/// </summary>
		[DataMember(Name = "context_field_list")]
		IEnumerable<string> ContextFieldList { get; set; }

		/// <summary>A system prompt to send to the model.</summary>
		[DataMember(Name = "system_prompt")]
		string SystemPrompt { get; set; }

		/// <summary>User instructions to send to the model.</summary>
		[DataMember(Name = "user_instructions")]
		string UserInstructions { get; set; }
	}

	/// <inheritdoc cref="IRetrievalAugmentedGenerationResponseProcessor"/>
	public class RetrievalAugmentedGenerationResponseProcessor
		: ResponseProcessorBase, IRetrievalAugmentedGenerationResponseProcessor
	{
		protected override string Name => "retrieval_augmented_generation";
		/// <inheritdoc />
		public string ModelId { get; set; }
		/// <inheritdoc />
		public IEnumerable<string> ContextFieldList { get; set; }
		/// <inheritdoc />
		public string SystemPrompt { get; set; }
		/// <inheritdoc />
		public string UserInstructions { get; set; }
	}

	/// <inheritdoc cref="IRetrievalAugmentedGenerationResponseProcessor"/>
	public class RetrievalAugmentedGenerationResponseProcessorDescriptor
		: ResponseProcessorDescriptorBase<RetrievalAugmentedGenerationResponseProcessorDescriptor,
			IRetrievalAugmentedGenerationResponseProcessor>,
		  IRetrievalAugmentedGenerationResponseProcessor
	{
		string IRetrievalAugmentedGenerationResponseProcessor.ModelId { get; set; }
		IEnumerable<string> IRetrievalAugmentedGenerationResponseProcessor.ContextFieldList { get; set; }
		string IRetrievalAugmentedGenerationResponseProcessor.SystemPrompt { get; set; }
		string IRetrievalAugmentedGenerationResponseProcessor.UserInstructions { get; set; }

		/// <inheritdoc cref="IRetrievalAugmentedGenerationResponseProcessor.ModelId"/>
		public RetrievalAugmentedGenerationResponseProcessorDescriptor ModelId(string modelId) =>
			Assign(modelId, (a, v) => a.ModelId = v);

		/// <inheritdoc cref="IRetrievalAugmentedGenerationResponseProcessor.ContextFieldList"/>
		public RetrievalAugmentedGenerationResponseProcessorDescriptor ContextFieldList(
			IEnumerable<string> fields) =>
			Assign(fields, (a, v) => a.ContextFieldList = v);

		/// <inheritdoc cref="IRetrievalAugmentedGenerationResponseProcessor.SystemPrompt"/>
		public RetrievalAugmentedGenerationResponseProcessorDescriptor SystemPrompt(string prompt) =>
			Assign(prompt, (a, v) => a.SystemPrompt = v);

		/// <inheritdoc cref="IRetrievalAugmentedGenerationResponseProcessor.UserInstructions"/>
		public RetrievalAugmentedGenerationResponseProcessorDescriptor UserInstructions(string instructions) =>
			Assign(instructions, (a, v) => a.UserInstructions = v);
	}

	// ──────────────────────────────────────────────────────────────────────────
	// rerank
	// ──────────────────────────────────────────────────────────────────────────

	/// <summary>ML model configuration for the <c>rerank</c> response processor.</summary>
	[InterfaceDataContract]
	[ReadAs(typeof(MLOpenSearchReranker))]
	public interface IMLOpenSearchReranker
	{
		/// <summary>The ID of the ML model to use for reranking.</summary>
		[DataMember(Name = "model_id")]
		string ModelId { get; set; }
	}

	/// <inheritdoc cref="IMLOpenSearchReranker"/>
	public class MLOpenSearchReranker : IMLOpenSearchReranker
	{
		/// <inheritdoc />
		public string ModelId { get; set; }
	}

	/// <summary>Context configuration for the <c>rerank</c> response processor.</summary>
	[InterfaceDataContract]
	[ReadAs(typeof(RerankContext))]
	public interface IRerankContext
	{
		/// <summary>The document fields used as context for reranking.</summary>
		[DataMember(Name = "document_fields")]
		IEnumerable<string> DocumentFields { get; set; }
	}

	/// <inheritdoc cref="IRerankContext"/>
	public class RerankContext : IRerankContext
	{
		/// <inheritdoc />
		public IEnumerable<string> DocumentFields { get; set; }
	}

	/// <summary>The <c>rerank</c> response processor reranks search results using an ML model.</summary>
	[InterfaceDataContract]
	public interface IRerankResponseProcessor : IResponseProcessor
	{
		/// <summary>ML model configuration for reranking.</summary>
		[DataMember(Name = "ml_opensearch")]
		IMLOpenSearchReranker MlOpenSearch { get; set; }

		/// <summary>Context configuration for reranking.</summary>
		[DataMember(Name = "context")]
		IRerankContext Context { get; set; }
	}

	/// <inheritdoc cref="IRerankResponseProcessor"/>
	public class RerankResponseProcessor : ResponseProcessorBase, IRerankResponseProcessor
	{
		protected override string Name => "rerank";
		/// <inheritdoc />
		public IMLOpenSearchReranker MlOpenSearch { get; set; }
		/// <inheritdoc />
		public IRerankContext Context { get; set; }
	}

	/// <inheritdoc cref="IRerankResponseProcessor"/>
	public class RerankResponseProcessorDescriptor
		: ResponseProcessorDescriptorBase<RerankResponseProcessorDescriptor,
			IRerankResponseProcessor>,
		  IRerankResponseProcessor
	{
		IMLOpenSearchReranker IRerankResponseProcessor.MlOpenSearch { get; set; }
		IRerankContext IRerankResponseProcessor.Context { get; set; }

		/// <inheritdoc cref="IRerankResponseProcessor.MlOpenSearch"/>
		public RerankResponseProcessorDescriptor MlOpenSearch(string modelId) =>
			Assign(new MLOpenSearchReranker { ModelId = modelId },
				(a, v) => a.MlOpenSearch = v);

		/// <inheritdoc cref="IRerankResponseProcessor.Context"/>
		public RerankResponseProcessorDescriptor Context(IEnumerable<string> documentFields) =>
			Assign(new RerankContext { DocumentFields = documentFields },
				(a, v) => a.Context = v);
	}

	// ──────────────────────────────────────────────────────────────────────────
	// rename_field
	// ──────────────────────────────────────────────────────────────────────────

	/// <summary>The <c>rename_field</c> response processor renames a field in each search hit.</summary>
	[InterfaceDataContract]
	public interface IRenameFieldResponseProcessor : IResponseProcessor
	{
		/// <summary>The field to rename.</summary>
		[DataMember(Name = "field")]
		string Field { get; set; }

		/// <summary>The new name for the field.</summary>
		[DataMember(Name = "target_field")]
		string TargetField { get; set; }
	}

	/// <inheritdoc cref="IRenameFieldResponseProcessor"/>
	public class RenameFieldResponseProcessor : ResponseProcessorBase, IRenameFieldResponseProcessor
	{
		protected override string Name => "rename_field";
		/// <inheritdoc />
		public string Field { get; set; }
		/// <inheritdoc />
		public string TargetField { get; set; }
	}

	/// <inheritdoc cref="IRenameFieldResponseProcessor"/>
	public class RenameFieldResponseProcessorDescriptor
		: ResponseProcessorDescriptorBase<RenameFieldResponseProcessorDescriptor,
			IRenameFieldResponseProcessor>,
		  IRenameFieldResponseProcessor
	{
		string IRenameFieldResponseProcessor.Field { get; set; }
		string IRenameFieldResponseProcessor.TargetField { get; set; }

		/// <inheritdoc cref="IRenameFieldResponseProcessor.Field"/>
		public RenameFieldResponseProcessorDescriptor Field(string field) =>
			Assign(field, (a, v) => a.Field = v);

		/// <inheritdoc cref="IRenameFieldResponseProcessor.TargetField"/>
		public RenameFieldResponseProcessorDescriptor TargetField(string targetField) =>
			Assign(targetField, (a, v) => a.TargetField = v);
	}

	// ──────────────────────────────────────────────────────────────────────────
	// truncate_hits
	// ──────────────────────────────────────────────────────────────────────────

	/// <summary>The <c>truncate_hits</c> response processor reduces the number of search hits.</summary>
	[InterfaceDataContract]
	public interface ITruncateHitsResponseProcessor : IResponseProcessor
	{
		/// <summary>The maximum number of hits to return.</summary>
		[DataMember(Name = "target_size")]
		int? TargetSize { get; set; }

		/// <summary>Prefix for the context variable holding the original size.</summary>
		[DataMember(Name = "context_prefix")]
		string ContextPrefix { get; set; }
	}

	/// <inheritdoc cref="ITruncateHitsResponseProcessor"/>
	public class TruncateHitsResponseProcessor : ResponseProcessorBase, ITruncateHitsResponseProcessor
	{
		protected override string Name => "truncate_hits";
		/// <inheritdoc />
		public int? TargetSize { get; set; }
		/// <inheritdoc />
		public string ContextPrefix { get; set; }
	}

	/// <inheritdoc cref="ITruncateHitsResponseProcessor"/>
	public class TruncateHitsResponseProcessorDescriptor
		: ResponseProcessorDescriptorBase<TruncateHitsResponseProcessorDescriptor,
			ITruncateHitsResponseProcessor>,
		  ITruncateHitsResponseProcessor
	{
		int? ITruncateHitsResponseProcessor.TargetSize { get; set; }
		string ITruncateHitsResponseProcessor.ContextPrefix { get; set; }

		/// <inheritdoc cref="ITruncateHitsResponseProcessor.TargetSize"/>
		public TruncateHitsResponseProcessorDescriptor TargetSize(int? size) =>
			Assign(size, (a, v) => a.TargetSize = v);

		/// <inheritdoc cref="ITruncateHitsResponseProcessor.ContextPrefix"/>
		public TruncateHitsResponseProcessorDescriptor ContextPrefix(string prefix) =>
			Assign(prefix, (a, v) => a.ContextPrefix = v);
	}

	// ──────────────────────────────────────────────────────────────────────────
	// sort
	// ──────────────────────────────────────────────────────────────────────────

	/// <summary>The <c>sort</c> response processor sorts an array field in each search hit.</summary>
	[InterfaceDataContract]
	public interface ISortResponseProcessor : IResponseProcessor
	{
		/// <summary>The array field to sort.</summary>
		[DataMember(Name = "field")]
		string Field { get; set; }

		/// <summary>The sort order. Use <c>"asc"</c> or <c>"desc"</c>.</summary>
		[DataMember(Name = "order")]
		string Order { get; set; }

		/// <summary>The field in which to store the sorted values. Defaults to <see cref="Field"/>.</summary>
		[DataMember(Name = "target_field")]
		string TargetField { get; set; }
	}

	/// <inheritdoc cref="ISortResponseProcessor"/>
	public class SortResponseProcessor : ResponseProcessorBase, ISortResponseProcessor
	{
		protected override string Name => "sort";
		/// <inheritdoc />
		public string Field { get; set; }
		/// <inheritdoc />
		public string Order { get; set; }
		/// <inheritdoc />
		public string TargetField { get; set; }
	}

	/// <inheritdoc cref="ISortResponseProcessor"/>
	public class SortResponseProcessorDescriptor
		: ResponseProcessorDescriptorBase<SortResponseProcessorDescriptor,
			ISortResponseProcessor>,
		  ISortResponseProcessor
	{
		string ISortResponseProcessor.Field { get; set; }
		string ISortResponseProcessor.Order { get; set; }
		string ISortResponseProcessor.TargetField { get; set; }

		/// <inheritdoc cref="ISortResponseProcessor.Field"/>
		public SortResponseProcessorDescriptor Field(string field) => Assign(field, (a, v) => a.Field = v);

		/// <inheritdoc cref="ISortResponseProcessor.Order"/>
		public SortResponseProcessorDescriptor Order(string order) => Assign(order, (a, v) => a.Order = v);

		/// <inheritdoc cref="ISortResponseProcessor.TargetField"/>
		public SortResponseProcessorDescriptor TargetField(string targetField) =>
			Assign(targetField, (a, v) => a.TargetField = v);
	}

	// ──────────────────────────────────────────────────────────────────────────
	// split
	// ──────────────────────────────────────────────────────────────────────────

	/// <summary>The <c>split</c> response processor splits a string field into an array.</summary>
	[InterfaceDataContract]
	public interface ISplitResponseProcessor : IResponseProcessor
	{
		/// <summary>The field to split.</summary>
		[DataMember(Name = "field")]
		string Field { get; set; }

		/// <summary>The separator string or regex pattern.</summary>
		[DataMember(Name = "separator")]
		string Separator { get; set; }

		/// <summary>Whether to preserve trailing empty strings after splitting.</summary>
		[DataMember(Name = "preserve_trailing")]
		bool? PreserveTrailing { get; set; }

		/// <summary>The field in which to store the split values. Defaults to <see cref="Field"/>.</summary>
		[DataMember(Name = "target_field")]
		string TargetField { get; set; }
	}

	/// <inheritdoc cref="ISplitResponseProcessor"/>
	public class SplitResponseProcessor : ResponseProcessorBase, ISplitResponseProcessor
	{
		protected override string Name => "split";
		/// <inheritdoc />
		public string Field { get; set; }
		/// <inheritdoc />
		public string Separator { get; set; }
		/// <inheritdoc />
		public bool? PreserveTrailing { get; set; }
		/// <inheritdoc />
		public string TargetField { get; set; }
	}

	/// <inheritdoc cref="ISplitResponseProcessor"/>
	public class SplitResponseProcessorDescriptor
		: ResponseProcessorDescriptorBase<SplitResponseProcessorDescriptor,
			ISplitResponseProcessor>,
		  ISplitResponseProcessor
	{
		string ISplitResponseProcessor.Field { get; set; }
		string ISplitResponseProcessor.Separator { get; set; }
		bool? ISplitResponseProcessor.PreserveTrailing { get; set; }
		string ISplitResponseProcessor.TargetField { get; set; }

		/// <inheritdoc cref="ISplitResponseProcessor.Field"/>
		public SplitResponseProcessorDescriptor Field(string field) => Assign(field, (a, v) => a.Field = v);

		/// <inheritdoc cref="ISplitResponseProcessor.Separator"/>
		public SplitResponseProcessorDescriptor Separator(string separator) =>
			Assign(separator, (a, v) => a.Separator = v);

		/// <inheritdoc cref="ISplitResponseProcessor.PreserveTrailing"/>
		public SplitResponseProcessorDescriptor PreserveTrailing(bool? preserve = true) =>
			Assign(preserve, (a, v) => a.PreserveTrailing = v);

		/// <inheritdoc cref="ISplitResponseProcessor.TargetField"/>
		public SplitResponseProcessorDescriptor TargetField(string targetField) =>
			Assign(targetField, (a, v) => a.TargetField = v);
	}

	// ──────────────────────────────────────────────────────────────────────────
	// collapse
	// ──────────────────────────────────────────────────────────────────────────

	/// <summary>The <c>collapse</c> response processor collapses hits based on a field value.</summary>
	[InterfaceDataContract]
	public interface ICollapseResponseProcessor : IResponseProcessor
	{
		/// <summary>The field to collapse on.</summary>
		[DataMember(Name = "field")]
		string Field { get; set; }

		/// <summary>Prefix for the context variable holding the original size.</summary>
		[DataMember(Name = "context_prefix")]
		string ContextPrefix { get; set; }
	}

	/// <inheritdoc cref="ICollapseResponseProcessor"/>
	public class CollapseResponseProcessor : ResponseProcessorBase, ICollapseResponseProcessor
	{
		protected override string Name => "collapse";
		/// <inheritdoc />
		public string Field { get; set; }
		/// <inheritdoc />
		public string ContextPrefix { get; set; }
	}

	/// <inheritdoc cref="ICollapseResponseProcessor"/>
	public class CollapseResponseProcessorDescriptor
		: ResponseProcessorDescriptorBase<CollapseResponseProcessorDescriptor,
			ICollapseResponseProcessor>,
		  ICollapseResponseProcessor
	{
		string ICollapseResponseProcessor.Field { get; set; }
		string ICollapseResponseProcessor.ContextPrefix { get; set; }

		/// <inheritdoc cref="ICollapseResponseProcessor.Field"/>
		public CollapseResponseProcessorDescriptor Field(string field) => Assign(field, (a, v) => a.Field = v);

		/// <inheritdoc cref="ICollapseResponseProcessor.ContextPrefix"/>
		public CollapseResponseProcessorDescriptor ContextPrefix(string prefix) =>
			Assign(prefix, (a, v) => a.ContextPrefix = v);
	}

	// ──────────────────────────────────────────────────────────────────────────
	// personalize_search_ranking
	// ──────────────────────────────────────────────────────────────────────────

	/// <summary>The <c>personalize_search_ranking</c> response processor reranks results using
	/// Amazon Personalize.</summary>
	[InterfaceDataContract]
	public interface IPersonalizeSearchRankingResponseProcessor : IResponseProcessor
	{
		/// <summary>The Amazon Personalize campaign ARN.</summary>
		[DataMember(Name = "campaign_arn")]
		string CampaignArn { get; set; }

		/// <summary>The Amazon Personalize recipe.</summary>
		[DataMember(Name = "recipe")]
		string Recipe { get; set; }

		/// <summary>The weight of the Amazon Personalize ranking (0 to 1).</summary>
		[DataMember(Name = "weight")]
		float Weight { get; set; }

		/// <summary>The field to use as the item ID. Defaults to <c>_id</c>.</summary>
		[DataMember(Name = "item_id_field")]
		string ItemIdField { get; set; }

		/// <summary>The IAM role ARN to use when calling Amazon Personalize.</summary>
		[DataMember(Name = "iam_role_arn")]
		string IamRoleArn { get; set; }
	}

	/// <inheritdoc cref="IPersonalizeSearchRankingResponseProcessor"/>
	public class PersonalizeSearchRankingResponseProcessor
		: ResponseProcessorBase, IPersonalizeSearchRankingResponseProcessor
	{
		protected override string Name => "personalize_search_ranking";
		/// <inheritdoc />
		public string CampaignArn { get; set; }
		/// <inheritdoc />
		public string Recipe { get; set; }
		/// <inheritdoc />
		public float Weight { get; set; }
		/// <inheritdoc />
		public string ItemIdField { get; set; }
		/// <inheritdoc />
		public string IamRoleArn { get; set; }
	}

	/// <inheritdoc cref="IPersonalizeSearchRankingResponseProcessor"/>
	public class PersonalizeSearchRankingResponseProcessorDescriptor
		: ResponseProcessorDescriptorBase<PersonalizeSearchRankingResponseProcessorDescriptor,
			IPersonalizeSearchRankingResponseProcessor>,
		  IPersonalizeSearchRankingResponseProcessor
	{
		string IPersonalizeSearchRankingResponseProcessor.CampaignArn { get; set; }
		string IPersonalizeSearchRankingResponseProcessor.Recipe { get; set; }
		float IPersonalizeSearchRankingResponseProcessor.Weight { get; set; }
		string IPersonalizeSearchRankingResponseProcessor.ItemIdField { get; set; }
		string IPersonalizeSearchRankingResponseProcessor.IamRoleArn { get; set; }

		/// <inheritdoc cref="IPersonalizeSearchRankingResponseProcessor.CampaignArn"/>
		public PersonalizeSearchRankingResponseProcessorDescriptor CampaignArn(string arn) =>
			Assign(arn, (a, v) => a.CampaignArn = v);

		/// <inheritdoc cref="IPersonalizeSearchRankingResponseProcessor.Recipe"/>
		public PersonalizeSearchRankingResponseProcessorDescriptor Recipe(string recipe) =>
			Assign(recipe, (a, v) => a.Recipe = v);

		/// <inheritdoc cref="IPersonalizeSearchRankingResponseProcessor.Weight"/>
		public PersonalizeSearchRankingResponseProcessorDescriptor Weight(float? weight) =>
			Assign(weight, (a, v) => a.Weight = v ?? 0);

		/// <inheritdoc cref="IPersonalizeSearchRankingResponseProcessor.ItemIdField"/>
		public PersonalizeSearchRankingResponseProcessorDescriptor ItemIdField(string field) =>
			Assign(field, (a, v) => a.ItemIdField = v);

		/// <inheritdoc cref="IPersonalizeSearchRankingResponseProcessor.IamRoleArn"/>
		public PersonalizeSearchRankingResponseProcessorDescriptor IamRoleArn(string arn) =>
			Assign(arn, (a, v) => a.IamRoleArn = v);
	}

	// ──────────────────────────────────────────────────────────────────────────
	// agentic_context (3.3+)
	// ──────────────────────────────────────────────────────────────────────────

	/// <summary>
	/// The <c>agentic_context</c> response processor adds agent execution context to the response.
	/// </summary>
	/// <remarks>Supported by OpenSearch servers of version 3.3 or greater.</remarks>
	[InterfaceDataContract]
	public interface IAgenticContextResponseProcessor : IResponseProcessor
	{
		/// <summary>Whether to include the agent's execution step summary in the response.</summary>
		[DataMember(Name = "agent_steps_summary")]
		bool? AgentStepsSummary { get; set; }

		/// <summary>Whether to include the generated DSL query in the response.</summary>
		[DataMember(Name = "dsl_query")]
		bool? DslQuery { get; set; }
	}

	/// <inheritdoc cref="IAgenticContextResponseProcessor"/>
	public class AgenticContextResponseProcessor : ResponseProcessorBase, IAgenticContextResponseProcessor
	{
		protected override string Name => "agentic_context";
		/// <inheritdoc />
		public bool? AgentStepsSummary { get; set; }
		/// <inheritdoc />
		public bool? DslQuery { get; set; }
	}

	/// <inheritdoc cref="IAgenticContextResponseProcessor"/>
	public class AgenticContextResponseProcessorDescriptor
		: ResponseProcessorDescriptorBase<AgenticContextResponseProcessorDescriptor,
			IAgenticContextResponseProcessor>,
		  IAgenticContextResponseProcessor
	{
		bool? IAgenticContextResponseProcessor.AgentStepsSummary { get; set; }
		bool? IAgenticContextResponseProcessor.DslQuery { get; set; }

		/// <inheritdoc cref="IAgenticContextResponseProcessor.AgentStepsSummary"/>
		public AgenticContextResponseProcessorDescriptor AgentStepsSummary(bool? include = true) =>
			Assign(include, (a, v) => a.AgentStepsSummary = v);

		/// <inheritdoc cref="IAgenticContextResponseProcessor.DslQuery"/>
		public AgenticContextResponseProcessorDescriptor DslQuery(bool? include = true) =>
			Assign(include, (a, v) => a.DslQuery = v);
	}
}
