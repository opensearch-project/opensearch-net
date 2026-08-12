/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.Runtime.Serialization;
using OpenSearch.Net;
using OpenSearch.Net.Utf8Json;

namespace OpenSearch.Client
{
	// ──────────────────────────────────────────────────────────────────────────
	// Base
	// ──────────────────────────────────────────────────────────────────────────

	/// <summary>Base interface for all search pipeline phase-results processors.</summary>
	[JsonFormatter(typeof(PhaseResultsProcessorFormatter))]
	public interface IPhaseResultsProcessor
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

	/// <inheritdoc cref="IPhaseResultsProcessor"/>
	public abstract class PhaseResultsProcessorBase : IPhaseResultsProcessor
	{
		/// <inheritdoc />
		public string Tag { get; set; }
		/// <inheritdoc />
		public string Description { get; set; }
		/// <inheritdoc />
		public bool? IgnoreFailure { get; set; }

		protected abstract string Name { get; }
		string IPhaseResultsProcessor.Name => Name;
	}

	/// <inheritdoc cref="IPhaseResultsProcessor"/>
	public abstract class PhaseResultsProcessorDescriptorBase<TDescriptor, TInterface>
		: DescriptorBase<TDescriptor, TInterface>, IPhaseResultsProcessor
		where TDescriptor : PhaseResultsProcessorDescriptorBase<TDescriptor, TInterface>, TInterface
		where TInterface : class, IPhaseResultsProcessor
	{
		string IPhaseResultsProcessor.Name => null;
		string IPhaseResultsProcessor.Tag { get; set; }
		string IPhaseResultsProcessor.Description { get; set; }
		bool? IPhaseResultsProcessor.IgnoreFailure { get; set; }

		/// <inheritdoc cref="IPhaseResultsProcessor.Tag"/>
		public TDescriptor Tag(string tag) => Assign(tag, (a, v) => a.Tag = v);

		/// <inheritdoc cref="IPhaseResultsProcessor.Description"/>
		public TDescriptor Description(string description) => Assign(description, (a, v) => a.Description = v);

		/// <inheritdoc cref="IPhaseResultsProcessor.IgnoreFailure"/>
		public TDescriptor IgnoreFailure(bool? ignoreFailure = true) =>
			Assign(ignoreFailure, (a, v) => a.IgnoreFailure = v);
	}

	// ──────────────────────────────────────────────────────────────────────────
	// Shared score normalization / combination models
	// ──────────────────────────────────────────────────────────────────────────

	/// <summary>Score normalization technique.</summary>
	[StringEnum]
	public enum ScoreNormalizationTechnique
	{
		/// <summary>L2 normalization.</summary>
		[EnumMember(Value = "l2")]
		L2,

		/// <summary>Min-max normalization.</summary>
		[EnumMember(Value = "min_max")]
		MinMax,
	}

	/// <summary>Score combination technique.</summary>
	[StringEnum]
	public enum ScoreCombinationTechnique
	{
		/// <summary>Arithmetic mean of scores.</summary>
		[EnumMember(Value = "arithmetic_mean")]
		ArithmeticMean,

		/// <summary>Geometric mean of scores.</summary>
		[EnumMember(Value = "geometric_mean")]
		GeometricMean,

		/// <summary>Harmonic mean of scores.</summary>
		[EnumMember(Value = "harmonic_mean")]
		HarmonicMean,
	}

	/// <summary>Normalization configuration for the <c>normalization-processor</c>.</summary>
	[InterfaceDataContract]
	[ReadAs(typeof(ScoreNormalization))]
	public interface IScoreNormalization
	{
		/// <summary>The normalization technique.</summary>
		[DataMember(Name = "technique")]
		ScoreNormalizationTechnique? Technique { get; set; }
	}

	/// <inheritdoc cref="IScoreNormalization"/>
	public class ScoreNormalization : IScoreNormalization
	{
		/// <inheritdoc />
		public ScoreNormalizationTechnique? Technique { get; set; }
	}

	/// <summary>Combination parameters for the <c>normalization-processor</c>.</summary>
	[InterfaceDataContract]
	[ReadAs(typeof(ScoreCombinationParameters))]
	public interface IScoreCombinationParameters
	{
		/// <summary>Per-query weights for the combination. Must sum to 1.0.</summary>
		[DataMember(Name = "weights")]
		float[] Weights { get; set; }
	}

	/// <inheritdoc cref="IScoreCombinationParameters"/>
	public class ScoreCombinationParameters : IScoreCombinationParameters
	{
		/// <inheritdoc />
		public float[] Weights { get; set; }
	}

	/// <summary>Combination configuration for the <c>normalization-processor</c>.</summary>
	[InterfaceDataContract]
	[ReadAs(typeof(ScoreCombination))]
	public interface IScoreCombination
	{
		/// <summary>The combination technique.</summary>
		[DataMember(Name = "technique")]
		ScoreCombinationTechnique? Technique { get; set; }

		/// <summary>Optional per-query weights.</summary>
		[DataMember(Name = "parameters")]
		IScoreCombinationParameters Parameters { get; set; }
	}

	/// <inheritdoc cref="IScoreCombination"/>
	public class ScoreCombination : IScoreCombination
	{
		/// <inheritdoc />
		public ScoreCombinationTechnique? Technique { get; set; }
		/// <inheritdoc />
		public IScoreCombinationParameters Parameters { get; set; }
	}

	// ──────────────────────────────────────────────────────────────────────────
	// normalization-processor
	// ──────────────────────────────────────────────────────────────────────────

	/// <summary>
	/// The <c>normalization-processor</c> normalizes and combines scores from hybrid queries.
	/// </summary>
	[InterfaceDataContract]
	public interface INormalizationPhaseResultsProcessor : IPhaseResultsProcessor
	{
		/// <summary>Score normalization configuration.</summary>
		[DataMember(Name = "normalization")]
		IScoreNormalization Normalization { get; set; }

		/// <summary>Score combination configuration.</summary>
		[DataMember(Name = "combination")]
		IScoreCombination Combination { get; set; }
	}

	/// <inheritdoc cref="INormalizationPhaseResultsProcessor"/>
	public class NormalizationPhaseResultsProcessor
		: PhaseResultsProcessorBase, INormalizationPhaseResultsProcessor
	{
		protected override string Name => "normalization-processor";
		/// <inheritdoc />
		public IScoreNormalization Normalization { get; set; }
		/// <inheritdoc />
		public IScoreCombination Combination { get; set; }
	}

	/// <inheritdoc cref="INormalizationPhaseResultsProcessor"/>
	public class NormalizationPhaseResultsProcessorDescriptor
		: PhaseResultsProcessorDescriptorBase<NormalizationPhaseResultsProcessorDescriptor,
			INormalizationPhaseResultsProcessor>,
		  INormalizationPhaseResultsProcessor
	{
		IScoreNormalization INormalizationPhaseResultsProcessor.Normalization { get; set; }
		IScoreCombination INormalizationPhaseResultsProcessor.Combination { get; set; }

		/// <inheritdoc cref="INormalizationPhaseResultsProcessor.Normalization"/>
		public NormalizationPhaseResultsProcessorDescriptor Normalization(
			ScoreNormalizationTechnique? technique) =>
			Assign(technique.HasValue ? new ScoreNormalization { Technique = technique } : null,
				(a, v) => a.Normalization = v);

		/// <inheritdoc cref="INormalizationPhaseResultsProcessor.Combination"/>
		public NormalizationPhaseResultsProcessorDescriptor Combination(
			ScoreCombinationTechnique technique, float[] weights = null) =>
			Assign(new ScoreCombination
				{
					Technique = technique,
					Parameters = weights != null
						? new ScoreCombinationParameters { Weights = weights }
						: null
				},
				(a, v) => a.Combination = v);
	}

	// ──────────────────────────────────────────────────────────────────────────
	// score-ranker-processor
	// ──────────────────────────────────────────────────────────────────────────

	/// <summary>Combination technique for the score ranker processor.</summary>
	[StringEnum]
	public enum ScoreRankerCombinationTechnique
	{
		/// <summary>Reciprocal Rank Fusion.</summary>
		[EnumMember(Value = "rrf")]
		Rrf,
	}

	/// <summary>Combination configuration for the <c>score-ranker-processor</c>.</summary>
	[InterfaceDataContract]
	[ReadAs(typeof(ScoreRankerCombination))]
	public interface IScoreRankerCombination
	{
		/// <summary>The combination technique.</summary>
		[DataMember(Name = "technique")]
		ScoreRankerCombinationTechnique Technique { get; set; }

		/// <summary>Rank constant for RRF. Minimum 1.</summary>
		[DataMember(Name = "rank_constant")]
		int? RankConstant { get; set; }
	}

	/// <inheritdoc cref="IScoreRankerCombination"/>
	public class ScoreRankerCombination : IScoreRankerCombination
	{
		/// <inheritdoc />
		public ScoreRankerCombinationTechnique Technique { get; set; }
		/// <inheritdoc />
		public int? RankConstant { get; set; }
	}

	/// <summary>The <c>score-ranker-processor</c> combines scores using rank-based fusion.</summary>
	[InterfaceDataContract]
	public interface IScoreRankerPhaseResultsProcessor : IPhaseResultsProcessor
	{
		/// <summary>Combination configuration.</summary>
		[DataMember(Name = "combination")]
		IScoreRankerCombination Combination { get; set; }
	}

	/// <inheritdoc cref="IScoreRankerPhaseResultsProcessor"/>
	public class ScoreRankerPhaseResultsProcessor
		: PhaseResultsProcessorBase, IScoreRankerPhaseResultsProcessor
	{
		protected override string Name => "score-ranker-processor";
		/// <inheritdoc />
		public IScoreRankerCombination Combination { get; set; }
	}

	/// <inheritdoc cref="IScoreRankerPhaseResultsProcessor"/>
	public class ScoreRankerPhaseResultsProcessorDescriptor
		: PhaseResultsProcessorDescriptorBase<ScoreRankerPhaseResultsProcessorDescriptor,
			IScoreRankerPhaseResultsProcessor>,
		  IScoreRankerPhaseResultsProcessor
	{
		IScoreRankerCombination IScoreRankerPhaseResultsProcessor.Combination { get; set; }

		/// <inheritdoc cref="IScoreRankerPhaseResultsProcessor.Combination"/>
		public ScoreRankerPhaseResultsProcessorDescriptor Combination(
			ScoreRankerCombinationTechnique technique, int? rankConstant = null) =>
			Assign(new ScoreRankerCombination { Technique = technique, RankConstant = rankConstant },
				(a, v) => a.Combination = v);
	}
}
