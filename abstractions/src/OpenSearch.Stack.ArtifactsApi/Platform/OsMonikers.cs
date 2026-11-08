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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
namespace OpenSearch.Stack.ArtifactsApi.Platform
{
	internal static class OsMonikers
	{
		public static readonly string Windows = "windows";
		public static readonly string Linux = "linux";
		public static readonly string OSX = "darwin";

		public static string From(OSPlatform platform)
		{
			if (platform == OSPlatform.Windows) return Windows;
			if (platform == OSPlatform.Linux) return Linux;
			if (platform == OSPlatform.OSX) return OSX;
			return "unknown";
		}

		public static OSPlatform CurrentPlatform()
		{
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return OSPlatform.Windows;
			if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) return OSPlatform.OSX;
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) return OSPlatform.Linux;
			throw new Exception(
				$"{RuntimeInformation.OSDescription} is currently not supported please open an issue @opensearch-project/opensearch-net");
		}

		/// <summary>
		/// Maps from current os and architecture to a suffix used for tagging platform-specific OpenSearch releases.
		/// </summary>
		/// <returns></returns>
		public static string GetOpenSearchPlatformMoniker(OSPlatform platform, Architecture architecture)
		{
			if (platform == OSPlatform.Windows)
			{
				if (architecture == Architecture.X64) return "windows-x64";
				if (architecture == Architecture.X86) return "windows-x86";
			}

			if (platform == OSPlatform.OSX)
			{
				if (architecture == Architecture.X64) return "macos-x64";
				if (architecture == Architecture.Arm64) return "macos-arm64";

			}

			if (platform == OSPlatform.Linux)
			{
				if (architecture == Architecture.X64) return "linux-x64";
				if (architecture == Architecture.Arm64) return "linux-arm64";
			}
			throw new Exception(
				$"{RuntimeInformation.OSDescription} is currently not supported please open an issue @opensearch-project/opensearch-net");
		}

		public static string GetPlatformArchiveExtension(OSPlatform platform)
		{
			if (platform == OSPlatform.Linux) return "tar.gz";
			if (platform == OSPlatform.OSX) return "tar.gz";
			if (platform == OSPlatform.Windows) return "zip";
			throw new Exception(
				$"{RuntimeInformation.OSDescription} is currently not supported please open an issue @opensearch-project/opensearch-net");

		}
		public static string CurrentPlatformArchiveExtension()
		{
			var platform = CurrentPlatform();
			return GetPlatformArchiveExtension(platform);
		}

		public static string CurrentPlatformPackageSuffix()
		{
			var intelX86Suffix = "x86_64";
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return $"{Windows}-{intelX86Suffix}";
			if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) return $"{OSX}-{intelX86Suffix}";
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) return $"{Linux}-{intelX86Suffix}";

			throw new Exception(
				$"{RuntimeInformation.OSDescription} is currently not supported please open an issue @opensearch-project/opensearch-net");
		}

		internal static string CurrentPlatformSearchFilter()
		{
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return "zip";
			if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) return "tar";
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) return "tar";

			throw new Exception(
				$"{RuntimeInformation.OSDescription} is currently not supported please open an issue @opensearch-project/opensearch-net");
		}
	}
}
