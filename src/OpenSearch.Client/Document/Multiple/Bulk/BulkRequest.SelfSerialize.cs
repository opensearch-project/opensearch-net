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

using System.IO;
using OpenSearch.Net;

namespace OpenSearch.Client
{
	// The _bulk body is NDJSON, which cannot be produced through System.Text.Json's single-root writer;
	// these requests write their body directly to the stream when the STJ serializer is in use (#388).
	public partial class BulkRequest : ISystemTextJsonSelfSerializable
	{
		void ISystemTextJsonSelfSerializable.Write(Stream stream, IOpenSearchSerializer builtInSerializer, SerializationFormatting formatting) =>
			BulkRequestJsonSerializer.Write(this, stream, builtInSerializer);
	}

	public partial class BulkDescriptor : ISystemTextJsonSelfSerializable
	{
		void ISystemTextJsonSelfSerializable.Write(Stream stream, IOpenSearchSerializer builtInSerializer, SerializationFormatting formatting) =>
			BulkRequestJsonSerializer.Write(this, stream, builtInSerializer);
	}
}
