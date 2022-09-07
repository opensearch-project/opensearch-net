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
using System.Runtime.InteropServices;
using OpenSearch.Stack.ArtifactsApi.Platform;
using OpenSearch.Stack.ArtifactsApi.Products;
using Version = SemVer.Version;

namespace OpenSearch.Stack.ArtifactsApi.Resolvers
{
	public static class ReleasedVersionResolver
	{
		private const string ArtifactsUrl = "https://artifacts.opensearch.org";

		public static bool TryResolve(Product product, Version version, OSPlatform platform, Architecture architecture, out Artifact artifact)
		{
			var downloadUrl = "";
			switch (product.ServerType)
			{
				case ServerType.OpenSearch:
					var productMoniker = product.Moniker;
					var platformMoniker = OsMonikers.GetOpenSearchPlatformMoniker(platform, architecture);
					var downloadPath = $"{ArtifactsUrl}/releases/bundle/{product}/{version}";
					var extension = OsMonikers.GetPlatformArchiveExtension(platform);
					var archive = $"{productMoniker}-{version}-{platformMoniker}.{extension}";
					downloadUrl = $"{downloadPath}/{archive}";
					break;
				case ServerType.OpenDistro:
					if (platform == OSPlatform.Linux)
					{
						// Only version 1.13.x supported, because it is based on elasticsearch 7.10.2: https://opendistro.github.io/for-elasticsearch-docs/version-history/
						if (version < new Version("1.13.0"))
							throw new ArgumentOutOfRangeException($"This OpenDistro version {version} is not supported, only 1.13.0 to 1.13.3 are supported");
						downloadUrl = $"https://d3g5vo6xdbdb9a.cloudfront.net/tarball/opendistro-elasticsearch/opendistroforelasticsearch-{version}-linux-x64.{product.Extension}";
					}
					else if (platform == OSPlatform.Windows)
					{
						// No Windows package for version < 1.3.0
						if (version < new Version("1.13.0"))
							throw new ArgumentOutOfRangeException($"This OpenDistro version {version} is not supported, only 1.13.0 to 1.13.3 are supported");
						downloadUrl = $"https://d3g5vo6xdbdb9a.cloudfront.net/downloads/odfe-windows/ode-windows-zip/opendistroforelasticsearch-{version}-windows-x64.{product.Extension}";
					}
					else
						throw new ArgumentOutOfRangeException($"OS {platform} is not supported by OpenDistro");
					break;
				case ServerType.ElasticSearch:
					// Only Elasticsearch v.7.10.2 is supported
					if (version != new Version("7.10.2"))
						throw new ArgumentOutOfRangeException($"Elasticsearch version {version} is not supported, the only supported version is 7.10.2");
					if (platform == OSPlatform.Windows) downloadUrl = "https://artifacts.elastic.co/downloads/elasticsearch/elasticsearch-7.10.2-windows-x86_64.zip";
					if (platform == OSPlatform.Linux) downloadUrl = "https://artifacts.elastic.co/downloads/elasticsearch/elasticsearch-7.10.2-linux-x86_64.tar.gz";
					if (platform == OSPlatform.OSX) downloadUrl = "https://artifacts.elastic.co/downloads/elasticsearch/elasticsearch-7.10.2-darwin-x86_64.tar.gz";
					break;
			}

			artifact = new Artifact(product, version, downloadUrl, ArtifactBuildState.Released, null);
			return true;
		}
	}
}
