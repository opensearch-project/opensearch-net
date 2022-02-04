/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*
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
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Osc
{
	/// <summary>
	/// Configures the source for a reindex API request
	/// </summary>
	public interface IReindexSource
	{
		/// <summary>
		/// The indices to target
		/// </summary>
		[DataMember(Name ="index")]
		Indices Index { get; set; }

		/// <summary>
		/// Search query to execute to match documents for reindexing
		/// </summary>
		[DataMember(Name ="query")]
		QueryContainer Query { get; set; }

		/// <summary>
		/// Reindex from a remote OpenSearch cluster
		/// </summary>
		[DataMember(Name ="remote")]
		IRemoteSource Remote { get; set; }

		/// <summary>
		/// The batch size of documents
		/// </summary>
		[DataMember(Name ="size")]
		int? Size { get; set; }

		/// <summary>
		/// Manually parallelize the reindexing process.
		/// This parallelization can improve efficiency and provide a convenient
		/// way to break the request down into smaller parts.
		/// </summary>
		/// <remarks>
		/// Automatic slicing can be performed using <see cref="ReindexOnServerRequest.Slices" />
		/// </remarks>
		[DataMember(Name ="slice")]
		ISlicedScroll Slice { get; set; }

		/// <summary>
		/// Individual fields from _source to reindex
		/// </summary>
		[DataMember(Name ="_source")]
		Fields Source { get; set; }
	}

	/// <inheritdoc />
	public class ReindexSource : IReindexSource
	{
		/// <inheritdoc />
		public Indices Index { get; set; }

		/// <inheritdoc />
		public QueryContainer Query { get; set; }

		/// <inheritdoc />
		public IRemoteSource Remote { get; set; }

		/// <inheritdoc />
		public int? Size { get; set; }

		/// <inheritdoc />
		public ISlicedScroll Slice { get; set; }

		/// <inheritdoc />
		public Fields Source { get; set; }
	}

	/// <inheritdoc cref="IReindexSource" />
	public class ReindexSourceDescriptor : DescriptorBase<ReindexSourceDescriptor, IReindexSource>, IReindexSource
	{
		Indices IReindexSource.Index { get; set; }
		QueryContainer IReindexSource.Query { get; set; }
		IRemoteSource IReindexSource.Remote { get; set; }
		int? IReindexSource.Size { get; set; }
		ISlicedScroll IReindexSource.Slice { get; set; }
		Fields IReindexSource.Source { get; set; }

		/// <inheritdoc cref="IReindexSource.Query" />
		public ReindexSourceDescriptor Query<T>(Func<QueryContainerDescriptor<T>, QueryContainer> querySelector) where T : class =>
			Assign(querySelector, (a, v) => a.Query = v?.Invoke(new QueryContainerDescriptor<T>()));

		/// <inheritdoc cref="IReindexSource.Remote" />
		public ReindexSourceDescriptor Remote(Func<RemoteSourceDescriptor, IRemoteSource> selector) =>
			Assign(selector, (a, v) => a.Remote = v?.Invoke(new RemoteSourceDescriptor()));

		/// <inheritdoc cref="IReindexSource.Index" />
		public ReindexSourceDescriptor Index(Indices indices) => Assign(indices, (a, v) => a.Index = v);

		/// <inheritdoc cref="IReindexSource.Size" />
		public ReindexSourceDescriptor Size(int? size) => Assign(size, (a, v) => a.Size = v);

		/// <inheritdoc cref="IReindexSource.Slice" />
		public ReindexSourceDescriptor Slice<T>(Func<SlicedScrollDescriptor<T>, ISlicedScroll> selector) where T : class =>
			Assign(selector, (a, v) => a.Slice = v?.Invoke(new SlicedScrollDescriptor<T>()));

		/// <inheritdoc cref="IReindexSource.Source" />
		public ReindexSourceDescriptor Source<T>(Func<FieldsDescriptor<T>, IPromise<Fields>> fields) where T : class =>
			Assign(fields, (a, v) => a.Source = v?.Invoke(new FieldsDescriptor<T>())?.Value);
	}
}
