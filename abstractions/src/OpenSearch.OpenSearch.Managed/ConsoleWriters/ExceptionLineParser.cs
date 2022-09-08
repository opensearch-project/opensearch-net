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

using System.Text.RegularExpressions;

namespace OpenSearch.OpenSearch.Managed.ConsoleWriters
{
	public static class ExceptionLineParser
	{
		private static readonly Regex CauseRegex = new Regex(@"^(?<cause>.*?Exception:)(?<message>.*?)$");

		private static readonly Regex
			LocRegex = new Regex(@"^(?<at>\s*?at )(?<method>.*?)\((?<file>.*?)\)(?<jar>.*?)$");

		public static bool TryParseCause(string line, out string cause, out string message)
		{
			cause = message = null;
			if (string.IsNullOrEmpty(line)) return false;
			var match = CauseRegex.Match(line);
			if (!match.Success) return false;
			cause = match.Groups["cause"].Value.Trim();
			message = match.Groups["message"].Value.Trim();
			return true;
		}

		public static bool TryParseStackTrace(string line, out string at, out string method, out string file,
			out string jar)
		{
			at = method = file = jar = null;
			if (string.IsNullOrEmpty(line)) return false;
			var match = LocRegex.Match(line);
			if (!match.Success) return false;
			at = match.Groups["at"].Value;
			method = match.Groups["method"].Value.Trim();
			file = match.Groups["file"].Value.Trim();
			jar = match.Groups["jar"].Value.Trim();
			return true;
		}
	}
}
