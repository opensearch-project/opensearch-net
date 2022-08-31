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
using System.Net;
using OpenSearch.OpenSearch.Managed.ConsoleWriters;

namespace OpenSearch.OpenSearch.Ephemeral.Tasks.ValidationTasks
{
	public class ValidateClusterStateTask : ClusterComposeTask
	{
		public override void Run(IEphemeralCluster<EphemeralClusterConfiguration> cluster)
		{
			cluster.Writer.WriteDiagnostic(
				$"{{{nameof(ValidateClusterStateTask)}}} waiting cluster to go into yellow health state");
			var healthyResponse = Get(cluster, "_cluster/health", "wait_for_status=yellow&timeout=20s");
			if (healthyResponse == null || healthyResponse.StatusCode != HttpStatusCode.OK)
				throw new Exception(
					$"Cluster health waiting for status yellow failed after 20s {GetResponseException(healthyResponse)}");
		}
	}
}
