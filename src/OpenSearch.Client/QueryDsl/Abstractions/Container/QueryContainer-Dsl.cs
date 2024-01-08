/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/
/*
* Modifications Copyright OpenSearch Contributors. See
* GitHub history for details.
*
*  Licensed to Elasticsearch B.V. under one or more contributor
*  license agreements. See the NOTICE file distributed with
*  this work for additional information regarding copyright
*  ownership. Elasticsearch B.V. licenses this file to you under
*  the Apache License, Version 2.0 (the "License"); you may
*  not use this file except in compliance with the License.
*  You may obtain a copy of the License at
*
* 	http://www.apache.org/licenses/LICENSE-2.0
*
*  Unless required by applicable law or agreed to in writing,
*  software distributed under the License is distributed on an
*  "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
*  KIND, either express or implied.  See the License for the
*  specific language governing permissions and limitations
*  under the License.
*/

using System;
using System.Runtime.Serialization;
using OpenSearch.Client.QueryDsl.Visitor;
using OpenSearch.Net.Utf8Json;

namespace OpenSearch.Client
{
	internal static class QueryContainerExtensions
	{
		public static bool IsConditionless(this QueryContainer q) => q == null || q.IsConditionless;
	}

	[JsonFormatter(typeof(QueryContainerFormatter))]
	public partial class QueryContainer : IQueryContainer, IDescriptor
	{
		public QueryContainer() { }

		public QueryContainer(QueryBase query) : this()
		{
			if (query == null) return;

			if (query.IsStrict && !query.IsWritable)
				throw new ArgumentException("Query is conditionless but strict is turned on");

			query.WrapInContainer(this);
		}

		[IgnoreDataMember]
		internal bool HoldsOnlyShouldMusts { get; set; }

		[IgnoreDataMember]
		internal bool IsConditionless => Self.IsConditionless;

		[IgnoreDataMember]
		internal bool IsStrict => Self.IsStrict;

		[IgnoreDataMember]
		internal bool IsVerbatim => Self.IsVerbatim;

		[IgnoreDataMember]
		internal bool IsWritable => Self.IsWritable;

		[IgnoreDataMember]
		bool IQueryContainer.IsConditionless => ContainedQuery?.Conditionless ?? true;

		[IgnoreDataMember]
		bool IQueryContainer.IsStrict { get; set; }

		[IgnoreDataMember]
		bool IQueryContainer.IsVerbatim { get; set; }

		[IgnoreDataMember]
		bool IQueryContainer.IsWritable => Self.IsVerbatim || !Self.IsConditionless;

		public void Accept(IQueryVisitor visitor)
		{
			if (visitor.Scope == VisitorScope.Unknown) visitor.Scope = VisitorScope.Query;
			new QueryWalker().Walk(this, visitor);
		}

		public static QueryContainer operator &(QueryContainer leftContainer, QueryContainer rightContainer) =>
			And(leftContainer, rightContainer);

		internal static QueryContainer And(QueryContainer leftContainer, QueryContainer rightContainer) =>
			IfEitherIsEmptyReturnTheOtherOrEmpty(leftContainer, rightContainer, out var queryContainer)
				? queryContainer
				: leftContainer.CombineAsMust(rightContainer);

		public static QueryContainer operator |(QueryContainer leftContainer, QueryContainer rightContainer) =>
			Or(leftContainer, rightContainer);

		internal static QueryContainer Or(QueryContainer leftContainer, QueryContainer rightContainer) =>
			IfEitherIsEmptyReturnTheOtherOrEmpty(leftContainer, rightContainer, out var queryContainer)
				? queryContainer
				: leftContainer.CombineAsShould(rightContainer);

		private static bool IfEitherIsEmptyReturnTheOtherOrEmpty(QueryContainer leftContainer, QueryContainer rightContainer,
			out QueryContainer queryContainer
		)
		{
			queryContainer = null;
			if (leftContainer == null && rightContainer == null) return true;

			var leftWritable = leftContainer?.IsWritable ?? false;
			var rightWritable = rightContainer?.IsWritable ?? false;
			if (leftWritable && rightWritable) return false;
			if (!leftWritable && !rightWritable) return true;

			queryContainer = leftWritable ? leftContainer : rightContainer;
			return true;
		}

		public static QueryContainer operator !(QueryContainer queryContainer) => queryContainer == null || !queryContainer.IsWritable
			? null
			: new QueryContainer(new BoolQuery { MustNot = new[] { queryContainer } });

		public static QueryContainer operator +(QueryContainer queryContainer) => queryContainer == null || !queryContainer.IsWritable
			? null
			: new QueryContainer(new BoolQuery { Filter = new[] { queryContainer } });

		public static bool operator false(QueryContainer a) => false;

		public static bool operator true(QueryContainer a) => false;

		// ReSharper disable once UnusedMember.Global
		internal bool ShouldSerialize(IJsonFormatterResolver formatterResolver) => IsWritable;


		public QueryContainer Name(string name)
		{
			ContainedQuery.Name = name;
			return this;
		}

		/// <summary>
		/// Applies the `strict` attribute to the query container and all child sub-queries.
		/// </summary>
		/// <param name="strict"></param>
		/// <returns></returns>
		public QueryContainer Strict(bool strict)
		{
			var visitor = new QueryNodeModifierVisitor((query, ctx) => query.IsStrict = strict);
			Accept(visitor);
			return this;
		}

		/// <summary>
		/// Applies the `verbatim` attribute to the query container and all child sub-queries.
		/// </summary>
		/// <param name="verbatim"></param>
		/// <returns></returns>
		public QueryContainer Verbatim(bool verbatim)
		{
			var visitor = new QueryNodeModifierVisitor((query, ctx) => query.IsVerbatim = verbatim);
			Accept(visitor);
			return this;
		}
	}
}
