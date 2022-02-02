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

namespace OpenSearch.Net
{
	internal abstract class VersionInfo
	{
		protected const string EmptyVersion = "0.0.0";

		public Version Version { get; protected set; }
		public bool IsPrerelease { get; protected set; }

		protected void StoreVersion(string fullVersion)
		{
			if (string.IsNullOrEmpty(fullVersion))
				fullVersion = EmptyVersion;

			var clientVersion = GetParsableVersionPart(fullVersion);

			if (!Version.TryParse(clientVersion, out var parsedVersion))
				throw new ArgumentException("Invalid version string", nameof(fullVersion));

			var finalVersion = parsedVersion;

			if (parsedVersion.Minor == -1 || parsedVersion.Build == -1)
				finalVersion = new Version(parsedVersion.Major, parsedVersion.Minor > -1
					? parsedVersion.Minor
					: 0, parsedVersion.Build > -1
						? parsedVersion.Build
						: 0);

			Version = finalVersion;
			IsPrerelease = ContainsPrerelease(fullVersion);
		}

		protected virtual bool ContainsPrerelease(string version) => version.Contains("-");

		private static string GetParsableVersionPart(string fullVersionName) =>
			new string(fullVersionName.TakeWhile(c => char.IsDigit(c) || c == '.').ToArray());

		public override string ToString() => IsPrerelease ? Version.ToString() + "p" : Version.ToString();
	}
}
