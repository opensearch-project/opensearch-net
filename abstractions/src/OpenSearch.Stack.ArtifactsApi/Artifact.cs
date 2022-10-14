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
using System.IO;
using OpenSearch.Stack.ArtifactsApi.Products;
using OpenSearch.Stack.ArtifactsApi.Resolvers;
using Version = SemVer.Version;

namespace OpenSearch.Stack.ArtifactsApi
{
	public class Artifact
	{
		private static readonly Uri BaseUri = new Uri("http://localhost");

		internal Artifact(Product product, Version version, string downloadUrl, ArtifactBuildState state,
			string buildHash)
		{
			ProductName = product?.ProductName ?? "opensearch";
			Version = version;
			DownloadUrl = product?.PatchDownloadUrl(downloadUrl);
			State = state;
			BuildHash = buildHash;
		}

		internal Artifact(Product product, Version version, SearchPackage package, string buildHash = null)
		{
			ProductName = product.ProductName;
			Version = version;
			State = ArtifactBuildState.Snapshot;
			DownloadUrl = product?.PatchDownloadUrl(package.DownloadUrl);
			ShaUrl = package.ShaUrl;
			AscUrl = package.AscUrl;
			BuildHash = buildHash;
		}

		public string LocalFolderName
		{
			get
			{
				var hashed = string.IsNullOrWhiteSpace(BuildHash) ? string.Empty : $"-build-{BuildHash}";
				switch (State)
				{
					case ArtifactBuildState.Released:
						return $"{ProductName}-{Version}";
					case ArtifactBuildState.Snapshot:
						return $"{ProductName}-{Version}{hashed}";
					case ArtifactBuildState.BuildCandidate:
						return $"{ProductName}-{Version}{hashed}";
					default:
						throw new ArgumentOutOfRangeException(nameof(State), $"{State} not expected here");
				}
			}
		}

		public string FolderInZip => $"{ProductName}-{Version}";

		public string Archive
		{
			get
			{
				if (!Uri.TryCreate(DownloadUrl, UriKind.Absolute, out var uri))
					uri = new Uri(BaseUri, DownloadUrl);

				return Path.GetFileName(uri.LocalPath);
			}
		}

		// ReSharper disable UnusedAutoPropertyAccessor.Global
		public string ProductName { get; }
		public Version Version { get; }
		public string DownloadUrl { get; }

		public ArtifactBuildState State { get; }
		public string BuildHash { get; }
		public string ShaUrl { get; }

		public string AscUrl { get; }
		// ReSharper restore UnusedAutoPropertyAccessor.Global
	}
}
