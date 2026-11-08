/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Runtime.Serialization;
using OpenSearch.Net.Utf8Json;

namespace OpenSearch.Client
{
	/// <summary>
	/// The combined_fields query supports searching multiple text fields as if their contents had been indexed
	/// into one combined field. The query analyzes the provided text before performing a search, and it scores
	/// documents using a term-centric approach that treats the matched fields as a single field.
	/// <para />
	/// All queried fields must have the same search analyzer. Only <c>text</c> fields are supported.
	/// </summary>
	[InterfaceDataContract]
	[ReadAs(typeof(CombinedFieldsQuery))]
	public interface ICombinedFieldsQuery : IQuery
	{
		/// <summary>
		/// The list of fields to search. Field wildcard patterns are allowed. Only <c>text</c> fields are supported,
		/// and they must all share the same search analyzer.
		/// </summary>
		[DataMember(Name = "fields")]
		Fields Fields { get; set; }

		/// <summary>
		/// A value controlling how many "should" clauses in the resulting boolean query should match.
		/// It can be an absolute value, a percentage or a combination of both.
		/// </summary>
		[DataMember(Name = "minimum_should_match")]
		MinimumShouldMatch MinimumShouldMatch { get; set; }

		/// <summary>
		/// The operator used to combine the analyzed terms of the query value.
		/// The default operator is <see cref="Client.Operator.Or" />.
		/// </summary>
		[DataMember(Name = "operator")]
		Operator? Operator { get; set; }

		/// <summary>
		/// The text to search for in the provided <see cref="Fields" />.
		/// The combined_fields query analyzes the provided text before performing a search.
		/// </summary>
		[DataMember(Name = "query")]
		string Query { get; set; }
	}

	/// <inheritdoc cref="ICombinedFieldsQuery" />
	public class CombinedFieldsQuery : QueryBase, ICombinedFieldsQuery
	{
		/// <inheritdoc />
		public Fields Fields { get; set; }

		/// <inheritdoc />
		public MinimumShouldMatch MinimumShouldMatch { get; set; }

		/// <inheritdoc />
		public Operator? Operator { get; set; }

		/// <inheritdoc />
		public string Query { get; set; }

		protected override bool Conditionless => IsConditionless(this);

		internal override void InternalWrapInContainer(IQueryContainer c) => c.CombinedFields = this;

		internal static bool IsConditionless(ICombinedFieldsQuery q) => q.Query.IsNullOrEmpty();
	}

	/// <inheritdoc cref="ICombinedFieldsQuery" />
	[DataContract]
	public class CombinedFieldsQueryDescriptor<T>
		: QueryDescriptorBase<CombinedFieldsQueryDescriptor<T>, ICombinedFieldsQuery>
			, ICombinedFieldsQuery where T : class
	{
		protected override bool Conditionless => CombinedFieldsQuery.IsConditionless(this);
		Fields ICombinedFieldsQuery.Fields { get; set; }
		MinimumShouldMatch ICombinedFieldsQuery.MinimumShouldMatch { get; set; }
		Operator? ICombinedFieldsQuery.Operator { get; set; }
		string ICombinedFieldsQuery.Query { get; set; }

		/// <inheritdoc cref="ICombinedFieldsQuery.Fields" />
		public CombinedFieldsQueryDescriptor<T> Fields(Func<FieldsDescriptor<T>, IPromise<Fields>> fields) =>
			Assign(fields, (a, v) => a.Fields = v?.Invoke(new FieldsDescriptor<T>())?.Value);

		/// <inheritdoc cref="ICombinedFieldsQuery.Fields" />
		public CombinedFieldsQueryDescriptor<T> Fields(Fields fields) => Assign(fields, (a, v) => a.Fields = v);

		/// <inheritdoc cref="ICombinedFieldsQuery.Query" />
		public CombinedFieldsQueryDescriptor<T> Query(string query) => Assign(query, (a, v) => a.Query = v);

		/// <inheritdoc cref="ICombinedFieldsQuery.Operator" />
		public CombinedFieldsQueryDescriptor<T> Operator(Operator? op) => Assign(op, (a, v) => a.Operator = v);

		/// <inheritdoc cref="ICombinedFieldsQuery.MinimumShouldMatch" />
		public CombinedFieldsQueryDescriptor<T> MinimumShouldMatch(MinimumShouldMatch minimumShouldMatch)
			=> Assign(minimumShouldMatch, (a, v) => a.MinimumShouldMatch = v);
	}
}
