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

using System.Runtime.Serialization;

namespace OpenSearch.Client.Specification.CatApi
{
	public class CatTasksRecord : ICatRecord
	{
		[DataMember(Name ="action")]
		public string Action { get; internal set; }

		[DataMember(Name ="ip")]
		public string Ip { get; internal set; }

		[DataMember(Name ="node")]
		public string Node { get; internal set; }

		[DataMember(Name ="parent_task_id")]
		public string ParentTaskId { get; internal set; }

		[DataMember(Name ="running_time")]
		public string RunningTime { get; internal set; }

		[DataMember(Name ="start_time")]
		public string StartTime { get; internal set; }

		[DataMember(Name ="task_id")]
		public string TaskId { get; internal set; }

		[DataMember(Name ="timestamp")]
		public string Timestamp { get; internal set; }

		[DataMember(Name ="type")]
		public string Type { get; internal set; }
	}
}
