/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.Runtime.Serialization;
using OpenSearch.Net.Utf8Json;

namespace OpenSearch.Client;

/// <summary>
/// A neural query.
/// </summary>
[InterfaceDataContract]
[JsonFormatter(typeof(FieldNameQueryFormatter<NeuralQuery, INeuralQuery>))]
public interface INeuralQuery : IFieldNameQuery
{
	/// <summary>
	/// The query text from which to produce queries.
	/// </summary>
	[DataMember(Name = "query_text")]
	string QueryText { get; set; }

	/// <summary>
	/// The number of results the k-NN search returns.
	/// </summary>
	[DataMember(Name = "k")]
	int? K { get; set; }

	/// <summary>
	/// The ID of the model that will be used in the embedding interface.
	/// The model must be indexed in OpenSearch before it can be used in Neural Search.
	/// </summary>
	[DataMember(Name = "model_id")]
	string ModelId { get; set; }
}

[DataContract]
public class NeuralQuery : FieldNameQueryBase, INeuralQuery
{
	/// <inheritdoc />
	public string QueryText { get; set; }
	/// <inheritdoc />
	public int? K { get; set; }
	/// <inheritdoc />
	public string ModelId { get; set; }

	protected override bool Conditionless => IsConditionless(this);

	internal override void InternalWrapInContainer(IQueryContainer container) => container.Neural = this;

	internal static bool IsConditionless(INeuralQuery q) => string.IsNullOrEmpty(q.QueryText) || q.K == null || q.K == 0 || string.IsNullOrEmpty(q.ModelId) || q.Field.IsConditionless();
}

public class NeuralQueryDescriptor<T>
	: FieldNameQueryDescriptorBase<NeuralQueryDescriptor<T>, INeuralQuery, T>,
		INeuralQuery
	where T : class
{
	protected override bool Conditionless => NeuralQuery.IsConditionless(this);
	string INeuralQuery.QueryText { get; set; }
	int? INeuralQuery.K { get; set; }
	string INeuralQuery.ModelId { get; set; }

	/// <inheritdoc cref="INeuralQuery.QueryText" />
	public NeuralQueryDescriptor<T> QueryText(string queryText) => Assign(queryText, (a, v) => a.QueryText = v);

	/// <inheritdoc cref="INeuralQuery.K" />
	public NeuralQueryDescriptor<T> K(int? k) => Assign(k, (a, v) => a.K = v);

	/// <inheritdoc cref="INeuralQuery.ModelId" />
	public NeuralQueryDescriptor<T> ModelId(string modelId) => Assign(modelId, (a, v) => a.ModelId = v);
}
