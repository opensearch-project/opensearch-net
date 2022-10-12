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
			var productMoniker = product.Moniker;
			var platformMoniker = OsMonikers.GetOpenSearchPlatformMoniker(platform, architecture);
			var downloadPath = $"{ArtifactsUrl}/releases/bundle/{product}/{version}";
			var extension = OsMonikers.GetPlatformArchiveExtension(platform);
			var archive = $"{productMoniker}-{version}-{platformMoniker}.{extension}";
			var downloadUrl = $"{downloadPath}/{archive}";

			artifact = new Artifact(product, version, downloadUrl, ArtifactBuildState.Released, null);
			return true;
		}
	}
}
