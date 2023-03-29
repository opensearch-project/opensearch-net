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

using System.IO;
using System.Reflection;

namespace ApiGenerator.Configuration
{
	public static class GeneratorLocations
	{
		// @formatter:off — disable formatter after this line
		public static string OpenSearchNetFolder { get; } = $@"{Root}../../src/OpenSearch.Net/";
		public static string LastDownloadedRef { get; } = Path.Combine(Root, "last_downloaded_version.txt");

		public static string OpenSearchClientFolder { get; } = $@"{Root}../../src/OpenSearch.Client/";
		public static string RestSpecificationFolder { get; } = $@"{Root}RestSpecification/";
		// @formatter:on — enable formatter after this line

		public static string HighLevel(params string[] paths) => OpenSearchClientFolder + string.Join("/", paths);
		public static string LowLevel(params string[] paths) => OpenSearchNetFolder + string.Join("/", paths);

		public static readonly Assembly Assembly = typeof(Generator.ApiGenerator).Assembly;

		private static string _root;
		public static string Root
		{
			get
			{
				if (_root != null) return _root;

				var directoryInfo = new DirectoryInfo(Directory.GetCurrentDirectory());

				var dotnetRun =
					directoryInfo.Name == "ApiGenerator" &&
					directoryInfo.Parent != null &&
					directoryInfo.Parent.Name == "src";

				_root = dotnetRun ? "" : @"../../../";
				return _root;
			}
		}

	}
}
