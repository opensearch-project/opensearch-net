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
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using OpenSearch.Net;

namespace OpenSearch.Client.Specification.SnapshotApi
{
	public class RestoreErrorEventArgs : EventArgs
	{
		public RestoreErrorEventArgs(Exception exception) => Exception = exception;

		public Exception Exception { get; }
	}

	public class RestoreStatusHumbleObject
	{
		private readonly IOpenSearchClient _opensearchClient;
		private readonly string _renamePattern;
		private readonly string _renameReplacement;
		private readonly IRestoreRequest _restoreRequest;

		public RestoreStatusHumbleObject(IOpenSearchClient opensearchClient, IRestoreRequest restoreRequest)
		{
			opensearchClient.ThrowIfNull(nameof(opensearchClient));
			restoreRequest.ThrowIfNull(nameof(restoreRequest));

			_opensearchClient = opensearchClient;
			_restoreRequest = restoreRequest;
			_renamePattern = string.IsNullOrEmpty(_restoreRequest.RenamePattern) ? string.Empty : _restoreRequest.RenamePattern;
			_renameReplacement = string.IsNullOrEmpty(_restoreRequest.RenameReplacement) ? string.Empty : _restoreRequest.RenameReplacement;
		}
	}
}
