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
using System.Text.RegularExpressions;
using OpenSearch.Stack.ArtifactsApi;

namespace OpenSearch.OpenSearch.Managed.ConsoleWriters
{
	public class LineOutParser
	{
		private LineOutParser() { }

		public static readonly LineOutParser OpenSearch = new(shortNamePrefix: "o.o", fullNamePrefix: "org.opensearch",
			securityPluginName: "OpenSearchSecurityPlugin");

		public static LineOutParser From(ServerType serverType)
		{
			switch (serverType)
			{
				case ServerType.OpenSearch:
					return LineOutParser.OpenSearch;
				default:
					throw new ApplicationException(
						$"Unexpected value for {nameof(serverType)} -- {serverType}");
			}
		}

/*
[2016-09-26T11:43:17,314][INFO ][o.e.n.Node               ] [readonly-node-a9c5f4] initializing ...
[2016-09-26T11:43:17,470][INFO ][o.e.e.NodeEnvironment    ] [readonly-node-a9c5f4] using [1] data paths, mounts [[BOOTCAMP (C:)]], net usable_space [27.7gb], net total_space [129.7gb], spins? [unknown], types [NTFS]
[2016-09-26T11:43:17,471][INFO ][o.e.e.NodeEnvironment    ] [readonly-node-a9c5f4] heap size [1.9gb], compressed ordinary object pointers [true]
[2016-09-26T11:43:17,475][INFO ][o.e.n.Node               ] [readonly-node-a9c5f4] version[5.0.0-beta1], pid[13172], build[7eb6260/2016-09-20T23:10:37.942Z], OS[Windows 10/10.0/amd64], JVM[Oracle Corporation/Java HotSpot(TM) 64-Bit Server VM/1.8.0_101/25.101-b13]
[2016-09-26T11:43:19,160][INFO ][o.e.p.PluginsService     ] [readonly-node-a9c5f4] loaded module [aggs-matrix-stats]
[2016-09-26T11:43:19,160][INFO ][o.e.p.PluginsService     ] [readonly-node-a9c5f4] loaded module [ingest-common]
[2016-09-26T11:43:19,161][INFO ][o.e.p.PluginsService     ] [readonly-node-a9c5f4] loaded module [lang-expression]
[2016-09-26T11:43:19,161][INFO ][o.e.p.PluginsService     ] [readonly-node-a9c5f4] loaded module [lang-groovy]
[2016-09-26T11:43:19,161][INFO ][o.e.p.PluginsService     ] [readonly-node-a9c5f4] loaded module [lang-mustache]
[2016-09-26T11:43:19,162][INFO ][o.e.p.PluginsService     ] [readonly-node-a9c5f4] loaded module [lang-painless]
[2016-09-26T11:43:19,162][INFO ][o.e.p.PluginsService     ] [readonly-node-a9c5f4] loaded module [percolator]
[2016-09-26T11:43:19,162][INFO ][o.e.p.PluginsService     ] [readonly-node-a9c5f4] loaded module [reindex]
[2016-09-26T11:43:19,162][INFO ][o.e.p.PluginsService     ] [readonly-node-a9c5f4] loaded module [transport-netty3]
[2016-09-26T11:43:19,163][INFO ][o.e.p.PluginsService     ] [readonly-node-a9c5f4] loaded module [transport-netty4]
[2016-09-26T11:43:19,163][INFO ][o.e.p.PluginsService     ] [readonly-node-a9c5f4] loaded plugin [ingest-attachment]
[2016-09-26T11:43:19,164][INFO ][o.e.p.PluginsService     ] [readonly-node-a9c5f4] loaded plugin [ingest-geoip]
[2016-09-26T11:43:19,164][INFO ][o.e.p.PluginsService     ] [readonly-node-a9c5f4] loaded plugin [mapper-attachments]
[2016-09-26T11:43:19,164][INFO ][o.e.p.PluginsService     ] [readonly-node-a9c5f4] loaded plugin [mapper-murmur3]
[2016-09-26T11:43:19,374][WARN ][d.m.attachment           ] [mapper-attachments] plugin has been deprecated and will be replaced by [ingest-attachment] plugin.
[2016-09-26T11:43:22,179][INFO ][o.e.n.Node               ] [readonly-node-a9c5f4] initialized
[2016-09-26T11:43:22,180][INFO ][o.e.n.Node               ] [readonly-node-a9c5f4] starting ...
*/
		private static readonly Regex ConsoleLineParser =
			new Regex(@"\[(?<date>.*?)\]\[(?<level>.*?)\](?:\[(?<section>.*?)\])(?: \[(?<node>.*?)\])? (?<message>.+)");

		private static readonly Regex PortParser = new Regex(@"bound_address(es)?(opensearch)? {.+\:(?<port>\d+)}");

		//[2016-09-26T11:43:17,475][INFO ][o.e.n.Node               ] [readonly-node-a9c5f4] version[5.0.0-beta1], pid[13172], build[7eb6260/2016-09-20T23:10:37.942Z], OS[Windows 10/10.0/amd64], JVM[Oracle Corporation/Java HotSpot(TM) 64-Bit Server VM/1.8.0_101/25.101-b13]
		private static readonly Regex InfoParser =
			new Regex(@"version\[(?<version>.*)\], pid\[(?<pid>.*)\], build\[(?<build>.+)\]");

		private LineOutParser(string shortNamePrefix, string fullNamePrefix, string securityPluginName) : this()
		{
			_shortNamePrefix = shortNamePrefix;
			_fullNamePrefix = fullNamePrefix;
			_securityPluginRegex = new Regex(Regex.Escape(securityPluginName));
		}

		private readonly Regex _securityPluginRegex;
		private readonly string _shortNamePrefix;
		private readonly string _fullNamePrefix ;

		public bool TryParse(string line,
			out string date, out string level, out string section, out string node, out string message,
			out bool started)
		{
			date = level = section = node = message = null;
			started = false;
			if (string.IsNullOrEmpty(line)) return false;

			var match = ConsoleLineParser.Match(line);
			if (!match.Success) return false;
			date = match.Groups["date"].Value.Trim();
			level = match.Groups["level"].Value.Trim();
			section = match.Groups["section"].Value.Trim().Replace(_fullNamePrefix + ".", "");
			node = match.Groups["node"].Value.Trim();
			message = match.Groups["message"].Value.Trim();
			started = TryGetStartedConfirmation(section, message);
			return true;
		}

		private bool TryGetStartedConfirmation(string section, string message)
		{
			return section == ShortName("n.Node") && message == "started";
		}

		public bool TryGetPortNumber(string section, string message, out int port)
		{
			port = 0;
			var inHttpSection =
				section == ShortName("h.HttpServer")
				|| section == "http"
				|| section == ShortName("h.AbstractHttpServerTransport")
				|| section == ShortName("h.n.Netty4HttpServerTransport")
				|| section == ShortName("x.s.t.n.SecurityNetty4HttpServerTransport");
			if (!inHttpSection) return false;

			if (string.IsNullOrWhiteSpace(message)) return false;

			var match = PortParser.Match(message);
			if (!match.Success) return false;

			var portString = match.Groups["port"].Value.Trim();
			port = int.Parse(portString);
			return true;
		}

		public bool TryParseNodeInfo(string section, string message, out string version, out int? pid)
		{
			var inNodeSection = section == ShortName("n.Node") || section == "node";

			version = null;
			pid = null;
			if (!inNodeSection) return false;

			var match = InfoParser.Match(message.Replace(Environment.NewLine, ""));
			if (!match.Success) return false;

			version = match.Groups["version"].Value.Trim();
			pid = int.Parse(match.Groups["pid"].Value.Trim());
			return true;
		}

		private string ShortName(string suffix) => $"{_shortNamePrefix}.{suffix}";
	}
}
