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
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using OpenSearch.Stack.ArtifactsApi.Platform;
using OpenSearch.Stack.ArtifactsApi.Products;
using SemanticVersioning;
using Version = SemanticVersioning.Version;

namespace OpenSearch.Stack.ArtifactsApi.Resolvers
{
	public static class SnapshotApiResolver
	{
		public static readonly System.Lazy<IReadOnlyCollection<Version>> AvailableVersions =
			new System.Lazy<IReadOnlyCollection<Version>>(LoadVersions, LazyThreadSafetyMode.ExecutionAndPublication);

		private static Regex PackageProductRegex { get; } =
			new Regex(@"(.*?)-(\d+\.\d+\.\d+(?:-(?:SNAPSHOT|alpha\d+|beta\d+|rc\d+))?)");

		private static Version IncludeOsMoniker { get; } = new Version("1.0.0");

		public static Version LatestReleaseOrSnapshot => AvailableVersions.Value.OrderByDescending(v => v).First();

		public static bool TryResolve(Product product, Version version, OSPlatform os, string filters,
			out Artifact artifact)
		{
			artifact = null;
			var p = product.SubProduct?.SubProductName ?? product.ProductName;
			var query = p;
			if (product.PlatformDependent && version > product.PlatformSuffixAfter)
				query += $",{OsMonikers.From(os)}";
			else if (product.PlatformDependent)
				query += $",{OsMonikers.CurrentPlatformSearchFilter()}";
			if (!string.IsNullOrWhiteSpace(filters))
				query += $",{filters}";

			var packages = new Dictionary<string, SearchPackage>();
			try
			{
				var json = ApiResolver.FetchJson($"search/{version}/{query}");
				// if packages is empty it turns into an array[] otherwise its a dictionary :/
				packages = JsonSerializer.Deserialize<ArtifactsSearchResponse>(json).Packages;
			}
			catch
			{
			}

			if (packages == null || packages.Count == 0) return false;
			var list = packages
				.OrderByDescending(k => k.Value.Classifier == null ? 1 : 0)
				.ToArray();

			var ext = OsMonikers.CurrentPlatformArchiveExtension();
			var shouldEndWith = $"{version}.{ext}";
			if (product.PlatformDependent && version > product.PlatformSuffixAfter)
				shouldEndWith = $"{version}-{OsMonikers.CurrentPlatformPackageSuffix()}.{ext}";
			foreach (var kv in list)
			{
				if (product.PlatformDependent && !kv.Key.EndsWith(shouldEndWith)) continue;


				var tokens = PackageProductRegex.Split(kv.Key).Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
				if (tokens.Length < 2) continue;

				if (!tokens[0].Equals(p, StringComparison.CurrentCultureIgnoreCase)) continue;
				if (!tokens[1].Equals(version.ToString(), StringComparison.CurrentCultureIgnoreCase)) continue;
				var buildHash = ApiResolver.GetBuildHash(kv.Value.DownloadUrl);
				artifact = new Artifact(product, version, kv.Value, buildHash);
			}

			return false;
		}


		private static IReadOnlyCollection<Version> LoadVersions()
		{
			var json = ApiResolver.FetchJson("versions");
			var versions = JsonSerializer.Deserialize<ArtifactsVersionsResponse>(json).Versions;

			return new List<Version>(versions.Select(v => new Version(v)));
		}

		public static Version LatestSnapshotForMajor(int major)
		{
			var range = new Range($"~{major}");
			return AvailableVersions.Value
				.Reverse()
				.FirstOrDefault(v =>
					v.PreRelease == "SNAPSHOT" && range.IsSatisfied(v.ToString().Replace("-SNAPSHOT", "")));
		}

		public static Version LatestReleaseOrSnapshotForMajor(int major)
		{
			var range = new Range($"~{major}");
			return AvailableVersions.Value
				.Reverse()
				.FirstOrDefault(v => range.IsSatisfied(v.ToString().Replace("-SNAPSHOT", "")));
		}
	}
}
