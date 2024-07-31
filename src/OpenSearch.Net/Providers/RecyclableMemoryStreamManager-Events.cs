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

// ---------------------------------------------------------------------
// Copyright (c) 2015 Microsoft
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// ---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Tracing;
using System.Threading;

namespace OpenSearch.Net
{
#if !NETSTANDARD2_1 && !NET6_0_OR_GREATER
	internal class PollingCounter : IDisposable
	{
		// ReSharper disable UnusedParameter.Local
		public PollingCounter(string largeBuffers, RecyclableMemoryStreamManager.Events eventsWriter, Func<double> func) { }
		// ReSharper restore UnusedParameter.Local

		public void Dispose() {}
	}
#endif

	internal sealed partial class RecyclableMemoryStreamManager
	{
		public static readonly Events EventsWriter = new Events();

		[EventSource(Name = "OpenSearch-Net-RecyclableMemoryStream", Guid = "{AD44FDAC-D3FC-460A-9EBE-E55A3569A8F6}")]
		public sealed class Events : EventSource
		{

			public enum MemoryStreamBufferType
			{
				Small,
				Large
			}

			public enum MemoryStreamDiscardReason
			{
				TooLarge,
				EnoughFree
			}

			[Event(1, Level = EventLevel.Verbose)]
			public void MemoryStreamCreated(Guid guid, string tag, int requestedSize)
			{
				if (IsEnabled(EventLevel.Verbose, EventKeywords.None))
					WriteEvent(1, guid, tag ?? string.Empty, requestedSize);
			}

			[Event(2, Level = EventLevel.Verbose)]
			public void MemoryStreamDisposed(Guid guid, string tag)
			{
				if (IsEnabled(EventLevel.Verbose, EventKeywords.None))
					WriteEvent(2, guid, tag ?? string.Empty);
			}

			[Event(3, Level = EventLevel.Critical)]
			public void MemoryStreamDoubleDispose(Guid guid, string tag, string allocationStack, string disposeStack1, string disposeStack2)
			{
				if (IsEnabled())
					WriteEvent(3, guid, tag ?? string.Empty, allocationStack ?? string.Empty,
						disposeStack1 ?? string.Empty, disposeStack2 ?? string.Empty);
			}

			[Event(4, Level = EventLevel.Error)]
			public void MemoryStreamFinalized(Guid guid, string tag, string allocationStack)
			{
				if (IsEnabled())
					WriteEvent(4, guid, tag ?? string.Empty, allocationStack ?? string.Empty);
			}

			[Event(5, Level = EventLevel.Verbose)]
			public void MemoryStreamToArray(Guid guid, string tag, string stack, int size)
			{
				if (IsEnabled(EventLevel.Verbose, EventKeywords.None))
					WriteEvent(5, guid, tag ?? string.Empty, stack ?? string.Empty, size);
			}

			[Event(6, Level = EventLevel.Informational)]
			public void MemoryStreamManagerInitialized(int blockSize, int largeBufferMultiple, int maximumBufferSize)
			{
				if (IsEnabled())
					WriteEvent(6, blockSize, largeBufferMultiple, maximumBufferSize);
			}

			[Event(7, Level = EventLevel.Verbose)]
			public void MemoryStreamNewBlockCreated(long smallPoolInUseBytes)
			{
				if (IsEnabled(EventLevel.Verbose, EventKeywords.None))
					WriteEvent(7, smallPoolInUseBytes);
			}

			[Event(8, Level = EventLevel.Verbose)]
			public void MemoryStreamNewLargeBufferCreated(int requiredSize, long largePoolInUseBytes)
			{
				if (IsEnabled(EventLevel.Verbose, EventKeywords.None))
					WriteEvent(8, requiredSize, largePoolInUseBytes);
			}

			[Event(9, Level = EventLevel.Verbose)]
			public void MemoryStreamNonPooledLargeBufferCreated(int requiredSize, string tag, string allocationStack)
			{
				if (IsEnabled(EventLevel.Verbose, EventKeywords.None))
					WriteEvent(9, requiredSize, tag ?? string.Empty, allocationStack ?? string.Empty);
			}

			[Event(10, Level = EventLevel.Warning)]
			public void MemoryStreamDiscardBuffer(MemoryStreamBufferType bufferType, string tag, MemoryStreamDiscardReason reason)
			{
				if (IsEnabled())
					WriteEvent(10, bufferType, tag ?? string.Empty, reason);
			}

			[Event(11, Level = EventLevel.Error)]
			public void MemoryStreamOverCapacity(int requestedCapacity, long maxCapacity, string tag, string allocationStack)
			{
				if (IsEnabled())
					WriteEvent(11, requestedCapacity, maxCapacity, tag ?? string.Empty, allocationStack ?? string.Empty);
			}
		}
	}
}
