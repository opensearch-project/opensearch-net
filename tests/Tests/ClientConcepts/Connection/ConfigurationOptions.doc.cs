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
using OpenSearch.Net;
using Osc;
using Tests.Domain;

namespace Tests.ClientConcepts.Connection
{
	public class ConfigurationOptions
	{
		/**[[configuration-options]]
		 * === Configuration options
		 *
		 * Connecting to OpenSearch with <<opensearch-net-getting-started,OpenSearch.Net>> and <<nest-getting-started,NEST>> is easy, but
		 * it's entirely possible that you'd like to change the default connection behaviour. There are a number of configuration options available
		 * on `ConnectionConfiguration` for the low level client and `ConnectionSettings` for the high level client that can be used to control
		 * how the clients interact with OpenSearch.
		 *
		 * ==== Options on ConnectionConfiguration
		 *
		 * The following is a list of available connection configuration options on `ConnectionConfiguration`; since
		 * `ConnectionSettings` derives from `ConnectionConfiguration`, these options are available for both
		 * the low level and high level client:
		 *
		 * :xml-docs: OpenSearch.Net:ConnectionConfiguration`1
		 *
		 * ==== ConnectionConfiguration with OpenSearchLowLevelClient
		 *
		 * Here's an example to demonstrate setting several configuration options using the low level client
		 */
		public void AvailableOptions()
		{
			var connectionConfiguration = new ConnectionConfiguration()
				.DisableAutomaticProxyDetection()
				.EnableHttpCompression()
				.DisableDirectStreaming()
				.PrettyJson()
				.RequestTimeout(TimeSpan.FromMinutes(2));

			var lowLevelClient = new OpenSearchLowLevelClient(connectionConfiguration);

			/**
			 * ==== Options on ConnectionSettings
			 *
			 * The following is a list of available connection configuration options on `ConnectionSettings`:
			 *
			 * :xml-docs: Nest:ConnectionSettingsBase`1
			 *
			 * ==== ConnectionSettings with OpenSearchClient
			 *
			 * Here's an example to demonstrate setting several configuration options using the high level client
			 */
			var connectionSettings = new ConnectionSettings()
				.DefaultMappingFor<Project>(i => i
					.IndexName("my-projects")
					.IdProperty(p => p.Name)
				)
				.EnableDebugMode()
				.PrettyJson()
				.RequestTimeout(TimeSpan.FromMinutes(2));

			var client = new OpenSearchClient(connectionSettings);

			/**[NOTE]
			* ====
			*
			* Basic Authentication credentials can alternatively be specified on the node URI directly
			*/
			var uri = new Uri("http://username:password@localhost:9200");
			var settings = new ConnectionConfiguration(uri);
		}
		/**
		* but this can be awkward when using connection pooling with multiple nodes, especially when the connection pool
		* used is one that is capable of reseeding itself. For this reason, we'd recommend specifying credentials
		* on `ConnectionSettings`.
		*====
		*/
	}
}
