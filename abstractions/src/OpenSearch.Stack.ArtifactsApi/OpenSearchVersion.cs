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
using System.Collections.Concurrent;
using System.Linq;
using System.Runtime.InteropServices;
using OpenSearch.Stack.ArtifactsApi.Platform;
using OpenSearch.Stack.ArtifactsApi.Products;
using OpenSearch.Stack.ArtifactsApi.Resolvers;
using SemVer;
using Version = SemVer.Version;

namespace OpenSearch.Stack.ArtifactsApi
{
	public class OpenSearchVersion : Version, IComparable<string>
	{
		private readonly ConcurrentDictionary<string, Artifact> _resolved = new();

		protected OpenSearchVersion(string version, ArtifactBuildState state, string buildHash = null) : base(version)
		{
			ArtifactBuildState = state;
			BuildHash = buildHash;
		}

		public ArtifactBuildState ArtifactBuildState { get; }
		private string BuildHash { get; }

		public int CompareTo(string other)
		{
			var v = (OpenSearchVersion)other;
			return CompareTo(v);
		}

		public Artifact Artifact(Product product)
		{
			var cacheKey = product.ToString();
			if (_resolved.TryGetValue(cacheKey, out var artifact))
				return artifact;
			var currentPlatform = OsMonikers.CurrentPlatform();
			switch (ArtifactBuildState)
			{
				case ArtifactBuildState.Released:
					ReleasedVersionResolver.TryResolve(product, this, currentPlatform, RuntimeInformation.OSArchitecture, out artifact);
					break;
				case ArtifactBuildState.Snapshot:
					SnapshotApiResolver.TryResolve(product, this, currentPlatform, null, out artifact);
					break;
				case ArtifactBuildState.BuildCandidate:
					StagingVersionResolver.TryResolve(product, this, BuildHash, out artifact);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(ArtifactBuildState),
						$"{ArtifactBuildState} not expected here");
			}

			_resolved.TryAdd(cacheKey, artifact);

			return artifact;
		}

		/// <summary>
		///     Resolves an OpenSearch version using either format '$version' or '$ServerType-$version', where version could be 'x.y.z' or 'latest' or even 'latest-x'
		/// </summary>
		public static OpenSearchVersion From(string managedVersionString)
		{
			if (managedVersionString == null)
				return null;

			var serverType = ServerType.DEFAULT;
			var hasServerType = Enum.GetNames(typeof(ServerType)).Any(s => managedVersionString.StartsWith(s, StringComparison.InvariantCultureIgnoreCase));
			if (hasServerType)
			{
				var parts = managedVersionString.Split('-');
				serverType = (ServerType)Enum.Parse(typeof(ServerType), parts[0], true);
				managedVersionString = string.Join("-", parts.Skip(1));
			}

			if (managedVersionString.StartsWith("latest") && serverType == ServerType.OpenDistro)
				// No releases for OpenDistro after 1.13.3 - all deveploment afterwards goes to OpenSearch
				managedVersionString = "1.13.3";
			if (managedVersionString.StartsWith("latest") && serverType == ServerType.ElasticSearch)
				// OpenDistro (1.13.x - another not supported) and OpenSearch both based on Elasticsearch v.7.10.2, other versions are incompatible
				managedVersionString = "7.10.2";

			// TODO resolve `latest` and `latest-x` for OpenSearch

			return new OpenSearchVersion(managedVersionString, ArtifactBuildState.Released, "");
		}

		internal static bool TryParseBuildCandidate(string passedVersion, out string version, out string gitHash)
		{
			version = null;
			gitHash = null;
			var tokens = passedVersion.Split(':');
			if (tokens.Length < 2)
				return false;
			version = tokens[1].Trim();
			gitHash = tokens[0].Trim();
			return true;
		}

		public bool InRange(string range)
		{
			var versionRange = new Range(range);
			return InRange(versionRange);
		}

		public bool InRange(Range versionRange)
		{
			var satisfied = versionRange.IsSatisfied(this);
			if (satisfied)
				return true;

			//Semver can only match snapshot version with ranges on the same major and minor
			//anything else fails but we want to know e.g 1.0.0-SNAPSHOT satisfied by 1.0.0;
			var wholeVersion = $"{Major}.{Minor}.{Patch}";
			return versionRange.IsSatisfied(wholeVersion);
		}


		public static implicit operator OpenSearchVersion(string version) => From(version);

		public static bool operator <(OpenSearchVersion first, string second) => first < (OpenSearchVersion)second;
		public static bool operator >(OpenSearchVersion first, string second) => first > (OpenSearchVersion)second;

		public static bool operator <(string first, OpenSearchVersion second) => (OpenSearchVersion)first < second;
		public static bool operator >(string first, OpenSearchVersion second) => (OpenSearchVersion)first > second;

		public static bool operator <=(OpenSearchVersion first, string second) => first <= (OpenSearchVersion)second;
		public static bool operator >=(OpenSearchVersion first, string second) => first >= (OpenSearchVersion)second;

		public static bool operator <=(string first, OpenSearchVersion second) => (OpenSearchVersion)first <= second;
		public static bool operator >=(string first, OpenSearchVersion second) => (OpenSearchVersion)first >= second;

		public static bool operator ==(OpenSearchVersion first, string second) => first == (OpenSearchVersion)second;
		public static bool operator !=(OpenSearchVersion first, string second) => first != (OpenSearchVersion)second;


		public static bool operator ==(string first, OpenSearchVersion second) => (OpenSearchVersion)first == second;
		public static bool operator !=(string first, OpenSearchVersion second) => (OpenSearchVersion)first != second;

		// ReSharper disable once UnusedMember.Local
		private bool Equals(OpenSearchVersion other) => base.Equals(other);
		public override bool Equals(object obj) => base.Equals(obj);

		public override int GetHashCode() => base.GetHashCode();
	}
}
