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
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using OpenSearch.Net.Extensions;

namespace OpenSearch.Net;

public static class SniffParser
{
    public static Regex AddressRegex { get; } = new Regex(@"^((?<fqdn>[^/]+)/)?(?<ip>[^:]+|\[[\da-fA-F:\.]+\]):(?<port>\d+)$");

    public static Uri ParseToUri(string boundAddress, bool forceHttp)
    {
        if (boundAddress == null) throw new ArgumentNullException(nameof(boundAddress));

        var suffix = forceHttp ? "s" : string.Empty;
        var match = AddressRegex.Match(boundAddress);
        if (!match.Success) throw new Exception($"Can not parse bound_address: {boundAddress} to Uri");

        var fqdn = match.Groups["fqdn"].Value.Trim();
        var ip = match.Groups["ip"].Value.Trim();
        var port = match.Groups["port"].Value.Trim();
        var host = !fqdn.IsNullOrEmpty() ? fqdn : ip;

        return new Uri($"http{suffix}://{host}:{port}");
    }
}

internal class SniffResponse : OpenSearchResponseBase
{
    // ReSharper disable InconsistentNaming
    public string cluster_name { get; set; }

    public Dictionary<string, NodeInfo> nodes { get; set; }

    public IEnumerable<Node> ToNodes(bool forceHttp = false)
    {
        foreach (var kv in nodes.Where(n => n.Value.HttpEnabled))
        {
            var info = kv.Value;
            var httpEndpoint = info.http?.publish_address;
            if (string.IsNullOrWhiteSpace(httpEndpoint))
                httpEndpoint = kv.Value.http?.bound_address.FirstOrDefault();
            if (string.IsNullOrWhiteSpace(httpEndpoint))
                continue;

            var uri = SniffParser.ParseToUri(httpEndpoint, forceHttp);
            var node = new Node(uri)
            {
                Name = info.name,
                Id = kv.Key,
                ClusterManagerEligible = info.ClusterManagerEligible,
                HoldsData = info.HoldsData,
                IngestEnabled = info.IngestEnabled,
                HttpEnabled = info.HttpEnabled,
                Settings = new ReadOnlyDictionary<string, object>(info.settings)
            };
            yield return node;
        }
    }
}

internal class NodeInfo
{
    public string build_hash { get; set; }
    public string host { get; set; }
    public NodeInfoHttp http { get; set; }
    public string ip { get; set; }
    public string name { get; set; }
    public IList<string> roles { get; set; }
    public IDictionary<string, object> settings { get; set; }
    public string transport_address { get; set; }
    public string version { get; set; }
    internal bool HoldsData => roles?.Contains("data") ?? false;

    internal bool HttpEnabled
    {
        get
        {
            if (settings != null && settings.TryGetValue("http.enabled", out var httpEnabled))
                return Convert.ToBoolean(httpEnabled);

            return http != null;
        }
    }

    internal bool IngestEnabled => roles?.Contains("ingest") ?? false;

    internal bool ClusterManagerEligible => (roles == null ? false : roles.Contains("master") || roles.Contains("cluster_manager"));
    [Obsolete("Use ClusterManagerEligible instead", false)]
    internal bool MasterEligible => ClusterManagerEligible;
}

internal class NodeInfoHttp
{
    public IList<string> bound_address { get; set; }
    public string publish_address { get; set; }
}
