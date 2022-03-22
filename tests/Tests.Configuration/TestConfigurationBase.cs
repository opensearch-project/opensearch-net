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
using System.Linq;
using OpenSearch.Stack.ArtifactsApi;

namespace Tests.Configuration
{
	public abstract class TestConfigurationBase
	{
		/// <summary> If specified will only run integration test against these comma separated clusters </summary>
		public string ClusterFilter { get; protected set; }

		/// <summary> comma separated list of test method names to execute </summary>
		public string TestFilter { get; protected set; }

		/// <summary> The OpenSearch version to test against, defined for both unit and integration tests</summary>
		public OpenSearchVersion OpenSearchVersion { get; protected set; }

		public bool OpenSearchVersionIsSnapshot => OpenSearchVersion.ArtifactBuildState == ArtifactBuildState.Snapshot;

		/// <summary> Force a reseed (bootstrap) of the cluster even if checks indicate bootstrap already ran </summary>
		public bool ForceReseed { get; protected set; }

		/// <summary>
		/// Signals to our test framework that the cluster was started externally. The framework will assert this before
		/// attempting to spin up a cluster. If no node is found the framework will still spin up a node.
		/// </summary>
		public bool TestAgainstAlreadyRunningOpenSearch { get; protected set; }

		/// <summary> The mode to run the tests under <see cref="TestMode"/></summary>
		public TestMode Mode { get; protected set; }

		/// <summary> Some test parameters are randomized, these are found under this property</summary>
		public RandomConfiguration Random { get; protected set; }

		/// <summary> The current configuration signals integration tests should be run </summary>
		public bool RunIntegrationTests => Mode == TestMode.Mixed || Mode == TestMode.Integration;

		/// <summary> The current configuration signals unit tests should be run </summary>
		public bool RunUnitTests => Mode == TestMode.Mixed || Mode == TestMode.Unit;

		/// <summary> The current configured seed used for random configuration </summary>
		public int Seed { get; private set; }

		/// <summary> whether the seed was provided externally to be fixated </summary>
		public bool SeedProvidedExternally { get; private set; }

		/// <summary>
		/// This is fixed for now, specifying false leads to flaky tests, warrants deeper investigation
		/// in our abstractions project
		/// </summary>
		public bool ShowOpenSearchOutputAfterStarted { get; } = true;

		/// <summary> When specified will only run one overload in API tests, helpful when debugging locally </summary>
		public bool TestOnlyOne { get; protected set; }

		public ServerType ServerType { get; protected set; }

		private static int CurrentSeed { get; } = new Random().Next(1, 1_00_000);

		protected void SetExternalSeed(int? seed, out Random randomizer)
		{
			SeedProvidedExternally = seed.HasValue;
			Seed = seed.GetValueOrDefault(CurrentSeed);
			randomizer = new Random(Seed);
		}

		protected void ParseServerType(string version)
		{
			// Assuming received string might be in format '$version' or '$ServerType-$version'
			var hasServerType = Enum.GetNames(ServerType.GetType()).Any(s => version.StartsWith(s, StringComparison.InvariantCultureIgnoreCase));
			if (hasServerType)
			{
				var parts = version.Split('-');
				ServerType = (ServerType)Enum.Parse(ServerType.GetType(), parts[0], true);
			}
			else
				ServerType = ServerType.OpenSearch;
		}
	}

	public class RandomConfiguration
	{
		/// <summary> Run tests with a custom source serializer rather than the build in one </summary>
		public bool SourceSerializer { get; set; }

		/// <summary> Randomly enable typed keys on searches (defaults to true) on OSC search requests</summary>
		public bool TypedKeys { get; set; }

		/// <summary> Randomly enable compression on the http requests</summary>
		public bool HttpCompression { get; set; }
	}
}
