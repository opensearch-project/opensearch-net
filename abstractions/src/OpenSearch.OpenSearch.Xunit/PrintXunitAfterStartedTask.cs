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

using OpenSearch.OpenSearch.Ephemeral;
using OpenSearch.OpenSearch.Ephemeral.Tasks;
using OpenSearch.OpenSearch.Managed.ConsoleWriters;

namespace OpenSearch.OpenSearch.Xunit
{
	/// <summary>
	///     A task that writes a diagnostic message to indicate that tests will now run
	/// </summary>
	public class PrintXunitAfterStartedTask : ClusterComposeTask
	{
		/// <inheritdoc />
		public override void Run(IEphemeralCluster<EphemeralClusterConfiguration> cluster)
		{
			var name = cluster.GetType().Name;
			cluster.Writer.WriteDiagnostic($"All good! kicking off [{name}] tests now");
		}
	}
}
