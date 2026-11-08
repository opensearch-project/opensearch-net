/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using OpenSearch.Net;
using OpenSearch.Net.Utf8Json;

namespace OpenSearch.Client
{
	[JsonFormatter(typeof(BulkStreamRequestFormatter))]
	[MapsApi("bulk_stream")]
	public partial interface IBulkStreamRequest
	{
		[IgnoreDataMember]
		BulkOperationsCollection<IBulkOperation> Operations { get; set; }
	}

	public partial class BulkStreamRequest
	{
		protected sealed override void RequestDefaults(BulkStreamRequestParameters parameters) =>
			parameters.CustomResponseBuilder = BulkStreamResponseBuilder.Instance;

		public BulkOperationsCollection<IBulkOperation> Operations { get; set; }
	}

	public partial class BulkStreamDescriptor
	{
		protected sealed override void RequestDefaults(BulkStreamRequestParameters parameters) =>
			parameters.CustomResponseBuilder = BulkStreamResponseBuilder.Instance;

		BulkOperationsCollection<IBulkOperation> IBulkStreamRequest.Operations { get; set; } = new BulkOperationsCollection<IBulkOperation>();

		public BulkStreamDescriptor Create<T>(Func<BulkCreateDescriptor<T>, IBulkCreateOperation<T>> bulkCreateSelector)
			where T : class =>
			AddOperation(bulkCreateSelector?.Invoke(new BulkCreateDescriptor<T>()));

		/// <summary>
		/// CreateMany, convenience method to create many documents at once.
		/// </summary>
		/// <param name="objects">the objects to create</param>
		/// <param name="bulkCreateSelector">A func called on each object to describe the individual create operation</param>
		public BulkStreamDescriptor CreateMany<T>(IEnumerable<T> @objects, Func<BulkCreateDescriptor<T>, T, IBulkCreateOperation<T>> bulkCreateSelector = null)
			where T : class =>
			AddOperations(@objects, bulkCreateSelector, o => new BulkCreateDescriptor<T>().Document(o));

		public BulkStreamDescriptor Index<T>(Func<BulkIndexDescriptor<T>, IBulkIndexOperation<T>> bulkIndexSelector)
			where T : class =>
			AddOperation(bulkIndexSelector?.Invoke(new BulkIndexDescriptor<T>()));

		/// <summary>
		/// IndexMany, convenience method to pass many objects at once.
		/// </summary>
		/// <param name="objects">the objects to index</param>
		/// <param name="bulkIndexSelector">A func called on each object to describe the individual index operation</param>
		public BulkStreamDescriptor IndexMany<T>(IEnumerable<T> @objects, Func<BulkIndexDescriptor<T>, T, IBulkIndexOperation<T>> bulkIndexSelector = null)
			where T : class =>
			AddOperations(@objects, bulkIndexSelector, o => new BulkIndexDescriptor<T>().Document(o));

		/// <summary>
		/// DeleteMany, convenience method to delete many objects at once.
		/// </summary>
		/// <param name="objects">the objects to delete</param>
		/// <param name="bulkDeleteSelector">A func called on each object to describe the individual delete operation</param>
		public BulkStreamDescriptor DeleteMany<T>(
			IEnumerable<T> @objects,
			Func<BulkDeleteDescriptor<T>, T, IBulkDeleteOperation<T>> bulkDeleteSelector = null
		)
			where T : class =>
			AddOperations(@objects, bulkDeleteSelector, o => new BulkDeleteDescriptor<T>().Document(o));

		/// <summary>
		/// DeleteMany, convenience method to delete many objects at once.
		/// </summary>
		/// <param name="ids">Enumerable of string ids to delete</param>
		/// <param name="bulkDeleteSelector">A func called on each ids to describe the individual delete operation</param>
		public BulkStreamDescriptor DeleteMany<T>(
			IEnumerable<string> ids,
			Func<BulkDeleteDescriptor<T>, string, IBulkDeleteOperation<T>> bulkDeleteSelector = null
		)
			where T : class =>
			AddOperations(ids, bulkDeleteSelector, id => new BulkDeleteDescriptor<T>().Id(id));

		/// <summary>
		/// DeleteMany, convenience method to delete many objects at once.
		/// </summary>
		/// <param name="ids">Enumerable of int ids to delete</param>
		/// <param name="bulkDeleteSelector">A func called on each ids to describe the individual delete operation</param>
		public BulkStreamDescriptor DeleteMany<T>(
			IEnumerable<long> ids,
			Func<BulkDeleteDescriptor<T>, long, IBulkDeleteOperation<T>> bulkDeleteSelector = null
		)
			where T : class =>
			AddOperations(ids, bulkDeleteSelector, id => new BulkDeleteDescriptor<T>().Id(id));

		public BulkStreamDescriptor Delete<T>(T obj, Func<BulkDeleteDescriptor<T>, IBulkDeleteOperation<T>> bulkDeleteSelector = null)
			where T : class =>
			AddOperation(bulkDeleteSelector.InvokeOrDefault(new BulkDeleteDescriptor<T>().Document(obj)));

		public BulkStreamDescriptor Delete<T>(Func<BulkDeleteDescriptor<T>, IBulkDeleteOperation<T>> bulkDeleteSelector)
			where T : class =>
			AddOperation(bulkDeleteSelector?.Invoke(new BulkDeleteDescriptor<T>()));

		/// <summary>
		/// UpdateMany, convenience method to pass many objects at once to do multiple updates.
		/// </summary>
		/// <param name="objects">the objects to update</param>
		/// <param name="bulkUpdateSelector">An func called on each object to describe the individual update operation</param>
		public BulkStreamDescriptor UpdateMany<T>(
			IEnumerable<T> @objects,
			Func<BulkUpdateDescriptor<T, T>, T, IBulkUpdateOperation<T, T>> bulkUpdateSelector
		)
			where T : class =>
			AddOperations(objects, bulkUpdateSelector, o => new BulkUpdateDescriptor<T, T>().IdFrom(o));

		/// <summary>
		/// UpdateMany, convenience method to pass many objects at once to do multiple updates.
		/// </summary>
		/// <param name="objects">the objects to update</param>
		/// <param name="bulkUpdateSelector">An func called on each object to describe the individual update operation</param>
		public BulkStreamDescriptor UpdateMany<T, TPartialDocument>(
			IEnumerable<T> @objects,
			Func<BulkUpdateDescriptor<T, TPartialDocument>, T, IBulkUpdateOperation<T, TPartialDocument>> bulkUpdateSelector
		)
			where T : class
			where TPartialDocument : class =>
			AddOperations(objects, bulkUpdateSelector, o => new BulkUpdateDescriptor<T, TPartialDocument>().IdFrom(o));

		public BulkStreamDescriptor Update<T>(Func<BulkUpdateDescriptor<T, T>, IBulkUpdateOperation<T, T>> bulkUpdateSelector)
			where T : class =>
			Update<T, T>(bulkUpdateSelector);

		public BulkStreamDescriptor Update<T, TPartialDocument>(
			Func<BulkUpdateDescriptor<T, TPartialDocument>, IBulkUpdateOperation<T, TPartialDocument>> bulkUpdateSelector
		)
			where T : class
			where TPartialDocument : class =>
			AddOperation(bulkUpdateSelector?.Invoke(new BulkUpdateDescriptor<T, TPartialDocument>()));

		public BulkStreamDescriptor AddOperation(IBulkOperation operation) => Assign(operation, (a, v) => a.Operations.AddIfNotNull(v));

		private BulkStreamDescriptor AddOperations<T, TDescriptor, TInterface>(
			IEnumerable<T> objects,
			Func<TDescriptor, T, TInterface> bulkIndexSelector,
			Func<T,  TDescriptor> defaultSelector
		)
			where TInterface : class, IBulkOperation
			where TDescriptor : class, TInterface
		{
			if (@objects == null) return this;

			var objectsList = @objects.ToList();
			var operations = new List<TInterface>(objectsList.Count());
			foreach (var o in objectsList)
			{
				var op = bulkIndexSelector.InvokeOrDefault(defaultSelector(o), o);
				if (op != null) operations.Add(op);
			}
			return Assign(operations, (a, v) => a.Operations.AddRange(v));
		}

	}
}
